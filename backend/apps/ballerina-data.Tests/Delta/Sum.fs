module Ballerina.Data.Tests.Delta.Sum

open NUnit.Framework
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Delta.Model
open Ballerina.Data.Delta.ToUpdater
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Types.Patterns

[<Test>]
let ``Delta.Sum: Updates correct case index in sum value`` () =
  let sumValue =
    Value<Unit>.Sum({ Case = 1; Count = 1 }, PrimitiveValue.Int32 42 |> Value<Unit>.Primitive)

  let delta =
    Delta.Sum(1, Delta.Replace(PrimitiveValue.Int32 100 |> Value<Unit>.Primitive))

  match Delta.ToUpdater delta with
  | Sum.Left updater ->
    match updater sumValue with
    | Sum.Left(Value.Sum(updatedIndex, updatedValue)) ->
      Assert.That(updatedIndex, Is.EqualTo { Case = 1; Count = 1 })
      Assert.That(updatedValue, Is.EqualTo(PrimitiveValue.Int32 100 |> Value<Unit>.Primitive))
    | _ -> Assert.Fail "Unexpected result shape"
  | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"

[<Test>]
let ``Delta.Sum: Returns original value when index does not match`` () =
  let sumValue =
    Value<Unit>.Sum({ Case = 2; Count = 1 }, PrimitiveValue.String "untouched" |> Value<Unit>.Primitive)

  let delta =
    Delta.Sum(1, Delta.Replace(PrimitiveValue.Int32 100 |> Value<Unit>.Primitive))

  match Delta.ToUpdater delta with
  | Sum.Left updater ->
    match updater sumValue with
    | Sum.Left v -> Assert.That(v, Is.EqualTo sumValue)
    | _ -> Assert.Fail "Expected original value unchanged"
  | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"


[<Test>]
let ``Delta.Sum: Fails when delta type does not match case type TODO:decide`` () =
  let sumValue =
    Value<Unit>.Sum({ Case = 1; Count = 1 }, PrimitiveValue.Int32 42 |> Value<Unit>.Primitive)

  let delta =
    Delta.Sum(1, Delta.Replace(PrimitiveValue.String "wrong type" |> Value<Unit>.Primitive))

  match Delta.ToUpdater delta with
  | Sum.Left updater ->
    match updater sumValue with
    | Sum.Right _ -> Assert.Fail("Updater shouldn't fail due to type mismatch, TODO: confirm")
    | Sum.Left _ -> Assert.Pass()
  | Sum.Right err -> Assert.Fail $"Unexpected failure in Delta.ToUpdater: %A{err}"
