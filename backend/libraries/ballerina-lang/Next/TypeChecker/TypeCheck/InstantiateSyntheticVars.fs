namespace Ballerina.DSL.Next.Types.TypeChecker

[<AutoOpen>]
module InstantiateSyntheticVars =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
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

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member InstantiateSyntheticVars<'valueExt when 'valueExt: comparison>
      (typeCheckExpr)
      (expr: Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>)
      : TypeCheckerResult<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>, 'valueExt> =
      state {
        let loc0 = expr.Location
        let (!) = Expr.InstantiateSyntheticVars typeCheckExpr

        match expr.Expr with
        | ExprRec.RecordDes({ Expr = r; Field = field }) ->
          let! r = !r
          return Expr.RecordDes(r, field, loc0, expr.Scope)
        | ExprRec.RecordWith({ Record = r; Fields = fields }) ->
          let! r = !r
          let! fields = fields |> List.map (fun (k, v) -> !v |> state.Map(fun v -> (k, v))) |> state.All
          return Expr.RecordWith(r, fields, loc0, expr.Scope)
        | ExprRec.RecordCons({ Fields = fields }) ->
          let! fields = fields |> List.map (fun (k, v) -> !v |> state.Map(fun v -> (k, v))) |> state.All
          return Expr.RecordCons(fields, loc0, expr.Scope)
        | ExprRec.TupleCons({ Items = items }) ->
          let! items = items |> List.map (!) |> state.All
          return Expr.TupleCons(items, loc0, expr.Scope)
        | ExprRec.TupleDes({ Tuple = t; Item = item }) ->
          let! t = !t
          return Expr.TupleDes(t, item, loc0, expr.Scope)
        | ExprRec.SumCons c -> return Expr.SumCons(c.Selector, loc0, expr.Scope)
        | ExprRec.SumDes({ Handlers = handlers }) ->
          let! handlers =
            handlers
            |> Map.map (fun _ (vn, v) -> !v |> state.Map(fun v -> (vn, v)))
            |> state.AllMap

          return Expr.SumDes(handlers, loc0, expr.Scope)
        | ExprRec.UnionDes({ Handlers = handlers
                             Fallback = fallback }) ->
          let! handlers =
            handlers
            |> Map.map (fun _ (vn, v) -> !v |> state.Map(fun v -> (vn, v)))
            |> state.AllMap

          let! fallback = fallback |> Option.map (!) |> state.RunOption
          return Expr.UnionDes(handlers, fallback, loc0, expr.Scope)
        | ExprRec.Let({ Var = v
                        Type = t
                        Val = value
                        Rest = body }) ->
          let! value = !value
          let! body = !body
          return Expr.Let(v, t, value, body)
        | ExprRec.Lambda({ Param = p; Body = b; ParamType = pt }) ->
          let! b = !b
          return Expr.Lambda(p, pt, b)
        | ExprRec.If({ Cond = c; Then = t; Else = e }) ->
          let! c = !c
          let! t = !t
          let! e = !e
          return Expr.If(c, t, e, loc0, expr.Scope)
        | ExprRec.Apply({ F = f; Arg = a }) ->
          let! f = !f
          let! a = !a
          return Expr.Apply(f, a, loc0, expr.Scope)
        | ExprRec.TypeApply({ Func = f
                              TypeArg = TypeValue.Var(t_var) as t_arg }) when t_var.Synthetic ->
          let! e = !f

          let! t_arg =
            t_arg
            |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
            |> Expr.liftInstantiation

          let res = Expr.TypeApply(e, t_arg)

          return res
        | ExprRec.Primitive p -> return Expr.Primitive(p, loc0, expr.Scope)
        | ExprRec.Lookup l -> return Expr.Lookup(l.Id, loc0, expr.Scope)
        | ExprRec.TypeLambda({ Param = p; Body = b }) ->
          let! b = !b
          return Expr.TypeLambda(p, b, loc0, expr.Scope)
        | ExprRec.TypeApply({ Func = f; TypeArg = t }) ->
          let! f = !f
          return Expr.TypeApply(f, t, loc0, expr.Scope)
        | ExprRec.TypeLet({ Name = n; TypeDef = td; Body = b }) ->
          let! b = !b
          return Expr.TypeLet(n, td, b, loc0, expr.Scope)
        | ExprRec.FromValue({ Value = v
                              ValueType = t
                              ValueKind = k }) -> return Expr.FromValue(v, t, k, loc0, expr.Scope)
        | ExprRec.EntitiesDes({ Expr = e }) ->
          let! e = !e
          return Expr.EntitiesDes(e, loc0, expr.Scope)
        | ExprRec.RelationsDes({ Expr = e }) ->
          let! e = !e
          return Expr.RelationsDes(e, loc0, expr.Scope)
        | ExprRec.EntityDes({ Expr = e; EntityName = entityName }) ->
          let! e = !e
          return Expr.EntityDes(e, entityName, loc0, expr.Scope)
        | ExprRec.RelationDes({ Expr = e
                                RelationName = relationName }) ->
          let! e = !e
          return Expr.RelationDes(e, relationName, loc0, expr.Scope)
        | ExprRec.RelationLookupDes({ Expr = e
                                      RelationName = relationName
                                      Direction = direction }) ->
          let! e = !e
          return Expr.RelationLookupDes(e, relationName, direction, loc0, expr.Scope)
      }
