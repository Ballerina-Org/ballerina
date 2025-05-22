module Ballerina.Core.Tests.State

open Ballerina.State.WithError
open Ballerina.Collections.Sum
open Ballerina.Errors
open NUnit.Framework

type private InnerState = { age: int }

type private OuterState = { inner: InnerState; name: string }


[<Test>]
let ``Should correctly map state`` () =
  let withInnerState: State<int, unit, InnerState, Errors> =
    State.State(fun (_, s) -> Sum.Left(1, Some { s with age = s.age + 1 }))

  let withOuterState: State<int, unit, OuterState, Errors> =
    withInnerState
    |> State.mapState (fun output -> output.inner) (fun inner outer -> { outer with inner = inner })

  let result = withOuterState.run ((), { inner = { age = 30 }; name = "John" })

  match result with
  | Sum.Left(res, state) ->
    Assert.That(res, Is.EqualTo 1)

    match state with
    | Some s -> Assert.That(s, Is.EqualTo { inner = { age = 31 }; name = "John" })
    | None -> Assert.Fail "Expected state, but got None"
  | Sum.Right(e, _) -> Assert.Fail $"Expected success, but got error {e.ToString()}"
