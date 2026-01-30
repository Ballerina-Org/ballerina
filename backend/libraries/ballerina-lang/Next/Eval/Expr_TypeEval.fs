namespace Ballerina.DSL.Next.Terms

module TypeEval =
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member TypeEval<'valueExt when 'valueExt: comparison>
      ()
      : TypeChecker<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt>
          -> Location
          -> Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>
          -> State<
            Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>,
            TypeCheckContext<'valueExt>,
            TypeCheckState<'valueExt>,
            Errors<Location>
           >
      =
      fun typeCheckExpr loc0 expr ->
        let (!) = Expr.TypeEval () typeCheckExpr loc0

        let (!!) t =
          state {
            let! t, _ = t |> TypeExpr.Eval () typeCheckExpr None loc0
            return t
          }

        state {
          let! ctx = state.GetContext()

          match expr.Expr with
          | ExprRec.Lambda({ Param = var
                             ParamType = t
                             Body = body }) ->
            let! bodyType = !body
            let! t = t |> Option.map (!!) |> state.RunOption
            return Expr.Lambda(var, t, bodyType, expr.Location, ctx.Scope)
          | ExprRec.Apply({ F = func; Arg = arg }) ->
            let! funcType = !func
            let! argType = !arg
            return Expr.Apply(funcType, argType, expr.Location, ctx.Scope)
          | ExprRec.FromValue({ Value = v
                                ValueType = v_t
                                ValueKind = v_k }) -> return Expr.FromValue(v, v_t, v_k, expr.Location, ctx.Scope)
          | ExprRec.Let({ Var = var
                          Type = var_type
                          Val = value
                          Rest = body }) ->
            let! valueType = !value
            let! bodyType = !body
            let! var_type = var_type |> Option.map (!!) |> state.RunOption
            return Expr.Let(var, var_type, valueType, bodyType, expr.Location, ctx.Scope)
          | ExprRec.RecordWith({ Record = record; Fields = fields }) ->
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

          | ExprRec.RecordCons { Fields = fields } ->
            let! fieldTypes =
              fields
              |> List.map (fun (name, value) ->
                state {
                  let! valueType = !value
                  return (name |> ctx.Scope.Resolve, valueType)
                })
              |> state.All

            return Expr.RecordCons(fieldTypes, expr.Location, ctx.Scope)
          | ExprRec.TupleCons { Items = values } ->
            let! valueTypes = values |> List.map (!) |> state.All
            return Expr.TupleCons(valueTypes, expr.Location, ctx.Scope)
          | ExprRec.SumCons({ Selector = selector }) -> return Expr.SumCons(selector, expr.Location, ctx.Scope)
          | ExprRec.RecordDes({ Expr = record; Field = field }) ->
            let! recordType = !record
            return Expr.RecordDes(recordType, field |> ctx.Scope.Resolve, expr.Location, ctx.Scope)
          | ExprRec.UnionDes({ Handlers = cases
                               Fallback = fallback }) ->
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
          | ExprRec.TupleDes({ Tuple = tuple; Item = selector }) ->
            let! tupleType = !tuple
            return Expr.TupleDes(tupleType, selector, expr.Location, ctx.Scope)
          | ExprRec.SumDes { Handlers = cases } ->
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
          | ExprRec.Lookup { Id = name } -> return Expr.Lookup(name |> ctx.Scope.Resolve, expr.Location, ctx.Scope)
          | ExprRec.If({ Cond = cond
                         Then = thenExpr
                         Else = elseExpr }) ->
            let! condType = !cond
            let! thenType = !thenExpr
            let! elseType = !elseExpr
            return Expr.If(condType, thenType, elseType, expr.Location, ctx.Scope)
          | ExprRec.TypeLambda({ Param = typeParam; Body = body }) ->
            let! bodyType = !body
            return Expr.TypeLambda(typeParam, bodyType, expr.Location, ctx.Scope)
          | ExprRec.TypeApply({ ExprTypeApply.Func = typeExpr
                                TypeArg = typeArg }) ->
            let! typeExprType = !typeExpr
            let! typeArg, _ = typeArg |> TypeExpr.Eval () typeCheckExpr None loc0
            return Expr.TypeApply(typeExprType, typeArg, expr.Location, ctx.Scope)
          | ExprRec.TypeLet({ ExprTypeLet.Name = var
                              TypeDef = value
                              Body = body }) ->
            let! valueType = value |> TypeExpr.Eval () typeCheckExpr None loc0
            do! TypeCheckState.bindType (var |> Identifier.LocalScope |> ctx.Scope.Resolve) valueType

            let! bodyType = !body

            return Expr.TypeLet(var, valueType |> fst, bodyType, expr.Location, ctx.Scope)
          | ExprRec.EntitiesDes({ Expr = entities_expr }) ->
            let! entitiesType = !entities_expr
            return Expr.EntitiesDes(entitiesType, expr.Location, ctx.Scope)
          | ExprRec.RelationsDes({ Expr = entities_expr }) ->
            let! entitiesType = !entities_expr
            return Expr.RelationsDes(entitiesType, expr.Location, ctx.Scope)
          | ExprRec.EntityDes({ Expr = entity_expr
                                EntityName = entity_name }) ->
            let! entityType = !entity_expr
            return Expr.EntityDes(entityType, entity_name, expr.Location, ctx.Scope)
          | ExprRec.RelationDes({ Expr = relation_expr
                                  RelationName = relation_name }) ->
            let! relationType = !relation_expr
            return Expr.RelationDes(relationType, relation_name, expr.Location, ctx.Scope)
          | ExprRec.RelationLookupDes({ Expr = record_v
                                        RelationName = relation_name
                                        Direction = direction }) ->
            let! recordType = !record_v
            return Expr.RelationLookupDes(recordType, relation_name, direction, expr.Location, ctx.Scope)
        }
