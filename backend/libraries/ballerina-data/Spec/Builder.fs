namespace Ballerina.Data.Spec

open Ballerina.DSL.Next.Types.Patterns
open Ballerina.Data.Schema.Model
open Ballerina.StdLib.OrderPreservingMap

module Builder =

  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Eval
  open Ballerina.State.WithError

  let typeContextFromSpecBody
    (schema: Schema<TypeExpr, Identifier>)
    : State<OrderedMap<Identifier, TypeValue>, TypeExprEvalContext, TypeExprEvalState, Errors> =
    schema.Types
    |> OrderedMap.map (fun identifier typeExpr ->
      state {
        let! tv, kind = TypeExpr.Eval None Location.Unknown typeExpr
        do! TypeExprEvalState.bindType (identifier |> TypeCheckScope.Empty.Resolve) (tv, kind)
        return tv
      })
    |> state.AllMapOrdered
//|> state.Map ignore
