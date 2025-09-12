namespace Ballerina.DSL.Next.StdLib.Float32

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

  let Float32Extension<'ext>
    (operationLens: PartialLens<'ext, Float32Operations<'ext>>)
    : OperationsExtension<'ext, Float32Operations<'ext>> =
    let float32PlusId = Identifier.FullyQualified([ "Float32" ], "+")

    let plusOperation: Identifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32PlusId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float32, TypeValue.Primitive PrimitiveType.Float32)
          )
        Kind = Kind.Star
        Operation = Float32Operations.Plus {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Plus v -> Some(Float32Operations.Plus v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float32Operations.AsPlus |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float32Operations.Plus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float32(vClosure + v))
            } }

    let float32MinusId = Identifier.FullyQualified([ "Float32" ], "-")

    let minusOperation: Identifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32MinusId,
      { Type = TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float32, TypeValue.Primitive PrimitiveType.Float32)
        Kind = Kind.Star
        Operation = Float32Operations.Minus {| v1 = () |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Minus v -> Some(Float32Operations.Minus v)
            | _ -> None)
        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat32 |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float32(-v))
            } }

    let float32DivideId = Identifier.FullyQualified([ "Float32" ], "/")

    let divideOperation: Identifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32DivideId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float32, TypeValue.Primitive PrimitiveType.Float32)
          )
        Kind = Kind.Star
        Operation = Float32Operations.Divide {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Divide v -> Some(Float32Operations.Divide v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float32Operations.AsDivide |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float32Operations.Divide({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float32(vClosure / v))
            } }

    let float32PowerId = Identifier.FullyQualified([ "Float32" ], "**")

    let powerOperation: Identifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32PowerId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Float32)
          )
        Kind = Kind.Star
        Operation = Float32Operations.Power {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Power v -> Some(Float32Operations.Power v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float32Operations.AsPower |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = v |> PrimitiveValue.AsFloat32 |> reader.OfSum
                return Float32Operations.Power({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum
                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float32(pown vClosure v))
            } }

    let float32ModId = Identifier.FullyQualified([ "Float32" ], "%")

    let modOperation: Identifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32ModId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float32, TypeValue.Primitive PrimitiveType.Float32)
          )
        Kind = Kind.Star
        Operation = Float32Operations.Mod {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Mod v -> Some(Float32Operations.Mod v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float32Operations.AsMod |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float32Operations.Mod({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Float32(vClosure % v))
            } }

    let float32EqualId = Identifier.FullyQualified([ "Float32" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32EqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float32Operations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.Equal v -> Some(Float32Operations.Equal v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float32Operations.AsEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float32Operations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let float32NotEqualId = Identifier.FullyQualified([ "Float32" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32NotEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float32Operations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.NotEqual v -> Some(Float32Operations.NotEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float32Operations.AsNotEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float32Operations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let float32GreaterThanId = Identifier.FullyQualified([ "Float32" ], ">")

    let greaterThanOperation: Identifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32GreaterThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float32Operations.GreaterThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.GreaterThan v -> Some(Float32Operations.GreaterThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float32Operations.AsGreaterThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float32Operations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let float32GreaterThanOrEqualId = Identifier.FullyQualified([ "Float32" ], ">=")

    let greaterThanOrEqualOperation: Identifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32GreaterThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float32Operations.GreaterThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.GreaterThanOrEqual v -> Some(Float32Operations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float32Operations.AsGreaterThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  Float32Operations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let float32LessThanId = Identifier.FullyQualified([ "Float32" ], "<")

    let lessThanOperation: Identifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32LessThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float32Operations.LessThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.LessThan v -> Some(Float32Operations.LessThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float32Operations.AsLessThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float32Operations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let float32LessThanOrEqualId = Identifier.FullyQualified([ "Float32" ], "<=")

    let lessThanOrEqualOperation: Identifier * OperationExtension<'ext, Float32Operations<'ext>> =
      float32LessThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Float32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Float32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Float32Operations.LessThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Float32Operations.LessThanOrEqual v -> Some(Float32Operations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Float32Operations.AsLessThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsFloat32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Float32Operations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
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
