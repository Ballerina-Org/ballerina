namespace Ballerina.DSL.Next.StdLib.DateOnly

[<AutoOpen>]
module Extension =
  open Ballerina.StdLib.Object
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open System
  open Ballerina
  open Ballerina.StdLib.Formats

  let DateOnlyExtension<'ext, 'extDTO when 'extDTO: not null and 'extDTO: not struct>
    (operationLens: PartialLens<'ext, DateOnlyOperations<'ext>>)
    : OperationsExtension<'ext, DateOnlyOperations<'ext>> =

    let dateTimeTypeValue = TypeValue.CreateDateTime()
    let dateOnlyTypeValue = TypeValue.CreateDateOnly()
    let timeSpanTypeValue = TypeValue.CreateTimeSpan()
    let boolTypeValue = TypeValue.CreateBool()
    let int32TypeValue = TypeValue.CreateInt32()

    let dateOnlyDiffId =
      Identifier.FullyQualified([ "dateOnly" ], "-") |> TypeCheckScope.Empty.Resolve

    let diffOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyDiffId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, timeSpanTypeValue)),
            Kind.Star,
            DateOnlyOperations.Diff {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.Diff v -> Some(DateOnlyOperations.Diff v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsDiff
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateOnlyOperations.Diff({| v1 = Some v |}) |> operationLens.Set, Some dateOnlyDiffId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let dateTime1 = DateTime(vClosure.Year, vClosure.Month, vClosure.Day)
                let dateTime2 = DateTime(v.Year, v.Month, v.Day)
                let difference = dateTime1 - dateTime2
                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.TimeSpan(difference))
            } }

    let dateOnlyEqualId =
      Identifier.FullyQualified([ "dateOnly" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue)),
            Kind.Star,
            DateOnlyOperations.Equal {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.Equal v -> Some(DateOnlyOperations.Equal v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateOnlyOperations.Equal({| v1 = Some v |}) |> operationLens.Set, Some dateOnlyEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let dateOnlyNotEqualId =
      Identifier.FullyQualified([ "dateOnly" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyNotEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue)),
            Kind.Star,
            DateOnlyOperations.NotEqual {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.NotEqual v -> Some(DateOnlyOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsNotEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateOnlyOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set, Some dateOnlyNotEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let dateOnlyGreaterThanId =
      Identifier.FullyQualified([ "dateOnly" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyGreaterThanId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue)),
            Kind.Star,
            DateOnlyOperations.GreaterThan {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.GreaterThan v -> Some(DateOnlyOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsGreaterThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateOnlyOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set, Some dateOnlyGreaterThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let dateOnlyGreaterThanOrEqualId =
      Identifier.FullyQualified([ "dateOnly" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyGreaterThanOrEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue)),
            Kind.Star,
            DateOnlyOperations.GreaterThanOrEqual {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.GreaterThanOrEqual v -> Some(DateOnlyOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsGreaterThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateOnlyOperations.GreaterThanOrEqual({| v1 = Some v |}) |> operationLens.Set,
                   Some dateOnlyGreaterThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let dateOnlyLessThanId =
      Identifier.FullyQualified([ "dateOnly" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyLessThanId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue)),
            Kind.Star,
            DateOnlyOperations.LessThan {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.LessThan v -> Some(DateOnlyOperations.LessThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsLessThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateOnlyOperations.LessThan({| v1 = Some v |}) |> operationLens.Set, Some dateOnlyLessThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let dateOnlyLessThanOrEqualId =
      Identifier.FullyQualified([ "dateOnly" ], "<=") |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyLessThanOrEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateOnlyTypeValue, TypeValue.CreateArrow(dateOnlyTypeValue, boolTypeValue)),
            Kind.Star,
            DateOnlyOperations.LessThanOrEqual {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.LessThanOrEqual v -> Some(DateOnlyOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsLessThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateOnlyOperations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set,
                   Some dateOnlyLessThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }


    let dateOnlyToDateTimeId =
      Identifier.FullyQualified([ "dateOnly" ], "toDateTime")
      |> TypeCheckScope.Empty.Resolve

    let toDateTimeOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyToDateTimeId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(
              dateOnlyTypeValue,
              TypeValue.CreateArrow(
                TypeValue.CreateTuple [ int32TypeValue; int32TypeValue; int32TypeValue ],
                dateTimeTypeValue
              )
            ),
            Kind.Star,
            DateOnlyOperations.ToDateTime {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.ToDateTime v -> Some(DateOnlyOperations.ToDateTime v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateOnlyOperations.AsToDateTime
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op, v with
              | None, Value.Primitive(PrimitiveValue.Date v) -> // the closure is empty - first step in the application
                return
                  (DateOnlyOperations.ToDateTime {| v1 = Some(v) |} |> operationLens.Set, Some dateOnlyToDateTimeId)
                  |> Ext
              | Some(vClosure), Value.Tuple v -> // the closure has the first operand - second step in the application
                let! v =
                  v
                  |> List.map (fun v ->
                    v
                    |> Value.AsPrimitive
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum)
                  |> reader.All

                let! v =
                  v
                  |> List.map (fun v ->
                    v
                    |> PrimitiveValue.AsInt32
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum)
                  |> reader.All

                match v with
                | [ v1; v2; v3 ] ->
                  let dateTime =
                    DateTime(vClosure.Year, vClosure.Month, vClosure.Day, v1, v2, v3, DateTimeKind.Utc)

                  return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.DateTime(dateTime))
                | _ ->
                  return!
                    sum.Throw(Errors.Singleton loc0 (fun () -> "Expected a tuple of 3 int32s"))
                    |> reader.OfSum
              | _ ->
                return!
                  sum.Throw(Errors.Singleton loc0 (fun () -> "Expected a tuple or date"))
                  |> reader.OfSum
            } }

    let dateOnlyYearId =
      Identifier.FullyQualified([ "dateOnly" ], "getYear")
      |> TypeCheckScope.Empty.Resolve

    let yearOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyYearId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue), Kind.Star, DateOnlyOperations.Year)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.Year -> Some(DateOnlyOperations.Year)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int32(v.Year))
            } }

    let dateOnlyMonthId =
      Identifier.FullyQualified([ "dateOnly" ], "getMonth")
      |> TypeCheckScope.Empty.Resolve

    let monthOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyMonthId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue), Kind.Star, DateOnlyOperations.Month)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.Month -> Some(DateOnlyOperations.Month)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int32(v.Month))
            } }

    let dateOnlyDayId =
      Identifier.FullyQualified([ "dateOnly" ], "getDay")
      |> TypeCheckScope.Empty.Resolve

    let dayOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyDayId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue), Kind.Star, DateOnlyOperations.Day)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.Day -> Some(DateOnlyOperations.Day)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int32(v.Day))
            } }

    let dateOnlyDayOfWeekId =
      Identifier.FullyQualified([ "dateOnly" ], "getDayOfWeek")
      |> TypeCheckScope.Empty.Resolve

    let dayOfWeekOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyDayOfWeekId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue), Kind.Star, DateOnlyOperations.DayOfWeek)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.DayOfWeek -> Some(DateOnlyOperations.DayOfWeek)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int32(v.DayOfWeek |> int))
            } }

    let dateOnlyDayOfYearId =
      Identifier.FullyQualified([ "dateOnly" ], "getDayOfYear")
      |> TypeCheckScope.Empty.Resolve

    let dayOfYearOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyDayOfYearId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue), Kind.Star, DateOnlyOperations.DayOfYear)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.DayOfYear -> Some(DateOnlyOperations.DayOfYear)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int32(v.DayOfYear))
            } }

    let dateOnlyNewId =
      Identifier.FullyQualified([ "dateOnly" ], "new") |> TypeCheckScope.Empty.Resolve

    let newOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyNewId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(
              TypeValue.CreatePrimitive PrimitiveType.String,
              TypeValue.CreateSum [ TypeValue.CreatePrimitive PrimitiveType.Unit; dateOnlyTypeValue ]
            ),
            Kind.Star,
            DateOnlyOperations.DateOnly_New
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.DateOnly_New -> Some(DateOnlyOperations.DateOnly_New)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return
                match Iso8601.DateOnly.tryParse v with
                | Some date -> Value.Sum({ Case = 2; Count = 2 }, date |> PrimitiveValue.Date |> Value.Primitive)
                | None -> Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Unit))
            } }

    let dateOnlyNowId =
      Identifier.FullyQualified([ "dateOnly" ], "now") |> TypeCheckScope.Empty.Resolve

    let nowOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyNowId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(TypeValue.CreatePrimitive PrimitiveType.Unit, dateOnlyTypeValue),
            Kind.Star,
            DateOnlyOperations.DateOnly_Now
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.DateOnly_Now -> Some(DateOnlyOperations.DateOnly_Now)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! _ =
                v
                |> PrimitiveValue.AsUnit
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let date = System.DateTime.Now
              let dateOnly = System.DateOnly(date.Year, date.Month, date.Day)
              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Date dateOnly)
            } }

    let dateOnlyUTCNowId =
      Identifier.FullyQualified([ "dateOnly" ], "utcNow")
      |> TypeCheckScope.Empty.Resolve

    let utcNowOperation: ResolvedIdentifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyUTCNowId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(TypeValue.CreatePrimitive PrimitiveType.Unit, dateOnlyTypeValue),
            Kind.Star,
            DateOnlyOperations.DateOnly_UTCNow
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.DateOnly_UTCNow -> Some(DateOnlyOperations.DateOnly_UTCNow)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! _ =
                v
                |> PrimitiveValue.AsUnit
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let date = System.DateTime.UtcNow
              let dateOnly = System.DateOnly(date.Year, date.Month, date.Day)
              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Date dateOnly)
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
          dayOfYearOperation
          newOperation
          nowOperation
          utcNowOperation ]
        |> Map.ofList }
