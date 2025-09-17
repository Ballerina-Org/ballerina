namespace ExtensionsBootstrapForTesting

module TestExt =

  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.List
  open Ballerina.DSL.Next.StdLib.Option
  open Ballerina.DSL.Next.StdLib.Int32
  open Ballerina.DSL.Next.StdLib.Int64
  open Ballerina.DSL.Next.StdLib.Float32
  open Ballerina.DSL.Next.StdLib.Float64
  open Ballerina.DSL.Next.StdLib.Decimal
  open Ballerina.DSL.Next.StdLib.DateOnly
  open Ballerina.DSL.Next.StdLib.DateTime
  open Ballerina.DSL.Next.StdLib.String
  open Ballerina.DSL.Next.StdLib.Guid
  open Ballerina.DSL.Next.StdLib.Bool

  type ValueExt =
    | ValueExt of Choice<ListExt, OptionExt, PrimitiveExt>

    static member Getters = {| ValueExt = fun (ValueExt e) -> e |}
    static member Updaters = {| ValueExt = fun u (ValueExt e) -> ValueExt(u e) |}

  and ListExt =
    | ListOperations of ListOperations<ValueExt>
    | ListValues of ListValues<ValueExt>

  and OptionExt =
    | OptionOperations of OptionOperations<ValueExt>
    | OptionValues of OptionValues<ValueExt>
    | OptionConstructors of OptionConstructors

  and PrimitiveExt =
    | Int32Operations of Int32Operations<ValueExt>
    | Int64Operations of Int64Operations<ValueExt>
    | Float32Operations of Float32Operations<ValueExt>
    | Float64Operations of Float64Operations<ValueExt>
    | DecimalOperations of DecimalOperations<ValueExt>
    | DateOnlyOperations of DateOnlyOperations<ValueExt>
    | DateTimeOperations of DateTimeOperations<ValueExt>
    | StringOperations of StringOperations<ValueExt>
    | GuidOperations of GuidOperations<ValueExt>
    | BoolOperations of BoolOperations<ValueExt>

  type TestExtensions =
    { List: TypeExtension<ValueExt, Unit, ListValues<ValueExt>, ListOperations<ValueExt>>
      Bool: OperationsExtension<ValueExt, BoolOperations<ValueExt>>
      Int32: OperationsExtension<ValueExt, Int32Operations<ValueExt>>
      Int64: OperationsExtension<ValueExt, Int64Operations<ValueExt>>
      Float32: OperationsExtension<ValueExt, Float32Operations<ValueExt>>
      Float64: OperationsExtension<ValueExt, Float64Operations<ValueExt>>
      Decimal: OperationsExtension<ValueExt, DecimalOperations<ValueExt>>
      DateOnly: OperationsExtension<ValueExt, DateOnlyOperations<ValueExt>>
      DateTime: OperationsExtension<ValueExt, DateTimeOperations<ValueExt>>
      String: OperationsExtension<ValueExt, StringOperations<ValueExt>>
      Guid: OperationsExtension<ValueExt, GuidOperations<ValueExt>> }

  let testExtensions =
    let listExtension =
      ListExtension<ValueExt>
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

    let boolExtension =
      BoolExtension<ValueExt>
        { Get =
            ValueExt.Getters.ValueExt
            >> (function
            | Choice3Of3(BoolOperations x) -> Some x
            | _ -> None)
          Set = BoolOperations >> Choice3Of3 >> ValueExt.ValueExt }

    let int32Extension =
      Int32Extension<ValueExt>
        { Get =
            ValueExt.Getters.ValueExt
            >> (function
            | Choice3Of3(Int32Operations x) -> Some x
            | _ -> None)
          Set = Int32Operations >> Choice3Of3 >> ValueExt.ValueExt }

    let int64Extension =
      Int64Extension<ValueExt>
        { Get =
            ValueExt.Getters.ValueExt
            >> (function
            | Choice3Of3(Int64Operations x) -> Some x
            | _ -> None)
          Set = Int64Operations >> Choice3Of3 >> ValueExt.ValueExt }

    let float32Extension =
      Float32Extension<ValueExt>
        { Get =
            ValueExt.Getters.ValueExt
            >> (function
            | Choice3Of3(Float32Operations x) -> Some x
            | _ -> None)
          Set = Float32Operations >> Choice3Of3 >> ValueExt.ValueExt }

    let float64Extension =
      Float64Extension<ValueExt>
        { Get =
            ValueExt.Getters.ValueExt
            >> (function
            | Choice3Of3(Float64Operations x) -> Some x
            | _ -> None)
          Set = Float64Operations >> Choice3Of3 >> ValueExt.ValueExt }

    let decimalExtension =
      DecimalExtension<ValueExt>
        { Get =
            ValueExt.Getters.ValueExt
            >> (function
            | Choice3Of3(DecimalOperations x) -> Some x
            | _ -> None)
          Set = DecimalOperations >> Choice3Of3 >> ValueExt.ValueExt }

    let dateOnlyExtension =
      DateOnlyExtension<ValueExt>
        { Get =
            ValueExt.Getters.ValueExt
            >> (function
            | Choice3Of3(DateOnlyOperations x) -> Some x
            | _ -> None)
          Set = DateOnlyOperations >> Choice3Of3 >> ValueExt.ValueExt }

    let dateTimeExtension =
      DateTimeExtension<ValueExt>
        { Get =
            ValueExt.Getters.ValueExt
            >> (function
            | Choice3Of3(DateTimeOperations x) -> Some x
            | _ -> None)
          Set = DateTimeOperations >> Choice3Of3 >> ValueExt.ValueExt }

    let stringExtension =
      StringExtension<ValueExt>
        { Get =
            ValueExt.Getters.ValueExt
            >> (function
            | Choice3Of3(StringOperations x) -> Some x
            | _ -> None)
          Set = StringOperations >> Choice3Of3 >> ValueExt.ValueExt }

    let guidExtension =
      GuidExtension<ValueExt>
        { Get =
            ValueExt.Getters.ValueExt
            >> (function
            | Choice3Of3(GuidOperations x) -> Some x
            | _ -> None)
          Set = GuidOperations >> Choice3Of3 >> ValueExt.ValueExt }

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
      Guid = guidExtension }
