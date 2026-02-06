namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module VectorEmbed =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Option
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker
  open System
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina
  open Ballerina.DSL.Next.StdLib.MemoryDB

  let embedder = new SmartComponents.LocalEmbeddings.LocalEmbedder()

  let MemoryDBQueryVectorEmbedExtension<'ext when 'ext: comparison>
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =

    let memoryDBQueryVectorEmbedId =
      Identifier.FullyQualified([ "MemoryDB" ], "vectorEmbed")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBQueryVectorEmbedType =
      TypeValue.CreateArrow(
        TypeValue.CreatePrimitive PrimitiveType.String,
        TypeValue.Lookup(Identifier.FullyQualified([ "MemoryDB" ], "Vector"))
      )

    let memoryDBQueryVectorEmbedKind = Kind.Star


    let queryVectorEmbedOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBQueryVectorEmbedType, memoryDBQueryVectorEmbedKind, MemoryDBValues.EmbedStringToVector())
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.EmbedStringToVector() -> Some(MemoryDBValues.EmbedStringToVector())
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> MemoryDBValues.AsEmbedStringToVector
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

              let v: SmartComponents.LocalEmbeddings.EmbeddingF32 = embedder.Embed v

              return Value.Ext(valueLens.Set(MemoryDBValues.VectorEmbedding { Vector = v }), None)
            } }

    memoryDBQueryVectorEmbedId, queryVectorEmbedOperation

  let MemoryDBQueryVectorSimilarityExtension<'ext when 'ext: comparison>
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =

    let memoryDBQueryVectorSimilarityId =
      Identifier.FullyQualified([ "MemoryDB" ], "vectorSimilarity")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBQueryVectorSimilarityType =
      TypeValue.CreateArrow(
        TypeValue.Lookup(Identifier.FullyQualified([ "MemoryDB" ], "Vector")),
        TypeValue.CreateArrow(
          TypeValue.Lookup(Identifier.FullyQualified([ "MemoryDB" ], "Vector")),
          TypeValue.CreatePrimitive PrimitiveType.Float32
        )
      )

    let memoryDBQueryVectorSimilarityKind = Kind.Star


    let queryVectorSimilarityOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBQueryVectorSimilarityType,
              memoryDBQueryVectorSimilarityKind,
              MemoryDBValues.VectorToVectorSimilarity {| Vector1 = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.VectorToVectorSimilarity v -> Some(MemoryDBValues.VectorToVectorSimilarity v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! v1 =
                op
                |> MemoryDBValues.AsVectorToVectorSimilarity
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match v1 with
              | None ->
                let! v1, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v1 =
                  v1
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Expected VectorEmbedding value"))
                  |> reader.OfSum

                let! v1 =
                  v1
                  |> MemoryDBValues.AsVectorEmbedding
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return Value.Ext(valueLens.Set(MemoryDBValues.VectorToVectorSimilarity {| Vector1 = Some v1 |}), None)


              | Some v1 ->
                let! v2, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v2 =
                  v2
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Expected VectorEmbedding value"))
                  |> reader.OfSum

                let! v2 =
                  v2
                  |> MemoryDBValues.AsVectorEmbedding
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum


                let similarity = v1.Vector.Similarity(v2.Vector)

                return Value.Primitive(PrimitiveValue.Float32 similarity)
            } }

    memoryDBQueryVectorSimilarityId, queryVectorSimilarityOperation
