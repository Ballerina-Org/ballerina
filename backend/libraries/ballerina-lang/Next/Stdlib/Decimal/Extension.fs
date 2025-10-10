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

  let private int32TypeValue = TypeValue.CreateInt32()
  let private decimalTypeValue = TypeValue.CreateDecimal()


  let private boolTypeValue = TypeValue.CreateBool()

  let DecimalExtension<'ext>
    (operationLens: PartialLens<'ext, DecimalOperations<'ext>>)
    : OperationsExtension<'ext, DecimalOperations<'ext>> =

    let decimalPlusId = Identifier.FullyQualified([ "decimal" ], "+")

    let plusOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalPlusId,
      { Type = TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, decimalTypeValue))
        Kind = Kind.Star
        Operation = DecimalOperations.Plus {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Plus v -> Some(DecimalOperations.Plus v)
            | _ -> None)

        Apply =
          fun loc0 (op, v) ->
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

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure + v))
            } }


    let decimalMinusId = Identifier.FullyQualified([ "decimal" ], "-")

    let minusOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalMinusId,
      { Type = TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, decimalTypeValue))
        Kind = Kind.Star
        Operation = DecimalOperations.Minus {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Minus v -> Some(DecimalOperations.Minus v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
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

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure - v))
            } }

    let decimalDivideId = Identifier.FullyQualified([ "decimal" ], "/")

    let divideOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalDivideId,
      { Type = TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, decimalTypeValue))
        Kind = Kind.Star
        Operation = DecimalOperations.Divide {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Divide v -> Some(DecimalOperations.Divide v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
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

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure / v))
            } }

    let decimalPowerId = Identifier.FullyQualified([ "decimal" ], "**")

    let powerOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalPowerId,
      { Type = TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(int32TypeValue, decimalTypeValue))
        Kind = Kind.Star
        Operation = DecimalOperations.Power {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Power v -> Some(DecimalOperations.Power v)
            | _ -> None)

        Apply =
          fun loc0 (op, v) ->
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

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Decimal(pown vClosure v))
            } }

    let decimalModId = Identifier.FullyQualified([ "decimal" ], "%")

    let modOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalModId,
      { Type = TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, decimalTypeValue))
        Kind = Kind.Star
        Operation = DecimalOperations.Mod {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Mod v -> Some(DecimalOperations.Mod v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
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

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure % v))
            } }

    let decimalEqualId = Identifier.FullyQualified([ "decimal" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalEqualId,
      { Type = TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DecimalOperations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Equal v -> Some(DecimalOperations.Equal v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
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

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let decimalNotEqualId = Identifier.FullyQualified([ "decimal" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalNotEqualId,
      { Type = TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DecimalOperations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.NotEqual v -> Some(DecimalOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
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

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let decimalGreaterThanId = Identifier.FullyQualified([ "decimal" ], ">")

    let greaterThanOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalGreaterThanId,
      { Type = TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DecimalOperations.GreaterThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.GreaterThan v -> Some(DecimalOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
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

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let decimalGreaterThanOrEqualId = Identifier.FullyQualified([ "decimal" ], ">=")

    let greaterThanOrEqualOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalGreaterThanOrEqualId,
      { Type = TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DecimalOperations.GreaterThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.GreaterThanOrEqual v -> Some(DecimalOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
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

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let decimalLessThanId = Identifier.FullyQualified([ "decimal" ], "<")

    let lessThanOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalLessThanId,
      { Type = TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DecimalOperations.LessThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.LessThan v -> Some(DecimalOperations.LessThan v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
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

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let decimalLessThanOrEqualId = Identifier.FullyQualified([ "decimal" ], "<=")

    let lessThanOrEqualOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalLessThanOrEqualId,
      { Type = TypeValue.CreateArrow(decimalTypeValue, TypeValue.CreateArrow(decimalTypeValue, boolTypeValue))
        Kind = Kind.Star
        Operation = DecimalOperations.LessThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.LessThanOrEqual v -> Some(DecimalOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 (op, v) ->
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

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    { TypeVars = []
      Operations =
        [ plusOperation
          minusOperation
          divideOperation
          powerOperation
          modOperation
          equalOperation
          notEqualOperation
          greaterThanOperation
          greaterThanOrEqualOperation
          lessThanOperation
          lessThanOrEqualOperation ]
        |> Map.ofList }
