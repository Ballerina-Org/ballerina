namespace Ballerina.DSL.Expr

module Patterns =
  open Model
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type Expr<'ExprExtension, 'ValueExtension> with
    static member AsLambda(e: Expr<'ExprExtension, 'ValueExtension>) =
      match e with
      | Expr.Value(Value.Lambda(v, t, returnType, b)) -> sum { return (v, t, returnType, b) }
      | _ -> sum.Throw(Errors.Singleton $"Error: expected lambda, found {e.ToString()}")

    static member FromTypeBindings
      (bindings: Map<ExprTypeId, ExprType>)
      (expr: Expr<'ExprExtension, 'ValueExtension>)
      : Expr<'ExprExtension, 'ValueExtension> =
      bindings
      |> Map.toSeq
      |> Seq.sortBy (fun (typeId, _) -> typeId.VarName) // Sort by variable name to ensure deterministic output
      |> Seq.fold
        (fun (expr: Expr<'ExprExtension, 'ValueExtension>) (typeId: ExprTypeId, t: ExprType) ->
          Expr.LetType(typeId, t, expr))
        expr
