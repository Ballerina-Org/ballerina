module Ballerina.Data.Tests.Delta.Record

open NUnit.Framework
open System
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Delta.ToUpdater
open Ballerina.Data.Delta.Model
open Ballerina.Collections.Sum
open Ballerina.StdLib.OrderPreservingMap
open Ballerina.Cat.Collections.OrderedMap

let symbol name : TypeSymbol =
  { Name = name |> Identifier.LocalScope
    Guid = Guid.CreateVersion7() }

[<Test>]
let ``Delta.Record: Updates field in a record correctly`` () =
  let fieldName = "age"
  let typeSymbol = symbol fieldName

  let recordValue =
    Value<Unit>
      .Record(
        Map.ofList
          [ typeSymbol.Name.LocalName
            |> Identifier.LocalScope
            |> TypeCheckScope.Empty.Resolve,
            Value<Unit>.Primitive(PrimitiveValue.Int32 99) ]
      )

  let delta =
    Delta.Record(fieldName, Delta.Replace(PrimitiveValue.Int32 100 |> Value<Unit>.Primitive))

  match Delta.ToUpdater delta with
  | Sum.Left updater ->
    match updater recordValue with
    | Sum.Left(Value.Record updated) ->
      let updatedValue =
        updated.[typeSymbol.Name.LocalName
                 |> Identifier.LocalScope
                 |> TypeCheckScope.Empty.Resolve]

      Assert.That(PrimitiveValue.Int32 100 |> Value<Unit>.Primitive, Is.EqualTo updatedValue)
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
    | _ -> Assert.Fail "Unexpected value shape"
  | Sum.Right err -> Assert.Fail $"Delta.ToUpdater failed: {err}"

[<Test>]
let ``Delta.Record: Fails when field not found in value`` () =
  let fieldName = "age"

  let recordValue = Value<Unit>.Record(Map.empty)

  let delta =
    Delta.Record(fieldName, Delta.Replace(PrimitiveValue.Int32 100 |> Value<Unit>.Primitive))

  match Delta.ToUpdater delta with
  | Sum.Left updater ->
    match updater recordValue with
    | Sum.Right _err -> Assert.Pass()
    | _ -> Assert.Fail "Expected error due to missing field in record value"
  | Sum.Right err -> Assert.Fail $"Unexpected failure in ToUpdater: {err}"
