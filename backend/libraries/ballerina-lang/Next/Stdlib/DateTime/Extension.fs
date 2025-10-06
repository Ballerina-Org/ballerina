namespace Ballerina.DSL.Next.StdLib.DateTime

[<AutoOpen>]
module Extension =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.Option
  open System

  let private dateTimeTypeValue = TypeValue.CreateDateTime()
  let private dateOnlyTypeValue = TypeValue.CreateDateOnly()
  let private timeSpanTypeValue = TypeValue.CreateTimeSpan()
  let private boolTypeValue = TypeValue.CreateBool()
  let private int32TypeValue = TypeValue.CreateInt32()


  let DateTimeExtension<'ext>
    (operationLens: PartialLens<'ext, DateTimeOperations<'ext>>)
    : OperationsExtension<'ext, DateTimeOperations<'ext>> =

    let dateTimeDiffId = Identifier.FullyQualified([ "DateTime" ], "-")

    let diffOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeDiffId,
      { Type = TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, timeSpanTypeValue))
        Kind = Kind.Star
        Operation = DateTimeOperations.Diff {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.Diff v -> Some(DateTimeOperations.Diff v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsDiff |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.Diff({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.TimeSpan(vClosure - v))
            } }

    let dateTimeEqualId = Identifier.FullyQualified([ "DateTime" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeEqualId,
      { Type = TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateTimeOperations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.Equal v -> Some(DateTimeOperations.Equal v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let dateTimeNotEqualId = Identifier.FullyQualified([ "DateTime" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeNotEqualId,
      { Type = TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateTimeOperations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.NotEqual v -> Some(DateTimeOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsNotEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let dateTimeGreaterThanId = Identifier.FullyQualified([ "DateTime" ], ">")

    let greaterThanOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeGreaterThanId,
      { Type = TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateTimeOperations.GreaterThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.GreaterThan v -> Some(DateTimeOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsGreaterThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let dateTimeGreaterThanOrEqualId = Identifier.FullyQualified([ "DateTime" ], ">=")

    let greaterThanOrEqualOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeGreaterThanOrEqualId,
      { Type = TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateTimeOperations.GreaterThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.GreaterThanOrEqual v -> Some(DateTimeOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsGreaterThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DateTimeOperations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let dateTimeLessThanId = Identifier.FullyQualified([ "DateTime" ], "<")

    let lessThanOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeLessThanId,
      { Type = TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateTimeOperations.LessThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.LessThan v -> Some(DateTimeOperations.LessThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsLessThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let dateTimeLessThanOrEqualId = Identifier.FullyQualified([ "DateTime" ], "<=")

    let lessThanOrEqualOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeLessThanOrEqualId,
      { Type = TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateTimeOperations.LessThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.LessThanOrEqual v -> Some(DateTimeOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsLessThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DateTimeOperations.LessThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    let dateTimeToDateOnlyId = Identifier.FullyQualified([ "DateTime" ], "toDateOnly")

    let toDateOnlyOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeToDateOnlyId,
      { Type = TypeValue.CreateArrow(dateTimeTypeValue, dateOnlyTypeValue)
        Kind = Kind.Star
        Operation = DateTimeOperations.ToDateOnly
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.ToDateOnly -> Some(DateTimeOperations.ToDateOnly)
            | _ -> None)
        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Date(System.DateOnly(v.Year, v.Month, v.Day)))
            } }

    let dateOnlyYearId = Identifier.FullyQualified([ "DateTime" ], "getYear")

    let yearOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateOnlyYearId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = DateTimeOperations.Year
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.Year -> Some(DateTimeOperations.Year)
            | _ -> None)
        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.Year))
            } }

    let dateOnlyMonthId = Identifier.FullyQualified([ "DateOnly" ], "getMonth")

    let monthOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateOnlyMonthId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = DateTimeOperations.Month
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.Month -> Some(DateTimeOperations.Month)
            | _ -> None)
        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.Month))
            } }

    let dateOnlyDayId = Identifier.FullyQualified([ "DateOnly" ], "getDay")

    let dayOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateOnlyDayId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = DateTimeOperations.Day
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.Day -> Some(DateTimeOperations.Day)
            | _ -> None)
        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.Day))
            } }

    let dateOnlyDayOfWeekId = Identifier.FullyQualified([ "DateOnly" ], "getDayOfWeek")

    let dayOfWeekOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateOnlyDayOfWeekId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = DateTimeOperations.DayOfWeek
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.DayOfWeek -> Some(DateTimeOperations.DayOfWeek)
            | _ -> None)
        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.DayOfWeek |> int))
            } }

    let dateOnlyDayOfYearId = Identifier.FullyQualified([ "DateOnly" ], "getDayOfYear")

    let dayOfYearOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateOnlyDayOfYearId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = DateTimeOperations.DayOfYear
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.DayOfYear -> Some(DateTimeOperations.DayOfYear)
            | _ -> None)
        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.DayOfYear))
            } }

    { TypeVars = []
      Operations =
        [ diffOperation
          equalOperation
          notEqualOperation
          greaterThanOperation
          greaterThanOrEqualOperation
          lessThanOperation
          lessThanOrEqualOperation
          toDateOnlyOperation
          yearOperation
          monthOperation
          dayOperation
          dayOfWeekOperation
          dayOfYearOperation ]
        |> Map.ofList }
