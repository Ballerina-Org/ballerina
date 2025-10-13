namespace Ballerina.DSL.Next.Types

[<AutoOpen>]
module TypeCheck =
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
  open Ballerina.DSL.Next.Types.AdHocPolymorphicOperators
  open Eval
  open Ballerina.Fun
  open System.Text.RegularExpressions
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Collections.NonEmptyList


  type TypeCheckContext =
    { Types: TypeExprEvalContext
      Values: Map<Identifier, TypeValue * Kind> }

  type TypeCheckState =
    { Types: TypeExprEvalState
      Vars: UnificationState }

  type TypeCheckerResult<'r> = State<'r, TypeCheckContext, TypeCheckState, Errors>
  type TypeChecker = Option<TypeValue> -> Expr<TypeExpr> -> TypeCheckerResult<Expr<TypeValue> * TypeValue * Kind>

  type TypeCheckContext with
    static member Empty: TypeCheckContext =
      { Types = TypeExprEvalContext.Empty
        Values = Map.empty }

    static member Getters =
      {| Types = fun (c: TypeCheckContext) -> c.Types
         Values = fun (c: TypeCheckContext) -> c.Values |}

    static member TryFindVar(id: Identifier, loc: Location) : TypeCheckerResult<TypeValue * Kind> =
      state {
        let! ctx = state.GetContext()

        return!
          ctx.Values
          |> Map.tryFindWithError id "variables" id.ToFSharpString loc
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

    static member TryFindTypeSymbol(id: Identifier, loc: Location) : TypeCheckerResult<TypeSymbol> =
      state {
        let! ctx = state.GetState()

        return!
          ctx.Types.Symbols.Types
          |> Map.tryFindWithError id "symbols" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindRecordFieldSymbol(id: Identifier, loc: Location) : TypeCheckerResult<TypeSymbol> =
      state {
        let! ctx = state.GetState()

        return!
          ctx.Types.Symbols.RecordFields
          |> Map.tryFindWithError id "record fields" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindUnionCaseSymbol(id: Identifier, loc: Location) : TypeCheckerResult<TypeSymbol> =
      state {
        let! ctx = state.GetState()

        return!
          ctx.Types.Symbols.UnionCases
          |> Map.tryFindWithError id "union cases" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindType(id: Identifier, loc: Location) : TypeCheckerResult<TypeValue * Kind> =
      state {
        let! ctx = state.GetState()

        return!
          ctx.Types.Bindings
          |> Map.tryFindWithError id "type bindings" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindUnionCaseConstructor
      (id: Identifier, loc: Location)
      : TypeCheckerResult<TypeValue * OrderedMap<TypeSymbol, TypeValue>> =
      state {
        let! ctx = state.GetState()

        return!
          ctx.Types.UnionCases
          |> Map.tryFindWithError id "union cases" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindRecordField
      (id: Identifier, loc: Location)
      : TypeCheckerResult<OrderedMap<TypeSymbol, TypeValue> * TypeValue> =
      state {
        let! ctx = state.GetState()

        return!
          ctx.Types.RecordFields
          |> Map.tryFindWithError id "record fields" id.ToFSharpString loc
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
      fun context_t t ->
        let (!) = Expr<'T>.TypeCheck context_t
        let (=>) c e = Expr<'T>.TypeCheck c e

        let loc0 = t.Location
        let error e = Errors.Singleton(loc0, e)

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          match t.Expr with
          | ExprRec.Primitive(PrimitiveValue.Int32 v) ->
            return
              Expr.Primitive(PrimitiveValue.Int32 v, loc0), TypeValue.CreatePrimitive PrimitiveType.Int32, Kind.Star

          | ExprRec.Primitive(PrimitiveValue.Int64 v) ->
            return
              Expr.Primitive(PrimitiveValue.Int64 v, loc0), TypeValue.CreatePrimitive PrimitiveType.Int64, Kind.Star

          | ExprRec.Primitive(PrimitiveValue.Float32 v) ->
            return
              Expr.Primitive(PrimitiveValue.Float32 v, loc0), TypeValue.CreatePrimitive PrimitiveType.Float32, Kind.Star

          | ExprRec.Primitive(PrimitiveValue.Float64 v) ->
            return
              Expr.Primitive(PrimitiveValue.Float64 v, loc0), TypeValue.CreatePrimitive PrimitiveType.Float64, Kind.Star

          | ExprRec.Primitive(PrimitiveValue.Bool v) ->
            return Expr.Primitive(PrimitiveValue.Bool v, loc0), TypeValue.CreatePrimitive PrimitiveType.Bool, Kind.Star

          | ExprRec.Primitive(PrimitiveValue.Date v) ->
            return
              Expr.Primitive(PrimitiveValue.Date v, loc0), TypeValue.CreatePrimitive PrimitiveType.DateOnly, Kind.Star

          | ExprRec.Primitive(PrimitiveValue.DateTime v) ->
            return
              Expr.Primitive(PrimitiveValue.DateTime v, loc0),
              TypeValue.CreatePrimitive PrimitiveType.DateTime,
              Kind.Star

          | ExprRec.Primitive(PrimitiveValue.TimeSpan v) ->
            return
              Expr.Primitive(PrimitiveValue.TimeSpan v, loc0),
              TypeValue.CreatePrimitive PrimitiveType.TimeSpan,
              Kind.Star

          | ExprRec.Primitive(PrimitiveValue.Decimal v) ->
            return
              Expr.Primitive(PrimitiveValue.Decimal v, loc0), TypeValue.CreatePrimitive PrimitiveType.Decimal, Kind.Star

          | ExprRec.Primitive(PrimitiveValue.Guid v) ->
            return Expr.Primitive(PrimitiveValue.Guid v, loc0), TypeValue.CreatePrimitive PrimitiveType.Guid, Kind.Star

          | ExprRec.Primitive(PrimitiveValue.String v) ->
            return
              Expr.Primitive(PrimitiveValue.String v, loc0), TypeValue.CreatePrimitive PrimitiveType.String, Kind.Star

          | ExprRec.Primitive(PrimitiveValue.Unit) ->
            return Expr.Primitive(PrimitiveValue.Unit, loc0), TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star

          | ExprRec.Lookup id ->
            let! t_id, id_k =
              state.Either
                (TypeCheckContext.TryFindVar(id, loc0))
                // (TypeCheckState.TryFindUnionCaseConstructor(id, loc0)
                //  |> state.Map(fun (t, cases) -> TypeValue.CreateArrow(t, TypeValue.CreateUnion(cases)), Kind.Star))
                // (TypeCheckState.TryFindRecordField(id, loc0)
                //  |> state.Map(fun (cases, t) -> TypeValue.CreateArrow(TypeValue.CreateRecord(cases), t), Kind.Star))
                (TypeCheckState.TryFindType(id, loc0))

            return Expr.Lookup(id, loc0), t_id, id_k
          | ExprRec.Apply(f, a_expr) ->
            return!
              state {
                let! a, t_a, a_k = !a_expr
                do! a_k |> Kind.AsStar |> ofSum |> state.Ignore

                return!
                  state.Either
                    (state {
                      let! f_lookup = f |> Expr.AsLookup |> ofSum

                      return!
                        state.Either
                          (state {
                            let! union_cons_t =
                              state.Either
                                (state {
                                  match context_t with
                                  | None ->
                                    return!
                                      (loc0, "Context is not set, skipping branch") |> Errors.Singleton |> state.Throw
                                  | Some context_t ->
                                    let! context_cases = context_t |> TypeValue.AsUnion |> ofSum

                                    let context_cases_by_id =
                                      context_cases.value
                                      |> OrderedMap.toSeq
                                      |> Seq.map (fun (k, v) -> (k.Name, (k, v)))
                                      |> OrderedMap.ofSeq

                                    let! _case_k, case_t =
                                      context_cases_by_id
                                      |> OrderedMap.tryFindWithError f_lookup "cases" f_lookup.ToFSharpString
                                      |> ofSum

                                    return case_t, context_cases.value
                                })
                                (state { return! TypeCheckState.TryFindUnionCaseConstructor(f_lookup, loc0) })

                            let f_i = union_cons_t |> fst
                            let f_o = union_cons_t |> snd |> TypeValue.CreateUnion
                            let f_k = Kind.Star
                            do! TypeValue.Unify(loc0, f_i, t_a) |> Expr<'T>.liftUnification

                            return Expr.Apply(Expr.Lookup f_lookup, a, loc0), f_o, f_k
                          })
                          (state {
                            let! resolved = TypeCheckContext.TryFindVar(f_lookup, loc0) |> state.Catch
                            // ensure we do not apply ad-hoc polymorphism to bound variables
                            do!
                              resolved
                              |> Sum.AsRight
                              |> Sum.fromOption (fun () -> $"Error: variable found, skipping branch" |> error)
                              |> state.OfSum
                              |> state.Ignore

                            match f_lookup with
                            | Identifier.LocalScope f_lookup when
                              adHocPolymorphismBinaryAllOperatorNames.Contains f_lookup
                              ->
                              return!
                                state {
                                  let! a_primitive = t_a |> TypeValue.AsPrimitive |> ofSum
                                  let a_primitive = a_primitive.value

                                  let! adHocResolution =
                                    adHocPolymorphismBinary
                                    |> Map.tryFindWithError
                                      (f_lookup, a_primitive)
                                      "ad-hoc polymorphism resolutions"
                                      f_lookup.ToFSharpString
                                      loc0
                                    |> state.OfSum

                                  let! adhoc_op, adhoc_op_t, adhoc_op_k =
                                    !Expr.Lookup(Identifier.FullyQualified([ adHocResolution.Namespace ], f_lookup),
                                                 loc0)

                                  do! adhoc_op_k |> Kind.AsStar |> ofSum |> state.Ignore

                                  do!
                                    TypeValue.Unify(
                                      loc0,
                                      TypeValue.CreateArrow(
                                        TypeValue.CreatePrimitive adHocResolution.MatchedInput,
                                        TypeValue.CreateArrow(
                                          TypeValue.CreatePrimitive adHocResolution.OtherInput,
                                          TypeValue.CreatePrimitive adHocResolution.Output
                                        )
                                      ),
                                      adhoc_op_t
                                    )
                                    |> Expr<'T>.liftUnification

                                  let t_res =
                                    TypeValue.CreateArrow(
                                      TypeValue.CreatePrimitive adHocResolution.OtherInput,
                                      TypeValue.CreatePrimitive adHocResolution.Output
                                    )

                                  let k_res = Kind.Star
                                  return Expr.Apply(adhoc_op, a, loc0), t_res, k_res

                                }
                                |> state.MapError(Errors.SetPriority ErrorPriority.High)
                            | Identifier.LocalScope name when (name = "!") ->
                              return!
                                state {
                                  do!
                                    TypeValue.Unify(loc0, TypeValue.CreatePrimitive PrimitiveType.Bool, t_a)
                                    |> Expr<'T>.liftUnification

                                  return!
                                    state {
                                      let! bool_op, bool_op_t, bool_op_k =
                                        !Expr.Lookup(Identifier.FullyQualified([ "bool" ], name), loc0)

                                      do! bool_op_k |> Kind.AsStar |> ofSum |> state.Ignore

                                      do!
                                        TypeValue.Unify(
                                          loc0,
                                          TypeValue.CreateArrow(
                                            TypeValue.CreatePrimitive PrimitiveType.Bool,
                                            TypeValue.CreatePrimitive PrimitiveType.Bool
                                          ),
                                          bool_op_t
                                        )
                                        |> Expr<'T>.liftUnification

                                      let t_res = TypeValue.CreatePrimitive PrimitiveType.Bool
                                      let k_res = Kind.Star
                                      return Expr.Apply(bool_op, a, loc0), t_res, k_res
                                    }
                                    |> state.MapError(Errors.SetPriority ErrorPriority.High)

                                }
                            | _ ->
                              return!
                                $"Error: cannot resolve with ad-hoc polymorphism, found variable {f_lookup}"
                                |> error
                                |> state.Throw
                                |> state.MapError(Errors.SetPriority ErrorPriority.Low)
                          })
                    })
                    (state {
                      let! t_a_has_structure =
                        state {
                          match t_a with
                          | TypeValue.Var _ -> return false
                          | _ -> return true
                        }

                      let f_constraint =
                        if t_a_has_structure |> not then
                          None
                        else
                          let guid = Guid.CreateVersion7()

                          let freshVar =
                            { TypeVar.Name = "_application_" + guid.ToString()
                              Guid = guid }
                            |> TypeValue.Var

                          Some(TypeValue.CreateArrow(t_a, freshVar))

                      let rec pad (f_expr: Expr<TypeExpr>) =
                        state {
                          let! f, t_f, f_k = f_constraint => f_expr

                          match f_k with
                          | Kind.Arrow(Kind.Star, _) ->
                            let guid = Guid.CreateVersion7()

                            let freshVar =
                              { TypeVar.Name = "fresh_var_application_" + guid.ToString()
                                Guid = guid }

                            do!
                              TypeExprEvalState.bindType (freshVar.Name) (freshVar |> TypeValue.Var, Kind.Star)
                              |> Expr.liftTypeEval

                            return!
                              pad (Expr.TypeApply(f_expr, freshVar.Name |> Identifier.LocalScope |> TypeExpr.Lookup))
                          | _ -> return f, t_f, f_k
                        }

                      let! f, t_f, f_k = pad f

                      do! f_k |> Kind.AsStar |> ofSum |> state.Ignore

                      let! (f_input, f_output) =
                        TypeValue.AsArrow t_f
                        |> ofSum
                        |> state.Map WithTypeExprSourceMapping.Getters.Value
                        |> state.MapError(Errors.SetPriority ErrorPriority.Medium)

                      return!
                        state.Any(
                          state {
                            let! aCasesT = t_a |> TypeValue.AsImportedUnionLike |> ofSum

                            return!
                              state {
                                let! aCasesT =
                                  aCasesT
                                  |> OrderedMap.map (fun _ -> TypeExpr.Eval None loc0 >> Expr<'T>.liftTypeEval)
                                  |> state.AllMapOrdered

                                let aCasesT = aCasesT |> OrderedMap.map (fun _ -> fst)

                                do!
                                  TypeValue.Unify(loc0, f_input, TypeValue.CreateUnion aCasesT)
                                  |> Expr<'T>.liftUnification

                                let! f_output = f_output |> TypeValue.Instantiate loc0 |> Expr<'T>.liftInstantiation
                                return Expr.Apply(f, a, loc0), f_output, Kind.Star
                              }
                              |> state.MapError(Errors.SetPriority ErrorPriority.High)
                          },
                          [ state {
                              do! TypeValue.Unify(loc0, f_input, t_a) |> Expr<'T>.liftUnification

                              let! f_output = f_output |> TypeValue.Instantiate loc0 |> Expr<'T>.liftInstantiation
                              return Expr.Apply(f, a, loc0), f_output, Kind.Star
                            }
                            |> state.MapError(Errors.SetPriority ErrorPriority.High)
                            // $"Error: cannot resolve application"
                            // |> error
                            // |> state.Throw
                            // |> state.MapError(Errors.SetPriority ErrorPriority.Medium)
                            ]
                        )
                        |> state.MapError Errors.FilterHighestPriorityOnly
                    })


              }
              |> state.MapError(Errors.FilterHighestPriorityOnly)

          // |> state.MapError(Errors.Map(String.appendNewline $"...when typechecking `{f} {a_expr} `"))

          | ExprRec.If(cond, thenBranch, elseBranch) ->
            return!
              state {
                let! cond, t_cond, cond_k = !cond
                do! cond_k |> Kind.AsStar |> ofSum |> state.Ignore

                do!
                  TypeValue.Unify(loc0, t_cond, TypeValue.CreatePrimitive PrimitiveType.Bool)
                  |> Expr<'T>.liftUnification

                let! thenBranch, t_then, then_k = !thenBranch
                let! elseBranch, t_else, else_k = !elseBranch
                do! then_k |> Kind.AsStar |> ofSum |> state.Ignore
                do! else_k |> Kind.AsStar |> ofSum |> state.Ignore

                do! TypeValue.Unify(loc0, t_then, t_else) |> Expr<'T>.liftUnification
                let! t_then = t_then |> TypeValue.Instantiate loc0 |> Expr<'T>.liftInstantiation

                return Expr.If(cond, thenBranch, elseBranch, loc0), t_then, Kind.Star
              }
          // |> state.MapError(Errors.Map(String.appendNewline $"...when typechecking `if {cond} ...`"))

          | ExprRec.Let(x, x_type, e1, e2) ->
            return!
              state {
                let! x_type =
                  x_type
                  |> Option.map (TypeExpr.Eval None loc0 >> Expr<'T>.liftTypeEval)
                  |> state.RunOption

                let! e1, t1, k1 = (x_type |> Option.map fst) => e1

                match x_type with
                | Some(x_type, x_type_kind) ->
                  do! x_type_kind |> Kind.AsStar |> ofSum |> state.Ignore
                  do! TypeValue.Unify(loc0, t1, x_type) |> Expr<'T>.liftUnification
                | _ -> ()

                let! e2, t2, k2 =
                  !e2
                  |> state.MapContext(TypeCheckContext.Updaters.Values(Map.add (Identifier.LocalScope x.Name) (t1, k1)))

                return Expr.Let(x, None, e1, e2, loc0), t2, k2
              }
          // |> state.MapError(Errors.Map(String.appendNewline $"...when typechecking `let {x.Name} = ...`"))

          | ExprRec.Lambda(x, t, body) ->
            return!
              state {
                let! t =
                  t
                  |> Option.map (fun t -> t |> TypeExpr.Eval None loc0 |> Expr<'T>.liftTypeEval)
                  |> state.RunOption

                // (p: State<'a, UnificationContext, UnificationState, Errors>)
                // : State<'a, TypeCheckContext, TypeCheckState, Errors> =

                let guid = Guid.CreateVersion7()

                let freshVar =
                  { TypeVar.Name = x.Name + "_lambda_" + guid.ToString()
                    Guid = guid }

                let freshVarType =
                  Option.defaultWith (fun () -> freshVar |> TypeValue.Var, Kind.Star) t

                do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists freshVar))

                let! body, t_body, body_k =
                  !body
                  |> state.MapContext(
                    TypeCheckContext.Updaters.Values(Map.add (Identifier.LocalScope x.Name) freshVarType)
                  )

                do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

                let! t_x = freshVarType |> fst |> TypeValue.Instantiate loc0 |> Expr<'T>.liftInstantiation
                // let! t_body = t_body |> TypeValue.Instantiate loc0 |> Expr<'T>.liftInstantiation

                // do!
                //     UnificationState.DeleteVariable freshVar
                //       |> TypeValue.EquivalenceClassesOp
                //       |> Expr<'T>.liftUnification

                return Expr.Lambda(x, Some t_x, body, loc0), TypeValue.CreateArrow(t_x, t_body), Kind.Star
              }
          // |> state.MapError(Errors.Map(String.appendNewline $"...when typechecking `fun {x.Name} -> ...`"))

          | ExprRec.RecordCons(fields) ->
            return!
              state {
                let! fields =
                  state {
                    match context_t with
                    | None ->
                      return!
                        fields
                        |> List.map (fun (k, v) ->
                          state {
                            let! v, t_v, v_k = !v
                            do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                            let! k_s = TypeCheckState.TryFindRecordFieldSymbol(k, loc0)
                            return (k, v), (k_s, t_v)
                          })
                        |> state.All
                    | Some context_t ->
                      let! context_fields = context_t |> TypeValue.AsRecord |> ofSum

                      let context_fields =
                        context_fields.value
                        |> OrderedMap.toSeq
                        |> Seq.map (fun (k, v) -> (k.Name, (k, v)))
                        |> OrderedMap.ofSeq

                      return!
                        fields
                        |> List.map (fun (k, v) ->
                          state {
                            let! k_s, k_t =
                              context_fields
                              |> OrderedMap.tryFindWithError k "fields" k.ToFSharpString
                              |> ofSum

                            let! v, t_v, v_k = (Some k_t) => v
                            do! v_k |> Kind.AsStar |> ofSum |> state.Ignore


                            do! TypeValue.Unify(loc0, t_v, k_t) |> Expr.liftUnification
                            return (k, v), (k_s, t_v)
                          })
                        |> state.All
                  }

                let fieldsExpr = fields |> List.map fst
                let fieldsTypes = fields |> List.map snd |> OrderedMap.ofList

                return Expr.RecordCons(fieldsExpr, loc0), TypeValue.CreateRecord fieldsTypes, Kind.Star
              }

          | ExprRec.RecordWith(record, fields) ->
            return!
              state {
                let! record, t_record, k_record = !record
                do! k_record |> Kind.AsStar |> ofSum |> state.Ignore

                let! t_record =
                  t_record
                  |> TypeValue.AsRecord
                  |> ofSum
                  |> state.Map WithTypeExprSourceMapping.Getters.Value

                let! fields =
                  fields
                  |> List.map (fun (k, v) ->
                    state {
                      let! v, t_v, v_k = !v
                      do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                      let! k_s = TypeCheckState.TryFindRecordFieldSymbol(k, loc0)

                      let! t_v_record = t_record |> OrderedMap.tryFindWithError k_s "fields" k.ToFSharpString |> ofSum
                      do! TypeValue.Unify(loc0, t_v, t_v_record) |> Expr.liftUnification

                      return (k, v), (k_s, t_v)
                    })
                  |> state.All

                let fieldsExpr = fields |> List.map fst

                let! t_record =
                  t_record
                  |> TypeValue.CreateRecord
                  |> TypeValue.Instantiate loc0
                  |> Expr.liftInstantiation

                return Expr.RecordWith(record, fieldsExpr, loc0), t_record, Kind.Star
              }
          // |> state.MapError(
          //   Errors.Map(
          //     String.appendNewline
          //       $"""...when typechecking `{{ {fields |> List.map (fun (id, _) -> id.ToFSharpString + "=...")} }}` = ...`"""
          //   )
          // )

          | ExprRec.TupleCons(fields) ->
            return!
              state {
                let! fields =
                  fields
                  |> List.map (fun (v) ->
                    state {
                      let! v, t_v, v_k = !v
                      do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                      return v, t_v
                    })
                  |> state.All

                let fieldsExpr = fields |> List.map fst
                let fieldsTypes = fields |> List.map snd

                return Expr.TupleCons(fieldsExpr, loc0), TypeValue.CreateTuple fieldsTypes, Kind.Star
              }
          // |> state.MapError(
          //   Errors.Map(
          //     String.appendNewline
          //       $"""...when typechecking `(( {fields |> List.map (fun f -> f.ToFSharpString + ", ")} ))` = ...`"""
          //         .ReasonablyClamped

          //   )
          // )

          | ExprRec.SumCons(cons) ->
            return!
              state {
                let cases =
                  [| 0 .. cons.Count - 1 |]
                  |> Array.map (fun i ->
                    let guid = Guid.CreateVersion7()

                    ({ TypeVar.Name = $"a_{i}_of_{cons.Count} " + guid.ToString()
                       Guid = guid }))

                for c in cases do
                  do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists c))

                let cases = cases |> Array.map TypeValue.Var

                let sum_t = TypeValue.CreateSum(cases |> List.ofSeq)

                return Expr.SumCons(cons, loc0), TypeValue.CreateArrow(cases[cons.Case - 1], sum_t), Kind.Star
              }
          // |> state.MapError(
          //   Errors.Map(
          //     String.appendNewline
          //       $"""...when typechecking `case{cons.Case}of{cons.Count}({value.ToFSharpString.ReasonablyClamped})` = ...`"""
          //   )
          // )

          | ExprRec.RecordDes(fields_expr, fieldName) ->
            let! fields, t_fields, fields_k = !fields_expr
            do! fields_k |> Kind.AsStar |> ofSum |> state.Ignore

            return!
              state.Either
                (state {
                  let! t_fields =
                    t_fields
                    |> TypeValue.AsRecord
                    |> ofSum
                    |> state.Map WithTypeExprSourceMapping.Getters.Value

                  return!
                    state {
                      let t_fields =
                        t_fields
                        |> OrderedMap.toSeq
                        |> Seq.map (fun (k, v) -> (k.Name, v))
                        |> OrderedMap.ofSeq

                      let! field_t =
                        t_fields
                        |> OrderedMap.tryFindWithError fieldName "fields" fieldName.ToFSharpString
                        |> ofSum

                      return Expr.RecordDes(fields, fieldName, loc0), field_t, Kind.Star
                    }
                    |> state.MapError(Errors.SetPriority ErrorPriority.High)
                })
                (state {
                  let! _record_t, field_t = TypeCheckState.TryFindRecordField(fieldName, loc0)

                  return!
                    state {
                      do!
                        TypeValue.Unify(loc0, t_fields, TypeValue.CreateRecord _record_t)
                        |> Expr.liftUnification

                      return Expr.RecordDes(fields, fieldName, loc0), field_t, Kind.Star
                    }
                    |> state.MapError(Errors.SetPriority ErrorPriority.High)
                })
              |> state.MapError Errors.FilterHighestPriorityOnly
          // |> state.MapError(
          //   Errors.Map(
          //     String.appendNewline
          //       $"""...when typechecking `({fields.ToFSharpString.ReasonablyClamped}).{fieldName.ToFSharpString}` = ...`"""
          //   )
          // )

          | ExprRec.TupleDes(fields, fieldName) ->
            return!
              state {
                let! fields, t_fields, fields_k = !fields
                do! fields_k |> Kind.AsStar |> ofSum |> state.Ignore

                let! t_fields =
                  t_fields
                  |> TypeValue.AsTuple
                  |> ofSum
                  |> state.Map WithTypeExprSourceMapping.Getters.Value

                let! t_field =
                  t_fields
                  |> List.tryItem (fieldName.Index - 1)
                  |> sum.OfOption($"Error: cannot find item {fieldName.Index} in tuple {fields}" |> error)
                  |> state.OfSum

                return Expr.TupleDes(fields, fieldName, loc0), t_field, Kind.Star
              }
          // |> state.MapError(
          //   Errors.Map(
          //     String.appendNewline
          //       $"""...when typechecking `({fields.ToFSharpString.ReasonablyClamped}).item{fieldName.Index}` = ...`"""
          //   )
          // )

          | ExprRec.UnionDes(handlers, fallback) ->
            return!
              state {
                let guid = Guid.CreateVersion7()

                let result_var =
                  { TypeVar.Name = $"res" + guid.ToString()
                    Guid = guid }

                do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists result_var))
                let result_t = result_var |> TypeValue.Var

                return!
                  (state {
                    let handler_case_resolvers =
                      match context_t with
                      | Some(TypeValue.Arrow({ value = TypeValue.Union { value = context_cases }, _ })) ->
                        let context_cases_by_id =
                          context_cases
                          |> OrderedMap.toSeq
                          |> Seq.map (fun (k, v) -> (k.Name, (k, v)))
                          |> OrderedMap.ofSeq

                        handlers
                        |> Map.toSeq
                        |> Seq.map (fun (id, case_handler) ->
                          state {
                            let! sym, cons =
                              context_cases_by_id
                              |> OrderedMap.tryFindWithError id "cases" id.ToFSharpString
                              |> ofSum

                            return (cons, context_cases), (id, sym, case_handler)
                          })
                        |> Seq.toList

                      | _ ->
                        handlers
                        |> Map.toSeq
                        |> Seq.map (fun (id, case_handler) ->
                          state {
                            let! cons = TypeCheckState.TryFindUnionCaseConstructor(id, loc0)
                            let! sym = TypeCheckState.TryFindUnionCaseSymbol(id, loc0)
                            return cons, (id, sym, case_handler)
                          })
                        |> Seq.toList

                    let handler_case_resolvers =
                      match handler_case_resolvers with
                      | x :: xs -> NonEmptyList.OfList(x, xs)
                      | [] ->
                        NonEmptyList.OfList(
                          state.Throw(error "No handlers provided")
                          |> state.MapError(Errors.SetPriority ErrorPriority.Medium),
                          []
                        )

                    let! handlers = handler_case_resolvers |> state.All

                    let! union_t =
                      handlers
                      |> Seq.map fst
                      |> Seq.tryHead
                      |> Sum.fromOption (fun () -> (loc0, "Error: no handlers provided") |> Errors.Singleton)
                      |> state.OfSum
                      // |> state.Map(snd >> TypeValue.CreateUnion)
                      |> state.Map snd

                    // let! handlers =
                    //   handlers
                    //   |> Map.toSeq
                    //   |> Seq.map (fun (k, value) ->
                    //     state {
                    //       let! k_s = TypeCheckState.TryFindUnionCaseSymbol(k, loc0)
                    //       return k, (value, k_s)
                    //     })
                    //   |> state.All
                    //   |> state.Map Map.ofSeq

                    let handlers =
                      handlers
                      |> Seq.map snd
                      |> Seq.map (fun (id, k_s, (var, body)) -> id, (id, k_s, (var, body)))
                      |> Map.ofSeq



                    return!
                      state {
                        let! handlers =
                          handlers
                          |> Map.map (fun _k (_id, k_s, (var, body)) ->
                            // (id, sym, case_handler)
                            state {
                              let! var_t =
                                union_t |> OrderedMap.tryFindWithError k_s "cases" k_s.ToFSharpString |> ofSum

                              let! body, body_t, body_k =
                                !body
                                |> state.MapContext(
                                  TypeCheckContext.Updaters.Values(
                                    Map.add (Identifier.LocalScope var.Name) (var_t, Kind.Star)
                                  )
                                )

                              do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

                              do! TypeValue.Unify(loc0, body_t, result_t) |> Expr.liftUnification

                              let! var_t = TypeValue.Instantiate loc0 var_t |> Expr.liftInstantiation

                              return (var, body), (k_s, var_t)
                            })
                          |> state.AllMap

                        let handlers = handlers |> Map.map (fun _ (vb, (kv, _)) -> vb, kv)

                        let handlerExprs = handlers |> Map.map (fun _ -> fst)

                        let! fallback = fallback |> Option.map (!) |> state.RunOption

                        let! fallback =
                          state {
                            match fallback with
                            | None -> return None
                            | Some(fallback, fallbackT, fallbackK) ->
                              do! fallbackK |> Kind.AsStar |> ofSum |> state.Ignore
                              do! TypeValue.Unify(loc0, fallbackT, result_t) |> Expr.liftUnification
                              return fallback |> Some
                          }

                        match fallback with
                        | Some _ -> ()
                        | None ->
                          let handlers = handlers |> Map.keys |> Set.ofSeq
                          let union_cases = union_t |> OrderedMap.keys |> Set.ofSeq

                          if handlers.Count <> union_cases.Count then
                            return! $"Error: incomplete pattern matching" |> error |> state.Throw

                        let! result_t = TypeValue.Instantiate loc0 result_t |> Expr.liftInstantiation

                        // do!
                        //     UnificationState.DeleteVariable result_var
                        //       |> TypeValue.EquivalenceClassesOp
                        //       |> Expr<'T>.liftUnification

                        let unionValue = TypeValue.CreateUnion union_t
                        let arrowValue = TypeValue.CreateArrow(unionValue, result_t)

                        let handlerExprs =
                          handlerExprs
                          |> Map.toSeq
                          |> Seq.map (fun (k, v) -> (k.LocalName |> Identifier.LocalScope), v)
                          |> Seq.fold (fun state (k, v) -> Map.add k v state) handlerExprs

                        return Expr.UnionDes(handlerExprs, fallback, loc0), arrowValue, Kind.Star
                      }
                      |> state.MapError(Errors.SetPriority ErrorPriority.High)
                  })

              }
          // |> state.MapError(
          //   Errors.Map(
          //     String.appendNewline
          //       $"""...when typechecking `match-case {{ {handlers
          //                                                |> Map.toSeq
          //                                                |> Seq.map (fun (id, (x, _)) -> "| " + id.ToFSharpString + " " + x.Name + " -> ...")} }}` = ...`"""
          //   )
          // )

          | ExprRec.SumDes(handlers) ->
            return!
              state {
                if
                  handlers
                  |> Map.keys
                  |> Seq.sortBy (fun k -> k.Case)
                  |> Seq.map (fun k -> k.Case)
                  |> Seq.toList
                  <> [ 1 .. handlers.Count ]
                then
                  return! $"Error: sum cases must be 1..{handlers.Count}" |> error |> state.Throw

                let! context_t =
                  context_t
                  |> Option.map (fun t ->
                    state {
                      let! ({ value = t_i, t_o }) = t |> TypeValue.AsArrow |> ofSum
                      let! t_i = t_i |> TypeValue.AsSum |> ofSum
                      return t_i.value, t_o
                    })
                  |> state.RunOption

                let result_var_t =
                  match context_t with
                  | Some(_t_i, t_o) -> t_o
                  | None ->
                    let guid = Guid.CreateVersion7()

                    let fresh_var =
                      { TypeVar.Name = "_sum_des_result_" + guid.ToString()
                        Guid = guid }

                    fresh_var |> TypeValue.Var

                let! handlers =
                  handlers
                  |> Map.map (fun _k (var, body) ->
                    state {
                      let! var_t =
                        state {
                          match context_t with
                          | None ->
                            let guid = Guid.CreateVersion7()

                            let fresh_var =
                              { TypeVar.Name = var.Name + $"_{_k.Case}Of{_k.Count}_" + guid.ToString()
                                Guid = guid }

                            do!
                              state.SetState(
                                TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists fresh_var)
                              )

                            return TypeValue.Var fresh_var
                          | Some(t_i, _t_o) ->
                            return!
                              t_i
                              |> List.tryItem (_k.Case - 1)
                              |> sum.OfOption("impossible" |> error)
                              |> state.OfSum
                        }

                      let! body, body_t, body_k =
                        !body
                        |> state.MapContext(
                          TypeCheckContext.Updaters.Values(Map.add (Identifier.LocalScope var.Name) (var_t, Kind.Star))
                        )

                      do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

                      do! TypeValue.Unify(loc0, body_t, result_var_t) |> Expr.liftUnification

                      let! var_t = TypeValue.Instantiate loc0 var_t |> Expr.liftInstantiation

                      return (var, body), var_t
                    })
                  |> state.AllMap

                let handlersSorted =
                  handlers
                  |> Map.map (fun _k -> snd)
                  |> Map.toSeq
                  |> Seq.sortBy (fun (k, _v) -> k.Case)
                  |> Seq.toList

                let handlerExprs = handlers |> Map.map (fun _k -> fst)

                let handlerTypes = handlersSorted |> List.map snd

                let! result_t = TypeValue.Instantiate loc0 result_var_t |> Expr.liftInstantiation

                let sumValue = TypeValue.CreateSum handlerTypes
                let arrowValue = TypeValue.CreateArrow(sumValue, result_t)
                let! arrowValue = TypeValue.Instantiate loc0 arrowValue |> Expr.liftInstantiation

                // for kv in handler_vars do
                //   do!
                //       UnificationState.DeleteVariable kv
                //         |> TypeValue.EquivalenceClassesOp
                //         |> Expr<'T>.liftUnification

                // do!
                //     UnificationState.DeleteVariable result_var
                //       |> TypeValue.EquivalenceClassesOp
                //       |> Expr<'T>.liftUnification

                return Expr.SumDes(handlerExprs, loc0), arrowValue, Kind.Star
              }
          // |> state.MapError(
          //   Errors.Map(
          //     String.appendNewline
          //       $"""...when typechecking `match-case {{ {handlers
          //                                                |> Seq.mapi (fun id ((x, _)) -> "| case" + id.ToString() + " " + x.Name + " -> ...")} }}` = ...`"""
          //   )
          // )

          | ExprRec.TypeLet(typeIdentifier, typeDefinition, rest) ->
            return!
              state {
                let! typeDefinition =
                  TypeExpr.Eval (Some(ExprTypeLetBindingName typeIdentifier)) loc0 typeDefinition
                  |> Expr.liftTypeEval
                  |> state.MapContext(
                    TypeCheckContext.Updaters.Types(
                      TypeExprEvalContext.Updaters.Scope(fun scope -> typeIdentifier :: scope)
                    )
                  )

                do! TypeExprEvalState.bindType typeIdentifier typeDefinition |> Expr.liftTypeEval

                let! scope = state.GetContext() |> state.Map(fun ctx -> ctx.Types.Scope)

                let bind_component (v, t, k) : Updater<Map<Identifier, (TypeValue * Kind)>> =
                  Map.add (Identifier.LocalScope v) (t, k)
                  >> Map.add (Identifier.FullyQualified(typeIdentifier :: scope, v)) (t, k)

                let! definition_cases =
                  typeDefinition
                  |> fst
                  |> TypeValue.AsUnion
                  |> ofSum
                  |> state.Catch
                  |> state.Map(Sum.toOption)
                  |> state.Map(Option.map WithTypeExprSourceMapping.Getters.Value)

                let! bind_definition_cases =
                  definition_cases
                  |> Option.map (fun definition_cases ->
                    definition_cases
                    |> OrderedMap.toSeq
                    |> Seq.map (fun (k, argT) ->
                      state {
                        do!
                          TypeExprEvalState.bindUnionCaseConstructor k.Name.LocalName (argT, definition_cases)
                          |> Expr.liftTypeEval

                        return
                          bind_component (
                            k.Name.LocalName,
                            TypeValue.CreateRecord(
                              OrderedMap.ofList
                                [ TypeSymbol.Create(Identifier.LocalScope "cons"),
                                  TypeValue.CreateArrow(argT, TypeValue.CreateUnion(definition_cases))
                                  TypeSymbol.Create(Identifier.LocalScope "map"),
                                  TypeValue.CreateArrow(
                                    TypeValue.CreateArrow(argT, argT),
                                    TypeValue.CreateArrow(
                                      TypeValue.CreateUnion(definition_cases),
                                      TypeValue.CreateUnion(definition_cases)
                                    )
                                  ) ]
                            ),
                            //
                            Kind.Star
                          )
                      })
                    |> state.All)
                  |> state.RunOption
                  |> state.MapContext(
                    TypeCheckContext.Updaters.Types(
                      TypeExprEvalContext.Updaters.Scope(fun scope -> typeIdentifier :: scope)
                    )
                  )
                  |> state.Map(Option.map (List.fold (>>) id) >> Option.defaultValue id)

                let! definition_fields =
                  typeDefinition
                  |> fst
                  |> TypeValue.AsRecord
                  |> ofSum
                  |> state.Catch
                  |> state.Map(Sum.toOption)
                  |> state.Map(Option.map WithTypeExprSourceMapping.Getters.Value)

                let! bind_definition_fields =
                  definition_fields
                  |> Option.map (fun definition_fields ->
                    definition_fields
                    |> OrderedMap.toSeq
                    |> Seq.map (fun (k, argT) ->
                      state {
                        do!
                          TypeExprEvalState.bindRecordField k.Name.LocalName (definition_fields, argT)
                          |> Expr.liftTypeEval

                        return
                          bind_component (
                            k.Name.LocalName,
                            TypeValue.CreateRecord(
                              OrderedMap.ofList
                                [ TypeSymbol.Create(Identifier.LocalScope "get"),
                                  TypeValue.CreateArrow(TypeValue.CreateRecord(definition_fields), argT)
                                  TypeSymbol.Create(Identifier.LocalScope "map"),
                                  TypeValue.CreateArrow(
                                    TypeValue.CreateArrow(argT, argT),
                                    TypeValue.CreateArrow(
                                      TypeValue.CreateRecord(definition_fields),
                                      TypeValue.CreateRecord(definition_fields)
                                    )
                                  ) ]
                            ),
                            Kind.Star
                          )
                      })
                    |> state.All)
                  |> state.RunOption
                  |> state.MapContext(
                    TypeCheckContext.Updaters.Types(
                      TypeExprEvalContext.Updaters.Scope(fun scope -> typeIdentifier :: scope)
                    )
                  )
                  |> state.Map(Option.map (List.fold (>>) id) >> Option.defaultValue id)

                let! rest, rest_t, rest_k =
                  !rest
                  |> state.MapContext(TypeCheckContext.Updaters.Values(bind_definition_cases >> bind_definition_fields))

                return Expr.TypeLet(typeIdentifier, typeDefinition |> fst, rest, loc0), rest_t, rest_k
              }
          // |> state.MapError(
          //   Errors.Map(
          //     String.appendNewline
          //       $"""...when typechecking `type {typeIdentifier} = {typeDefinition.ToFSharpString.ReasonablyClamped} ...`"""
          //   )
          // )

          | ExprRec.TypeLambda(t_par, body) ->
            return!
              state {
                let fresh_t_par_var =
                  let id = Guid.CreateVersion7()

                  { TypeVar.Name = t_par.Name; Guid = id }

                do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists fresh_t_par_var))

                let! t_par_type =
                  TypeExprEvalState.tryFindType (Identifier.LocalScope t_par.Name, loc0)
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
                  UnificationState.TryDeleteFreeVariable(fresh_t_par_var, loc0)
                  |> TypeValue.EquivalenceClassesOp loc0
                  |> Expr.liftUnification

                return
                  Expr.TypeLambda(t_par, body, loc0),
                  TypeValue.CreateLambda(t_par, t_body.AsExpr),
                  Kind.Arrow(t_par.Kind, body_k)
              }
          // |> state.MapError(
          //   Errors.Map(
          //     String.appendNewline
          //       $"""...when typechecking `fun {t_par.Name} => {body.ToFSharpString.ReasonablyClamped} ...`"""
          //   )
          // )

          | ExprRec.TypeApply(fExpr, tExpr) ->
            return!
              state {
                let! f, f_t, f_k = !fExpr

                let! f_k_i, f_k_o = f_k |> Kind.AsArrow |> ofSum
                let! t_val, t_k = tExpr |> TypeExpr.Eval None loc0 |> Expr.liftTypeEval

                if f_k_i <> t_k then
                  return!
                    $"Error: mismatched kind, expected {f_k_i} but got {t_k}"
                    |> error
                    |> state.Throw
                else
                  let! f_res, _ =
                    TypeExpr.Apply(f_t.AsExpr, tExpr)
                    |> TypeExpr.Eval None loc0
                    |> Expr.liftTypeEval

                  return Expr.TypeApply(f, t_val, loc0), f_res, f_k_o
              }
        // |> state.MapError(
        //   Errors.Map(
        //     String.appendNewline
        //       $"""...when typechecking `{fExpr.ToFSharpString.ReasonablyClamped}[{t.ToFSharpString.ReasonablyClamped}] ...`"""
        //   )
        // )
        }
