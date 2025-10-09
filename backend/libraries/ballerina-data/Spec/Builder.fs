namespace Ballerina.Data.Spec

module Builder =

  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Eval
  open Ballerina.Data.Spec.Model
  open Ballerina.State.WithError

  let typeContextFromSpecBody (spec: V2Format) : State<unit, TypeExprEvalContext, TypeExprEvalState, Errors> =
    spec.TypesV2
    |> List.map (fun (name, expr) ->
      state {
        let! tv = TypeExpr.Eval None Location.Unknown expr
        do! TypeExprEvalState.bindType name tv
        return ()
      })
    |> state.All
    |> state.Map ignore
