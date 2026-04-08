namespace Ballerina.StdLib.Tests.Algorithms

open NUnit.Framework
open Ballerina.StdLib.Algorithms
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.Errors

module TopologicalSort =

  [<Test>]
  let ``Should produce correct topological sort`` () =
    let graph =
      Map.ofList
        [ "A", Set.ofList [ "B"; "C" ]
          "B", Set.ofList [ "D" ]
          "C", Set.ofList [ "D" ]
          "D", Set.empty ]

    let result = TopologicalSort.sort graph

    match result with
    | Left sortedList -> Assert.That(sortedList, Is.EqualTo<string list> [ "D"; "C"; "B"; "A" ])
    | Right err -> Assert.Fail $"Expected successful sort, but got error: {err}"

  [<Test>]
  let ``Should fail if cycle`` () =
    let graph = Map.ofList [ "A", Set.ofList [ "B" ]; "B", Set.ofList [ "A" ] ]
    let result = TopologicalSort.sort graph

    match result with
    | Left _ -> Assert.Fail "Expected error due to cycle, but got successful result"
    | Right errors -> Assert.That((errors.Errors()).Head.Message, Does.Contain "Cycle detected")

  [<Test>]
  let ``Should fail if dependency node not actually in graph`` () =
    let graph = Map.ofList [ "A", Set.ofList [ "B"; "C" ]; "B", Set.ofList [ "D" ] ]
    let result = TopologicalSort.sort graph

    match result with
    | Left _ -> Assert.Fail "Expected error due to missing node, but got successful result"
    | Right errors -> Assert.That((errors.Errors()).Head.Message, Does.Contain "cannot find node")
