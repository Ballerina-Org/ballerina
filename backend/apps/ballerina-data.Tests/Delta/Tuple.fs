module Ballerina.Data.Tests.Delta.Tuple

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
let ``Delta.Tuple: Updates correct index in a tuple`` () =
  let tupleType =
    [ TypeValue.Primitive PrimitiveType.Int32
      TypeValue.Primitive PrimitiveType.String ]
    |> TypeValue.Tuple

  let tupleValue =
    [ PrimitiveValue.Int32 42 |> Value<Unit>.Primitive
      PrimitiveValue.String "hello" |> Value<Unit>.Primitive ]
    |> Value<Unit>.Tuple

  let delta =
    Delta.Tuple(0, Delta.Replace(PrimitiveValue.Int32 99 |> Value<Unit>.Primitive))

  match Delta.ToUpdater tupleType delta with
  | Sum.Left updater ->
    match updater tupleValue with
    | Sum.Left(Value.Tuple [ updated; second ]) ->
      Assert.That(updated, Is.EqualTo(PrimitiveValue.Int32 99 |> Value<Unit>.Primitive))
      Assert.That(second, Is.EqualTo(PrimitiveValue.String "hello" |> Value<Unit>.Primitive))
    | _ -> Assert.Fail "Unexpected shape of updated tuple"
  | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"

[<Test>]
let ``Delta.Tuple: Fails if index out of bounds in type`` () =
  let tupleType = [ TypeValue.Primitive PrimitiveType.Int32 ] |> TypeValue.Tuple

  let delta =
    Delta.Tuple(5, Delta.Replace(PrimitiveValue.Int32 1 |> Value<Unit>.Primitive))

  match Delta.ToUpdater tupleType delta with
  | Sum.Left _ -> Assert.Fail "Expected error due to out-of-range index"
  | Sum.Right _ -> Assert.Pass()

[<Test>]
let ``Delta.Tuple: Fails if index out of bounds in value`` () =
  let tupleType =
    [ TypeValue.Primitive PrimitiveType.Int32
      TypeValue.Primitive PrimitiveType.String ]
    |> TypeValue.Tuple

  let tupleValue =
    [ PrimitiveValue.Int32 1 |> Value<Unit>.Primitive ] |> Value<Unit>.Tuple

  let delta =
    Delta.Tuple(1, Delta.Replace(PrimitiveValue.String "changed" |> Value<Unit>.Primitive))

  match Delta.ToUpdater tupleType delta with
  | Sum.Left updater ->
    match updater tupleValue with
    | Sum.Right _ -> Assert.Pass()
    | _ -> Assert.Fail "Expected error when tuple value is missing index"
  | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
