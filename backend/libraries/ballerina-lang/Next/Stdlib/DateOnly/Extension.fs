namespace Ballerina.DSL.Next.StdLib.DateOnly

[<AutoOpen>]
module Extension =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions

  let private dateTimeTypeValue = TypeValue.CreateDateTime()
  let private dateOnlyTypeValue = TypeValue.CreateDateOnly()
  let private timeSpanTypeValue = TypeValue.CreateTimeSpan()
  let private boolTypeValue = TypeValue.CreateBool()
  let private int32TypeValue = TypeValue.CreateInt32()

  let DateOnlyExtension<'ext>
    (operationLens: PartialLens<'ext, DateOnlyOperations<'ext>>)
    : OperationsExtension<'ext, DateOnlyOperations<'ext>> =

    let dateOnlyDiffId = Identifier.FullyQualified([ "dateOnly" ], "-")

    let diffOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyDiffId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, timeSpanTypeValue))
        Kind = Kind.Star
        Operation = DateOnlyOperations.Diff {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.Diff v -> Some(DateOnlyOperations.Diff v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsDiff
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateOnlyOperations.Diff({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let dateTime1 = System.DateTime(vClosure.Year, vClosure.Month, vClosure.Day)
                let dateTime2 = System.DateTime(v.Year, v.Month, v.Day)
                let difference = dateTime1 - dateTime2
                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.TimeSpan(difference))
            } }

    let dateOnlyEqualId = Identifier.FullyQualified([ "dateOnly" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyEqualId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateOnlyOperations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.Equal v -> Some(DateOnlyOperations.Equal v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateOnlyOperations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let dateOnlyNotEqualId = Identifier.FullyQualified([ "dateOnly" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyNotEqualId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateOnlyOperations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.NotEqual v -> Some(DateOnlyOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsNotEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateOnlyOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let dateOnlyGreaterThanId = Identifier.FullyQualified([ "dateOnly" ], ">")

    let greaterThanOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyGreaterThanId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateOnlyOperations.GreaterThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.GreaterThan v -> Some(DateOnlyOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsGreaterThan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateOnlyOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let dateOnlyGreaterThanOrEqualId = Identifier.FullyQualified([ "dateOnly" ], ">=")

    let greaterThanOrEqualOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyGreaterThanOrEqualId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateOnlyOperations.GreaterThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.GreaterThanOrEqual v -> Some(DateOnlyOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsGreaterThanOrEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DateOnlyOperations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let dateOnlyLessThanId = Identifier.FullyQualified([ "dateOnly" ], "<")

    let lessThanOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyLessThanId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateOnlyOperations.LessThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.LessThan v -> Some(DateOnlyOperations.LessThan v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsLessThan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateOnlyOperations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let dateOnlyLessThanOrEqualId = Identifier.FullyQualified([ "dateOnly" ], "<=")

    let lessThanOrEqualOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyLessThanOrEqualId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateOnlyOperations.LessThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.LessThanOrEqual v -> Some(DateOnlyOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsLessThanOrEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DateOnlyOperations.LessThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }


    let dateOnlyToDateTimeId = Identifier.FullyQualified([ "dateOnly" ], "toDateTime")

    let toDateTimeOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyToDateTimeId,
      { Type =
          TypeValue.CreateArrow(
            dateOnlyTypeValue,
            TypeValue.CreateArrow(
              TypeValue.CreateTuple [ int32TypeValue; int32TypeValue; int32TypeValue ],
              dateTimeTypeValue
            )
          )
        Kind = Kind.Star
        Operation = DateOnlyOperations.ToDateTime {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.ToDateTime v -> Some(DateOnlyOperations.ToDateTime v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsToDateTime
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op, v with
              | None, Value.Primitive(PrimitiveValue.Date v) -> // the closure is empty - first step in the application
                return DateOnlyOperations.ToDateTime {| v1 = Some(v) |} |> operationLens.Set |> Ext
              | Some(vClosure), Value.Tuple v -> // the closure has the first operand - second step in the application
                let! v =
                  v
                  |> List.map (fun v -> v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum)
                  |> reader.All

                let! v =
                  v
                  |> List.map (fun v ->
                    v
                    |> PrimitiveValue.AsInt32
                    |> sum.MapError(Errors.FromErrors loc0)
                    |> reader.OfSum)
                  |> reader.All

                match v with
                | [ v1; v2; v3 ] ->
                  let dateTime =
                    System.DateTime(vClosure.Year, vClosure.Month, vClosure.Day, v1, v2, v3)

                  return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.DateTime(dateTime))
                | _ ->
                  return!
                    sum.Throw(Errors.Singleton(loc0, "Expected a tuple of 3 int32s"))
                    |> reader.OfSum
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a tuple or date")) |> reader.OfSum
            } }

    let dateOnlyYearId = Identifier.FullyQualified([ "dateOnly" ], "getYear")

    let yearOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyYearId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = DateOnlyOperations.Year
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.Year -> Some(DateOnlyOperations.Year)
            | _ -> None)
        Apply =
          fun loc0 (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.Year))
            } }

    let dateOnlyMonthId = Identifier.FullyQualified([ "dateOnly" ], "getMonth")

    let monthOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyMonthId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = DateOnlyOperations.Month
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.Month -> Some(DateOnlyOperations.Month)
            | _ -> None)
        Apply =
          fun loc0 (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.Month))
            } }

    let dateOnlyDayId = Identifier.FullyQualified([ "dateOnly" ], "getDay")

    let dayOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyDayId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = DateOnlyOperations.Day
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.Day -> Some(DateOnlyOperations.Day)
            | _ -> None)
        Apply =
          fun loc0 (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.Day))
            } }

    let dateOnlyDayOfWeekId = Identifier.FullyQualified([ "dateOnly" ], "getDayOfWeek")

    let dayOfWeekOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyDayOfWeekId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = DateOnlyOperations.DayOfWeek
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.DayOfWeek -> Some(DateOnlyOperations.DayOfWeek)
            | _ -> None)
        Apply =
          fun loc0 (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.DayOfWeek |> int))
            } }

    let dateOnlyDayOfYearId = Identifier.FullyQualified([ "dateOnly" ], "getDayOfYear")

    let dayOfYearOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyDayOfYearId,
      { Type = TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = DateOnlyOperations.DayOfYear
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.DayOfYear -> Some(DateOnlyOperations.DayOfYear)
            | _ -> None)
        Apply =
          fun loc0 (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

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
          toDateTimeOperation
          yearOperation
          monthOperation
          dayOperation
          dayOfWeekOperation
          dayOfYearOperation ]
        |> Map.ofList }
