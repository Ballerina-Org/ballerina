namespace Ballerina.DSL.Next.Types.TypeChecker

module SumDes =
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

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member internal TypeCheckSumDes
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprSumDes<TypeExpr, Identifier, 'valueExt>, 'valueExt> =
      fun context_t ({ Handlers = handlers }) ->
        let (!) = typeCheckExpr None

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        let error e = Errors.Singleton(loc0, e)

        state {
          let! ctx = state.GetContext()

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
                  Synthetic = true
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
                          Synthetic = true
                          Guid = guid }

                      do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists fresh_var))

                      return TypeValue.Var fresh_var
                    | Some(t_i, _t_o) ->
                      return!
                        t_i
                        |> List.tryItem (_k.Case - 1)
                        |> sum.OfOption("impossible" |> error)
                        |> state.OfSum
                  }

                let! body, body_t, body_k, _ =
                  !body
                  |> state.MapContext(
                    TypeCheckContext.Updaters.Values(
                      Map.add (var.Name |> Identifier.LocalScope |> ctx.Scope.Resolve) (var_t, Kind.Star)
                    )
                  )

                do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

                do! TypeValue.Unify(loc0, body_t, result_var_t) |> Expr.liftUnification

                let! var_t = TypeValue.Instantiate TypeExpr.Eval loc0 var_t |> Expr.liftInstantiation

                return (var, body), var_t
              })
            |> state.AllMap
            |> state.MapError(Errors.SetPriority ErrorPriority.High)

          let handlersSorted =
            handlers
            |> Map.map (fun _k -> snd)
            |> Map.toSeq
            |> Seq.sortBy (fun (k, _v) -> k.Case)
            |> Seq.toList

          let handlerExprs = handlers |> Map.map (fun _k -> fst)

          let handlerTypes = handlersSorted |> List.map snd

          let! result_t = TypeValue.Instantiate TypeExpr.Eval loc0 result_var_t |> Expr.liftInstantiation

          let sumValue = TypeValue.CreateSum handlerTypes
          let arrowValue = TypeValue.CreateArrow(sumValue, result_t)
          let! arrowValue = TypeValue.Instantiate TypeExpr.Eval loc0 arrowValue |> Expr.liftInstantiation

          // for kv in handler_vars do
          //   do!
          //       UnificationState.DeleteVariable kv
          //         |> TypeValue.EquivalenceClassesOp
          //         |> Expr<'T, 'Id, 'valueExt>.liftUnification

          // do!
          //     UnificationState.DeleteVariable result_var
          //       |> TypeValue.EquivalenceClassesOp
          //       |> Expr<'T, 'Id, 'valueExt>.liftUnification

          return Expr.SumDes(handlerExprs, loc0, ctx.Scope), arrowValue, Kind.Star, ctx
        }
