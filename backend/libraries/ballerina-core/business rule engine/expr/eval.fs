namespace Ballerina.DSL.Expr

module Eval =
  open System
  open System.Linq
  open Ballerina.Fun
  open Ballerina.Coroutines.Model
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Expr
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.TypeCheck
  open Ballerina.Errors

  type ExprEvalContext = { Vars: Vars }

  type ExprEvalState = unit

  type Expr with
    static member eval(e: Expr) : Coroutine<Value, ExprEvalContext, ExprEvalState, Unit, Errors> =
      co {
        if e.IsBinary then
          return Value.ConstString "Placeholder, check Expr.eval"
        else
          return Value.ConstString "Placeholder, check Expr.eval"
      }
// let rec eval (vars: Vars) (e: Expr) : list<Vars * Value> =
//   match e with
//   | VarLookup v when vars |> Map.containsKey v -> [ vars, (Value.Var(vars.[v])) ]
//   | Value v -> v
//   | Binary(Plus, e1, e2) ->
//     [ for vars', (i1, i2) in eval2AsInt vars e1 e2 do
//         yield vars', Value.ConstInt(i1 + i2) ]
//   | Binary(Equals, e1, e2) ->
//     [ for vars', res1 in eval vars e1 do
//         for vars'', res2 in eval vars' e2 do
//           yield vars'', Value.ConstBool(res1 = res2) ]
//   | e ->
//     printfn "not implemented Expr evaluator for %A" e
//     []

// and eval2AsInt vars e1 e2 =
//   [ for vars', v1 in eval vars e1 do
//       for vars'', v2 in eval vars' e2 do
//         match v1, v2 with
//         | Value.ConstInt i1, Value.ConstInt i2 -> yield vars'', (i1, i2)
//         | _ -> () ]

// eval vars
