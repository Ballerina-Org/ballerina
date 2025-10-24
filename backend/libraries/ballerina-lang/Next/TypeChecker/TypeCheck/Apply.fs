namespace Ballerina.DSL.Next.Types.TypeChecker

module Apply =
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
  open Ballerina.DSL.Next.Types.TypeChecker.Primitive
  open Ballerina.DSL.Next.Types.TypeChecker.Lookup
  open Ballerina.DSL.Next.Types.TypeChecker.Lambda
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal TypeCheckApply
      (typeCheckExpr: TypeChecker, loc0: Location)
      : TypeChecker<ExprApply<TypeExpr, Identifier>> =
      fun context_t ({ F = f_expr; Arg = a_expr }) ->
        let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          let f = f_expr
          let! a, t_a, a_k = !a_expr
          do! a_k |> Kind.AsStar |> ofSum |> state.Ignore
          let! ctx = state.GetContext()
          let error e = Errors.Singleton(loc0, e)

          let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
            p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

          return!
            state.Either
              (state {
                let! { Id = f_lookup } = f |> Expr.AsLookup |> ofSum
                let f_lookup = ctx.Types.Scope.Resolve f_lookup

                return!
                  state.Either
                    (state {
                      let! union_cons_t =
                        state.Either
                          (state {
                            match context_t with
                            | None ->
                              return! (loc0, "Context is not set, skipping branch") |> Errors.Singleton |> state.Throw
                            | Some context_t ->
                              let! context_cases = context_t |> TypeValue.AsUnion |> ofSum

                              let context_cases_by_id =
                                context_cases.value
                                |> OrderedMap.toSeq
                                |> Seq.map (fun (k, v) -> (k.Name |> ctx.Types.Scope.Resolve, (k, v)))
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
                      do! TypeValue.Unify(loc0, f_i, t_a) |> Expr<'T, 'Id>.liftUnification
                      let! f_o = f_o |> TypeValue.Instantiate loc0 |> Expr.liftInstantiation

                      return Expr.Apply(Expr.Lookup f_lookup, a, loc0, ctx.Types.Scope), f_o, f_k
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

                      if adHocPolymorphismBinaryAllOperatorNames.Contains f_lookup.Name then
                        return!
                          state {
                            let! a_primitive = t_a |> TypeValue.AsPrimitive |> ofSum
                            let a_primitive = a_primitive.value

                            let! adHocResolution =
                              adHocPolymorphismBinary
                              |> Map.tryFindWithError
                                (f_lookup.Name, a_primitive)
                                "ad-hoc polymorphism resolutions"
                                f_lookup.ToFSharpString
                                loc0
                              |> state.OfSum

                            let! adhoc_op, adhoc_op_t, adhoc_op_k =
                              !Expr.Lookup(Identifier.FullyQualified([ adHocResolution.Namespace ], f_lookup.Name),
                                           loc0,
                                           ctx.Types.Scope)

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
                              |> Expr<'T, 'Id>.liftUnification

                            let! t_res =
                              TypeValue.CreateArrow(
                                TypeValue.CreatePrimitive adHocResolution.OtherInput,
                                TypeValue.CreatePrimitive adHocResolution.Output
                              )
                              |> TypeValue.Instantiate loc0
                              |> Expr.liftInstantiation

                            let k_res = Kind.Star
                            return Expr.Apply(adhoc_op, a, loc0, ctx.Types.Scope), t_res, k_res
                          }
                          |> state.MapError(Errors.SetPriority ErrorPriority.Medium)
                      elif f_lookup.Name = "!" then
                        return!
                          state {
                            do!
                              TypeValue.Unify(loc0, TypeValue.CreatePrimitive PrimitiveType.Bool, t_a)
                              |> Expr<'T, 'Id>.liftUnification

                            return!
                              state {
                                let! bool_op, bool_op_t, bool_op_k =
                                  !Expr.Lookup(Identifier.FullyQualified([ "bool" ], f_lookup.Name),
                                               loc0,
                                               ctx.Types.Scope)

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
                                  |> Expr<'T, 'Id>.liftUnification

                                let t_res = TypeValue.CreatePrimitive PrimitiveType.Bool
                                let k_res = Kind.Star
                                return Expr.Apply(bool_op, a, loc0, ctx.Types.Scope), t_res, k_res
                              }
                              |> state.MapError(Errors.SetPriority ErrorPriority.High)

                          }
                      else
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

                let rec pad (f_expr: Expr<TypeExpr, Identifier>) =
                  state {
                    let! f, t_f, f_k = f_constraint => f_expr

                    match f_k with
                    | Kind.Arrow(Kind.Star, _) ->
                      let guid = Guid.CreateVersion7()

                      let freshVar =
                        { TypeVar.Name = "fresh_var_application_" + guid.ToString()
                          Guid = guid }

                      do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists freshVar))

                      do!
                        TypeExprEvalState.bindType
                          (freshVar.Name |> Identifier.LocalScope |> ctx.Types.Scope.Resolve)
                          (freshVar |> TypeValue.Var, Kind.Star)
                        |> Expr.liftTypeEval

                      return! pad (Expr.TypeApply(f_expr, freshVar.Name |> Identifier.LocalScope |> TypeExpr.Lookup))
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
                            |> OrderedMap.map (fun _ -> TypeExpr.Eval None loc0 >> Expr<'T, 'Id>.liftTypeEval)
                            |> state.AllMapOrdered

                          let aCasesT = aCasesT |> OrderedMap.map (fun _ -> fst)

                          do!
                            TypeValue.Unify(loc0, f_input, TypeValue.CreateUnion aCasesT)
                            |> Expr<'T, 'Id>.liftUnification

                          let! f_output = f_output |> TypeValue.Instantiate loc0 |> Expr<'T, 'Id>.liftInstantiation

                          return Expr.Apply(f, a, loc0, ctx.Types.Scope), f_output, Kind.Star
                        }
                        |> state.MapError(Errors.SetPriority ErrorPriority.High)
                    },
                    [ state {
                        do! TypeValue.Unify(loc0, f_input, t_a) |> Expr<'T, 'Id>.liftUnification
                        let! f_output = f_output |> TypeValue.Instantiate loc0 |> Expr<'T, 'Id>.liftInstantiation

                        return Expr.Apply(f, a, loc0, ctx.Types.Scope), f_output, Kind.Star
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
