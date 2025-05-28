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
  Unit,
  unit,
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
  zeroRes: BasicFun<Unit, Res>;
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

      if (type.kind == "primitive") {
        return ValueOrErrors.Default.return(traverseNode);
      }

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
                        traversalContext.zeroRes(unit),
                      ),
                    );
              });
            }),
          );
        });
      }
      if (
        type.kind == "sum" &&
        (renderer.kind == "sumRenderer" ||
          renderer.kind == "sumUnitDateRenderer")
      ) {
        // this will be removed when sums become proper partials
        if (renderer.kind == "sumUnitDateRenderer") {
          if (traverseNode.kind == "l") {
            return ValueOrErrors.Default.return(Option.Default.none());
          }
          return ValueOrErrors.Default.return(
            Option.Default.some((evalContext: EvalContext<T, Res>) =>
              traverseNode.value(evalContext),
            ),
          );
        }
        if (renderer.kind == "sumRenderer") {
          const res: ValueOrErrors<
            Option<ValueTraversal<T, Res>>,
            string
          > = rec(type.args[0], renderer, traversalContext).Then(
            (leftTraversal) =>
              rec(type.args[1], renderer, traversalContext).Then(
                (rightTraversal) => {
                  if (
                    leftTraversal.kind == "l" &&
                    rightTraversal.kind == "l" &&
                    traverseNode.kind == "l"
                  ) {
                    return ValueOrErrors.Default.return(Option.Default.none());
                  }
                  return ValueOrErrors.Default.return(
                    Option.Default.some<ValueTraversal<T, Res>>(
                      (evalContext: EvalContext<T, Res>) => {
                        const iterator = evalContext.traversalIterator;
                        if (!PredicateValue.Operations.IsSum(iterator)) {
                          return ValueOrErrors.Default.throwOne(
                            `Error: traversal iterator is not a sum, got ${evalContext.traversalIterator}`,
                          );
                        }

                        const isRight = iterator.value.kind == "r";

                        if (isRight && rightTraversal.kind == "r") {
                          if (traverseNode.kind == "r") {
                            return traverseNode
                              .value(evalContext)
                              .Then((nodeResult: Res) => {
                                return rightTraversal
                                  .value({
                                    ...evalContext,
                                    traversalIterator: iterator.value.value,
                                  })
                                  .Then((rightRes) => {
                                    return ValueOrErrors.Default.return<
                                      Res,
                                      string
                                    >(
                                      traversalContext.joinRes([
                                        nodeResult,
                                        rightRes,
                                      ]),
                                    );
                                  });
                              });
                          }
                          return rightTraversal
                            .value({
                              ...evalContext,
                              traversalIterator: iterator.value.value,
                            })
                            .Then((rightRes) => {
                              return ValueOrErrors.Default.return<Res, string>(
                                rightRes,
                              );
                            });
                        }

                        if (!isRight && leftTraversal.kind == "r") {
                          if (traverseNode.kind == "r") {
                            return traverseNode
                              .value(evalContext)
                              .Then((nodeResult: Res) => {
                                return leftTraversal
                                  .value({
                                    ...evalContext,
                                    traversalIterator: iterator.value.value,
                                  })
                                  .Then((leftRes) => {
                                    return ValueOrErrors.Default.return<
                                      Res,
                                      string
                                    >(
                                      traversalContext.joinRes([
                                        nodeResult,
                                        leftRes,
                                      ]),
                                    );
                                  });
                              });
                          }
                          return leftTraversal
                            .value({
                              ...evalContext,
                              traversalIterator: iterator.value.value,
                            })
                            .Then((leftRes) => {
                              return ValueOrErrors.Default.return<Res, string>(
                                leftRes,
                              );
                            });
                        }

                        return ValueOrErrors.Default.return<Res, string>(
                          traversalContext.zeroRes(unit),
                        );
                      },
                    ),
                  );
                },
              ),
          );
          return res;
        }
      }

      if (type.kind == "tuple" && renderer.kind == "tupleRenderer") {
        return ValueOrErrors.Operations.All(
          List(
            renderer.itemRenderers.map((itemRenderer, index) => {
              return rec(
                type.args[index],
                itemRenderer.renderer,
                traversalContext,
              ).Then((itemTraversal) => {
                return ValueOrErrors.Default.return({
                  index: index,
                  itemTraversal: itemTraversal,
                });
              });
            }),
          ),
        ).Then((itemTraversals) => {
          if (
            itemTraversals.every((i) => i.itemTraversal.kind == "l") &&
            traverseNode.kind == "l"
          ) {
            return ValueOrErrors.Default.return(Option.Default.none());
          }

          return ValueOrErrors.Default.return(
            Option.Default.some((evalContext: EvalContext<T, Res>) => {
              const iterator = evalContext.traversalIterator;
              if (!PredicateValue.Operations.IsTuple(iterator)) {
                return ValueOrErrors.Default.throwOne(
                  `Error: traversal iterator is not a tuple, got ${evalContext.traversalIterator}`,
                );
              }
              return ValueOrErrors.Operations.All(
                itemTraversals.flatMap((i) =>
                  i.itemTraversal.kind == "r"
                    ? [
                        i.itemTraversal.value({
                          ...evalContext,
                          traversalIterator: iterator.values.get(i.index)!,
                        }),
                      ]
                    : [],
                ),
              ).Then((itemResults) => {
                return traverseNode.kind == "r"
                  ? traverseNode
                      .value(evalContext)
                      .Then((nodeResult: Res) =>
                        ValueOrErrors.Default.return(
                          itemResults.reduce(
                            (acc, res) => traversalContext.joinRes([acc, res]),
                            nodeResult,
                          ),
                        ),
                      )
                  : ValueOrErrors.Default.return(
                      itemResults.reduce(
                        (acc, res) => traversalContext.joinRes([acc, res]),
                        traversalContext.zeroRes(unit),
                      ),
                    );
              });
            }),
          );
        });
      }

      return ValueOrErrors.Default.return(Option.Default.none());
    },
  },
};
