namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Expr =

  open System
  open Ballerina.Collections.Option
  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Model
  open Common
  open Precedence
  open Type
  open Ballerina.DSL.Next.Syntax
  open Ballerina
  open Ballerina.Grammar

  type ComplexExpressionKind =
    | ScopedIdentifier
    | RecordDes
    | TupleCons
    | ApplicationArguments
    | BinaryExpressionChain

  type ComplexExpression<'valueExt> =
    | ScopedIdentifier of NonEmptyList<string>
    | DanglingScopedIdentifier of Location
    | RecordOrTupleDesChain of NonEmptyList<Sum<string, int>>
    | DanglingRecordDes of Location
    | TupleCons of
      NonEmptyList<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
    | ApplicationArguments of
      NonEmptyList<
        Sum<
          Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          TypeExpr<'valueExt>
         >
       >
    | BinaryExpressionChain of
      NonEmptyList<
        BinaryExprOperator * Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>
       >

  let private parseAllComplexShapes: Set<ComplexExpressionKind> =
    [ ComplexExpressionKind.ApplicationArguments
      ComplexExpressionKind.BinaryExpressionChain
      ComplexExpressionKind.RecordDes
      ComplexExpressionKind.TupleCons
      ComplexExpressionKind.ScopedIdentifier ]
    |> Set.ofList

  let private parseNoComplexShapes: Set<ComplexExpressionKind> = Set.empty

  let exprRule: NamedRule =
    { Name = "expr"
      Rule =
        Alt
          [ NonTerminal "string-literal"; NonTerminal "int-literal"; NonTerminal "int64-literal"
            NonTerminal "decimal-literal"; NonTerminal "float32-literal"; NonTerminal "float64-literal"
            NonTerminal "bool-literal"; NonTerminal "unit-literal"
            NonTerminal "match-expr"; NonTerminal "lambda-expr"
            NonTerminal "let-expr"; NonTerminal "do-expr"
            NonTerminal "conditional-expr"; NonTerminal "record-cons"
            NonTerminal "type-let"; NonTerminal "identifier-lookup"
            NonTerminal "application" ] }

  let rec expr
    (depth: int)
    (parseComplexShapes: Set<ComplexExpressionKind>)
    =

    let expr = expr (depth + 1)
    let singleIdentifier = singleTermIdentifier

    let typeDecl v = typeDecl ((expr parseAllComplexShapes).Parser) v

    let parseBoundBody () =
      (expr parseAllComplexShapes).Parser
      |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

    // let indent = "--"

    let stringLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.StringLiteral s ->
          Expr.Primitive(
            PrimitiveValue.String s,
            t.Location,
            TypeCheckScope.Empty
          )
          |> Some
        | _ -> None)

    let intLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.IntLiteral s ->
          Expr.Primitive(
            PrimitiveValue.Int32 s,
            t.Location,
            TypeCheckScope.Empty
          )
          |> Some
        | _ -> None)

    let int64Literal () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.Int64Literal s ->
          Expr.Primitive(
            PrimitiveValue.Int64 s,
            t.Location,
            TypeCheckScope.Empty
          )
          |> Some
        | _ -> None)

    let decimalLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.DecimalLiteral d ->
          Expr.Primitive(
            PrimitiveValue.Decimal d,
            t.Location,
            TypeCheckScope.Empty
          )
          |> Some
        | _ -> None)

    let float32Literal () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.Float32Literal s ->
          Expr.Primitive(
            PrimitiveValue.Float32 s,
            t.Location,
            TypeCheckScope.Empty
          )
          |> Some
        | _ -> None)

    let float64Literal () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.Float64Literal s ->
          Expr.Primitive(
            PrimitiveValue.Float64 s,
            t.Location,
            TypeCheckScope.Empty
          )
          |> Some
        | _ -> None)

    let boolLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.BoolLiteral b ->
          Expr.Primitive(
            PrimitiveValue.Bool b,
            t.Location,
            TypeCheckScope.Empty
          )
          |> Some
        | _ -> None)

    let unitLiteral () =
      parser {
        do! openRoundBracketOperator
        do! closeRoundBracketOperator
        let! loc = parser.Location
        return Expr.Primitive(PrimitiveValue.Unit, loc, TypeCheckScope.Empty)
      }

    let matchWith () =
      parser {
        do! parseKeyword Keyword.Match
        let! loc = parser.Location

        return!
          parser {
            let! matchedExpr = (expr parseAllComplexShapes).Parser
            do! parseKeyword Keyword.With

            let! cases =
              parser.AtLeastOne(
                parser {
                  do! pipeOperator

                  let! id =
                    parser.Any
                      [ (identifierLocalOrFullyQualified ()).Parser |> parser.Map Left
                        (caseLiteral ()).Parser |> parser.Map Right ]

                  let pattern =
                    parser {
                      let! paramName =
                        parser.Any
                          [ singleIdentifier.Parser |> parser.Map Some
                            unitLiteral () |> parser.Map(fun _ -> None)
                            parser.Lookahead(parseOperator Operator.SingleArrow)
                            |> parser.Map("@anonymous" |> Some |> replaceWith)
                            parser {
                              let! loc = parser.Location

                              return!
                                (fun () ->
                                  "Expected identifier or unit literal as pattern parameter.")
                                |> Errors.Singleton loc
                                |> Errors.MapPriority(
                                  replaceWith ErrorPriority.High
                                )
                                |> parser.Throw
                            } ]
                        |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

                      do! parseOperator Operator.SingleArrow
                      let! body = (expr parseAllComplexShapes).Parser
                      return id, (paramName |> Option.map Var.Create, body)
                    }

                  return!
                    parser.Any [ betweenBrackets (fun () -> pattern); pattern ]
                    |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
                }
              )
              |> parser.Map(NonEmptyList.ToList)

            let! fallback =
              parser {
                do! pipeOperator
                do! openRoundBracketOperator
                do! timesOperator

                do! parseOperator Operator.SingleArrow
                let! body = (expr parseAllComplexShapes).Parser
                do! closeRoundBracketOperator
                return body
              }
              |> parser.Try
              |> parser.Map Sum.toOption

            let unionCases =
              cases
              |> List.collect (fun (id, c) ->
                match id with
                | Left id -> [ id, c ]
                | _ -> [])

            let sumCases =
              cases
              |> List.collect (fun (id, c) ->
                match id with
                | Right id -> [ id, c ]
                | _ -> [])

            if unionCases.Length > 0 && sumCases.Length > 0 then
              return!
                (fun () ->
                  "Error: cannot mix union cases and sum cases in match expression")
                |> Errors.Singleton loc
                |> parser.Throw
            else if unionCases.Length > 0 then
              let unionCases = Map.ofList unionCases

              return
                Expr.Apply(
                  Expr.UnionDes(unionCases, fallback, loc, TypeCheckScope.Empty),
                  matchedExpr,
                  loc,
                  TypeCheckScope.Empty
                )
            else
              let sumCases = Map.ofList sumCases

              return
                Expr.Apply(
                  Expr.SumDes(sumCases, loc, TypeCheckScope.Empty),
                  matchedExpr,
                  loc,
                  TypeCheckScope.Empty
                )

          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }

    let termParam =
      parser.Any
        [ parser {
            do! openRoundBracketOperator
            let! paramName = singleIdentifier.Parser
            do! colonOperator
            let! typeDecl = (typeDecl parseAllComplexTypeShapes).Parser
            do! closeRoundBracketOperator
            return paramName, typeDecl |> Some
          }
          parser {
            let! paramName = singleIdentifier.Parser
            return paramName, None
          } ]

    let exprLambda () =
      parser {
        do! parseKeyword Keyword.Fun
        let! loc = parser.Location

        return!
          parser {

            let! pars =
              parser.AtLeastOne(
                parser.Any
                  [ typeParam.Parser |> parser.Map Left
                    termParam |> parser.Map Right ]
              )

            let pars = pars |> NonEmptyList.ToSeq

            let! colon = colonOperator |> parser.Try

            let! bodyType =
              match colon with
              | Left _ ->
                (typeDecl (
                  parseAllComplexTypeShapes
                  |> Set.remove ComplexTypeKind.BinaryExpressionChain
                )).Parser
                |> parser.Map Some
              | Right _ -> parser { return None }

            do! parseOperator Operator.SingleArrow
            let! body = (expr parseAllComplexShapes).Parser

            return
              (body, true)
              |> Seq.foldBack
                (fun p (acc, is_first) ->
                  match p with
                  | Right(paramName, paramType: Option<TypeExpr<'valueExt>>) ->
                    Expr.Lambda(
                      Var.Create paramName,
                      paramType,
                      acc,
                      (if is_first then bodyType else None),
                      loc,
                      TypeCheckScope.Empty
                    ),
                    false
                  | Left(paramName: string, paramKind: Kind) ->
                    let p = (paramName, paramKind) |> TypeParameter.Create
                    Expr.TypeLambda(p, acc, loc, TypeCheckScope.Empty), false)
                pars
              |> fst
          // Expr.Lambda(Var.Create paramName, paramType, body, loc, TypeCheckScope.Empty)
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))

      }

    let exprLet () =
      parser {
        do! letKeyword
        let! loc = parser.Location

        let! paramName, paramType =
          parser.Any
            [ parser {
                do! openRoundBracketOperator
                let! paramName = singleIdentifier.Parser
                do! colonOperator

                return!
                  parser {
                    let! typeDecl = (typeDecl parseAllComplexTypeShapes).Parser
                    do! closeRoundBracketOperator
                    return paramName, typeDecl |> Some
                  }
                  |> parser.MapError(
                    Errors.MapPriority(replaceWith ErrorPriority.High)
                  )
              }
              parser {
                let! paramName = singleIdentifier.Parser

                let! paramType =
                  parser.Any
                    [ parser {
                        do! colonOperator

                        return!
                          parser {
                            let! typeDecl = (typeDecl parseAllComplexTypeShapes).Parser
                            return typeDecl |> Some
                          }
                          |> parser.MapError(
                            Errors.MapPriority(replaceWith ErrorPriority.High)
                          )
                      }
                      parser { return None } ]

                return paramName, paramType
              }
              (fun () -> "Expected let parameter.")
              |> Errors.Singleton loc
              |> Errors.MapPriority(replaceWith ErrorPriority.High)
              |> parser.Throw ]
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))

        do! equalsOperator

        let! loc' = parser.Location

        let! value =
          (expr parseAllComplexShapes).Parser
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

        do!
          parser.Any
            [ semicolonOperator
              (fun () ->
                "Expected ';' before expression after let binding")
              |> Errors.Singleton loc'
              |> Errors.MapPriority(replaceWith ErrorPriority.High)
              |> parser.Throw ]
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

        let! body =
          parseBoundBody ()

        return
          Expr.Let(
            paramName |> Var.Create,
            paramType,
            value,
            body,
            loc,
            TypeCheckScope.Empty
          )
      }
      |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

    let exprDo () =
      parser {
        do! doKeyword
        let! loc = parser.Location

        let! value =
          (expr parseAllComplexShapes).Parser
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

        do!
          parser.Any
            [ semicolonOperator
              (fun () ->
                "Expected ';' after 'do' expression")
              |> Errors.Singleton loc
              |> Errors.MapPriority(replaceWith ErrorPriority.High)
              |> parser.Throw ]
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

        let! body =
          (expr parseAllComplexShapes).Parser
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

        return Expr.Do(value, body, loc, TypeCheckScope.Empty)
      }
      |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

    let exprConditional () =
      parser {
        do! ifKeyword
        let! loc = parser.Location

        return!
          parser {
            let! cond = (expr parseAllComplexShapes).Parser
            do! thenKeyword
            let! thenBranch = (expr parseAllComplexShapes).Parser
            do! elseKeyword
            let! elseBranch = (expr parseAllComplexShapes).Parser

            return
              Expr.If(cond, thenBranch, elseBranch, loc, TypeCheckScope.Empty)
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }
      |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

    let recordCons () =
      parser {
        do! openCurlyBracketOperator
        let! loc = parser.Location

        let mkListNil () =
          Expr.Apply(
            Expr.Lookup(
              Identifier.FullyQualified([ "List" ], "Nil"),
              loc,
              TypeCheckScope.Empty
            ),
            Expr.Primitive(PrimitiveValue.Unit, loc, TypeCheckScope.Empty),
            loc,
            TypeCheckScope.Empty
          )

        let mkListCons
          (head: Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
          (tail: Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
          =
          Expr.Apply(
            Expr.Lookup(
              Identifier.FullyQualified([ "List" ], "Cons"),
              loc,
              TypeCheckScope.Empty
            ),
            Expr.TupleCons([ head; tail ], loc, TypeCheckScope.Empty),
            loc,
            TypeCheckScope.Empty
          )

        let mkListLiteral
          (items: List<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>>)
          =
          let rec build remaining =
            match remaining with
            | [] -> mkListNil ()
            | head :: tail -> mkListCons head (build tail)

          build items

        let mkMapEmpty () =
          Expr.Apply(
            Expr.Lookup(
              Identifier.FullyQualified([ "Map" ], "Empty"),
              loc,
              TypeCheckScope.Empty
            ),
            Expr.Primitive(PrimitiveValue.Unit, loc, TypeCheckScope.Empty),
            loc,
            TypeCheckScope.Empty
          )

        let mkMapSet
          (key: Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
          (value: Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
          (acc: Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
          =
          Expr.Apply(
            Expr.Apply(
              Expr.Lookup(
                Identifier.FullyQualified([ "Map" ], "set"),
                loc,
                TypeCheckScope.Empty
              ),
              Expr.TupleCons([ key; value ], loc, TypeCheckScope.Empty),
              loc,
              TypeCheckScope.Empty
            ),
            acc,
            loc,
            TypeCheckScope.Empty
          )

        let mkMapLiteral
          (entries:
            List<
              Identifier * Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>
             >)
          =
          entries
          |> List.fold
            (fun acc (key, value) ->
              let keyExpr = Expr.Lookup(key, loc, TypeCheckScope.Empty)
              mkMapSet keyExpr value acc)
            (mkMapEmpty ())

        return!
          parser {
            let! isEmpty = closeCurlyBracketOperator |> parser.Try

            match isEmpty with
            | Left _ ->
              return mkListLiteral []
            | Right _ ->
              let! firstExpr = (expr parseAllComplexShapes).Parser

              let firstLookupId =
                match firstExpr.Expr with
                | ExprRec.Lookup({ Id = id }) -> Some id
                | _ -> None

              let parseListLiteralFromFirst () =
                parser {
                  let! tailItems =
                    parser.ManyIndex(fun _ ->
                      parser {
                        do! semicolonOperator
                        let! value = (expr parseAllComplexShapes).Parser
                        return value
                      })

                  do! semicolonOperator |> parser.Try |> parser.Ignore
                  do! closeCurlyBracketOperator
                  return mkListLiteral (firstExpr :: tailItems)
                }

              let parseRecordWithFromFirst () =
                parser {
                  do! withKeyword

                  let! fields =
                    parser.ManyIndex(fun i ->
                      parser {
                        if i > 0 then
                          do! semicolonOperator

                        let! id = (identifierLocalOrFullyQualified ()).Parser
                        do! equalsOperator
                        let! value = (expr parseAllComplexShapes).Parser
                        return (id, value)
                      })

                  do! semicolonOperator |> parser.Try |> parser.Ignore
                  do! closeCurlyBracketOperator
                  return Expr.RecordWith(firstExpr, fields, loc, TypeCheckScope.Empty)
                }

              let parseRecordFromFirst
                (firstField: Identifier)
                : Parser<_, _, _, _> =
                parser {
                  do! equalsOperator
                  let! firstValue = (expr parseAllComplexShapes).Parser

                  let! fields =
                    parser.ManyIndex(fun _ ->
                      parser {
                        do! semicolonOperator
                        let! id = (identifierLocalOrFullyQualified ()).Parser
                        do! equalsOperator
                        let! value = (expr parseAllComplexShapes).Parser
                        return (id, value)
                      })

                  do! semicolonOperator |> parser.Try |> parser.Ignore
                  do! closeCurlyBracketOperator

                  return
                    Expr.RecordCons(
                      (firstField, firstValue) :: fields,
                      loc,
                      TypeCheckScope.Empty
                    )
                }

              let parseMapFromFirst
                (firstKey: Identifier)
                : Parser<_, _, _, _> =
                parser {
                  do! singleArrowOperator
                  let! firstValue = (expr parseAllComplexShapes).Parser

                  let! entries =
                    parser.ManyIndex(fun _ ->
                      parser {
                        do! semicolonOperator
                        let! key = (identifierLocalOrFullyQualified ()).Parser
                        do! singleArrowOperator
                        let! value = (expr parseAllComplexShapes).Parser
                        return (key, value)
                      })

                  do! semicolonOperator |> parser.Try |> parser.Ignore
                  do! closeCurlyBracketOperator

                  return mkMapLiteral ((firstKey, firstValue) :: entries)
                }

              match firstLookupId with
              | Some firstId ->
                return!
                  parser.Any
                    [ parseRecordWithFromFirst ()
                      parseRecordFromFirst firstId
                      parseMapFromFirst firstId
                      parseListLiteralFromFirst () ]
                  |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
              | None ->
                return! parseListLiteralFromFirst ()
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }

    let tupleConsTail () =
      parser {
        do! commaOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            let! fields =
              parser.AtLeastOne(
                parser {
                  do! commaOperator

                  let! value =
                    (expr (
                      parseComplexShapes
                      |> Set.remove ComplexExpressionKind.TupleCons
                    )).Parser

                  return value
                }
              )

            return fields |> ComplexExpression.TupleCons
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }

    let recordDes () =
      parser {
        do! dotOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            do! dotOperator
            let! dotLoc = parser.Location

            let! firstFieldOrRecover =
              parser.Any
                [ singleIdentifier.Parser |> parser.Map(Left >> Some)
                  (intLiteralToken ()).Parser |> parser.Map(Right >> Some)
                  parser { return None } ]

            match firstFieldOrRecover with
            | Some firstField ->
              let! tailFields =
                parser.AtLeastOne(
                  parser {
                    do! dotOperator

                    return!
                      parser.Any
                        [ singleIdentifier.Parser |> parser.Map Left
                          (intLiteralToken ()).Parser |> parser.Map Right ]
                  }
                )
                |> parser.Try

              let fields =
                match tailFields with
                | Left tail -> NonEmptyList.OfList(firstField, tail |> NonEmptyList.ToList)
                | Right _ -> NonEmptyList.OfList(firstField, [])

              return fields |> ComplexExpression.RecordOrTupleDesChain
            | None ->
              return dotLoc |> ComplexExpression.DanglingRecordDes
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }

    let typeLet () =
      parser {
        do! typeKeyword
        let! loc = parser.Location

        let! id =
          singleIdentifier.Parser
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))

        do! equalsOperator

        let! typeDecl =
          parser.Any
            [ (typeDecl parseAllComplexTypeShapes).Parser
              |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.Low))
              (fun () ->
                $"Malformed type declaration for '{id}' at {loc}")
              |> Errors.Singleton loc
              |> Errors.MapPriority(replaceWith ErrorPriority.High)
              |> parser.Throw ]
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
          |> parser.MapError(Errors.MapContext(replaceWith loc))
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

        let! loc' = parser.Location

        do!
          parser.Any
            [ semicolonOperator
              (fun () ->
                "Expected ';' before expression after type let declaration")
              |> Errors.Singleton loc'
              |> Errors.MapPriority(replaceWith ErrorPriority.High)
              |> parser.Throw ]
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

        let! body =
          parseBoundBody ()

        let symbols, symbolsKind =
          let rec stripLambdas t =
            match t with
            | TypeExpr.Lambda(_, t) -> stripLambdas t
            | _ -> t

          match typeDecl |> stripLambdas with
          | TypeExpr.Record fields ->
            fields
            |> List.map fst
            |> List.collect (function
              | TypeExpr.Lookup(Identifier.LocalScope id) -> [ id ]
              | _ -> []),
            SymbolsKind.RecordFields
          | TypeExpr.Union cases ->
            cases
            |> List.map fst
            |> List.collect (function
              | TypeExpr.Lookup(Identifier.LocalScope id) -> [ id ]
              | _ -> []),
            SymbolsKind.UnionConstructors
          | _ -> [], SymbolsKind.RecordFields

        let typeDecl = TypeExpr.LetSymbols(symbols, symbolsKind, typeDecl)

        return Expr.TypeLet(id, typeDecl, body, loc, TypeCheckScope.Empty)
      }
      |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

    let unaryOperatorIdentifier () =
      let singleOperator op =
        parser {
          do! parseOperator op
          let! loc = parser.Location

          return
            Expr.Lookup(
              op.ToString() |> Identifier.LocalScope,
              loc,
              TypeCheckScope.Empty
            )
        }

      singleOperator Operator.Bang
    // parser.Any [
    // ]

    let identifierLookup () =
      parser {
        let! id =
          parser.Any
            [ singleIdentifier.Parser
              schemaKeyword |> parser.Map(replaceWith "schema")
              entityKeyword |> parser.Map(replaceWith "entity")
              relationKeyword |> parser.Map(replaceWith "relation") ]

        let! loc = parser.Location
        // do Console.WriteLine($"{String.replicate (depth * 2) indent}> Parsed identifier: {id.ToFSharpString}")
        return Expr.Lookup(Identifier.LocalScope id, loc, TypeCheckScope.Empty)
      }

    let scopedIdentifier () =
      parser {
        do! doubleColonOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            do! doubleColonOperator
            let! colonLoc = parser.Location

            let! firstIdOrNone =
              parser.Any
                [ singleIdentifier.Parser |> parser.Map Some
                  parser { return None } ]

            match firstIdOrNone with
            | None ->
              return colonLoc |> DanglingScopedIdentifier
            | Some firstId ->
              let! tailIds =
                parser.AtLeastOne(
                  parser {
                    do! doubleColonOperator
                    return! singleIdentifier.Parser
                  }
                )
                |> parser.Try

              let ids =
                match tailIds with
                | Left tail -> NonEmptyList.OfList(firstId, tail |> NonEmptyList.ToList)
                | Right _ -> NonEmptyList.OfList(firstId, [])

              return ids |> ScopedIdentifier
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }

    let binaryExpressionChainTail () =
      parser {
        do! binaryExprOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            let! fields =
              parser.AtLeastOne(
                parser {
                  let! op = binaryExprOperator.Parser

                  let! value =
                    (expr (
                      parseComplexShapes
                      |> Set.remove ComplexExpressionKind.BinaryExpressionChain
                    )).Parser

                  return op, value
                }
              )

            return fields |> ComplexExpression.BinaryExpressionChain
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }

    let argExpr () =
      parser {
        let! res =
          parser.Any
            [ (expr (Set.singleton ComplexExpressionKind.RecordDes)).Parser
              |> parser.Map Sum.Left
              (fun () -> (typeDecl parseAllComplexTypeShapes).Parser)
              |> betweenSquareBrackets
              |> parser.Map(Sum.Right) ]
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
          |> parser.MapError(Errors<_>.DeduplicateByMessageKeepBest)

        return res
      }
      |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
      |> parser.MapError(Errors<_>.DeduplicateByMessageKeepBest)

    let application () =
      parser {
        let! args = parser.AtLeastOne(argExpr ())
        return args |> ComplexExpression.ApplicationArguments
      }
      |> parser.MapError(Errors<_>.DeduplicateByMessageKeepBest)

    let startsApplicationArgument (t: LocalizedToken) =
      match t.Token with
      | Token.Identifier _
      | Token.StringLiteral _
      | Token.BoolLiteral _
      | Token.IntLiteral _
      | Token.Int64Literal _
      | Token.DecimalLiteral _
      | Token.Float32Literal _
      | Token.Float64Literal _
      | Token.CaseLiteral _
      | Token.Keyword Keyword.Fun
      | Token.Keyword Keyword.If
      | Token.Keyword Keyword.Match
      | Token.Keyword Keyword.Query
      | Token.Keyword Keyword.View
      | Token.Keyword Keyword.Co
      | Token.Operator(Operator.RoundBracket Bracket.Open)
      | Token.Operator(Operator.SquareBracket Bracket.Open)
      | Token.Operator(Operator.CurlyBracket Bracket.Open)
      | Token.Operator Operator.Minus
      | Token.Operator Operator.Bang -> true
      | _ -> false

    let simpleShapesByToken (t: LocalizedToken) =
      match t.Token with
      | Token.StringLiteral _ -> [ stringLiteral () ]
      | Token.IntLiteral _ -> [ intLiteral () ]
      | Token.Int64Literal _ -> [ int64Literal () ]
      | Token.CaseLiteral _ -> [ (caseLiteral ()).Parser |> parser.Map Expr.SumCons ]
      | Token.DecimalLiteral _ -> [ decimalLiteral () ]
      | Token.Float32Literal _ -> [ float32Literal () ]
      | Token.Float64Literal _ -> [ float64Literal () ]
      | Token.BoolLiteral _ -> [ boolLiteral () ]
      | Token.Operator(Operator.RoundBracket Bracket.Open) ->
        [ unitLiteral ()
          betweenBrackets (fun () -> (expr parseAllComplexShapes).Parser) ]
      | Token.Keyword Keyword.Fun -> [ exprLambda () ]
      | Token.Keyword Keyword.If -> [ exprConditional () ]
      | Token.Operator(Operator.CurlyBracket Bracket.Open) -> [ recordCons () ]
      | Token.Keyword Keyword.Type -> [ typeLet () ]
      | Token.Keyword Keyword.Match -> [ matchWith () ]
      | Token.Keyword Keyword.Query ->
        [ (query (fun () -> (expr parseAllComplexShapes).Parser) ()).Parser ]
      | Token.Keyword Keyword.View ->
        [ // Try View::name first, then fall back to view expression
          parser {
            do! parseKeyword Keyword.View
            do! doubleColonOperator
            let! name = singleIdentifier.Parser
            let! loc = parser.Location
            match ViewOperationKind.TryParse name with
            | Some op ->
              return
                { Expr = ExprRec.ViewOp op
                  Location = loc
                  Scope = TypeCheckScope.Empty }
            | None ->
              return!
                (fun () -> $"Unknown View operation: View::{name}")
                |> Errors.Singleton loc
                |> parser.Throw
          }
          (viewExpr (fun () -> (expr parseAllComplexShapes).Parser) ()).Parser ]
      | Token.Keyword Keyword.Co ->
        [ // Try Co::name first, then fall back to co expression
          parser {
            do! parseKeyword Keyword.Co
            do! doubleColonOperator
            let! name = singleIdentifier.Parser
            let! loc = parser.Location
            match CoOperationKind.TryParse name with
            | Some op ->
              return
                { Expr = ExprRec.CoOp op
                  Location = loc
                  Scope = TypeCheckScope.Empty }
            | None ->
              return!
                (fun () -> $"Unknown Co operation: Co::{name}")
                |> Errors.Singleton loc
                |> parser.Throw
          }
          (coExpr (fun () -> (expr parseAllComplexShapes).Parser) ()).Parser ]
      | Token.Operator Operator.LessThan ->
        [ (viewNodeExpr (fun () -> (expr parseAllComplexShapes).Parser) ()).Parser ]
      | Token.Identifier _
      | Token.Keyword Keyword.Schema
      | Token.Keyword Keyword.Entity
      | Token.Keyword Keyword.Relation -> [ identifierLookup () ]
      | Token.Operator Operator.Bang -> [ unaryOperatorIdentifier () ]
      | Token.Keyword Keyword.Let -> [ exprLet () ]
      | Token.Keyword Keyword.Do -> [ exprDo () ]
      | _ -> []

    parser {
      // let! s = parser.Stream

      // do
      //   Console.WriteLine(
      //     $"expr(parseComplexShapes={parseComplexShapes}) Stream = {s |> Seq.map (fun t -> t.Token.ToString()) |> Seq.truncate 10 |> Seq.toList}"
      //   )

      // do Console.ReadLine() |> ignore

      if parseComplexShapes |> Set.isEmpty then
        let! stream = parser.Stream
        match stream with
        | t :: _ ->
          match simpleShapesByToken t with
          | [ single ] -> return! single
          | multiple when multiple.Length > 0 ->
            return!
              multiple
              |> parser.Any
              |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
          | _ ->
            let! loc = parser.Location
            return!
              (fun () -> $"Unexpected symbol: `{t.Token}`")
              |> Errors.Singleton loc
              |> parser.Throw
        | [] ->
          let! loc = parser.Location
          return!
            (fun () -> "Unexpected end of input")
            |> Errors.Singleton loc
            |> parser.Throw
      else
        // let! s = parser.Stream

        // do
        //   Console.WriteLine(
        //     $"{String.replicate (depth * 2) indent}> nested-expr(parseComplexShapes=false) Stream = {s |> Seq.map (fun t -> t.Token.ToString()) |> Seq.truncate 10 |> Seq.toList}"
        //   )

        // do Console.ReadLine() |> ignore
        let! e = (expr parseNoComplexShapes).Parser
        // do Console.Write $"{e.ToFSharpString}"
        // do Console.WriteLine $"included = {parseComplexShapes.ToFSharpString}"
        // do Console.ReadLine() |> ignore

        // do
        //   Console.WriteLine(
        //     $"{String.replicate (depth * 2) indent}> Trying to parse complex shape starting with {e.ToFSharpString}"
        //   )

        // do Console.ReadLine() |> ignore

        let complexShapes =
          [ parser {
              let! loc = parser.Location

              return!
                (fun () -> "Expected composite expression.")
                |> Errors.Singleton loc
                |> parser.Throw
            } ]

        let complexShapes =
          if
            parseComplexShapes.Contains ComplexExpressionKind.ScopedIdentifier
          then
            scopedIdentifier () :: complexShapes
          else
            complexShapes

        let complexShapes =
          if
            parseComplexShapes.Contains
              ComplexExpressionKind.BinaryExpressionChain
          then
            binaryExpressionChainTail () :: complexShapes
          else
            complexShapes

        let complexShapes =
          if
            parseComplexShapes.Contains
              ComplexExpressionKind.ApplicationArguments
          then
            application () :: complexShapes
          else
            complexShapes

        let complexShapes =
          if parseComplexShapes.Contains ComplexExpressionKind.RecordDes then
            recordDes () :: complexShapes
          else
            complexShapes

        let complexShapes =
          if parseComplexShapes.Contains ComplexExpressionKind.TupleCons then
            tupleConsTail () :: complexShapes
          else
            complexShapes

        let! res =
          parser {
            let! stream = parser.Stream

            let commitToFirstComplexShape =
              match stream with
              | t :: _ ->
                parseComplexShapes.Contains
                  ComplexExpressionKind.ApplicationArguments
                && startsApplicationArgument t
              | [] -> false

            if commitToFirstComplexShape then
              let! firstComplexShape =
                complexShapes
                |> parser.Any
                |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
                |> parser.MapError(Errors<_>.DeduplicateByMessageKeepBest)

              let! remainingComplexShapes =
                complexShapes
                |> parser.Any
                |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
                |> parser.MapError(Errors<_>.DeduplicateByMessageKeepBest)
                |> parser.Many

              return firstComplexShape :: remainingComplexShapes
            else
              return!
                complexShapes
                |> parser.Any
                |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
                |> parser.MapError(Errors<_>.DeduplicateByMessageKeepBest)
                |> parser.Many
          }

        // do Console.WriteLine $"~~{res.ToFSharpString}"
        // do Console.ReadLine() |> ignore
        let! loc = parser.Location

        let res
          : Sum<
              Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>,
              Errors<Location>
             > =
          res
          |> List.fold
            (fun acc e ->
              sum {
                let! acc = acc

                match e with
                | BinaryExpressionChain fields ->
                  let fields
                    : List<
                        BinaryOperatorsElement<
                          Expr<_, _, _>,
                          BinaryExprOperator
                         >
                       > =
                    fields
                    |> NonEmptyList.ToList
                    |> Seq.collect (fun (op, e) ->
                      [ op |> Precedence.Operator
                        (e, NonMergeable) |> Precedence.Operand ])
                    |> List.ofSeq

                  let chain = Operand(acc, Mergeable) :: fields

                  let precedence: List<OperatorsPrecedence<BinaryExprOperator>> =
                    [ { Operators =
                          [ BinaryExprOperator.DoubleGreaterThan
                            BinaryExprOperator.PipeGreaterThan ]
                          |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators =
                          [ BinaryExprOperator.Div
                            BinaryExprOperator.Times
                            BinaryExprOperator.Mod ]
                          |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators =
                          [ BinaryExprOperator.Plus; BinaryExprOperator.Minus ]
                          |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators =
                          [ BinaryExprOperator.GreaterEqual
                            BinaryExprOperator.GreaterThan
                            BinaryExprOperator.LessThan
                            BinaryExprOperator.LessThanOrEqual
                            BinaryExprOperator.Equal
                            BinaryExprOperator.NotEqual ]
                          |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators =
                          [ BinaryExprOperator.And; BinaryExprOperator.Or ]
                          |> Set.ofList
                        Associativity = AssociateLeft } ]

                  return!
                    collapseBinaryOperatorsChain
                      { Compose =
                          fun (e1, _src1, op, e2, _src2) ->
                            match op with
                            | BinaryExprOperator.DoubleGreaterThan ->
                              // Expr.Apply(Expr.Apply(Expr.Lookup(Identifier.LocalScope(">>"), loc), e1, loc), e2, loc)
                              Expr.Lambda(
                                Var.Create "x",
                                None,
                                Expr.Apply(
                                  e2,
                                  Expr.Apply(
                                    e1,
                                    Expr.Lookup(
                                      Identifier.LocalScope("x"),
                                      loc,
                                      TypeCheckScope.Empty
                                    ),
                                    loc,
                                    TypeCheckScope.Empty
                                  ),
                                  loc,
                                  TypeCheckScope.Empty
                                ),
                                None,
                                loc,
                                TypeCheckScope.Empty
                              ),
                              NonMergeable
                            | BinaryExprOperator.PipeGreaterThan ->
                              Expr.Apply(e2, e1, loc, TypeCheckScope.Empty),
                              NonMergeable
                            | _ ->
                              Expr.Apply(
                                Expr.Apply(
                                  Expr.Lookup(
                                    Identifier.LocalScope(op.ToString()),
                                    loc,
                                    TypeCheckScope.Empty
                                  ),
                                  e1,
                                  loc,
                                  TypeCheckScope.Empty
                                ),
                                e2,
                                loc,
                                TypeCheckScope.Empty
                              ),
                              NonMergeable
                        ToExpr = id }
                      loc
                      precedence
                      chain
                | ScopedIdentifier ids ->
                  match acc.Expr with
                  | ExprRec.Lookup({ Id = Identifier.LocalScope id }) ->
                    let ids = (id :: (ids |> NonEmptyList.ToList)) |> List.rev

                    return
                      Expr.Lookup(
                        Identifier.FullyQualified(ids.Tail, ids.Head),
                        loc,
                        TypeCheckScope.Empty
                      )
                  | _ ->
                    return!
                      (fun () ->
                        $"Error: cannot collapse scoped identifier chain on non-identifier")
                      |> Errors.Singleton loc
                      |> sum.Throw
                | DanglingScopedIdentifier errorLoc ->
                  match acc.Expr with
                  | ExprRec.Lookup({ Id = Identifier.LocalScope id }) ->
                    return
                      { Expr = ExprRec.ErrorDanglingScopedIdentifier({ PrefixParts = [ id ] })
                        Location = errorLoc
                        Scope = TypeCheckScope.Empty }
                  | _ ->
                    return!
                      (fun () ->
                        $"Error: cannot collapse dangling scoped identifier on non-identifier")
                      |> Errors.Singleton loc
                      |> sum.Throw
                | RecordOrTupleDesChain ids ->
                  return
                    ids
                    |> NonEmptyList.ToList
                    |> List.fold
                      (fun acc id ->
                        match id with
                        | Sum.Left id ->
                          Expr.RecordDes(
                            acc,
                            id |> Identifier.LocalScope,
                            loc,
                            TypeCheckScope.Empty
                          )
                        | Sum.Right idx ->
                          Expr.TupleDes(
                            acc,
                            { Index = idx },
                            loc,
                            TypeCheckScope.Empty
                          ))
                      acc
                | DanglingRecordDes errorLoc ->
                  return
                    { Expr = ExprRec.ErrorDanglingRecordDes({ Expr = acc; Field = None })
                      Location = errorLoc
                      Scope = TypeCheckScope.Empty }
                | TupleCons fields ->
                  return
                    Expr.TupleCons(
                      acc :: (fields |> NonEmptyList.ToList),
                      loc,
                      TypeCheckScope.Empty
                    )
                | ApplicationArguments args ->
                  let smartApply (t1, t2) =
                    match t2 with
                    | Sum.Left t2 ->
                      Expr.Apply(t1, t2, loc, TypeCheckScope.Empty)
                    | Sum.Right t2 ->
                      Expr.TypeApply(t1, t2, loc, TypeCheckScope.Empty)

                  return
                    args
                    |> NonEmptyList.ToList
                    |> List.fold (fun acc e -> smartApply (acc, e)) acc
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
    |> AnnotatedParser.withNamedRule exprRule

  let programRule: NamedRule =
    { Name = "program"
      Rule = Seq [ NonTerminal "expr"; Terminal "<eof>" ] }

  let program<'valueExt>
    ()
    : AnnotatedParser<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>>
    =
    parser {
      let! e = (expr 0 parseAllComplexShapes).Parser

      let! loc = parser.Location

      do!
        parser.Any
          [ semicolonOperator
            (fun () -> "Expected ';' at end of program")
            |> Errors.Singleton loc
            |> parser.Throw ]
        |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)

      do! parser.EndOfStream()
      return e
    }
    |> AnnotatedParser.withNamedRule programRule

  let grammarRules: NamedRule list = [ exprRule; programRule ]
