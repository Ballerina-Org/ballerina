namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module QueryExpand =
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

  let MemoryDBQueryExpandExtension<'ext when 'ext: comparison>
    (listLens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =

    let memoryDBQueryExpandId =
      Identifier.FullyQualified([ "MemoryDB" ], "queryExpand")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBQueryExpandType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("a", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("b", Kind.Star),
            TypeExpr.Arrow(
              TypeExpr.Apply(
                TypeExpr.Apply(
                  TypeExpr.Lookup("Query" |> Identifier.LocalScope),
                  TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                ),
                TypeExpr.Lookup("a" |> Identifier.LocalScope)
              ),
              TypeExpr.Arrow(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Lookup("Query" |> Identifier.LocalScope),
                    TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("b" |> Identifier.LocalScope)
                ),
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Lookup("Query" |> Identifier.LocalScope),
                    TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Tuple
                    [ TypeExpr.Lookup("a" |> Identifier.LocalScope)
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Lookup("Query" |> Identifier.LocalScope),
                          TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                        ),
                        TypeExpr.Lookup("b" |> Identifier.LocalScope)
                      ) ]
                )
              )
            )
          )
        )
      )

    let memoryDBQueryExpandKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))

    let queryExpandOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBQueryExpandType, memoryDBQueryExpandKind, MemoryDBValues.QueryExpand {| Query1 = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.QueryExpand v -> Some(MemoryDBValues.QueryExpand v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsQueryExpand
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! query1 =
                  v
                  |> listLens.Get
                  |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
                  |> reader.OfSum

                return
                  (MemoryDBValues.QueryExpand({| Query1 = Some query1 |}) |> valueLens.Set, Some memoryDBQueryExpandId)
                  |> Ext
              | Some(query1) ->
                return
                  Ext(
                    listLens.Set
                      [ for e1 in query1 do
                          Value.Tuple [ e1; v ] ],
                    None
                  )
            } }

    memoryDBQueryExpandId, queryExpandOperation
