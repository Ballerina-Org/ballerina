module Ballerina.DSL.Next.StdLib.Extensions

open System
open Ballerina.DSL.Next.StdLib.TimeSpan
open Ballerina.Lenses
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.StdLib
open Ballerina.DSL.Next.Types

type ValueExt =
  | ValueExt of Choice<ListExt, OptionExt, PrimitiveExt, CompositeTypeExt, MemoryDBExt, MapExt>

  override self.ToString() =
    match self with
    | ValueExt(Choice1Of6 ext) -> ext.ToString()
    | ValueExt(Choice2Of6 ext) -> ext.ToString()
    | ValueExt(Choice3Of6 ext) -> ext.ToString()
    | ValueExt(Choice4Of6 ext) -> ext.ToString()
    | ValueExt(Choice5Of6 ext) -> ext.ToString()
    | ValueExt(Choice6Of6 ext) -> ext.ToString()

  static member Getters = {| ValueExt = fun (ValueExt e) -> e |}

  static member Updaters = {| ValueExt = fun u (ValueExt e) -> ValueExt(u e) |}

and MemoryDBExt =
  | MemoryDBValues of MemoryDB.Model.MemoryDBValues<ValueExt>

  override self.ToString() : string =
    match self with
    | MemoryDBValues vals -> vals.ToString()

and MapExt =
  | MapOperations of Map.Model.MapOperations<ValueExt>
  | MapValues of Map.Model.MapValues<ValueExt>

  override self.ToString() : string =
    match self with
    | MapOperations ops -> ops.ToString()
    | MapValues vals -> vals.ToString()

and CompositeTypeExt =
  | CompositeType of Choice<DateOnlyExt, DateTimeExt, GuidExt, TimeSpanExt>

  override self.ToString() : string =
    match self with
    | CompositeType(Choice1Of4 ct) -> ct.ToString()
    | CompositeType(Choice2Of4 ct) -> ct.ToString()
    | CompositeType(Choice3Of4 ct) -> ct.ToString()
    | CompositeType(Choice4Of4 ct) -> ct.ToString()

and ListExt =
  | ListOperations of List.Model.ListOperations<ValueExt>
  | ListValues of List.Model.ListValues<ValueExt>

  override self.ToString() : string =
    match self with
    | ListOperations ops -> ops.ToString()
    | ListValues vals -> vals.ToString()

and OptionExt =
  | OptionOperations of Option.Model.OptionOperations<ValueExt>
  | OptionValues of Option.Model.OptionValues<ValueExt>
  | OptionConstructors of Option.Model.OptionConstructors

  override self.ToString() : string =
    match self with
    | OptionOperations ops -> ops.ToString()
    | OptionValues vals -> vals.ToString()
    | OptionConstructors cons -> cons.ToString()

and DateOnlyExt =
  | DateOnlyOperations of DateOnly.Model.DateOnlyOperations<ValueExt>
  | DateOnlyConstructors of DateOnly.Model.DateOnlyConstructors

  override self.ToString() : string =
    match self with
    | DateOnlyOperations ops -> ops.ToString()
    | DateOnlyConstructors cons -> cons.ToString()

and DateTimeExt =
  | DateTimeOperations of DateTime.Model.DateTimeOperations<ValueExt>
  | DateTimeConstructors of DateTime.Model.DateTimeConstructors

  override self.ToString() : string =
    match self with
    | DateTimeOperations ops -> ops.ToString()
    | DateTimeConstructors cons -> cons.ToString()

and GuidExt =
  | GuidOperations of Guid.Model.GuidOperations<ValueExt>
  | GuidConstructors of Guid.Model.GuidConstructors

  override self.ToString() : string =
    match self with
    | GuidOperations ops -> ops.ToString()
    | GuidConstructors cons -> cons.ToString()

and TimeSpanExt =
  | TimeSpanOperations of TimeSpanOperations<ValueExt>
  | TimeSpanConstructors of TimeSpanConstructors

  override self.ToString() : string =
    match self with
    | TimeSpanOperations ops -> ops.ToString()
    | TimeSpanConstructors cons -> cons.ToString()

and PrimitiveExt =
  | BoolOperations of Bool.Model.BoolOperations<ValueExt>
  | Int32Operations of Int32.Model.Int32Operations<ValueExt>
  | Int64Operations of Int64.Model.Int64Operations<ValueExt>
  | Float32Operations of Float32.Model.Float32Operations<ValueExt>
  | Float64Operations of Float64.Model.Float64Operations<ValueExt>
  | DecimalOperations of Decimal.Model.DecimalOperations<ValueExt>
  | StringOperations of String.Model.StringOperations<ValueExt>

  override self.ToString() : string =
    match self with
    | BoolOperations ops -> ops.ToString()
    | Int32Operations ops -> ops.ToString()
    | Int64Operations ops -> ops.ToString()
    | Float32Operations ops -> ops.ToString()
    | Float64Operations ops -> ops.ToString()
    | DecimalOperations ops -> ops.ToString()
    | StringOperations ops -> ops.ToString()

type StdExtensions =
  { List: TypeExtension<ValueExt, Unit, List.Model.ListValues<ValueExt>, List.Model.ListOperations<ValueExt>>
    Bool: OperationsExtension<ValueExt, Bool.Model.BoolOperations<ValueExt>>
    Int32: OperationsExtension<ValueExt, Int32.Model.Int32Operations<ValueExt>>
    Int64: OperationsExtension<ValueExt, Int64.Model.Int64Operations<ValueExt>>
    Float32: OperationsExtension<ValueExt, Float32.Model.Float32Operations<ValueExt>>
    Float64: OperationsExtension<ValueExt, Float64.Model.Float64Operations<ValueExt>>
    Decimal: OperationsExtension<ValueExt, Decimal.Model.DecimalOperations<ValueExt>>
    DateOnly:
      TypeExtension<
        ValueExt,
        DateOnly.Model.DateOnlyConstructors,
        PrimitiveValue,
        DateOnly.Model.DateOnlyOperations<ValueExt>
       >
    DateTime:
      TypeExtension<
        ValueExt,
        DateTime.Model.DateTimeConstructors,
        PrimitiveValue,
        DateTime.Model.DateTimeOperations<ValueExt>
       >
    String: OperationsExtension<ValueExt, String.Model.StringOperations<ValueExt>>
    Guid: TypeExtension<ValueExt, Guid.Model.GuidConstructors, PrimitiveValue, Guid.Model.GuidOperations<ValueExt>>
    TimeSpan:
      TypeExtension<
        ValueExt,
        TimeSpan.Model.TimeSpanConstructors,
        PrimitiveValue,
        TimeSpan.Model.TimeSpanOperations<ValueExt>
       >
    Map: TypeExtension<ValueExt, Unit, Map.Model.MapValues<ValueExt>, Map.Model.MapOperations<ValueExt>> }

type ListExt with
  static member ValueLens =
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice1Of6(ListValues x) -> Some x
        | _ -> None)
      Set = ListValues >> Choice1Of6 >> ValueExt.ValueExt }

type MemoryDBExt with
  static member ValueLens =
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice5Of6(MemoryDBExt.MemoryDBValues x) -> Some x
        | _ -> None)
      Set = MemoryDBExt.MemoryDBValues >> Choice5Of6 >> ValueExt.ValueExt }

type OptionExt with
  static member ValueLens =
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of6(OptionValues x) -> Some x
        | _ -> None)
      Set = OptionValues >> Choice2Of6 >> ValueExt.ValueExt }

type MapExt with
  static member ValueLens =
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice6Of6(MapValues x) -> Some x
        | _ -> None)
      Set = MapValues >> Choice6Of6 >> ValueExt.ValueExt }

let stdExtensions =

  let memoryDBRunExtension =
    MemoryDB.Extension.MemoryDBRunExtension<ValueExt> MemoryDBExt.ValueLens

  let memoryDBGetByIdExtension =
    MemoryDB.Extension.MemoryDBGetByIdExtension<ValueExt> MemoryDBExt.ValueLens

  let memoryDBCUDExtension =
    MemoryDB.Extension.MemoryDBCUDExtension<ValueExt> MemoryDBExt.ValueLens

  let listExtension =
    List.Extension.ListExtension<ValueExt>
      ListExt.ValueLens
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice1Of6(ListOperations x) -> Some x
          | _ -> None)
        Set = ListOperations >> Choice1Of6 >> ValueExt.ValueExt }

  let optionExtension =
    Option.Extension.OptionExtension<ValueExt>
      OptionExt.ValueLens
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice2Of6(OptionConstructors x) -> Some x
          | _ -> None)
        Set = OptionConstructors >> Choice2Of6 >> ValueExt.ValueExt }
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice2Of6(OptionOperations x) -> Some x
          | _ -> None)
        Set = OptionOperations >> Choice2Of6 >> ValueExt.ValueExt }

  let dateOnlyExtension =
    DateOnly.Extension.DateOnlyExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice4Of6(CompositeType(Choice1Of4(DateOnlyConstructors x))) -> Some x
          | _ -> None)
        Set =
          DateOnlyConstructors
          >> Choice1Of4
          >> CompositeType
          >> Choice4Of6
          >> ValueExt.ValueExt }
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice4Of6(CompositeType(Choice1Of4(DateOnlyOperations x))) -> Some x
          | _ -> None)
        Set =
          DateOnlyOperations
          >> Choice1Of4
          >> CompositeType
          >> Choice4Of6
          >> ValueExt.ValueExt }

  let boolExtension =
    Bool.Extension.BoolExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of6(BoolOperations x) -> Some x
          | _ -> None)
        Set = BoolOperations >> Choice3Of6 >> ValueExt.ValueExt }

  let int32Extension =
    Int32.Extension.Int32Extension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of6(Int32Operations x) -> Some x
          | _ -> None)
        Set = Int32Operations >> Choice3Of6 >> ValueExt.ValueExt }

  let int64Extension =
    Int64.Extension.Int64Extension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of6(Int64Operations x) -> Some x
          | _ -> None)
        Set = Int64Operations >> Choice3Of6 >> ValueExt.ValueExt }

  let float32Extension =
    Float32.Extension.Float32Extension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of6(Float32Operations x) -> Some x
          | _ -> None)
        Set = Float32Operations >> Choice3Of6 >> ValueExt.ValueExt }

  let float64Extension =
    Float64.Extension.Float64Extension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of6(Float64Operations x) -> Some x
          | _ -> None)
        Set = Float64Operations >> Choice3Of6 >> ValueExt.ValueExt }

  let decimalExtension =
    Decimal.Extension.DecimalExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of6(DecimalOperations x) -> Some x
          | _ -> None)
        Set = DecimalOperations >> Choice3Of6 >> ValueExt.ValueExt }

  let dateTimeExtension =
    DateTime.Extension.DateTimeExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice4Of6(CompositeType(Choice2Of4(DateTimeConstructors x))) -> Some x
          | _ -> None)
        Set =
          DateTimeConstructors
          >> Choice2Of4
          >> CompositeType
          >> Choice4Of6
          >> ValueExt.ValueExt }
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice4Of6(CompositeType(Choice2Of4(DateTimeOperations x))) -> Some x
          | _ -> None)
        Set =
          DateTimeOperations
          >> Choice2Of4
          >> CompositeType
          >> Choice4Of6
          >> ValueExt.ValueExt }

  let timeSpanExtension =
    TimeSpanExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice4Of6(CompositeType(Choice4Of4(TimeSpanConstructors x))) -> Some x
          | _ -> None)
        Set =
          TimeSpanConstructors
          >> Choice4Of4
          >> CompositeType
          >> Choice4Of6
          >> ValueExt.ValueExt }
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice4Of6(CompositeType(Choice4Of4(TimeSpanOperations x))) -> Some x
          | _ -> None)
        Set =
          TimeSpanOperations
          >> Choice4Of4
          >> CompositeType
          >> Choice4Of6
          >> ValueExt.ValueExt }

  let stringExtension =
    String.Extension.StringExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of6(StringOperations x) -> Some x
          | _ -> None)
        Set = StringOperations >> Choice3Of6 >> ValueExt.ValueExt }

  let guidExtension =
    Guid.Extension.GuidExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice4Of6(CompositeType(Choice3Of4(GuidConstructors x))) -> Some x
          | _ -> None)
        Set =
          GuidConstructors
          >> Choice3Of4
          >> CompositeType
          >> Choice4Of6
          >> ValueExt.ValueExt }
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice4Of6(CompositeType(Choice3Of4(GuidOperations x))) -> Some x
          | _ -> None)
        Set = GuidOperations >> Choice3Of4 >> CompositeType >> Choice4Of6 >> ValueExt.ValueExt }

  let mapExtension =
    Map.Extension.MapExtension<ValueExt>
      MapExt.ValueLens
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice6Of6(MapOperations x) -> Some x
          | _ -> None)
        Set = MapOperations >> Choice6Of6 >> ValueExt.ValueExt }
      (Some ListExt.ValueLens)

  let context =
    LanguageContext<ValueExt>.Empty
    |> (memoryDBRunExtension |> TypeLambdaExtension.RegisterLanguageContext)
    |> (memoryDBGetByIdExtension |> OperationsExtension.RegisterLanguageContext)
    |> (memoryDBCUDExtension |> OperationsExtension.RegisterLanguageContext)
    |> (listExtension |> TypeExtension.RegisterLanguageContext)
    |> (optionExtension |> TypeExtension.RegisterLanguageContext)
    |> (dateOnlyExtension |> TypeExtension.RegisterLanguageContext)
    |> (dateTimeExtension |> TypeExtension.RegisterLanguageContext)
    |> (guidExtension |> TypeExtension.RegisterLanguageContext)
    |> (timeSpanExtension |> TypeExtension.RegisterLanguageContext)
    |> (boolExtension |> OperationsExtension.RegisterLanguageContext)
    |> (int32Extension |> OperationsExtension.RegisterLanguageContext)
    |> (int64Extension |> OperationsExtension.RegisterLanguageContext)
    |> (float32Extension |> OperationsExtension.RegisterLanguageContext)
    |> (float64Extension |> OperationsExtension.RegisterLanguageContext)
    |> (decimalExtension |> OperationsExtension.RegisterLanguageContext)
    |> (stringExtension |> OperationsExtension.RegisterLanguageContext)
    |> (mapExtension |> TypeExtension.RegisterLanguageContext)

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
    Map = mapExtension },
  context
