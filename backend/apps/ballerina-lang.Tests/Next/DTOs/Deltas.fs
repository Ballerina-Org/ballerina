module Ballerina.Cat.Tests.BusinessRuleEngine.Next.DTOs.Deltas

open NUnit.Framework
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.List.Model
open Ballerina.DSL.Next.StdLib.MutableMemoryDB

type private V =
  Value<TypeValue<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>>, ValueExt<unit, MutableMemoryDB<unit, unit>, unit>>

let private intVal (i: int) : V = Value.Primitive(PrimitiveValue.Int32 i)

let private mkList (items: V list) : V =
  Value.Ext(ValueExt(Choice1Of7(ListValues(ListValues.List items))), None)

let private getListItems (v: V) : V list =
  match v with
  | Value.Ext(ValueExt(Choice1Of7(ListValues(ListValues.List items))), _) -> items
  | other -> failwith $"Expected list value but got {other}"

let private applyMoveElement (fromIndex: int) (toIndex: int) (listValue: V) : V =
  let delta =
    DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>
      .DeltaExtension(Choice1Of4(ListDeltaExt.MoveElement(fromIndex, toIndex)))

  match DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>.ToUpdater delta listValue with
  | Sum.Left result -> result
  | Sum.Right err -> failwith $"MoveElement updater failed: {err}"

[<Test>]
let ``MoveElement forward moves element to a later position`` () =
  // [1, 2, 3, 4, 5] -> move index 1 to index 3 -> [1, 3, 4, 2, 5]
  let list = mkList [ intVal 1; intVal 2; intVal 3; intVal 4; intVal 5 ]
  let result = applyMoveElement 1 3 list
  let items = getListItems result

  let expected = [ intVal 1; intVal 3; intVal 4; intVal 2; intVal 5 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement backward moves element to an earlier position`` () =
  // [1, 2, 3, 4, 5] -> move index 3 to index 1 -> [1, 4, 2, 3, 5]
  let list = mkList [ intVal 1; intVal 2; intVal 3; intVal 4; intVal 5 ]
  let result = applyMoveElement 3 1 list
  let items = getListItems result

  let expected = [ intVal 1; intVal 4; intVal 2; intVal 3; intVal 5 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement to same index returns unchanged list`` () =
  let list = mkList [ intVal 1; intVal 2; intVal 3 ]
  let result = applyMoveElement 1 1 list
  let items = getListItems result

  let expected = [ intVal 1; intVal 2; intVal 3 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement first to last`` () =
  // [1, 2, 3, 4, 5] -> move index 0 to index 4 -> [2, 3, 4, 5, 1]
  let list = mkList [ intVal 1; intVal 2; intVal 3; intVal 4; intVal 5 ]
  let result = applyMoveElement 0 4 list
  let items = getListItems result

  let expected = [ intVal 2; intVal 3; intVal 4; intVal 5; intVal 1 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement last to first`` () =
  // [1, 2, 3, 4, 5] -> move index 4 to index 0 -> [5, 1, 2, 3, 4]
  let list = mkList [ intVal 1; intVal 2; intVal 3; intVal 4; intVal 5 ]
  let result = applyMoveElement 4 0 list
  let items = getListItems result

  let expected = [ intVal 5; intVal 1; intVal 2; intVal 3; intVal 4 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement with negative fromIndex returns unchanged list`` () =
  let list = mkList [ intVal 1; intVal 2; intVal 3 ]
  let result = applyMoveElement -1 1 list
  let items = getListItems result

  let expected = [ intVal 1; intVal 2; intVal 3 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement with negative toIndex returns unchanged list`` () =
  let list = mkList [ intVal 1; intVal 2; intVal 3 ]
  let result = applyMoveElement 1 -1 list
  let items = getListItems result

  let expected = [ intVal 1; intVal 2; intVal 3 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement with fromIndex out of bounds returns unchanged list`` () =
  let list = mkList [ intVal 1; intVal 2; intVal 3 ]
  let result = applyMoveElement 5 1 list
  let items = getListItems result

  let expected = [ intVal 1; intVal 2; intVal 3 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement with toIndex out of bounds returns unchanged list`` () =
  let list = mkList [ intVal 1; intVal 2; intVal 3 ]
  let result = applyMoveElement 1 5 list
  let items = getListItems result

  let expected = [ intVal 1; intVal 2; intVal 3 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement in two-element list swaps elements`` () =
  // [1, 2] -> move index 0 to index 1 -> [2, 1]
  let list = mkList [ intVal 1; intVal 2 ]
  let result = applyMoveElement 0 1 list
  let items = getListItems result

  let expected = [ intVal 2; intVal 1 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement adjacent forward`` () =
  // [1, 2, 3, 4] -> move index 1 to index 2 -> [1, 3, 2, 4]
  let list = mkList [ intVal 1; intVal 2; intVal 3; intVal 4 ]
  let result = applyMoveElement 1 2 list
  let items = getListItems result

  let expected = [ intVal 1; intVal 3; intVal 2; intVal 4 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement adjacent backward`` () =
  // [1, 2, 3, 4] -> move index 2 to index 1 -> [1, 3, 2, 4]
  let list = mkList [ intVal 1; intVal 2; intVal 3; intVal 4 ]
  let result = applyMoveElement 2 1 list
  let items = getListItems result

  let expected = [ intVal 1; intVal 3; intVal 2; intVal 4 ]
  Assert.That(items, Is.EqualTo(expected :> obj))

[<Test>]
let ``MoveElement in single-element list returns same list`` () =
  let list = mkList [ intVal 42 ]
  let result = applyMoveElement 0 0 list
  let items = getListItems result

  let expected = [ intVal 42 ]
  Assert.That(items, Is.EqualTo(expected :> obj))
