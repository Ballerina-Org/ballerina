namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module QueryOrderBy =
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

  let MemoryDBQueryOrderByExtension<'ext when 'ext: comparison> (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>) =

    let memoryDBQueryOrderById =
      Identifier.FullyQualified([ "MemoryDB" ], "queryOrderBy")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBQueryOrderByType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("a", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("b", Kind.Star),
            TypeExpr.Arrow(
              TypeExpr.Arrow(
                TypeExpr.Lookup("a" |> Identifier.LocalScope),
                TypeExpr.Lookup("b" |> Identifier.LocalScope)
              ),
              TypeExpr.Arrow(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Lookup("Query" |> Identifier.LocalScope),
                    TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("a" |> Identifier.LocalScope)
                ),
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Lookup("Query" |> Identifier.LocalScope),
                    TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("a" |> Identifier.LocalScope)
                )
              )
            )
          )
        )
      )


    let memoryDBQueryOrderByKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))

    let queryOrderByOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBQueryOrderByType, memoryDBQueryOrderByKind, MemoryDBValues.GetById {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.GetById v -> Some(MemoryDBValues.GetById v)
            | _ -> None)
        Apply =
          fun _loc0 _rest (_op, v) ->
            reader {
              let forwardToListMapExpr =
                Expr.Apply(
                  Expr.Lookup(
                    Identifier.FullyQualified([ "List" ], "orderBy")
                    |> ResolvedIdentifier.FromIdentifier
                  ),
                  Expr.FromValue(v, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                )

              return! NonEmptyList.OfList(forwardToListMapExpr, _rest) |> Expr.Eval
            } }

    memoryDBQueryOrderById, queryOrderByOperation
