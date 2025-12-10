namespace Ballerina.DSL.Next.Types.TypeChecker

module TupleDes =
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
    static member internal TypeCheckTupleDes
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprTupleDes<TypeExpr, Identifier, 'valueExt>, 'valueExt> =
      fun
          context_t
          ({ ExprTupleDes.Tuple = fields
             Item = fieldName }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        let error e = Errors.Singleton(loc0, e)

        state {
          let! ctx = state.GetContext()
          let! fields, t_fields, fields_k, _ = !fields
          do! fields_k |> Kind.AsStar |> ofSum |> state.Ignore

          let! t_fields =
            t_fields
            |> TypeValue.AsTuple
            |> ofSum
            |> state.Map WithSourceMapping.Getters.Value

          let! t_field =
            t_fields
            |> List.tryItem (fieldName.Index - 1)
            |> sum.OfOption($"Error: cannot find item {fieldName.Index} in tuple {fields}" |> error)
            |> state.OfSum

          return Expr.TupleDes(fields, fieldName, loc0, ctx.Scope), t_field, Kind.Star, ctx
        }
