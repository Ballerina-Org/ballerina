namespace Ballerina.DSL.Next.Terms

module TypeEval =
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.State.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Types.Eval

  type Expr<'T> with
    static member TypeEval
      : Location -> Expr<TypeExpr> -> State<Expr<TypeValue>, TypeExprEvalContext, TypeExprEvalState, Errors> =
      fun loc0 expr ->
        let (!) = Expr.TypeEval loc0

        let (!!) t =
          state {
            let! t, _ = t |> TypeExpr.Eval None loc0
            return t
          }

        state {
          match expr.Expr with
          | ExprRec.Lambda(var, t, body) ->
            let! bodyType = !body
            let! t = t |> Option.map (!!) |> state.RunOption
            return Expr.Lambda(var, t, bodyType, expr.Location)
          | ExprRec.Apply(func, arg) ->
            let! funcType = !func
            let! argType = !arg
            return Expr.Apply(funcType, argType, expr.Location)
          | ExprRec.Let(var, var_type, value, body) ->
            let! valueType = !value
            let! bodyType = !body
            let! var_type = var_type |> Option.map (!!) |> state.RunOption
            return Expr.Let(var, var_type, valueType, bodyType, expr.Location)
          | ExprRec.RecordCons fields ->
            let! fieldTypes =
              fields
              |> List.map (fun (name, value) ->
                state {
                  let! valueType = !value
                  return (name, valueType)
                })
              |> state.All

            return Expr.RecordCons(fieldTypes, expr.Location)
          | ExprRec.UnionCons(name, value) ->
            let! valueType = !value
            return Expr.UnionCons(name, valueType, expr.Location)
          | ExprRec.TupleCons values ->
            let! valueTypes = values |> List.map (!) |> state.All
            return Expr.TupleCons(valueTypes, expr.Location)
          | ExprRec.SumCons(selector, value) ->
            let! valueType = !value
            return Expr.SumCons(selector, valueType, expr.Location)
          | ExprRec.RecordDes(record, field) ->
            let! recordType = !record
            return Expr.RecordDes(recordType, field, expr.Location)
          | ExprRec.UnionDes(cases, fallback) ->
            let! caseTypes =
              cases
              |> Map.map (fun _ (v, handler) ->
                state {
                  let! handlerType = !handler
                  return v, handlerType
                })
              |> state.AllMap

            let! fallback = fallback |> Option.map (!) |> state.RunOption

            return Expr.UnionDes(caseTypes, fallback, expr.Location)
          | ExprRec.TupleDes(tuple, selector) ->
            let! tupleType = !tuple
            return Expr.TupleDes(tupleType, selector, expr.Location)
          | ExprRec.SumDes cases ->
            let! caseTypes =
              cases
              |> Seq.map (fun (v, handler) ->
                state {
                  let! handlerType = !handler
                  return v, handlerType
                })
              |> state.All

            return Expr.SumDes(caseTypes, expr.Location)
          | ExprRec.Primitive p -> return Expr.Primitive(p, expr.Location)
          | ExprRec.Lookup name -> return Expr.Lookup(name, expr.Location)
          | ExprRec.If(cond, thenExpr, elseExpr) ->
            let! condType = !cond
            let! thenType = !thenExpr
            let! elseType = !elseExpr
            return Expr.If(condType, thenType, elseType, expr.Location)
          | ExprRec.TypeLambda(typeParam, body) ->
            let! bodyType = !body
            return Expr.TypeLambda(typeParam, bodyType, expr.Location)
          | ExprRec.TypeApply(typeExpr, typeArg) ->
            let! typeExprType = !typeExpr
            let! typeArg, _ = typeArg |> TypeExpr.Eval None loc0
            return Expr.TypeApply(typeExprType, typeArg, expr.Location)
          | ExprRec.TypeLet(var, value, body) ->
            let! valueType = value |> TypeExpr.Eval None loc0
            do! TypeExprEvalState.bindType var valueType

            let! bodyType = !body

            return Expr.TypeLet(var, valueType |> fst, bodyType, expr.Location)
        }
