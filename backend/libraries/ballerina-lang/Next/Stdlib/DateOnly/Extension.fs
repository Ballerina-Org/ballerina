namespace Ballerina.DSL.Next.StdLib.DateOnly

[<AutoOpen>]
module Extension =
  open Ballerina.StdLib.Object
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open System

  let private dateTimeTypeValue = TypeValue.CreateDateTime()
  let private dateOnlyTypeValue = TypeValue.CreateDateOnly()
  let private timeSpanTypeValue = TypeValue.CreateTimeSpan()
  let private boolTypeValue = TypeValue.CreateBool()
  let private int32TypeValue = TypeValue.CreateInt32()
  let private stringTypeValue = TypeValue.CreateString()
  let private unitTypeValue = TypeValue.CreateUnit()

  let DateOnlyExtension<'ext>
    (consLens: PartialLens<'ext, DateOnlyConstructors>)
    (operationLens: PartialLens<'ext, DateOnlyOperations<'ext>>)
    : TypeExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =

    let dateOnlyId = Identifier.LocalScope "dateOnly"
    let dateOnlySymbolId = dateOnlyId |> TypeSymbol.Create
    let dateOnlyId = dateOnlyId |> TypeCheckScope.Empty.Resolve

    let dateOnlyConstructors = DateOnlyConstructorsExtension<'ext> consLens

    let dateOnlyDiffId =
      Identifier.FullyQualified([ "dateOnly" ], "-") |> TypeCheckScope.Empty.Resolve

    let diffOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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
                let dateTime1 = DateTime(vClosure.Year, vClosure.Month, vClosure.Day)
                let dateTime2 = DateTime(v.Year, v.Month, v.Day)
                let difference = dateTime1 - dateTime2
                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.TimeSpan(difference))
            } }

    let dateOnlyEqualId =
      Identifier.FullyQualified([ "dateOnly" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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

    let dateOnlyNotEqualId =
      Identifier.FullyQualified([ "dateOnly" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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

    let dateOnlyGreaterThanId =
      Identifier.FullyQualified([ "dateOnly" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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

    let dateOnlyGreaterThanOrEqualId =
      Identifier.FullyQualified([ "dateOnly" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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

    let dateOnlyLessThanId =
      Identifier.FullyQualified([ "dateOnly" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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

    let dateOnlyLessThanOrEqualId =
      Identifier.FullyQualified([ "dateOnly" ], "<=") |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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


    let dateOnlyToDateTimeId =
      Identifier.FullyQualified([ "dateOnly" ], "toDateTime")
      |> TypeCheckScope.Empty.Resolve

    let toDateTimeOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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
                    DateTime(vClosure.Year, vClosure.Month, vClosure.Day, v1, v2, v3, DateTimeKind.Utc)

                  return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.DateTime(dateTime))
                | _ ->
                  return!
                    sum.Throw(Errors.Singleton(loc0, "Expected a tuple of 3 int32s"))
                    |> reader.OfSum
              | _ -> return! sum.Throw(Errors.Singleton(loc0, "Expected a tuple or date")) |> reader.OfSum
            } }

    let dateOnlyYearId =
      Identifier.FullyQualified([ "dateOnly" ], "getYear")
      |> TypeCheckScope.Empty.Resolve

    let yearOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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

    let dateOnlyMonthId =
      Identifier.FullyQualified([ "dateOnly" ], "getMonth")
      |> TypeCheckScope.Empty.Resolve

    let monthOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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

    let dateOnlyDayId =
      Identifier.FullyQualified([ "dateOnly" ], "getDay")
      |> TypeCheckScope.Empty.Resolve

    let dayOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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

    let dateOnlyDayOfWeekId =
      Identifier.FullyQualified([ "dateOnly" ], "getDayOfWeek")
      |> TypeCheckScope.Empty.Resolve

    let dayOfWeekOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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

    let dateOnlyDayOfYearId =
      Identifier.FullyQualified([ "dateOnly" ], "getDayOfYear")
      |> TypeCheckScope.Empty.Resolve

    let dayOfYearOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateOnlyConstructors, PrimitiveValue, DateOnlyOperations<'ext>> =
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

    { TypeName = dateOnlyId, dateOnlySymbolId
      TypeVars = []
      Cases = dateOnlyConstructors |> Map.ofList
      // WrapTypeVars =
      //   fun t ->
      //     match t with
      //     | TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.String, _) ->
      //       TypeValue.CreateArrow(stringTypeValue, TypeValue.CreateSum [ dateOnlyTypeValue; unitTypeValue ])
      //     | TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Unit, _) ->
      //       TypeValue.CreateArrow(unitTypeValue, dateOnlyTypeValue)
      //     | TypeExpr.FromTypeValue(TypeValue.Imported _) -> dateOnlyTypeValue
      //     | _ -> failwith $"Expected a Arrow or Imported, got {t}"
      Deconstruct =
        fun (v) ->
          match v with
          | PrimitiveValue.Date v -> Value.Primitive(PrimitiveValue.Date v)
          | _ -> Value.Primitive(PrimitiveValue.Unit)
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
