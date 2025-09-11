module Ballerina.Data.Tests.Delta.Replace

open NUnit.Framework
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Delta.Model
open Ballerina.Data.Delta.ToUpdater
open Ballerina.Collections.Sum

[<Test>]
let ``Delta.Replace: replaces primitive int value`` () =
  let t = TypeValue.Primitive PrimitiveType.Int32
  let original = Value<Unit>.Primitive(PrimitiveValue.Int 10)
  let replacement = Value<Unit>.Primitive(PrimitiveValue.Int 99)
  let delta = Delta.Replace(replacement)

  match Delta.ToUpdater t delta with
  | Sum.Left updater ->
    match updater original with
    | Sum.Left result -> Assert.That(result, Is.EqualTo replacement)
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
  | Sum.Right err -> Assert.Fail $"ToUpdater failed: {err}"

[<Test>]
let ``Delta.Replace: replaces string with anything (no validation)`` () =
  let t = TypeValue.Primitive PrimitiveType.String
  let original = Value<Unit>.Primitive(PrimitiveValue.String "abc")
  let replacement = Value<Unit>.Primitive(PrimitiveValue.Bool true)
  let delta = Delta.Replace(replacement)

  match Delta.ToUpdater t delta with
  | Sum.Left updater ->
    match updater original with
    | Sum.Left result -> Assert.That(result, Is.EqualTo replacement)
    | Sum.Right _ -> Assert.Fail "Unexpected error during replace"
  | Sum.Right _ -> Assert.Fail "Expected updater to be generated"
