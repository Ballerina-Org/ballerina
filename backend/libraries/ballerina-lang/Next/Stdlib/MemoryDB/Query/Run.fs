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

  let MemoryDBQueryRunnerExtension<'ext when 'ext: comparison>
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : TypeExtension<'ext, Unit, Unit, MemoryDBValues<'ext>> =
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
            TypeExpr.Apply(
              TypeExpr.Apply(
                TypeExpr.Lookup("Query" |> Identifier.LocalScope),
                TypeExpr.Lookup("schema" |> Identifier.LocalScope)
              ),
              TypeExpr.Lookup("a" |> Identifier.LocalScope)
            ),
            TypeExpr.Arrow(
              TypeExpr.Tuple
                [ TypeExpr.Primitive PrimitiveType.Int32
                  TypeExpr.Primitive PrimitiveType.Int32 ],
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
        Operation = MemoryDBValues.GetById {| EntityRef = None |}
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.GetById v -> Some(MemoryDBValues.GetById v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsGetById
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v =
                  v
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
                  |> reader.OfSum

                let! v =
                  v
                  |> MemoryDBValues.AsEntityRef
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (MemoryDBValues.GetById({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBRunQueryId)
                  |> Ext
              | Some(_schema, _db, _entity, _schema_as_value) -> // the closure has the first operand - second step in the application
                let v =
                  option {
                    let! entity = _db.entities |> Map.tryFind _entity.Name
                    let! value = entity |> Map.tryFind v
                    return value
                  }

                match v with
                | None -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | Some value -> return Value.Sum({ Case = 2; Count = 2 }, value)
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
        Operation = MemoryDBValues.GetById {| EntityRef = None |}
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.GetById v -> Some(MemoryDBValues.GetById v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsGetById
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! v =
                  v
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
                  |> reader.OfSum

                let! v =
                  v
                  |> MemoryDBValues.AsEntityRef
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (MemoryDBValues.GetById({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBRunQueryId)
                  |> Ext
              | Some(_schema, _db, _entity, _schema_as_value) -> // the closure has the first operand - second step in the application
                let v =
                  option {
                    let! entity = _db.entities |> Map.tryFind _entity.Name
                    let! value = entity |> Map.tryFind v
                    return value
                  }

                match v with
                | None -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | Some value -> return Value.Sum({ Case = 2; Count = 2 }, value)
            } }

    { TypeName = queryId, querySymbolId
      TypeVars = [ (aVar, aKind); (schemaVar, schemaKind) ]
      Cases = Map.empty
      Operations =
        [ memoryDBRunQueryId, runQueryOperation
          memoryDBQueryToListId, queryToListOperation ]
        |> Map.ofList }
