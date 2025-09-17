namespace Ballerina.DSL.Next.StdLib.String

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

  let StringExtension<'ext>
    (operationLens: PartialLens<'ext, StringOperations<'ext>>)
    : OperationsExtension<'ext, StringOperations<'ext>> =

    let stringPlusId = Identifier.FullyQualified([ "String" ], "+")

    let plusOperation: Identifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringPlusId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.String,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.String, TypeValue.Primitive PrimitiveType.String)
          )
        Kind = Kind.Star
        Operation = StringOperations.Concat {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.Concat v -> Some(StringOperations.Concat v)
            | _ -> None)

        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> StringOperations.AsConcat |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsString |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return StringOperations.Concat({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.String(vClosure + v))
            } }


    let stringEqualId = Identifier.FullyQualified([ "String" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.String,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.String, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = StringOperations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.Equal v -> Some(StringOperations.Equal v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> StringOperations.AsEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsString |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return StringOperations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let stringNotEqualId = Identifier.FullyQualified([ "String" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringNotEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.String,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.String, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = StringOperations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.NotEqual v -> Some(StringOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> StringOperations.AsNotEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsString |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return StringOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let stringGreaterThanId = Identifier.FullyQualified([ "String" ], ">")

    let greaterThanOperation: Identifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringGreaterThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.String,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.String, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = StringOperations.GreaterThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.GreaterThan v -> Some(StringOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> StringOperations.AsGreaterThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsString |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return StringOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let stringGreaterThanOrEqualId = Identifier.FullyQualified([ "String" ], ">=")

    let greaterThanOrEqualOperation: Identifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringGreaterThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.String,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.String, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = StringOperations.GreaterThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.GreaterThanOrEqual v -> Some(StringOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> StringOperations.AsGreaterThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsString |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  StringOperations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let stringLessThanId = Identifier.FullyQualified([ "String" ], "<")

    let lessThanOperation: Identifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringLessThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.String,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.String, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = StringOperations.LessThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.LessThan v -> Some(StringOperations.LessThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> StringOperations.AsLessThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsString |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return StringOperations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let stringLessThanOrEqualId = Identifier.FullyQualified([ "String" ], "<=")

    let lessThanOrEqualOperation: Identifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringLessThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.String,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.String, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = StringOperations.LessThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.LessThanOrEqual v -> Some(StringOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> StringOperations.AsLessThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsString |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return StringOperations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    { TypeVars = []
      Operations =
        [ plusOperation
          equalOperation
          notEqualOperation
          greaterThanOperation
          greaterThanOrEqualOperation
          lessThanOperation
          lessThanOrEqualOperation ]
        |> Map.ofList }
