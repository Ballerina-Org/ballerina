namespace Ballerina.DSL.Next.StdLib.Decimal

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

  let DecimalExtension<'ext>
    (operationLens: PartialLens<'ext, DecimalOperations<'ext>>)
    : OperationsExtension<'ext, DecimalOperations<'ext>> =

    let decimalPlusId = Identifier.FullyQualified([ "Decimal" ], "+")

    let plusOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalPlusId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Decimal,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Decimal)
          )
        Kind = Kind.Star
        Operation = DecimalOperations.Plus {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Plus v -> Some(DecimalOperations.Plus v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DecimalOperations.AsPlus |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDecimal |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.Plus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure + v))
            } }


    let decimalMinusId = Identifier.FullyQualified([ "Decimal" ], "-")

    let minusOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalMinusId,
      { Type = TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Decimal)
        Kind = Kind.Star
        Operation = DecimalOperations.Minus {| v1 = () |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Minus v -> Some(DecimalOperations.Minus v)
            | _ -> None)
        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDecimal |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Decimal(-v))
            } }

    let decimalDivideId = Identifier.FullyQualified([ "Decimal" ], "/")

    let divideOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalDivideId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Decimal,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Decimal)
          )
        Kind = Kind.Star
        Operation = DecimalOperations.Divide {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Divide v -> Some(DecimalOperations.Divide v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DecimalOperations.AsDivide |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDecimal |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.Divide({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure / v))
            } }

    let decimalPowerId = Identifier.FullyQualified([ "Decimal" ], "**")

    let powerOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalPowerId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Decimal,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Decimal)
          )
        Kind = Kind.Star
        Operation = DecimalOperations.Power {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Power v -> Some(DecimalOperations.Power v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DecimalOperations.AsPower |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = v |> PrimitiveValue.AsDecimal |> reader.OfSum
                return DecimalOperations.Power({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum
                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Decimal(pown vClosure v))
            } }

    let decimalModId = Identifier.FullyQualified([ "Decimal" ], "%")

    let modOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalModId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Decimal,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Decimal)
          )
        Kind = Kind.Star
        Operation = DecimalOperations.Mod {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Mod v -> Some(DecimalOperations.Mod v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DecimalOperations.AsMod |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDecimal |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.Mod({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Decimal(vClosure % v))
            } }

    let decimalEqualId = Identifier.FullyQualified([ "Decimal" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Decimal,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DecimalOperations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.Equal v -> Some(DecimalOperations.Equal v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DecimalOperations.AsEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDecimal |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let decimalNotEqualId = Identifier.FullyQualified([ "Decimal" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalNotEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Decimal,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DecimalOperations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.NotEqual v -> Some(DecimalOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DecimalOperations.AsNotEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDecimal |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let decimalGreaterThanId = Identifier.FullyQualified([ "Decimal" ], ">")

    let greaterThanOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalGreaterThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Decimal,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DecimalOperations.GreaterThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.GreaterThan v -> Some(DecimalOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DecimalOperations.AsGreaterThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDecimal |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let decimalGreaterThanOrEqualId = Identifier.FullyQualified([ "Decimal" ], ">=")

    let greaterThanOrEqualOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalGreaterThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Decimal,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DecimalOperations.GreaterThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.GreaterThanOrEqual v -> Some(DecimalOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DecimalOperations.AsGreaterThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDecimal |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DecimalOperations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let decimalLessThanId = Identifier.FullyQualified([ "Decimal" ], "<")

    let lessThanOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalLessThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Decimal,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DecimalOperations.LessThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.LessThan v -> Some(DecimalOperations.LessThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DecimalOperations.AsLessThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDecimal |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DecimalOperations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let decimalLessThanOrEqualId = Identifier.FullyQualified([ "Decimal" ], "<=")

    let lessThanOrEqualOperation: Identifier * OperationExtension<'ext, DecimalOperations<'ext>> =
      decimalLessThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Decimal,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DecimalOperations.LessThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DecimalOperations.LessThanOrEqual v -> Some(DecimalOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DecimalOperations.AsLessThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDecimal |> reader.OfSum

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
