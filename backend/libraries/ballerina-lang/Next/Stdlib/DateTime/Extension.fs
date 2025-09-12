namespace Ballerina.DSL.Next.StdLib.DateTime

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
  open System

  let DateTimeExtension<'ext>
    (operationLens: PartialLens<'ext, DateTimeOperations<'ext>>)
    : OperationsExtension<'ext, DateTimeOperations<'ext>> =
    let dateTimeEqualId = Identifier.FullyQualified([ "DateTime" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateTime,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateTime, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateTimeOperations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.Equal v -> Some(DateTimeOperations.Equal v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let dateTimeNotEqualId = Identifier.FullyQualified([ "DateTime" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeNotEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateTime,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateTime, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateTimeOperations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.NotEqual v -> Some(DateTimeOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsNotEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let dateTimeGreaterThanId = Identifier.FullyQualified([ "DateTime" ], ">")

    let greaterThanOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeGreaterThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateTime,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateTime, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateTimeOperations.GreaterThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.GreaterThan v -> Some(DateTimeOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsGreaterThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let dateTimeGreaterThanOrEqualId = Identifier.FullyQualified([ "DateTime" ], ">=")

    let greaterThanOrEqualOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeGreaterThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateTime,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateTime, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateTimeOperations.GreaterThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.GreaterThanOrEqual v -> Some(DateTimeOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsGreaterThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DateTimeOperations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let dateTimeLessThanId = Identifier.FullyQualified([ "DateTime" ], "<")

    let lessThanOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeLessThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateTime,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateTime, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateTimeOperations.LessThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.LessThan v -> Some(DateTimeOperations.LessThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsLessThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateTimeOperations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let dateTimeLessThanOrEqualId = Identifier.FullyQualified([ "DateTime" ], "<=")

    let lessThanOrEqualOperation: Identifier * OperationExtension<'ext, DateTimeOperations<'ext>> =
      dateTimeLessThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateTime,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateTime, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateTimeOperations.LessThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateTimeOperations.LessThanOrEqual v -> Some(DateTimeOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateTimeOperations.AsLessThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDateTime |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DateTimeOperations.LessThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    { TypeVars = []
      Operations =
        [ equalOperation
          notEqualOperation
          greaterThanOperation
          greaterThanOrEqualOperation
          lessThanOperation
          lessThanOrEqualOperation ]
        |> Map.ofList }
