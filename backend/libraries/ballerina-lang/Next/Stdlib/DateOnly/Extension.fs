namespace Ballerina.DSL.Next.StdLib.DateOnly

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

  let DateOnlyExtension<'ext>
    (operationLens: PartialLens<'ext, DateOnlyOperations<'ext>>)
    : OperationsExtension<'ext, DateOnlyOperations<'ext>> =

    let dateOnlyEqualId = Identifier.FullyQualified([ "DateOnly" ], "==")

    let equalOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateOnly,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateOnly, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateOnlyOperations.Equal {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.Equal v -> Some(DateOnlyOperations.Equal v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateOnlyOperations.AsEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateOnlyOperations.Equal({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let dateOnlyNotEqualId = Identifier.FullyQualified([ "DateOnly" ], "!=")

    let notEqualOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyNotEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateOnly,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateOnly, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateOnlyOperations.NotEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.NotEqual v -> Some(DateOnlyOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateOnlyOperations.AsNotEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateOnlyOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let dateOnlyGreaterThanId = Identifier.FullyQualified([ "DateOnly" ], ">")

    let greaterThanOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyGreaterThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateOnly,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateOnly, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateOnlyOperations.GreaterThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.GreaterThan v -> Some(DateOnlyOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateOnlyOperations.AsGreaterThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateOnlyOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let dateOnlyGreaterThanOrEqualId = Identifier.FullyQualified([ "DateOnly" ], ">=")

    let greaterThanOrEqualOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyGreaterThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateOnly,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateOnly, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateOnlyOperations.GreaterThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.GreaterThanOrEqual v -> Some(DateOnlyOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateOnlyOperations.AsGreaterThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DateOnlyOperations.GreaterThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let dateOnlyLessThanId = Identifier.FullyQualified([ "DateOnly" ], "<")

    let lessThanOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyLessThanId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateOnly,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateOnly, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateOnlyOperations.LessThan {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.LessThan v -> Some(DateOnlyOperations.LessThan v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateOnlyOperations.AsLessThan |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return DateOnlyOperations.LessThan({| v1 = Some v |}) |> operationLens.Set |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let dateOnlyLessThanOrEqualId = Identifier.FullyQualified([ "DateOnly" ], "<=")

    let lessThanOrEqualOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
      dateOnlyLessThanOrEqualId,
      { Type =
          TypeValue.Arrow(
            TypeValue.Primitive PrimitiveType.DateOnly,
            TypeValue.Arrow(TypeValue.Primitive PrimitiveType.DateOnly, TypeValue.Primitive PrimitiveType.Bool)
          )
        Kind = Kind.Star
        Operation = DateOnlyOperations.LessThanOrEqual {| v1 = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | DateOnlyOperations.LessThanOrEqual v -> Some(DateOnlyOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun (op, v) ->
            reader {
              let! op = op |> DateOnlyOperations.AsLessThanOrEqual |> reader.OfSum
              let! v = v |> Value.AsPrimitive |> reader.OfSum
              let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  DateOnlyOperations.LessThanOrEqual({| v1 = Some v |})
                  |> operationLens.Set
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }


    // let dateOnlyToDateTimeId = Identifier.FullyQualified([ "DateOnly" ], "toDateTime")

    // let toDateTimeOperation: Identifier * OperationExtension<'ext, DateOnlyOperations<'ext>> =
    //   dateOnlyToDateTimeId,
    //   { Type =
    //       TypeValue.Arrow(
    //         TypeValue.Primitive PrimitiveType.DateOnly,
    //         TypeValue.Arrow(
    //           TypeValue.Primitive PrimitiveType.Int32,
    //           TypeValue.Arrow(
    //             TypeValue.Primitive PrimitiveType.Int32,
    //             TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.DateTime)
    //           )
    //         )
    //       )
    //     Kind = Kind.Star
    //     Operation = DateOnlyOperations.ToDateTime {| v1 = None |}
    //     OperationsLens =
    //       operationLens
    //       |> PartialLens.BindGet (function
    //         | DateOnlyOperations.ToDateTime v -> Some(DateOnlyOperations.ToDateTime v)
    //         | _ -> None)
    //     Apply =
    //       fun (op, v) ->
    //         reader {
    //           let! op = op |> DateOnlyOperations.AsToDateTime |> reader.OfSum
    //           let! v = v |> Value.AsPrimitive |> reader.OfSum
    //           let! v = v |> PrimitiveValue.AsDate |> reader.OfSum

    //           match op with
    //           | None -> // the closure is empty - first step in the application
    //             return DateOnlyOperations.ToDateTime({| v1 = Some v |}) |> operationLens.Set |> Ext
    //           | Some vClosure -> // the closure has the first operand - second step in the application

    //             return Value<TypeValue, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
    //         } }

    { TypeVars = []
      Operations =
        [ equalOperation
          notEqualOperation
          greaterThanOperation
          greaterThanOrEqualOperation
          lessThanOperation
          lessThanOrEqualOperation ]
        |> Map.ofList }
