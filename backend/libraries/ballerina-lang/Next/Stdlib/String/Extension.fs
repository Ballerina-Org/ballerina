namespace Ballerina.DSL.Next.StdLib.String

[<AutoOpen>]
module Extension =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.Option
  open Ballerina

  let StringExtension<'ext>
    (operationLens: PartialLens<'ext, StringOperations<'ext>>)
    : OperationsExtension<'ext, StringOperations<'ext>> =

    let int32TypeValue = TypeValue.CreateInt32()
    let boolTypeValue = TypeValue.CreateBool()
    let stringTypeValue = TypeValue.CreateString()


    let stringLengthId =
      Identifier.FullyQualified([ "string" ], "length")
      |> TypeCheckScope.Empty.Resolve

    let lengthOperation: ResolvedIdentifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringLengthId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(stringTypeValue, int32TypeValue), Kind.Star, StringOperations.Length)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.Length -> Some(StringOperations.Length)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> StringOperations.AsLength
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum


              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Int32(v.Length))
            } }

    let stringPlusId =
      Identifier.FullyQualified([ "string" ], "+") |> TypeCheckScope.Empty.Resolve

    let plusOperation: ResolvedIdentifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringPlusId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(stringTypeValue, TypeValue.CreateArrow(stringTypeValue, stringTypeValue)),
              Kind.Star,
              StringOperations.Concat {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.Concat v -> Some(StringOperations.Concat v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> StringOperations.AsConcat
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (StringOperations.Concat({| v1 = Some v |}) |> operationLens.Set, Some stringPlusId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.String(vClosure + v))
            } }


    let stringEqualId =
      Identifier.FullyQualified([ "string" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation: ResolvedIdentifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(stringTypeValue, TypeValue.CreateArrow(stringTypeValue, boolTypeValue)),
              Kind.Star,
              StringOperations.Equal {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.Equal v -> Some(StringOperations.Equal v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> StringOperations.AsEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (StringOperations.Equal({| v1 = Some v |}) |> operationLens.Set, Some stringEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let stringNotEqualId =
      Identifier.FullyQualified([ "string" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation: ResolvedIdentifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringNotEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(stringTypeValue, TypeValue.CreateArrow(stringTypeValue, boolTypeValue)),
              Kind.Star,
              StringOperations.NotEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.NotEqual v -> Some(StringOperations.NotEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> StringOperations.AsNotEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (StringOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set, Some stringNotEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let stringGreaterThanId =
      Identifier.FullyQualified([ "string" ], ">") |> TypeCheckScope.Empty.Resolve

    let greaterThanOperation: ResolvedIdentifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringGreaterThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(stringTypeValue, TypeValue.CreateArrow(stringTypeValue, boolTypeValue)),
              Kind.Star,
              StringOperations.GreaterThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.GreaterThan v -> Some(StringOperations.GreaterThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> StringOperations.AsGreaterThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (StringOperations.GreaterThan({| v1 = Some v |}) |> operationLens.Set, Some stringGreaterThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure > v))
            } }

    let stringGreaterThanOrEqualId =
      Identifier.FullyQualified([ "string" ], ">=") |> TypeCheckScope.Empty.Resolve

    let greaterThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringGreaterThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(stringTypeValue, TypeValue.CreateArrow(stringTypeValue, boolTypeValue)),
              Kind.Star,
              StringOperations.GreaterThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.GreaterThanOrEqual v -> Some(StringOperations.GreaterThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> StringOperations.AsGreaterThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (StringOperations.GreaterThanOrEqual({| v1 = Some v |}) |> operationLens.Set,
                   Some stringGreaterThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure >= v))
            } }

    let stringLessThanId =
      Identifier.FullyQualified([ "string" ], "<") |> TypeCheckScope.Empty.Resolve

    let lessThanOperation: ResolvedIdentifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringLessThanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(stringTypeValue, TypeValue.CreateArrow(stringTypeValue, boolTypeValue)),
              Kind.Star,
              StringOperations.LessThan {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.LessThan v -> Some(StringOperations.LessThan v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> StringOperations.AsLessThan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (StringOperations.LessThan({| v1 = Some v |}) |> operationLens.Set, Some stringLessThanId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure < v))
            } }

    let stringLessThanOrEqualId =
      Identifier.FullyQualified([ "string" ], "<=") |> TypeCheckScope.Empty.Resolve

    let lessThanOrEqualOperation: ResolvedIdentifier * OperationExtension<'ext, StringOperations<'ext>> =
      stringLessThanOrEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(stringTypeValue, TypeValue.CreateArrow(stringTypeValue, boolTypeValue)),
              Kind.Star,
              StringOperations.LessThanOrEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | StringOperations.LessThanOrEqual v -> Some(StringOperations.LessThanOrEqual v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> StringOperations.AsLessThanOrEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (StringOperations.LessThanOrEqual({| v1 = Some v |}) |> operationLens.Set,
                   Some stringLessThanOrEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <= v))
            } }

    { TypeVars = []
      Operations =
        [ lengthOperation
          plusOperation
          equalOperation
          notEqualOperation
          greaterThanOperation
          greaterThanOrEqualOperation
          lessThanOperation
          lessThanOrEqualOperation ]
        |> Map.ofList }
