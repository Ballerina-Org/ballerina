namespace Ballerina.DSL.Next.Types.TypeChecker

module Lookup =
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

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckLookup<'valueExt when 'valueExt: comparison>
      (_typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<
          ExprLookup<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          'valueExt
         >
      =
      fun _context_t ({ Id = id }) ->
        state {
          let! ctx = state.GetContext()
          let! st = state.GetState()

          let id_original = TypeCheckScope.Empty.Resolve id
          let id_resolved = ctx.Scope.Resolve id

          let error e = Errors.Singleton loc0 e

          match id with
          | Identifier.FullyQualified(prefixParts, _name) ->
            let prefix =
              match prefixParts with
              | [ p ] -> p
              | ps -> String.Join("::", ps)

            let availableFromValues =
              ctx.Values
              |> Map.toSeq
              |> Seq.choose (fun (rid, (tv, _)) ->
                match rid.Type with
                | Some t when t = prefix ->
                  Some(rid.Name, tv.ToInlayString())
                | _ -> None)

            let availableFromBindings =
              st.Bindings
              |> Map.toSeq
              |> Seq.choose (fun (rid, (tv, _)) ->
                match rid.Type with
                | Some t when t = prefix ->
                  Some(rid.Name, tv.ToInlayString())
                | _ -> None)

            let availableSymbols =
              Seq.append availableFromValues availableFromBindings
              |> Map.ofSeq

            if not (Map.isEmpty availableSymbols) then
              do!
                TypeCheckState.bindScopeAccessHint(
                  loc0,
                  prefix,
                  availableSymbols
                )
          | _ -> ()

          return!
            state.Either3
              (state {
                let! t_id, id_k =
                  state.Either
                    (TypeCheckContext.TryFindVar(id_resolved, loc0))
                    (TypeCheckState.TryFindType(id_resolved, loc0))

                return
                  TypeCheckedExpr.Lookup(
                    id_resolved,
                    t_id,
                    id_k,
                    loc0,
                    ctx.Scope
                  ),
                  ctx
              })
              (state {
                let! t_id, id_k =
                  state.Either
                    (TypeCheckContext.TryFindVar(id_original, loc0))
                    (TypeCheckState.TryFindType(id_original, loc0))

                return
                  TypeCheckedExpr.Lookup(
                    id_original,
                    t_id,
                    id_k,
                    loc0,
                    ctx.Scope
                  ),
                  ctx

              })
              (state.Throw(
                (fun () -> $"Error: cannot resolve identifier '{id_resolved}'/'{id_original}'.")
               |> error
               |> Errors<_>.MapPriority(replaceWith ErrorPriority.High)
              ))
            |> state.MapError(Errors<_>.FilterHighestPriorityOnly)

        }

    static member internal TypeCheckErrorDanglingScopedIdentifier<'valueExt
      when 'valueExt: comparison>
      (prefixParts: List<string>)
      (loc0: Location)
      : TypeCheckerResult<
          TypeCheckedExpr<'valueExt> * TypeCheckContext<'valueExt>,
          'valueExt
         >
      =
        state {
          let! ctx = state.GetContext()
          let! st = state.GetState()

          let prefix =
            match prefixParts with
            | [ p ] -> p
            | ps -> String.Join("::", ps)

          let availableFromValues =
            ctx.Values
            |> Map.toSeq
            |> Seq.choose (fun (rid, (tv, _)) ->
              match rid.Type with
              | Some t when t = prefix ->
                Some(rid.Name, tv.ToInlayString())
              | _ -> None)

          let availableFromBindings =
            st.Bindings
            |> Map.toSeq
            |> Seq.choose (fun (rid, (tv, _)) ->
              match rid.Type with
              | Some t when t = prefix ->
                Some(rid.Name, tv.ToInlayString())
              | _ -> None)

          let availableSymbols =
            Seq.append availableFromValues availableFromBindings
            |> Map.ofSeq

          if not (Map.isEmpty availableSymbols) then
            do!
              TypeCheckState.bindScopeAccessHint(
                loc0,
                prefix,
                availableSymbols
              )

          return
            { TypeCheckedExpr.Expr =
                TypeCheckedExprRec.ErrorDanglingScopedIdentifier(
                  { TypeCheckedExprErrorDanglingScopedIdentifier.PrefixParts = prefixParts }
                )
              Location = loc0
              Type = TypeValue.CreateUnit()
              Kind = Kind.Star
              Scope = ctx.Scope },
            ctx
        }
