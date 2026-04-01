module Ballerina.DSL.Next.StdLib.Extensions

open Ballerina.DSL.Next.Types
open Ballerina.DSL.Next.StdLib.TimeSpan
open Ballerina.Lenses
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.StdLib
open Ballerina.Collections.Sum
open Ballerina.Errors
open Ballerina.DSL.Next.Serialization.PocoObjects
open System
open Ballerina.DSL.Next.StdLib.List.Model
open Ballerina.DSL.Next.StdLib.Map.Model
open Ballerina.DSL.Next.StdLib.DB
open Ballerina.Data.Delta
open Ballerina.DSL.Next.StdLib.String
open Ballerina.DSL.Next.Types.TypeChecker.Model

type ValueExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | ValueExt of
    Choice<
      ListExt<'runtimeContext, 'db, 'customExtension>,
      Unit,
      PrimitiveExt<'runtimeContext, 'db, 'customExtension>,
      CompositeTypeExt<'runtimeContext, 'db, 'customExtension>,
      DBExt<'runtimeContext, 'db, 'customExtension>,
      MapExt<'runtimeContext, 'db, 'customExtension>,
      'customExtension
     >

  override self.ToString() =
    match self with
    | ValueExt(Choice1Of7 ext) -> ext.ToString()
    | ValueExt(Choice2Of7 ext) -> ext.ToString()
    | ValueExt(Choice3Of7 ext) -> ext.ToString()
    | ValueExt(Choice4Of7 ext) -> ext.ToString()
    | ValueExt(Choice5Of7 ext) -> ext.ToString()
    | ValueExt(Choice6Of7 ext) -> ext.ToString()
    | ValueExt(Choice7Of7 ext) -> ext.ToString()


  static member Getters = {| ValueExt = fun (ValueExt e) -> e |}

  static member Updaters = {| ValueExt = fun u (ValueExt e) -> ValueExt(u e) |}

and ValueExtDTO =
  { List: List.Model.ListValueDTO<ValueExtDTO>
    Map: Map.Model.MapValueDTO<ValueExtDTO> }

  static member Empty = { List = null; Map = null }

and DBExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | DBValues of DB.Model.DBValues<'runtimeContext, 'db, ValueExt<'runtimeContext, 'db, 'customExtension>>

  override self.ToString() : string =
    match self with
    | DBValues vals -> vals.ToString()

  static member inline ValueLens
    : PartialLens<
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        DBValues<'runtimeContext, 'db, ValueExt<'runtimeContext, 'db, 'customExtension>>
       > =
    { Get =
        function
        | ValueExt(Choice5Of7(DBExt.DBValues x)) -> Some x
        | _ -> None
      Set = DBExt.DBValues >> Choice5Of7 >> ValueExt.ValueExt }

and MapExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | MapOperations of Map.Model.MapOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | MapValues of Map.Model.MapValues<ValueExt<'runtimeContext, 'db, 'customExtension>>

  override self.ToString() : string =
    match self with
    | MapOperations ops -> ops.ToString()
    | MapValues vals -> vals.ToString()

  static member inline ValueLens
    : PartialLens<
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        MapValues<ValueExt<'runtimeContext, 'db, 'customExtension>>
       > =
    { Get =
        function
        | ValueExt(Choice6Of7(MapValues x)) -> Some x
        | _ -> None
      Set = MapValues >> Choice6Of7 >> ValueExt.ValueExt }

  static member inline ValueDTOLens =
    { Get =
        fun valueExtDTO ->
          match valueExtDTO.Map with
          | null -> None
          | map -> Some map
      Set =
        fun mapValueDTO ->
          { ValueExtDTO.Empty with
              Map = mapValueDTO } }

and CompositeTypeExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | CompositeType of
    Choice<
      DateOnlyExt<'runtimeContext, 'db, 'customExtension>,
      DateTimeExt<'runtimeContext, 'db, 'customExtension>,
      GuidExt<'runtimeContext, 'db, 'customExtension>,
      TimeSpanExt<'runtimeContext, 'db, 'customExtension>,
      UpdaterExt<'runtimeContext, 'db, 'customExtension>
     >

  override self.ToString() : string =
    match self with
    | CompositeType(Choice1Of5 ct) -> ct.ToString()
    | CompositeType(Choice2Of5 ct) -> ct.ToString()
    | CompositeType(Choice3Of5 ct) -> ct.ToString()
    | CompositeType(Choice4Of5 ct) -> ct.ToString()
    | CompositeType(Choice5Of5 ct) -> ct.ToString()

and ListExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | ListOperations of List.Model.ListOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | ListValues of List.Model.ListValues<ValueExt<'runtimeContext, 'db, 'customExtension>>

  override self.ToString() : string =
    match self with
    | ListOperations ops -> ops.ToString()
    | ListValues vals -> vals.ToString()

  static member inline ValueLens
    : PartialLens<
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ListValues<ValueExt<'runtimeContext, 'db, 'customExtension>>
       > =
    { Get =
        fun (v: ValueExt<'runtimeContext, 'db, 'customExtension>) ->
          match v with
          | ValueExt(Choice1Of7(ListValues x)) -> Some x
          | _ -> None
      Set = ListValues >> Choice1Of7 >> ValueExt.ValueExt }


  static member inline ValueDTOLens =
    { Get =
        fun valueExtDTO ->
          match valueExtDTO.List with
          | null -> None
          | list -> Some list
      Set =
        fun valueExtDTO ->
          { ValueExtDTO.Empty with
              List = valueExtDTO } }

and DateOnlyExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | DateOnlyOperations of DateOnly.Model.DateOnlyOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>

  override self.ToString() : string =
    match self with
    | DateOnlyOperations ops -> ops.ToString()

and DateTimeExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | DateTimeOperations of DateTime.Model.DateTimeOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>

  override self.ToString() : string =
    match self with
    | DateTimeOperations ops -> ops.ToString()

and GuidExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | GuidOperations of Guid.Model.GuidOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>

  override self.ToString() : string =
    match self with
    | GuidOperations ops -> ops.ToString()

and TimeSpanExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | TimeSpanOperations of TimeSpan.Model.TimeSpanOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>

  override self.ToString() : string =
    match self with
    | TimeSpanOperations ops -> ops.ToString()

and UpdaterExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | UpdaterOperations of Updater.Model.UpdaterOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>

and PrimitiveExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | BoolOperations of Bool.Model.BoolOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | Int32Operations of Int32.Model.Int32Operations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | Int64Operations of Int64.Model.Int64Operations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | Float32Operations of Float32.Model.Float32Operations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | Float64Operations of Float64.Model.Float64Operations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | DecimalOperations of Decimal.Model.DecimalOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>
  | StringOperations of String.Model.StringOperations<ValueExt<'runtimeContext, 'db, 'customExtension>>

  override self.ToString() : string =
    match self with
    | BoolOperations ops -> ops.ToString()
    | Int32Operations ops -> ops.ToString()
    | Int64Operations ops -> ops.ToString()
    | Float32Operations ops -> ops.ToString()
    | Float64Operations ops -> ops.ToString()
    | DecimalOperations ops -> ops.ToString()
    | StringOperations ops -> ops.ToString()

and DeltaExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | DeltaExtension of
    Choice<
      ListDeltaExt<ValueExt<'runtimeContext, 'db, 'customExtension>, DeltaExt<'runtimeContext, 'db, 'customExtension>>,
      TupleDeltaExt<'runtimeContext, 'db, 'customExtension>,
      OptionDeltaExt,
      Map.Model.MapDeltaExt<
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExt<'runtimeContext, 'db, 'customExtension>
       >
     >

  static member inline ListDeltaLens
    : PartialLens<
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        ListDeltaExt<ValueExt<'runtimeContext, 'db, 'customExtension>, DeltaExt<'runtimeContext, 'db, 'customExtension>>
       > =
    { Get =
        (function
        | DeltaExtension(Choice1Of4 listDelta) -> Some listDelta
        | _ -> None)
      Set = Choice1Of4 >> DeltaExtension }

  static member inline ListDeltaDTOLens =
    { Get =
        fun deltaExtDTO ->
          match deltaExtDTO.ListDelta with
          | null -> None
          | listDelta -> Some listDelta
      Set =
        fun deltaExtDTO ->
          { DeltaExtDTO.Empty with
              ListDelta = deltaExtDTO } }

  static member inline MapDeltaLens
    : PartialLens<
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        Map.Model.MapDeltaExt<
          ValueExt<'runtimeContext, 'db, 'customExtension>,
          DeltaExt<'runtimeContext, 'db, 'customExtension>
         >
       > =
    { Get =
        (function
        | DeltaExtension(Choice4Of4 mapDelta) -> Some mapDelta
        | _ -> None)
      Set = Choice4Of4 >> DeltaExtension }

  static member inline MapDeltaDTOLens =
    { Get =
        fun deltaExtDTO ->
          match deltaExtDTO.MapDelta with
          | null -> None
          | mapDelta -> Some mapDelta
      Set =
        fun deltaExtDTO ->
          { DeltaExtDTO.Empty with
              MapDelta = deltaExtDTO } }

  static member Getters = {| DeltaExt = fun (DeltaExtension e) -> e |}

  static member ToUpdater
    : (DeltaExt<'runtimeContext, 'db, 'customExtension>
        -> Model.Value<
          Model.TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
          ValueExt<'runtimeContext, 'db, 'customExtension>
         >
        -> Sum<
          Model.Value<
            Model.TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
            ValueExt<'runtimeContext, 'db, 'customExtension>
           >,
          Errors<unit>
         >) =
    fun (delta: DeltaExt<'runtimeContext, 'db, 'customExtension>) ->
      fun
          (value:
            Model.Value<
              Model.TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
              ValueExt<'runtimeContext, 'db, 'customExtension>
             >) ->
        sum {
          match value with
          | Value.Ext(ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List l))), _) ->
            match delta with
            | DeltaExt.DeltaExtension(Choice1Of4(UpdateElement(i, delta))) ->
              let! updater = Delta.ToUpdater DeltaExt<'runtimeContext, 'db, 'customExtension>.ToUpdater delta
              let currentElement = List.item i l
              let! updatedElement = updater currentElement
              let next = List.updateAt i updatedElement l

              return
                (ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List next))), None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.AppendElement(v))) ->
              let next = List.append l [ v ]

              return
                (ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List next))), None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.RemoveElement(i))) ->
              let next = List.removeAt i l

              return
                (ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List next))), None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.InsertElement(i, v))) ->
              let next = List.insertAt (i + 1) v l

              return
                (ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List next))), None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.DuplicateElement(i))) ->
              let next = List.insertAt (i + 1) (List.item i l) l

              return
                (ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List next))), None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.SetAllElements(v))) ->
              let next = List.map (fun _ -> v) l

              return
                (ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List next))), None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.RemoveAllElements)) ->
              let next = List.empty

              return
                (ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List next))), None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.MoveElement(fromIndex, toIndex))) ->
              let len = List.length l

              let next =
                if fromIndex < 0 || fromIndex >= len then
                  l
                elif toIndex < 0 || toIndex >= len then
                  l
                elif fromIndex = toIndex then
                  l
                else
                  let element = List.item fromIndex l
                  let withoutElement = List.removeAt fromIndex l
                  List.insertAt toIndex element withoutElement

              return
                (ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List next))), None)
                |> Value.Ext
            | other ->
              return! sum.Throw(Errors.Singleton () (fun () -> $"Unimplemented delta ext toUpdater for {other}"))
          | Value.Ext(ValueExt(Choice6Of7(MapExt.MapValues(Map.Model.MapValues.Map m))), _) ->
            match delta with
            | DeltaExt.DeltaExtension(Choice4Of4(Map.Model.MapDeltaExt.UpdateKey(oldKey, newKey))) ->
              match Map.tryFind oldKey m with
              | Some value ->
                let next = m |> Map.remove oldKey |> Map.add newKey value

                return
                  (ValueExt(Choice6Of7(MapExt.MapValues(Map.Model.MapValues.Map next))), None)
                  |> Value.Ext
              | None -> return! sum.Throw(Errors.Singleton () (fun () -> $"Key not found in map for UpdateKey delta"))
            | DeltaExt.DeltaExtension(Choice4Of4(Map.Model.MapDeltaExt.UpdateValue(key, delta))) ->
              match Map.tryFind key m with
              | Some currentValue ->
                let! updater = Delta.ToUpdater DeltaExt<'runtimeContext, 'db, 'customExtension>.ToUpdater delta
                let! updatedValue = updater currentValue
                let next = m |> Map.add key updatedValue

                return
                  (ValueExt(Choice6Of7(MapExt.MapValues(Map.Model.MapValues.Map next))), None)
                  |> Value.Ext
              | None -> return! sum.Throw(Errors.Singleton () (fun () -> $"Key not found in map for UpdateValue delta"))
            | DeltaExt.DeltaExtension(Choice4Of4(Map.Model.MapDeltaExt.AddItem(key, value))) ->
              let next = m |> Map.add key value

              return
                (ValueExt(Choice6Of7(MapExt.MapValues(Map.Model.MapValues.Map next))), None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice4Of4(Map.Model.MapDeltaExt.RemoveItem(key))) ->
              let next = m |> Map.remove key

              return
                (ValueExt(Choice6Of7(MapExt.MapValues(Map.Model.MapValues.Map next))), None)
                |> Value.Ext
            | other ->
              return!
                sum.Throw(Errors.Singleton () (fun () -> $"Unimplemented delta ext toUpdater for map op: {other}"))
          | Value.Tuple elements ->
            match delta with
            | DeltaExt.DeltaExtension(Choice2Of4(TupleDeltaExt.RemoveElement(i))) ->
              let next = List.removeAt i elements
              return Value.Tuple next
            | DeltaExt.DeltaExtension(Choice2Of4(TupleDeltaExt.AppendElement(value))) ->
              let next = List.append elements [ value ]
              return Value.Tuple next
            | other ->
              return!
                sum.Throw(Errors.Singleton () (fun () -> $"Unimplemented delta ext toUpdater for tuple op: {other}"))
          | other ->
            return!
              sum.Throw(
                Errors.Singleton () (fun () -> $"Expected value to be a list or map for Delta ext, got {other}")
              )
        }

and TupleDeltaExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  | RemoveElement of index: int
  | AppendElement of
    value:
      Model.Value<
        Model.TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >

and OptionDeltaExt = OptionDeltaExt

and DeltaExtDTO =
  { ListDelta: ListDeltaExtDTO<ValueExtDTO, DeltaExtDTO> | null
    MapDelta: Map.Model.MapDeltaExtDTO<ValueExtDTO, DeltaExtDTO> | null }

  static member Empty = { ListDelta = null; MapDelta = null }

type StdExtensions<'runtimeContext, 'valueExt, 'valueExtDTO, 'deltaExt, 'deltaExtDTO
  when 'valueExt: comparison
  and 'deltaExt: comparison
  and 'valueExtDTO: not null
  and 'valueExtDTO: not struct
  and 'deltaExtDTO: not null
  and 'deltaExtDTO: not struct> =
  { List:
      TypeExtension<
        'runtimeContext,
        'valueExt,
        'valueExtDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        List.Model.ListValues<'valueExt>,
        List.Model.ListOperations<'valueExt>
       >
    Bool: OperationsExtension<'runtimeContext, 'valueExt, Bool.Model.BoolOperations<'valueExt>>
    Int32: OperationsExtension<'runtimeContext, 'valueExt, Int32.Model.Int32Operations<'valueExt>>
    Int64: OperationsExtension<'runtimeContext, 'valueExt, Int64.Model.Int64Operations<'valueExt>>
    Float32: OperationsExtension<'runtimeContext, 'valueExt, Float32.Model.Float32Operations<'valueExt>>
    Float64: OperationsExtension<'runtimeContext, 'valueExt, Float64.Model.Float64Operations<'valueExt>>
    Decimal: OperationsExtension<'runtimeContext, 'valueExt, Decimal.Model.DecimalOperations<'valueExt>>
    DateOnly: OperationsExtension<'runtimeContext, 'valueExt, DateOnly.Model.DateOnlyOperations<'valueExt>>
    DateTime: OperationsExtension<'runtimeContext, 'valueExt, DateTime.Model.DateTimeOperations<'valueExt>>
    String: OperationsExtension<'runtimeContext, 'valueExt, String.Model.StringOperations<'valueExt>>
    Guid: OperationsExtension<'runtimeContext, 'valueExt, Guid.Model.GuidOperations<'valueExt>>
    TimeSpan: OperationsExtension<'runtimeContext, 'valueExt, TimeSpan.Model.TimeSpanOperations<'valueExt>>
    Updater: OperationsExtension<'runtimeContext, 'valueExt, Updater.Model.UpdaterOperations<'valueExt>>
    Map:
      TypeExtension<
        'runtimeContext,
        'valueExt,
        'valueExtDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        Map.Model.MapValues<'valueExt>,
        Map.Model.MapOperations<'valueExt>
       > }

let makeExtensions<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison>
  (string_ops: StringTypeClass<ValueExt<'runtimeContext, 'db, 'customExtension>>)
  (db_ops: DBTypeClass<'runtimeContext, 'db, ValueExt<'runtimeContext, 'db, 'customExtension>>)
  (typeEvalConfig: Option<TypeEvalConfig<ValueExt<'runtimeContext, 'db, 'customExtension>>>)
  : StdExtensions<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     > *
    LanguageContext<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     > *
    TypeEvalConfig<ValueExt<'runtimeContext, 'db, 'customExtension>>
  =

  let registerDBExtensions, query_sym, mk_query =
    Ballerina.DSL.Next.StdLib.DB.Extension.CUD.registerDBExtensions
      db_ops
      { Get =
          function
          | ValueExt(Choice1Of7(ListExt.ListValues(List.Model.ListValues.List values))) -> Some values
          | _ -> None
        Set =
          List.Model.ListValues.List
          >> ListExt.ListValues
          >> Choice1Of7
          >> ValueExt.ValueExt }
      { Get =
          function
          | ValueExt(Choice6Of7(MapValues(Map.Model.MapValues.Map x))) -> Some x
          | _ -> None
        Set = Map.Model.MapValues.Map >> MapValues >> Choice6Of7 >> ValueExt.ValueExt }
      DBExt<_, _, _>.ValueLens
      typeEvalConfig

  let listExtension, list_sym, mk_list_type =
    List.Extension.ListExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     >
      ListExt<_, _, _>.ValueLens
      { Get =
          function
          | ValueExt(Choice1Of7(ListOperations x)) -> Some x
          | _ -> None
        Set = ListOperations >> Choice1Of7 >> ValueExt.ValueExt }
      ListExt<_, _, _>.ValueDTOLens
      DeltaExt<_, _, _>.ListDeltaLens
      DeltaExt<_, _, _>.ListDeltaDTOLens
      (typeEvalConfig |> Option.map (fun cfg -> cfg.ListTypeSymbol))

  let dateOnlyExtension =
    DateOnly.Extension.DateOnlyExtension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>, ValueExtDTO>
      { Get =
          function
          | ValueExt(Choice4Of7(CompositeType(Choice1Of5(DateOnlyOperations x)))) -> Some x
          | _ -> None
        Set =
          DateOnlyOperations
          >> Choice1Of5
          >> CompositeType
          >> Choice4Of7
          >> ValueExt.ValueExt }

  let boolExtension =
    Bool.Extension.BoolExtension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(BoolOperations x)) -> Some x
          | _ -> None
        Set = BoolOperations >> Choice3Of7 >> ValueExt.ValueExt }

  let int32Extension =
    Int32.Extension.Int32Extension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(Int32Operations x)) -> Some x
          | _ -> None
        Set = Int32Operations >> Choice3Of7 >> ValueExt.ValueExt }

  let int64Extension =
    Int64.Extension.Int64Extension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(Int64Operations x)) -> Some x
          | _ -> None
        Set = Int64Operations >> Choice3Of7 >> ValueExt.ValueExt }

  let float32Extension =
    Float32.Extension.Float32Extension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(Float32Operations x)) -> Some x
          | _ -> None
        Set = Float32Operations >> Choice3Of7 >> ValueExt.ValueExt }

  let float64Extension =
    Float64.Extension.Float64Extension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(Float64Operations x)) -> Some x
          | _ -> None
        Set = Float64Operations >> Choice3Of7 >> ValueExt.ValueExt }

  let decimalExtension =
    Decimal.Extension.DecimalExtension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(DecimalOperations x)) -> Some x
          | _ -> None
        Set = DecimalOperations >> Choice3Of7 >> ValueExt.ValueExt }

  let dateTimeExtension =
    DateTime.Extension.DateTimeExtension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>, ValueExtDTO>
      { Get =
          function
          | ValueExt(Choice4Of7(CompositeType(Choice2Of5(DateTimeOperations x)))) -> Some x
          | _ -> None
        Set =
          DateTimeOperations
          >> Choice2Of5
          >> CompositeType
          >> Choice4Of7
          >> ValueExt.ValueExt }

  let timeSpanExtension =
    TimeSpanExtension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>, ValueExtDTO>
      { Get =
          function
          | ValueExt(Choice4Of7(CompositeType(Choice4Of5(TimeSpanOperations x)))) -> Some x
          | _ -> None
        Set =
          TimeSpanOperations
          >> Choice4Of5
          >> CompositeType
          >> Choice4Of7
          >> ValueExt.ValueExt }

  let stringExtension =
    String.Extension.StringExtension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>>
      string_ops
      { Get =
          function
          | ValueExt(Choice3Of7(StringOperations x)) -> Some x
          | _ -> None
        Set = StringOperations >> Choice3Of7 >> ValueExt.ValueExt }

  let guidExtension =
    Guid.Extension.GuidExtension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>, ValueExtDTO>
      { Get =
          function
          | ValueExt(Choice4Of7(CompositeType(Choice3Of5(GuidOperations x)))) -> Some x
          | _ -> None
        Set = GuidOperations >> Choice3Of5 >> CompositeType >> Choice4Of7 >> ValueExt.ValueExt }

  let updaterExtension =
    Updater.Extension.UpdaterExtension<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>>
      { Get =
          function
          | ValueExt(Choice4Of7(CompositeType(Choice5Of5(UpdaterOperations x)))) -> Some x
          | _ -> None
        Set =
          UpdaterOperations
          >> Choice5Of5
          >> CompositeType
          >> Choice4Of7
          >> ValueExt.ValueExt }

  let mapExtension =
    Map.Extension.MapExtension<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     >
      MapExt<_, _, _>.ValueLens
      { Get =
          function
          | ValueExt(Choice6Of7(MapOperations x)) -> Some x
          | _ -> None
        Set = MapOperations >> Choice6Of7 >> ValueExt.ValueExt }
      (Some ListExt<'runtimeContext, 'db, 'customExtension>.ValueLens)
      MapExt<_, _, _>.ValueDTOLens
      DeltaExt<_, _, _>.MapDeltaLens
      DeltaExt<_, _, _>.MapDeltaDTOLens

  let context
    : LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       > =
    LanguageContext<
      'runtimeContext,
      ValueExt<'runtimeContext, 'db, 'customExtension>,
      ValueExtDTO,
      DeltaExt<'runtimeContext, 'db, 'customExtension>,
      DeltaExtDTO
     >.Empty

  let context =
    context
    |> registerDBExtensions
    |> (listExtension |> TypeExtension.RegisterLanguageContext)
    |> (dateOnlyExtension |> OperationsExtension.RegisterLanguageContext)
    |> (dateTimeExtension |> OperationsExtension.RegisterLanguageContext)
    |> (guidExtension |> OperationsExtension.RegisterLanguageContext)
    |> (timeSpanExtension |> OperationsExtension.RegisterLanguageContext)
    |> (boolExtension |> OperationsExtension.RegisterLanguageContext)
    |> (int32Extension |> OperationsExtension.RegisterLanguageContext)
    |> (int64Extension |> OperationsExtension.RegisterLanguageContext)
    |> (float32Extension |> OperationsExtension.RegisterLanguageContext)
    |> (float64Extension |> OperationsExtension.RegisterLanguageContext)
    |> (decimalExtension |> OperationsExtension.RegisterLanguageContext)
    |> (stringExtension |> OperationsExtension.RegisterLanguageContext)
    |> (updaterExtension |> OperationsExtension.RegisterLanguageContext)
    |> (mapExtension |> TypeExtension.RegisterLanguageContext)

  let extensions =
    { List = listExtension
      Int32 = int32Extension
      Bool = boolExtension
      Int64 = int64Extension
      Float32 = float32Extension
      Float64 = float64Extension
      Decimal = decimalExtension
      DateOnly = dateOnlyExtension
      DateTime = dateTimeExtension
      String = stringExtension
      Guid = guidExtension
      TimeSpan = timeSpanExtension
      Map = mapExtension
      Updater = updaterExtension }

  let typeEvalConfig =
    { QueryTypeSymbol = query_sym
      ListTypeSymbol = list_sym
      MkQueryType = mk_query
      MkListType = mk_list_type }

  extensions, context, typeEvalConfig

let stdExtensions<'runtimeContext, 'db when 'db: comparison> =
  fun str_ops db_ops -> makeExtensions<'runtimeContext, 'db, unit> str_ops db_ops None

let stdExtensionsWithTypeEvalConfig<'runtimeContext, 'db when 'db: comparison> =
  fun str_ops db_ops typeEvalConfig -> makeExtensions<'runtimeContext, 'db, unit> str_ops db_ops typeEvalConfig

let customStdExtensions<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> =
  fun str_ops db_ops -> makeExtensions<'runtimeContext, 'db, 'customExtension> str_ops db_ops None

let customStdExtensionsWithTypeEvalConfig<'runtimeContext, 'db, 'customExtension
  when 'db: comparison and 'customExtension: comparison> =
  makeExtensions<'runtimeContext, 'db, 'customExtension>
