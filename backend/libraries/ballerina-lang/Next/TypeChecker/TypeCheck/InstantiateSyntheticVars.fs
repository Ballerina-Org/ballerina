namespace Ballerina.DSL.Next.Types.TypeChecker

[<AutoOpen>]
module InstantiateSyntheticVars =
  open Ballerina.StdLib.String
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open System
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.AdHocPolymorphicOperators
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type TypeCheckedExpr<'ve> with
    static member InstantiateSyntheticVars<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr)
      (expr: TypeCheckedExpr<'valueExt>)
      : TypeCheckerResult<TypeCheckedExpr<'valueExt>, 'valueExt> =
      state {
        let loc0 = expr.Location

        let (!) = TypeCheckedExpr.InstantiateSyntheticVars config typeCheckExpr

        match expr.Expr with
        | TypeCheckedExprRec.RecordDes({ Expr = r; Field = field }) ->
          let! r = !r

          return
            TypeCheckedExpr.RecordDes(
              r,
              field,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.RecordWith({ Record = r; Fields = fields }) ->
          let! r = !r

          let! fields =
            fields
            |> List.map (fun (k, v) -> !v |> state.Map(fun v -> (k, v)))
            |> state.All

          return
            TypeCheckedExpr.RecordWith(
              r,
              fields,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.RecordCons({ Fields = fields }) ->
          let! fields =
            fields
            |> List.map (fun (k, v) -> !v |> state.Map(fun v -> (k, v)))
            |> state.All

          return
            TypeCheckedExpr.RecordCons(
              fields,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.TupleCons({ Items = items }) ->
          let! items = items |> List.map (!) |> state.All

          return
            TypeCheckedExpr.TupleCons(
              items,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.TupleDes({ Tuple = t; Item = item }) ->
          let! t = !t

          return
            TypeCheckedExpr.TupleDes(
              t,
              item,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.SumCons c ->
          return
            TypeCheckedExpr.SumCons(
              c.Selector,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.SumDes({ Handlers = handlers }) ->
          let! handlers =
            handlers
            |> Map.map (fun _ (vn, v) -> !v |> state.Map(fun v -> (vn, v)))
            |> state.AllMap

          return
            TypeCheckedExpr.SumDes(
              handlers,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.UnionDes({ Handlers = handlers
                                        Fallback = fallback }) ->
          let! handlers =
            handlers
            |> Map.map (fun _ (vn, v) -> !v |> state.Map(fun v -> (vn, v)))
            |> state.AllMap

          let! fallback = fallback |> Option.map (!) |> state.RunOption

          return
            TypeCheckedExpr.UnionDes(
              handlers,
              fallback,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.Let({ Var = v
                                   Type = t
                                   Val = value
                                   Rest = body }) ->
          let! value = !value
          let! body = !body
          return TypeCheckedExpr.Let(v, t, value, body, expr.Type, expr.Kind)
        | TypeCheckedExprRec.Do({ Val = e1; Rest = e2 }) ->
          let! e1 = !e1
          let! e2 = !e2

          return
            TypeCheckedExpr.Do(e1, e2, expr.Type, expr.Kind, loc0, expr.Scope)
        | TypeCheckedExprRec.Lambda({ Param = p
                                      Body = b
                                      ParamType = pt
                                      BodyType = bt }) ->
          let! b = !b
          return TypeCheckedExpr.Lambda(p, pt, b, bt, expr.Type, expr.Kind)
        | TypeCheckedExprRec.If({ Cond = c; Then = t; Else = e }) ->
          let! c = !c
          let! t = !t
          let! e = !e

          return
            TypeCheckedExpr.If(c, t, e, expr.Type, expr.Kind, loc0, expr.Scope)
        | TypeCheckedExprRec.Apply({ F = f; Arg = a }) ->
          let! f = !f
          let! a = !a

          return
            TypeCheckedExpr.Apply(f, a, expr.Type, expr.Kind, loc0, expr.Scope)
        | TypeCheckedExprRec.TypeApply({ Func = f
                                         TypeArg = TypeValue.Var(t_var) as t_arg }) when
          t_var.Synthetic
          ->
          let! e = !f

          let! t_arg =
            t_arg
            |> TypeValue.Instantiate
              ()
              (TypeExpr.Eval config typeCheckExpr)
              loc0
            |> Expr.liftInstantiation

          let res = TypeCheckedExpr.TypeApply(e, t_arg, expr.Type, expr.Kind)

          return res
        | TypeCheckedExprRec.Primitive p ->
          return
            TypeCheckedExpr.Primitive(p, expr.Type, expr.Kind, loc0, expr.Scope)
        | TypeCheckedExprRec.Lookup l ->
          return
            TypeCheckedExpr.Lookup(l.Id, expr.Type, expr.Kind, loc0, expr.Scope)
        | TypeCheckedExprRec.TypeLambda({ Param = p; Body = b }) ->
          let! b = !b

          return
            TypeCheckedExpr.TypeLambda(
              p,
              b,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.TypeApply({ Func = f; TypeArg = t }) ->
          let! f = !f

          return
            TypeCheckedExpr.TypeApply(
              f,
              t,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.TypeLet({ Name = n; TypeDef = td; Body = b }) ->
          let! b = !b

          return
            TypeCheckedExpr.TypeLet(
              n,
              td,
              b,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.FromValue({ Value = v
                                         ValueType = t
                                         ValueKind = k }) ->
          return TypeCheckedExpr.FromValue(v, t, k, loc0, expr.Scope)
        | TypeCheckedExprRec.EntitiesDes({ Expr = e }) ->
          let! e = !e

          return
            TypeCheckedExpr.EntitiesDes(
              e,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.RelationsDes({ Expr = e }) ->
          let! e = !e

          return
            TypeCheckedExpr.RelationsDes(
              e,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.EntityDes({ Expr = e; EntityName = entityName }) ->
          let! e = !e

          return
            TypeCheckedExpr.EntityDes(
              e,
              entityName,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.RelationDes({ Expr = e
                                           RelationName = relationName }) ->
          let! e = !e

          return
            TypeCheckedExpr.RelationDes(
              e,
              relationName,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.RelationLookupDes({ Expr = e
                                                 RelationName = relationName
                                                 Direction = direction }) ->
          let! e = !e

          return
            TypeCheckedExpr.RelationLookupDes(
              e,
              relationName,
              direction,
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.Query q ->
          return
            TypeCheckedExpr.Query(q, expr.Type, expr.Kind, loc0, expr.Scope)
        | TypeCheckedExprRec.View v ->
          let rec instantiateNode (node: TypeCheckedViewNode<'valueExt>) =
            state {
              match node.Node with
              | TypeCheckedViewNodeRec.ViewText _ -> return node
              | TypeCheckedViewNodeRec.ViewExprContainer e ->
                let! e = !e
                return
                  { node with
                      Node = TypeCheckedViewNodeRec.ViewExprContainer e }
              | TypeCheckedViewNodeRec.ViewFragment children ->
                let! children = children |> List.map instantiateNode |> state.All
                return
                  { node with
                      Node = TypeCheckedViewNodeRec.ViewFragment children }
              | TypeCheckedViewNodeRec.ViewElement el ->
                let! attrs =
                  el.Attributes
                  |> List.map (fun attr ->
                    state {
                      match attr with
                      | TypeCheckedViewAttribute.ViewAttrStringValue _ -> return attr
                      | TypeCheckedViewAttribute.ViewAttrExprValue(name, e) ->
                        let! e = !e
                        return TypeCheckedViewAttribute.ViewAttrExprValue(name, e)
                    })
                  |> state.All

                let! children = el.Children |> List.map instantiateNode |> state.All
                return
                  { node with
                      Node =
                        TypeCheckedViewNodeRec.ViewElement
                          { el with
                              Attributes = attrs
                              Children = children } }
            }

          let! body = instantiateNode v.Body
          return
            TypeCheckedExpr.View(
              { v with Body = body },
              expr.Type,
              expr.Kind,
              loc0,
              expr.Scope
            )
        | TypeCheckedExprRec.Co c ->
          return
            { Expr = TypeCheckedExprRec.Co c
              Location = loc0
              Type = expr.Type
              Kind = expr.Kind
              Scope = expr.Scope }
        | TypeCheckedExprRec.RecoveredSyntaxError err ->
          return
            { Expr = TypeCheckedExprRec.RecoveredSyntaxError err
              Location = loc0
              Type = expr.Type
              Kind = expr.Kind
              Scope = expr.Scope }
        | TypeCheckedExprRec.ErrorDanglingRecordDes err ->
          return
            { Expr = TypeCheckedExprRec.ErrorDanglingRecordDes err
              Location = loc0
              Type = expr.Type
              Kind = expr.Kind
              Scope = expr.Scope }
        | TypeCheckedExprRec.ErrorDanglingScopedIdentifier err ->
          return
            { Expr = TypeCheckedExprRec.ErrorDanglingScopedIdentifier err
              Location = loc0
              Type = expr.Type
              Kind = expr.Kind
              Scope = expr.Scope }
        | TypeCheckedExprRec.ErrorRecordDesButInvalidField err ->
          return
            { Expr = TypeCheckedExprRec.ErrorRecordDesButInvalidField err
              Location = loc0
              Type = expr.Type
              Kind = expr.Kind
              Scope = expr.Scope }

      }
