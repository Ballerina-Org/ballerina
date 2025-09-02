module Ballerina.Data.Tests.Delta.Union

open NUnit.Framework
open System
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Delta.ToUpdater
open Ballerina.Data.Delta.Model
open Ballerina.Collections.Sum

let symbol name : TypeSymbol =
  { Name = name
    Guid = Guid.CreateVersion7() }

[<Test>]
let ``Delta.Union: Updates matching union case correctly`` () =
  let caseName = "some"
  let caseSymbol = symbol caseName
  let caseType = TypeValue.Primitive PrimitiveType.Int32

  let unionType = TypeValue.Union(Map.ofList [ caseSymbol, caseType ])

  let unionValue =
    Value.UnionCase(caseSymbol, PrimitiveValue.Int 10 |> Value.Primitive)

  let delta =
    Delta.Union(caseName, Delta.Replace(PrimitiveValue.Int 99 |> Value.Primitive))

  match Delta.ToUpdater unionType delta with
  | Sum.Left updater ->
    match updater unionValue with
    | Sum.Left(Value.UnionCase(updatedSymbol, updatedValue)) ->
      Assert.That(updatedSymbol, Is.EqualTo caseSymbol)
      Assert.That(updatedValue, Is.EqualTo(PrimitiveValue.Int 99 |> Value.Primitive))
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
    | _ -> Assert.Fail "Unexpected value shape"
  | Sum.Right err -> Assert.Fail $"Delta.ToUpdater failed: {err}"

[<Test>]
let ``Delta.Union: Returns original value when case does not match`` () =
  let actualSymbol = symbol "actual"
  let unmatchedSymbol = symbol "unmatched"
  let caseType = TypeValue.Primitive PrimitiveType.Int32

  let unionType = TypeValue.Union(Map.ofList [ unmatchedSymbol, caseType ])

  let unionValue =
    Value.UnionCase(actualSymbol, PrimitiveValue.Int 42 |> Value.Primitive)

  let delta =
    Delta.Union(unmatchedSymbol.Name, Delta.Replace(PrimitiveValue.Int 999 |> Value.Primitive))

  match Delta.ToUpdater unionType delta with
  | Sum.Left updater ->
    match updater unionValue with
    | Sum.Left(v) -> Assert.That(v, Is.EqualTo unionValue)
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
  | Sum.Right err -> Assert.Fail $"Delta.ToUpdater failed: {err}"

[<Test>]
let ``Delta.Union: Fails when case not found in type`` () =
  let unionType = TypeValue.Union(Map.empty)

  let delta =
    Delta.Union("missing", Delta.Replace(PrimitiveValue.Int 1 |> Value.Primitive))

  match Delta.ToUpdater unionType delta with
  | Sum.Left _ -> Assert.Fail "Expected failure due to missing case in union type"
  | Sum.Right _ -> Assert.Pass()
