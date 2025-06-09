namespace Ballerina.DSL.Expr

module Patterns =
  open System
  open Model
  open Ballerina.Fun
  open Ballerina.Collections.Option
  open Ballerina.Collections.Map
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type Value<'ExprExtension, 'ValueExtension> with
    static member AsInt(v: Value<'ExprExtension, 'ValueExtension>) =
      match v with
      | Value.ConstInt v -> sum { return v }
      | _ -> sum.Throw(Errors.Singleton $"Error: expected int, found {v.ToString()}")

  type Expr<'ExprExtension, 'ValueExtension> with
    static member AsLambda(e: Expr<'ExprExtension, 'ValueExtension>) =
      match e with
      | Expr.Value(Value.Lambda(v, b)) -> sum { return (v, b) }
      | _ -> sum.Throw(Errors.Singleton $"Error: expected lambda, found {e.ToString()}")
