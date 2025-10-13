namespace Ballerina.Data.Schema

open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.Data.Schema.Model
open Ballerina.Data.Schema.Patterns
open Ballerina.Errors

module Value =
  let rec insert
    (value: Value<TypeValue, ValueExt>)
    (into: Value<TypeValue, ValueExt>)
    (path: List<UpdaterPathStep>)
    : Sum<Value<TypeValue, ValueExt>, Errors> =

    sum {
      match path with
      | [] -> return! sum.Throw(Errors.Singleton "Empty path is invalid")
      | [ lastStep ] ->
        let! target = Value.AsRecord into
        let! field = UpdaterPathStep.AsField lastStep
        return Value.Record(Map.add (Identifier.LocalScope field) value target)
      | step :: rest ->
        match into, step with
        | Value.Record r, UpdaterPathStep.Field f ->
          let! ts, v =
            r
            |> Map.tryFindByWithError
              (fun (ts, _) -> ts.LocalName = f)
              "value insert"
              $"field '{f}' not present in record"

          let! updated = insert value v rest
          let fields = Map.add ts updated r
          return Value.Record fields

        | Value.UnionCase(c, target), UpdaterPathStep.UnionCase(caseName, _var) when caseName = c.Name.LocalName ->
          let! updated = insert value target rest
          return Value.UnionCase(c, updated)

        | Value.Tuple [ into ], _ ->
          let! updated = insert value into (step :: rest)
          return Value.Tuple [ updated ]
        | _ -> return into
    }
