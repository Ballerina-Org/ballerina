namespace Ballerina.DSL.Next.Syntax.Parser

module TypeSchema =

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

  let cardinality () =
    parser.Any
      [ parser {
          let! v = intLiteralToken ()

          if v = 0 then
            return Cardinality.Zero
          elif v = 1 then
            return Cardinality.One
          else
            let! loc = parser.Location

            return!
              (fun () -> $"Error: invalid cardinality value {v}, expected 0 or 1")
              |> Errors.Singleton loc
              |> parser.Throw
        }
        parser {
          do! starOperator
          return Cardinality.Many
        }
        parser {
          let! loc = parser.Location

          return!
            (fun () -> $"Error: invalid cardinality")
            |> Errors.Singleton loc
            |> parser.Throw
        }
        |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High)) ]
    |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)

  let rec schemaPath () =
    parser {
      match! openSquareBracketOperator |> parser.Try with
      | Right _ -> return None
      | Left _ ->
        return!
          parser {
            let! segments = schemaPathSegments ()
            do! closeSquareBracketOperator
            return Some segments
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
    }

  and schemaPathSegments () =
    parser.ManyIndex(fun i ->
      parser {
        if i > 0 then
          do! semicolonOperator

        let! var =
          parser {
            let! no_binding = letKeyword |> parser.Try

            match no_binding with
            | Right _ -> return None
            | Left _ ->
              let! id = singleIdentifier
              do! colonOperator
              let id = id |> LocalIdentifier.Create
              return id |> Some
          }

        let! res =
          parser.Any
            [ afterKeyword
                fieldKeyword
                (parser {
                  let! id = identifierLocalOrFullyQualified ()
                  return SchemaPathTypeDecompositionExpr.Field id
                })
              afterKeyword
                caseKeyword
                (parser.Any
                  [ parser {
                      let! id = identifierLocalOrFullyQualified ()
                      return SchemaPathTypeDecompositionExpr.UnionCase id
                    }
                    parser {
                      let! case = caseLiteral ()
                      return SchemaPathTypeDecompositionExpr.SumCase case
                    } ])
              afterKeyword
                itemKeyword
                (parser {
                  let! item = intLiteralToken ()
                  return SchemaPathTypeDecompositionExpr.Item { Index = item }
                })
              afterKeyword
                iteratorKeyword
                (parser {
                  do! openRoundBracketOperator
                  let! mapper = identifierLocalOrFullyQualified ()
                  do! commaOperator
                  let! containerType = identifierLocalOrFullyQualified ()
                  do! commaOperator
                  let! typeDef = identifierLocalOrFullyQualified ()
                  do! closeRoundBracketOperator

                  return
                    SchemaPathTypeDecompositionExpr.Iterator
                      {| Mapper = mapper
                         Container = containerType
                         TypeDef = typeDef |}
                }) ]
          |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)

        return var, res
      })
    |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)

  let relation_extension
    (parseExpr: TypeExprParser<'valueExt>)
    ()
    : Parser<(SchemaRelationName * SchemaRelationHooksExpr<'valueExt>), LocalizedToken, Location, Errors<Location>> =
    afterKeyword
      relationKeyword
      (parser {
        let! relationName = singleIdentifier
        do! openCurlyBracketOperator

        let! hooks = TypeHooksAndProperties.relation_hooks parseExpr ()

        let onLinking = hooks |> Map.tryFind SchemaRelationHook.Linking
        let onLinked = hooks |> Map.tryFind SchemaRelationHook.Linked
        let onUnlinking = hooks |> Map.tryFind SchemaRelationHook.Unlinking
        let onUnlinked = hooks |> Map.tryFind SchemaRelationHook.Unlinked

        let relationHooksExpr: SchemaRelationHooksExpr<'valueExt> =
          { SchemaRelationHooksExpr.OnLinking = onLinking
            SchemaRelationHooksExpr.OnLinked = onLinked
            SchemaRelationHooksExpr.OnUnlinking = onUnlinking
            SchemaRelationHooksExpr.OnUnlinked = onUnlinked }

        do! closeCurlyBracketOperator

        return { SchemaRelationName.Name = relationName }, relationHooksExpr
      })

  let relation
    (parseExpr: TypeExprParser<'valueExt>)
    ()
    : Parser<SchemaRelationExpr<'valueExt>, LocalizedToken, Location, Errors<Location>> =
    afterKeyword
      relationKeyword
      (parser {
        let! relationName = singleIdentifier
        do! openCurlyBracketOperator
        do! fromKeyword
        let! fromEntity = identifierLocalOrFullyQualified ()
        let! fromPath = schemaPath ()

        do! toKeyword
        let! toEntity = identifierLocalOrFullyQualified ()
        let! toPath = schemaPath ()

        let! hasCardinality = cardinalityKeyword |> parser.Try

        let! cardinality =
          match hasCardinality with
          | Right _ -> parser { return None }
          | Left() ->
            parser {
              let! from = cardinality ()
              do! doubleDotOperator
              let! to_ = cardinality ()

              return
                Some(
                  { SchemaRelationCardinality.From = from
                    To = to_ }
                )
            }


        let! hooks = TypeHooksAndProperties.relation_hooks parseExpr ()

        let onLinking = hooks |> Map.tryFind SchemaRelationHook.Linking
        let onLinked = hooks |> Map.tryFind SchemaRelationHook.Linked
        let onUnlinking = hooks |> Map.tryFind SchemaRelationHook.Unlinking
        let onUnlinked = hooks |> Map.tryFind SchemaRelationHook.Unlinked

        do! closeCurlyBracketOperator

        return
          { SchemaRelationExpr.Name = { SchemaRelationName.Name = relationName }
            From = (fromEntity, fromPath)
            To = (toEntity, toPath)
            Cardinality = cardinality
            Hooks =
              { OnLinking = onLinking
                OnLinked = onLinked
                OnUnlinking = onUnlinking
                OnUnlinked = onUnlinked } }
      })

  let entity
    (parseExpr: TypeExprParser<'valueExt>)
    (parseTypeDecl: unit -> TypeDeclParser<'valueExt>)
    ()
    : Parser<SchemaEntityExpr<'valueExt>, LocalizedToken, Location, Errors<Location>> =
    afterKeyword
      entityKeyword
      (parser {
        let! entityName = singleIdentifier
        do! openCurlyBracketOperator

        do!
          typeKeyword
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
          |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)

        let! entityType = parseTypeDecl ()
        let! idType = parseTypeDecl ()

        let! properties = TypeHooksAndProperties.entity_properties parseExpr schemaPath parseTypeDecl ()

        let! vectors = TypeHooksAndProperties.entity_vectors parseExpr ()

        let! hooks = TypeHooksAndProperties.entity_hooks parseExpr ()
        let onCreating = hooks |> Map.tryFind SchemaEntityHook.Creating
        let onCreated = hooks |> Map.tryFind SchemaEntityHook.Created
        let onUpdating = hooks |> Map.tryFind SchemaEntityHook.Updating
        let onUpdated = hooks |> Map.tryFind SchemaEntityHook.Updated
        let onDeleting = hooks |> Map.tryFind SchemaEntityHook.Deleting
        let onDeleted = hooks |> Map.tryFind SchemaEntityHook.Deleted
        let onBackground = hooks |> Map.tryFind SchemaEntityHook.Background
        let canCreate = hooks |> Map.tryFind SchemaEntityHook.CanCreate
        let canRead = hooks |> Map.tryFind SchemaEntityHook.CanRead
        let canUpdate = hooks |> Map.tryFind SchemaEntityHook.CanUpdate
        let canDelete = hooks |> Map.tryFind SchemaEntityHook.CanDelete

        let! hasUnexpectedLet = letKeyword |> parser.Lookahead |> parser.Try

        match hasUnexpectedLet with
        | Left _ ->
          let! loc = parser.Location
          let! stream = parser.Stream

          let loc =
            match stream |> Seq.tryHead with
            | Some token -> token.Location
            | None -> loc

          return!
            (fun () ->
              "Error: unexpected declaration in entity body. Expected property, vector, hook, or closing brace")
            |> Errors.Singleton loc
            |> Errors.MapPriority(replaceWith ErrorPriority.High)
            |> parser.Throw
        | Right _ -> ()

        let entityHooksExpr: SchemaEntityHooksExpr<'valueExt> =
          { OnCreating = onCreating
            OnCreated = onCreated
            OnUpdating = onUpdating
            OnUpdated = onUpdated
            OnDeleting = onDeleting
            OnDeleted = onDeleted
            OnBackground = onBackground
            CanCreate = canCreate
            CanRead = canRead
            CanUpdate = canUpdate
            CanDelete = canDelete }

        do! closeCurlyBracketOperator

        return
          { SchemaEntityExpr.Name = { SchemaEntityName.Name = entityName }
            Type = entityType
            Id = idType
            Properties = properties
            Vectors = vectors
            Hooks = entityHooksExpr }
      })

  let entity_extension
    (parseExpr: TypeExprParser<'valueExt>)
    ()
    : Parser<(SchemaEntityName * SchemaEntityHooksExpr<'valueExt>), LocalizedToken, Location, Errors<Location>> =
    afterKeyword
      entityKeyword
      (parser {
        let! entityName = singleIdentifier
        do! openCurlyBracketOperator

        let! hooks = TypeHooksAndProperties.entity_hooks parseExpr ()
        let onCreating = hooks |> Map.tryFind SchemaEntityHook.Creating
        let onCreated = hooks |> Map.tryFind SchemaEntityHook.Created
        let onUpdating = hooks |> Map.tryFind SchemaEntityHook.Updating
        let onUpdated = hooks |> Map.tryFind SchemaEntityHook.Updated
        let onDeleting = hooks |> Map.tryFind SchemaEntityHook.Deleting
        let onDeleted = hooks |> Map.tryFind SchemaEntityHook.Deleted
        let onBackground = hooks |> Map.tryFind SchemaEntityHook.Background
        let canCreate = hooks |> Map.tryFind SchemaEntityHook.CanCreate
        let canRead = hooks |> Map.tryFind SchemaEntityHook.CanRead
        let canUpdate = hooks |> Map.tryFind SchemaEntityHook.CanUpdate
        let canDelete = hooks |> Map.tryFind SchemaEntityHook.CanDelete

        let entityHooksExpr: SchemaEntityHooksExpr<'valueExt> =
          { OnCreating = onCreating
            OnCreated = onCreated
            OnUpdating = onUpdating
            OnUpdated = onUpdated
            OnDeleting = onDeleting
            OnDeleted = onDeleted
            OnBackground = onBackground
            CanCreate = canCreate
            CanRead = canRead
            CanUpdate = canUpdate
            CanDelete = canDelete }

        do! closeCurlyBracketOperator

        return { SchemaEntityName.Name = entityName }, entityHooksExpr
      })

  let schema
    (parseExpr: TypeExprParser<'valueExt>)
    (parseTypeDecl: unit -> TypeDeclParser<'valueExt>)
    ()
    : Parser<TypeExpr<'valueExt>, LocalizedToken, Location, Errors<Location>> =
    afterKeyword
      schemaKeyword
      (parser {
        let rec parseEntityOrRelationExtensions () =
          parser {
            let! hasClosed = closeCurlyBracketOperator |> parser.Lookahead |> parser.Try

            match hasClosed with
            | Left _ -> return []
            | Right _ ->
              let! head =
                parser {
                  let! isEntity = entityKeyword |> parser.Lookahead |> parser.Try

                  match isEntity with
                  | Left _ ->
                    return!
                      entity_extension parseExpr ()
                      |> parser.Map Sum.Left
                      |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)
                  | Right _ ->
                    let! isRelation = relationKeyword |> parser.Lookahead |> parser.Try

                    match isRelation with
                    | Left _ ->
                      return!
                        relation_extension parseExpr ()
                        |> parser.Map Sum.Right
                        |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)
                    | Right _ ->
                      let! loc = parser.Location
                      let! stream = parser.Stream

                      let loc =
                        match stream |> Seq.tryHead with
                        | Some token -> token.Location
                        | None -> loc

                      return!
                        (fun () -> "Expected 'entity' or 'relation' in schema include extensions")
                        |> Errors.Singleton loc
                        |> Errors.MapPriority(replaceWith ErrorPriority.High)
                        |> parser.Throw
                }
                |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)

              let! tail = parseEntityOrRelationExtensions ()
              return head :: tail
          }

        let rec parseEntitiesAndRelations () =
          parser {
            let! hasClosed = closeCurlyBracketOperator |> parser.Lookahead |> parser.Try

            match hasClosed with
            | Left _ -> return []
            | Right _ ->
              let! head =
                parser {
                  let! isEntity = entityKeyword |> parser.Lookahead |> parser.Try

                  match isEntity with
                  | Left _ ->
                    return!
                      entity parseExpr parseTypeDecl ()
                      |> parser.Map Sum.Left
                      |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)
                  | Right _ ->
                    let! isRelation = relationKeyword |> parser.Lookahead |> parser.Try

                    match isRelation with
                    | Left _ ->
                      return!
                        relation parseExpr ()
                        |> parser.Map Sum.Right
                        |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)
                    | Right _ ->
                      let! loc = parser.Location
                      let! stream = parser.Stream

                      let loc =
                        match stream |> Seq.tryHead with
                        | Some token -> token.Location
                        | None -> loc

                      return!
                        (fun () -> "Expected 'entity' or 'relation' in schema body")
                        |> Errors.Singleton loc
                        |> Errors.MapPriority(replaceWith ErrorPriority.High)
                        |> parser.Throw
                }
                |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)

              let! tail = parseEntitiesAndRelations ()
              return head :: tail
          }

        let! loc = parser.Location
        do! openCurlyBracketOperator

        let! includes =
          parser {
            let! hasInclude = includeKeyword |> parser.Try

            match hasInclude with
            | Right _ -> return None
            | Left() ->
              let! schemaName = singleIdentifier
              let! hasWith = withKeyword |> parser.Try

              let! entities, relations =
                match hasWith with
                | Right _ -> parser { return [], [] }
                | Left() ->
                  parser {
                    do! openCurlyBracketOperator

                    let! entitiesAndRelations = parseEntityOrRelationExtensions ()

                    let entities =
                      entitiesAndRelations
                      |> List.choose (function
                        | Left e -> Some e
                        | _ -> None)

                    let relations =
                      entitiesAndRelations
                      |> List.choose (function
                        | Right r -> Some r
                        | _ -> None)

                    do! closeCurlyBracketOperator

                    return entities, relations
                  }

              return (schemaName |> LocalIdentifier.Create, entities, relations) |> Some
          }

        let! entitiesAndRelations = parseEntitiesAndRelations ()

        let entities =
          entitiesAndRelations
          |> List.choose (function
            | Left e -> Some e
            | _ -> None)

        let relations =
          entitiesAndRelations
          |> List.choose (function
            | Right r -> Some r
            | _ -> None)

        do! closeCurlyBracketOperator

        return
          TypeExpr.Schema
            { SchemaExpr.DeclaredAtForNominalEquality = loc
              Includes = includes
              Entities = entities
              Relations = relations }
      })
