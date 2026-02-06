namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module QueryRunner =
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

  let MemoryDBQueryRunnerExtension<'ext, 'extDTO when 'ext: comparison and 'extDTO: not null and 'extDTO: not struct>
    (listLens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : TypeExtension<'ext, 'extDTO, Unit, Unit, MemoryDBValues<'ext>> =
    let queryId = Identifier.LocalScope "Query"
    let querySymbolId = queryId |> TypeSymbol.Create
    let schemaVar, schemaKind = TypeVar.Create("s"), Kind.Schema
    let aVar, aKind = TypeVar.Create("a"), Kind.Star
    let queryId = queryId |> TypeCheckScope.Empty.Resolve

    let memoryDBRunQueryId =
      Identifier.FullyQualified([ "MemoryDB" ], "runQuery")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBRunQueryType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("a", Kind.Star),
          TypeExpr.Arrow(
            TypeExpr.Tuple
              [ TypeExpr.Primitive PrimitiveType.Int32
                TypeExpr.Primitive PrimitiveType.Int32 ],
            TypeExpr.Arrow(
              TypeExpr.Apply(
                TypeExpr.Apply(
                  TypeExpr.Lookup("Query" |> Identifier.LocalScope),
                  TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                ),
                TypeExpr.Lookup("a" |> Identifier.LocalScope)
              ),
              TypeExpr.Apply(
                TypeExpr.Lookup("List" |> Identifier.LocalScope),
                TypeExpr.Lookup("a" |> Identifier.LocalScope)
              )
            )
          )
        )
      )

    let memoryDBRunQueryKind = Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Star))

    let runQueryOperation: TypeOperationExtension<'ext, Unit, Unit, MemoryDBValues<'ext>> =
      { Type = memoryDBRunQueryType
        Kind = memoryDBRunQueryKind
        Operation = MemoryDBValues.QueryRun {| Range = None |}
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.QueryRun v -> Some(MemoryDBValues.QueryRun v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsQueryRun
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! (skip, take) =
                  v
                  |> Value.AsTuple2
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! skip =
                  skip
                  |> Value.AsPrimitive
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! skip =
                  skip
                  |> PrimitiveValue.AsInt32
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! take =
                  take
                  |> Value.AsPrimitive
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! take =
                  take
                  |> PrimitiveValue.AsInt32
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (MemoryDBValues.QueryRun {| Range = Some(skip, take) |} |> valueLens.Set, Some memoryDBRunQueryId)
                  |> Ext
              | Some(skip, take) -> // the closure has the first operand - second step in the application
                let! v, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! query =
                  v
                  |> listLens.Get
                  |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
                  |> reader.OfSum

                let query = query |> List.skip skip |> List.truncate take
                return Ext(listLens.Set query, None)
            } }

    let memoryDBQueryToListId =
      Identifier.FullyQualified([ "MemoryDB" ], "toList")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBQueryToListType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("a", Kind.Star),
          TypeExpr.Arrow(
            TypeExpr.Apply(
              TypeExpr.Apply(
                TypeExpr.Lookup("Query" |> Identifier.LocalScope),
                TypeExpr.Lookup("schema" |> Identifier.LocalScope)
              ),
              TypeExpr.Lookup("a" |> Identifier.LocalScope)
            ),
            TypeExpr.Apply(
              TypeExpr.Lookup("List" |> Identifier.LocalScope),
              TypeExpr.Lookup("a" |> Identifier.LocalScope)
            )
          )
        )
      )

    let memoryDBQueryToListKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Star))

    let queryToListOperation: TypeOperationExtension<'ext, Unit, Unit, MemoryDBValues<'ext>> =
      { Type = memoryDBQueryToListType
        Kind = memoryDBQueryToListKind
        Operation = MemoryDBValues.QueryToList()
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.QueryToList() -> Some(MemoryDBValues.QueryToList())
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> MemoryDBValues.AsQueryToList
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum


              return v
            } }

    { TypeName = queryId, querySymbolId
      TypeVars = [ (aVar, aKind); (schemaVar, schemaKind) ]
      Cases = Map.empty
      Serialization = None
      Operations =
        [ memoryDBRunQueryId, runQueryOperation
          memoryDBQueryToListId, queryToListOperation ]
        |> Map.ofList }
