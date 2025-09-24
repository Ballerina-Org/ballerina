namespace Ballerina.DSL.Next.Syntax

[<AutoOpen>]
module Parser =

  open System
  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms


  let parser =
    ParserBuilder<LocalizedToken, Location, Errors>(
      {| Step = fun lt _ -> lt.Location |},
      {| UnexpectedEndOfFile = fun loc -> (loc, $"Unexpected end of file at {loc}") |> Errors.Singleton
         AnyFailed = fun loc -> (loc, "No matching token") |> Errors.Singleton
         NotFailed = fun loc -> (loc, $"Expected token not found at {loc}") |> Errors.Singleton
         UnexpectedSymbol = fun loc c -> (loc, $"Unexpected symbol: {c}") |> Errors.Singleton
         FilterHighestPriorityOnly = Errors.HighestPriority
         Concat = Errors.Concat |}
    )

  let parseOperator op =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.Operator actualOp when op = actualOp -> true
      | _ -> false)
    |> parser.Ignore

  let timesOperator = parseOperator Operator.Times
  let pipeOperator = parseOperator Operator.Pipe
  let colonOperator = parseOperator Operator.Colon
  let semicolonOperator = parseOperator Operator.SemiColon
  let commaOperator = parseOperator Operator.Comma
  let dotOperator = parseOperator Operator.Dot
  let doubleColonOperator = parseOperator Operator.DoubleColon
  let openRoundBracketOperator = parseOperator (Operator.RoundBracket Bracket.Open)
  let closeRoundBracketOperator = parseOperator (Operator.RoundBracket Bracket.Close)
  let openSquareBracketOperator = parseOperator (Operator.SquareBracket Bracket.Open)

  let closeSquareBracketOperator =
    parseOperator (Operator.SquareBracket Bracket.Close)

  let openCurlyBracketOperator = parseOperator (Operator.CurlyBracket Bracket.Open)
  let closeCurlyBracketOperator = parseOperator (Operator.CurlyBracket Bracket.Close)
  let equalsOperator = parseOperator (Operator.Equals)

  let parseKeyword kw =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.Keyword actualKw when kw = actualKw -> true
      | _ -> false)
    |> parser.Ignore

  let typeKeyword = parseKeyword Keyword.Type
  let ofKeyword = parseKeyword Keyword.Of
  let letKeyword = parseKeyword Keyword.Let
  let inKeyword = parseKeyword Keyword.In

  let identifierMatch =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.Identifier id -> Some id
      | _ -> None)

  let rec identifiersMatch () =
    parser {
      let! id = identifierMatch

      return!
        parser.Any
          [ parser {
              do! doubleColonOperator

              return!
                parser {
                  let! ids = identifiersMatch () |> parser.Map(NonEmptyList.ToList)
                  return NonEmptyList.OfList(id, ids)
                }
                |> parser.MapError(Errors.SetPriority ErrorPriority.High)
            }
            parser { return NonEmptyList.OfList(id, []) } ]
    }

  let identifierLocalOrFullyQualified () =
    parser.Any
      [ parser {
          let! ids = identifiersMatch ()
          let ids = ids |> NonEmptyList.rev

          match ids.Tail with
          | [] -> return Identifier.LocalScope ids.Head
          | _ -> return Identifier.FullyQualified(ids.Tail, ids.Head)
        }
        parser {
          let! id = identifierMatch
          return Identifier.LocalScope id
        } ]

  let betweenBrackets p =
    parser {
      do! openRoundBracketOperator

      return!
        parser {
          let! res = p ()
          do! closeRoundBracketOperator
          return res
        }
        |> parser.MapError(Errors.SetPriority ErrorPriority.High)
    }

  let betweenSquareBrackets p =
    parser {
      do! openSquareBracketOperator

      return!
        parser {
          let! res = p ()
          do! closeSquareBracketOperator
          return res
        }
        |> parser.MapError(Errors.SetPriority ErrorPriority.High)
    }

  let rec typeDecl () =
    let lookupTypeDecl () =
      parser {
        let! id = identifierMatch
        return [], TypeExpr.Lookup(Identifier.LocalScope id)
      }

    let boolTypeDecl () =
      parser {
        let! id = identifierMatch

        match id with
        | "bool" -> return [], TypeExpr.Primitive PrimitiveType.Bool
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected bool, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let stringTypeDecl () =
      parser {
        let! id = identifierMatch

        match id with
        | "string" -> return [], TypeExpr.Primitive PrimitiveType.String
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected string, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let guidTypeDecl () =
      parser {
        let! id = identifierMatch

        match id with
        | "guid" -> return [], TypeExpr.Primitive PrimitiveType.Guid
        | _ ->
          let! loc = parser.Location
          return! (loc, $"Error: expected guid, got {id}") |> Errors.Singleton |> parser.Throw
      }

    let unitTypeDecl () =
      parser {
        do! openRoundBracketOperator
        do! closeRoundBracketOperator
        return [], TypeExpr.Primitive PrimitiveType.Unit
      }

    let record () =
      parser {
        do! openCurlyBracketOperator

        return!
          parser {
            let! fields =
              parser.Many(
                parser {
                  let! id = identifierMatch
                  do! colonOperator

                  return!
                    parser {
                      let! _, typeDecl = typeDecl ()
                      do! semicolonOperator
                      return (id, typeDecl)
                    }
                    |> parser.MapError(Errors.SetPriority ErrorPriority.High)
                }
              )

            do! closeCurlyBracketOperator

            return
              fields |> List.map fst,
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
                      let! id = identifierMatch
                      do! ofKeyword
                      let! _, typeDecl = typeDecl ()
                      return (id, typeDecl)
                    }
                    |> parser.MapError(Errors.SetPriority ErrorPriority.High)
                }
              )
              |> parser.Map(NonEmptyList.ToList)

            return
              cases |> List.map fst,
              TypeExpr.Union(
                cases
                |> List.map (fun (id, td) -> (id |> Identifier.LocalScope |> TypeExpr.Lookup, td))
              )
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    parser.Any
      [ betweenBrackets typeDecl
        record ()
        unionTypeDecl ()
        unitTypeDecl ()
        boolTypeDecl ()
        stringTypeDecl ()
        guidTypeDecl ()
        lookupTypeDecl () ]


  type ComplexExpression =
    | ScopedIdentifier of NonEmptyList<string>
    | RecordDes of NonEmptyList<string>
    | TupleCons of NonEmptyList<Expr<TypeExpr>>
    | ApplicationArguments of NonEmptyList<Sum<Expr<TypeExpr>, TypeExpr>>

  let rec expr (depth: int) (parseComplexShapes: bool) =
    let expr = expr (depth + 1)
    // let indent = "--"

    let stringLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.StringLiteral s -> Expr.Primitive(PrimitiveValue.String s) |> Some
        | _ -> None)

    let boolLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.BoolLiteral b -> Expr.Primitive(PrimitiveValue.Bool b) |> Some
        | _ -> None)

    let unitLiteral () =
      parser {
        do! openRoundBracketOperator
        do! closeRoundBracketOperator
        return Expr.Primitive PrimitiveValue.Unit
      }

    let matchWith () =
      parser {
        do! parseKeyword Keyword.Match

        return!
          parser {
            let! matchedExpr = expr true
            do! parseKeyword Keyword.With

            let! cases =
              parser.AtLeastOne(
                parser {
                  do! pipeOperator
                  let! id = identifierMatch

                  do! openRoundBracketOperator
                  let! paramName = identifierMatch

                  do! parseOperator Operator.SingleArrow
                  let! body = expr true
                  do! closeRoundBracketOperator
                  return (id |> Identifier.LocalScope, (Var.Create paramName, body))
                }
              )
              |> parser.Map(NonEmptyList.ToList >> Map.ofList)

            let! fallback =
              parser {
                do! pipeOperator
                do! openRoundBracketOperator
                do! timesOperator

                do! parseOperator Operator.SingleArrow
                let! body = expr true
                do! closeRoundBracketOperator
                return body
              }
              |> parser.Try
              |> parser.Map Sum.toOption

            return Expr.Apply(Expr.UnionDes(cases, fallback), matchedExpr)
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let exprLambda () =
      parser {
        do! parseKeyword Keyword.Fun

        return!
          parser {

            let! paramName, paramType =
              parser.Any
                [ parser {
                    do! openRoundBracketOperator
                    let! paramName = identifierMatch
                    do! colonOperator
                    let! _, typeDecl = typeDecl ()
                    do! closeRoundBracketOperator
                    return paramName, typeDecl |> Some
                  }
                  parser {
                    let! paramName = identifierMatch
                    return paramName, None
                  } ]

            do! parseOperator Operator.SingleArrow
            let! body = expr true
            return Expr.Lambda(Var.Create paramName, paramType, body)
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)

      }

    let exprLet () =
      parser {
        do! letKeyword

        return!
          parser {
            let! varName = identifierMatch
            do! equalsOperator
            let! value = expr true
            do! inKeyword
            let! body = expr true
            return Expr.Let(varName |> Var.Create, value, body)
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let recordCons () =
      parser {
        do! openCurlyBracketOperator

        return!
          parser {
            let! fields =
              parser.Many(
                parser {
                  let! id = identifierLocalOrFullyQualified ()
                  do! equalsOperator
                  let! value = expr true
                  do! semicolonOperator
                  return (id, value)
                }
              )

            do! closeCurlyBracketOperator

            return Expr.RecordCons fields
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
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
                  let! value = expr true
                  return value
                }
              )

            return fields |> ComplexExpression.TupleCons
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
                  return! identifierMatch
                }
              )

            return fields |> ComplexExpression.RecordDes
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let typeLet () =
      parser {
        do! typeKeyword

        return!
          parser {
            let! id = identifierMatch
            do! equalsOperator
            let! symbols, typeDecl = typeDecl ()
            do! inKeyword
            let! body = expr true

            let typeDecl =
              symbols
              |> List.fold (fun acc s -> TypeExpr.Let(s, TypeExpr.NewSymbol s, acc)) typeDecl

            return Expr.TypeLet(id, typeDecl, body)
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let identifierLookup () =
      parser {
        let! id = identifierMatch
        // do Console.WriteLine($"{String.replicate (depth * 2) indent}> Parsed identifier: {id.ToFSharpString}")
        return Expr.Lookup(Identifier.LocalScope id)
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
                  return! identifierMatch
                }
              )

            return ids |> ScopedIdentifier
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let argExpr () =
      parser {
        return!
          parser {
            let! res =
              parser.Any
                [ expr false |> parser.Map Left
                  typeDecl |> betweenSquareBrackets |> parser.Map(snd >> Right) ]

            return res
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let application () =
      parser {
        let! args = parser.AtLeastOne(argExpr ())
        return args |> ComplexExpression.ApplicationArguments
      }

    let simpleShapes =
      [ stringLiteral ()
        boolLiteral ()
        unitLiteral ()
        exprLet ()
        exprLambda ()
        recordCons ()
        betweenBrackets (fun () -> expr true)
        typeLet ()
        matchWith ()
        identifierLookup () ]

    parser {
      // let! s = parser.Stream

      // do
      //   Console.WriteLine(
      //     $"{String.replicate (depth * 2) indent}> expr(parseComplexShapes={parseComplexShapes}) Stream = {s |> Seq.map (fun t -> t.Token.ToString()) |> Seq.truncate 10 |> Seq.toList}"
      //   )

      // do Console.ReadLine() |> ignore

      if parseComplexShapes |> not then
        return! simpleShapes |> parser.Any
      else
        // let! s = parser.Stream

        // do
        //   Console.WriteLine(
        //     $"{String.replicate (depth * 2) indent}> nested-expr(parseComplexShapes=false) Stream = {s |> Seq.map (fun t -> t.Token.ToString()) |> Seq.truncate 10 |> Seq.toList}"
        //   )

        // do Console.ReadLine() |> ignore
        let! e = expr false

        // do
        //   Console.WriteLine(
        //     $"{String.replicate (depth * 2) indent}> Trying to parse complex shape starting with {e.ToFSharpString}"
        //   )

        // do Console.ReadLine() |> ignore

        let! res =
          [ scopedIdentifier (); recordDes (); tupleConsTail (); application () ]
          |> parser.Any
          |> parser.Many


        let res =
          res
          |> List.fold
            (fun acc e ->
              match acc with
              | None -> None
              | Some acc ->
                match e with
                | ScopedIdentifier ids ->
                  match acc with
                  | Expr.Lookup(Identifier.LocalScope id) ->
                    let ids = (id :: (ids |> NonEmptyList.ToList)) |> List.rev
                    Expr.Lookup(Identifier.FullyQualified(ids.Tail, ids.Head)) |> Some
                  | _ -> None
                | RecordDes ids ->
                  Expr.RecordDes(acc, ids |> NonEmptyList.ToList |> List.rev |> List.head |> Identifier.LocalScope)
                  |> Some
                | TupleCons fields -> Expr.TupleCons(acc :: (fields |> NonEmptyList.ToList)) |> Some
                | ApplicationArguments args ->
                  let smartApply (t1, t2) =
                    match t2 with
                    | Left t2 -> Expr.Apply(t1, t2)
                    | Right t2 -> Expr.TypeApply(t1, t2)

                  args
                  |> NonEmptyList.ToList
                  |> List.fold (fun acc e -> smartApply (acc, e)) acc
                  |> Some)
            (Some e)

        // let! s = parser.Stream

        // do
        //   Console.WriteLine(
        //     $"{String.replicate (depth * 2) indent}> success! nested-expr(parseComplexShapes=false) = {res.ToFSharpString}, remaining stream = {s |> Seq.map (fun t -> t.Token.ToString()) |> Seq.truncate 10 |> Seq.toList}"
        //   )

        return res |> Option.defaultValue e
    }

  let program =
    parser {
      let! e = expr 0 true
      do! parser.EndOfStream()
      return e
    }
