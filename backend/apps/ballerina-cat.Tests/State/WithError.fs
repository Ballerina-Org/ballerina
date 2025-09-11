module Ballerina.Tests.State.WithError

open NUnit.Framework
open Ballerina.State.WithError
open Ballerina.Collections.Sum
open Ballerina.Collections.NonEmptyList

type Error = { Message: string }

type Errors =
  { Errors: NonEmptyList<Error> }

  static member Concat(e1: Errors, e2: Errors) : Errors =
    { Errors = NonEmptyList.OfList(e1.Errors.Head, e1.Errors.Tail @ (e2.Errors |> NonEmptyList.ToList)) }

  static member Singleton message =
    { Errors = NonEmptyList.One { Message = message } }

module Any =

  [<Test>]
  let ``Test Any should return first successful result`` () =
    let failingParser1 = state.Throw(Errors.Singleton "Error1")
    let successfulParser = state.Return 42
    let failingParser2 = state.Throw(Errors.Singleton "Error2")

    let parsers =
      NonEmptyList.OfList(failingParser1, [ successfulParser; failingParser2 ])


    let result = (state { return! parsers |> state.Any }).run ((), ())

    match result with
    | Sum.Left(value, _) -> Assert.That(value, Is.EqualTo 42)
    | Sum.Right _ -> Assert.Fail "Expected successful result"

  [<Test>]
  let ``Test Any should return first successful result when first parser succeeds`` () =
    let successfulParser = state.Return 42
    let failingParser1 = state.Throw(Errors.Singleton "Error1")
    let failingParser2 = state.Throw(Errors.Singleton "Error2")

    let parsers =
      NonEmptyList.OfList(successfulParser, [ failingParser1; failingParser2 ])

    let result = (state { return! parsers |> state.Any }).run ((), ())

    match result with
    | Sum.Left(value, _) -> Assert.That(value, Is.EqualTo 42)
    | Sum.Right _ -> Assert.Fail "Expected successful result"

  [<Test>]
  let ``Test Any should return last successful result`` () =
    let failingParser1 = state.Throw(Errors.Singleton "Error1")
    let failingParser2 = state.Throw(Errors.Singleton "Error2")
    let successfulParser = state.Return 42

    let parsers =
      NonEmptyList.OfList(failingParser1, [ failingParser2; successfulParser ])

    let result = (state { return! parsers |> state.Any }).run ((), ())

    match result with
    | Sum.Left(value, _) -> Assert.That(value, Is.EqualTo 42)
    | Sum.Right _ -> Assert.Fail "Expected successful result"

  [<Test>]
  let ``Test Any should throw single error when only one parser`` () =
    let failingParser = state.Throw(Errors.Singleton "SingleError")

    let parsers = NonEmptyList.OfList(failingParser, [])

    let result = (state { return! parsers |> state.Any }).run ((), ())

    match result with
    | Sum.Left _ -> Assert.Fail "Expected error"
    | Sum.Right(error, _) ->
      match error.Errors |> NonEmptyList.ToList with
      | [ singleError ] -> Assert.That(singleError.Message, Is.EqualTo "SingleError")
      | _ -> Assert.Fail "Expected single error"

  [<Test>]
  let ``Test Any should combine errors when all parsers fail`` () =
    let failingParser1 = state.Throw(Errors.Singleton "Error1")
    let failingParser2 = state.Throw(Errors.Singleton "Error2")
    let failingParser3 = state.Throw(Errors.Singleton "Error3")

    let parsers =
      NonEmptyList.OfList(failingParser1, [ failingParser2; failingParser3 ])

    let result = (state { return! parsers |> state.Any }).run ((), ())

    match result with
    | Sum.Left _ -> Assert.Fail "Expected error"
    | Sum.Right(error, _) ->
      let errorMessages =
        error.Errors |> NonEmptyList.ToList |> List.map (fun e -> e.Message)

      Assert.That(errorMessages, Is.EquivalentTo [ "Error1"; "Error2"; "Error3" ])

  [<Test>]
  let ``Test Any should return first successful result in case of multiple successful parsers`` () =
    let successfulParser1 = state.Return 42
    let successfulParser2 = state.Return 43
    let failingParser = state.Throw(Errors.Singleton "Error")

    let parsers =
      NonEmptyList.OfList(successfulParser1, [ successfulParser2; failingParser ])

    let result = (state { return! parsers |> state.Any }).run ((), ())

    match result with
    | Sum.Left(value, _) -> Assert.That(value, Is.EqualTo 42)
    | Sum.Right _ -> Assert.Fail "Expected successful result"

  [<Test>]
  let ``Any should only take state change from first successful parser`` () =
    let failingParser1 =
      state {
        let! _ = state.SetState((*) 2) // Use primes
        return! state.Throw(Errors.Singleton "Error1")
      }

    let failingParser2 =
      state {
        let! _ = state.SetState((*) 3)
        return! state.Throw(Errors.Singleton "Error2")
      }

    let successfulParser =
      state {
        let! _ = state.SetState((*) 5)
        42
      }

    let successfulParser2 =
      state {
        let! _ = state.SetState((*) 7)
        43
      }

    let parsers =
      NonEmptyList.OfList(failingParser1, [ failingParser2; successfulParser; successfulParser2 ])

    let result = (state { return! parsers |> state.Any }).run ((), 1)

    match result with
    | Sum.Left(value, s) ->
      Assert.That(value, Is.EqualTo 42)
      Assert.That(s, Is.EqualTo(Some 5))
    | Sum.Right _ -> Assert.Fail "Expected successful result"
