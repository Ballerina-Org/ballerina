namespace Ballerina.DSL.Next.Types.TypeChecker

#nowarn 0086

[<AutoOpen>]
module Value =
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
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLet
  open Ballerina.DSL.Next.Types.TypeChecker.TypeApply
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Reader.WithError

  type IsValueInstanceOf<'valueExt when 'valueExt: comparison> =
    Value<TypeValue<'valueExt>, 'valueExt> * TypeValue<'valueExt>
      -> Reader<
        // Value<TypeValue<'valueExt>, 'valueExt>,
        Unit,
        TypeCheckContext<'valueExt> * TypeCheckState<'valueExt>,
        Errors<Unit>
       >

  and IsExtInstanceOf<'valueExt when 'valueExt: comparison> =
    IsValueInstanceOf<'valueExt>
      -> 'valueExt
      -> TypeValue<'valueExt>
      -> Reader<Unit, TypeCheckContext<'valueExt> * TypeCheckState<'valueExt>, Errors<Unit>>

  type Value<'T, 'ext> with
    static member IsInstanceOf<'valueExt when 'valueExt: comparison>
      (ext_checker: IsExtInstanceOf<'valueExt>)
      : IsValueInstanceOf<'valueExt> =
      fun (v, t) ->
        reader {
          let! (_ctx: TypeCheckContext<'valueExt>, s: TypeCheckState<'valueExt>) = reader.GetContext()
          let (<=) v t = Value.IsInstanceOf ext_checker (v, t)

          match v, t with
          | _, TypeValue.Lookup l ->
            let! resolved_t, _ =
              s.Bindings
              |> Map.tryFindWithError
                (l |> ResolvedIdentifier.FromIdentifier)
                "type bindings"
                (fun () -> $"Error: cannot find type binding for {l.LocalName}")
                ()
              |> reader.OfSum

            return! v <= resolved_t
          | Value.Primitive(PrimitiveValue.Bool _), TypeValue.Primitive { value = PrimitiveType.Bool }
          | Value.Primitive(PrimitiveValue.Date _), TypeValue.Primitive { value = PrimitiveType.DateOnly }
          | Value.Primitive(PrimitiveValue.DateTime _), TypeValue.Primitive { value = PrimitiveType.DateTime }
          | Value.Primitive(PrimitiveValue.Decimal _), TypeValue.Primitive { value = PrimitiveType.Decimal }
          | Value.Primitive(PrimitiveValue.Float32 _), TypeValue.Primitive { value = PrimitiveType.Float32 }
          | Value.Primitive(PrimitiveValue.Float64 _), TypeValue.Primitive { value = PrimitiveType.Float64 }
          | Value.Primitive(PrimitiveValue.Guid _), TypeValue.Primitive { value = PrimitiveType.Guid }
          | Value.Primitive(PrimitiveValue.Int32 _), TypeValue.Primitive { value = PrimitiveType.Int32 }
          | Value.Primitive(PrimitiveValue.Int64 _), TypeValue.Primitive { value = PrimitiveType.Int64 }
          | Value.Primitive(PrimitiveValue.String _), TypeValue.Primitive { value = PrimitiveType.String }
          | Value.Primitive(PrimitiveValue.TimeSpan _), TypeValue.Primitive { value = PrimitiveType.TimeSpan }
          | Value.Primitive(PrimitiveValue.Unit), TypeValue.Primitive { value = PrimitiveType.Unit } -> return ()
          | Value.Sum(case, value), TypeValue.Sum { value = case_types } ->
            let! case_type =
              case_types
              |> List.tryItem (case.Case - 1)
              |> sum.OfOption(
                (fun () -> $"Error: case {case.Case} does not exist in type {case_types}")
                |> Errors.Singleton()
              )
              |> reader.OfSum

            return! value <= case_type
          | Value.UnionCase(case, value), TypeValue.Union { value = case_types } ->
            let! case_symbol =
              s.Symbols.UnionCases
              |> Map.tryFind case
              |> sum.OfOption((fun () -> $"Error: case {case.Name} does not exist") |> Errors.Singleton())
              |> reader.OfSum

            let! case_type =
              case_types
              |> OrderedMap.tryFind case_symbol
              |> sum.OfOption(
                (fun () -> $"Error: case {case.Name} does not exist in type {case_types}")
                |> Errors.Singleton()
              )
              |> reader.OfSum

            return! value <= case_type
          | Value.Tuple fields, TypeValue.Tuple { value = field_types } ->
            if List.length fields <> List.length field_types then
              return!
                Errors.Singleton () (fun () ->
                  $"Error: tuple length mismatch, expected {List.length field_types} but got {List.length fields}")
                |> reader.Throw
            else
              return!
                List.zip fields field_types
                |> Seq.map (fun (field_value, field_type) -> field_value <= field_type)
                |> reader.All
                |> reader.Ignore
          | Value.Record fields, TypeValue.Record { value = field_types } ->
            do!
              fields
              |> Map.toSeq
              |> Seq.map (fun (field_name, field_value) ->
                reader {
                  let! field_symbol =
                    s.Symbols.RecordFields
                    |> Map.tryFind field_name
                    |> sum.OfOption((fun () -> $"Error: field {field_name} does not exist") |> Errors.Singleton())
                    |> reader.OfSum

                  let! field_type, _ =
                    field_types
                    |> OrderedMap.tryFind field_symbol
                    |> sum.OfOption(
                      (fun () -> $"Error: field {field_name} does not exist in type {field_types}")
                      |> Errors.Singleton()
                    )
                    |> reader.OfSum

                  return! field_value <= field_type
                })
              |> reader.All
              |> reader.Ignore
          | Value.Ext(ext, _), t -> return! ext_checker (Value.IsInstanceOf ext_checker) ext t
          | Value.Var _, _ -> return! failwith "Not implemented"
          | Value.RecordDes _, _ -> return! failwith "Not implemented" // this is a lambda
          | Value.UnionCons _, _ -> return! failwith "Not implemented" // this is a lambda
          | Value.Lambda _, _ -> return! failwith "Not implemented"
          | Value.TypeLambda _, _ -> return! failwith "Not implemented"
          | _ ->
            return!
              Errors.Singleton () (fun () -> $"Error: value {v} is not an instance of type {t}")
              |> reader.Throw
        }
