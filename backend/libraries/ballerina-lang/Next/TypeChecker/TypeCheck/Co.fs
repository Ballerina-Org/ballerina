namespace Ballerina.DSL.Next.Types.TypeChecker

module Co =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.Fun

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckCo<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      (context_t: Option<TypeValue<'valueExt>>)
      (co: ExprCo<TypeExpr<'valueExt>, Identifier, 'valueExt>)
      : TypeCheckerResult<
          TypeCheckedExprCo<'valueExt> *
          TypeValue<'valueExt> *
          Kind *
          TypeCheckContext<'valueExt>,
          'valueExt
         >
      =
      let { CoTypeSymbol = co_type_symbol
            MkCoType = mk_co_type } =
        config

      let (=>) c e = typeCheckExpr c e
      let loc0 = co.Location

      let _ofSum (p: Sum<'a, Errors<Unit>>) =
        p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

      let mkFreshVar (name: string) =
        let guid = Guid.CreateVersion7()

        { TypeVar.Name = name + "_co_" + guid.ToString()
          Synthetic = true
          Guid = guid }

      let ensureVar (v: TypeVar) =
        state.SetState(
          TypeCheckState.Updaters.Vars(
            UnificationState.EnsureVariableExists v
          )
        )

      // Extract schema/ctx/st/res from an expected Co type, or create fresh vars
      let decomposeExpectedCoType () =
        state {
          match context_t with
          | Some(TypeValue.Imported { Sym = sym
                                      Arguments = [ schema; ctx; st; res ] }) when
            sym = co_type_symbol
            ->
            return schema, ctx, st, Some res
          | _ ->
            let schema_var = mkFreshVar "schema"
            let ctx_var = mkFreshVar "ctx"
            let st_var = mkFreshVar "st"
            do! ensureVar schema_var
            do! ensureVar ctx_var
            do! ensureVar st_var
            return
              TypeValue.Var schema_var,
              TypeValue.Var ctx_var,
              TypeValue.Var st_var,
              None
        }

      // Decompose a type-checked expression's type as Co[schema][ctx][st][a],
      // returning a (for the bound variable type) plus the co parameters.
      let decomposeCoType
        (step_loc: Location)
        (t: TypeValue<'valueExt>)
        : TypeCheckerResult<
            TypeValue<'valueExt> * TypeValue<'valueExt> * TypeValue<'valueExt> * TypeValue<'valueExt>,
            'valueExt
           >
        =
        state {
          match t with
          | TypeValue.Imported { Sym = sym
                                 Arguments = [ schema; ctx; st; a ] } when
            sym = co_type_symbol
            ->
            return schema, ctx, st, a
          | _ ->
            return!
              Errors.Singleton step_loc (fun () ->
                $"Error: expected a Co type but got {t}")
              |> state.Throw
        }

      let rec typeCheckCoStep
        (schema: TypeValue<'valueExt>)
        (ctx: TypeValue<'valueExt>)
        (st: TypeValue<'valueExt>)
        (expected_res: Option<TypeValue<'valueExt>>)
        (step: ExprCoStep<TypeExpr<'valueExt>, Identifier, 'valueExt>)
        : TypeCheckerResult<
            TypeCheckedCoStep<'valueExt> *
            TypeValue<'valueExt> *
            TypeCheckContext<'valueExt>,
            'valueExt
           >
        =
        let step_loc = step.Location

        state {
          match step.Step with
          | ExprCoStepRec.CoLetBang(x, e, rest) ->
            // typecheck e, expecting Co[schema][ctx][st][_]
            let expected_e_t =
              let a_var = mkFreshVar "a"
              mk_co_type schema ctx st (TypeValue.Var a_var) |> Some

            let! e_checked, _ = expected_e_t => e

            let! e_schema, e_ctx, e_st, a =
              decomposeCoType e.Location e_checked.Type

            // Unify the co parameters
            do!
              TypeValue.Unify(step_loc, schema, e_schema)
              |> Expr<'T, 'Id, 'valueExt>.liftUnification

            do!
              TypeValue.Unify(step_loc, ctx, e_ctx)
              |> Expr<'T, 'Id, 'valueExt>.liftUnification

            do!
              TypeValue.Unify(step_loc, st, e_st)
              |> Expr<'T, 'Id, 'valueExt>.liftUnification

            // Bind x : a in context, then typecheck rest
            let! tc_ctx = state.GetContext()

            let! rest_checked, rest_result_t, ctx_rest =
              typeCheckCoStep schema ctx st expected_res rest
              |> state.MapContext(
                TypeCheckContext.Updaters.Values(
                  Map.add
                    (x.Name |> Identifier.LocalScope |> tc_ctx.Scope.Resolve)
                    (a, Kind.Star)
                )
              )

            return
              { TypeCheckedCoStep.Location = step_loc
                Step =
                  TypeCheckedCoStepRec.CoLetBang(
                    x,
                    e_checked,
                    rest_checked
                  ) },
              rest_result_t,
              ctx_rest

          | ExprCoStepRec.CoDoBang(e, rest) ->
            // typecheck e, expecting Co[schema][ctx][st][Unit]
            let expected_e_t =
              mk_co_type schema ctx st (TypeValue.CreateUnit()) |> Some

            let! e_checked, _ = expected_e_t => e

            let! e_schema, e_ctx, e_st, _a =
              decomposeCoType e.Location e_checked.Type

            // Unify the co parameters
            do!
              TypeValue.Unify(step_loc, schema, e_schema)
              |> Expr<'T, 'Id, 'valueExt>.liftUnification

            do!
              TypeValue.Unify(step_loc, ctx, e_ctx)
              |> Expr<'T, 'Id, 'valueExt>.liftUnification

            do!
              TypeValue.Unify(step_loc, st, e_st)
              |> Expr<'T, 'Id, 'valueExt>.liftUnification

            // Typecheck rest
            let! rest_checked, rest_result_t, ctx_rest =
              typeCheckCoStep schema ctx st expected_res rest

            return
              { TypeCheckedCoStep.Location = step_loc
                Step =
                  TypeCheckedCoStepRec.CoDoBang(
                    e_checked,
                    rest_checked
                  ) },
              rest_result_t,
              ctx_rest

          | ExprCoStepRec.CoReturn e ->
            // typecheck e to get result type r
            let! e_checked, ctx_e = expected_res => e
            let r = e_checked.Type

            // Unify with expected result if available
            match expected_res with
            | Some expected_r ->
              do!
                TypeValue.Unify(step_loc, r, expected_r)
                |> Expr<'T, 'Id, 'valueExt>.liftUnification
            | None -> ()

            let result_t = mk_co_type schema ctx st r

            return
              { TypeCheckedCoStep.Location = step_loc
                Step = TypeCheckedCoStepRec.CoReturn(e_checked) },
              result_t,
              ctx_e

          | ExprCoStepRec.CoReturnBang e ->
            // typecheck e, expecting Co[schema][ctx][st][res]
            let expected_e_t =
              match expected_res with
              | Some res -> mk_co_type schema ctx st res |> Some
              | None -> None

            let! e_checked, ctx_e = expected_e_t => e

            let! e_schema, e_ctx, e_st, _r =
              decomposeCoType e.Location e_checked.Type

            // Unify the co parameters
            do!
              TypeValue.Unify(step_loc, schema, e_schema)
              |> Expr<'T, 'Id, 'valueExt>.liftUnification

            do!
              TypeValue.Unify(step_loc, ctx, e_ctx)
              |> Expr<'T, 'Id, 'valueExt>.liftUnification

            do!
              TypeValue.Unify(step_loc, st, e_st)
              |> Expr<'T, 'Id, 'valueExt>.liftUnification

            return
              { TypeCheckedCoStep.Location = step_loc
                Step = TypeCheckedCoStepRec.CoReturnBang(e_checked) },
              e_checked.Type,
              ctx_e
        }

      state {
        let! tc_ctx = state.GetContext()

        // Decompose expected type or create fresh vars
        let! schema, ctx_t, st, expected_res = decomposeExpectedCoType ()

        // Typecheck the body
        let! body_checked, result_t, _ctx_body =
          typeCheckCoStep schema ctx_t st expected_res co.Body

        // result_t is already the full Co type from CoReturn/CoReturnBang
        // Unify with expected type if available
        match context_t with
        | Some expected_t ->
          do!
            TypeValue.Unify(loc0, result_t, expected_t)
            |> Expr<'T, 'Id, 'valueExt>.liftUnification
        | None -> ()

        let co_checked =
          { TypeCheckedExprCo.Body = body_checked
            TypeCheckedExprCo.Location = loc0 }

        return co_checked, result_t, Kind.Star, tc_ctx
      }
