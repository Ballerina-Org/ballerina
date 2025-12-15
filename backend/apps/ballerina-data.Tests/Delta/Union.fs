module Ballerina.Data.Tests.Delta.Union

open NUnit.Framework
open System
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Delta.ToUpdater
open Ballerina.Data.Delta.Model
open Ballerina.Collections.Sum

let deltaExt (_ext: unit) : Value<TypeValue, Unit> -> Sum<Value<TypeValue, Unit>, 'a> =
  fun (v: Value<TypeValue, Unit>) -> sum.Return v

let symbol name : TypeSymbol =
  { Name = name |> Identifier.LocalScope
    Guid = Guid.CreateVersion7() }

[<Test>]
let ``Delta.Union: Updates matching union case correctly`` () =
  let caseName = "some"
  let caseSymbol = symbol caseName

  let unionValue =
    Value<TypeValue, Unit>
      .UnionCase(
        caseSymbol.Name |> TypeCheckScope.Empty.Resolve,
        PrimitiveValue.Int32 10 |> Value<TypeValue, Unit>.Primitive
      )

  let delta =
    Delta<Unit, Unit>.Union(caseName, Delta.Replace(PrimitiveValue.Int32 99 |> Value<TypeValue, Unit>.Primitive))

  match Delta.ToUpdater deltaExt delta with
  | Sum.Left updater ->
    match updater unionValue with
    | Sum.Left(Value.UnionCase(updatedSymbol, updatedValue)) ->
      Assert.That(updatedSymbol, Is.EqualTo(caseSymbol.Name |> TypeCheckScope.Empty.Resolve))
      Assert.That(updatedValue, Is.EqualTo(PrimitiveValue.Int32 99 |> Value<TypeValue, Unit>.Primitive))
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
    | _ -> Assert.Fail "Unexpected value shape"
  | Sum.Right err -> Assert.Fail $"Delta.ToUpdater failed: {err}"

[<Test>]
let ``Delta.Union: Returns original value when case does not match`` () =
  let actualSymbol = symbol "actual"

  let unionValue =
    Value<TypeValue, Unit>
      .UnionCase(
        actualSymbol.Name |> TypeCheckScope.Empty.Resolve,
        PrimitiveValue.Int32 42 |> Value<TypeValue, Unit>.Primitive
      )

  let delta =
    Delta.Union("unmatched", Delta.Replace(PrimitiveValue.Int32 999 |> Value<TypeValue, Unit>.Primitive))

  match Delta.ToUpdater deltaExt delta with
  | Sum.Left updater ->
    match updater unionValue with
    | Sum.Left(v) -> Assert.That(v, Is.EqualTo unionValue)
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
  | Sum.Right err -> Assert.Fail $"Delta.ToUpdater failed: {err}"
