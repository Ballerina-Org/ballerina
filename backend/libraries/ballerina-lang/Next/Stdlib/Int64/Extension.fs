namespace Ballerina.DSL.Next.StdLib.Int64

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

  let Int64Extension<'ext>
    (operationLens: PartialLens<'ext, Int64Operations<'ext>>)
    : OperationsExtension<'ext, Int64Operations<'ext>> =

    let int64PlusId = Identifier.FullyQualified([ "Int64" ], "+")

    let plusOperation: Identifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64PlusId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int64, TypeValue.Primitive PrimitiveType.Int64)
          )
        Kind = Kind.Star
        Operation = Int64Operations.Plus {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Plus v -> Some(Int64Operations.Plus v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int64Operations.AsPlus |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.Plus({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int64(vClosure + v))
            } }

    let int64MinusId = Identifier.FullyQualified([ "Int64" ], "-")

    let minusOperation: Identifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64MinusId,
      { Type = TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int64, TypeValue.Primitive PrimitiveType.Int64)
        Kind = Kind.Star
        Operation = Int64Operations.Minus {| v1 = () |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Minus v -> Some(Int64Operations.Minus v)
            | _ -> None)
        Apply =
          fun (_, v) ->
            reader {
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt64 |> reader.OfSum

              return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int64(-v))
            } }

    let int64DivideId = Identifier.FullyQualified([ "Int64" ], "/")

    let divideOperation: Identifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64DivideId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int64, TypeValue.Primitive PrimitiveType.Int64)
          )
        Kind = Kind.Star
        Operation = Int64Operations.Divide {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Divide v -> Some(Int64Operations.Divide v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int64Operations.AsDivide |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.Divide({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int64(vClosure / v))
            } }

    let int64PowerId = Identifier.FullyQualified([ "Int64" ], "**")

    let powerOperation: Identifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64PowerId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Int64)
          )
        Kind = Kind.Star
        Operation = Int64Operations.Power {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Power v -> Some(Int64Operations.Power v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int64Operations.AsPower |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = v |> PrimitiveValue.AsInt64 |> reader.OfSum
                return Int64Operations.Power({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application
                let! v = v |> PrimitiveValue.AsInt32 |> reader.OfSum
                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int64(pown vClosure v))
            } }

    let int64ModId = Identifier.FullyQualified([ "Int64" ], "%")

    let modOperation: Identifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64ModId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int64, TypeValue.Primitive PrimitiveType.Int64)
          )
        Kind = Kind.Star
        Operation = Int64Operations.Mod {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Mod v -> Some(Int64Operations.Mod v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int64Operations.AsMod |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.Mod({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Int64(vClosure % v))
            } }

    let int64EqualId = Identifier.FullyQualified([ "Int64" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64EqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int64Operations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.Equal v -> Some(Int64Operations.Equal v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int64Operations.AsEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let int64NotEqualId = Identifier.FullyQualified([ "Int64" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64NotEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int64Operations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.NotEqual v -> Some(Int64Operations.NotEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int64Operations.AsNotEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let int64GreaterThanId = Identifier.FullyQualified([ "Int64" ], ">")

    let greaterThanOperation: Identifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64GreaterThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int64Operations.GreaterThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.GreaterThan v -> Some(Int64Operations.GreaterThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int64Operations.AsGreaterThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let int64GreaterThanOrEqualId = Identifier.FullyQualified([ "Int64" ], ">=")

    let greaterThanOrEqualOperation: Identifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64GreaterThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int64Operations.GreaterThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.GreaterThanOrEqual v -> Some(Int64Operations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int64Operations.AsGreaterThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  Int64Operations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let int64LessThanId = Identifier.FullyQualified([ "Int64" ], "<")

    let lessThanOperation: Identifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64LessThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int64Operations.LessThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.LessThan v -> Some(Int64Operations.LessThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int64Operations.AsLessThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let int64LessThanOrEqualId = Identifier.FullyQualified([ "Int64" ], "<=")

    let lessThanOrEqualOperation: Identifier * OperationExtension<'ext, Int64Operations<'ext>> =
      int64LessThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.Int64,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int64, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = Int64Operations.LessThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Int64Operations.LessThanOrEqual v -> Some(Int64Operations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> Int64Operations.AsLessThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsInt64 |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return Int64Operations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
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
