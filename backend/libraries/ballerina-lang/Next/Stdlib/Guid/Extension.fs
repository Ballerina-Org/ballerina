namespace Ballerina.DSL.Next.StdLib.Guid

[<AutoOpen>]
module Extension =
  open Ballerina
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
  open FSharp.Data

  let GuidExtension<'ext, 'extDTO when 'extDTO: not null and 'extDTO: not struct>
    (operationLens: PartialLens<'ext, GuidOperations<'ext>>)
    // : TypeExtension<'ext, GuidConstructors, PrimitiveValue, GuidOperations<'ext>> =
    : OperationsExtension<'ext, GuidOperations<'ext>> =

    let boolTypeValue = TypeValue.CreatePrimitive PrimitiveType.Bool
    let guidTypeValue = TypeValue.CreatePrimitive PrimitiveType.Guid

    // let guidId = Identifier.LocalScope "guid"
    // let guidSymbolId = guidId |> TypeSymbol.Create
    // let guidId = guidId |> TypeCheckScope.Empty.Resolve

    // let guidConstructors = GuidConstructorsExtension<'ext> consLens

    let guidEqualId =
      Identifier.FullyQualified([ "guid" ], "==") |> TypeCheckScope.Empty.Resolve

    let equalOperation: ResolvedIdentifier * OperationExtension<'ext, GuidOperations<'ext>> =
      guidEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(guidTypeValue, TypeValue.CreateArrow(guidTypeValue, boolTypeValue)),
              Kind.Star,
              GuidOperations.Equal {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | GuidOperations.Equal v -> Some(GuidOperations.Equal v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> GuidOperations.AsEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsGuid
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (GuidOperations.Equal({| v1 = Some v |}) |> operationLens.Set, Some guidEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure = v))
            } }

    let guidNotEqualId =
      Identifier.FullyQualified([ "guid" ], "!=") |> TypeCheckScope.Empty.Resolve

    let notEqualOperation: ResolvedIdentifier * OperationExtension<'ext, GuidOperations<'ext>> =
      guidNotEqualId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(guidTypeValue, TypeValue.CreateArrow(guidTypeValue, boolTypeValue)),
              Kind.Star,
              GuidOperations.NotEqual {| v1 = None |})
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | GuidOperations.NotEqual v -> Some(GuidOperations.NotEqual v)
            | _ -> None)

        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> GuidOperations.AsNotEqual
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> PrimitiveValue.AsGuid
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                return
                  (GuidOperations.NotEqual({| v1 = Some v |}) |> operationLens.Set, Some guidNotEqualId)
                  |> Ext
              | Some vClosure -> // the closure has the first operand - second step in the application

                return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.Bool(vClosure <> v))
            } }

    let guidNewId = Identifier.FullyQualified([ "guid" ], "new")
    let guidNewId = guidNewId |> TypeCheckScope.Empty.Resolve

    let guidNew: ResolvedIdentifier * OperationExtension<'ext, GuidOperations<'ext>> =
      guidNewId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                TypeValue.CreatePrimitive PrimitiveType.String,
                TypeValue.CreateSum
                  [ TypeValue.CreatePrimitive PrimitiveType.Unit
                    TypeValue.CreatePrimitive PrimitiveType.Guid ]
              ),
              Kind.Star,
              Guid_New)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Guid_New -> Some(Guid_New)
            | _ -> None)
        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              match v with
              | Value.Primitive(PrimitiveValue.String v) ->
                return
                  try
                    Value.Sum({ Case = 2; Count = 2 }, Value.Primitive(PrimitiveValue.Guid(System.Guid.Parse(v))))
                  with _ ->
                    Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Unit))
              | _ -> return! sum.Throw(Errors.Singleton loc0 (fun () -> "Expected a string")) |> reader.OfSum
            } }

    let guidV4Id = Identifier.FullyQualified([ "guid" ], "v4")
    let guidV4Id = guidV4Id |> TypeCheckScope.Empty.Resolve

    let guidV4: ResolvedIdentifier * OperationExtension<'ext, GuidOperations<'ext>> =
      guidV4Id,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(
                TypeValue.CreatePrimitive PrimitiveType.Unit,
                TypeValue.CreatePrimitive PrimitiveType.Guid
              ),
              Kind.Star,
              GuidOperations.Guid_V4)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | GuidOperations.Guid_V4 -> Some(GuidOperations.Guid_V4)
            | _ -> None)

        Apply =
          fun loc0 _rest (_, v) ->
            reader {
              match v with
              | Value.Primitive PrimitiveValue.Unit ->
                return Value.Primitive(PrimitiveValue.Guid(System.Guid.NewGuid()))
              | _ -> return! sum.Throw(Errors.Singleton loc0 (fun () -> "Expected a unit")) |> reader.OfSum
            } }

    { TypeVars = []
      Operations = [ guidNew; guidV4; equalOperation; notEqualOperation ] |> Map.ofList }
