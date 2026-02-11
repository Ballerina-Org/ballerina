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
open Ballerina.DSL.Next.StdLib.MemoryDB

type ValueExt<'customExtension when 'customExtension: comparison> =
  | ValueExt of
    Choice<
      ListExt<'customExtension>,
      Unit,
      PrimitiveExt<'customExtension>,
      CompositeTypeExt<'customExtension>,
      MemoryDBExt<'customExtension>,
      MapExt<'customExtension>,
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

and ValueExtDTOKind =
  | List = 1
  | Map = 2

and ValueExtDTO =
  { Kind: ValueExtDTOKind
    List: List.Model.ListValueDTO<ValueExtDTO> | null
    Map: Map.Model.MapValueDTO<ValueExtDTO> | null }

  static member Empty =
    { Kind = ValueExtDTOKind.List
      List = null
      Map = null }

and MemoryDBExt<'customExtension when 'customExtension: comparison> =
  | MemoryDBValues of MemoryDB.Model.MemoryDBValues<ValueExt<'customExtension>>

  override self.ToString() : string =
    match self with
    | MemoryDBValues vals -> vals.ToString()

  static member inline ValueLens: PartialLens<ValueExt<'customExtension>, MemoryDBValues<ValueExt<'customExtension>>> =
    { Get =
        function
        | ValueExt(Choice5Of7(MemoryDBExt.MemoryDBValues x)) -> Some x
        | _ -> None
      Set = MemoryDBExt.MemoryDBValues >> Choice5Of7 >> ValueExt.ValueExt }

and MapExt<'customExtension when 'customExtension: comparison> =
  | MapOperations of Map.Model.MapOperations<ValueExt<'customExtension>>
  | MapValues of Map.Model.MapValues<ValueExt<'customExtension>>

  override self.ToString() : string =
    match self with
    | MapOperations ops -> ops.ToString()
    | MapValues vals -> vals.ToString()

  static member inline ValueLens: PartialLens<ValueExt<'customExtension>, MapValues<ValueExt<'customExtension>>> =
    { Get =
        function
        | ValueExt(Choice6Of7(MapValues x)) -> Some x
        | _ -> None
      Set = MapValues >> Choice6Of7 >> ValueExt.ValueExt }

  static member inline ValueDTOLens =
    { Get =
        fun valueExtDTO ->
          match valueExtDTO.Kind with
          | ValueExtDTOKind.Map -> Some valueExtDTO.Map
          | _ -> None
      Set =
        fun mapValueDTO ->
          { ValueExtDTO.Empty with
              Kind = ValueExtDTOKind.Map
              Map = mapValueDTO } }

and CompositeTypeExt<'customExtension when 'customExtension: comparison> =
  | CompositeType of
    Choice<
      DateOnlyExt<'customExtension>,
      DateTimeExt<'customExtension>,
      GuidExt<'customExtension>,
      TimeSpanExt<'customExtension>
     >

  override self.ToString() : string =
    match self with
    | CompositeType(Choice1Of4 ct) -> ct.ToString()
    | CompositeType(Choice2Of4 ct) -> ct.ToString()
    | CompositeType(Choice3Of4 ct) -> ct.ToString()
    | CompositeType(Choice4Of4 ct) -> ct.ToString()

and ListExt<'customExtension when 'customExtension: comparison> =
  | ListOperations of List.Model.ListOperations<ValueExt<'customExtension>>
  | ListValues of List.Model.ListValues<ValueExt<'customExtension>>

  override self.ToString() : string =
    match self with
    | ListOperations ops -> ops.ToString()
    | ListValues vals -> vals.ToString()

  static member inline ValueLens: PartialLens<ValueExt<'customExtension>, ListValues<ValueExt<'customExtension>>> =
    { Get =
        fun (v: ValueExt<'customExtension>) ->
          match v with
          | ValueExt(Choice1Of7(ListValues x)) -> Some x
          | _ -> None
      Set = ListValues >> Choice1Of7 >> ValueExt.ValueExt }


  static member inline ValueDTOLens =
    { Get =
        fun valueExtDTO ->
          match valueExtDTO.Kind with
          | ValueExtDTOKind.List -> Some valueExtDTO.List
          | _ -> None
      Set =
        fun valueExtDTO ->
          { ValueExtDTO.Empty with
              Kind = ValueExtDTOKind.List
              List = valueExtDTO } }

and DateOnlyExt<'customExtension when 'customExtension: comparison> =
  | DateOnlyOperations of DateOnly.Model.DateOnlyOperations<ValueExt<'customExtension>>

  override self.ToString() : string =
    match self with
    | DateOnlyOperations ops -> ops.ToString()

and DateTimeExt<'customExtension when 'customExtension: comparison> =
  | DateTimeOperations of DateTime.Model.DateTimeOperations<ValueExt<'customExtension>>

  override self.ToString() : string =
    match self with
    | DateTimeOperations ops -> ops.ToString()

and GuidExt<'customExtension when 'customExtension: comparison> =
  | GuidOperations of Guid.Model.GuidOperations<ValueExt<'customExtension>>

  override self.ToString() : string =
    match self with
    | GuidOperations ops -> ops.ToString()

and TimeSpanExt<'customExtension when 'customExtension: comparison> =
  | TimeSpanOperations of TimeSpanOperations<ValueExt<'customExtension>>

  override self.ToString() : string =
    match self with
    | TimeSpanOperations ops -> ops.ToString()

and PrimitiveExt<'customExtension when 'customExtension: comparison> =
  | BoolOperations of Bool.Model.BoolOperations<ValueExt<'customExtension>>
  | Int32Operations of Int32.Model.Int32Operations<ValueExt<'customExtension>>
  | Int64Operations of Int64.Model.Int64Operations<ValueExt<'customExtension>>
  | Float32Operations of Float32.Model.Float32Operations<ValueExt<'customExtension>>
  | Float64Operations of Float64.Model.Float64Operations<ValueExt<'customExtension>>
  | DecimalOperations of Decimal.Model.DecimalOperations<ValueExt<'customExtension>>
  | StringOperations of String.Model.StringOperations<ValueExt<'customExtension>>

  override self.ToString() : string =
    match self with
    | BoolOperations ops -> ops.ToString()
    | Int32Operations ops -> ops.ToString()
    | Int64Operations ops -> ops.ToString()
    | Float32Operations ops -> ops.ToString()
    | Float64Operations ops -> ops.ToString()
    | DecimalOperations ops -> ops.ToString()
    | StringOperations ops -> ops.ToString()

and DeltaExt<'customExtension when 'customExtension: comparison> =
  | DeltaExtension of Choice<ListDeltaExt<ValueExt<'customExtension>>, TupleDeltaExt<'customExtension>, OptionDeltaExt>

  static member inline ListDeltaLens: PartialLens<DeltaExt<'customExtension>, ListDeltaExt<ValueExt<'customExtension>>> =
    { Get =
        (function
        | DeltaExtension(Choice1Of3 listDelta) -> Some listDelta
        | _ -> None)
      Set = Choice1Of3 >> DeltaExtension }

  static member inline ListDeltaDTOLens =
    { Get =
        fun deltaExtDTO ->
          match deltaExtDTO.Discriminator with
          | DeltaExtDiscriminator.List -> Some deltaExtDTO.ListDelta
          | _ -> None
      Set =
        fun deltaExtDTO ->
          { DeltaExtDTO.Empty with
              Discriminator = DeltaExtDiscriminator.List
              ListDelta = deltaExtDTO } }

  static member Getters = {| DeltaExt = fun (DeltaExtension e) -> e |}

  static member ToUpdater
    : (DeltaExt<'customExtension>
        -> Model.Value<Model.TypeValue<ValueExt<'customExtension>>, ValueExt<'customExtension>>
        -> Sum<Model.Value<Model.TypeValue<ValueExt<'customExtension>>, ValueExt<'customExtension>>, Errors<unit>>) =
    fun (delta: DeltaExt<'customExtension>) ->
      fun (value: Model.Value<Model.TypeValue<ValueExt<'customExtension>>, ValueExt<'customExtension>>) ->
        sum {
          match value with
          | Value.Ext(ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List l))), _) ->
            match delta with
            | DeltaExt.DeltaExtension(Choice1Of3(UpdateElement(i, v))) ->
              let next = List.updateAt i v l

              return
                (ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List next))), None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of3(ListDeltaExt.AppendElement(v))) ->
              let next = List.append l [ v ]

              return
                (ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List next))), None)
                |> Value.Ext
            | DeltaExt.DeltaExtension(Choice1Of3(ListDeltaExt.RemoveElement(i))) ->
              let next = List.removeAt i l

              return
                (ValueExt(Choice1Of7(ListValues(List.Model.ListValues.List next))), None)
                |> Value.Ext
            | other ->
              return! sum.Throw(Errors.Singleton () (fun () -> $"Unimplemented delta ext toUpdater for {other}"))
          | Value.Tuple elements ->
            match delta with
            | DeltaExt.DeltaExtension(Choice2Of3(TupleDeltaExt.RemoveElement(i))) ->
              let next = List.removeAt i elements
              return Value.Tuple next
            | DeltaExt.DeltaExtension(Choice2Of3(TupleDeltaExt.AppendElement(value))) ->
              let next = List.append elements [ value ]
              return Value.Tuple next
            | other ->
              return!
                sum.Throw(Errors.Singleton () (fun () -> $"Unimplemented delta ext toUpdater for tuple op: {other}"))
          | other ->
            return!
              sum.Throw(Errors.Singleton () (fun () -> $"Expected value to be a list for List Delta ext, got {other}"))
        }

and TupleDeltaExt<'customExtension when 'customExtension: comparison> =
  | RemoveElement of index: int
  | AppendElement of value: Model.Value<Model.TypeValue<ValueExt<'customExtension>>, ValueExt<'customExtension>>

and OptionDeltaExt = OptionDeltaExt

and DeltaExtDiscriminator =
  | List = 1

and DeltaExtDTO =
  { Discriminator: DeltaExtDiscriminator
    ListDelta: ListDeltaExtDTO<ValueExtDTO> | null }

  static member Empty =
    { Discriminator = DeltaExtDiscriminator.List
      ListDelta = null }

type StdExtensions<'valueExt, 'valueExtDTO, 'deltaExt, 'deltaExtDTO
  when 'valueExt: comparison
  and 'deltaExt: comparison
  and 'valueExtDTO: not null
  and 'valueExtDTO: not struct
  and 'deltaExtDTO: not null
  and 'deltaExtDTO: not struct> =
  { List:
      TypeExtension<
        'valueExt,
        'valueExtDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        List.Model.ListValues<'valueExt>,
        List.Model.ListOperations<'valueExt>
       >
    Bool: OperationsExtension<'valueExt, Bool.Model.BoolOperations<'valueExt>>
    Int32: OperationsExtension<'valueExt, Int32.Model.Int32Operations<'valueExt>>
    Int64: OperationsExtension<'valueExt, Int64.Model.Int64Operations<'valueExt>>
    Float32: OperationsExtension<'valueExt, Float32.Model.Float32Operations<'valueExt>>
    Float64: OperationsExtension<'valueExt, Float64.Model.Float64Operations<'valueExt>>
    Decimal: OperationsExtension<'valueExt, Decimal.Model.DecimalOperations<'valueExt>>
    DateOnly: OperationsExtension<'valueExt, DateOnly.Model.DateOnlyOperations<'valueExt>>
    DateTime: OperationsExtension<'valueExt, DateTime.Model.DateTimeOperations<'valueExt>>
    String: OperationsExtension<'valueExt, String.Model.StringOperations<'valueExt>>
    Guid: OperationsExtension<'valueExt, Guid.Model.GuidOperations<'valueExt>>
    TimeSpan: OperationsExtension<'valueExt, TimeSpan.Model.TimeSpanOperations<'valueExt>>
    Map:
      TypeExtension<
        'valueExt,
        'valueExtDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        Map.Model.MapValues<'valueExt>,
        Map.Model.MapOperations<'valueExt>
       > }

let makeExtensions<'customExtension when 'customExtension: comparison>
  ()
  : StdExtensions<ValueExt<'customExtension>, ValueExtDTO, DeltaExt<'customExtension>, DeltaExtDTO> *
    LanguageContext<ValueExt<'customExtension>, ValueExtDTO, DeltaExt<'customExtension>, DeltaExtDTO>
  =

  let memoryDBRunQueryExtension =
    MemoryDB.Extension.QueryRunner.MemoryDBQueryRunnerExtension<
      ValueExt<'customExtension>,
      ValueExtDTO,
      DeltaExt<'customExtension>,
      DeltaExtDTO
     >
      { Get =
          function
          | ValueExt(Choice1Of7(ListExt.ListValues(List.Model.ListValues.List values))) -> Some values
          | _ -> None
        Set =
          List.Model.ListValues.List
          >> ListExt.ListValues
          >> Choice1Of7
          >> ValueExt.ValueExt }
      MemoryDBExt<_>.ValueLens

  let memoryDBCUDExtension =
    MemoryDB.Extension.CUD.MemoryDBCUDExtension<ValueExt<'customExtension>, ValueExtDTO>
      // (fun values ->
      //   ListExt.ListValues(List.Model.ListValues.List values)
      //   |> Choice1Of6
      //   |> ValueExt.ValueExt)
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

      MemoryDBExt<_>.ValueLens

  let memoryDBRunExtension =
    MemoryDB.Extension.DBRun.MemoryDBRunExtension<ValueExt<'customExtension>, ValueExtDTO> MemoryDBExt<_>.ValueLens

  let memoryDBGetByIdExtension =
    MemoryDB.Extension.GetById.MemoryDBGetByIdExtension<ValueExt<'customExtension>> MemoryDBExt<_>.ValueLens

  let memoryDBGetManyExtension =
    MemoryDB.Extension.GetMany.MemoryDBGetManyExtension<ValueExt<'customExtension>>
      (fun values ->
        ListExt.ListValues(List.Model.ListValues.List values)
        |> Choice1Of7
        |> ValueExt.ValueExt)
      MemoryDBExt<_>.ValueLens

  let memoryDBLookupsExtension =
    MemoryDB.Extension.Lookups.MemoryDBLookupsExtensions<ValueExt<'customExtension>>
      (fun values ->
        ListExt.ListValues(List.Model.ListValues.List values)
        |> Choice1Of7
        |> ValueExt.ValueExt)
      MemoryDBExt<_>.ValueLens

  let listExtension =
    List.Extension.ListExtension<ValueExt<'customExtension>, ValueExtDTO, DeltaExt<'customExtension>, DeltaExtDTO>
      ListExt<_>.ValueLens
      { Get =
          function
          | ValueExt(Choice1Of7(ListOperations x)) -> Some x
          | _ -> None
        Set = ListOperations >> Choice1Of7 >> ValueExt.ValueExt }
      ListExt<_>.ValueDTOLens
      DeltaExt<_>.ListDeltaLens
      DeltaExt<_>.ListDeltaDTOLens

  let dateOnlyExtension =
    DateOnly.Extension.DateOnlyExtension<ValueExt<'customExtension>, ValueExtDTO>
      { Get =
          function
          | ValueExt(Choice4Of7(CompositeType(Choice1Of4(DateOnlyOperations x)))) -> Some x
          | _ -> None
        Set =
          DateOnlyOperations
          >> Choice1Of4
          >> CompositeType
          >> Choice4Of7
          >> ValueExt.ValueExt }

  let boolExtension =
    Bool.Extension.BoolExtension<ValueExt<'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(BoolOperations x)) -> Some x
          | _ -> None
        Set = BoolOperations >> Choice3Of7 >> ValueExt.ValueExt }

  let int32Extension =
    Int32.Extension.Int32Extension<ValueExt<'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(Int32Operations x)) -> Some x
          | _ -> None
        Set = Int32Operations >> Choice3Of7 >> ValueExt.ValueExt }

  let int64Extension =
    Int64.Extension.Int64Extension<ValueExt<'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(Int64Operations x)) -> Some x
          | _ -> None
        Set = Int64Operations >> Choice3Of7 >> ValueExt.ValueExt }

  let float32Extension =
    Float32.Extension.Float32Extension<ValueExt<'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(Float32Operations x)) -> Some x
          | _ -> None
        Set = Float32Operations >> Choice3Of7 >> ValueExt.ValueExt }

  let float64Extension =
    Float64.Extension.Float64Extension<ValueExt<'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(Float64Operations x)) -> Some x
          | _ -> None
        Set = Float64Operations >> Choice3Of7 >> ValueExt.ValueExt }

  let decimalExtension =
    Decimal.Extension.DecimalExtension<ValueExt<'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(DecimalOperations x)) -> Some x
          | _ -> None
        Set = DecimalOperations >> Choice3Of7 >> ValueExt.ValueExt }

  let dateTimeExtension =
    DateTime.Extension.DateTimeExtension<ValueExt<'customExtension>, ValueExtDTO>
      { Get =
          function
          | ValueExt(Choice4Of7(CompositeType(Choice2Of4(DateTimeOperations x)))) -> Some x
          | _ -> None
        Set =
          DateTimeOperations
          >> Choice2Of4
          >> CompositeType
          >> Choice4Of7
          >> ValueExt.ValueExt }

  let timeSpanExtension =
    TimeSpanExtension<ValueExt<'customExtension>, ValueExtDTO>
      { Get =
          function
          | ValueExt(Choice4Of7(CompositeType(Choice4Of4(TimeSpanOperations x)))) -> Some x
          | _ -> None
        Set =
          TimeSpanOperations
          >> Choice4Of4
          >> CompositeType
          >> Choice4Of7
          >> ValueExt.ValueExt }

  let stringExtension =
    String.Extension.StringExtension<ValueExt<'customExtension>>
      { Get =
          function
          | ValueExt(Choice3Of7(StringOperations x)) -> Some x
          | _ -> None
        Set = StringOperations >> Choice3Of7 >> ValueExt.ValueExt }

  let guidExtension =
    Guid.Extension.GuidExtension<ValueExt<'customExtension>, ValueExtDTO>
      { Get =
          function
          | ValueExt(Choice4Of7(CompositeType(Choice3Of4(GuidOperations x)))) -> Some x
          | _ -> None
        Set = GuidOperations >> Choice3Of4 >> CompositeType >> Choice4Of7 >> ValueExt.ValueExt }

  let mapExtension =
    Map.Extension.MapExtension<ValueExt<'customExtension>, ValueExtDTO, DeltaExt<'customExtension>, DeltaExtDTO>
      MapExt<_>.ValueLens
      { Get =
          function
          | ValueExt(Choice6Of7(MapOperations x)) -> Some x
          | _ -> None
        Set = MapOperations >> Choice6Of7 >> ValueExt.ValueExt }
      (Some ListExt<'customExtension>.ValueLens)
      MapExt<_>.ValueDTOLens

  let context =
    LanguageContext<ValueExt<'customExtension>, ValueExtDTO, DeltaExt<'customExtension>, DeltaExtDTO>.Empty
    |> (memoryDBRunQueryExtension |> TypeExtension.RegisterLanguageContext)
    |> (memoryDBRunExtension |> TypeLambdaExtension.RegisterLanguageContext)
    |> (memoryDBGetByIdExtension |> OperationsExtension.RegisterLanguageContext)
    |> (memoryDBGetManyExtension |> OperationsExtension.RegisterLanguageContext)
    |> (memoryDBCUDExtension |> OperationsExtension.RegisterLanguageContext)
    |> (memoryDBLookupsExtension |> OperationsExtension.RegisterLanguageContext)
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
      Map = mapExtension }

  extensions, context

let stdExtensions = makeExtensions<unit> ()
