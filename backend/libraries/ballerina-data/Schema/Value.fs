namespace Ballerina.Data.Schema

open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types
open Ballerina.Data.Arity.Model
open Ballerina.Data.Schema.Model
open Ballerina.Data.Schema.ActivePatterns
open Ballerina.Errors

module Value =
  let private emptyRecord = Value.Record Map.empty

  let private valueWrapper value =
    Value.Record(Map.ofList [ (Identifier.LocalScope "value") |> TypeCheckScope.Empty.Resolve, value ])

  let private right (value: Value<_, _> option) =
    Value.Record(
      Map.ofList
        [ (Identifier.LocalScope "IsSome") |> TypeCheckScope.Empty.Resolve,
          Value.Primitive(PrimitiveValue.Bool value.IsSome)
          (Identifier.LocalScope "Value") |> TypeCheckScope.Empty.Resolve,
          match value with
          | None -> emptyRecord
          | Some(Value.UnionCase _) ->
            Value.Record(Map.ofList [ (Identifier.LocalScope "Value") |> TypeCheckScope.Empty.Resolve, value.Value ])
          | Some _ -> value.Value ]
    )

  let enwrapArity (value: Value<_, 'valueExt>) (typeValue: TypeValue) (arity: LookupArity) =

    match Cardinality.FromArity arity, value, typeValue with
    | One, Value.UnionCase _, _
    | One, Value.Record(CollectionReferenceValue _), _ -> right (Some value)
    //| Many, Value.Tuple elements, TypeValue.Set { value = TypeValue.Union _ } -> elements |> List.map valueWrapper |> Value.Tuple
    | Many, Value.UnionCase _, _ -> Value.Tuple [ valueWrapper value ]
    | _ -> value

  let rec insert
    (value: Value<TypeValue, 'valueExt>)
    (into: Value<TypeValue, 'valueExt>)
    (path: List<UpdaterPathStep>)
    : Sum<Value<TypeValue, 'valueExt>, Errors> =

    sum {
      match path with
      | [] -> return! sum.Throw(Errors.Singleton "Empty path is invalid")
      | [ lastStep ] ->
        match into, lastStep with
        | Value.Record r, UpdaterPathStep.Field f ->
          return Value.Record(Map.add (f |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) value r)

        | Value.UnionCase(c, target), UpdaterPathStep.UnionCase(caseName, _var) when caseName = c.Name ->
          return Value.UnionCase(c, target)
        | _ -> return into
      | step :: rest ->
        match into, step with
        | Value.Record r, UpdaterPathStep.Field f ->
          let! ts, v =
            r
            |> Map.tryFindByWithError (fun (ts, _) -> ts.Name = f) "value insert" $"field '{f}' not present in record"

          let! updated = insert value v rest
          let fields = Map.add ts updated r
          return Value.Record fields

        | Value.UnionCase(c, target), UpdaterPathStep.UnionCase(caseName, _var) when caseName = c.Name ->
          let! updated = insert value target rest
          return Value.UnionCase(c, updated)
        | Value.Tuple elements, UpdaterPathStep.TupleItem index ->
          let element = elements |> List.item index
          let! value = insert value element rest
          let updated = elements |> List.updateAt index value
          return Value.Tuple updated
        | Value.Tuple [ into ], _ ->
          let! updated = insert value into (step :: rest)
          return Value.Tuple [ updated ]
        | _ -> return into
    }
