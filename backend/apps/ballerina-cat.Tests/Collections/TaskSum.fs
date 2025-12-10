module Ballerina.Tests.Collections.TaskSum

open System.Threading.Tasks
open AutoFixture.NUnit4
open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.Collections.TaskSum

let private liftLeft: int -> Sum<int, string> Task = Task.FromResult << Left

let private liftRight: string -> Sum<int, string> Task = Task.FromResult << Right

let private leftShouldEqualTo (expected: 'a) (sum: Sum<'a, 'b>) =
  match sum with
  | Left actual -> Assert.That(expected, Is.EqualTo<'a> actual)
  | Right _ -> Assert.Fail()

let private rightShouldEqualTo (expected: 'b) (sum: Sum<'a, 'b>) =
  match sum with
  | Left _ -> Assert.Fail()
  | Right actual -> Assert.That(expected, Is.EqualTo<'b> actual)

[<Test; AutoData>]
let ``Expect return to lift a value`` (expected: int) =
  task {
    let! actual = taskSum { return expected }
    actual |> leftShouldEqualTo expected
  }

[<Test; AutoData>]
let ``Expect let! to bind a value`` expected modifier =
  task {
    let! actual =
      taskSum {
        let! value = liftLeft expected
        return value + modifier
      }

    actual |> leftShouldEqualTo (expected + modifier)
  }

[<Test; AutoData>]
let ``Expect let! to bind a sum`` (expected: int) =
  task {
    let! actual =
      taskSum {
        let! value = Left expected
        return value
      }

    actual |> leftShouldEqualTo expected
  }

[<Test; AutoData>]
let ``When sum is Right expect let! preserve the sum`` expected =
  task {
    let! actual =
      taskSum {
        let! value = liftRight expected
        return value
      }

    actual |> rightShouldEqualTo expected
  }

[<Test; AutoData>]
let ``When sum is Right expect let! ignore the rest of the computation`` expected =
  let mutable flag = true

  task {
    let! _ =
      taskSum {
        let! _ = liftRight expected
        flag <- false
        return ()
      }

    Assert.That(flag, Is.True)
  }

[<Test>]
let ``Expect do! to bind a task of unit`` () =
  task {
    let! actual = taskSum { do! Task.FromResult() }
    actual |> leftShouldEqualTo ()
  }

[<Test; AutoData>]
let ``Expect return! to return wrapped value`` expected =
  task {
    let! actual = taskSum { return! liftLeft expected }
    actual |> leftShouldEqualTo expected
  }

[<Test; AutoData>]
let ``Expect Lift to lift a generic task`` (expected: int) =
  task {
    let! actual = taskSum.Lift <| Task.FromResult expected
    actual |> leftShouldEqualTo expected
  }
