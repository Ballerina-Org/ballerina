namespace Ballerina.StdLib.Tests.Core

open NUnit.Framework
open Ballerina.StdLib.OrderPreservingMap
open Ballerina.Collections.Sum
open Ballerina.Errors

module OrderedMap =

  [<Test>]
  let ``Empty map should have no keys or values`` () =
    let empty = OrderedMap<int, string>.empty
    Assert.That(OrderedMap.IsEmpty empty, Is.True)
    Assert.That(OrderedMap.Count empty, Is.EqualTo(0))
    Assert.That(OrderedMap.Keys empty, Is.EqualTo<int list>([]: int list))
    Assert.That(OrderedMap.Values empty, Is.EqualTo<string list>([]: string list))
    Assert.That(OrderedMap.toList empty, Is.EqualTo<(int * string) list>([]: (int * string) list))

  [<Test>]
  let ``AddKeyExistsOk should add new key`` () =
    let empty = OrderedMap<int, string>.empty
    let result = OrderedMap.Add empty 1 "hello"
    Assert.That(OrderedMap.IsEmpty result, Is.False)
    Assert.That(OrderedMap.Count result, Is.EqualTo(1))
    Assert.That(OrderedMap.ContainsKey result 1, Is.True)
    Assert.That(OrderedMap.TryFind result 1, Is.EqualTo(Some "hello"))

  [<Test>]
  let ``AddKeyExistsOk should overwrite existing key`` () =
    let empty = OrderedMap<int, string>.empty
    let withFirst = OrderedMap.Add empty 1 "hello"
    let withSecond = OrderedMap.Add withFirst 1 "world"
    Assert.That(OrderedMap.Count withSecond, Is.EqualTo(1))
    Assert.That(OrderedMap.TryFind withSecond 1, Is.EqualTo(Some "world"))

  [<Test>]
  let ``AddKey should succeed for new key`` () =
    let empty = OrderedMap<int, string>.empty
    let result = OrderedMap.AddIfNotExists empty 1 "hello"

    match result with
    | Some om ->
      Assert.That(OrderedMap.IsEmpty om, Is.False)
      Assert.That(OrderedMap.Count om, Is.EqualTo(1))
      Assert.That(OrderedMap.TryFind om 1, Is.EqualTo(Some "hello"))
    | None -> Assert.Fail "Should have succeeded"

  [<Test>]
  let ``AddKey should fail for existing key`` () =
    let empty = OrderedMap<int, string>.empty
    let withFirst = OrderedMap.Add empty 1 "hello"
    let result = OrderedMap.AddIfNotExists withFirst 1 "world"

    match result with
    | None -> Assert.Pass()
    | Some _ -> Assert.Fail "Should have failed"

  [<Test>]
  let ``Keys should preserve insertion order`` () =
    let empty = OrderedMap<int, string>.empty
    let withFirst = OrderedMap.Add empty 1 "first"
    let withSecond = OrderedMap.Add withFirst 2 "second"
    let withThird = OrderedMap.Add withSecond 3 "third"
    let keys = OrderedMap.Keys withThird
    let values = OrderedMap.Values withThird
    Assert.That(keys, Is.EqualTo<int list>([ 1; 2; 3 ]))
    Assert.That(values, Is.EqualTo<string list>([ "first"; "second"; "third" ]))

  [<Test>]
  let ``ToList should preserve insertion order`` () =
    let empty = OrderedMap<int, string>.empty
    let withFirst = OrderedMap.Add empty 1 "first"
    let withSecond = OrderedMap.Add withFirst 2 "second"
    let withThird = OrderedMap.Add withSecond 3 "third"
    let list = OrderedMap.toList withThird
    Assert.That(list, Is.EqualTo<(int * string) list>([ (1, "first"); (2, "second"); (3, "third") ]))

  [<Test>]
  let ``Map should transform values and preserve insertion order`` () =
    let empty = OrderedMap<int, int>.empty
    let withValues = OrderedMap.Add empty 1 2
    let withMore = OrderedMap.Add withValues 2 3
    let doubled = OrderedMap.map (fun _ v -> v * 2) withMore
    let keys = OrderedMap.Keys doubled
    let values = OrderedMap.Values doubled
    Assert.That(keys, Is.EqualTo<int list>([ 1; 2 ]))
    Assert.That(values, Is.EqualTo<int list>([ 4; 6 ]))

  [<Test>]
  let ``OfListDuplicatesOk should create map from list`` () =
    let input = [ (1, "first"); (2, "second"); (3, "third") ]
    let result = OrderedMap.ofList input
    Assert.That(OrderedMap.Count result, Is.EqualTo(3))
    Assert.That(OrderedMap.Keys result, Is.EqualTo<int list>([ 1; 2; 3 ]))
    Assert.That(OrderedMap.Values result, Is.EqualTo<string list>([ "first"; "second"; "third" ]))

  [<Test>]
  let ``OfListDuplicatesOk should handle duplicates by overwriting`` () =
    let input = [ (1, "first"); (1, "second"); (2, "third") ]
    let result = OrderedMap.ofList input
    Assert.That(OrderedMap.Count result, Is.EqualTo(2))
    Assert.That(OrderedMap.Keys result, Is.EqualTo<int list>([ 1; 2 ]))
    Assert.That(OrderedMap.Values result, Is.EqualTo<string list>([ "second"; "third" ]))

  [<Test>]
  let ``OfList should succeed for unique keys`` () =
    let input = [ (1, "first"); (2, "second"); (3, "third") ]
    let result = OrderedMap.ofListIfNoDuplicates input

    match result with
    | Left om ->
      Assert.That(OrderedMap.Count om, Is.EqualTo(3))
      Assert.That(OrderedMap.Keys om, Is.EqualTo<int list>([ 1; 2; 3 ]))
      Assert.That(OrderedMap.Values om, Is.EqualTo<string list>([ "first"; "second"; "third" ]))
    | Right _ -> Assert.Fail("Should have succeeded")

  [<Test>]
  let ``OfList should fail for duplicate keys`` () =
    let input = [ (1, "first"); (1, "firstDup"); (2, "second"); (2, "secondDup") ]
    let result = OrderedMap.ofListIfNoDuplicates input

    match result with
    | Left _ -> Assert.Fail("Should have failed")
    | Right errors -> Assert.That(errors.Errors.Head.Message.Contains("Duplicate keys: [1; 2]"), Is.True)

  [<Test>]
  let ``ContainsKey should work correctly`` () =
    let empty = OrderedMap<int, string>.empty
    let withKey = OrderedMap.Add empty 1 "hello"
    Assert.That(OrderedMap.ContainsKey withKey 1, Is.True)
    Assert.That(OrderedMap.ContainsKey withKey 2, Is.False)

  [<Test>]
  let ``TryFind should return correct values`` () =
    let empty = OrderedMap<int, string>.empty
    let withKey = OrderedMap.Add empty 1 "hello"
    Assert.That(OrderedMap.TryFind withKey 1, Is.EqualTo(Some "hello"))
    Assert.That(OrderedMap.TryFind withKey 2, Is.EqualTo(None))

  [<Test>]
  let ``Complex scenario with multiple operations`` () =
    let empty = OrderedMap<int, string>.empty
    let step1 = OrderedMap.Add empty 1 "one"
    let step2 = OrderedMap.Add step1 2 "two"
    let step3 = OrderedMap.Add step2 3 "three"
    let step4 = OrderedMap.Add step3 1 "ONE" // overwrite
    let step5 = OrderedMap.map (fun _ (v: string) -> v.ToUpper()) step4

    Assert.That(OrderedMap.Count step5, Is.EqualTo(3))
    Assert.That(OrderedMap.Keys step5, Is.EqualTo<int list>([ 1; 2; 3 ]))
    Assert.That(OrderedMap.Values step5, Is.EqualTo<string list>([ "ONE"; "TWO"; "THREE" ]))
    Assert.That(OrderedMap.toList step5, Is.EqualTo<(int * string) list>([ (1, "ONE"); (2, "TWO"); (3, "THREE") ]))

  [<Test>]
  let ``mergeSecondAfterFirst should succeed for non-conflicting maps`` () =
    let map1 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"

    let map2 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 3 "third") 4 "fourth"

    let result = OrderedMap.mergeSecondAfterFirstIfNoDuplicates map1 map2

    match result with
    | Left merged ->
      Assert.That(OrderedMap.Count merged, Is.EqualTo(4))
      Assert.That(OrderedMap.Keys merged, Is.EqualTo<int list>([ 1; 2; 3; 4 ])) // om2 keys first, then om1 keys
      Assert.That(OrderedMap.Values merged, Is.EqualTo<string list>([ "first"; "second"; "third"; "fourth" ]))
    | Right _ -> Assert.Fail("Should have succeeded")

  [<Test>]
  let ``mergeSecondAfterFirst should fail for conflicting keys`` () =
    let map1 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"

    let map2 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 2 "conflict") 3 "third"

    let result = OrderedMap.mergeSecondAfterFirstIfNoDuplicates map1 map2

    match result with
    | Left _ -> Assert.Fail("Should have failed")
    | Right errors ->
      Assert.That(errors.Errors.Head.Message.Contains("Key conflicts during merge"), Is.True)
      Assert.That(errors.Errors.Head.Message.Contains("2"), Is.True)

  [<Test>]
  let ``mergeSecondAfterFirstDuplicatesOk should merge with om1 values taking precedence`` () =
    let map1 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"

    let map2 =
      OrderedMap.Add (OrderedMap.Add OrderedMap.empty 2 "overwritten") 3 "third"

    let result = OrderedMap.mergeSecondAfterFirst map1 map2
    Assert.That(OrderedMap.Count result, Is.EqualTo(3))
    Assert.That(OrderedMap.Keys result, Is.EqualTo<int list>([ 1; 2; 3 ])) // om2 non-conflicting keys first, then om1 keys
    Assert.That(OrderedMap.Values result, Is.EqualTo<string list>([ "first"; "second"; "third" ]))
    Assert.That(OrderedMap.TryFind result 2, Is.EqualTo(Some "second")) // om2 value takes precedence

  [<Test>]
  let ``mergeSecondAfterFirstDuplicatesOk should handle empty first map`` () =
    let map1 = OrderedMap.empty

    let map2 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"

    let result = OrderedMap.mergeSecondAfterFirst map1 map2
    Assert.That(OrderedMap.Count result, Is.EqualTo(2))
    Assert.That(OrderedMap.Keys result, Is.EqualTo<int list>([ 1; 2 ]))
    Assert.That(OrderedMap.Values result, Is.EqualTo<string list>([ "first"; "second" ]))

  [<Test>]
  let ``mergeSecondAfterFirstDuplicatesOk should handle empty second map`` () =
    let map1 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"

    let map2 = OrderedMap.empty
    let result = OrderedMap.mergeSecondAfterFirst map1 map2
    Assert.That(OrderedMap.Count result, Is.EqualTo(2))
    Assert.That(OrderedMap.Keys result, Is.EqualTo<int list>([ 1; 2 ]))
    Assert.That(OrderedMap.Values result, Is.EqualTo<string list>([ "first"; "second" ]))

  [<Test>]
  let ``mergeSecondAfterFirstDuplicatesOk should handle both maps empty`` () =
    let map1 = OrderedMap.empty
    let map2 = OrderedMap.empty
    let result = OrderedMap.mergeSecondAfterFirst map1 map2
    Assert.That(OrderedMap.Count result, Is.EqualTo(0))
