module Ballerina.Data.Tests.Delta.Extensions

open NUnit.Framework
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Delta.Model
open Ballerina.Data.Delta.Extensions
open Ballerina.Data.Delta
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.List.Model

[<Test>]
let ``Delta extensions, list update string element at`` () =

  let str v =
    Value<ValueExt>.Primitive(PrimitiveValue.String v)

  let list =
    ValueExt(Choice1Of4(ListExt.ListValues(List [ str "a"; str "b"; str "c" ])))
    |> Value<ValueExt>.Ext

  let delta =
    Delta.Ext(DeltaExt.DeltaExt(Choice1Of3(ListDeltaExt.UpdateElement(1, str "-"))))

  match Delta.ToUpdater DeltaExt.ToUpdater delta with
  | Sum.Left updater ->
    match updater list with
    | Sum.Left result ->

      Assert.That(
        result,
        Is.EqualTo(
          ValueExt(Choice1Of4(ListExt.ListValues(List [ str "a"; str "-"; str "c" ])))
          |> Value<ValueExt>.Ext
        )
      )
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
  | Sum.Right err -> Assert.Fail $"ToUpdater failed: {err}"

[<Test>]
let ``Delta extensions, list update complex value at`` () =
  let str v =
    Value<ValueExt>.Primitive(PrimitiveValue.String v)

  let sum i length =
    Value<ValueExt>.Sum({ Case = i; Count = length }, str $"{i}/{length}")

  let list =
    ValueExt(Choice1Of4(ListExt.ListValues(List [ sum 0 3; sum 1 3; sum 2 3 ])))
    |> Value<ValueExt>.Ext

  let delta =
    Delta.Ext(DeltaExt.DeltaExt(Choice1Of3(ListDeltaExt.UpdateElement(1, sum 4 4))))

  match Delta.ToUpdater DeltaExt.ToUpdater delta with
  | Sum.Left updater ->
    match updater list with
    | Sum.Left result ->

      Assert.That(
        result,
        Is.EqualTo(
          ValueExt(Choice1Of4(ListExt.ListValues(List [ sum 0 3; sum 4 4; sum 2 3 ])))
          |> Value<ValueExt>.Ext
        )
      )
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
  | Sum.Right err -> Assert.Fail $"ToUpdater failed: {err}"

[<Test>]
let ``Delta extensions, list append element`` () =

  let str v =
    Value<ValueExt>.Primitive(PrimitiveValue.String v)

  let list =
    ValueExt(Choice1Of4(ListExt.ListValues(List [ str "a"; str "b"; str "c" ])))
    |> Value<ValueExt>.Ext

  let delta =
    Delta.Ext(DeltaExt.DeltaExt(Choice1Of3(ListDeltaExt.AppendElement(str "d"))))

  match Delta.ToUpdater DeltaExt.ToUpdater delta with
  | Sum.Left updater ->
    match updater list with
    | Sum.Left result ->

      Assert.That(
        result,
        Is.EqualTo(
          ValueExt(Choice1Of4(ListExt.ListValues(List [ str "a"; str "b"; str "c"; str "d" ])))
          |> Value<ValueExt>.Ext
        )
      )
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
  | Sum.Right err -> Assert.Fail $"ToUpdater failed: {err}"

[<Test>]
let ``Delta extensions, list remove element`` () =

  let str v =
    Value<ValueExt>.Primitive(PrimitiveValue.String v)

  let list =
    ValueExt(Choice1Of4(ListExt.ListValues(List [ str "a"; str "b"; str "c" ])))
    |> Value<ValueExt>.Ext

  let delta = Delta.Ext(DeltaExt.DeltaExt(Choice1Of3(ListDeltaExt.RemoveElement(1))))

  match Delta.ToUpdater DeltaExt.ToUpdater delta with
  | Sum.Left updater ->
    match updater list with
    | Sum.Left result ->

      Assert.That(
        result,
        Is.EqualTo(
          ValueExt(Choice1Of4(ListExt.ListValues(List [ str "a"; str "c" ])))
          |> Value<ValueExt>.Ext
        )
      )
    | Sum.Right err -> Assert.Fail $"Unexpected error: {err}"
  | Sum.Right err -> Assert.Fail $"ToUpdater failed: {err}"
