module Ballerina.Data.Tests.Delta.Replace

open NUnit.Framework
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Delta.Model
open Ballerina.Data.Delta.ToUpdater
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Types.Patterns

let deltaExt (_ext: unit) : Value<TypeValue, Unit> -> Sum<Value<TypeValue, Unit>, 'a> =
  fun (v: Value<TypeValue, Unit>) -> sum.Return v

[<Test>]
let ``Delta.Replace: replaces primitive int value`` () =
  let original = Value<TypeValue, Unit>.Primitive(PrimitiveValue.Int32 10)
  let replacement = Value<TypeValue, Unit>.Primitive(PrimitiveValue.Int32 99)
  let delta = Delta.Replace(replacement)

  match Delta.ToUpdater deltaExt delta with
  | Sum.Left updater ->
    match updater original with
    | Sum.Left result -> Assert.That(result, Is.EqualTo replacement)
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
  | Sum.Right err -> Assert.Fail $"ToUpdater failed: {err}"

[<Test>]
let ``Delta.Replace: replaces string with anything (no validation)`` () =
  let original = Value<TypeValue, Unit>.Primitive(PrimitiveValue.String "abc")
  let replacement = Value<TypeValue, Unit>.Primitive(PrimitiveValue.Bool true)
  let delta = Delta.Replace(replacement)

  match Delta.ToUpdater deltaExt delta with
  | Sum.Left updater ->
    match updater original with
    | Sum.Left result -> Assert.That(result, Is.EqualTo replacement)
    | Sum.Right _ -> Assert.Fail "Unexpected error during replace"
  | Sum.Right _ -> Assert.Fail "Expected updater to be generated"
