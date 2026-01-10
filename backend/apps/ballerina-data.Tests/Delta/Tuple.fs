module Ballerina.Data.Tests.Delta.Tuple

open NUnit.Framework
open System
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Delta.ToUpdater
open Ballerina.Data.Delta.Model
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Types.Patterns

let deltaExt (_ext: unit) : Value<TypeValue<Unit>, Unit> -> Sum<Value<TypeValue<Unit>, Unit>, 'a> =
  fun (v: Value<TypeValue<Unit>, Unit>) -> sum.Return v

let symbol name : TypeSymbol =
  { Name = name
    Guid = Guid.CreateVersion7() }

[<Test>]
let ``Delta.Tuple: Updates correct index in a tuple`` () =
  let tupleValue =
    [ PrimitiveValue.Int32 42 |> Value<TypeValue<Unit>, Unit>.Primitive
      PrimitiveValue.String "hello" |> Value<TypeValue<Unit>, Unit>.Primitive ]
    |> Value<TypeValue<Unit>, Unit>.Tuple

  let delta =
    Delta.Tuple(0, Delta.Replace(PrimitiveValue.Int32 99 |> Value<TypeValue<Unit>, Unit>.Primitive))

  match Delta.ToUpdater deltaExt delta with
  | Sum.Left updater ->
    match updater tupleValue with
    | Sum.Left(Value.Tuple [ updated; second ]) ->
      Assert.That(updated, Is.EqualTo(PrimitiveValue.Int32 99 |> Value<TypeValue<Unit>, Unit>.Primitive))
      Assert.That(second, Is.EqualTo(PrimitiveValue.String "hello" |> Value<TypeValue<Unit>, Unit>.Primitive))
    | _ -> Assert.Fail "Unexpected shape of updated tuple"
  | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"

[<Test>]
let ``Delta.Tuple: Fails if index out of bounds in value`` () =
  let tupleValue =
    [ PrimitiveValue.Int32 1 |> Value<TypeValue<Unit>, Unit>.Primitive ]
    |> Value<TypeValue<Unit>, Unit>.Tuple

  let delta =
    Delta.Tuple(1, Delta.Replace(PrimitiveValue.String "changed" |> Value<TypeValue<Unit>, Unit>.Primitive))

  match Delta.ToUpdater deltaExt delta with
  | Sum.Left updater ->
    match updater tupleValue with
    | Sum.Right _ -> Assert.Pass()
    | _ -> Assert.Fail "Expected error when tuple value is missing index"
  | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
