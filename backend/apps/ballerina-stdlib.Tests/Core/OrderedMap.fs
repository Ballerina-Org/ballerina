namespace Ballerina.StdLib.Tests.Core

open NUnit.Framework
open Ballerina.StdLib.OrderPreservingMap
open Ballerina.Cat.Collections.OrderedMap
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.Errors

module OrderedMap =

  [<Test>]
  let ``Empty map should have no keys or values`` () =
    let empty = OrderedMap<int, string>.empty
    Assert.That(OrderedMap.isEmpty empty, Is.True)
    Assert.That(OrderedMap.count empty, Is.EqualTo(0))
    Assert.That(OrderedMap.keys empty, Is.EqualTo<int list>([]: int list))
    Assert.That(OrderedMap.values empty, Is.EqualTo<string list>([]: string list))
    Assert.That(OrderedMap.toList empty, Is.EqualTo<(int * string) list>([]: (int * string) list))

  [<Test>]
  let ``AddKeyExistsOk should add new key`` () =
    let empty = OrderedMap<int, string>.empty
    let result = OrderedMap.Add empty 1 "hello"
    Assert.That(OrderedMap.isEmpty result, Is.False)
    Assert.That(OrderedMap.count result, Is.EqualTo(1))
    Assert.That(OrderedMap.containsKey result 1, Is.True)
    Assert.That(OrderedMap.tryFind 1 result, Is.EqualTo(Some "hello"))

  [<Test>]
  let ``AddKeyExistsOk should overwrite existing key`` () =
    let empty = OrderedMap<int, string>.empty
    let withFirst = OrderedMap.Add empty 1 "hello"
    let withSecond = OrderedMap.Add withFirst 1 "world"
    Assert.That(OrderedMap.count withSecond, Is.EqualTo(1))
    Assert.That(OrderedMap.tryFind 1 withSecond, Is.EqualTo(Some "world"))

  [<Test>]
  let ``AddKey should succeed for new key`` () =
    let empty = OrderedMap<int, string>.empty
    let result = OrderedMap.addIfNotExists empty 1 "hello"

    match result with
    | Some om ->
      Assert.That(OrderedMap.isEmpty om, Is.False)
      Assert.That(OrderedMap.count om, Is.EqualTo(1))
      Assert.That(OrderedMap.tryFind 1 om, Is.EqualTo(Some "hello"))
    | None -> Assert.Fail "Should have succeeded"

  [<Test>]
  let ``AddKey should fail for existing key`` () =
    let empty = OrderedMap<int, string>.empty
    let withFirst = OrderedMap.Add empty 1 "hello"
    let result = OrderedMap.addIfNotExists withFirst 1 "world"

    match result with
    | None -> Assert.Pass()
    | Some _ -> Assert.Fail "Should have failed"

  [<Test>]
  let ``Keys should preserve insertion order`` () =
    let empty = OrderedMap<int, string>.empty
    let withFirst = OrderedMap.Add empty 1 "first"
    let withSecond = OrderedMap.Add withFirst 2 "second"
    let withThird = OrderedMap.Add withSecond 3 "third"
    let keys = OrderedMap.keys withThird
    let values = OrderedMap.values withThird
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
    let keys = OrderedMap.keys doubled
    let values = OrderedMap.values doubled
    Assert.That(keys, Is.EqualTo<int list>([ 1; 2 ]))
    Assert.That(values, Is.EqualTo<int list>([ 4; 6 ]))

  [<Test>]
  let ``OfListDuplicatesOk should create map from list`` () =
    let input = [ (1, "first"); (2, "second"); (3, "third") ]
    let result = OrderedMap.ofList input
    Assert.That(OrderedMap.count result, Is.EqualTo(3))
    Assert.That(OrderedMap.keys result, Is.EqualTo<int list>([ 1; 2; 3 ]))
    Assert.That(OrderedMap.values result, Is.EqualTo<string list>([ "first"; "second"; "third" ]))

  [<Test>]
  let ``OfListDuplicatesOk should handle duplicates by overwriting`` () =
    let input = [ (1, "first"); (1, "second"); (2, "third") ]
    let result = OrderedMap.ofList input
    Assert.That(OrderedMap.count result, Is.EqualTo(2))
    Assert.That(OrderedMap.keys result, Is.EqualTo<int list>([ 1; 2 ]))
    Assert.That(OrderedMap.values result, Is.EqualTo<string list>([ "second"; "third" ]))

  [<Test>]
  let ``OfList should succeed for unique keys`` () =
    let input = [ (1, "first"); (2, "second"); (3, "third") ]
    let result = OrderedMap.ofListIfNoDuplicates input

    match result with
    | Left om ->
      Assert.That(OrderedMap.count om, Is.EqualTo(3))
      Assert.That(OrderedMap.keys om, Is.EqualTo<int list>([ 1; 2; 3 ]))
      Assert.That(OrderedMap.values om, Is.EqualTo<string list>([ "first"; "second"; "third" ]))
    | Right _ -> Assert.Fail("Should have succeeded")

  [<Test>]
  let ``OfList should fail for duplicate keys`` () =
    let input = [ (1, "first"); (1, "firstDup"); (2, "second"); (2, "secondDup") ]
    let result = OrderedMap.ofListIfNoDuplicates input

    match result with
    | Left _ -> Assert.Fail("Should have failed")
    | Right errors -> Assert.That((errors.Errors()).Head.Message.Contains("Duplicate keys: [1; 2]"), Is.True)

  [<Test>]
  let ``ContainsKey should work correctly`` () =
    let empty = OrderedMap<int, string>.empty
    let withKey = OrderedMap.Add empty 1 "hello"
    Assert.That(OrderedMap.containsKey withKey 1, Is.True)
    Assert.That(OrderedMap.containsKey withKey 2, Is.False)

  [<Test>]
  let ``TryFind should return correct values`` () =
    let empty = OrderedMap<int, string>.empty
    let withKey = OrderedMap.Add empty 1 "hello"
    Assert.That(OrderedMap.tryFind 1 withKey, Is.EqualTo(Some "hello"))
    Assert.That(OrderedMap.tryFind 2 withKey, Is.EqualTo(None))

  [<Test>]
  let ``Complex scenario with multiple operations`` () =
    let empty = OrderedMap<int, string>.empty
    let step1 = OrderedMap.Add empty 1 "one"
    let step2 = OrderedMap.Add step1 2 "two"
    let step3 = OrderedMap.Add step2 3 "three"
    let step4 = OrderedMap.Add step3 1 "ONE" // overwrite
    let step5 = OrderedMap.map (fun _ (v: string) -> v.ToUpper()) step4

    Assert.That(OrderedMap.count step5, Is.EqualTo(3))
    Assert.That(OrderedMap.keys step5, Is.EqualTo<int list>([ 1; 2; 3 ]))
    Assert.That(OrderedMap.values step5, Is.EqualTo<string list>([ "ONE"; "TWO"; "THREE" ]))
    Assert.That(OrderedMap.toList step5, Is.EqualTo<(int * string) list>([ (1, "ONE"); (2, "TWO"); (3, "THREE") ]))

  [<Test>]
  let ``mergeSecondAfterFirst should succeed for non-conflicting maps`` () =
    let map1 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"

    let map2 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 3 "third") 4 "fourth"

    let result = OrderedMap.mergeSecondAfterFirstIfNoDuplicates map1 map2

    match result with
    | Left merged ->
      Assert.That(OrderedMap.count merged, Is.EqualTo(4))
      Assert.That(OrderedMap.keys merged, Is.EqualTo<int list>([ 1; 2; 3; 4 ])) // om2 keys first, then om1 keys
      Assert.That(OrderedMap.values merged, Is.EqualTo<string list>([ "first"; "second"; "third"; "fourth" ]))
    | Right _ -> Assert.Fail("Should have succeeded")

  [<Test>]
  let ``mergeSecondAfterFirst should fail for conflicting keys`` () =
    let map1 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"

    let map2 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 2 "conflict") 3 "third"

    let result = OrderedMap.mergeSecondAfterFirstIfNoDuplicates map1 map2

    match result with
    | Left _ -> Assert.Fail("Should have failed")
    | Right errors ->
      Assert.That((errors.Errors()).Head.Message.Contains("Key conflicts during merge"), Is.True)
      Assert.That((errors.Errors()).Head.Message.Contains("2"), Is.True)

  [<Test>]
  let ``mergeSecondAfterFirstDuplicatesOk should merge with om1 values taking precedence`` () =
    let map1 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"

    let map2 =
      OrderedMap.Add (OrderedMap.Add OrderedMap.empty 2 "overwritten") 3 "third"

    let result = OrderedMap.mergeSecondAfterFirst map1 map2
    Assert.That(OrderedMap.count result, Is.EqualTo(3))
    Assert.That(OrderedMap.keys result, Is.EqualTo<int list>([ 1; 2; 3 ])) // om2 non-conflicting keys first, then om1 keys
    Assert.That(OrderedMap.values result, Is.EqualTo<string list>([ "first"; "second"; "third" ]))
    Assert.That(OrderedMap.tryFind 2 result, Is.EqualTo(Some "second")) // om2 value takes precedence

  [<Test>]
  let ``mergeSecondAfterFirstDuplicatesOk should handle empty first map`` () =
    let map1 = OrderedMap.empty

    let map2 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"

    let result = OrderedMap.mergeSecondAfterFirst map1 map2
    Assert.That(OrderedMap.count result, Is.EqualTo(2))
    Assert.That(OrderedMap.keys result, Is.EqualTo<int list>([ 1; 2 ]))
    Assert.That(OrderedMap.values result, Is.EqualTo<string list>([ "first"; "second" ]))

  [<Test>]
  let ``mergeSecondAfterFirstDuplicatesOk should handle empty second map`` () =
    let map1 = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"

    let map2 = OrderedMap.empty
    let result = OrderedMap.mergeSecondAfterFirst map1 map2
    Assert.That(OrderedMap.count result, Is.EqualTo(2))
    Assert.That(OrderedMap.keys result, Is.EqualTo<int list>([ 1; 2 ]))
    Assert.That(OrderedMap.values result, Is.EqualTo<string list>([ "first"; "second" ]))

  [<Test>]
  let ``mergeSecondAfterFirstDuplicatesOk should handle both maps empty`` () =
    let map1 = OrderedMap.empty
    let map2 = OrderedMap.empty
    let result = OrderedMap.mergeSecondAfterFirst map1 map2
    Assert.That(OrderedMap.count result, Is.EqualTo(0))

  [<Test>]
  let ``tryFindWithError should return value for existing key`` () =
    let map = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"
    let result = OrderedMap.tryFindWithError 1 "key" "1" map

    match result with
    | Left value -> Assert.That(value, Is.EqualTo("first"))
    | Right _ -> Assert.Fail("Should have found the key")

  [<Test>]
  let ``tryFindWithError should return error for missing key`` () =
    let map = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"
    let result = OrderedMap.tryFindWithError 3 "key" "3" map

    match result with
    | Left _ -> Assert.Fail("Should have failed")
    | Right errors -> Assert.That((errors.Errors()).Head.Message, Is.EqualTo("Cannot find key '3'"))

  [<Test>]
  let ``tryFindByWithError should return first matching key-value pair`` () =
    let map =
      OrderedMap.Add (OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second") 3 "third"

    let result =
      OrderedMap.tryFindByWithError (fun (k: int, v: string) -> k > 1 && v.Length > 5) "item" "matching" map

    match result with
    | Left(key, value) ->
      Assert.That(key, Is.EqualTo(2))
      Assert.That(value, Is.EqualTo("second"))
    | Right _ -> Assert.Fail("Should have found a matching item")

  [<Test>]
  let ``tryFindByWithError should return error when no match found`` () =
    let map = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"

    let result =
      OrderedMap.tryFindByWithError (fun (k, _) -> k > 10) "item" "matching" map

    match result with
    | Left _ -> Assert.Fail("Should have failed")
    | Right errors -> Assert.That((errors.Errors()).Head.Message, Is.EqualTo("Cannot find item 'matching'"))

  [<Test>]
  let ``filter should keep only matching items`` () =
    let map =
      OrderedMap.Add (OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second") 3 "third"

    let filtered = OrderedMap.filter (fun k _ -> k > 1) map

    Assert.That(OrderedMap.count filtered, Is.EqualTo(2))
    Assert.That(OrderedMap.keys filtered, Is.EqualTo<int list>([ 2; 3 ]))
    Assert.That(OrderedMap.values filtered, Is.EqualTo<string list>([ "second"; "third" ]))

  [<Test>]
  let ``filter should preserve order of remaining items`` () =
    let map =
      OrderedMap.Add
        (OrderedMap.Add (OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second") 3 "third")
        4
        "fourth"

    let filtered = OrderedMap.filter (fun k _ -> k % 2 = 0) map

    Assert.That(OrderedMap.count filtered, Is.EqualTo(2))
    Assert.That(OrderedMap.keys filtered, Is.EqualTo<int list>([ 2; 4 ]))
    Assert.That(OrderedMap.values filtered, Is.EqualTo<string list>([ "second"; "fourth" ]))

  [<Test>]
  let ``filter should return empty map when no items match`` () =
    let map = OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second"
    let filtered = OrderedMap.filter (fun k _ -> k > 10) map

    Assert.That(OrderedMap.isEmpty filtered, Is.True)
    Assert.That(OrderedMap.count filtered, Is.EqualTo(0))

  [<Test>]
  let ``ofSeq should create map from sequence`` () =
    let input =
      seq {
        (1, "first")
        (2, "second")
        (3, "third")
      }

    let result = OrderedMap.ofSeq input

    Assert.That(OrderedMap.count result, Is.EqualTo(3))
    Assert.That(OrderedMap.keys result, Is.EqualTo<int list>([ 1; 2; 3 ]))
    Assert.That(OrderedMap.values result, Is.EqualTo<string list>([ "first"; "second"; "third" ]))

  [<Test>]
  let ``ofSeq should handle duplicates by overwriting`` () =
    let input =
      seq {
        (1, "first")
        (1, "second")
        (2, "third")
      }

    let result = OrderedMap.ofSeq input

    Assert.That(OrderedMap.count result, Is.EqualTo(2))
    Assert.That(OrderedMap.keys result, Is.EqualTo<int list>([ 1; 2 ]))
    Assert.That(OrderedMap.values result, Is.EqualTo<string list>([ "second"; "third" ]))

  [<Test>]
  let ``toSeq should convert to sequence preserving order`` () =
    let map =
      OrderedMap.Add (OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second") 3 "third"

    let seq = OrderedMap.toSeq map
    let list = Seq.toList seq

    Assert.That(list, Is.EqualTo<(int * string) list>([ (1, "first"); (2, "second"); (3, "third") ]))

  [<Test>]
  let ``toArray should convert to array preserving order`` () =
    let map =
      OrderedMap.Add (OrderedMap.Add (OrderedMap.Add OrderedMap.empty 1 "first") 2 "second") 3 "third"

    let array = OrderedMap.toArray map

    let expected = [| (1, "first"); (2, "second"); (3, "third") |]
    Assert.That(array, Is.EqualTo<(int * string) array>(expected))

  [<Test>]
  let ``toArray should handle empty map`` () =
    let empty = OrderedMap.empty
    let array = OrderedMap.toArray empty

    Assert.That(array.Length, Is.EqualTo(0))
