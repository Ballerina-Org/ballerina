module Ballerina.Tests.Collections.Sum

open NUnit.Framework
open Ballerina.Collections.Sum
open Ballerina.Collections.NonEmptyList

type Error = { Message: string }

type Errors =
  { Errors: NonEmptyList<Error> }

  static member Concat(e1, e2) =
    { Errors = NonEmptyList.OfList(e1.Errors.Head, e1.Errors.Tail @ (e2.Errors |> NonEmptyList.ToList)) }

[<Test>]
let TestAllShouldCollectAllErrors () =
  let dataWithError: List<Sum<unit, Errors>> =
    [ Sum.Right({ Errors = NonEmptyList.One { Message = "Error1" } })
      Sum.Left()
      Sum.Right({ Errors = NonEmptyList.One { Message = "Error2" } }) ]

  let merged: Sum<List<unit>, Errors> = sum.All dataWithError

  match merged with
  | Sum.Left _ -> Assert.Fail "Expected Right"
  | Sum.Right errors ->
    match errors.Errors |> NonEmptyList.ToList with
    | [ error1; error2 ] ->
      Assert.That(error1.Message, Is.EqualTo "Error1")
      Assert.That(error2.Message, Is.EqualTo "Error2")
    | _ -> Assert.Fail "Expected two errors"

[<Test>]
let TestAllShouldCollectReturnValuesIfNoErrors () =
  let dataWithError: List<Sum<int, Errors>> = [ Sum.Left(1) ]

  let merged: Sum<List<int>, Errors> = sum.All dataWithError

  let expected: List<int> = [ 1 ]

  match merged with
  | Sum.Left values -> Assert.That(values, Is.EqualTo<int list> expected)
  | Sum.Right _ -> Assert.Fail "Expected Left"

[<Test>]
let TestAllNonEmptyShouldCollectAllErrors () =
  let dataWithError: NonEmptyList<Sum<unit, Errors>> =
    NonEmptyList.OfList(
      Sum.Right({ Errors = NonEmptyList.One { Message = "Error1" } }),
      [ Sum.Left(); Sum.Right({ Errors = NonEmptyList.One { Message = "Error2" } }) ]
    )

  let merged: Sum<NonEmptyList<unit>, Errors> = sum.AllNonEmpty dataWithError

  match merged with
  | Sum.Left _ -> Assert.Fail "Expected Right"
  | Sum.Right errors ->
    match errors.Errors |> NonEmptyList.ToList with
    | [ error1; error2 ] ->
      Assert.That(error1.Message, Is.EqualTo "Error1")
      Assert.That(error2.Message, Is.EqualTo "Error2")
    | _ -> Assert.Fail "Expected two errors"

[<Test>]
let TestAllNonEmptyShouldCollectSingleError () =
  let dataWithError: NonEmptyList<Sum<unit, Errors>> =
    NonEmptyList.OfList(Sum.Right({ Errors = NonEmptyList.One { Message = "Error1" } }), [ Sum.Left() ])

  let merged: Sum<NonEmptyList<unit>, Errors> = sum.AllNonEmpty dataWithError

  match merged with
  | Sum.Left _ -> Assert.Fail "Expected Right"
  | Sum.Right errors ->
    match errors.Errors |> NonEmptyList.ToList with
    | [ error1 ] -> Assert.That(error1.Message, Is.EqualTo "Error1")
    | _ -> Assert.Fail "Expected one error"

[<Test>]
let TestAllNonEmptyShouldCollectReturnValuesIfNoErrors () =
  let dataWithError: NonEmptyList<Sum<int, Errors>> =
    NonEmptyList.OfList(Sum.Left(1), [ Sum.Left(2) ])

  let merged: Sum<NonEmptyList<int>, Errors> = sum.AllNonEmpty dataWithError

  let expected: NonEmptyList<int> = NonEmptyList.OfList(1, [ 2 ])

  match merged with
  | Sum.Left values -> Assert.That(values, Is.EqualTo<NonEmptyList<int>> expected)
  | Sum.Right _ -> Assert.Fail "Expected Left"
