namespace Ballerina.DSL.Next.StdLib.TimeSpan

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

  let private timeSpanTypeValue = TypeValue.CreateTimeSpan()
  let private boolTypeValue = TypeValue.CreateBool()
  let private int32TypeValue = TypeValue.CreateInt32()
  let private float64TypeValue = TypeValue.CreateFloat64()
  let private stringTypeValue = TypeValue.CreateString()
  let private unitTypeValue = TypeValue.CreateUnit()

  let TimeSpanExtension<'ext>
    (consLens: PartialLens<'ext, TimeSpanConstructors>)
    (operationLens: PartialLens<'ext, TimeSpanOperations<'ext>>)
    : TypeExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =

    let timeSpanId = Identifier.LocalScope "timeSpan"
    let timeSpanSymbolId = timeSpanId |> TypeSymbol.Create
    let timeSpanId = timeSpanId |> TypeCheckScope.Empty.Resolve

    let timeSpanConstructors = TimeSpanConstructorsExtension<'ext> consLens

    let TimeSpanPlusId =
      Identifier.FullyQualified([ "timeSpan" ], "+") |> TypeCheckScope.Empty.Resolve

    let plusOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      TimeSpanPlusId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, timeSpanTypeValue))
        Kind = Kind.Star
        Operation = TimeSpanOperations.Plus {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return TimeSpanOperations.Plus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.TimeSpan(vClosure + v))
            } }

    let TimeSpanMinusId =
      Identifier.FullyQualified([ "timeSpan" ], "-") |> TypeCheckScope.Empty.Resolve

    let minusOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      TimeSpanMinusId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, timeSpanTypeValue)
        Kind = Kind.Star
        Operation = TimeSpanOperations.Minus {| v1 = () |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Minus v -> Some(TimeSpanOperations.Minus v)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.TimeSpan(-v))
            } }

    let TimeSpanEqualId =
      Identifier.FullyQualified([ "timeSpan" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      TimeSpanEqualId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = TimeSpanOperations.Equal {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return TimeSpanOperations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let TimeSpanNotEqualId =
      Identifier.FullyQualified([ "timeSpan" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      TimeSpanNotEqualId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = TimeSpanOperations.NotEqual {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return TimeSpanOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let TimeSpanGreaterThanId =
      Identifier.FullyQualified([ "timeSpan" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      TimeSpanGreaterThanId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = TimeSpanOperations.GreaterThan {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return TimeSpanOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let TimeSpanGreaterThanOrEqualId =
      Identifier.FullyQualified([ "timeSpan" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      TimeSpanGreaterThanOrEqualId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, TypeValue.CreateArrow(timeSpanTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = TimeSpanOperations.GreaterThanOrEqual {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  TimeSpanOperations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let TimeSpanLessThanId =
      Identifier.FullyQualified([ "timeSpan" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      TimeSpanLessThanId,
      { Type =
          TypeValue.CreateArrow(
            TypeValue.CreatePrimitive PrimitiveType.TimeSpan,
            TypeValue.CreateArrow(
              TypeValue.CreatePrimitive PrimitiveType.TimeSpan,
              TypeValue.CreatePrimitive PrimitiveType.Bool
            )
          )
        Kind = Kind.Star
        Operation = TimeSpanOperations.LessThan {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return TimeSpanOperations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let TimeSpanLessThanOrEqualId =
      Identifier.FullyQualified([ "timeSpan" ], "<=") |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      TimeSpanLessThanOrEqualId,
      { Type =
          TypeValue.CreateArrow(
            TypeValue.CreatePrimitive PrimitiveType.TimeSpan,
            TypeValue.CreateArrow(
              TypeValue.CreatePrimitive PrimitiveType.TimeSpan,
              TypeValue.CreatePrimitive PrimitiveType.Bool
            )
          )
        Kind = Kind.Star
        Operation = TimeSpanOperations.LessThanOrEqual {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  TimeSpanOperations.LessThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    let timeSpanGetDaysId =
      Identifier.FullyQualified([ "timeSpan" ], "getDays")
      |> TypeCheckScope.Empty.Resolve

    let getDaysOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      timeSpanGetDaysId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = TimeSpanOperations.Days
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Days -> Some(TimeSpanOperations.Days)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.Days))
            } }

    let timeSpanGetHoursId =
      Identifier.FullyQualified([ "timeSpan" ], "getHours")
      |> TypeCheckScope.Empty.Resolve

    let getHoursOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      timeSpanGetHoursId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = TimeSpanOperations.Hours
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Hours -> Some(TimeSpanOperations.Hours)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.Hours))
            } }

    let timeSpanGetMinutesId =
      Identifier.FullyQualified([ "timeSpan" ], "getMinutes")
      |> TypeCheckScope.Empty.Resolve

    let getMinutesOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      timeSpanGetMinutesId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = TimeSpanOperations.Minutes
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Minutes -> Some(TimeSpanOperations.Minutes)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.Minutes))
            } }

    let timeSpanGetSecondsId =
      Identifier.FullyQualified([ "timeSpan" ], "getSeconds")
      |> TypeCheckScope.Empty.Resolve

    let getSecondsOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      timeSpanGetSecondsId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = TimeSpanOperations.Seconds
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Seconds -> Some(TimeSpanOperations.Seconds)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.Seconds))
            } }

    let timeSpanGetMillisecondsId =
      Identifier.FullyQualified([ "timeSpan" ], "getMilliseconds")
      |> TypeCheckScope.Empty.Resolve

    let getMillisecondsOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      timeSpanGetMillisecondsId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, int32TypeValue)
        Kind = Kind.Star
        Operation = TimeSpanOperations.Milliseconds
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.Milliseconds -> Some(TimeSpanOperations.Milliseconds)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.Milliseconds))
            } }

    let timeSpanGetTotalDaysId =
      Identifier.FullyQualified([ "timeSpan" ], "totalDays")
      |> TypeCheckScope.Empty.Resolve

    let getTotalDaysOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      timeSpanGetTotalDaysId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, float64TypeValue)
        Kind = Kind.Star
        Operation = TimeSpanOperations.TotalDays
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TotalDays -> Some(TimeSpanOperations.TotalDays)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float64(v.TotalDays))
            } }

    let timeSpanGetTotalHoursId =
      Identifier.FullyQualified([ "timeSpan" ], "totalHours")
      |> TypeCheckScope.Empty.Resolve

    let getTotalHoursOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      timeSpanGetTotalHoursId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, float64TypeValue)
        Kind = Kind.Star
        Operation = TimeSpanOperations.TotalHours
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TotalHours -> Some(TimeSpanOperations.TotalHours)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float64(v.TotalHours))
            } }

    let timeSpanGetTotalMinutesId =
      Identifier.FullyQualified([ "timeSpan" ], "totalMinutes")
      |> TypeCheckScope.Empty.Resolve

    let getTotalMinutesOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      timeSpanGetTotalMinutesId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, float64TypeValue)
        Kind = Kind.Star
        Operation = TimeSpanOperations.TotalMinutes
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TotalMinutes -> Some(TimeSpanOperations.TotalMinutes)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float64(v.TotalMinutes))
            } }

    let timeSpanGetTotalSecondsId =
      Identifier.FullyQualified([ "timeSpan" ], "totalSeconds")
      |> TypeCheckScope.Empty.Resolve

    let getTotalSecondsOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      timeSpanGetTotalSecondsId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, float64TypeValue)
        Kind = Kind.Star
        Operation = TimeSpanOperations.TotalSeconds
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TotalSeconds -> Some(TimeSpanOperations.TotalSeconds)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float64(v.TotalSeconds))
            } }

    let timeSpanGetTotalMillisecondsId =
      Identifier.FullyQualified([ "timeSpan" ], "totalMilliseconds")
      |> TypeCheckScope.Empty.Resolve

    let getTotalMillisecondsOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, TimeSpanConstructors, PrimitiveValue, TimeSpanOperations<'ext>> =
      timeSpanGetTotalMillisecondsId,
      { Type = TypeValue.CreateArrow(timeSpanTypeValue, float64TypeValue)
        Kind = Kind.Star
        Operation = TimeSpanOperations.TotalMilliseconds
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | TimeSpanOperations.TotalMilliseconds -> Some(TimeSpanOperations.TotalMilliseconds)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsTimeSpan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float64(v.TotalMilliseconds))
            } }

    { TypeName = timeSpanId, timeSpanSymbolId
      TypeVars = []
      Cases = timeSpanConstructors |> Map.ofList
      Deconstruct =
        fun (v) ->
          match v with
          | PrimitiveValue.TimeSpan v -> Value.Primitive(PrimitiveValue.TimeSpan v)
          | _ -> Value.Primitive(PrimitiveValue.Unit)
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
          getTotalMillisecondsOperation ]
        |> Map.ofList }
