module Ballerina.DSL.Next.StdLib.Extensions

open Ballerina.DSL.Next.StdLib.TimeSpan
open Ballerina.Lenses
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.StdLib

type ValueExt =
  | ValueExt of Choice<ListExt, OptionExt, PrimitiveExt>

  override self.ToString() =
    match self with
    | ValueExt(Choice1Of3 ext) -> ext.ToString()
    | ValueExt(Choice2Of3 ext) -> ext.ToString()
    | ValueExt(Choice3Of3 ext) -> ext.ToString()

  static member Getters = {| ValueExt = fun (ValueExt e) -> e |}
  static member Updaters = {| ValueExt = fun u (ValueExt e) -> ValueExt(u e) |}

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

and PrimitiveExt =
  | BoolOperations of Bool.Model.BoolOperations<ValueExt>
  | Int32Operations of Int32.Model.Int32Operations<ValueExt>
  | Int64Operations of Int64.Model.Int64Operations<ValueExt>
  | Float32Operations of Float32.Model.Float32Operations<ValueExt>
  | Float64Operations of Float64.Model.Float64Operations<ValueExt>
  | DecimalOperations of Decimal.Model.DecimalOperations<ValueExt>
  | DateOnlyOperations of DateOnly.Model.DateOnlyOperations<ValueExt>
  | DateTimeOperations of DateTime.Model.DateTimeOperations<ValueExt>
  | TimeSpanOperations of TimeSpan.Model.TimeSpanOperations<ValueExt>
  | StringOperations of String.Model.StringOperations<ValueExt>
  | GuidOperations of Guid.Model.GuidOperations<ValueExt>

  override self.ToString() : string =
    match self with
    | BoolOperations ops -> ops.ToString()
    | Int32Operations ops -> ops.ToString()
    | Int64Operations ops -> ops.ToString()
    | Float32Operations ops -> ops.ToString()
    | Float64Operations ops -> ops.ToString()
    | DecimalOperations ops -> ops.ToString()
    | DateOnlyOperations ops -> ops.ToString()
    | DateTimeOperations ops -> ops.ToString()
    | TimeSpanOperations ops -> ops.ToString()
    | StringOperations ops -> ops.ToString()
    | GuidOperations ops -> ops.ToString()

type StdExtensions =
  { List: TypeExtension<ValueExt, Unit, List.Model.ListValues<ValueExt>, List.Model.ListOperations<ValueExt>>
    Bool: OperationsExtension<ValueExt, Bool.Model.BoolOperations<ValueExt>>
    Int32: OperationsExtension<ValueExt, Int32.Model.Int32Operations<ValueExt>>
    Int64: OperationsExtension<ValueExt, Int64.Model.Int64Operations<ValueExt>>
    Float32: OperationsExtension<ValueExt, Float32.Model.Float32Operations<ValueExt>>
    Float64: OperationsExtension<ValueExt, Float64.Model.Float64Operations<ValueExt>>
    Decimal: OperationsExtension<ValueExt, Decimal.Model.DecimalOperations<ValueExt>>
    DateOnly: OperationsExtension<ValueExt, DateOnly.Model.DateOnlyOperations<ValueExt>>
    DateTime: OperationsExtension<ValueExt, DateTime.Model.DateTimeOperations<ValueExt>>
    String: OperationsExtension<ValueExt, String.Model.StringOperations<ValueExt>>
    Guid: OperationsExtension<ValueExt, Guid.Model.GuidOperations<ValueExt>> }

let stdExtensions =

  let listExtension =
    List.Extension.ListExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice1Of3(ListValues x) -> Some x
          | _ -> None)
        Set = ListValues >> Choice1Of3 >> ValueExt.ValueExt }
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice1Of3(ListOperations x) -> Some x
          | _ -> None)
        Set = ListOperations >> Choice1Of3 >> ValueExt.ValueExt }

  let optionExtension =
    Option.Extension.OptionExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice2Of3(OptionValues x) -> Some x
          | _ -> None)
        Set = OptionValues >> Choice2Of3 >> ValueExt.ValueExt }
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice2Of3(OptionConstructors x) -> Some x
          | _ -> None)
        Set = OptionConstructors >> Choice2Of3 >> ValueExt.ValueExt }
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice2Of3(OptionOperations x) -> Some x
          | _ -> None)
        Set = OptionOperations >> Choice2Of3 >> ValueExt.ValueExt }

  let boolExtension =
    Bool.Extension.BoolExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of3(BoolOperations x) -> Some x
          | _ -> None)
        Set = BoolOperations >> Choice3Of3 >> ValueExt.ValueExt }

  let int32Extension =
    Int32.Extension.Int32Extension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of3(Int32Operations x) -> Some x
          | _ -> None)
        Set = Int32Operations >> Choice3Of3 >> ValueExt.ValueExt }

  let int64Extension =
    Int64.Extension.Int64Extension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of3(Int64Operations x) -> Some x
          | _ -> None)
        Set = Int64Operations >> Choice3Of3 >> ValueExt.ValueExt }

  let float32Extension =
    Float32.Extension.Float32Extension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of3(Float32Operations x) -> Some x
          | _ -> None)
        Set = Float32Operations >> Choice3Of3 >> ValueExt.ValueExt }

  let float64Extension =
    Float64.Extension.Float64Extension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of3(Float64Operations x) -> Some x
          | _ -> None)
        Set = Float64Operations >> Choice3Of3 >> ValueExt.ValueExt }

  let decimalExtension =
    Decimal.Extension.DecimalExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of3(DecimalOperations x) -> Some x
          | _ -> None)
        Set = DecimalOperations >> Choice3Of3 >> ValueExt.ValueExt }

  let dateOnlyExtension =
    DateOnly.Extension.DateOnlyExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of3(DateOnlyOperations x) -> Some x
          | _ -> None)
        Set = DateOnlyOperations >> Choice3Of3 >> ValueExt.ValueExt }

  let dateTimeExtension =
    DateTime.Extension.DateTimeExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of3(DateTimeOperations x) -> Some x
          | _ -> None)
        Set = DateTimeOperations >> Choice3Of3 >> ValueExt.ValueExt }

  let timeSpanExtension =
    TimeSpanExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of3(TimeSpanOperations x) -> Some x
          | _ -> None)
        Set = TimeSpanOperations >> Choice3Of3 >> ValueExt.ValueExt }

  let stringExtension =
    String.Extension.StringExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of3(StringOperations x) -> Some x
          | _ -> None)
        Set = StringOperations >> Choice3Of3 >> ValueExt.ValueExt }

  let guidExtension =
    Guid.Extension.GuidExtension<ValueExt>
      { Get =
          ValueExt.Getters.ValueExt
          >> (function
          | Choice3Of3(GuidOperations x) -> Some x
          | _ -> None)
        Set = GuidOperations >> Choice3Of3 >> ValueExt.ValueExt }

  let context =
    LanguageContext<ValueExt>.Empty
    |> (listExtension |> TypeExtension.RegisterLanguageContext)
    |> (optionExtension |> TypeExtension.RegisterLanguageContext)
    |> (boolExtension |> OperationsExtension.RegisterLanguageContext)
    |> (int32Extension |> OperationsExtension.RegisterLanguageContext)
    |> (int64Extension |> OperationsExtension.RegisterLanguageContext)
    |> (float32Extension |> OperationsExtension.RegisterLanguageContext)
    |> (float64Extension |> OperationsExtension.RegisterLanguageContext)
    |> (decimalExtension |> OperationsExtension.RegisterLanguageContext)
    |> (dateOnlyExtension |> OperationsExtension.RegisterLanguageContext)
    |> (dateTimeExtension |> OperationsExtension.RegisterLanguageContext)
    |> (timeSpanExtension |> OperationsExtension.RegisterLanguageContext)
    |> (stringExtension |> OperationsExtension.RegisterLanguageContext)
    |> (guidExtension |> OperationsExtension.RegisterLanguageContext)

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
    Guid = guidExtension },
  context
