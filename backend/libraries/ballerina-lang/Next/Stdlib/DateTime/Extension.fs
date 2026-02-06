namespace Ballerina.DSL.Next.StdLib.DateTime

[<AutoOpen>]
module Extension =
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
  open Ballerina.StdLib.Formats

  let DateTimeExtension<'ext>
    (operationLens: PartialLens<'ext, DateTimeOperations<'ext>>)
    : OperationsExtension<'ext, DateTimeOperations<'ext>> =

    let dateTimeTypeValue = TypeValue.CreateDateTime()
    let dateOnlyTypeValue = TypeValue.CreateDateOnly()
    let timeSpanTypeValue = TypeValue.CreateTimeSpan()
    let boolTypeValue = TypeValue.CreateBool()
    let int32TypeValue = TypeValue.CreateInt32()

    let dateTimeNewId =
      Identifier.FullyQualified([ "dateTime" ], "new") |> TypeCheckScope.Empty.Resolve

    let dateTimeNew: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeNewId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(
              TypeValue.CreatePrimitive PrimitiveType.String,
              TypeValue.CreateSum
                [ TypeValue.CreatePrimitive PrimitiveType.Unit
                  TypeValue.CreatePrimitive PrimitiveType.DateTime ]
            ),
            Kind.Star,
            DateTimeOperations.DateTime_New
          )
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.String v) ->
                return
                  match Iso8601.DateTime.tryParse v with
                  | Some date -> Value.Sum({ Case = 0; Count = 1 }, date |> PrimitiveValue.DateTime |> Value.Primitive)
                  | None -> Value.Sum({ Case = 1; Count = 1 }, Value.Primitive(PrimitiveValue.Unit))
              | _ -> return! sum.Throw(Errors.Singleton loc0 (fun () -> "Expected a string")) |> reader.OfSum
            }
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTime_New -> Some DateTime_New
            | _ -> None) }

    let dateTimeNowId =
      Identifier.FullyQualified([ "dateTime" ], "now") |> TypeCheckScope.Empty.Resolve

    let dateTimeNow: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeNowId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(
              TypeValue.CreatePrimitive PrimitiveType.Unit,
              TypeValue.CreatePrimitive PrimitiveType.DateTime
            ),
            Kind.Star,
            DateTimeOperations.DateTime_Now
          )
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.Unit) ->
                return Value.Primitive(PrimitiveValue.DateTime System.DateTime.Now)
              | _ ->
                return!
                  sum.Throw(Errors.Singleton loc0 (fun () -> "Expected a unit (now)"))
                  |> reader.OfSum
            }
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTime_Now -> Some DateTime_Now
            | _ -> None) }

    let dateTimeUTCNowId =
      Identifier.FullyQualified([ "dateTime" ], "utcNow")
      |> TypeCheckScope.Empty.Resolve

    let dateTimeUTCNow: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeUTCNowId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(
              TypeValue.CreatePrimitive PrimitiveType.Unit,
              TypeValue.CreatePrimitive PrimitiveType.DateTime
            ),
            Kind.Star,
            DateTimeOperations.DateTime_UTCNow
          )
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.Unit) ->
                return Value.Primitive(PrimitiveValue.DateTime System.DateTime.UtcNow)
              | _ ->
                return!
                  sum.Throw(Errors.Singleton loc0 (fun () -> "Expected a unit (UTC now)"))
                  |> reader.OfSum
            }
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTime_UTCNow -> Some DateTime_UTCNow
            | _ -> None) }

    let dateTimeDiffId =
      Identifier.FullyQualified([ "dateTime" ], "-") |> TypeCheckScope.Empty.Resolve

    let diffOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeDiffId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, timeSpanTypeValue)),
            Kind.Star,
            DateTimeOperations.Diff {| v1 = None |}
          )

        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.Diff v -> Some(DateTimeOperations.Diff v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsDiff
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateTimeOperations.Diff({| v1 = Some v |}) |> operationLens.Set, Some dateTimeDiffId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.TimeSpan(vClosure - v))
            } }

    let dateTimeEqualId =
      Identifier.FullyQualified([ "dateTime" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue)),
            Kind.Star,
            DateTimeOperations.Equal {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.Equal v -> Some(DateTimeOperations.Equal v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateTimeOperations.Equal({| v1 = Some v |}) |> operationLens.Set, Some dateTimeEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let dateTimeNotEqualId =
      Identifier.FullyQualified([ "dateTime" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeNotEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue)),
            Kind.Star,
            DateTimeOperations.NotEqual {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.NotEqual v -> Some(DateTimeOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsNotEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateTimeOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set, Some dateTimeNotEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let dateTimeGreaterThanId =
      Identifier.FullyQualified([ "dateTime" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeGreaterThanId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue)),
            Kind.Star,
            DateTimeOperations.GreaterThan {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.GreaterThan v -> Some(DateTimeOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsGreaterThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateTimeOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set, Some dateTimeGreaterThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let dateTimeGreaterThanOrEqualId =
      Identifier.FullyQualified([ "dateTime" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeGreaterThanOrEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue)),
            Kind.Star,
            DateTimeOperations.GreaterThanOrEqual {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.GreaterThanOrEqual v -> Some(DateTimeOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsGreaterThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateTimeOperations.GreaterThanOrEqual({| v1 = Some v |}) |> operationLens.Set,
                   Some dateTimeGreaterThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let dateTimeLessThanId =
      Identifier.FullyQualified([ "dateTime" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeLessThanId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue)),
            Kind.Star,
            DateTimeOperations.LessThan {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.LessThan v -> Some(DateTimeOperations.LessThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsLessThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateTimeOperations.LessThan({| v1 = Some v |}) |> operationLens.Set, Some dateTimeLessThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let dateTimeLessThanOrEqualId =
      Identifier.FullyQualified([ "dateTime" ], "<=") |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeLessThanOrEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue)),
            Kind.Star,
            DateTimeOperations.LessThanOrEqual {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.LessThanOrEqual v -> Some(DateTimeOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsLessThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (DateTimeOperations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set,
                   Some dateTimeLessThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    let dateTimeToDateOnlyId =
      Identifier.FullyQualified([ "dateTime" ], "toDateOnly")
      |> TypeCheckScope.Empty.Resolve

    let toDateOnlyOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeToDateOnlyId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(dateTimeTypeValue, dateOnlyTypeValue), Kind.Star, DateTimeOperations.ToDateOnly)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.ToDateOnly -> Some(DateTimeOperations.ToDateOnly)
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
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return
                Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Date(System.DateOnly(v.Year, v.Month, v.Day)))
            } }

    let dateOnlyYearId =
      Identifier.FullyQualified([ "dateTime" ], "getYear")
      |> TypeCheckScope.Empty.Resolve

    let yearOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateOnlyYearId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue), Kind.Star, DateTimeOperations.Year)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.Year -> Some(DateTimeOperations.Year)
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
      Identifier.FullyQualified([ "DateOnly" ], "getMonth")
      |> TypeCheckScope.Empty.Resolve

    let monthOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateOnlyMonthId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue), Kind.Star, DateTimeOperations.Month)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.Month -> Some(DateTimeOperations.Month)
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
      Identifier.FullyQualified([ "DateOnly" ], "getDay")
      |> TypeCheckScope.Empty.Resolve

    let dayOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateOnlyDayId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue), Kind.Star, DateTimeOperations.Day)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.Day -> Some(DateTimeOperations.Day)
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
      Identifier.FullyQualified([ "DateOnly" ], "getDayOfWeek")
      |> TypeCheckScope.Empty.Resolve

    let dayOfWeekOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateOnlyDayOfWeekId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue), Kind.Star, DateTimeOperations.DayOfWeek)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.DayOfWeek -> Some(DateTimeOperations.DayOfWeek)
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
      Identifier.FullyQualified([ "DateOnly" ], "getDayOfYear")
      |> TypeCheckScope.Empty.Resolve

    let dayOfYearOperation: ResolvedIdentifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateOnlyDayOfYearId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(dateOnlyTypeValue, int32TypeValue), Kind.Star, DateTimeOperations.DayOfYear)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.DayOfYear -> Some(DateTimeOperations.DayOfYear)
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
          dayOfYearOperation
          dateTimeNew
          dateTimeNow
          dateTimeUTCNow ]
        |> Map.ofList }
