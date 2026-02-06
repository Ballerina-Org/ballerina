namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module QueryWhere =
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

  let MemoryDBQueryWhereExtension<'ext when 'ext: comparison> (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>) =

    let memoryDBQueryWhereId =
      Identifier.FullyQualified([ "MemoryDB" ], "queryWhere")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBQueryWhereType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("a", Kind.Star),
          TypeExpr.Arrow(
            TypeExpr.Arrow(TypeExpr.Lookup("a" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Bool),
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


    let memoryDBQueryWhereKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Star))

    let queryWhereOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBQueryWhereType, memoryDBQueryWhereKind, MemoryDBValues.GetById {| EntityRef = None |})
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
                    Identifier.FullyQualified([ "List" ], "filter")
                    |> ResolvedIdentifier.FromIdentifier
                  ),
                  Expr.FromValue(v, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                )

              return! NonEmptyList.OfList(forwardToListMapExpr, _rest) |> Expr.Eval
            } }

    memoryDBQueryWhereId, queryWhereOperation
