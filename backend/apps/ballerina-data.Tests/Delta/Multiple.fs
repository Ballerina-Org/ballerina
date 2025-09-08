module Ballerina.Data.Tests.Delta.Multiple

open NUnit.Framework
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Delta.Model
open Ballerina.Data.Delta.ToUpdater
open Ballerina.Collections.Sum

[<Test>]
let ``Delta.Multiple: applies multiple replace deltas sequentially`` () =
  let t = TypeValue.Primitive PrimitiveType.Int32
  let v0 = Value<Unit>.Primitive(PrimitiveValue.Int 1)
  let v1 = Value<Unit>.Primitive(PrimitiveValue.Int 2)
  let v2 = Value<Unit>.Primitive(PrimitiveValue.Int 3)

  let delta = Delta.Multiple([ Delta.Replace(v1); Delta.Replace(v2) ])

  match Delta.ToUpdater t delta with
  | Sum.Left updater ->
    match updater v0 with
    | Sum.Left result -> Assert.That(result, Is.EqualTo v2)
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
  | Sum.Right err -> Assert.Fail $"ToUpdater failed: {err}"

[<Test>]
let ``Delta.Multiple Identity: empty delta list returns original value`` () =
  let t = TypeValue.Primitive PrimitiveType.String
  let v = Value<Unit>.Primitive(PrimitiveValue.String "keep me")

  let delta = Delta.Multiple([])

  match Delta.ToUpdater t delta with
  | Sum.Left updater ->
    match updater v with
    | Sum.Left result -> Assert.That(result, Is.EqualTo v)
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
  | Sum.Right err -> Assert.Fail $"ToUpdater failed: {err}"
