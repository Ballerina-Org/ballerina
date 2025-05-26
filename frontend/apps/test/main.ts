import {
  Sum,
  Option,
  Errors,
  ValueOrErrors,
  MapRepo,
  PredicateValueExtractor,
  PredicateValue,
  DispatchParsedType,
  Predicate,
  Renderer,
  BasicFun,
  Expr,
  Updater,
  BasicUpdater
} from "ballerina-core";
import { List, Map, Set } from "immutable";

export type EvalContext<T, Res> = {
  global: PredicateValue,
  root: PredicateValue,
  local: PredicateValue,
  traversalIterator: PredicateValue,
}

export type TraversalContext<T, Res> = {
  types: Map<string, DispatchParsedType<T>>;
  forms: Map<string, Renderer<T>>;
  primitiveRendererNamesByType: Map<string, Set<string>>;
  joinRes: BasicFun<[Res, Res], Res>
  traverseSingleType: Traversal<T, Res>
}

export type Traversal<T, Res> =
  BasicFun<
    DispatchParsedType<T>,
    Option< // based on the type provided, it can return `None` depending on some predicate such as "there are no children with the desired type T that we are searching". This is an important performance optimization
      ValueTraversal<T, Res>
    >
  >

export type ValueTraversal<T, Res> =
  BasicFun<
    EvalContext<T, Res>, // the proper dynamic part of the evaluation depends solely on the eval context (root, global, local) and the actual value being traversed
    ValueOrErrors<Res, string>
  >

export const RendererTraversal = {
  Operations: {
    Run: <T, Res>(
      type: DispatchParsedType<T>,
      renderer: Renderer<T>,
      traversalContext: TraversalContext<T, Res>
    ): ValueOrErrors<Option<ValueTraversal<T, Res>>, string> => {
      const rec = RendererTraversal.Operations.Run<T, Res>

      const mapEvalContext = (f:BasicUpdater<EvalContext<T,Res>>): Updater<Option<ValueTraversal<T, Res>>> => 
          Updater(Option.Updaters.some<ValueTraversal<T,Res>>(v => ctx => v(f(ctx))))

      const traverseNode = traversalContext.traverseSingleType(type)
      if (type.kind == "primitive") {
        return ValueOrErrors.Default.return(traverseNode)
      }
      if (type.kind == "lookup" && renderer.kind == "lookupRenderer") {
        if (traversalContext.primitiveRendererNamesByType.has(type.name)) {
          if (traversalContext.primitiveRendererNamesByType.get(type.name)!.has(renderer.renderer)) {
            return ValueOrErrors.Default.return(traverseNode)
          }
        }
        if (traversalContext.forms.has(type.name)) {
          // this is a form lookup, so "local" changes here to the traversed value
          return rec(type, traversalContext.forms.get(type.name)!, traversalContext).Then((valueTraversal:Option<ValueTraversal<T,Res>>) => {
            return ValueOrErrors.Default.return(mapEvalContext(ctx => ({...ctx, local:ctx.traversalIterator}))(valueTraversal))
          })
        }
        return ValueOrErrors.Default.throwOne(`Error: cannot resolve lookup renderer ${renderer.renderer} for type ${type.name}.`)
      }
      if (type.kind == "record" && renderer.kind == "recordRenderer") {
        return ValueOrErrors.Operations.All(
          List(
            type.fields.map((fieldType, fieldName) =>
              MapRepo.Operations.tryFindWithError(fieldName, renderer.fields, () => `Error: cannot find field ${fieldName} in renderer for type ${type.name}`)
                .Then(fieldRenderer =>
                  rec(fieldType, fieldRenderer.renderer, traversalContext)
                  .Then(fieldTraversal => {
                    return ValueOrErrors.Default.return({ fieldName:fieldName, visibility:fieldRenderer.visible, fieldTraversal:fieldTraversal})
                  })
                )
            ).valueSeq()
          )
        ).Then(fieldTraversals => {
          if (fieldTraversals.every(f => f.fieldTraversal.kind == "l") && traverseNode.kind == "l") {
            return ValueOrErrors.Default.return(Option.Default.none())
          }
          return ValueOrErrors.Default.return(Option.Default.some((evalContext: EvalContext<T, Res>) => {
            if (!PredicateValue.Operations.IsRecord(evalContext.traversalIterator))
              return ValueOrErrors.Default.throwOne(`Error: traversal iterator is not a record, got ${evalContext.traversalIterator}`)
            const traversalIteratorFields = evalContext.traversalIterator.fields
            return ValueOrErrors.Operations.All(fieldTraversals.flatMap(f => {
                if (f.fieldTraversal.kind == "l") return [] 
                if (f.visibility != undefined) {
                  const visible = Expr.Operations.EvaluateAsBoolean(Map([["global", evalContext.global], ["local", evalContext.local], ["root", evalContext.root]]))(f.visibility)
                  if (visible.kind == "value" && !visible.value) {
                    return []
                  }
                }
                return [f.fieldTraversal.value({ ...evalContext, traversalIterator: traversalIteratorFields.get(f.fieldName)! })]
              }))
                .Then((fieldResults: List<Res>) =>
                  (traverseNode.kind == "r") ?
                    traverseNode.value(evalContext).Then((nodeResult: Res) =>
                      ValueOrErrors.Default.return(fieldResults.reduce((acc, res) => traversalContext.joinRes([acc, res]), nodeResult))
                    )
                    :
                    ValueOrErrors.Default.return(fieldResults.reduce((acc, res) => traversalContext.joinRes([acc, res])))
                )
              }))
        })
      }
      return null!
    }
  }
}

const testInvocation = RendererTraversal.Operations.Run<any, Array<PredicateValue>>(null!, null!, {
  types: null!,
  forms: null!,
  primitiveRendererNamesByType: null!,
  joinRes: null!, // basically append or concat the arrays of the individual traversals
  traverseSingleType: (t => 
    t.kind == "lookup" && t.name == "Evidence" ?
      Option.Default.some((ctx: EvalContext<any, Array<PredicateValue>>) => ValueOrErrors.Default.return([ctx.traversalIterator])) :
    Option.Default.none())
})
