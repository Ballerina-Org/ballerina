namespace Ballerina.Data.Delta

open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.List.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.Errors

module Extensions =

  type Value = Value<TypeValue, ValueExt>

  type DeltaExt = DeltaExt of Choice<ListDeltaExt, TupleDeltaExt, OptionDeltaExt>

  and ListDeltaExt =
    | UpdateElement of index: int * value: Value
    | AppendElement of value: Value
    | RemoveElement of index: int

  and TupleDeltaExt =
    | RemoveElement of index: int
    | AppendElement of value: Value

  and OptionDeltaExt = OptionDeltaExt

  type DeltaExt with
    static member ToUpdater: (DeltaExt -> Value -> Sum<Value, Errors>) =
      fun (delta: DeltaExt) ->
        fun (value: Value) ->
          sum {
            match value with
            | Value.Ext(ValueExt(Choice1Of4(ListValues(List l)))) ->
              match delta with
              | DeltaExt.DeltaExt(Choice1Of3(UpdateElement(i, v))) ->
                let next = List.updateAt i v l
                return ValueExt(Choice1Of4(ListValues(List next))) |> Value.Ext
              | DeltaExt.DeltaExt(Choice1Of3(ListDeltaExt.AppendElement(v))) ->
                let next = List.append l [ v ]
                return ValueExt(Choice1Of4(ListValues(List next))) |> Value.Ext
              | DeltaExt.DeltaExt(Choice1Of3(ListDeltaExt.RemoveElement(i))) ->
                let next = List.removeAt i l
                return ValueExt(Choice1Of4(ListValues(List next))) |> Value.Ext
              | other -> return! sum.Throw(Errors.Singleton $"Unimplemented delta ext toUpdater for {other}")
            | Value.Tuple elements ->
              match delta with
              | DeltaExt.DeltaExt(Choice2Of3(TupleDeltaExt.RemoveElement(i))) ->
                let next = List.removeAt i elements
                return Value.Tuple next
              | DeltaExt.DeltaExt(Choice2Of3(TupleDeltaExt.AppendElement(value))) ->
                let next = List.append elements [ value ]
                return Value.Tuple next
              | other -> return! sum.Throw(Errors.Singleton $"Unimplemented delta ext toUpdater for tuple op: {other}")
            | other ->
              return! sum.Throw(Errors.Singleton $"Expected value to be a list for List Delta ext, got {other}")
          }
