namespace Ballerina.DSL.Next.Types.TypeChecker

module UnionDes =
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
  open Ballerina.DSL.Next.Types.TypeChecker.Apply
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal TypeCheckUnionDes
      (typeCheckExpr: TypeChecker, loc0: Location)
      : TypeChecker<ExprUnionDes<TypeExpr, Identifier>> =
      fun
          context_t
          ({ Handlers = handlers
             Fallback = fallback }) ->
        let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        let error e = Errors.Singleton(loc0, e)

        state {
          let! ctx = state.GetContext()
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

                      let! id =
                        TypeExprEvalState.tryFindResolvedIdentifier (sym, loc0)
                        |> state.OfStateReader
                        |> Expr.liftTypeEval

                      return (cons, context_cases), (id, sym, case_handler)
                    })
                  |> Seq.toList

                | _ ->
                  handlers
                  |> Map.toSeq
                  |> Seq.map (fun (id, case_handler) ->
                    state {
                      let id = id |> TypeCheckScope.Empty.Resolve
                      let! cons = TypeCheckState.TryFindUnionCaseConstructor(id, loc0)
                      let! sym = TypeCheckState.TryFindUnionCaseSymbol(id, loc0)

                      let! id =
                        TypeExprEvalState.tryFindResolvedIdentifier (sym, loc0)
                        |> state.OfStateReader
                        |> Expr.liftTypeEval

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
              //       let! k_s = TypeCheckState.TryFindUnionCaseSymbol(k, loc0, ctx.Types.Scope)
              //       return k, (value, k_s)
              //     })
              //   |> state.All
              //   |> state.Map Map.ofSeq

              let handlers =
                handlers
                |> Seq.map (fun ((cons_t, _), (id, k_s, (var, body))) -> id, (cons_t, id, k_s, (var, body)))
                |> Map.ofSeq

              return!
                state {
                  let! handlers =
                    handlers
                    |> Map.map (fun _k (cons_t, _id, k_s, (var, body)) ->
                      // (id, sym, case_handler)
                      state {
                        let! var_t = union_t |> OrderedMap.tryFindWithError k_s "cases" k_s.ToFSharpString |> ofSum

                        let! body, body_t, body_k =
                          (Some cons_t) => body
                          |> state.MapContext(
                            TypeCheckContext.Updaters.Values(
                              Map.add (var.Name |> Identifier.LocalScope |> ctx.Types.Scope.Resolve) (var_t, Kind.Star)
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
                  //       |> Expr<'T, 'Id>.liftUnification

                  let unionValue = TypeValue.CreateUnion union_t

                  let! arrowValue =
                    TypeValue.CreateArrow(unionValue, result_t)
                    |> TypeValue.Instantiate loc0
                    |> Expr.liftInstantiation

                  let handlerExprs =
                    handlerExprs
                    |> Map.toSeq
                    |> Seq.map (fun (k, v) -> k, v)
                    |> Seq.fold (fun state (k, v) -> Map.add k v state) handlerExprs

                  return Expr.UnionDes(handlerExprs, fallback, loc0, ctx.Types.Scope), arrowValue, Kind.Star
                }
                |> state.MapError(Errors.SetPriority ErrorPriority.High)
            })

        }
