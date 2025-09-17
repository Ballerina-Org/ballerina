namespace Ballerina.Data.Spec

module Builder =

  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Eval
  open Ballerina.Data.Spec.Model
  open Ballerina.State.WithError

  let typeContextFromSpecBody (spec: SpecFormat) : State<unit, TypeExprEvalContext, TypeExprEvalState, Errors> =
    spec.TypesV2
    |> List.map (fun (name, expr) ->
      state {

        let! tv = TypeExpr.Eval expr
        let! sb = TypeExpr.EvalAsSymbol expr
        do! TypeExprEvalState.bindType name tv
        do! TypeExprEvalState.bindSymbol name sb
        return ()
      })
    |> state.All
    |> state.Map(fun _ -> ())
