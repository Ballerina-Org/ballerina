module Ballerina.Cat.Tests.State

open Ballerina.State.WithError
open Ballerina.Collections.Sum
open Ballerina.Errors
open NUnit.Framework

type private InnerState =
  { age: int }

  static member Updaters = {| Age = fun u s -> { s with age = u s.age } |}

type private OuterState =
  { inner: InnerState
    name: string }

  static member Getters =
    {| Inner = fun s -> s.inner
       Name = fun s -> s.name |}

  static member Replacers =
    {| Inner = fun v s -> { s with inner = v }
       Name = fun v s -> { s with name = v } |}


[<Test>]
let ``Should correctly map state`` () =
  let value = "some value"

  let withInnerState: State<string, unit, InnerState, Errors> =
    state {
      do! state.SetState(InnerState.Updaters.Age((+) 1))
      value
    }

  let withOuterState: State<string, unit, OuterState, Errors> =
    withInnerState
    |> State.mapState OuterState.Getters.Inner OuterState.Replacers.Inner

  let result = withOuterState.run ((), { inner = { age = 30 }; name = "John" })

  match result with
  | Sum.Left(res, state) ->
    Assert.That(res, Is.EqualTo value)

    match state with
    | Some s -> Assert.That(s, Is.EqualTo { inner = { age = 31 }; name = "John" })
    | None -> Assert.Fail "Expected state, but got None"
  | Sum.Right(e, _) -> Assert.Fail $"Expected success, but got error {e.ToString()}"
