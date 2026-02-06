namespace Ballerina.DSL.Next.Types.TypeChecker

module Lambda =
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
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckLambda<'valueExt when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprLambda<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun
          context_t
          ({ Param = x
             ParamType = t
             Body = body }) ->
        // let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          let! t =
            t
            |> Option.map (fun t ->
              t
              |> TypeExpr.Eval () typeCheckExpr None loc0
              |> Expr<'T, 'Id, 'valueExt>.liftTypeEval)
            |> state.RunOption

          // (p: State<'a, UnificationContext, UnificationState, Errors>)
          // : State<'a, TypeCheckContext, TypeCheckState, Errors> =

          // do Console.WriteLine($"Typechecking lambda parameter {x.Name} with context {context_t}")
          // do Console.ReadLine() |> ignore

          let! var_type =
            match t, context_t with
            | Some t, _ -> state { t }
            | _, Some(TypeValue.Arrow({ value = input, _ })) -> state { input, Kind.Star }
            | _ ->
              state {
                let guid = Guid.CreateVersion7()

                let freshVar =
                  { TypeVar.Name = x.Name + "_lambda_" + guid.ToString()
                    Synthetic = true
                    Guid = guid }

                do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists freshVar))
                freshVar |> TypeValue.Var, Kind.Star
              }

          let body_constraint_t =
            match context_t with
            | Some(TypeValue.Arrow({ value = _, TypeValue.Var v })) when v.Synthetic -> None
            | Some(TypeValue.Arrow({ value = _, ret_t })) -> Some ret_t
            | _ -> None


          let! body, t_body, body_k, _ =
            body_constraint_t => body
            |> state.MapContext(
              TypeCheckContext.Updaters.Values(Map.add (x.Name |> Identifier.LocalScope |> ctx.Scope.Resolve) var_type)
            )

          do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

          let! t_x =
            var_type
            |> fst
            |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
            |> Expr<'T, 'Id, 'valueExt>.liftInstantiation
          // let! t_body = t_body |> TypeValue.Instantiate () (TypeExpr.Eval ()) loc0 |> Expr<'T, 'Id, 'valueExt>.liftInstantiation

          // do!
          //     UnificationState.DeleteVariable freshVar
          //       |> TypeValue.EquivalenceClassesOp
          //       |> Expr<'T, 'Id, 'valueExt>.liftUnification

          let! t_res =
            TypeValue.CreateArrow(t_x, t_body)
            |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
            |> Expr.liftInstantiation

          return Expr.Lambda(x, Some t_x, body, loc0, ctx.Scope), t_res, Kind.Star, ctx
        }
// |> state.MapError(Errors.Map(String.appendNewline $"...when typechecking `fun {x.Name} -> ...`"))
