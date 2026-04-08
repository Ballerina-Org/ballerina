module Ballerina.Tests.State.StacklessWithError

open NUnit.Framework
open Ballerina.Stackless.State.WithError.StacklessStateWithError
open Ballerina.Collections.Sum

type TestContext = unit
type TestValue = int
type TestState = string
type TestError = string

type TestFreeNode<'a> = FreeNode<'a, TestContext, TestState, TestError>

let setState (f: TestState -> TestState) = TestFreeNode<unit>.setState f

module LogicTests =
  module Bind =
    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Bind should chain computations successfully`` () =
      let m1 = FreeNode.Return 5
      let m2 = FreeNode.bind m1 (fun x -> FreeNode.Return(x * 2))
      let result = FreeNode.run () "" m2

      match result with
      | Left(value, _) -> Assert.That(unbox<int> value, Is.EqualTo 10)
      | Right _ -> Assert.Fail "Expected success"

    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Bind should propagate state changes`` () =
      let m1 = setState (fun s -> s + "a")

      let m2 = FreeNode.bind m1 (fun _ -> setState (fun s -> s + "b"))

      let result = FreeNode.run () "" m2

      match result with
      | Left(_, state) ->
        match state with
        | Some s -> Assert.That(s, Is.EqualTo "ab")
        | None -> Assert.Fail "Expected state update"
      | Right _ -> Assert.Fail "Expected success"

    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Bind should stop on error`` () =
      let m1 = FreeNode.Return 5
      let m2 = FreeNode.bind m1 (fun _ -> FreeNode.throw "error")
      let m3 = FreeNode.bind m2 (fun x -> FreeNode.Return(x * 2))
      let result = FreeNode.run () "" m3

      match result with
      | Left _ -> Assert.Fail "Expected error"
      | Right(error, _) -> Assert.That(error, Is.EqualTo "error")

    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Bind should preserve state on error`` () =
      let m1 = setState (fun s -> s + "a")

      let m2 = FreeNode.bind m1 (fun _ -> FreeNode.throw "error")
      let result = FreeNode.run () "" m2

      match result with
      | Left _ -> Assert.Fail "Expected error"
      | Right(error, state) ->
        Assert.That(error, Is.EqualTo "error")

        match state with
        | Some s -> Assert.That(s, Is.EqualTo "a")
        | None -> ()

  module All =
    [<Test>]
    [<Category("StacklessWithError")>]
    let ``All should return all values when all succeed`` () =
      let ps = [ FreeNode.Return 1; FreeNode.Return 2; FreeNode.Return 3 ]
      let m = FreeNode.all ps
      let result = FreeNode.run () "" m

      match result with
      | Left(value, _) ->
        let values = unbox<List<int>> value
        Assert.That(values, Is.EqualTo<int list> [ 1; 2; 3 ])
      | Right _ -> Assert.Fail "Expected success"

    [<Test>]
    [<Category("StacklessWithError")>]
    let ``All should fail on first error`` () =
      let ps = [ FreeNode.Return 1; FreeNode.throw "error"; FreeNode.Return 3 ]
      let m = FreeNode.all ps
      let result = FreeNode.run () "" m

      match result with
      | Left _ -> Assert.Fail "Expected error"
      | Right(error, _) -> Assert.That(error, Is.EqualTo "error")

    [<Test>]
    [<Category("StacklessWithError")>]
    let ``All should accumulate state from all computations`` () =
      let ps =
        [ FreeNode.bind (setState (fun s -> s + "1")) (fun () -> FreeNode.Return 1)
          FreeNode.bind (setState (fun s -> s + "2")) (fun () -> FreeNode.Return 2)
          FreeNode.bind (setState (fun s -> s + "3")) (fun () -> FreeNode.Return 3) ]

      let m = FreeNode.all ps
      let result = FreeNode.run () "" m

      match result with
      | Left(value, state) ->
        let values = unbox<List<int>> value
        Assert.That(values, Is.EqualTo<int list> [ 1; 2; 3 ])

        match state with
        | Some s -> Assert.That(s, Is.EqualTo "123")
        | None -> Assert.Fail "Expected state update"
      | Right _ -> Assert.Fail "Expected success"

  module Any =
    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Any should return first successful value`` () =
      let ps = [ FreeNode.throw "error1"; FreeNode.Return 42; FreeNode.throw "error2" ]
      let concat = fun (e1: string, e2: string) -> e1 + ";" + e2
      let m = FreeNode.any concat ps
      let result = FreeNode.run () "" m

      match result with
      | Left(value, _) -> Assert.That(unbox<int> value, Is.EqualTo 42)
      | Right _ -> Assert.Fail "Expected success"

    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Any should combine errors when all fail`` () =
      let ps =
        [ FreeNode.throw "error1"; FreeNode.throw "error2"; FreeNode.throw "error3" ]

      let concat = fun (e1: string, e2: string) -> e1 + ";" + e2
      let m = FreeNode.any concat ps
      let result = FreeNode.run () "" m

      match result with
      | Left _ -> Assert.Fail "Expected error"
      | Right(error, _) -> Assert.That(error, Is.EqualTo "error1;error2;error3")

    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Any should use state from first successful computation`` () =
      let ps =
        [ FreeNode.bind (setState (fun s -> s + "1")) (fun () -> FreeNode.throw "error1")
          FreeNode.bind (setState (fun s -> s + "2")) (fun () -> FreeNode.Return 42)
          FreeNode.bind (setState (fun s -> s + "3")) (fun () -> FreeNode.Return 43) ]

      let concat = fun (e1: string, e2: string) -> e1 + ";" + e2
      let m = FreeNode.any concat ps
      let result = FreeNode.run () "" m

      match result with
      | Left(value, state) ->
        Assert.That(unbox<int> value, Is.EqualTo 42)

        match state with
        | Some s -> Assert.That(s, Is.EqualTo "2")
        | None -> Assert.Fail "Expected state update"
      | Right _ -> Assert.Fail "Expected success"

    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Catch within any should preserve state correctly`` () =
      let ps =
        [ FreeNode.bind (setState (fun s -> s + "p1")) (fun () ->
            FreeNode.bind
              (FreeNode.catch (FreeNode.bind (setState (fun s -> s + "p1-inner")) (fun () -> FreeNode.throw "p1-err")))
              (fun res ->
                match res with
                | Left _ -> FreeNode.throw "p1-should-fail"
                | Right e -> FreeNode.throw e))
          FreeNode.bind (setState (fun s -> s + "p2")) (fun () ->
            FreeNode.bind (setState (fun s -> s + "p2-step2")) (fun () -> FreeNode.Return 42)) ]

      let concat = fun (e1: string, e2: string) -> e1 + ";" + e2
      let m = FreeNode.any concat ps
      let result = FreeNode.run () "init" m

      match result with
      | Left(value, state) ->
        Assert.That(unbox<int> value, Is.EqualTo 42)

        match state with
        | Some s -> Assert.That(s, Is.EqualTo "initp2p2-step2")
        | None -> Assert.Fail "Expected state update"
      | Right(error, Some s) -> Assert.Fail $"Catch within any should succeed, got error '{error}', state={s}"
      | Right(error, None) -> Assert.Fail $"Catch within any should succeed, got error '{error}' without state"

  module Catch =
    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Catch should not leak beyond its scope`` () =
      // One nasty thing that might go wrong -> not correctly cleaning up exception handlers
      // this test makes sure that an earlier catch handler doesn't leak/linger beyond its scope
      let m1 = FreeNode.catch (FreeNode.Return 1)

      let m2 =
        FreeNode.bind m1 (fun res ->
          match res with
          | Left _ -> FreeNode.throw "boom" // should escape and fail the whole program
          | Right _ -> FreeNode.Return 42) // should never be hit

      let result = FreeNode.run () "" m2

      match result with
      | Right(err, _) -> Assert.That(err, Is.EqualTo "boom")
      | Left v -> Assert.Fail $"Expected failure, got success with value: {v}"

    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Catch must catch errors in its scope`` () =
      let m =
        FreeNode.catch (FreeNode.bind (FreeNode.Return 1) (fun _ -> FreeNode.throw "boom"))

      let result = FreeNode.run () "" m

      match result with
      | Left(value, _) ->
        match unbox<Sum<obj, string>> value with
        | Right err -> Assert.That(err, Is.EqualTo "boom")
        | Left _ -> Assert.Fail "Expected inner failure to be caught"
      | Right _ -> Assert.Fail "Expected success: catch should convert inner failure into a value"

module PerformanceTests =

  module StackDepth =
    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Bind should handle deep nesting without stack overflow`` () =
      let depth = 10000

      let m =
        [ 1..depth ]
        |> List.fold (fun acc _ -> FreeNode.bind acc (fun x -> FreeNode.Return(x + 1))) (FreeNode.Return 0)

      let result = FreeNode.run () "" m

      match result with
      | Left(_, _) -> Assert.Pass()
      | Right _ -> Assert.Fail "Expected success"

    [<Test>]
    [<Category("StacklessWithError")>]
    let ``All should handle many computations without stack overflow`` () =
      let count = 10000

      let m =
        [ 1..count ]
        |> List.fold
          (fun acc i ->
            let ps =
              [ FreeNode.bind (setState (fun s -> s + string i)) (fun () -> FreeNode.Return i)
                FreeNode.bind (setState (fun s -> s + "-" + string i)) (fun () -> FreeNode.Return(i + 1)) ]

            FreeNode.bind acc (fun accValue ->
              FreeNode.bind (FreeNode.all ps) (fun results -> FreeNode.Return(accValue + List.sum results))))
          (FreeNode.Return 0)

      let result = FreeNode.run () "" m

      match result with
      | Left(_, _) -> Assert.Pass()
      | Right _ -> Assert.Fail "Expected success"

    [<Test>]
    [<Category("StacklessWithError")>]
    let ``Any should handle many computations without stack overflow`` () =
      let count = 10000
      let concat = fun (e1: string, e2: string) -> e1 + ";" + e2

      let m =
        [ 1..count ]
        |> List.fold
          (fun acc i ->
            let ps =
              [ FreeNode.bind (setState (fun s -> s + string i)) (fun () -> FreeNode.throw ("error" + string i))
                FreeNode.bind (setState (fun s -> s + "-" + string i)) (fun () -> FreeNode.Return i) ]

            FreeNode.bind acc (fun accValue ->
              FreeNode.bind (FreeNode.any concat ps) (fun result -> FreeNode.Return(accValue + result))))
          (FreeNode.Return 0)

      let result = FreeNode.run () "" m

      match result with
      | Left(_, _) -> Assert.Pass()
      | Right _ -> Assert.Fail "Expected success"
