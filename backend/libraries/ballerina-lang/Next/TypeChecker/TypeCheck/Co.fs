namespace Ballerina.DSL.Next.Types.TypeChecker

module Co =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckCoStep<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      (schema_t: TypeValue<'valueExt>)
      (ctx_t: TypeValue<'valueExt>)
      (st_t: TypeValue<'valueExt>)
      (res_t: TypeValue<'valueExt>)
      (step: ExprCoStep<TypeExpr<'valueExt>, Identifier, 'valueExt>)
      : State<
          TypeCheckedCoStep<'valueExt>,
          TypeCheckContext<'valueExt>,
          TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =
      let loc0 = step.Location
      let (=>) c e = typeCheckExpr c e

      let typeCheckStep =
        Expr<'T, 'Id, 'valueExt>.TypeCheckCoStep
          config typeCheckExpr schema_t ctx_t st_t res_t

      state {
        let! ctx = state.GetContext()

        match step.Step with
        // let! x = expr; rest
        // expr must produce Co[schema][ctx][st][a] for some a
        // bind x : a and typecheck rest
        | ExprCoStepRec.CoLetBang(var, valueExpr, rest) ->
          // Create a fresh type var for the bound value type
          let aGuid = Guid.CreateVersion7()

          let freshA =
            { TypeVar.Name = var.Name + "_cobind_" + aGuid.ToString()
              Synthetic = true
              Guid = aGuid }

          do!
            state.SetState(
              TypeCheckState.Updaters.Vars(
                UnificationState.EnsureVariableExists freshA
              )
            )

          let a_t = TypeValue.Var freshA
          let expectedCoType = config.MkCoType schema_t ctx_t st_t a_t

          let! checkedValue, _ = (Some expectedCoType) => valueExpr

          // After typechecking, instantiate a_t to see what it resolved to
          let! a_t_resolved =
            a_t
            |> TypeValue.Instantiate
              ()
              (TypeExpr.Eval config typeCheckExpr)
              loc0
            |> Expr.liftInstantiation

          // Bind x : a_t_resolved in context for the rest
          let! checkedRest =
            typeCheckStep rest
            |> state.MapContext(
              TypeCheckContext.Updaters.Values(
                Map.add
                  (var.Name |> Identifier.LocalScope |> ctx.Scope.Resolve)
                  (a_t_resolved, Kind.Star)
              )
            )

          return
            { TypeCheckedCoStep.Location = loc0
              Step = TypeCheckedCoStepRec.CoLetBang(var, checkedValue, checkedRest) }

        // do! expr; rest
        // expr must produce Co[schema][ctx][st][()]
        | ExprCoStepRec.CoDoBang(valueExpr, rest) ->
          let expectedCoType =
            config.MkCoType schema_t ctx_t st_t (TypeValue.CreatePrimitive PrimitiveType.Unit)

          let! checkedValue, _ = (Some expectedCoType) => valueExpr

          let! checkedRest = typeCheckStep rest

          return
            { TypeCheckedCoStep.Location = loc0
              Step = TypeCheckedCoStepRec.CoDoBang(checkedValue, checkedRest) }

        // return expr
        // expr must have type res
        | ExprCoStepRec.CoReturn expr ->
          let! checkedExpr, _ = (Some res_t) => expr

          return
            { TypeCheckedCoStep.Location = loc0
              Step = TypeCheckedCoStepRec.CoReturn checkedExpr }

        // return! expr
        // expr must have type Co[schema][ctx][st][res]
        | ExprCoStepRec.CoReturnBang expr ->
          let expectedCoType = config.MkCoType schema_t ctx_t st_t res_t
          let! checkedExpr, _ = (Some expectedCoType) => expr

          return
            { TypeCheckedCoStep.Location = loc0
              Step = TypeCheckedCoStepRec.CoReturnBang checkedExpr }
      }

    static member internal TypeCheckCo<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      : TypeChecker<
          Location * ExprCo<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          'valueExt
         >
      =
      fun
          context_t
          (coLoc,
           { Body = body
             Location = _ }) ->

        let loc0 = coLoc

        state {
          let! ctx = state.GetContext()

          // Create fresh type vars for schema, ctx, st, res
          let mkFresh name kind =
            let guid = Guid.CreateVersion7()

            let freshVar =
              { TypeVar.Name = name + "_co_" + guid.ToString()
                Synthetic = true
                Guid = guid }

            freshVar, kind

          let schemaVar, _schemaKind = mkFresh "schema" Kind.Schema
          let ctxVar, _ctxKind = mkFresh "ctx" Kind.Star
          let stVar, _stKind = mkFresh "st" Kind.Star
          let resVar, _resKind = mkFresh "res" Kind.Star

          do!
            [ schemaVar; ctxVar; stVar; resVar ]
            |> List.map (fun v ->
              state.SetState(
                TypeCheckState.Updaters.Vars(
                  UnificationState.EnsureVariableExists v
                )
              ))
            |> state.All
            |> state.Ignore

          let schema_t = TypeValue.Var schemaVar
          let ctx_t = TypeValue.Var ctxVar
          let st_t = TypeValue.Var stVar
          let res_t = TypeValue.Var resVar

          // If there's an expected type, decompose it as Co[schema][ctx][st][res]
          do!
            match context_t with
            | Some(TypeValue.Imported { Sym = sym
                                        Arguments = [ s; c; st; r ] })
              when sym = config.CoTypeSymbol ->
              state {
                do!
                  TypeValue.Unify(loc0, schema_t, s)
                  |> Expr<'T, 'Id, 'valueExt>.liftUnification

                do!
                  TypeValue.Unify(loc0, ctx_t, c)
                  |> Expr<'T, 'Id, 'valueExt>.liftUnification

                do!
                  TypeValue.Unify(loc0, st_t, st)
                  |> Expr<'T, 'Id, 'valueExt>.liftUnification

                do!
                  TypeValue.Unify(loc0, res_t, r)
                  |> Expr<'T, 'Id, 'valueExt>.liftUnification
              }
            | Some expected_t ->
              let expectedCoType = config.MkCoType schema_t ctx_t st_t res_t

              TypeValue.Unify(loc0, expectedCoType, expected_t)
              |> Expr<'T, 'Id, 'valueExt>.liftUnification
            | None -> state { return () }

          // Typecheck the body steps
          let! checkedBody =
            Expr<'T, 'Id, 'valueExt>.TypeCheckCoStep
              config typeCheckExpr schema_t ctx_t st_t res_t body

          // Construct result type
          let result_t = config.MkCoType schema_t ctx_t st_t res_t

          let! result_t =
            result_t
            |> TypeValue.Instantiate
              ()
              (TypeExpr.Eval config typeCheckExpr)
              loc0
            |> Expr.liftInstantiation

          return
            TypeCheckedExpr.Co(
              { TypeCheckedExprCo.Body = checkedBody
                Location = loc0 },
              result_t,
              Kind.Star,
              loc0,
              ctx.Scope
            ),
            ctx
        }
