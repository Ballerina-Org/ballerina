namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Query =

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

  type ComplexExpressionKind =
    | ScopedIdentifier
    | RecordDes
    | TupleCons
    | ApplicationArguments
    | BinaryExpressionChain

  type ComplexExpression<'valueExt> =
    | ScopedIdentifier of NonEmptyList<string>
    | RecordOrTupleDesChain of NonEmptyList<Sum<string, int>>
    | TupleCons of NonEmptyList<ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
    | ApplicationArguments of NonEmptyList<ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>>
    | BinaryExpressionChain of
      NonEmptyList<BinaryExprOperator * ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>>

  let private parseAllComplexShapes: Set<ComplexExpressionKind> =
    [ ComplexExpressionKind.ApplicationArguments
      ComplexExpressionKind.BinaryExpressionChain
      ComplexExpressionKind.RecordDes
      ComplexExpressionKind.TupleCons
      ComplexExpressionKind.ScopedIdentifier ]
    |> Set.ofList

  let private parseNoComplexShapes: Set<ComplexExpressionKind> = Set.empty

  let rec queryexpr<'valueExt>
    (query: unit -> _)
    (depth: int)
    (parseComplexShapes: Set<ComplexExpressionKind>)
    : Parser<ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =

    let expr = queryexpr query (depth + 1)

    let singleTermIdentifier =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.Identifier id -> Some id
        | Token.Keyword(Keyword.Schema) -> Keyword.Schema.ToString() |> Some
        | Token.Keyword(Keyword.Entity) -> Keyword.Entity.ToString() |> Some
        | Token.Keyword(Keyword.Relation) -> Keyword.Relation.ToString() |> Some
        | Token.Keyword(Keyword.Include) -> Keyword.Include.ToString() |> Some
        | _ -> None)

    let singleIdentifier = singleTermIdentifier

    let stringLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.StringLiteral s ->
          QueryConstant(PrimitiveValue.String s)
          |> ExprQueryExpr.Create t.Location
          |> Some // , t.Location, TypeCheckScope.Empty
        | _ -> None)

    let intLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.IntLiteral s -> QueryConstant(PrimitiveValue.Int32 s) |> ExprQueryExpr.Create t.Location |> Some
        | _ -> None)

    let int64Literal () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.Int64Literal s -> QueryConstant(PrimitiveValue.Int64 s) |> ExprQueryExpr.Create t.Location |> Some
        | _ -> None)

    let decimalLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.DecimalLiteral d ->
          QueryConstant(PrimitiveValue.Decimal d)
          |> ExprQueryExpr.Create t.Location
          |> Some
        | _ -> None)

    let float32Literal () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.Float32Literal s ->
          QueryConstant(PrimitiveValue.Float32 s)
          |> ExprQueryExpr.Create t.Location
          |> Some
        | _ -> None)

    let float64Literal () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.Float64Literal s ->
          QueryConstant(PrimitiveValue.Float64 s)
          |> ExprQueryExpr.Create t.Location
          |> Some
        | _ -> None)

    let boolLiteral () =
      parser.Exactly(fun t ->
        match t.Token with
        | Token.BoolLiteral b -> QueryConstant(PrimitiveValue.Bool b) |> ExprQueryExpr.Create t.Location |> Some
        | _ -> None)

    let unitLiteral () =
      parser {
        do! openRoundBracketOperator
        do! closeRoundBracketOperator
        let! loc = parser.Location
        return QueryConstant(PrimitiveValue.Unit) |> ExprQueryExpr.Create loc
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

            return
              ExprQueryExprRec.QueryConditional(cond, thenBranch, elseBranch)
              |> ExprQueryExpr.Create loc
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }

    let exprCount () =
      parser {
        do! countKeyword
        let! loc = parser.Location

        return!
          parser {
            let! q = query ()

            return ExprQueryExprRec.QueryCount(q) |> ExprQueryExpr.Create loc
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }

    let exprExists () =
      parser {
        do! existsKeyword
        let! loc = parser.Location

        return!
          parser {
            let! q = query ()

            return ExprQueryExprRec.QueryExists(q) |> ExprQueryExpr.Create loc
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }

    let exprArray () =
      parser {
        do! arrayKeyword
        let! loc = parser.Location

        return!
          parser {
            let! q = query ()

            return ExprQueryExprRec.QueryArray(q) |> ExprQueryExpr.Create loc
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
                  let! value = expr (parseComplexShapes |> Set.remove ComplexExpressionKind.TupleCons)
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
            let! fields =
              parser.AtLeastOne(
                parser {
                  do! dotOperator
                  return! parser.Any [ singleIdentifier |> parser.Map Left; intLiteralToken () |> parser.Map Right ]
                }
              )

            return fields |> ComplexExpression.RecordOrTupleDesChain
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }

    let unaryOperatorIdentifier () =
      let singleOperator op =
        parser {
          do! parseOperator op
          let! loc = parser.Location

          return
            ExprQueryExprRec.QueryLookup(Identifier.LocalScope(op.ToString()))
            |> ExprQueryExpr.Create loc
        }

      singleOperator Operator.Bang

    let identifierLookup () =
      parser {
        let! id =
          parser.Any
            [ singleIdentifier
              schemaKeyword |> parser.Map(replaceWith "schema")
              entityKeyword |> parser.Map(replaceWith "entity")
              relationKeyword |> parser.Map(replaceWith "relation") ]

        let! loc = parser.Location
        // do Console.WriteLine($"{String.replicate (depth * 2) indent}> Parsed identifier: {id.ToFSharpString}")
        return
          ExprQueryExprRec.QueryLookup(Identifier.LocalScope id)
          |> ExprQueryExpr.Create loc
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
                  let! op = binaryExprOperator
                  let! value = expr (parseComplexShapes |> Set.remove ComplexExpressionKind.BinaryExpressionChain)
                  return op, value
                }
              )

            return fields |> ComplexExpression.BinaryExpressionChain
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
      }

    let argExpr () =
      expr parseNoComplexShapes
      |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))

    let application () =
      parser {
        let! args = parser.AtLeastOne(argExpr ())
        return args |> ComplexExpression.ApplicationArguments
      }

    let simpleShapes =
      [ stringLiteral ()
        intLiteral ()
        int64Literal ()
        float32Literal ()
        float64Literal ()
        decimalLiteral ()
        boolLiteral ()
        unitLiteral ()
        betweenBrackets (fun () -> expr parseAllComplexShapes)
        exprConditional ()
        exprCount ()
        exprExists ()
        exprArray ()
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
        return!
          simpleShapes
          |> parser.Any
          |> parser.MapError(Errors<_>.FilterHighestPriorityOnly)
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

        let res: Sum<ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>, Errors<Location>> =
          res
          |> List.fold
            (fun acc e ->
              sum {
                let! acc = acc

                match e with
                | BinaryExpressionChain fields ->
                  let fields: List<BinaryOperatorsElement<ExprQueryExpr<_, _, _>, BinaryExprOperator>> =
                    fields
                    |> NonEmptyList.ToList
                    |> Seq.collect (fun (op, e) ->
                      [ op |> Precedence.Operator; (e, NonMergeable) |> Precedence.Operand ])
                    |> List.ofSeq

                  let chain = Operand(acc, Mergeable) :: fields

                  let precedence: List<OperatorsPrecedence<BinaryExprOperator>> =
                    [ { Operators =
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
                          fun (e1, _src1, op, e2, _src2) ->
                            let query_op =
                              match op with
                              | BinaryExprOperator.Plus -> QueryIntrinsic.Plus
                              | BinaryExprOperator.Times -> QueryIntrinsic.Multiply
                              | BinaryExprOperator.Div -> QueryIntrinsic.Divide
                              | BinaryExprOperator.Mod -> QueryIntrinsic.Modulo
                              | BinaryExprOperator.Minus -> QueryIntrinsic.Minus
                              | BinaryExprOperator.And -> QueryIntrinsic.And
                              | BinaryExprOperator.Or -> QueryIntrinsic.Or
                              | BinaryExprOperator.Equal -> QueryIntrinsic.Equals
                              | BinaryExprOperator.NotEqual -> QueryIntrinsic.NotEquals
                              | BinaryExprOperator.GreaterThan -> QueryIntrinsic.GreaterThan
                              | BinaryExprOperator.LessThan -> QueryIntrinsic.LessThan
                              | BinaryExprOperator.GreaterEqual -> QueryIntrinsic.GreaterThanOrEqual
                              | BinaryExprOperator.LessThanOrEqual -> QueryIntrinsic.LessThanOrEqual
                              | BinaryExprOperator.PipeGreaterThan ->
                                failwith "PipeGreaterThan operator is not supported in query expressions"
                              | BinaryExprOperator.DoubleGreaterThan ->
                                failwith "DoubleGreaterThan operator is not supported in query expressions"

                            ExprQueryExprRec.QueryApply(
                              ExprQueryExprRec.QueryIntrinsic(
                                query_op,
                                TypeQueryRow.PrimitiveType(PrimitiveType.Unit, false)
                              )
                              |> ExprQueryExpr.Create loc,
                              ExprQueryExprRec.QueryTupleCons([ e1; e2 ]) |> ExprQueryExpr.Create loc
                            )
                            |> ExprQueryExpr.Create loc,
                            NonMergeable
                        ToExpr = id }
                      loc
                      precedence
                      chain
                | ScopedIdentifier ids ->
                  match acc.Expr with
                  | ExprQueryExprRec.QueryLookup(Identifier.LocalScope id) ->
                    let ids = (id :: (ids |> NonEmptyList.ToList)) |> List.rev

                    return
                      ExprQueryExprRec.QueryLookup(Identifier.FullyQualified(ids.Tail, ids.Head))
                      |> ExprQueryExpr.Create loc
                  | _ ->
                    return!
                      (fun () -> $"Error: cannot collapse scoped identifier chain on non-identifier")
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
                          ExprQueryExprRec.QueryRecordDes(acc, id |> Identifier.LocalScope, false)
                          |> ExprQueryExpr.Create loc
                        | Sum.Right idx ->
                          ExprQueryExprRec.QueryTupleDes(acc, { Index = idx }, false)
                          |> ExprQueryExpr.Create loc)
                      acc
                | TupleCons fields ->
                  return
                    ExprQueryExprRec.QueryTupleCons(acc :: (fields |> NonEmptyList.ToList))
                    |> ExprQueryExpr.Create loc
                | ApplicationArguments args ->
                  let smartApply (t1, t2) =
                    ExprQueryExprRec.QueryApply(t1, t2) |> ExprQueryExpr.Create loc

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


  let rec private query_iterators_and_datasources<'valueExt>
    (expr: unit -> Parser<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>>)
    query
    =
    parser {
      do! fromKeyword

      return!
        parser {

          let! loc = parser.Location
          let! _iterators = queryexpr<'valueExt> query 0 parseAllComplexShapes

          let! _iterators =
            [ parser {
                let! singleIterator =
                  _iterators
                  |> ExprQueryExpr.AsLookup
                  |> sum.MapError(Errors.MapContext(replaceWith loc))
                  |> parser.OfSum

                let! singleIterator =
                  singleIterator
                  |> Identifier.AsLocalScope
                  |> sum.MapError(Errors.MapContext(replaceWith loc))
                  |> parser.OfSum

                return singleIterator |> List.singleton
              }
              parser {
                let! items =
                  _iterators
                  |> ExprQueryExpr.AsTupleCons
                  |> sum.MapError(Errors.MapContext(replaceWith loc))
                  |> parser.OfSum

                let! items =
                  items
                  |> List.map (fun item ->
                    item
                    |> ExprQueryExpr.AsLookup
                    |> sum.MapError(Errors.MapContext(replaceWith loc))
                    |> parser.OfSum)
                  |> parser.All

                let! items =
                  items
                  |> List.map (fun item ->
                    item
                    |> Identifier.AsLocalScope
                    |> sum.MapError(Errors.MapContext(replaceWith loc))
                    |> parser.OfSum)
                  |> parser.All

                return items
              } ]
            |> parser.Any

          do! inKeyword
          let! _dataSources = expr ()

          let! _dataSources =
            [ parser {
                let! items =
                  _dataSources
                  |> Expr.AsTupleCons
                  |> sum.MapError(Errors.MapContext(replaceWith _dataSources.Location))
                  |> parser.OfSum

                return items.Items
              }
              parser { return _dataSources |> List.singleton } ]
            |> parser.Any

          if List.length _iterators <> List.length _dataSources then
            return!
              (fun () -> $"Error: number of iterators and data sources must be the same")
              |> Errors.Singleton loc
              |> parser.Throw
          else
            let iterators_with_datasources = List.zip _iterators _dataSources

            let! iterators_with_datasources =
              iterators_with_datasources
              |> NonEmptyList.TryOfList
              |> sum.OfOption(
                Errors.Singleton loc (fun () ->
                  "Failed to parse query: number of iterators and data sources must be the same")
              )
              |> parser.OfSum

            let iterators_with_datasources =
              iterators_with_datasources
              |> NonEmptyList.map (fun (v, ds) ->
                { ExprQueryIterator.Location = ds.Location
                  Var = v |> Var.Create
                  VarType = None
                  Source = ds })

            return iterators_with_datasources
        }
        |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
    }

  let private query_joins<'valueExt> query =
    parser {
      let! loc = parser.Location
      let! starts_with_join = joinKeyword |> parser.Try

      match starts_with_join with
      | Right _ -> return None
      | Left _ ->
        return!
          parser {
            do! onKeyword

            let! _joinConditions =
              parser.ManyIndex(fun i ->
                parser {
                  if i > 0 then
                    do! andKeyword

                  let! join_terms = queryexpr<'valueExt> query 0 parseAllComplexShapes

                  let! join_terms =
                    join_terms
                    |> ExprQueryExpr.AsTupleCons
                    |> sum.MapError(Errors.MapContext(replaceWith loc))
                    |> parser.OfSum

                  match join_terms with
                  | [ left; right ] ->
                    return
                      { ExprQueryJoin.Location = left.Location
                        Left = left
                        Right = right }
                  | _ ->
                    return!
                      (fun () -> $"Error: each join condition must consist of exactly two terms")
                      |> Errors.Singleton loc
                      |> parser.Throw
                })

            let! _joinConditions =
              _joinConditions
              |> NonEmptyList.TryOfList
              |> sum.OfOption(Errors.Singleton loc (fun () -> "At least one join condition is mandatory"))
              |> parser.OfSum

            return Some _joinConditions

          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
    }

  let private query_where<'valueExt> query =
    parser {
      let! starts_with_where = whereKeyword |> parser.Try

      match starts_with_where with
      | Right _ -> return None
      | Left _ ->
        return!
          parser {
            let! predicate = queryexpr<'valueExt> query 0 parseAllComplexShapes
            return Some predicate
          }
          |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
    }

  let private query_select<'valueExt> query =
    parser {
      do! selectKeyword

      return!
        queryexpr<'valueExt> query 0 parseAllComplexShapes
        |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
    }

  let private query_orderby<'valueExt> query =
    parser {
      let! starts_with_orderby = orderByKeyword |> parser.Try

      return!
        parser {
          match starts_with_orderby with
          | Right _ -> return None
          | Left _ ->
            let! predicate = queryexpr<'valueExt> query 0 parseAllComplexShapes
            let! is_ascending = ascendingKeyword |> parser.Try

            if is_ascending.IsLeft then
              return Some(predicate, OrderByDirection.Asc)
            else
              let! is_descending = descendingKeyword |> parser.Try

              if is_descending.IsLeft then
                return Some(predicate, OrderByDirection.Desc)
              else
                let! loc = parser.Location

                return!
                  (fun () -> $"Error: expected 'ascending' or 'descending' after 'orderby'")
                  |> Errors.Singleton loc
                  |> parser.Throw
        }
        |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
    }

  let private query_distinct<'valueExt> (query) =
    parser {
      let! starts_with_distinct = distinctKeyword |> parser.Try

      return!
        parser {
          match starts_with_distinct with
          | Right _ -> return None
          | Left _ ->
            return!
              parser {
                let! distinction = queryexpr<'valueExt> query 0 parseAllComplexShapes
                return Some distinction
              }
              |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
        }
        |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
    }

  let rec private query'<'valueExt>
    (expr: unit -> Parser<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>>)
    ()
    : Parser<ExprQuery<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
    parser {
      do! queryKeyword

      return!
        parser {
          let query = query'
          do! openCurlyBracketOperator

          let! iterators_with_datasources = query_iterators_and_datasources expr (query expr)
          let! joins = query_joins (query expr)
          let! where_ = query_where (query expr)
          let! select_expr = query_select<'valueExt> (query expr)
          let! orderby_ = query_orderby (query expr)
          let! distinct_ = query_distinct (query expr)

          do! closeCurlyBracketOperator

          let res =
            { Iterators = iterators_with_datasources
              Joins = joins
              Where = where_
              Select = select_expr
              OrderBy = orderby_
              Distinct = distinct_
              // Closure and DeserializeFrom are placeholders, they will be calculated by the type checker
              Closure = Map.empty
              DeserializeFrom = TypeQueryRow.PrimitiveType(PrimitiveType.Unit, false) }
            |> SimpleQuery

          return res
        }
        |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
    }

  let query<'valueExt>
    (expr: unit -> Parser<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>>)
    ()
    : Parser<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>> =
    parser {
      let! loc = parser.Location

      let! q = query' expr ()
      let res = Expr.Query(q, loc, TypeCheckScope.Empty)
      return res
    }
