namespace Ballerina.DSL.Next.Syntax

[<AutoOpen>]
module Parser =

  open System
  open Ballerina.Collections.Option
  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms

  type BinaryOperatorsElement<'operand, 'operator> =
    | Operand of 'operand
    | Operator of 'operator

  type BinaryOperatorsOperations<'operand, 'operator, 'expr> =
    { Compose: 'operand * 'operator * 'operand -> 'operand
      ToExpr: 'operand -> 'expr }

  type OperatorAssociativity =
    | AssociateLeft
    | AssociateRight

  type OperatorsPrecedence<'operator when 'operator: comparison> =
    { Operators: Set<'operator>
      Associativity: OperatorAssociativity }

  let rec collapseBinaryOperatorsChain<'operand, 'operator, 'expr when 'operator: comparison>
    (binOp: BinaryOperatorsOperations<'operand, 'operator, 'expr>)
    (loc: Location)
    (precedence: List<OperatorsPrecedence<'operator>>)
    (l0: List<BinaryOperatorsElement<'operand, 'operator>>)
    =
    let rec collapse (p: OperatorsPrecedence<'operator>) (l: List<BinaryOperatorsElement<'operand, 'operator>>) =
      sum {
        match l with
        | [ _ ] -> return l
        | Operand e1 :: Operator op :: Operand e2 :: rest when p.Operators |> Set.contains op ->
          return! (binOp.Compose(e1, op, e2) |> Operand) :: rest |> collapse p
        | Operand e1 :: Operator op :: rest ->
          let! rest1 = collapse p rest
          return Operand e1 :: Operator op :: rest1
        | _ ->
          return!
            (loc, $"Error: cannot collapse operators sequence {l.ToFSharpString}")
            |> Errors.Singleton
            |> sum.Throw
      }

    sum {
      match precedence with
      | [] ->
        match l0 with
        | [ Operand x ] -> return binOp.ToExpr x
        | l ->
          return!
            (loc, $"Error: invalid operator sequence, residual elements {l} cannot be further processes")
            |> Errors.Singleton
            |> sum.Throw
      | p :: ps ->
        let! l1 =
          sum {
            match p.Associativity with
            | OperatorAssociativity.AssociateLeft ->
              // do Console.WriteLine $"Collapsing left-associative operators chain with precedence {p.Operators.ToFSharpString} on {l0.ToFSharpString}"
              // do Console.ReadLine() |> ignore
              let! res = l0 |> collapse p
              // do Console.WriteLine $"Collapsed left-associative operators chain: {res.ToFSharpString}"
              // do Console.ReadLine() |> ignore
              return res
            | OperatorAssociativity.AssociateRight ->
              let l0 = l0 |> List.rev
              let! res = l0 |> collapse p
              return res |> List.rev
          }

        return! collapseBinaryOperatorsChain binOp loc ps l1
    }


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

  type BinaryOperator =
    | Times
    | Div
    | Plus
    | Minus
    | And
    | Or
    | Equal
    | NotEqual
    | GreaterThan
    | LessThan
    | GreaterEqual
    | LessThanOrEqual

    override this.ToString() =
      match this with
      | Times -> "*"
      | Div -> "/"
      | Plus -> "+"
      | Minus -> "-"
      | And -> "&&"
      | Or -> "||"
      | Equal -> "=="
      | NotEqual -> "!="
      | GreaterThan -> ">"
      | LessThan -> "<"
      | GreaterEqual -> ">="
      | LessThanOrEqual -> "<="

  let binaryOperator =
    parser.Any
      [ parseOperator Operator.Times |> parser.Map(fun () -> BinaryOperator.Times)
        parseOperator Operator.Div |> parser.Map(fun () -> BinaryOperator.Div)
        parseOperator Operator.Plus |> parser.Map(fun () -> BinaryOperator.Plus)
        parseOperator Operator.Minus |> parser.Map(fun () -> BinaryOperator.Minus)
        parseOperator Operator.DoubleAmpersand
        |> parser.Map(fun () -> BinaryOperator.And)
        parseOperator Operator.DoublePipe |> parser.Map(fun () -> BinaryOperator.Or)
        parseOperator Operator.Equal |> parser.Map(fun () -> BinaryOperator.Equal)
        parseOperator Operator.NotEqual |> parser.Map(fun () -> BinaryOperator.NotEqual)
        parseOperator Operator.GreaterThan
        |> parser.Map(fun () -> BinaryOperator.GreaterThan)
        parseOperator Operator.LessThan |> parser.Map(fun () -> BinaryOperator.LessThan)
        parseOperator Operator.GreaterEqual
        |> parser.Map(fun () -> BinaryOperator.GreaterEqual)
        parseOperator Operator.LessThanOrEqual
        |> parser.Map(fun () -> BinaryOperator.LessThanOrEqual) ]


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


  type ComplexExpressionKind =
    | ScopedIdentifier
    | RecordDes
    | TupleCons
    | ApplicationArguments
    | BinaryExpressionChain

  type ComplexExpression =
    | ScopedIdentifier of NonEmptyList<string>
    | RecordDes of NonEmptyList<string>
    | TupleCons of NonEmptyList<Expr<TypeExpr>>
    | ApplicationArguments of NonEmptyList<Sum<Expr<TypeExpr>, TypeExpr>>
    | BinaryExpressionChain of NonEmptyList<BinaryOperator * Expr<TypeExpr>>

  let private parseAllComplexShapes: Set<ComplexExpressionKind> =
    [ ComplexExpressionKind.ApplicationArguments
      ComplexExpressionKind.BinaryExpressionChain
      ComplexExpressionKind.RecordDes
      ComplexExpressionKind.TupleCons
      ComplexExpressionKind.ScopedIdentifier ]
    |> Set.ofList

  let private parseNoComplexShapes: Set<ComplexExpressionKind> = Set.empty

  let rec expr (depth: int) (parseComplexShapes: Set<ComplexExpressionKind>) =

    let expr = expr (depth + 1)
    // let indent = "--"

    let stringLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.StringLiteral s -> Expr.Primitive(PrimitiveValue.String s) |> Some
        | _ -> None)

    let intLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.IntLiteral s -> Expr.Primitive(PrimitiveValue.Int32 s) |> Some
        | _ -> None)

    let decimalLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.DecimalLiteral d -> Expr.Primitive(PrimitiveValue.Decimal d) |> Some
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
            let! matchedExpr = expr parseAllComplexShapes
            do! parseKeyword Keyword.With

            let! cases =
              parser.AtLeastOne(
                parser {
                  do! pipeOperator
                  let! id = identifierMatch

                  do! openRoundBracketOperator
                  let! paramName = identifierMatch

                  do! parseOperator Operator.SingleArrow
                  let! body = expr parseAllComplexShapes
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
                let! body = expr parseAllComplexShapes
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
            let! body = expr parseAllComplexShapes
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
            let! value = expr parseAllComplexShapes
            do! inKeyword
            let! body = expr parseAllComplexShapes
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
                  let! value = expr parseAllComplexShapes
                  do! semicolonOperator
                  return (id, value)
                }
              )

            do! closeCurlyBracketOperator

            return Expr.RecordCons fields
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let binaryExpressionChainTail () =
      parser {
        do! binaryOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            let! fields =
              parser.AtLeastOne(
                parser {
                  let! op = binaryOperator
                  let! value = expr (parseComplexShapes |> Set.remove ComplexExpressionKind.BinaryExpressionChain)
                  return op, value
                }
              )

            return fields |> ComplexExpression.BinaryExpressionChain
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
                  let! value = expr (parseComplexShapes |> Set.remove ComplexExpressionKind.TupleCons)
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
            let! body = expr parseAllComplexShapes

            let typeDecl =
              symbols
              |> List.fold (fun acc s -> TypeExpr.Let(s, TypeExpr.NewSymbol s, acc)) typeDecl

            return Expr.TypeLet(id, typeDecl, body)
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let unaryOperatorIdentifier () =
      let singleOperator op =
        parser {
          do! parseOperator op
          return op.ToString() |> Identifier.LocalScope |> Expr.Lookup
        }

      singleOperator Operator.Bang
    // parser.Any [
    // ]

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
                [ expr (parseComplexShapes |> Set.remove ComplexExpressionKind.ApplicationArguments)
                  |> parser.Map Sum.Left
                  typeDecl |> betweenSquareBrackets |> parser.Map(snd >> Sum.Right) ]

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
        intLiteral ()
        decimalLiteral ()
        boolLiteral ()
        unitLiteral ()
        exprLet ()
        exprLambda ()
        recordCons ()
        betweenBrackets (fun () -> expr parseAllComplexShapes)
        typeLet ()
        matchWith ()
        identifierLookup ()
        unaryOperatorIdentifier () ]

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
        let! e = expr parseNoComplexShapes
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
          if parseComplexShapes.Contains ComplexExpressionKind.ScopedIdentifier then
            scopedIdentifier () :: complexShapes
          else
            complexShapes

        let complexShapes =
          if parseComplexShapes.Contains ComplexExpressionKind.BinaryExpressionChain then
            binaryExpressionChainTail () :: complexShapes
          else
            complexShapes

        let complexShapes =
          if parseComplexShapes.Contains ComplexExpressionKind.ApplicationArguments then
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
                  let fields: List<BinaryOperatorsElement<Expr<_>, BinaryOperator>> =
                    fields
                    |> NonEmptyList.ToList
                    |> Seq.collect (fun (op, e) -> [ op |> Operator; e |> Operand ])
                    |> List.ofSeq

                  let chain = Operand acc :: fields

                  let precedence: List<OperatorsPrecedence<BinaryOperator>> =
                    [ { Operators = [ BinaryOperator.Div; BinaryOperator.Times ] |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators = [ BinaryOperator.Plus; BinaryOperator.Minus ] |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators = [ BinaryOperator.And; BinaryOperator.Or ] |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators =
                          [ BinaryOperator.GreaterEqual
                            BinaryOperator.GreaterThan
                            BinaryOperator.LessThan
                            BinaryOperator.LessThanOrEqual
                            BinaryOperator.Equal
                            BinaryOperator.NotEqual ]
                          |> Set.ofList
                        Associativity = AssociateLeft } ]

                  return!
                    collapseBinaryOperatorsChain
                      { Compose =
                          fun (e1, op, e2) ->
                            Expr.Apply(Expr.Apply(Expr.Lookup(Identifier.LocalScope(op.ToString())), e1), e2)
                        ToExpr = id }
                      loc
                      precedence
                      chain
                | ScopedIdentifier ids ->
                  match acc with
                  | Expr.Lookup(Identifier.LocalScope id) ->
                    let ids = (id :: (ids |> NonEmptyList.ToList)) |> List.rev
                    return Expr.Lookup(Identifier.FullyQualified(ids.Tail, ids.Head))
                  | _ ->
                    return!
                      (loc, $"Error: cannot collapse scoped identifier chain on non-identifier")
                      |> Errors.Singleton
                      |> sum.Throw
                | RecordDes ids ->
                  return
                    Expr.RecordDes(acc, ids |> NonEmptyList.ToList |> List.rev |> List.head |> Identifier.LocalScope)
                | TupleCons fields -> return Expr.TupleCons(acc :: (fields |> NonEmptyList.ToList))
                | ApplicationArguments args ->
                  let smartApply (t1, t2) =
                    match t2 with
                    | Sum.Left t2 -> Expr.Apply(t1, t2)
                    | Sum.Right t2 -> Expr.TypeApply(t1, t2)

                  return args |> NonEmptyList.ToList |> List.fold (fun acc e -> smartApply (acc, e)) acc
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

  let program =
    parser {
      let! e = expr 0 parseAllComplexShapes
      do! parser.EndOfStream()
      return e
    }
