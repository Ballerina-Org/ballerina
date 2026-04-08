module BallerinaLangDeltaDTO

open Ballerina.Data.Delta
open Ballerina.Data.Delta.Serialization.DeltaSerializer
open Ballerina.Data.Delta.Serialization.DeltaDeserializer
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.StdLib.List.Model
open Ballerina.Reader.WithError
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.MutableMemoryDB

open System
open Ballerina.DSL.Next.StdLib.String

let deltaUpdate
  : Delta<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>, DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>> =
  Delta.Ext(DeltaExtension(Choice1Of4(UpdateElement(0, Delta.Replace(Value.Primitive(PrimitiveValue.Int32 3))))))

let deltaAppend
  : Delta<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>, DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>> =
  Delta.Ext(DeltaExtension(Choice1Of4(AppendElement(Primitive(PrimitiveValue.Int32 15)))))

let deltaRemove
  : Delta<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>, DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>> =
  Delta.Ext(DeltaExtension(Choice1Of4(RemoveElement 3)))

let deltaReplace
  : Delta<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>, DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>> =
  Delta.Replace(
    Value.Ext(
      ValueExt(
        Choice1Of7(
          ListExt.ListValues(
            List
              [ Value.Primitive(PrimitiveValue.Int32 1)
                Value.Primitive(PrimitiveValue.Int32 2)
                Value.Primitive(PrimitiveValue.Int32 3) ]
          )
        )
      ),
      None
    )
  )

let deltaRecord
  : Delta<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>, DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>> =
  Delta.Record("Field1", deltaReplace)

let deltaRecordNested
  : Delta<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>, DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>> =
  Delta.Record("A", Delta.Tuple(0, Delta.Union("U1", Delta.Replace(Value.Primitive(PrimitiveValue.Float64 5.3)))))

let deltaUnion
  : Delta<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>, DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>> =
  Delta.Union("Case2", deltaReplace)

let deltaTuple
  : Delta<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>, DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>> =
  Delta.Tuple(4, deltaReplace)

let deltaSum
  : Delta<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>, DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>> =
  Delta.Sum(2, deltaReplace)

let deltaMultiple
  : Delta<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>, DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>> =
  Delta.Multiple
    [ deltaReplace
      deltaRecord
      deltaUnion
      deltaUnion
      deltaSum
      deltaRecordNested
      deltaUpdate
      deltaAppend
      deltaRemove ]


let roundtrip
  (delta: Delta<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>, DeltaExt<unit, MutableMemoryDB<unit, unit>, unit>>)
  =
  reader {
    let! deltaJson = Delta.JsonSerializeV2 delta
    let! deserializedDelta = Delta.JsonDeserializeV2 deltaJson
    return deltaJson, deserializedDelta
  }

let _, context, typeEvalConfig =
  db_ops () |> stdExtensions (StringTypeClass<_>.Console())

[<EntryPoint>]
let main _ =
  let result = roundtrip deltaMultiple |> Reader.Run context.SerializationContext

  match result with
  | Left(deltaJson, deserializedDelta) ->
    Console.WriteLine $"DELTA JSON: {deltaJson}\n\n\nDESERIALIZED DELTA:{deserializedDelta}\n"
    0
  | Right errors ->
    Console.Error.WriteLine $"ERROR: {errors.Errors}"
    1
