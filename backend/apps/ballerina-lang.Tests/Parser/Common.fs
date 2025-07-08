module Ballerina.Cat.Tests.BusinessRuleEngine.Parser.Common

open Ballerina.Collections.Sum
open Ballerina.DSL.Parser.Patterns
open NUnit.Framework
open Ballerina.Errors

let assertSuccess<'T, 'E> (result: Sum<'T, 'E>) (expected: 'T) =
  match result with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail($"Expected success but got error: {err}")

let contextActions: ContextOperations<unit> =
  { TryFindType = fun _ _ -> sum.Throw(Errors.Singleton "Type lookup not expected") }
