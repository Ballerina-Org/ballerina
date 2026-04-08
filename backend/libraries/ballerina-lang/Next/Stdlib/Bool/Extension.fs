namespace Ballerina.DSL.Next.StdLib.Bool

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


  let BoolExtension<'runtimeContext, 'ext>
    (operationLens: PartialLens<'ext, BoolOperations<'ext>>)
    : OperationsExtension<'runtimeContext, 'ext, BoolOperations<'ext>> =

    let boolTypeValue = TypeValue.CreateBool()
    let stringTypeValue = TypeValue.CreatePrimitive PrimitiveType.String

    let boolToStringId =
      Identifier.FullyQualified([ "bool" ], "toString")
      |> TypeCheckScope.Empty.Resolve

    let toStringOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, BoolOperations<'ext>> =
      boolToStringId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(boolTypeValue, stringTypeValue), Kind.Star, BoolOperations.String)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | BoolOperations.String -> Some(BoolOperations.String)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> BoolOperations.AsToString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsBool
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.String(v |> string))
            } }

    let boolTryParseId =
      Identifier.FullyQualified([ "bool" ], "tryParse")
      |> TypeCheckScope.Empty.Resolve

    let tryParseOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, BoolOperations<'ext>> =
      boolTryParseId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                stringTypeValue,
                TypeValue.CreateSum
                  [ TypeValue.CreatePrimitive PrimitiveType.Unit
                    TypeValue.CreatePrimitive PrimitiveType.Bool ]
              ),
              Kind.Star,
              BoolOperations.TryParse)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | BoolOperations.TryParse -> Some(BoolOperations.TryParse)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> BoolOperations.AsTryParse
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

              return
                match System.Boolean.TryParse(v) with
                | true, result -> Value.Sum({ Case = 2; Count = 2 }, Value.Primitive(PrimitiveValue.Bool result))
                | false, _ -> Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Unit))
            } }

    let boolAndId =
      Identifier.FullyQualified([ "bool" ], "&&") |> TypeCheckScope.Empty.Resolve

    let andOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, BoolOperations<'ext>> =
      boolAndId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(boolTypeValue, TypeValue.CreateArrow(boolTypeValue, boolTypeValue)),
              Kind.Star,
              BoolOperations.And {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | BoolOperations.And v -> Some(BoolOperations.And v)
            | _ -> None)

        Apply =
          fun loc _rest (op, v) ->
            reader {
              let! op =
                op
                |> BoolOperations.AsAnd
                |> sum.MapError(Errors.MapContext(replaceWith loc))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsBool
                |> sum.MapError(Errors.MapContext(replaceWith loc))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (BoolOperations.And({| v1 = Some v |}) |> operationLens.Set, Some boolAndId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure && v))
            } }

    let boolOrId =
      Identifier.FullyQualified([ "bool" ], "||") |> TypeCheckScope.Empty.Resolve

    let orOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, BoolOperations<'ext>> =
      boolOrId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(boolTypeValue, TypeValue.CreateArrow(boolTypeValue, boolTypeValue)),
              Kind.Star,
              BoolOperations.Or {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | BoolOperations.Or v -> Some(BoolOperations.Or v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> BoolOperations.AsOr
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsBool
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (BoolOperations.Or({| v1 = Some v |}) |> operationLens.Set, Some boolOrId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure || v))
            } }

    let boolNotId =
      Identifier.FullyQualified([ "bool" ], "!") |> TypeCheckScope.Empty.Resolve

    let notOperation: ResolvedIdentifier * OperationExtension<'runtimeContext, 'ext, BoolOperations<'ext>> =
      boolNotId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(boolTypeValue, boolTypeValue), Kind.Star, BoolOperations.Not {| v1 = () |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | BoolOperations.Not v -> Some(BoolOperations.Not v)
            | _ -> None)

        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsBool
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(not v))
            } }

    { TypeVars = []
      Operations =
        [ toStringOperation
          tryParseOperation
          andOperation
          orOperation
          notOperation ]
        |> Map.ofList }
