namespace Ballerina.DSL.Next.Types.TypeChecker

module Apply =
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
  open Ballerina.DSL.Next.Types.TypeChecker.Primitive
  open Ballerina.DSL.Next.Types.TypeChecker.Lookup
  open Ballerina.DSL.Next.Types.TypeChecker.Lambda
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckApply<'valueExt when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprApply<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun context_t ({ F = f_expr; Arg = a_expr }) ->
        let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        state {
          // do Console.WriteLine($"Typechecking application expression at {loc0}...")
          // do Console.WriteLine($"Function expression: {f_expr}")
          // do Console.WriteLine($"Argument expression: {a_expr}")
          let f = f_expr
          let! a, t_a, a_k, _ = None => a_expr
          do! a_k |> Kind.AsStar |> ofSum |> state.Ignore
          let! ctx = state.GetContext()
          let error e = Errors.Singleton loc0 e
          // do Console.WriteLine($"t_a: {t_a}")
          // do Console.WriteLine($"a_k: {a_k}")
          // do Console.ReadLine() |> ignore

          let ofSum (p: Sum<'a, Errors<Unit>>) =
            p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

          return!
            state.Either
              (state {
                let! { Id = f_lookup } = f |> Expr.AsLookup |> ofSum
                let f_lookup = ctx.Scope.Resolve f_lookup

                return!
                  state.Either
                    (state {
                      let! (union_cons_t:
                        TypeValue<'valueExt> * TypeParameter list * OrderedMap<TypeSymbol, TypeValue<'valueExt>>) =
                        state {
                          match context_t with
                          | None ->
                            return!
                              (fun () -> "Context is not set, skipping branch")
                              |> Errors.Singleton loc0
                              |> state.Throw
                          | Some context_t ->
                            let! context_cases = context_t |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                            let context_cases_by_id =
                              context_cases
                              |> OrderedMap.toSeq
                              |> Seq.map (fun (k, v) -> (k.Name |> ctx.Scope.Resolve, (k, v)))
                              |> OrderedMap.ofSeq

                            let! _case_k, case_t =
                              context_cases_by_id
                              |> OrderedMap.tryFindWithError f_lookup "cases" f_lookup.AsFSharpString
                              |> ofSum

                            return case_t, [], context_cases
                        }

                      return!
                        state {
                          let f_i, union_type_parameters, union_cases = union_cons_t
                          let f_o = union_cases |> TypeValue.CreateUnion
                          let f_k = Kind.Star

                          let! union_type_parameters_fres_vars =
                            union_type_parameters
                            |> Seq.map (fun tp ->
                              state {
                                let guid = Guid.CreateVersion7()

                                let freshVar =
                                  { TypeVar.Name = "fresh_var_application_" + guid.ToString()
                                    Synthetic = true
                                    Guid = guid }

                                do!
                                  state.SetState(
                                    TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists freshVar)
                                  )

                                do!
                                  TypeCheckState.bindType
                                    (freshVar.Name |> Identifier.LocalScope |> ctx.Scope.Resolve)
                                    (freshVar |> TypeValue.Var, tp.Kind)
                                  |> Expr.liftTypeEval

                                return freshVar
                              })
                            |> state.All

                          let f_o =
                            union_type_parameters
                            |> List.fold (fun acc tp -> TypeExpr.Lambda(tp, acc)) (f_o |> TypeExpr.FromTypeValue)

                          let f_o =
                            union_type_parameters_fres_vars
                            |> List.fold
                              (fun acc tp -> TypeExpr.Apply(acc, tp.Name |> Identifier.LocalScope |> TypeExpr.Lookup))
                              f_o

                          let f_i =
                            union_type_parameters_fres_vars
                            |> List.fold
                              (fun acc tp -> TypeExpr.Apply(acc, tp.Name |> Identifier.LocalScope |> TypeExpr.Lookup))
                              (f_i |> TypeExpr.FromTypeValue)

                          let! f_i, _ = f_i |> TypeExpr.Eval () typeCheckExpr None loc0 |> Expr.liftTypeEval
                          do! TypeValue.Unify(loc0, f_i, t_a) |> Expr<'T, 'Id, 'valueExt>.liftUnification
                          let! f_o, _ = f_o |> TypeExpr.Eval () typeCheckExpr None loc0 |> Expr.liftTypeEval

                          let! f_o =
                            f_o
                            |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
                            |> Expr.liftInstantiation

                          return Expr.Apply(Expr.Lookup f_lookup, a, loc0, ctx.Scope), f_o, f_k, ctx
                        }
                        |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
                    })
                    (state {
                      let! resolved = TypeCheckContext.TryFindVar(f_lookup, loc0) |> state.Catch
                      // ensure we do not apply ad-hoc polymorphism to bound variables
                      do!
                        resolved
                        |> Sum.AsRight
                        |> Sum.fromOption (fun () -> (fun () -> $"Error: variable found, skipping branch") |> error)
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
                                (fun () -> f_lookup.AsFSharpString)
                                loc0
                              |> state.OfSum

                            let! adhoc_op, adhoc_op_t, adhoc_op_k, _ =
                              !Expr.Lookup(Identifier.FullyQualified([ adHocResolution.Namespace ], f_lookup.Name),
                                           loc0,
                                           ctx.Scope)

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
                              |> Expr<'T, 'Id, 'valueExt>.liftUnification

                            let! t_res =
                              TypeValue.CreateArrow(
                                TypeValue.CreatePrimitive adHocResolution.OtherInput,
                                TypeValue.CreatePrimitive adHocResolution.Output
                              )
                              |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
                              |> Expr.liftInstantiation

                            let k_res = Kind.Star
                            return Expr.Apply(adhoc_op, a, loc0, ctx.Scope), t_res, k_res, ctx
                          }
                          |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.Medium))
                      elif f_lookup.Name = "!" then
                        return!
                          state {
                            do!
                              TypeValue.Unify(loc0, TypeValue.CreatePrimitive PrimitiveType.Bool, t_a)
                              |> Expr<'T, 'Id, 'valueExt>.liftUnification

                            return!
                              state {
                                let! bool_op, bool_op_t, bool_op_k, _ =
                                  !Expr.Lookup(Identifier.FullyQualified([ "bool" ], f_lookup.Name), loc0, ctx.Scope)

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
                                  |> Expr<'T, 'Id, 'valueExt>.liftUnification

                                let t_res = TypeValue.CreatePrimitive PrimitiveType.Bool
                                let k_res = Kind.Star
                                return Expr.Apply(bool_op, a, loc0, ctx.Scope), t_res, k_res, ctx
                              }
                              |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))

                          }
                      else
                        return!
                          (fun () -> $"Error: cannot resolve with ad-hoc polymorphism, found variable {f_lookup}")
                          |> error
                          |> state.Throw
                          |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.Low))
                    })
                  |> state.MapError(Errors<_>.FilterHighestPriorityOnly)
              })
              (state {
                let! t_a_has_structure =
                  state {
                    match t_a with
                    | TypeValue.Var _ -> return false
                    | _ -> return true
                  }

                // do Console.WriteLine($"t_a_has_structure: {t_a_has_structure}")
                // do Console.WriteLine($"t_a: {t_a}")
                // do Console.WriteLine($"f_expr: {f_expr}")
                // do Console.WriteLine($"a_expr: {a_expr}")
                // do Console.ReadLine() |> ignore

                let f_constraint =
                  if t_a_has_structure |> not then
                    None
                  else
                    let guid = Guid.CreateVersion7()

                    let freshVar =
                      { TypeVar.Name = "_application_" + guid.ToString()
                        Synthetic = true
                        Guid = guid }
                      |> TypeValue.Var

                    Some(TypeValue.CreateArrow(t_a, freshVar))

                let rec pad (f_expr: Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>) =
                  state {
                    // do Console.WriteLine($"typechecking function expression {f_expr}...")
                    // do Console.WriteLine($"with constraint {f_constraint}...")
                    // do Console.ReadLine() |> ignore
                    let! f, t_f, f_k, _ = f_constraint => f_expr
                    // do Console.WriteLine($"t_f: {t_f}")
                    // do Console.WriteLine($"f_k: {f_k}")
                    // do Console.ReadLine() |> ignore
                    match f_k with
                    | Kind.Arrow(Kind.Star, _) ->
                      let guid = Guid.CreateVersion7()

                      let freshVar =
                        { TypeVar.Name = "fresh_var_application_" + guid.ToString()
                          Synthetic = true
                          Guid = guid }

                      do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists freshVar))

                      do!
                        TypeCheckState.bindType
                          (freshVar.Name |> Identifier.LocalScope |> ctx.Scope.Resolve)
                          (freshVar |> TypeValue.Var, Kind.Star)
                        |> Expr.liftTypeEval

                      return! pad (Expr.TypeApply(f_expr, freshVar.Name |> Identifier.LocalScope |> TypeExpr.Lookup))
                    | _ -> return f, t_f, f_k
                  }

                // do Console.WriteLine($"Padding and typechecking function expression {f}...")
                let! f, t_f, f_k = pad f
                // do Console.WriteLine($"t_f: {t_f}")
                // do Console.WriteLine($"f_k: {f_k}")
                // do Console.ReadLine() |> ignore
                do! f_k |> Kind.AsStar |> ofSum |> state.Ignore

                let! (f_input, f_output) =
                  TypeValue.AsArrow t_f
                  |> ofSum
                  |> state.Map WithSourceMapping.Getters.Value
                  |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.Medium))

                return!
                  state.Any(
                    state {
                      let! aCasesT = t_a |> TypeValue.AsImportedUnionLike |> ofSum

                      return!
                        state {
                          let! aCasesT =
                            aCasesT
                            |> OrderedMap.map (fun _ ->
                              TypeExpr.Eval () typeCheckExpr None loc0
                              >> Expr<'T, 'Id, 'valueExt>.liftTypeEval)
                            |> state.AllMapOrdered

                          let aCasesT = aCasesT |> OrderedMap.map (fun _ -> fst)

                          do!
                            TypeValue.Unify(loc0, f_input, TypeValue.CreateUnion aCasesT)
                            |> Expr<'T, 'Id, 'valueExt>.liftUnification

                          let! f_output =
                            f_output
                            |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
                            |> Expr<'T, 'Id, 'valueExt>.liftInstantiation

                          return Expr.Apply(f, a, loc0, ctx.Scope), f_output, Kind.Star, ctx
                        }
                        |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
                    },
                    [ state {
                        do! TypeValue.Unify(loc0, f_input, t_a) |> Expr<'T, 'Id, 'valueExt>.liftUnification

                        let! f_output =
                          f_output
                          |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
                          |> Expr<'T, 'Id, 'valueExt>.liftInstantiation

                        return Expr.Apply(f, a, loc0, ctx.Scope), f_output, Kind.Star, ctx
                      }
                      |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
                      // $"Error: cannot resolve application"
                      // |> error
                      // |> state.Throw
                      // |> state.MapError(Errors.MapPriority(replaceWith  ErrorPriority.Medium))
                      ]
                  )
                  |> state.MapError(Errors<_>.FilterHighestPriorityOnly)
              })
            |> state.MapError(Errors<_>.FilterHighestPriorityOnly)


        }
        |> state.MapError(Errors<_>.FilterHighestPriorityOnly)
