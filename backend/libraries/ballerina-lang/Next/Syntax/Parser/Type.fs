namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Type =

  open System
  open Ballerina.Collections.Option
  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Syntax
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms

  type ComplexTypeKind =
    | ScopedIdentifier
    | ApplicationArguments
    | BinaryExpressionChain
    | RecordOrTupleDesChain

  type ComplexType<'valueExt> =
    | ScopedIdentifier of NonEmptyList<string>
    | ApplicationArguments of NonEmptyList<TypeExpr<'valueExt>>
    | BinaryExpressionChain of NonEmptyList<BinaryTypeOperator * TypeExpr<'valueExt>>
    | RecordOrTupleDesChain of NonEmptyList<Sum<string, int>>

  let parseAllComplexTypeShapes: Set<ComplexTypeKind> =
    [ ComplexTypeKind.ApplicationArguments
      ComplexTypeKind.BinaryExpressionChain
      ComplexTypeKind.RecordOrTupleDesChain
      ComplexTypeKind.ScopedIdentifier ]
    |> Set.ofList

  let parseNoComplexTypeShapes: Set<ComplexTypeKind> = Set.empty

  let typeParam =
    parser {
      do! openSquareBracketOperator
      let! paramName = singleIdentifier
      do! colonOperator
      let! kind = kindDecl ()
      do! closeSquareBracketOperator
      return paramName, kind
    }

  let rec typeDecl
    (parseExpr: Parser<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors>)
    (parseComplexShapes: Set<ComplexTypeKind>)
    : Parser<TypeExpr<'valueExt>, _, _, Errors> =
    let lookupTypeDecl () =
      parser {
        let! id = singleIdentifier
        return TypeExpr.Lookup(Identifier.LocalScope id)
      }

    let boolTypeDecl () =
      parser {
        let! id = singleIdentifier

        match id with
        | "bool" -> return TypeExpr.Primitive PrimitiveType.Bool
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected bool, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let int32TypeDecl () =
      parser {
        let! id = singleIdentifier

        match id with
        | "int32" -> return TypeExpr.Primitive PrimitiveType.Int32
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected int32, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let int64TypeDecl () =
      parser {
        let! id = singleIdentifier

        match id with
        | "int64" -> return TypeExpr.Primitive PrimitiveType.Int64
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected int64, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let float32TypeDecl () =
      parser {
        let! id = singleIdentifier

        match id with
        | "float32" -> return TypeExpr.Primitive PrimitiveType.Float32
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected float32, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let float64TypeDecl () =
      parser {
        let! id = singleIdentifier

        match id with
        | "float64" -> return TypeExpr.Primitive PrimitiveType.Float64
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected float64, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let decimalTypeDecl () =
      parser {
        let! id = singleIdentifier

        match id with
        | "decimal" -> return TypeExpr.Primitive PrimitiveType.Decimal
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected decimal, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let stringTypeDecl () =
      parser {
        let! id = singleIdentifier

        match id with
        | "string" -> return TypeExpr.Primitive PrimitiveType.String
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected string, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let guidTypeDecl () =
      parser {
        let! id = singleIdentifier

        match id with
        | "guid" -> return TypeExpr.Primitive PrimitiveType.Guid
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected guid, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let unitTypeDecl () =
      parser {
        do! openRoundBracketOperator
        do! closeRoundBracketOperator
        return TypeExpr.Primitive PrimitiveType.Unit
      }

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
                (loc, $"Error: invalid cardinality value {v}, expected 0 or 1")
                |> Errors.Singleton
                |> parser.Throw
          }
          parser {
            do! starOperator
            return Cardinality.Many
          } ]

    let schemaPathSegments () =
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
              [ parser {
                  do! fieldKeyword
                  let! id = identifierLocalOrFullyQualified ()
                  return SchemaPathTypeDecompositionExpr.Field id
                }
                parser {
                  do! caseKeyword
                  let! id = identifierLocalOrFullyQualified ()
                  return SchemaPathTypeDecompositionExpr.UnionCase id
                }
                parser {
                  do! caseKeyword
                  let! case = caseLiteral ()
                  return SchemaPathTypeDecompositionExpr.SumCase case
                }
                parser {
                  do! itemKeyword
                  let! item = intLiteralToken ()
                  return SchemaPathTypeDecompositionExpr.Item { Index = item }
                }
                parser {
                  do! iteratorKeyword
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
                } ]

          return var, res
        })

    let schemaPath () =
      parser {
        match! openSquareBracketOperator |> parser.Try with
        | Right _ -> return None
        | Left _ ->

          let! segments = schemaPathSegments ()

          do! closeSquareBracketOperator

          return Some segments
      }

    let relation () =
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

          do! closeCurlyBracketOperator

          return
            { SchemaRelationExpr.Name = { SchemaRelationName.Name = relationName }
              From = (fromEntity, fromPath)
              To = (toEntity, toPath)
              Cardinality = cardinality }
        })

    let entity () =
      parser {
        do! entityKeyword

        return!
          parser {
            let! entityName = singleIdentifier
            do! openCurlyBracketOperator
            do! typeKeyword
            let! entityType = typeDecl parseExpr parseAllComplexTypeShapes
            let! idType = typeDecl parseExpr parseAllComplexTypeShapes
            let! searchBy = searchByKeyword |> parser.Try

            let! searchBy =
              parser {
                match searchBy with
                | Right _ -> return []
                | Left() ->
                  do! openSquareBracketOperator

                  let! ids =
                    parser.ManyIndex(fun i ->
                      parser {
                        if i > 0 then
                          do! semicolonOperator

                        let! id, lookups = identifierWithLookups ()
                        return { Identifier = id; Lookups = lookups }
                      })

                  do! closeSquareBracketOperator
                  return ids
              }

            let! properties =
              parser.Many(
                parser {
                  do! propertyKeyword
                  let! path = schemaPath ()
                  let! propertyName = singleIdentifier
                  do! colonOperator
                  let! propertyType = typeDecl parseExpr parseAllComplexTypeShapes
                  do! equalsOperator
                  let! propertyBody = parseExpr

                  return
                    { SchemaEntityPropertyExpr.Name = LocalIdentifier.Create propertyName
                      Path = path
                      Type = propertyType
                      Body = propertyBody }
                }
              )

            do! closeCurlyBracketOperator

            return
              { SchemaEntityExpr.Name = { SchemaEntityName.Name = entityName }
                Type = entityType
                Id = idType
                SearchBy = searchBy
                Properties = properties }
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let schema () =
      afterKeyword
        schemaKeyword
        (parser {
          let! loc = parser.Location
          do! openCurlyBracketOperator

          let! entitiesAndRelations =
            parser.Many(parser.Any [ entity () |> parser.Map Sum.Left; relation () |> parser.Map Sum.Right ])

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
                Entities = entities
                Relations = relations }
        })

    let record () =
      parser {
        do! openCurlyBracketOperator

        return!
          parser {
            let! fields =
              parser.ManyIndex(fun i ->
                parser {
                  if i > 0 then
                    do! semicolonOperator

                  let! id = singleIdentifier
                  do! colonOperator

                  return!
                    parser {
                      let! typeDecl = typeDecl parseExpr parseAllComplexTypeShapes
                      return (id, typeDecl)
                    }
                    |> parser.MapError(Errors.SetPriority ErrorPriority.High)
                })

            do! semicolonOperator |> parser.Try |> parser.Ignore
            do! closeCurlyBracketOperator

            return
              TypeExpr.Record(
                fields
                |> List.map (fun (id, td) -> (id |> Identifier.LocalScope |> TypeExpr.Lookup, td))
              )
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.Medium)

      }

    let unionTypeDecl () =
      parser {
        do! pipeOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            let! cases =

              parser.AtLeastOne(
                parser {
                  do! pipeOperator

                  return!
                    parser {
                      let! id = singleIdentifier
                      do! ofKeyword
                      let! typeDecl = typeDecl parseExpr parseAllComplexTypeShapes
                      return (id, typeDecl)
                    }
                    |> parser.MapError(Errors.SetPriority ErrorPriority.High)
                }
              )
              |> parser.Map(NonEmptyList.ToList)

            return
              TypeExpr.Union(
                cases
                |> List.map (fun (id, td) -> (id |> Identifier.LocalScope |> TypeExpr.Lookup, td))
              )
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let scopedIdentifier () =
      parser {
        do! doubleColonOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            let! ids =
              parser.AtLeastOne(
                parser {
                  do! doubleColonOperator
                  return! singleIdentifier
                }
              )

            return ids |> ScopedIdentifier
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let binaryExpressionChainTail () =
      parser {
        do! binaryTypeOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            let! fields =
              parser.AtLeastOne(
                parser {
                  let! op = binaryTypeOperator

                  let! value =
                    typeDecl parseExpr (parseComplexShapes |> Set.remove ComplexTypeKind.BinaryExpressionChain)

                  return op, value
                }
              )

            return fields |> ComplexType.BinaryExpressionChain
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let recordDes () =
      parser {
        do! dotOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            let! fields =
              parser.AtLeastOne(
                parser {
                  do! dotOperator
                  return! parser.Any [ singleIdentifier |> parser.Map Left; intLiteralToken () |> parser.Map Right ]
                }
              )

            return fields |> ComplexType.RecordOrTupleDesChain
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }


    let application () =
      parser {
        let! args =
          parser.AtLeastOne(
            (fun () -> typeDecl parseExpr parseAllComplexTypeShapes)
            |> betweenSquareBrackets
          )

        return args |> ComplexType.ApplicationArguments
      }

    let typeLambda () =
      parser {
        let! pars = parser.AtLeastOne typeParam
        do! parseOperator Operator.SingleArrow
        let pars = pars |> NonEmptyList.ToSeq
        let! body = typeDecl parseExpr parseAllComplexTypeShapes

        return
          Seq.foldBack
            (fun (par_name, par_kind) (body: TypeExpr<'valueExt>) ->
              TypeExpr.Lambda(TypeParameter.Create(par_name, par_kind), body))
            pars
            body
      }

    let simpleShapes =
      [ (fun () -> typeDecl parseExpr parseAllComplexTypeShapes) |> betweenBrackets
        typeLambda ()
        schema ()
        record ()
        unionTypeDecl ()
        unitTypeDecl ()
        boolTypeDecl ()
        int32TypeDecl ()
        int64TypeDecl ()
        float32TypeDecl ()
        float64TypeDecl ()
        decimalTypeDecl ()
        stringTypeDecl ()
        guidTypeDecl ()
        lookupTypeDecl () ]

    parser {
      // let! s = parser.Stream

      // do
      //   Console.WriteLine(
      //     $"expr(parseComplexShapes={parseComplexShapes}) Stream = {s |> Seq.map (fun t -> t.Token.ToString()) |> Seq.truncate 10 |> Seq.toList}"
      //   )

      // do Console.ReadLine() |> ignore

      if parseComplexShapes |> Set.isEmpty then
        return! simpleShapes |> parser.Any
      else
        // let! s = parser.Stream

        // do
        //   Console.WriteLine(
        //     $"{String.replicate (depth * 2) indent}> nested-expr(parseComplexShapes=false) Stream = {s |> Seq.map (fun t -> t.Token.ToString()) |> Seq.truncate 10 |> Seq.toList}"
        //   )

        // do Console.ReadLine() |> ignore
        let! e = typeDecl parseExpr parseNoComplexTypeShapes
        // do Console.Write $"{e.ToFSharpString}"
        // do Console.WriteLine $"included = {parseComplexShapes.ToFSharpString}"
        // do Console.ReadLine() |> ignore

        // do
        //   Console.WriteLine(
        //     $"{String.replicate (depth * 2) indent}> Trying to parse complex shape starting with {e.ToFSharpString}"
        //   )

        // do Console.ReadLine() |> ignore

        let complexShapes = []

        let complexShapes =
          if parseComplexShapes.Contains ComplexTypeKind.ScopedIdentifier then
            scopedIdentifier () :: complexShapes
          else
            complexShapes

        let complexShapes =
          if parseComplexShapes.Contains ComplexTypeKind.BinaryExpressionChain then
            binaryExpressionChainTail () :: complexShapes
          else
            complexShapes

        let complexShapes =
          if parseComplexShapes.Contains ComplexTypeKind.ApplicationArguments then
            application () :: complexShapes
          else
            complexShapes

        let complexShapes =
          if parseComplexShapes.Contains ComplexTypeKind.RecordOrTupleDesChain then
            recordDes () :: complexShapes
          else
            complexShapes

        let! res = complexShapes |> parser.Any |> parser.Many

        // do Console.WriteLine $"~~{res.ToFSharpString}"
        // do Console.ReadLine() |> ignore
        let! loc = parser.Location

        let res =
          res
          |> List.fold
            (fun acc e ->
              sum {
                let! acc = acc

                match e with
                | BinaryExpressionChain fields ->
                  let fields: List<BinaryOperatorsElement<TypeExpr<'valueExt>, BinaryTypeOperator>> =
                    fields
                    |> NonEmptyList.ToList
                    |> Seq.collect (fun (op, e) -> [ op |> Precedence.Operator; e |> Precedence.Operand ])
                    |> List.ofSeq

                  let chain = Operand acc :: fields

                  let precedence: List<OperatorsPrecedence<BinaryTypeOperator>> =
                    [ { Operators = [ BinaryTypeOperator.SingleArrow ] |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators = [ BinaryTypeOperator.Times ] |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators = [ BinaryTypeOperator.Plus ] |> Set.ofList
                        Associativity = AssociateLeft } ]

                  return!
                    collapseBinaryOperatorsChain
                      { Compose =
                          fun (e1, op, e2) ->
                            match op with
                            | BinaryTypeOperator.Times ->
                              match e1, e2 with
                              | TypeExpr.Tuple l1, TypeExpr.Tuple l2 -> TypeExpr.Tuple(l1 @ l2)
                              | TypeExpr.Tuple l1, _ -> TypeExpr.Tuple(l1 @ [ e2 ])
                              | _, TypeExpr.Tuple l2 -> TypeExpr.Tuple(e1 :: l2)
                              | _ -> TypeExpr.Tuple [ e1; e2 ]
                            | BinaryTypeOperator.Plus ->
                              match e1, e2 with
                              | TypeExpr.Sum l1, TypeExpr.Sum l2 -> TypeExpr.Sum(l1 @ l2)
                              | TypeExpr.Sum l1, _ -> TypeExpr.Sum(l1 @ [ e2 ])
                              | _, TypeExpr.Sum l2 -> TypeExpr.Sum(e1 :: l2)
                              | _ -> TypeExpr.Sum [ e1; e2 ]
                            | BinaryTypeOperator.SingleArrow -> TypeExpr.Arrow(e1, e2)
                        // | _ ->
                        //   TypeExpr.Apply(
                        //     TypeExpr.Apply(TypeExpr.Lookup(Identifier.LocalScope(op.ToString())), e1),
                        //     e2
                        //   )
                        ToExpr = id }
                      loc
                      precedence
                      chain
                | ScopedIdentifier ids ->
                  match acc with
                  | TypeExpr.Lookup(Identifier.LocalScope id) ->
                    let ids = (id :: (ids |> NonEmptyList.ToList)) |> List.rev
                    return TypeExpr.Lookup(Identifier.FullyQualified(ids.Tail, ids.Head))
                  | _ ->
                    return!
                      (loc, $"Error: cannot collapse scoped identifier chain on non-identifier")
                      |> Errors.Singleton
                      |> sum.Throw
                | ApplicationArguments args ->
                  return
                    args
                    |> NonEmptyList.ToList
                    |> List.fold (fun acc e -> TypeExpr.Apply(acc, e)) acc
                | RecordOrTupleDesChain segments ->
                  return
                    segments
                    |> NonEmptyList.ToList
                    |> List.fold
                      (fun acc e ->
                        match e with
                        | Left field_name -> TypeExpr.RecordDes(acc, Sum.Left(LocalIdentifier.Create field_name))
                        | Right index -> TypeExpr.RecordDes(acc, Sum.Right index))
                      acc
              })
            (Sum.Left e)

        // let! s = parser.Stream

        // do
        //   Console.WriteLine(
        //     $"{String.replicate (depth * 2) indent}> success! nested-expr(parseComplexShapes=false) = {res.ToFSharpString}, remaining stream = {s |> Seq.map (fun t -> t.Token.ToString()) |> Seq.truncate 10 |> Seq.toList}"
        //   )

        match res with
        | Sum.Right e -> return! e |> parser.Throw
        | Sum.Left res -> return res
    }
