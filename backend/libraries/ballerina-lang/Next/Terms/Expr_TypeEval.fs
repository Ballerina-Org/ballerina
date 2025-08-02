namespace Ballerina.DSL.Next.Terms

module TypeEval =
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Eval

  type Expr<'T> with
    static member TypeEval: Expr<TypeExpr> -> Reader<Expr<TypeValue>, TypeExprEvalContext, Errors> =
      fun expr ->
        let (!) = Expr.TypeEval

        reader {
          match expr with
          | Expr.Lambda(var, body) ->
            let! bodyType = !body
            return Expr.Lambda(var, bodyType)
          | Expr.Apply(func, arg) ->
            let! funcType = !func
            let! argType = !arg
            return Expr.Apply(funcType, argType)
          | Expr.Let(var, value, body) ->
            let! valueType = !value
            let! bodyType = !body
            return Expr.Let(var, valueType, bodyType)
          | Expr.RecordCons fields ->
            let! fieldTypes =
              fields
              |> List.map (fun (name, value) ->
                reader {
                  let! valueType = !value
                  return (name, valueType)
                })
              |> reader.All

            return Expr.RecordCons fieldTypes
          | Expr.UnionCons(name, value) ->
            let! valueType = !value
            return Expr.UnionCons(name, valueType)
          | Expr.TupleCons values ->
            let! valueTypes = values |> List.map (!) |> reader.All
            return Expr.TupleCons valueTypes
          | Expr.SumCons(selector, value) ->
            let! valueType = !value
            return Expr.SumCons(selector, valueType)
          | Expr.RecordDes(record, field) ->
            let! recordType = !record
            return Expr.RecordDes(recordType, field)
          | Expr.UnionDes cases ->
            let! caseTypes =
              cases
              |> Map.map (fun _ (v, handler) ->
                reader {
                  let! handlerType = !handler
                  return v, handlerType
                })
              |> reader.AllMap

            return Expr.UnionDes caseTypes
          | Expr.TupleDes(tuple, selector) ->
            let! tupleType = !tuple
            return Expr.TupleDes(tupleType, selector)
          | Expr.SumDes cases ->
            let! caseTypes =
              cases
              |> Map.map (fun _ (i, handler) ->
                reader {
                  let! handlerType = !handler
                  return i, handlerType
                })
              |> reader.AllMap

            return Expr.SumDes caseTypes
          | Expr.Primitive p -> return Expr.Primitive p
          | Expr.Lookup name -> return Expr.Lookup name
          | Expr.If(cond, thenExpr, elseExpr) ->
            let! condType = !cond
            let! thenType = !thenExpr
            let! elseType = !elseExpr
            return Expr.If(condType, thenType, elseType)
          | Expr.TypeLambda(typeParam, body) ->
            let! bodyType = !body
            return Expr.TypeLambda(typeParam, bodyType)
          | Expr.TypeApply(typeExpr, typeArg) ->
            let! typeExprType = !typeExpr
            let! typeArg = typeArg |> TypeExpr.Eval
            return Expr.TypeApply(typeExprType, typeArg)
          | Expr.TypeLet(var, value, body) ->
            let! valueType = value |> TypeExpr.Eval

            let! bodyType =
              !body
              |> reader.MapContext(TypeExprEvalContext.Updaters.Bindings(Map.add var.Name valueType))

            return Expr.TypeLet(var, valueType, bodyType)
        }
