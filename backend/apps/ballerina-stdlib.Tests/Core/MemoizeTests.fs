module Ballerina.StdLib.Tests.Core.MemoizeTests

open AutoFixture.NUnit4
open NUnit.Framework
open Ballerina.StdLib

[<Test; AutoData>]
let ``Expect memoize to return result of a function`` expected =
  let sut = memoize id
  let actual = expected |> Seq.map sut
  Assert.That(actual, Is.EqualTo<int> expected)

[<Test; AutoData>]
let ``Expect memoize to call a function once per argument value`` (value: int) =
  let mutable timesCalled = 0

  let f x =
    timesCalled <- timesCalled + 1
    x

  let sut = memoize f
  ignore <| sut value
  ignore <| sut value
  Assert.That(timesCalled, Is.EqualTo 1)
