namespace Ballerina.DSL.Next.Types

[<AutoOpen>]
module TypeCheck =
  open Ballerina.StdLib.String
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
      Values: Map<Identifier, TypeValue * Kind> }

  type TypeCheckState =
    { Types: TypeExprEvalState
      Vars: UnificationState }

  type TypeCheckerResult<'r> = State<'r, TypeCheckContext, TypeCheckState, Errors>
  type TypeChecker = Expr<TypeExpr> -> TypeCheckerResult<Expr<TypeValue> * TypeValue * Kind>

  type TypeCheckContext with
    static member Empty: TypeCheckContext =
      { Types = TypeExprEvalContext.Empty
        Values = Map.empty }

    static member Getters =
      {| Types = fun (c: TypeCheckContext) -> c.Types
         Values = fun (c: TypeCheckContext) -> c.Values |}

    static member TryFindVar(id: Identifier) : TypeCheckerResult<TypeValue * Kind> =
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

    static member TryFindType(id: Identifier) : TypeCheckerResult<TypeValue * Kind> =
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


  type Expr<'T> with
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
        let (!) = Expr<'T>.TypeCheck

        state {
          match t with
          | Expr.Primitive(PrimitiveValue.Int v) ->
            return Expr.Primitive(PrimitiveValue.Int v), TypeValue.Primitive(PrimitiveType.Int32), Kind.Star

          | Expr.Primitive(PrimitiveValue.Bool v) ->
            return Expr.Primitive(PrimitiveValue.Bool v), TypeValue.Primitive(PrimitiveType.Bool), Kind.Star

          | Expr.Primitive(PrimitiveValue.Date v) ->
            return Expr.Primitive(PrimitiveValue.Date v), TypeValue.Primitive(PrimitiveType.DateOnly), Kind.Star

          | Expr.Primitive(PrimitiveValue.DateTime v) ->
            return Expr.Primitive(PrimitiveValue.DateTime v), TypeValue.Primitive(PrimitiveType.DateTime), Kind.Star

          | Expr.Primitive(PrimitiveValue.Decimal v) ->
            return Expr.Primitive(PrimitiveValue.Decimal v), TypeValue.Primitive(PrimitiveType.Decimal), Kind.Star

          | Expr.Primitive(PrimitiveValue.Guid v) ->
            return Expr.Primitive(PrimitiveValue.Guid v), TypeValue.Primitive(PrimitiveType.Guid), Kind.Star

          | Expr.Primitive(PrimitiveValue.String v) ->
            return Expr.Primitive(PrimitiveValue.String v), TypeValue.Primitive(PrimitiveType.String), Kind.Star

          | Expr.Primitive(PrimitiveValue.Unit) ->
            return Expr.Primitive(PrimitiveValue.Unit), TypeValue.Primitive(PrimitiveType.Unit), Kind.Star

          | Expr.Lookup id ->
            let! t_id, id_k = TypeCheckContext.TryFindVar id
            return Expr.Lookup id, t_id, id_k

          | Expr.Apply(f, a) ->
            return!
              state {
                let! f, t_f, f_k = !f
                let! a, t_a, a_k = !a
                do! f_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                do! a_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                let! (f_input, f_output) = TypeValue.AsArrow t_f |> state.OfSum

                do! TypeValue.Unify(f_input, t_a) |> Expr<'T>.liftUnification

                let! f_output = f_output |> TypeValue.Instantiate |> Expr<'T>.liftInstantiation

                return Expr.Apply(f, a), f_output, Kind.Star
              }
              |> state.MapError(Errors.Map(String.appendNewline $"...when typechecking `{f} {a} `"))

          | Expr.If(cond, thenBranch, elseBranch) ->
            return!
              state {
                let! cond, t_cond, cond_k = !cond
                do! cond_k |> Kind.AsStar |> state.OfSum |> state.Ignore

                do!
                  TypeValue.Unify(t_cond, TypeValue.Primitive(PrimitiveType.Bool))
                  |> Expr<'T>.liftUnification

                let! thenBranch, t_then, then_k = !thenBranch
                let! elseBranch, t_else, else_k = !elseBranch
                do! then_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                do! else_k |> Kind.AsStar |> state.OfSum |> state.Ignore

                do! TypeValue.Unify(t_then, t_else) |> Expr<'T>.liftUnification
                let! t_then = t_then |> TypeValue.Instantiate |> Expr<'T>.liftInstantiation

                return Expr.If(cond, thenBranch, elseBranch), t_then, Kind.Star
              }
              |> state.MapError(Errors.Map(String.appendNewline $"...when typechecking `if {cond} ...`"))

          | Expr.Let(x, e1, e2) ->
            return!
              state {
                let! e1, t1, k1 = !e1

                let! e2, t2, k2 =
                  !e2
                  |> state.MapContext(TypeCheckContext.Updaters.Values(Map.add (Identifier.LocalScope x.Name) (t1, k1)))

                return Expr.Let(x, e1, e2), t2, k2
              }
              |> state.MapError(Errors.Map(String.appendNewline $"...when typechecking `let {x.Name} = ...`"))

          | Expr.Lambda(x, t, body) ->
            return!
              state {
                let! t =
                  t
                  |> Option.map (fun t -> t |> TypeExpr.Eval |> Expr<'T>.liftTypeEval)
                  |> state.RunOption

                // (p: State<'a, UnificationContext, UnificationState, Errors>)
                // : State<'a, TypeCheckContext, TypeCheckState, Errors> =

                let freshVarType =
                  Option.defaultWith
                    (fun () ->
                      TypeValue.Var(
                        { TypeVar.Name = x.Name
                          Guid = Guid.CreateVersion7() }
                      ),
                      Kind.Star)
                    t

                let! body, t_body, body_k =
                  !body
                  |> state.MapContext(
                    TypeCheckContext.Updaters.Values(Map.add (Identifier.LocalScope x.Name) freshVarType)
                  )

                do! body_k |> Kind.AsStar |> state.OfSum |> state.Ignore

                let! t_x = freshVarType |> fst |> TypeValue.Instantiate |> Expr<'T>.liftInstantiation

                return Expr.Lambda(x, Some t_x, body), TypeValue.Arrow(t_x, t_body), Kind.Star
              }
              |> state.MapError(Errors.Map(String.appendNewline $"...when typechecking `fun {x.Name} -> ...`"))

          | Expr.RecordCons(fields) ->
            return!
              state {
                let! fields =
                  fields
                  |> List.map (fun (k, v) ->
                    state {
                      let! v, t_v, v_k = !v
                      do! v_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                      let! k_s = TypeCheckState.TryFindSymbol k
                      return (k, v), (k_s, t_v)
                    })
                  |> state.All

                let fieldsExpr = fields |> List.map fst
                let fieldsTypes = fields |> List.map snd |> Map.ofList

                return Expr.RecordCons(fieldsExpr), TypeValue.Record(fieldsTypes), Kind.Star
              }
              |> state.MapError(
                Errors.Map(
                  String.appendNewline
                    $"""...when typechecking `{{ {fields |> List.map (fun (id, _) -> id.ToFSharpString + "=...")} }}` = ...`"""
                )
              )

          | Expr.UnionCons(cons, value) ->
            return!
              state {
                let! cons_symbol = TypeCheckState.TryFindSymbol cons
                let! union_t, union_k = TypeCheckState.TryFindType cons
                do! union_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                let! cases = union_t |> TypeValue.AsUnion |> state.OfSum

                let! case_t =
                  cases
                  |> Map.tryFindWithError cons_symbol "cases" cons.ToFSharpString
                  |> state.OfSum

                let! value, t_value, value_k = !value
                do! value_k |> Kind.AsStar |> state.OfSum |> state.Ignore

                do! TypeValue.Unify(t_value, case_t) |> Expr.liftUnification

                let! union_t = union_t |> TypeValue.Instantiate |> Expr.liftInstantiation

                return Expr.UnionCons(cons, value), union_t, Kind.Star
              }
              |> state.MapError(
                Errors.Map(
                  String.appendNewline
                    $"""...when typechecking `{cons.ToFSharpString}({value.ToFSharpString.ReasonablyClamped})` = ...`"""
                )
              )

          | Expr.TupleCons(fields) ->
            return!
              state {
                let! fields =
                  fields
                  |> List.map (fun (v) ->
                    state {
                      let! v, t_v, v_k = !v
                      do! v_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                      return v, t_v
                    })
                  |> state.All

                let fieldsExpr = fields |> List.map fst
                let fieldsTypes = fields |> List.map snd

                return Expr.TupleCons(fieldsExpr), TypeValue.Tuple(fieldsTypes), Kind.Star
              }
              |> state.MapError(
                Errors.Map(
                  String.appendNewline
                    $"""...when typechecking `(( {fields |> List.map (fun f -> f.ToFSharpString + ", ")} ))` = ...`"""
                      .ReasonablyClamped

                )
              )

          | Expr.SumCons(cons, value) ->
            return!
              state {
                let! value, t_value, value_k = !value
                do! value_k |> Kind.AsStar |> state.OfSum |> state.Ignore

                let cases =
                  [ 0 .. cons.Count - 1 ]
                  |> List.map (fun i ->
                    if i = cons.Case then
                      t_value
                    else
                      TypeValue.Var(
                        { TypeVar.Name = $"a_{i}_of_{cons.Count}"
                          Guid = Guid.CreateVersion7() }
                      ))

                let sum_t = TypeValue.Sum(cases)

                return Expr.SumCons(cons, value), sum_t, Kind.Star
              }
              |> state.MapError(
                Errors.Map(
                  String.appendNewline
                    $"""...when typechecking `case{cons.Case}of{cons.Count}({value.ToFSharpString.ReasonablyClamped})` = ...`"""
                )
              )

          | Expr.RecordDes(fields, fieldName) ->
            return!
              state {
                let! fields, t_fields, fields_k = !fields
                do! fields_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                let! t_fields = t_fields |> TypeValue.AsRecord |> state.OfSum

                return!
                  state.Either
                    (state {
                      let! field_symbol = TypeCheckState.TryFindSymbol fieldName

                      let! t_field =
                        t_fields
                        |> Map.tryFindWithError field_symbol "fields" fieldName.ToFSharpString
                        |> state.OfSum

                      return Expr.RecordDes(fields, fieldName), t_field, Kind.Star
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

                      return Expr.RecordDes(fields, fieldName), t_field, Kind.Star
                    })
              }
              |> state.MapError(
                Errors.Map(
                  String.appendNewline
                    $"""...when typechecking `({fields.ToFSharpString.ReasonablyClamped}).{fieldName.ToFSharpString}` = ...`"""
                )
              )

          | Expr.TupleDes(fields, fieldName) ->
            return!
              state {
                let! fields, t_fields, fields_k = !fields
                do! fields_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                let! t_fields = t_fields |> TypeValue.AsTuple |> state.OfSum

                let! t_field =
                  t_fields
                  |> List.tryItem fieldName.Index
                  |> sum.OfOption(
                    $"Error: cannot find item {fieldName.Index} in tuple {fields}"
                    |> Errors.Singleton
                  )
                  |> state.OfSum

                return Expr.TupleDes(fields, fieldName), t_field, Kind.Star
              }
              |> state.MapError(
                Errors.Map(
                  String.appendNewline
                    $"""...when typechecking `({fields.ToFSharpString.ReasonablyClamped}).item{fieldName.Index}` = ...`"""
                )
              )

          | Expr.UnionDes(handlers) ->
            return!
              state {
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

                      let! body, body_t, body_k =
                        !body
                        |> state.MapContext(
                          TypeCheckContext.Updaters.Values(Map.add (Identifier.LocalScope var.Name) (var_t, Kind.Star))
                        )

                      do! body_k |> Kind.AsStar |> state.OfSum |> state.Ignore

                      do! TypeValue.Unify(body_t, result_t) |> Expr.liftUnification

                      let! var_t = TypeValue.Instantiate var_t |> Expr.liftInstantiation

                      let! k_s = TypeCheckState.TryFindSymbol k

                      return (var, body), (k_s, var_t)
                    })
                  |> state.AllMap

                let handlerExprs = handlers |> Map.map (fun _ -> fst)
                let handlerTypes = handlers |> Map.map (fun _ -> snd) |> Map.values |> Map.ofSeq

                let! result_t = TypeValue.Instantiate result_t |> Expr.liftInstantiation

                return Expr.UnionDes handlerExprs, TypeValue.Arrow(TypeValue.Union(handlerTypes), result_t), Kind.Star
              }
              |> state.MapError(
                Errors.Map(
                  String.appendNewline
                    $"""...when typechecking `match-case {{ {handlers
                                                             |> Map.toSeq
                                                             |> Seq.map (fun (id, (x, _)) -> "| " + id.ToFSharpString + " " + x.Name + " -> ...")} }}` = ...`"""
                )
              )

          | Expr.SumDes(handlers) ->
            return!
              state {
                let result_t =
                  TypeValue.Var(
                    { TypeVar.Name = $"res"
                      Guid = Guid.CreateVersion7() }
                  )

                let! handlers =
                  handlers
                  |> Seq.map (fun (var, body) ->
                    state {
                      let var_t =
                        { TypeVar.Name = var.Name
                          Guid = Guid.CreateVersion7() }

                      do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists var_t))
                      let var_t = TypeValue.Var var_t

                      let! body, body_t, body_k =
                        !body
                        |> state.MapContext(
                          TypeCheckContext.Updaters.Values(Map.add (Identifier.LocalScope var.Name) (var_t, Kind.Star))
                        )

                      do! body_k |> Kind.AsStar |> state.OfSum |> state.Ignore

                      do! TypeValue.Unify(body_t, result_t) |> Expr.liftUnification

                      let! var_t = TypeValue.Instantiate var_t |> Expr.liftInstantiation

                      return (var, body), var_t
                    })
                  |> state.All

                let handlerExprs = handlers |> List.map fst
                let handlerTypes = handlers |> List.map snd

                let! result_t = TypeValue.Instantiate result_t |> Expr.liftInstantiation

                return Expr.SumDes handlerExprs, TypeValue.Arrow(TypeValue.Sum(handlerTypes), result_t), Kind.Star
              }
              |> state.MapError(
                Errors.Map(
                  String.appendNewline
                    $"""...when typechecking `match-case {{ {handlers
                                                             |> Seq.mapi (fun id ((x, _)) -> "| case" + id.ToString() + " " + x.Name + " -> ...")} }}` = ...`"""
                )
              )

          | TypeLet(typeIdentifier, typeDefinition, rest) ->
            return!
              state {
                let! typeDefinition =
                  TypeExpr.Eval typeDefinition
                  |> Expr.liftTypeEval
                  |> state.MapContext(
                    TypeCheckContext.Updaters.Types(
                      TypeExprEvalContext.Updaters.Scope(fun scope -> typeIdentifier :: scope)
                    )
                  )

                do! TypeExprEvalState.bindType typeIdentifier typeDefinition |> Expr.liftTypeEval

                let! definition_cases =
                  typeDefinition
                  |> fst
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
                        do! TypeExprEvalState.bindType k.Name typeDefinition |> Expr.liftTypeEval

                        do! TypeExprEvalState.bindType typeIdentifier typeDefinition |> Expr.liftTypeEval
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
              }
              |> state.MapError(
                Errors.Map(
                  String.appendNewline
                    $"""...when typechecking `type {typeIdentifier} = {typeDefinition.ToFSharpString.ReasonablyClamped} ...`"""
                )
              )

          | Expr.TypeLambda(t_par, body) ->
            return!
              state {
                let fresh_t_par_var =
                  let id = Guid.CreateVersion7()
                  { TypeVar.Name = t_par.Name; Guid = id }

                do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists fresh_t_par_var))

                let! t_par_type =
                  TypeExprEvalState.tryFindType (Identifier.LocalScope t_par.Name)
                  |> state.OfStateReader
                  |> Expr.liftTypeEval
                  |> state.Catch

                // push binding
                do!
                  TypeExprEvalState.bindType t_par.Name (TypeValue.Var fresh_t_par_var, t_par.Kind)
                  |> Expr.liftTypeEval

                let! body, t_body, body_k = !body

                // pop binding
                match t_par_type with
                | Left t_par_type -> do! TypeExprEvalState.bindType t_par.Name t_par_type |> Expr.liftTypeEval
                | Right _ -> do! TypeExprEvalState.unbindType t_par.Name |> Expr.liftTypeEval

                // cleanup unification state, slightly more radical than pop
                do!
                  TypeValue.EquivalenceClassesOp(UnificationState.TryDeleteFreeVariable fresh_t_par_var)
                  |> Expr.liftUnification

                return
                  Expr.TypeLambda(t_par, body), TypeValue.Lambda(t_par, t_body.AsExpr), Kind.Arrow(t_par.Kind, body_k)
              }
              |> state.MapError(
                Errors.Map(
                  String.appendNewline
                    $"""...when typechecking `fun {t_par.Name} => {body.ToFSharpString.ReasonablyClamped} ...`"""
                )
              )

          | Expr.TypeApply(f, t) ->
            return!
              state {
                let! f, f_t, f_k = !f

                let! f_k_i, f_k_o = f_k |> Kind.AsArrow |> state.OfSum
                let! t_val, t_k = t |> TypeExpr.Eval |> Expr.liftTypeEval

                if f_k_i <> t_k then
                  return!
                    $"Error: mismatched kind, expected {f_k_i} but got {t_k}"
                    |> Errors.Singleton
                    |> state.Throw
                else
                  let! f_res, _ = TypeExpr.Apply(f_t.AsExpr, t_val.AsExpr) |> TypeExpr.Eval |> Expr.liftTypeEval

                  return Expr.TypeApply(f, t_val), f_res, f_k_o
              }
              |> state.MapError(
                Errors.Map(
                  String.appendNewline
                    $"""...when typechecking `{f.ToFSharpString.ReasonablyClamped}[{t.ToFSharpString.ReasonablyClamped}] ...`"""
                )
              )
        }
