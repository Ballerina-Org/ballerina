namespace Ballerina.DSL.Next.StdLib.Decimal

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
  open Ballerina.DSL.Next.StdLib.Option

  let DecimalExtension<'ext>
    (operationLens: PartialLens<'ext, DecimalOperations<'ext>>)
    : OperationsExtension<'ext, DecimalOperations<'ext>> =

    let int32TypeValue = TypeValue.CreateInt32()
    let decimalTypeValue = TypeValue.CreateDecimal()
    let boolTypeValue = TypeValue.CreateBool()

    let decimalToStringId =
      Identifier.FullyQualified([ "decimal" ], "string")
      |> TypeCheckScope.Empty.Resolve

    let toStringOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalToStringId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateString()), Kind.Star, DecimalOperations.String)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.String -> Some(DecimalOperations.String)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> DecimalOperations.AsString
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum


              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.String(v |> string))
            } }

    let decimalPlusId =
      Identifier.FullyQualified([ "decimal" ], "+") |> TypeCheckScope.Empty.Resolve

    let plusOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalPlusId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, decimalTypeValue)),
              Kind.Star,
              DecimalOperations.Plus {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Plus v -> Some(DecimalOperations.Plus v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsPlus
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.Plus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure + v))
            } }


    let decimalMinusId =
      Identifier.FullyQualified([ "decimal" ], "-") |> TypeCheckScope.Empty.Resolve

    let minusOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalMinusId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, decimalTypeValue)),
              Kind.Star,
              DecimalOperations.Minus {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Minus v -> Some(DecimalOperations.Minus v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsMinus
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.Minus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure - v))
            } }

    let decimalProductId =
      Identifier.FullyQualified([ "decimal" ], "*") |> TypeCheckScope.Empty.Resolve

    let productOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalProductId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, decimalTypeValue)),
              Kind.Star,
              DecimalOperations.Times {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Times v -> Some(DecimalOperations.Times v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsTimes
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.Times({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure * v))
            } }

    let decimalDivideId =
      Identifier.FullyQualified([ "decimal" ], "/") |> TypeCheckScope.Empty.Resolve

    let divideOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalDivideId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, decimalTypeValue)),
              Kind.Star,
              DecimalOperations.Divide {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Divide v -> Some(DecimalOperations.Divide v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsDivide
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.Divide({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure / v))
            } }

    let decimalPowerId =
      Identifier.FullyQualified([ "decimal" ], "**") |> TypeCheckScope.Empty.Resolve

    let powerOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalPowerId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(int32TypeValue, decimalTypeValue)),
              Kind.Star,
              DecimalOperations.Power {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Power v -> Some(DecimalOperations.Power v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsPower
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v =
                  v
                  |> PrimitiveValue.AsDecimal
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return DecimalOperations.Power({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let! v =
                  v
                  |> PrimitiveValue.AsInt32
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Decimal(pown vClosure v))
            } }

    let decimalModId =
      Identifier.FullyQualified([ "decimal" ], "%") |> TypeCheckScope.Empty.Resolve

    let modOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalModId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, decimalTypeValue)),
              Kind.Star,
              DecimalOperations.Mod {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Mod v -> Some(DecimalOperations.Mod v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsMod
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.Mod({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure % v))
            } }

    let decimalEqualId =
      Identifier.FullyQualified([ "decimal" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue)),
              Kind.Star,
              DecimalOperations.Equal {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Equal v -> Some(DecimalOperations.Equal v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let decimalNotEqualId =
      Identifier.FullyQualified([ "decimal" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalNotEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue)),
              Kind.Star,
              DecimalOperations.NotEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.NotEqual v -> Some(DecimalOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsNotEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let decimalGreaterThanId =
      Identifier.FullyQualified([ "decimal" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalGreaterThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue)),
              Kind.Star,
              DecimalOperations.GreaterThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.GreaterThan v -> Some(DecimalOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsGreaterThan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let decimalGreaterThanOrEqualId =
      Identifier.FullyQualified([ "decimal" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalGreaterThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue)),
              Kind.Star,
              DecimalOperations.GreaterThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.GreaterThanOrEqual v -> Some(DecimalOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsGreaterThanOrEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DecimalOperations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let decimalLessThanId =
      Identifier.FullyQualified([ "decimal" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalLessThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue)),
              Kind.Star,
              DecimalOperations.LessThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.LessThan v -> Some(DecimalOperations.LessThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsLessThan
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let decimalLessThanOrEqualId =
      Identifier.FullyQualified([ "decimal" ], "<=") |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalLessThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue)),
              Kind.Star,
              DecimalOperations.LessThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.LessThanOrEqual v -> Some(DecimalOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DecimalOperations.AsLessThanOrEqual
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              let! v = v |> Value.AsPrimitive |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsDecimal
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    { TypeVars = []
      Operations =
        [ toStringOperation
          plusOperation
          minusOperation
          divideOperation
          productOperation
          powerOperation
          modOperation
          equalOperation
          notEqualOperation
          greaterThanOperation
          greaterThanOrEqualOperation
          lessThanOperation
          lessThanOrEqualOperation ]
        |> Map.ofList }
