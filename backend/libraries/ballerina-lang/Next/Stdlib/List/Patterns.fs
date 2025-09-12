namespace Ballerina.DSL.Next.StdLib.List

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  // type ListConstructors<'ext> with

  type ListOperations<'ext> with
    static member AsMap(op: ListOperations<'ext>) : Sum<Option<Value<TypeValue, 'ext>>, Errors> =
      match op with
      | List_Map v -> v.f |> sum.Return
      | _ -> $"Error: Expected List_Map, found {op}" |> Errors.Singleton |> sum.Throw

    static member AsFilter(op: ListOperations<'ext>) : Sum<Option<Value<TypeValue, 'ext>>, Errors> =
      match op with
      | List_Filter v -> v.f |> sum.Return
      | _ -> $"Error: Expected List_Filter, found {op}" |> Errors.Singleton |> sum.Throw

    static member AsCons(op: ListOperations<'ext>) : Sum<Unit, Errors> =
      match op with
      | List_Cons -> () |> sum.Return
      | _ -> $"Error: Expected List_Cons, found {op}" |> Errors.Singleton |> sum.Throw

    static member AsNil(op: ListOperations<'ext>) : Sum<Unit, Errors> =
      match op with
      | List_Nil -> () |> sum.Return
      | _ -> $"Error: Expected List_Nil, found {op}" |> Errors.Singleton |> sum.Throw


  type ListValues<'ext> with
    static member AsList(op: ListValues<'ext>) : Sum<List<Value<TypeValue, 'ext>>, Errors> =
      match op with
      | ListValues.List v -> v |> sum.Return
