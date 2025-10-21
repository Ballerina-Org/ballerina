namespace Ballerina.DSL.Next.Types.TypeChecker

module RecordDes =
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
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal TypeCheckRecordDes
      (typeCheckExpr: TypeChecker, loc0: Location)
      : TypeChecker<ExprRecordDes<TypeExpr, Identifier>> =
      fun
          context_t
          ({ Expr = fields_expr
             Field = fieldName }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          let! ctx = state.GetContext()
          let! fields, t_fields, fields_k = !fields_expr
          do! fields_k |> Kind.AsStar |> ofSum |> state.Ignore

          let! t_fields =
            t_fields
            |> TypeValue.AsRecord
            |> ofSum
            |> state.Map WithTypeExprSourceMapping.Getters.Value

          return!
            state {
              let! field_k, field_t =
                t_fields
                |> OrderedMap.toSeq
                |> Seq.map (fun (k, v) -> (k, v))
                |> Seq.tryFind (fun (k, _v) -> k.Name.LocalName = fieldName.LocalName)
                |> sum.OfOption(
                  $"Error: cannot find field {fieldName} in record {fields}"
                  |> Ballerina.Errors.Errors.Singleton
                )
                |> ofSum

              // do Console.WriteLine($"---- TypeCheck RecordDes {fieldName} ----")

              let! fieldName =
                state.Either
                  (TypeCheckState.TryResolveIdentifier(field_k, loc0))
                  (state { return fieldName |> ctx.Types.Scope.Resolve })

              // do Console.WriteLine($"---- TypeChecked RecordDes {fieldName} ----")
              // t_fields
              // |> OrderedMap.keys

              // do Console.WriteLine($"{fields_expr}.{fieldName}")
              // do Console.WriteLine($"fields: {fields.ToFSharpString}")
              // do Console.ReadLine() |> ignore

              // let! field_t =
              //   t_fields
              //   |> OrderedMap.tryFindWithError fieldName "fields" fieldName.ToFSharpString
              //   |> ofSum

              return Expr.RecordDes(fields, fieldName, loc0, ctx.Types.Scope), field_t, Kind.Star
            }
            |> state.MapError(Errors.SetPriority ErrorPriority.High)
        }
