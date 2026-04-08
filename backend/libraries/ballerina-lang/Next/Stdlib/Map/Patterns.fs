namespace Ballerina.DSL.Next.StdLib.Map

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open Model

  type MapOperations<'ext> with
    static member AsMap(op: MapOperations<'ext>) : Sum<Option<Value<TypeValue<'ext>, 'ext>>, Errors<Unit>> =
      match op with
      | Map_Map v -> v.f |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Map_Map, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsSet
      (op: MapOperations<'ext>)
      : Sum<Option<Value<TypeValue<'ext>, 'ext> * Value<TypeValue<'ext>, 'ext>>, Errors<Unit>> =
      match op with
      | Map_Set v -> v |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Map_Set, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsEmpty(op: MapOperations<'ext>) : Sum<Unit, Errors<Unit>> =
      match op with
      | Map_Empty -> () |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Map_Empty, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsMapToList(op: MapOperations<'ext>) : Sum<Unit, Errors<Unit>> =
      match op with
      | Map_MapToList -> () |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Map_MapToList, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

  type MapValues<'ext when 'ext: comparison> with
    static member AsMap
      (op: MapValues<'ext>)
      : Sum<Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>, Errors<Unit>> =
      match op with
      | MapValues.Map v -> v |> sum.Return
