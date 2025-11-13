namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Expr =

  open System
  open Ballerina.Collections.Option
  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Model
  open Common
  open Precedence
  open Type
  open Ballerina.DSL.Next.Syntax

  type ComplexExpressionKind =
    | ScopedIdentifier
    | RecordDes
    | TupleCons
    | ApplicationArguments
    | BinaryExpressionChain

  type ComplexExpression<'valueExt> =
    | ScopedIdentifier of NonEmptyList<string>
    | RecordOrTupleDesChain of NonEmptyList<Sum<string, int>>
    | TupleCons of NonEmptyList<Expr<TypeExpr, Identifier, 'valueExt>>
    | ApplicationArguments of NonEmptyList<Sum<Expr<TypeExpr, Identifier, 'valueExt>, TypeExpr>>
    | BinaryExpressionChain of NonEmptyList<BinaryExprOperator * Expr<TypeExpr, Identifier, 'valueExt>>

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
        | Token.StringLiteral s ->
          Expr.Primitive(PrimitiveValue.String s, t.Location, TypeCheckScope.Empty)
          |> Some
        | _ -> None)

    let caseLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.CaseLiteral(i, n) -> { Case = i; Count = n } |> Some
        | _ -> None)

    let actualInt () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.IntLiteral s -> s |> Some
        | _ -> None)

    let intLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.IntLiteral s -> Expr.Primitive(PrimitiveValue.Int32 s, t.Location, TypeCheckScope.Empty) |> Some
        | _ -> None)

    let int64Literal () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.Int64Literal s -> Expr.Primitive(PrimitiveValue.Int64 s, t.Location, TypeCheckScope.Empty) |> Some
        | _ -> None)

    let decimalLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.DecimalLiteral d ->
          Expr.Primitive(PrimitiveValue.Decimal d, t.Location, TypeCheckScope.Empty)
          |> Some
        | _ -> None)

    let float32Literal () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.Float32Literal s ->
          Expr.Primitive(PrimitiveValue.Float32 s, t.Location, TypeCheckScope.Empty)
          |> Some
        | _ -> None)

    let float64Literal () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.Float64Literal s ->
          Expr.Primitive(PrimitiveValue.Float64 s, t.Location, TypeCheckScope.Empty)
          |> Some
        | _ -> None)

    let boolLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.BoolLiteral b -> Expr.Primitive(PrimitiveValue.Bool b, t.Location, TypeCheckScope.Empty) |> Some
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
            let! matchedExpr = expr parseAllComplexShapes
            do! parseKeyword Keyword.With

            let! cases =
              parser.AtLeastOne(
                parser {
                  do! pipeOperator

                  let! id =
                    parser.Any
                      [ identifierLocalOrFullyQualified () |> parser.Map Left
                        caseLiteral () |> parser.Map Right ]

                  do! openRoundBracketOperator
                  let! paramName = identifierMatch

                  do! parseOperator Operator.SingleArrow
                  let! body = expr parseAllComplexShapes
                  do! closeRoundBracketOperator
                  return id, (Var.Create paramName, body)
                }
              )
              |> parser.Map(NonEmptyList.ToList)

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
                (loc, "Error: cannot mix union cases and sum cases in match expression")
                |> Errors.Singleton
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
                Expr.Apply(Expr.SumDes(sumCases, loc, TypeCheckScope.Empty), matchedExpr, loc, TypeCheckScope.Empty)

          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let exprLambda () =
      parser {
        do! parseKeyword Keyword.Fun
        let! loc = parser.Location

        return!
          parser {

            let! paramName, paramType =
              parser.Any
                [ parser {
                    do! openRoundBracketOperator
                    let! paramName = identifierMatch
                    do! colonOperator
                    let! typeDecl = typeDecl parseAllComplexTypeShapes
                    do! closeRoundBracketOperator
                    return paramName, typeDecl |> Some
                  }
                  parser {
                    let! paramName = identifierMatch
                    return paramName, None
                  } ]

            do! parseOperator Operator.SingleArrow
            let! body = expr parseAllComplexShapes
            return Expr.Lambda(Var.Create paramName, paramType, body, loc, TypeCheckScope.Empty)
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)

      }

    let exprLet () =
      parser {
        do! letKeyword
        let! loc = parser.Location

        return!
          parser {
            let! paramName, paramType =
              parser.Any
                [ parser {
                    do! openRoundBracketOperator
                    let! paramName = identifierMatch
                    do! colonOperator

                    return!
                      parser {
                        let! typeDecl = typeDecl parseAllComplexTypeShapes
                        do! closeRoundBracketOperator
                        return paramName, typeDecl |> Some
                      }
                      |> parser.MapError(Errors.SetPriority ErrorPriority.High)
                  }
                  parser {
                    let! paramName = identifierMatch

                    let! paramType =
                      parser.Any
                        [ parser {
                            do! colonOperator

                            return!
                              parser {
                                let! typeDecl = typeDecl parseAllComplexTypeShapes
                                return typeDecl |> Some
                              }
                              |> parser.MapError(Errors.SetPriority ErrorPriority.High)
                          }
                          parser { return None } ]

                    return paramName, paramType
                  } ]

            do! equalsOperator
            let! value = expr parseAllComplexShapes
            do! inKeyword
            let! body = expr parseAllComplexShapes
            return Expr.Let(paramName |> Var.Create, paramType, value, body, loc, TypeCheckScope.Empty)
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let exprConditional () =
      parser {
        do! ifKeyword
        let! loc = parser.Location

        return!
          parser {
            let! cond = expr parseAllComplexShapes
            do! thenKeyword
            let! thenBranch = expr parseAllComplexShapes
            do! elseKeyword
            let! elseBranch = expr parseAllComplexShapes
            return Expr.If(cond, thenBranch, elseBranch, loc, TypeCheckScope.Empty)
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let recordCons () =
      parser {
        do! openCurlyBracketOperator
        let! loc = parser.Location

        return!
          parser {
            let! firstFieldOrWithExpr =
              parser {
                let! e = expr parseAllComplexShapes

                return!
                  parser.Any
                    [ parser {
                        do! withKeyword |> parser.Ignore
                        return Sum.Right e
                      }

                      parser {
                        let! e =
                          e
                          |> Expr.AsLookup
                          |> sum.MapError(Errors.FromErrors(e.Location))
                          |> parser.OfSum

                        do! equalsOperator |> parser.Ignore
                        let! value = expr parseAllComplexShapes
                        return Sum.Left(e, value)
                      } ]
              }

            let! fields =
              parser.ManyIndex(fun i ->
                parser {
                  if firstFieldOrWithExpr.IsLeft || i > 0 then
                    do! semicolonOperator

                  let! id = identifierLocalOrFullyQualified ()
                  do! equalsOperator
                  let! value = expr parseAllComplexShapes
                  return (id, value)
                })

            do! semicolonOperator |> parser.Try |> parser.Ignore

            do! closeCurlyBracketOperator

            match firstFieldOrWithExpr with
            | Sum.Left({ Id = f }, v) -> return Expr.RecordCons((f, v) :: fields, loc, TypeCheckScope.Empty)
            | Sum.Right e -> return Expr.RecordWith(e, fields, loc, TypeCheckScope.Empty)
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
                  return! parser.Any [ identifierMatch |> parser.Map Left; actualInt () |> parser.Map Right ]
                }
              )

            return fields |> ComplexExpression.RecordOrTupleDesChain
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let typeLet () =
      parser {
        do! typeKeyword
        let! loc = parser.Location

        return!
          parser {
            let! id = identifierMatch
            do! equalsOperator
            let! typeDecl = typeDecl parseAllComplexTypeShapes
            do! inKeyword
            let! body = expr parseAllComplexShapes

            let symbols, symbolsKind =
              match typeDecl with
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
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let unaryOperatorIdentifier () =
      let singleOperator op =
        parser {
          do! parseOperator op
          let! loc = parser.Location
          return Expr.Lookup(op.ToString() |> Identifier.LocalScope, loc, TypeCheckScope.Empty)
        }

      singleOperator Operator.Bang
    // parser.Any [
    // ]

    let identifierLookup () =
      parser {
        let! id = identifierMatch
        let! loc = parser.Location
        // do Console.WriteLine($"{String.replicate (depth * 2) indent}> Parsed identifier: {id.ToFSharpString}")
        return Expr.Lookup(Identifier.LocalScope id, loc, TypeCheckScope.Empty)
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

    let binaryExpressionChainTail () =
      parser {
        do! binaryExprOperator |> parser.Lookahead |> parser.Ignore

        return!
          parser {
            let! fields =
              parser.AtLeastOne(
                parser {
                  let! op = binaryExprOperator
                  let! value = expr (parseComplexShapes |> Set.remove ComplexExpressionKind.BinaryExpressionChain)
                  return op, value
                }
              )

            return fields |> ComplexExpression.BinaryExpressionChain
          }
          |> parser.MapError(Errors.SetPriority ErrorPriority.High)
      }

    let argExpr () =
      parser {
        return!
          parser {
            let! res =
              parser.Any
                [ expr parseNoComplexShapes // (parseComplexShapes |> Set.remove ComplexExpressionKind.ApplicationArguments)
                  |> parser.Map Sum.Left
                  (fun () -> typeDecl parseAllComplexTypeShapes)
                  |> betweenSquareBrackets
                  |> parser.Map(Sum.Right) ]

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
        int64Literal ()
        caseLiteral () |> parser.Map Expr.SumCons
        decimalLiteral ()
        float32Literal ()
        float64Literal ()
        boolLiteral ()
        unitLiteral ()
        exprLet ()
        exprLambda ()
        exprConditional ()
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
                  let fields: List<BinaryOperatorsElement<Expr<_, _, _>, BinaryExprOperator>> =
                    fields
                    |> NonEmptyList.ToList
                    |> Seq.collect (fun (op, e) -> [ op |> Precedence.Operator; e |> Precedence.Operand ])
                    |> List.ofSeq

                  let chain = Operand acc :: fields

                  let precedence: List<OperatorsPrecedence<BinaryExprOperator>> =
                    [ { Operators =
                          [ BinaryExprOperator.DoubleGreaterThan; BinaryExprOperator.PipeGreaterThan ]
                          |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators =
                          [ BinaryExprOperator.Div; BinaryExprOperator.Times; BinaryExprOperator.Mod ]
                          |> Set.ofList
                        Associativity = AssociateLeft }
                      { Operators = [ BinaryExprOperator.Plus; BinaryExprOperator.Minus ] |> Set.ofList
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
                      { Operators = [ BinaryExprOperator.And; BinaryExprOperator.Or ] |> Set.ofList
                        Associativity = AssociateLeft } ]

                  return!
                    collapseBinaryOperatorsChain
                      { Compose =
                          fun (e1, op, e2) ->
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
                                    Expr.Lookup(Identifier.LocalScope("x"), loc, TypeCheckScope.Empty),
                                    loc,
                                    TypeCheckScope.Empty
                                  ),
                                  loc,
                                  TypeCheckScope.Empty
                                ),
                                loc,
                                TypeCheckScope.Empty
                              )
                            | BinaryExprOperator.PipeGreaterThan -> Expr.Apply(e2, e1, loc, TypeCheckScope.Empty)
                            | _ ->
                              Expr.Apply(
                                Expr.Apply(
                                  Expr.Lookup(Identifier.LocalScope(op.ToString()), loc, TypeCheckScope.Empty),
                                  e1,
                                  loc,
                                  TypeCheckScope.Empty
                                ),
                                e2,
                                loc,
                                TypeCheckScope.Empty
                              )
                        ToExpr = id }
                      loc
                      precedence
                      chain
                | ScopedIdentifier ids ->
                  match acc.Expr with
                  | ExprRec.Lookup({ Id = Identifier.LocalScope id }) ->
                    let ids = (id :: (ids |> NonEmptyList.ToList)) |> List.rev
                    return Expr.Lookup(Identifier.FullyQualified(ids.Tail, ids.Head), loc, TypeCheckScope.Empty)
                  | _ ->
                    return!
                      (loc, $"Error: cannot collapse scoped identifier chain on non-identifier")
                      |> Errors.Singleton
                      |> sum.Throw
                | RecordOrTupleDesChain ids ->
                  return
                    ids
                    |> NonEmptyList.ToList
                    |> List.fold
                      (fun acc id ->
                        match id with
                        | Sum.Left id -> Expr.RecordDes(acc, id |> Identifier.LocalScope, loc, TypeCheckScope.Empty)
                        | Sum.Right idx -> Expr.TupleDes(acc, { Index = idx }, loc, TypeCheckScope.Empty))
                      acc
                | TupleCons fields ->
                  return Expr.TupleCons(acc :: (fields |> NonEmptyList.ToList), loc, TypeCheckScope.Empty)
                | ApplicationArguments args ->
                  let smartApply (t1, t2) =
                    match t2 with
                    | Sum.Left t2 -> Expr.Apply(t1, t2, loc, TypeCheckScope.Empty)
                    | Sum.Right t2 -> Expr.TypeApply(t1, t2, loc, TypeCheckScope.Empty)

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

  let program<'valueExt> () : Parser<Expr<TypeExpr, Identifier, 'valueExt>, _, _, _> =
    parser {
      let! e = expr 0 parseAllComplexShapes
      do! parser.EndOfStream()
      return e
    }
