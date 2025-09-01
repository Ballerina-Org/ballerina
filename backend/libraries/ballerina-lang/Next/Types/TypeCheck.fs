namespace Ballerina.DSL.Next.Types

[<AutoOpen>]
module TypeCheck =
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.Errors
  open System
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Unification
  open Eval
  open Ballerina.Fun

  type TypeCheckContext =
    { Types: TypeExprEvalContext
      Values: Map<Identifier, TypeValue> }

  type TypeCheckState =
    { Types: TypeExprEvalState
      Vars: UnificationState }

  type TypeCheckerResult<'r> = State<'r, TypeCheckContext, TypeCheckState, Errors>
  type TypeChecker = Expr<TypeExpr> -> TypeCheckerResult<Expr<TypeValue> * TypeValue>

  type TypeCheckContext with
    static member Empty: TypeCheckContext =
      { Types = TypeExprEvalContext.Empty
        Values = Map.empty }

    static member Getters =
      {| Types = fun (c: TypeCheckContext) -> c.Types
         Values = fun (c: TypeCheckContext) -> c.Values |}

    static member TryFindVar(id: Identifier) : TypeCheckerResult<TypeValue> =
      state {
        let! ctx = state.GetContext()

        return!
          ctx.Values
          |> Map.tryFindWithError id "variables" id.ToFSharpString
          |> state.OfSum
      }

    static member Updaters =
      {| Types = fun u (c: TypeCheckContext) -> { c with Types = c.Types |> u }
         Values = fun u (c: TypeCheckContext) -> { c with Values = c.Values |> u } |}

  type TypeCheckState with
    static member Empty: TypeCheckState =
      { Types = TypeExprEvalState.Empty
        Vars = UnificationState.Empty }

    static member Getters =
      {| Types = fun (c: TypeCheckState) -> c.Types
         Vars = fun (c: TypeCheckState) -> c.Vars |}

    static member ToInstantiationContext(ctx: TypeCheckState) : TypeInstantiateContext =
      { Bindings = ctx.Types
        VisitedVars = Set.empty }

    static member TryFindSymbol(id: Identifier) : TypeCheckerResult<TypeSymbol> =
      state {
        let! ctx = state.GetState()

        return!
          ctx.Types.Symbols
          |> Map.tryFindWithError id "symbols" id.ToFSharpString
          |> state.OfSum
      }

    static member TryFindType(id: Identifier) : TypeCheckerResult<TypeValue> =
      state {
        let! ctx = state.GetState()

        return!
          ctx.Types.Bindings
          |> Map.tryFindWithError id "symbols" id.ToFSharpString
          |> state.OfSum
      }

    static member Updaters =
      {| Types = fun u (c: TypeCheckState) -> { c with Types = c.Types |> u }
         Vars = fun (u: Updater<UnificationState>) (c: TypeCheckState) -> { c with Vars = c.Vars |> u } |}


  type TypeExpr with
    static member private liftUnification
      (p: State<'a, UnificationContext, UnificationState, Errors>)
      : State<'a, TypeCheckContext, TypeCheckState, Errors> =
      state {
        let! s = state.GetState()

        let newUnificationState = p |> State.Run(s.Types, s.Vars)

        match newUnificationState with
        | Left(res, newUnificationState) ->
          do!
            newUnificationState
            |> Option.map (fun (newUnificationState: UnificationState) ->
              state.SetState(TypeCheckState.Updaters.Vars(replaceWith newUnificationState)))
            |> state.RunOption
            |> state.Map ignore

          return res
        | Right(err, _) -> return! state.Throw err
      }

    static member liftTypeEval
      (p: State<'a, TypeExprEvalContext, TypeExprEvalState, Errors>)
      : State<'a, TypeCheckContext, TypeCheckState, Errors> =
      state {
        let! s = state.GetState()
        let! ctx = state.GetContext()

        let newTypesState = p |> State.Run(ctx.Types, s.Types)

        match newTypesState with
        | Left(res, newTypesState) ->
          do!
            newTypesState
            |> Option.map (fun (newTypesState: TypeExprEvalState) ->
              state.SetState(TypeCheckState.Updaters.Types(replaceWith newTypesState)))
            |> state.RunOption
            |> state.Map ignore

          return res
        | Right(err, _) -> return! state.Throw err
      }

    static member private liftInstantiation
      (p: State<'a, TypeInstantiateContext, UnificationState, Errors>)
      : State<'a, TypeCheckContext, TypeCheckState, Errors> =
      state {
        let! s = state.GetState()

        let newUnificationState =
          p |> State.Run(s |> TypeCheckState.ToInstantiationContext, s.Vars)

        match newUnificationState with
        | Left(res, newUnificationState) ->
          do!
            newUnificationState
            |> Option.map (fun (newUnificationState: UnificationState) ->
              state.SetState(TypeCheckState.Updaters.Vars(replaceWith newUnificationState)))
            |> state.RunOption
            |> state.Map ignore

          return res
        | Right(err, _) -> return! state.Throw err
      }

    static member TypeCheck: TypeChecker =
      fun t ->
        let (!) = TypeExpr.TypeCheck

        state {
          match t with
          | Expr.Primitive(PrimitiveValue.Int v) ->
            return Expr.Primitive(PrimitiveValue.Int v), TypeValue.Primitive(PrimitiveType.Int32)

          | Expr.Primitive(PrimitiveValue.Bool v) ->
            return Expr.Primitive(PrimitiveValue.Bool v), TypeValue.Primitive(PrimitiveType.Bool)

          | Expr.Primitive(PrimitiveValue.Date v) ->
            return Expr.Primitive(PrimitiveValue.Date v), TypeValue.Primitive(PrimitiveType.DateOnly)

          | Expr.Primitive(PrimitiveValue.DateTime v) ->
            return Expr.Primitive(PrimitiveValue.DateTime v), TypeValue.Primitive(PrimitiveType.DateTime)

          | Expr.Primitive(PrimitiveValue.Decimal v) ->
            return Expr.Primitive(PrimitiveValue.Decimal v), TypeValue.Primitive(PrimitiveType.Decimal)

          | Expr.Primitive(PrimitiveValue.Guid v) ->
            return Expr.Primitive(PrimitiveValue.Guid v), TypeValue.Primitive(PrimitiveType.Guid)

          | Expr.Primitive(PrimitiveValue.String v) ->
            return Expr.Primitive(PrimitiveValue.String v), TypeValue.Primitive(PrimitiveType.String)

          | Expr.Primitive(PrimitiveValue.Unit) ->
            return Expr.Primitive(PrimitiveValue.Unit), TypeValue.Primitive(PrimitiveType.Unit)

          | Expr.Lookup id ->
            let! t_id = TypeCheckContext.TryFindVar id
            return Expr.Lookup id, t_id

          | Expr.Apply(f, a) ->
            let! f, t_f = TypeExpr.TypeCheck f
            let! a, t_a = TypeExpr.TypeCheck a
            let! (f_input, f_output) = TypeValue.AsArrow t_f |> state.OfSum

            do! TypeValue.Unify(f_input, t_a) |> TypeExpr.liftUnification

            let! f_output = f_output |> TypeValue.Instantiate |> TypeExpr.liftInstantiation

            return Expr.Apply(f, a), f_output

          | Expr.If(cond, thenBranch, elseBranch) ->
            let! cond, t_cond = TypeExpr.TypeCheck cond

            do!
              TypeValue.Unify(t_cond, TypeValue.Primitive(PrimitiveType.Bool))
              |> TypeExpr.liftUnification

            let! thenBranch, t_then = TypeExpr.TypeCheck thenBranch
            let! elseBranch, t_else = TypeExpr.TypeCheck elseBranch

            do! TypeValue.Unify(t_then, t_else) |> TypeExpr.liftUnification
            let! t_then = t_then |> TypeValue.Instantiate |> TypeExpr.liftInstantiation

            return Expr.If(cond, thenBranch, elseBranch), t_then

          | Expr.Let(x, e1, e2) ->
            let! e1, t1 = e1 |> TypeExpr.TypeCheck

            let! e2, t2 =
              e2
              |> TypeExpr.TypeCheck
              |> state.MapContext(TypeCheckContext.Updaters.Values(Map.add (Identifier.LocalScope x.Name) t1))

            return Expr.Let(x, e1, e2), t2

          | Expr.Lambda(x, t, body) ->
            let! t =
              t
              |> Option.map (fun t -> t |> TypeExpr.Eval |> TypeExpr.liftTypeEval)
              |> state.RunOption

            // (p: State<'a, UnificationContext, UnificationState, Errors>)
            // : State<'a, TypeCheckContext, TypeCheckState, Errors> =

            let freshVarType =
              Option.defaultWith
                (fun () ->
                  TypeValue.Var(
                    { TypeVar.Name = x.Name
                      Guid = Guid.CreateVersion7() }
                  ))
                t

            let! body, t_body =
              body
              |> TypeExpr.TypeCheck
              |> state.MapContext(TypeCheckContext.Updaters.Values(Map.add (Identifier.LocalScope x.Name) freshVarType))

            let! t_x = freshVarType |> TypeValue.Instantiate |> TypeExpr.liftInstantiation

            return Expr.Lambda(x, Some t_x, body), TypeValue.Arrow(t_x, t_body)

          | Expr.RecordCons(fields) ->
            let! fields =
              fields
              |> List.map (fun (k, v) ->
                state {
                  let! v, t_v = !v
                  let! k_s = TypeCheckState.TryFindSymbol k
                  return (k, v), (k_s, t_v)
                })
              |> state.All

            let fieldsExpr = fields |> List.map fst
            let fieldsTypes = fields |> List.map snd |> Map.ofList

            return Expr.RecordCons(fieldsExpr), TypeValue.Record(fieldsTypes)

          | Expr.UnionCons(cons, value) ->
            let! cons_symbol = TypeCheckState.TryFindSymbol cons
            let! union_t = TypeCheckState.TryFindType cons
            let! cases = union_t |> TypeValue.AsUnion |> state.OfSum

            let! case_t =
              cases
              |> Map.tryFindWithError cons_symbol "cases" cons.ToFSharpString
              |> state.OfSum

            let! value, t_value = !value

            do! TypeValue.Unify(t_value, case_t) |> TypeExpr.liftUnification

            let! union_t = union_t |> TypeValue.Instantiate |> TypeExpr.liftInstantiation

            return Expr.UnionCons(cons, value), union_t

          | Expr.TupleCons(fields) ->
            let! fields =
              fields
              |> List.map (fun (v) ->
                state {
                  let! v, t_v = !v
                  return v, t_v
                })
              |> state.All

            let fieldsExpr = fields |> List.map fst
            let fieldsTypes = fields |> List.map snd

            return Expr.TupleCons(fieldsExpr), TypeValue.Tuple(fieldsTypes)

          | Expr.SumCons(cons, value) ->
            let! value, t_value = !value

            let cases =
              [ 1 .. cons.Count ]
              |> List.map (fun i ->
                if i = cons.Case then
                  t_value
                else
                  TypeValue.Var(
                    { TypeVar.Name = $"a_{i}_of_{cons.Count}"
                      Guid = Guid.CreateVersion7() }
                  ))

            let sum_t = TypeValue.Sum(cases)

            return Expr.SumCons(cons, value), sum_t

          | Expr.RecordDes(fields, fieldName) ->
            let! fields, t_fields = !fields
            let! t_fields = t_fields |> TypeValue.AsRecord |> state.OfSum

            return!
              state.Either
                (state {
                  let! field_symbol = TypeCheckState.TryFindSymbol fieldName

                  let! t_field =
                    t_fields
                    |> Map.tryFindWithError field_symbol "fields" fieldName.ToFSharpString
                    |> state.OfSum

                  return Expr.RecordDes(fields, fieldName), t_field
                })
                (state {
                  let! localFieldName = Identifier.AsLocalScope fieldName |> state.OfSum

                  let! t_field =
                    t_fields
                    |> Map.toSeq
                    |> Seq.tryFind (fun (k, _) -> k.Name = localFieldName)
                    |> sum.OfOption($"Error: cannot find symbol {fieldName}" |> Errors.Singleton)
                    |> state.OfSum
                    |> state.Map snd

                  return Expr.RecordDes(fields, fieldName), t_field
                })


          | Expr.TupleDes(fields, fieldName) ->
            let! fields, t_fields = !fields
            let! t_fields = t_fields |> TypeValue.AsTuple |> state.OfSum

            let! t_field =
              t_fields
              |> List.tryItem fieldName.Index
              |> sum.OfOption(
                $"Error: cannot find item {fieldName.Index} in tuple {fields}"
                |> Errors.Singleton
              )
              |> state.OfSum

            return Expr.TupleDes(fields, fieldName), t_field

          | Expr.UnionDes(handlers) ->
            let result_t =
              { TypeVar.Name = $"res"
                Guid = Guid.CreateVersion7() }

            do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists result_t))
            let result_t = result_t |> TypeValue.Var

            let! handlers =
              handlers
              |> Map.map (fun k (var, body) ->
                state {
                  let var_t =
                    { TypeVar.Name = var.Name
                      Guid = Guid.CreateVersion7() }

                  do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists var_t))
                  let var_t = var_t |> TypeValue.Var

                  let! body, body_t =
                    !body
                    |> state.MapContext(
                      TypeCheckContext.Updaters.Values(Map.add (Identifier.LocalScope var.Name) var_t)
                    )

                  do! TypeValue.Unify(body_t, result_t) |> TypeExpr.liftUnification

                  let! var_t = TypeValue.Instantiate var_t |> TypeExpr.liftInstantiation

                  let! k_s = TypeCheckState.TryFindSymbol k

                  return (var, body), (k_s, var_t)
                })
              |> state.AllMap

            let handlerExprs = handlers |> Map.map (fun _ -> fst)
            let handlerTypes = handlers |> Map.map (fun _ -> snd) |> Map.values |> Map.ofSeq

            let! result_t = TypeValue.Instantiate result_t |> TypeExpr.liftInstantiation

            return Expr.UnionDes handlerExprs, TypeValue.Arrow(TypeValue.Union(handlerTypes), result_t)

          | Expr.SumDes(handlers) ->
            let result_t =
              TypeValue.Var(
                { TypeVar.Name = $"res"
                  Guid = Guid.CreateVersion7() }
              )

            let! handlers =
              handlers
              |> Map.map (fun _ (var, body) ->
                state {
                  let var_t =
                    { TypeVar.Name = var.Name
                      Guid = Guid.CreateVersion7() }

                  do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists var_t))
                  let var_t = TypeValue.Var var_t

                  let! body, body_t =
                    !body
                    |> state.MapContext(
                      TypeCheckContext.Updaters.Values(Map.add (Identifier.LocalScope var.Name) var_t)
                    )

                  do! TypeValue.Unify(body_t, result_t) |> TypeExpr.liftUnification

                  let! var_t = TypeValue.Instantiate var_t |> TypeExpr.liftInstantiation

                  return (var, body), var_t
                })
              |> state.AllMap

            let handlerExprs = handlers |> Map.map (fun _ -> fst)
            let handlerTypes = handlers |> Map.map (fun _ -> snd)

            let! result_t = TypeValue.Instantiate result_t |> TypeExpr.liftInstantiation

            return
              Expr.SumDes handlerExprs,
              TypeValue.Arrow(
                TypeValue.Sum(handlerTypes |> Map.toSeq |> Seq.sortBy fst |> Seq.map snd |> Seq.toList),
                result_t
              )

          | TypeLet(typeIdentifier, typeDefinition, rest) ->
            let! typeDefinition =
              TypeExpr.Eval typeDefinition
              |> TypeExpr.liftTypeEval
              |> state.MapContext(
                TypeCheckContext.Updaters.Types(
                  TypeExprEvalContext.Updaters.Scope(fun scope -> typeIdentifier :: scope)
                )
              )

            do!
              TypeExprEvalState.bindType typeIdentifier typeDefinition
              |> TypeExpr.liftTypeEval

            let! definition_cases =
              typeDefinition
              |> TypeValue.AsUnion
              |> state.OfSum
              |> state.Catch
              |> state.Map(Sum.toOption)

            do!
              definition_cases
              |> Option.map (fun definition_cases ->
                definition_cases
                |> Map.toSeq
                |> Seq.map (fun (k, _) ->
                  state {
                    do! TypeExprEvalState.bindType k.Name typeDefinition |> TypeExpr.liftTypeEval

                    do!
                      TypeExprEvalState.bindType typeIdentifier typeDefinition
                      |> TypeExpr.liftTypeEval
                  })
                |> state.All
                |> state.Map ignore)
              |> state.RunOption
              |> state.Map ignore
              |> state.MapContext(
                TypeCheckContext.Updaters.Types(
                  TypeExprEvalContext.Updaters.Scope(fun scope -> typeIdentifier :: scope)
                )
              )

            return! !rest

          | Expr.TypeLambda(t_par, body) ->
            let fresh_t_par_var =
              let id = Guid.CreateVersion7()
              { TypeVar.Name = t_par.Name; Guid = id }

            do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists fresh_t_par_var))

            let! t_par_type =
              TypeExprEvalState.tryFindType (Identifier.LocalScope t_par.Name)
              |> state.OfStateReader
              |> TypeExpr.liftTypeEval
              |> state.Catch

            // push binding
            do!
              TypeExprEvalState.bindType t_par.Name (TypeValue.Var fresh_t_par_var)
              |> TypeExpr.liftTypeEval

            let! body, t_body = body |> TypeExpr.TypeCheck

            // pop binding
            match t_par_type with
            | Left t_par_type -> do! TypeExprEvalState.bindType t_par.Name t_par_type |> TypeExpr.liftTypeEval
            | Right _ -> do! TypeExprEvalState.unbindType t_par.Name |> TypeExpr.liftTypeEval

            // cleanup unification state, slightly more radical than pop
            do!
              TypeValue.EquivalenceClassesOp(UnificationState.TryDeleteFreeVariable fresh_t_par_var)
              |> TypeExpr.liftUnification

            return Expr.TypeLambda(t_par, body), TypeValue.Lambda(t_par, t_body.AsExpr)

          | Expr.TypeApply(f, t) ->
            let! f, f_t = !f
            let! t_val = t |> TypeExpr.Eval |> TypeExpr.liftTypeEval

            let! f_res =
              TypeExpr.Apply(f_t.AsExpr, t_val.AsExpr)
              |> TypeExpr.Eval
              |> TypeExpr.liftTypeEval

            return Expr.TypeApply(f, t_val), f_res
        }
