namespace Ballerina.DSL.Next.StdLib.DateTime

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
  let private stringTypeValue = TypeValue.CreateString()
  let private unitTypeValue = TypeValue.CreateUnit()


  let DateTimeExtension<'ext>
    (consLens: PartialLens<'ext, DateTimeConstructors>)
    (operationLens: PartialLens<'ext, DateTimeOperations<'ext>>)
    : TypeExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =

    let dateTimeId = Identifier.LocalScope "dateTime"
    let dateTimeSymbolId = dateTimeId |> TypeSymbol.Create
    let dateTimeId = dateTimeId |> TypeCheckScope.Empty.Resolve

    let dateTimeConstructors = DateTimeConstructorsExtension<'ext> consLens

    let dateTimeDiffId =
      Identifier.FullyQualified([ "dateTime" ], "-") |> TypeCheckScope.Empty.Resolve

    let diffOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsDiff
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.Diff({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.TimeSpan(vClosure - v))
            } }

    let dateTimeEqualId =
      Identifier.FullyQualified([ "dateTime" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let dateTimeNotEqualId =
      Identifier.FullyQualified([ "dateTime" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsNotEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let dateTimeGreaterThanId =
      Identifier.FullyQualified([ "dateTime" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsGreaterThan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let dateTimeGreaterThanOrEqualId =
      Identifier.FullyQualified([ "dateTime" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsGreaterThanOrEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DateTimeOperations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let dateTimeLessThanId =
      Identifier.FullyQualified([ "dateTime" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DateTimeOperations.AsLessThan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let dateTimeLessThanOrEqualId = Identifier.FullyQualified([ "dateTime" ], "<=")

    let lessThanOrEqualOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
      dateTimeLessThanOrEqualId |> TypeCheckScope.Empty.Resolve,
      { Type = TypeValue.CreateArrow(dateTimeTypeValue, TypeValue.CreateArrow(dateTimeTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DateTimeOperations.LessThanOrEqual {| v1 = None |}
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DateTimeOperations.LessThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    let dateTimeToDateOnlyId =
      Identifier.FullyQualified([ "dateTime" ], "toDateOnly")
      |> TypeCheckScope.Empty.Resolve

    let toDateOnlyOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDateTime
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Date(System.DateOnly(v.Year, v.Month, v.Day)))
            } }

    let dateOnlyYearId =
      Identifier.FullyQualified([ "dateTime" ], "getYear")
      |> TypeCheckScope.Empty.Resolve

    let yearOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (_, v) ->
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
      Identifier.FullyQualified([ "DateOnly" ], "getMonth")
      |> TypeCheckScope.Empty.Resolve

    let monthOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (_, v) ->
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
      Identifier.FullyQualified([ "DateOnly" ], "getDay")
      |> TypeCheckScope.Empty.Resolve

    let dayOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (_, v) ->
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
      Identifier.FullyQualified([ "DateOnly" ], "getDayOfWeek")
      |> TypeCheckScope.Empty.Resolve

    let dayOfWeekOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (_, v) ->
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
      Identifier.FullyQualified([ "DateOnly" ], "getDayOfYear")
      |> TypeCheckScope.Empty.Resolve

    let dayOfYearOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'ext, DateTimeConstructors, PrimitiveValue, DateTimeOperations<'ext>> =
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
          fun loc0 _rest (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDate
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(v.DayOfYear))
            } }

    { TypeName = dateTimeId, dateTimeSymbolId
      TypeVars = []
      Cases = dateTimeConstructors |> Map.ofList
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
          | PrimitiveValue.DateTime v -> Value.Primitive(PrimitiveValue.DateTime v)
          | _ -> Value.Primitive(PrimitiveValue.Unit)
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
