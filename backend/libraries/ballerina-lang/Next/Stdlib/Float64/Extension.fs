namespace Ballerina.DSL.Next.StdLib.Float64

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

  let Float64Extension<'ext>
    (operationLens: PartialLens<'ext, Float64Operations<'ext>>)
    : OperationsExtension<'ext, Float64Operations<'ext>> =
    let float64PlusId = Identifier.FullyQualified([ "Float64" ], "+")

    let plusOperation: Identifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64PlusId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float64, TypeValue.Primitive PrimitiveType.Float64)
          )
        Kind = Kind.Star
        Operation = Float64Operations.Plus {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Plus v -> Some(Float64Operations.Plus v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float64Operations.AsPlus |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.Plus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float64(vClosure + v))
            } }

    let float64MinusId = Identifier.FullyQualified([ "Float64" ], "-")

    let minusOperation: Identifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64MinusId,
      { Type = TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float64, TypeValue.Primitive PrimitiveType.Float64)
        Kind = Kind.Star
        Operation = Float64Operations.Minus {| v1 = () |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Minus v -> Some(Float64Operations.Minus v)
            | _ -> None)
        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat64 |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float64(-v))
            } }

    let float64DivideId = Identifier.FullyQualified([ "Float64" ], "/")

    let divideOperation: Identifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64DivideId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float64, TypeValue.Primitive PrimitiveType.Float64)
          )
        Kind = Kind.Star
        Operation = Float64Operations.Divide {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Divide v -> Some(Float64Operations.Divide v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float64Operations.AsDivide |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.Divide({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float64(vClosure / v))
            } }

    let float64PowerId = Identifier.FullyQualified([ "Float64" ], "**")

    let powerOperation: Identifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64PowerId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Float64)
          )
        Kind = Kind.Star
        Operation = Float64Operations.Power {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Power v -> Some(Float64Operations.Power v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float64Operations.AsPower |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = v |> PrimitiveValue.AsFloat64 |> reader.OfSum
                return Float64Operations.Power({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum
                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float64(pown vClosure v))
            } }

    let float64ModId = Identifier.FullyQualified([ "Float64" ], "%")

    let modOperation: Identifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64ModId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float64, TypeValue.Primitive PrimitiveType.Float64)
          )
        Kind = Kind.Star
        Operation = Float64Operations.Mod {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Mod v -> Some(Float64Operations.Mod v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float64Operations.AsMod |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.Mod({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float64(vClosure % v))
            } }

    let float64EqualId = Identifier.FullyQualified([ "Float64" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64EqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float64Operations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.Equal v -> Some(Float64Operations.Equal v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float64Operations.AsEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let float64NotEqualId = Identifier.FullyQualified([ "Float64" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64NotEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float64Operations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.NotEqual v -> Some(Float64Operations.NotEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float64Operations.AsNotEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let float64GreaterThanId = Identifier.FullyQualified([ "Float64" ], ">")

    let greaterThanOperation: Identifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64GreaterThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float64Operations.GreaterThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.GreaterThan v -> Some(Float64Operations.GreaterThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float64Operations.AsGreaterThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let float64GreaterThanOrEqualId = Identifier.FullyQualified([ "Float64" ], ">=")

    let greaterThanOrEqualOperation: Identifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64GreaterThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float64Operations.GreaterThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.GreaterThanOrEqual v -> Some(Float64Operations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float64Operations.AsGreaterThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  Float64Operations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let float64LessThanId = Identifier.FullyQualified([ "Float64" ], "<")

    let lessThanOperation: Identifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64LessThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float64Operations.LessThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.LessThan v -> Some(Float64Operations.LessThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float64Operations.AsLessThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let float64LessThanOrEqualId = Identifier.FullyQualified([ "Float64" ], "<=")

    let lessThanOrEqualOperation: Identifier * OperationExtension<'ext, Float64Operations<'ext>> =
      float64LessThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float64Operations.LessThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float64Operations.LessThanOrEqual v -> Some(Float64Operations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float64Operations.AsLessThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float64Operations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
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
