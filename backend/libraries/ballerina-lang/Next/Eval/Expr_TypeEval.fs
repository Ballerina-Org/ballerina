namespace Ballerina.DSL.Next.Terms

module TypeEval =
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.State.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Types.TypeChecker.TypeCheck
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.TypeCheck
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns

  type Expr<'T, 'Id when 'Id: comparison> with
    static member TypeEval
      : Location
          -> Expr<TypeExpr, Identifier>
          -> State<Expr<TypeValue, ResolvedIdentifier>, TypeExprEvalContext, TypeExprEvalState, Errors> =
      fun loc0 expr ->
        let (!) = Expr.TypeEval loc0

        let (!!) t =
          state {
            let! t, _ = t |> TypeExpr.Eval None loc0
            return t
          }

        state {
          let! ctx = state.GetContext()

          match expr.Expr with
          | ExprRec.Lambda(var, t, body) ->
            let! bodyType = !body
            let! t = t |> Option.map (!!) |> state.RunOption
            return Expr.Lambda(var, t, bodyType, expr.Location, ctx.Scope)
          | ExprRec.Apply(func, arg) ->
            let! funcType = !func
            let! argType = !arg
            return Expr.Apply(funcType, argType, expr.Location, ctx.Scope)
          | ExprRec.Let(var, var_type, value, body) ->
            let! valueType = !value
            let! bodyType = !body
            let! var_type = var_type |> Option.map (!!) |> state.RunOption
            return Expr.Let(var, var_type, valueType, bodyType, expr.Location, ctx.Scope)
          | ExprRec.RecordWith(record, fields) ->
            let! recordType = !record

            let! fieldTypes =
              fields
              |> List.map (fun (name, value) ->
                state {
                  let! valueType = !value
                  return (name |> ctx.Scope.Resolve, valueType)
                })
              |> state.All

            return Expr.RecordWith(recordType, fieldTypes, expr.Location, ctx.Scope)

          | ExprRec.RecordCons fields ->
            let! fieldTypes =
              fields
              |> List.map (fun (name, value) ->
                state {
                  let! valueType = !value
                  return (name |> ctx.Scope.Resolve, valueType)
                })
              |> state.All

            return Expr.RecordCons(fieldTypes, expr.Location, ctx.Scope)
          | ExprRec.TupleCons values ->
            let! valueTypes = values |> List.map (!) |> state.All
            return Expr.TupleCons(valueTypes, expr.Location, ctx.Scope)
          | ExprRec.SumCons(selector) -> return Expr.SumCons(selector, expr.Location, ctx.Scope)
          | ExprRec.RecordDes(record, field) ->
            let! recordType = !record
            return Expr.RecordDes(recordType, field |> ctx.Scope.Resolve, expr.Location, ctx.Scope)
          | ExprRec.UnionDes(cases, fallback) ->
            let! caseTypes =
              cases
              |> Map.toSeq
              |> Seq.map (fun (k, (v, handler)) ->
                state {
                  let! handlerType = !handler
                  return k |> ctx.Scope.Resolve, (v, handlerType)
                })
              |> state.All
              |> state.Map Map.ofSeq

            let! fallback = fallback |> Option.map (!) |> state.RunOption

            return Expr.UnionDes(caseTypes, fallback, expr.Location, ctx.Scope)
          | ExprRec.TupleDes(tuple, selector) ->
            let! tupleType = !tuple
            return Expr.TupleDes(tupleType, selector, expr.Location, ctx.Scope)
          | ExprRec.SumDes cases ->
            let! caseTypes =
              cases
              |> Map.map (fun _k (v, handler) ->
                state {
                  let! handlerType = !handler
                  return v, handlerType
                })
              |> state.AllMap

            return Expr.SumDes(caseTypes, expr.Location, ctx.Scope)
          | ExprRec.Primitive p -> return Expr.Primitive(p, expr.Location, ctx.Scope)
          | ExprRec.Lookup name -> return Expr.Lookup(name |> ctx.Scope.Resolve, expr.Location, ctx.Scope)
          | ExprRec.If(cond, thenExpr, elseExpr) ->
            let! condType = !cond
            let! thenType = !thenExpr
            let! elseType = !elseExpr
            return Expr.If(condType, thenType, elseType, expr.Location, ctx.Scope)
          | ExprRec.TypeLambda(typeParam, body) ->
            let! bodyType = !body
            return Expr.TypeLambda(typeParam, bodyType, expr.Location, ctx.Scope)
          | ExprRec.TypeApply(typeExpr, typeArg) ->
            let! typeExprType = !typeExpr
            let! typeArg, _ = typeArg |> TypeExpr.Eval None loc0
            return Expr.TypeApply(typeExprType, typeArg, expr.Location, ctx.Scope)
          | ExprRec.TypeLet(var, value, body) ->
            let! valueType = value |> TypeExpr.Eval None loc0
            do! TypeExprEvalState.bindType (var |> Identifier.LocalScope |> ctx.Scope.Resolve) valueType

            let! bodyType = !body

            return Expr.TypeLet(var, valueType |> fst, bodyType, expr.Location, ctx.Scope)
        }
