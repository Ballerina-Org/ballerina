import {
  Option,
  ValueOrErrors,
  MapRepo,
  PredicateValue,
  DispatchParsedType,
  Renderer,
  BasicFun,
  Expr,
  Updater,
  BasicUpdater,
  FormLayout,
} from "ballerina-core";
import { List, Map, Set } from "immutable";

export type EvalContext<T, Res> = {
  global: PredicateValue;
  root: PredicateValue;
  local: PredicateValue;
  traversalIterator: PredicateValue;
};

export type TraversalContext<T, Res> = {
  types: Map<string, DispatchParsedType<T>>;
  forms: Map<string, Renderer<T>>;
  primitiveRendererNamesByType: Map<string, Set<string>>;
  joinRes: BasicFun<[Res, Res], Res>;
  traverseSingleType: Traversal<T, Res>;
};

export type Traversal<T, Res> = BasicFun<
  DispatchParsedType<T>,
  Option<
    // based on the type provided, it can return `None` depending on some predicate such as "there are no children with the desired type T that we are searching". This is an important performance optimization
    ValueTraversal<T, Res>
  >
>;

export type ValueTraversal<T, Res> = BasicFun<
  EvalContext<T, Res>, // the proper dynamic part of the evaluation depends solely on the eval context (root, global, local) and the actual value being traversed
  ValueOrErrors<Res, string>
>;

export const RendererTraversal = {
  Operations: {
    Run: <T, Res>(
      type: DispatchParsedType<T>,
      renderer: Renderer<T>,
      traversalContext: TraversalContext<T, Res>,
    ): ValueOrErrors<Option<ValueTraversal<T, Res>>, string> => {
      console.debug("running traversal", type, renderer);
      const rec = RendererTraversal.Operations.Run<T, Res>;

      const mapEvalContext = (
        f: BasicUpdater<EvalContext<T, Res>>,
      ): Updater<Option<ValueTraversal<T, Res>>> =>
        Updater(
          Option.Updaters.some<ValueTraversal<T, Res>>(
            (v) => (ctx) => v(f(ctx)),
          ),
        );

      const traverseNode = traversalContext.traverseSingleType(type);
      console.debug("traverseNode", traverseNode);
      if (type.kind == "primitive") {
        return ValueOrErrors.Default.return(traverseNode);
      }
      //   if(type.kind == "lookup"){
      //     MapRepo.Operations.tryFindWithError(
      //       type.name,
      //       traversalContext.types,
      //       () => `Error: cannot find type ${type.name} in types`,
      //     ).Then((resolvedType) => {
      //       return rec(resolvedType, renderer, traversalContext);
      //     });
      //   }

      if (
        (type.kind == "lookup" || type.kind == "record") &&
        renderer.kind == "lookupRenderer"
      ) {
        if (traversalContext.primitiveRendererNamesByType.has(type.name)) {
          if (
            traversalContext.primitiveRendererNamesByType
              .get(type.name)!
              .has(renderer.renderer)
          ) {
            return ValueOrErrors.Default.return(traverseNode);
          }
        }
        if (traversalContext.forms.has(renderer.renderer)) {
          // renderer.renderer is the form name
          // this is a form lookup, so "local" changes here to the traversed value
          return (
            type.kind == "lookup"
              ? MapRepo.Operations.tryFindWithError(
                  type.name,
                  traversalContext.types,
                  () => `Error: cannot find type ${type.name} in types`,
                )
              : ValueOrErrors.Default.return<DispatchParsedType<T>, string>(
                  type,
                )
          ).Then((resolvedType) => {
            return rec(
              resolvedType,
              traversalContext.forms.get(renderer.renderer)!,
              traversalContext,
            ).Then((valueTraversal: Option<ValueTraversal<T, Res>>) => {
              return ValueOrErrors.Default.return(
                mapEvalContext((ctx) => ({
                  ...ctx,
                  local: ctx.traversalIterator,
                }))(valueTraversal),
              );
            });
          });
        }
        return ValueOrErrors.Default.throwOne(
          `Error: cannot resolve lookup renderer ${renderer.renderer} for type ${type.name}.`,
        );
      }
      if (type.kind == "record" && renderer.kind == "recordRenderer") {
        return ValueOrErrors.Operations.All(
          List(
            renderer.fields
              .map((fieldRenderer, fieldName) =>
                rec(
                  fieldRenderer.renderer.type,
                  fieldRenderer.renderer,
                  traversalContext,
                ).Then((fieldTraversal) => {
                  return ValueOrErrors.Default.return({
                    fieldName: fieldName,
                    visibility: fieldRenderer.visible,
                    fieldTraversal: fieldTraversal,
                  });
                }),
              )
              .valueSeq(),
          ),
        ).Then((fieldTraversals) => {
          if (
            fieldTraversals.every((f) => f.fieldTraversal.kind == "l") &&
            traverseNode.kind == "l"
          ) {
            return ValueOrErrors.Default.return(Option.Default.none());
          }
          return ValueOrErrors.Default.return(
            Option.Default.some((evalContext: EvalContext<T, Res>) => {
              if (
                !PredicateValue.Operations.IsRecord(
                  evalContext.traversalIterator,
                )
              )
                return ValueOrErrors.Default.throwOne(
                  `Error: traversal iterator is not a record, got ${evalContext.traversalIterator}`,
                );
              const visibleFieldsRes =
                FormLayout.Operations.ComputeVisibleFieldsForRecord(
                  Map([
                    ["global", evalContext.global],
                    ["local", evalContext.local],
                    ["root", evalContext.root],
                  ]),
                  renderer.tabs,
                );
              // TODO later, make this monadic
              if (visibleFieldsRes.kind == "errors") {
                return visibleFieldsRes;
              }
              const visibleFields = visibleFieldsRes.value;
              console.debug("visibleFields", visibleFields);
              const traversalIteratorFields =
                evalContext.traversalIterator.fields;
              return ValueOrErrors.Operations.All(
                fieldTraversals.flatMap((f) => {
                  // should be a map and instead of flatmap and [] a VoE.default.return([]) then an All on this and then a flatmap, everything is returned in a VoE
                  if (
                    f.fieldTraversal.kind == "l" ||
                    !visibleFields.includes(f.fieldName)
                  )
                    return [];
                  if (f.visibility != undefined) {
                    const visible = Expr.Operations.Evaluate(
                      Map([
                        ["global", evalContext.global],
                        ["local", evalContext.local],
                        ["root", evalContext.root],
                      ]),
                    )(f.visibility);
                    if (visible.kind == "value" && !visible.value) {
                      return [];
                    }
                  }
                  console.debug("f", f.fieldName, [
                    f.fieldTraversal.value({
                      ...evalContext,
                      traversalIterator: traversalIteratorFields.get(
                        f.fieldName,
                      )!,
                    }),
                  ]);
                  return [
                    f.fieldTraversal.value({
                      ...evalContext,
                      traversalIterator: traversalIteratorFields.get(
                        f.fieldName,
                      )!,
                    }),
                  ];
                }),
              ).Then((fieldResults: List<Res>) => {
                return traverseNode.kind == "r"
                  ? traverseNode
                      .value(evalContext)
                      .Then((nodeResult: Res) =>
                        ValueOrErrors.Default.return(
                          fieldResults.reduce(
                            (acc, res) => traversalContext.joinRes([acc, res]),
                            nodeResult,
                          ),
                        ),
                      )
                  : ValueOrErrors.Default.return(
                      fieldResults.reduce(
                        (acc, res) => traversalContext.joinRes([acc, res]),
                        [] as Res,
                      ),
                    );
              });
            }),
          );
        });
      }
      return null!;
    },
  },
};

//   const testInvocation = RendererTraversal.Operations.Run<
//     any,
//     Array<PredicateValue>
//   >(null!, null!, {
//     types: null!,
//     forms: null!,
//     primitiveRendererNamesByType: null!,
//     joinRes: null!, // basically append or concat the arrays of the individual traversals
//     traverseSingleType: (t) =>
//       t.kind == "lookup" && t.name == "Evidence"
//         ? Option.Default.some((ctx: EvalContext<any, Array<PredicateValue>>) =>
//             ValueOrErrors.Default.return([ctx.traversalIterator]),
//           )
//         : Option.Default.none(),
//   });

// return ValueOrErrors.Operations.All(
//     fieldTraversals.flatMap((f) => {
//       console.debug("f", f);
//       // should be a map and instead of flatmap and [] a VoE.default.return([]) then an All on this and then a flatmap, everything is returned in a VoE, we do an all and then get a
//       if (f.fieldTraversal.kind == "l") return [];
//       if (f.visibility != undefined) {
//         const visible = Expr.Operations.Evaluate(
//           Map([
//             ["global", evalContext.global],
//             ["local", evalContext.local],
//             ["root", evalContext.root],
//           ]),
//         )(f.visibility);
//         console.debug("visible", visible);
//         if (visible.kind == "value" && !visible.value) {
//           console.debug("visible is false", f.fieldName);
//           return [];
//         }
//       }
//       return [
//         f.fieldTraversal.value({
//           ...evalContext,
//           traversalIterator: traversalIteratorFields.get(
//             f.fieldName,
//           )!,
//         }),
//       ];
//     }),
//   )
