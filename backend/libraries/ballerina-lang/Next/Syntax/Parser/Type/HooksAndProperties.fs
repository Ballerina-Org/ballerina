namespace Ballerina.DSL.Next.Syntax.Parser

module TypeHooksAndProperties =

  open Ballerina.Parser
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Option
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Syntax.Lexer
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina

  type TypeExprParser<'valueExt> =
    Parser<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>>

  type TypeDeclParser<'valueExt> = Parser<TypeExpr<'valueExt>, LocalizedToken, Location, Errors<Location>>

  let relation_hooks (parseExpr: TypeExprParser<'valueExt>) () =
    let onHook (hookKeyword, hookKeywordParser) =
      parser {
        let! startsWithHookKeyword =
          parser {
            do! letKeyword
            do! onKeyword
            do! hookKeywordParser
          }
          |> parser.Lookahead
          |> parser.Try

        let! loc = parser.Location

        match startsWithHookKeyword with
        | Right _ -> return! parser.Throw(Errors.Singleton loc (fun () -> "No hook found"))
        | Left _ ->
          do! letKeyword
          do! onKeyword
          do! hookKeywordParser
          do! equalsOperator
          let! hookExpr = parseExpr
          return hookKeyword, hookExpr
      }

    [ (SchemaRelationHook.Linking, linkingKeyword) |> onHook
      (SchemaRelationHook.Linked, linkedKeyword) |> onHook
      (SchemaRelationHook.Unlinking, unlinkingKeyword) |> onHook
      (SchemaRelationHook.Unlinked, unlinkedKeyword) |> onHook ]
    |> parser.Any
    |> parser.Many
    |> parser.Map(Map.ofList)

  let entity_hooks (parseExpr: TypeExprParser<'valueExt>) () =
    let onHook (preHookKeyword) (hookKeyword, hookKeywordParser) =
      parser {
        let! startsWithHookKeyword =
          parser {
            do! letKeyword
            do! preHookKeyword
            do! hookKeywordParser
          }
          |> parser.Lookahead
          |> parser.Try

        let! loc = parser.Location

        match startsWithHookKeyword with
        | Right _ -> return! parser.Throw(Errors.Singleton loc (fun () -> "No hook found"))
        | Left _ ->
          do! letKeyword
          do! preHookKeyword
          do! hookKeywordParser
          do! equalsOperator
          let! hookExpr = parseExpr
          return hookKeyword, hookExpr
      }

    [ (SchemaEntityHook.Creating, creatingKeyword) |> onHook onKeyword
      (SchemaEntityHook.Created, createdKeyword) |> onHook onKeyword
      (SchemaEntityHook.Updating, updatingKeyword) |> onHook onKeyword
      (SchemaEntityHook.Updated, updatedKeyword) |> onHook onKeyword
      (SchemaEntityHook.Deleting, deletingKeyword) |> onHook onKeyword
      (SchemaEntityHook.Deleted, deletedKeyword) |> onHook onKeyword
      (SchemaEntityHook.Background, backgroundKeyword) |> onHook onKeyword
      (SchemaEntityHook.CanCreate, canCreateKeyword) |> onHook canKeyword
      (SchemaEntityHook.CanRead, canReadKeyword) |> onHook canKeyword
      (SchemaEntityHook.CanUpdate, canUpdateKeyword) |> onHook canKeyword
      (SchemaEntityHook.CanDelete, canDeleteKeyword) |> onHook canKeyword ]
    |> parser.Any
    |> parser.Many
    |> parser.Map(Map.ofList)

  let entity_properties
    (parseExpr: TypeExprParser<'valueExt>)
    parseSchemaPath
    (parseTypeDecl: unit -> TypeDeclParser<'valueExt>)
    ()
    : Parser<SchemaEntityPropertyExpr<'valueExt> list, LocalizedToken, Location, Errors<Location>> =
    parser.Many(
      parser {
        do! letKeyword
        do! propertyKeyword
        let! path = parseSchemaPath ()
        let! propertyName = singleIdentifier
        do! colonOperator
        let! propertyType = parseTypeDecl ()
        do! equalsOperator
        let! propertyBody = parseExpr

        return
          { SchemaEntityPropertyExpr.Name = LocalIdentifier.Create propertyName
            Path = path
            Type = propertyType
            Body = propertyBody }
      }
    )
    |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)

  let entity_vectors
    (parseExpr: TypeExprParser<'valueExt>)
    ()
    : Parser<SchemaEntityVectorExpr<'valueExt> list, LocalizedToken, Location, Errors<Location>> =
    parser.Many(
      parser {
        do! letKeyword
        let! loc = parser.Location
        let! stream = parser.Stream

        let loc =
          match stream |> Seq.tryHead with
          | Some token -> token.Location
          | None -> loc

        do!
          vectorKeyword
          |> parser.MapError(Errors.MapContext(replaceWith loc))
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
          |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)

        let! vectorName = singleIdentifier
        do! equalsOperator
        let! vectorBody = parseExpr

        return
          { SchemaEntityVectorExpr.Name = LocalIdentifier.Create vectorName
            Body = vectorBody }
      }
    )
    |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)
