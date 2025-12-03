namespace Ballerina.DSL.Next.Types.TypeChecker

module TypeApply =
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
  open Ballerina.DSL.Next.Types.TypeChecker.If
  open Ballerina.DSL.Next.Types.TypeChecker.Let
  open Ballerina.DSL.Next.Types.TypeChecker.RecordCons
  open Ballerina.DSL.Next.Types.TypeChecker.RecordWith
  open Ballerina.DSL.Next.Types.TypeChecker.RecordDes
  open Ballerina.DSL.Next.Types.TypeChecker.UnionDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumCons
  open Ballerina.DSL.Next.Types.TypeChecker.TupleDes
  open Ballerina.DSL.Next.Types.TypeChecker.TupleCons
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLambda
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member internal TypeCheckTypeApply
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprTypeApply<TypeExpr, Identifier, 'valueExt>, 'valueExt> =
      fun context_t ({ Func = fExpr; TypeArg = tExpr }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        let error e = Errors.Singleton(loc0, e)

        state {
          let! ctx = state.GetContext()
          // do Console.WriteLine($"TypeApply: fExpr = {fExpr}")
          let! f, f_t, f_k = !fExpr

          let! f_k_i, f_k_o = f_k |> Kind.AsArrow |> ofSum

          // do Console.WriteLine($"TypeApply: f = {f}, f_t = {f_t}, f_k = {f_k}, f_k_i = {f_k_i}, f_k_o = {f_k_o}")

          let! t_val, t_k = tExpr |> TypeExpr.Eval None loc0 |> Expr.liftTypeEval
          // do Console.WriteLine($"TypeApply: tExpr = {tExpr}, t_val = {t_val}, t_k = {t_k}")

          if f_k_i <> t_k then
            return!
              $"Error: mismatched kind, expected {f_k_i} but got {t_k}"
              |> error
              |> state.Throw
          else
            // do Console.WriteLine($"TypeApply: from f_t to f_res")

            let! f_res, _ =
              TypeExpr.Apply(f_t.AsExpr, tExpr)
              |> TypeExpr.Eval None loc0
              |> Expr.liftTypeEval

            // let! f_res = f_res |> TypeValue.Instantiate TypeExpr.Eval loc0 |> Expr.liftInstantiation

            // do Console.WriteLine($"TypeApply: f_res = {f_res}")

            let res = Expr.TypeApply(f, t_val, loc0, ctx.Scope), f_res, f_k_o
            // do Console.WriteLine($"TypeApply: {res}")
            // do Console.ReadLine() |> ignore
            return res
        }
