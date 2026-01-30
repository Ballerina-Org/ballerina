namespace Ballerina.DSL.Next.StdLib.TimeSpan

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
  open System.Globalization

  let TimeSpanExtension<'ext>
    (operationLens: PartialLens<'ext, TimeSpanOperations<'ext>>)
    : OperationsExtension<'ext, TimeSpanOperations<'ext>> =

    let timeSpanTypeValue = TypeValue.CreateTimeSpan()
    let boolTypeValue = TypeValue.CreateBool()
    let int32TypeValue = TypeValue.CreateInt32()
    let float64TypeValue = TypeValue.CreateFloat64()

    let TimeSpanPlusId =
      Identifier.FullyQualified([ "timeSpan" ], "+") |> TypeCheckScope.Empty.Resolve

    let plusOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      TimeSpanPlusId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, timeSpanTypeValue)),
            Kind.Star,
            TimeSpanOperations.Plus {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Plus v -> Some(TimeSpanOperations.Plus v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> TimeSpanOperations.AsPlus
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None ->
                return
                  (TimeSpanOperations.Plus({| v1 = Some v |}) |> operationLens.Set, Some TimeSpanPlusId)
                  |> Ext
              | Some vClosure -> return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.TimeSpan(vClosure + v))
            } }

    let TimeSpanMinusId =
      Identifier.FullyQualified([ "timeSpan" ], "-") |> TypeCheckScope.Empty.Resolve

    let minusOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      TimeSpanMinusId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(timeSpanTypeValue, timeSpanTypeValue),
            Kind.Star,
            TimeSpanOperations.Minus {| v1 = () |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Minus v -> Some(TimeSpanOperations.Minus v)
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
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.TimeSpan(-v))
            } }

    let TimeSpanEqualId =
      Identifier.FullyQualified([ "timeSpan" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      TimeSpanEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, boolTypeValue)),
            Kind.Star,
            TimeSpanOperations.Equal {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Equal v -> Some(TimeSpanOperations.Equal v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> TimeSpanOperations.AsEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None ->
                return
                  (TimeSpanOperations.Equal({| v1 = Some v |}) |> operationLens.Set, Some TimeSpanEqualId)
                  |> Ext
              | Some vClosure -> return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let TimeSpanNotEqualId =
      Identifier.FullyQualified([ "timeSpan" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      TimeSpanNotEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, boolTypeValue)),
            Kind.Star,
            TimeSpanOperations.NotEqual {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.NotEqual v -> Some(TimeSpanOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> TimeSpanOperations.AsNotEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None ->
                return
                  (TimeSpanOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set, Some TimeSpanNotEqualId)
                  |> Ext
              | Some vClosure -> return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let TimeSpanGreaterThanId =
      Identifier.FullyQualified([ "timeSpan" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      TimeSpanGreaterThanId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, boolTypeValue)),
            Kind.Star,
            TimeSpanOperations.GreaterThan {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.GreaterThan v -> Some(TimeSpanOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> TimeSpanOperations.AsGreaterThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None ->
                return
                  (TimeSpanOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set, Some TimeSpanGreaterThanId)
                  |> Ext
              | Some vClosure -> return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let TimeSpanGreaterThanOrEqualId =
      Identifier.FullyQualified([ "timeSpan" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      TimeSpanGreaterThanOrEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, boolTypeValue)),
            Kind.Star,
            TimeSpanOperations.GreaterThanOrEqual {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.GreaterThanOrEqual v -> Some(TimeSpanOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> TimeSpanOperations.AsGreaterThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None ->
                return
                  (TimeSpanOperations.GreaterThanOrEqual({| v1 = Some v |}) |> operationLens.Set,
                   Some TimeSpanGreaterThanOrEqualId)
                  |> Ext
              | Some vClosure -> return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let TimeSpanLessThanId =
      Identifier.FullyQualified([ "timeSpan" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      TimeSpanLessThanId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, boolTypeValue)),
            Kind.Star,
            TimeSpanOperations.LessThan {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.LessThan v -> Some(TimeSpanOperations.LessThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> TimeSpanOperations.AsLessThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None ->
                return
                  (TimeSpanOperations.LessThan({| v1 = Some v |}) |> operationLens.Set, Some TimeSpanLessThanId)
                  |> Ext
              | Some vClosure -> return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let TimeSpanLessThanOrEqualId =
      Identifier.FullyQualified([ "timeSpan" ], "<=") |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      TimeSpanLessThanOrEqualId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, boolTypeValue)),
            Kind.Star,
            TimeSpanOperations.LessThanOrEqual {| v1 = None |}
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.LessThanOrEqual v -> Some(TimeSpanOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> TimeSpanOperations.AsLessThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None ->
                return
                  (TimeSpanOperations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set,
                   Some TimeSpanLessThanOrEqualId)
                  |> Ext
              | Some vClosure -> return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    let timeSpanGetDaysId =
      Identifier.FullyQualified([ "timeSpan" ], "getDays")
      |> TypeCheckScope.Empty.Resolve

    let getDaysOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanGetDaysId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(timeSpanTypeValue, int32TypeValue), Kind.Star, TimeSpanOperations.Days)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Days -> Some(TimeSpanOperations.Days)
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
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int32(v.Days))
            } }

    let timeSpanGetHoursId =
      Identifier.FullyQualified([ "timeSpan" ], "getHours")
      |> TypeCheckScope.Empty.Resolve

    let getHoursOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanGetHoursId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(timeSpanTypeValue, int32TypeValue), Kind.Star, TimeSpanOperations.Hours)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Hours -> Some(TimeSpanOperations.Hours)
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
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int32(v.Hours))
            } }

    let timeSpanGetMinutesId =
      Identifier.FullyQualified([ "timeSpan" ], "getMinutes")
      |> TypeCheckScope.Empty.Resolve

    let getMinutesOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanGetMinutesId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(timeSpanTypeValue, int32TypeValue), Kind.Star, TimeSpanOperations.Minutes)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Minutes -> Some(TimeSpanOperations.Minutes)
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
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int32(v.Minutes))
            } }

    let timeSpanGetSecondsId =
      Identifier.FullyQualified([ "timeSpan" ], "getSeconds")
      |> TypeCheckScope.Empty.Resolve

    let getSecondsOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanGetSecondsId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(timeSpanTypeValue, int32TypeValue), Kind.Star, TimeSpanOperations.Seconds)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Seconds -> Some(TimeSpanOperations.Seconds)
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
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int32(v.Seconds))
            } }

    let timeSpanGetMillisecondsId =
      Identifier.FullyQualified([ "timeSpan" ], "getMilliseconds")
      |> TypeCheckScope.Empty.Resolve

    let getMillisecondsOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanGetMillisecondsId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(timeSpanTypeValue, int32TypeValue), Kind.Star, TimeSpanOperations.Milliseconds)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Milliseconds -> Some(TimeSpanOperations.Milliseconds)
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
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int32(v.Milliseconds))
            } }

    let timeSpanGetTotalDaysId =
      Identifier.FullyQualified([ "timeSpan" ], "totalDays")
      |> TypeCheckScope.Empty.Resolve

    let getTotalDaysOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanGetTotalDaysId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(timeSpanTypeValue, float64TypeValue), Kind.Star, TimeSpanOperations.TotalDays)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TotalDays -> Some(TimeSpanOperations.TotalDays)
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
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float64(v.TotalDays))
            } }

    let timeSpanGetTotalHoursId =
      Identifier.FullyQualified([ "timeSpan" ], "totalHours")
      |> TypeCheckScope.Empty.Resolve

    let getTotalHoursOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanGetTotalHoursId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(timeSpanTypeValue, float64TypeValue), Kind.Star, TimeSpanOperations.TotalHours)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TotalHours -> Some(TimeSpanOperations.TotalHours)
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
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float64(v.TotalHours))
            } }

    let timeSpanGetTotalMinutesId =
      Identifier.FullyQualified([ "timeSpan" ], "totalMinutes")
      |> TypeCheckScope.Empty.Resolve

    let getTotalMinutesOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanGetTotalMinutesId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(timeSpanTypeValue, float64TypeValue), Kind.Star, TimeSpanOperations.TotalMinutes)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TotalMinutes -> Some(TimeSpanOperations.TotalMinutes)
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
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float64(v.TotalMinutes))
            } }

    let timeSpanGetTotalSecondsId =
      Identifier.FullyQualified([ "timeSpan" ], "totalSeconds")
      |> TypeCheckScope.Empty.Resolve

    let getTotalSecondsOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanGetTotalSecondsId,
      { PublicIdentifiers =
          Some(TypeValue.CreateArrow(timeSpanTypeValue, float64TypeValue), Kind.Star, TimeSpanOperations.TotalSeconds)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TotalSeconds -> Some(TimeSpanOperations.TotalSeconds)
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
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float64(v.TotalSeconds))
            } }

    let timeSpanGetTotalMillisecondsId =
      Identifier.FullyQualified([ "timeSpan" ], "totalMilliseconds")
      |> TypeCheckScope.Empty.Resolve

    let getTotalMillisecondsOperation: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanGetTotalMillisecondsId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(timeSpanTypeValue, float64TypeValue),
            Kind.Star,
            TimeSpanOperations.TotalMilliseconds
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TotalMilliseconds -> Some(TimeSpanOperations.TotalMilliseconds)
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
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Float64(v.TotalMilliseconds))
            } }

    let timeSpanNewId =
      Identifier.FullyQualified([ "timeSpan" ], "new") |> TypeCheckScope.Empty.Resolve

    let timeSpanNew: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanNewId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(
              TypeValue.CreatePrimitive PrimitiveType.String,
              TypeValue.CreateSum
                [ TypeValue.CreatePrimitive PrimitiveType.Unit

                  TypeValue.CreatePrimitive PrimitiveType.TimeSpan ]
            ),
            Kind.Star,
            TimeSpanOperations.TimeSpan_New
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TimeSpan_New -> Some(TimeSpanOperations.TimeSpan_New)
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
                try
                  Value.Sum(
                    { Case = 2; Count = 2 },
                    Value.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan.Parse(v, CultureInfo.InvariantCulture)))
                  )
                with _ ->
                  Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Unit))
            } }

    let timeSpanZeroId =
      Identifier.FullyQualified([ "timeSpan" ], "zero")
      |> TypeCheckScope.Empty.Resolve

    let timeSpanZero: ResolvedIdentifier * OperationExtension<'ext, TimeSpanOperations<'ext>> =
      timeSpanZeroId,
      { PublicIdentifiers =
          Some(
            TypeValue.CreateArrow(
              TypeValue.CreatePrimitive PrimitiveType.Unit,
              TypeValue.CreatePrimitive PrimitiveType.TimeSpan
            ),
            Kind.Star,
            TimeSpanOperations.TimeSpan_Zero
          )
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TimeSpan_Zero -> Some(TimeSpanOperations.TimeSpan_Zero)
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

              return Value.Primitive(PrimitiveValue.TimeSpan System.TimeSpan.Zero)
            } }

    { TypeVars = []
      Operations =
        [ plusOperation
          minusOperation
          equalOperation
          notEqualOperation
          greaterThanOperation
          greaterThanOrEqualOperation
          lessThanOperation
          lessThanOrEqualOperation
          getDaysOperation
          getHoursOperation
          getMinutesOperation
          getSecondsOperation
          getMillisecondsOperation
          getTotalDaysOperation
          getTotalHoursOperation
          getTotalMinutesOperation
          getTotalSecondsOperation
          getTotalMillisecondsOperation
          timeSpanNew
          timeSpanZero ]
        |> Map.ofList }
