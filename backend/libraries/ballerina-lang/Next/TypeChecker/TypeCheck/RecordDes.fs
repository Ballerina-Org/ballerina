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
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member internal TypeCheckRecordDes
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprRecordDes<TypeExpr, Identifier, 'valueExt>, 'valueExt> =
      fun
          context_t
          ({ Expr = record_expr
             Field = fieldName }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          let! ctx = state.GetContext()
          let! record_v, record_t, record_k = !record_expr
          do! record_k |> Kind.AsStar |> ofSum |> state.Ignore

          let! fields_t =
            state.Either
              (record_t
               |> TypeValue.AsRecord
               |> ofSum
               |> state.Map WithTypeExprSourceMapping.Getters.Value)
              (state {
                let! id = TypeCheckState.TryResolveIdentifier(fieldName, loc0)
                let! fields_t = TypeCheckState.TryFindRecordField(id, loc0) |> state.Map fst
                let expected_record_t = TypeValue.CreateRecord fields_t

                do!
                  TypeValue.Unify(loc0, record_t, expected_record_t)
                  |> Expr.liftUnification
                  |> state.MapError(Errors.SetPriority ErrorPriority.High)

                return fields_t
              })
            |> state.MapError Errors.FilterHighestPriorityOnly

          return!
            state {
              let! field_k, field_t =
                fields_t
                |> OrderedMap.toSeq
                |> Seq.map (fun (k, v) -> (k, v))
                |> Seq.tryFind (fun (k, _v) -> k.Name.LocalName = fieldName.LocalName)
                |> sum.OfOption(
                  $"Error: cannot find field {fieldName} in record {record_v}"
                  |> Ballerina.Errors.Errors.Singleton
                )
                |> ofSum

              // do Console.WriteLine($"---- TypeCheck RecordDes {fieldName} ----")

              let! fieldName =
                state.Either
                  (TypeCheckState.TryResolveIdentifier(field_k, loc0))
                  (state { return fieldName |> ctx.Scope.Resolve })

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

              return Expr.RecordDes(record_v, fieldName, loc0, ctx.Scope), field_t, Kind.Star
            }
            |> state.MapError(Errors.SetPriority ErrorPriority.High)
        }
