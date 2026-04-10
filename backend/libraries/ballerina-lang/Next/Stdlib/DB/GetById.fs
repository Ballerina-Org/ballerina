namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module GetById =
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
  open Ballerina.DSL.Next.StdLib.DB

  let DBGetByIdExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =

    let memoryDBGetById =
      Identifier.FullyQualified([ "DB" ], "getById")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBGetByIdType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("entity", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("entity_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("entityId", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup("SchemaEntity" |> Identifier.LocalScope),
                        TypeExpr.Lookup("schema" |> Identifier.LocalScope)

                      ),
                      TypeExpr.Lookup("entity" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Lookup(
                      "entity_with_props" |> Identifier.LocalScope
                    )
                  ),
                  TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                ),
                TypeExpr.Arrow(
                  TypeExpr.Lookup("entityId" |> Identifier.LocalScope),
                  TypeExpr.Sum(
                    [ TypeExpr.Primitive PrimitiveType.Unit
                      TypeExpr.Lookup(
                        "entity_with_props" |> Identifier.LocalScope
                      ) ]
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBGetByIdKind =
      Kind.Arrow(
        Kind.Schema,
        Kind.Arrow(
          Kind.Star,
          Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
        )
      )

    let getByIdOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBGetByIdType,
              memoryDBGetByIdKind,
              DBValues.GetById {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.GetById v -> Some(DBValues.GetById v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsGetById
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
                  |> sum.OfOption(
                    Errors.Singleton loc0 (fun () ->
                      "Cannot get value from extension")
                  )
                  |> reader.OfSum

                let! v =
                  v
                  |> DBValues.AsEntityRef
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (DBValues.GetById({| EntityRef = Some v |}) |> valueLens.Set,
                   Some memoryDBGetById)
                  |> Ext
              | Some(entity_ref) -> // the closure has the first operand - second step in the application
                let! v =
                  db_ops.GetById entity_ref v
                  |> reader.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.Catch

                match v with
                | Right _ ->
                  return
                    Value.Sum(
                      { Case = 1; Count = 2 },
                      Value.Primitive PrimitiveValue.Unit
                    )
                | Left value -> return Value.Sum({ Case = 2; Count = 2 }, value)
            } }

    memoryDBGetById, getByIdOperation
