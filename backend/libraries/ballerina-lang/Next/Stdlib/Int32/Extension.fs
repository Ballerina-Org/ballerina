namespace Ballerina.DSL.Next.StdLib.Int32

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

  let Int32Extension<'ext>
    (operationLens: PartialLens<'ext, Int32Operations<'ext>>)
    : OperationsExtension<'ext, Int32Operations<'ext>> =
    let int32PlusId = Identifier.FullyQualified([ "Int32" ], "+")

    let plusOperation: Identifier * OperationExtension<'ext, Int32Operations<'ext>> =
      int32PlusId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Int32)
          )
        Kind = Kind.Star
        Operation = Int32Operations.Plus {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Plus v -> Some(Int32Operations.Plus v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int32Operations.AsPlus |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int32Operations.Plus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(vClosure + v))
            } }

    let int32MinusId = Identifier.FullyQualified([ "Int32" ], "-")

    let minusOperation: Identifier * OperationExtension<'ext, Int32Operations<'ext>> =
      int32MinusId,
      { Type = TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Int32)
        Kind = Kind.Star
        Operation = Int32Operations.Minus {| v1 = () |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Minus v -> Some(Int32Operations.Minus v)
            | _ -> None)
        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(-v))
            } }

    let int32DivideId = Identifier.FullyQualified([ "Int32" ], "/")

    let divideOperation: Identifier * OperationExtension<'ext, Int32Operations<'ext>> =
      int32DivideId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Int32)
          )
        Kind = Kind.Star
        Operation = Int32Operations.Divide {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Divide v -> Some(Int32Operations.Divide v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int32Operations.AsDivide |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int32Operations.Divide({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(vClosure / v))
            } }

    let int32PowerId = Identifier.FullyQualified([ "Int32" ], "**")

    let powerOperation: Identifier * OperationExtension<'ext, Int32Operations<'ext>> =
      int32PowerId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Int32)
          )
        Kind = Kind.Star
        Operation = Int32Operations.Power {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Power v -> Some(Int32Operations.Power v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int32Operations.AsPower |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int32Operations.Power({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(pown vClosure v))
            } }

    let int32ModId = Identifier.FullyQualified([ "Int32" ], "%")

    let modOperation: Identifier * OperationExtension<'ext, Int32Operations<'ext>> =
      int32ModId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Int32)
          )
        Kind = Kind.Star
        Operation = Int32Operations.Mod {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Mod v -> Some(Int32Operations.Mod v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int32Operations.AsMod |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int32Operations.Mod({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int32(vClosure % v))
            } }

    let int32EqualId = Identifier.FullyQualified([ "Int32" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, Int32Operations<'ext>> =
      int32EqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int32Operations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.Equal v -> Some(Int32Operations.Equal v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int32Operations.AsEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int32Operations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let int32NotEqualId = Identifier.FullyQualified([ "Int32" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, Int32Operations<'ext>> =
      int32NotEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int32Operations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.NotEqual v -> Some(Int32Operations.NotEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int32Operations.AsNotEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int32Operations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let int32GreaterThanId = Identifier.FullyQualified([ "Int32" ], ">")

    let greaterThanOperation: Identifier * OperationExtension<'ext, Int32Operations<'ext>> =
      int32GreaterThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int32Operations.GreaterThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.GreaterThan v -> Some(Int32Operations.GreaterThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int32Operations.AsGreaterThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int32Operations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let res = vClosure > v

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(res))
            } }

    let int32GreaterThanOrEqualId = Identifier.FullyQualified([ "Int32" ], ">=")

    let greaterThanOrEqualOperation: Identifier * OperationExtension<'ext, Int32Operations<'ext>> =
      int32GreaterThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int32Operations.GreaterThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.GreaterThanOrEqual v -> Some(Int32Operations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int32Operations.AsGreaterThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  Int32Operations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let int32LessThanId = Identifier.FullyQualified([ "Int32" ], "<")

    let lessThanOperation: Identifier * OperationExtension<'ext, Int32Operations<'ext>> =
      int32LessThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int32Operations.LessThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.LessThan v -> Some(Int32Operations.LessThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int32Operations.AsLessThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int32Operations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let int32LessThanOrEqualId = Identifier.FullyQualified([ "Int32" ], "<=")

    let lessThanOrEqualOperation: Identifier * OperationExtension<'ext, Int32Operations<'ext>> =
      int32LessThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int32,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int32Operations.LessThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int32Operations.LessThanOrEqual v -> Some(Int32Operations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int32Operations.AsLessThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int32Operations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
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
