namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Common =

  open System
  open Ballerina.Collections.Option
  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Syntax
  open Model
  open Ballerina

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
  let doubleDotOperator = parseOperator Operator.DoubleDot
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
  let pipeGreaterThanOperator = parseOperator (Operator.PipeGreaterThan)
  let doubleGreaterThanOperator = parseOperator (Operator.DoubleGreaterThan)
  let starOperator = parseOperator (Operator.Times)
  let singleArrowOperator = parseOperator (Operator.SingleArrow)

  type BinaryExprOperator =
    | Times
    | Div
    | Mod
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
    | PipeGreaterThan
    | DoubleGreaterThan

    override this.ToString() =
      match this with
      | Times -> "*"
      | Div -> "/"
      | Mod -> "%"
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
      | PipeGreaterThan -> "|>"
      | DoubleGreaterThan -> ">>"

  let binaryExprOperator =
    parser.Any
      [ parseOperator Operator.Times |> parser.Map(fun () -> BinaryExprOperator.Times)
        parseOperator Operator.Div |> parser.Map(fun () -> BinaryExprOperator.Div)
        parseOperator Operator.Percentage
        |> parser.Map(fun () -> BinaryExprOperator.Mod)
        parseOperator Operator.Plus |> parser.Map(fun () -> BinaryExprOperator.Plus)
        parseOperator Operator.Minus |> parser.Map(fun () -> BinaryExprOperator.Minus)
        parseOperator Operator.DoubleAmpersand
        |> parser.Map(fun () -> BinaryExprOperator.And)
        parseOperator Operator.DoublePipe |> parser.Map(fun () -> BinaryExprOperator.Or)
        parseOperator Operator.Equal |> parser.Map(fun () -> BinaryExprOperator.Equal)
        parseOperator Operator.NotEqual
        |> parser.Map(fun () -> BinaryExprOperator.NotEqual)
        parseOperator Operator.GreaterThan
        |> parser.Map(fun () -> BinaryExprOperator.GreaterThan)
        parseOperator Operator.LessThan
        |> parser.Map(fun () -> BinaryExprOperator.LessThan)
        parseOperator Operator.GreaterEqual
        |> parser.Map(fun () -> BinaryExprOperator.GreaterEqual)
        parseOperator Operator.LessThanOrEqual
        |> parser.Map(fun () -> BinaryExprOperator.LessThanOrEqual)
        parseOperator Operator.PipeGreaterThan
        |> parser.Map(fun () -> BinaryExprOperator.PipeGreaterThan)
        parseOperator Operator.DoubleGreaterThan
        |> parser.Map(fun () -> BinaryExprOperator.DoubleGreaterThan) ]

  type BinaryTypeOperator =
    | Times
    | Plus
    | SingleArrow

    override this.ToString() =
      match this with
      | Times -> "*"
      | Plus -> "+"
      | SingleArrow -> "->"

  let binaryTypeOperator =
    parser.Any
      [ parseOperator Operator.Times |> parser.Map(fun () -> BinaryTypeOperator.Times)
        parseOperator Operator.Plus |> parser.Map(fun () -> BinaryTypeOperator.Plus)
        parseOperator Operator.SingleArrow
        |> parser.Map(fun () -> BinaryTypeOperator.SingleArrow) ]

  let parseKeyword kw =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.Keyword actualKw when kw = actualKw -> true
      | _ -> false)
    |> parser.Ignore

  let typeKeyword = parseKeyword Keyword.Type
  let ofKeyword = parseKeyword Keyword.Of
  let letKeyword = parseKeyword Keyword.Let
  let withKeyword = parseKeyword Keyword.With
  let inKeyword = parseKeyword Keyword.In
  let ifKeyword = parseKeyword Keyword.If
  let thenKeyword = parseKeyword Keyword.Then
  let elseKeyword = parseKeyword Keyword.Else

  let singleIdentifier =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.Identifier id -> Some id
      | _ -> None)

  let caseLiteral () =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.CaseLiteral(i, n) -> { Case = i; Count = n } |> Some
      | _ -> None)

  let softKeyword k =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.Identifier id when k = id -> Some()
      | _ -> None)

  let schemaKeyword = softKeyword "schema"
  let entityKeyword = softKeyword "entity"
  let relationKeyword = softKeyword "relation"
  let searchByKeyword = softKeyword "searchBy"
  let propertyKeyword = softKeyword "property"
  let onKeyword = softKeyword "on"
  let creatingKeyword = softKeyword "creating"
  let updatingKeyword = softKeyword "updating"
  let deletingKeyword = softKeyword "deleting"
  let createdKeyword = softKeyword "created"
  let updatedKeyword = softKeyword "updated"
  let deletedKeyword = softKeyword "deleted"
  let linkingKeyword = softKeyword "linking"

  let unlinkingKeyword = softKeyword "unlinking"
  let linkedKeyword = softKeyword "linked"
  let unlinkedKeyword = softKeyword "unlinked"
  let fromKeyword = softKeyword "from"
  let toKeyword = softKeyword "to"
  let cardinalityKeyword = softKeyword "cardinality"
  let fieldKeyword = softKeyword "field"
  let mapKeyword = softKeyword "map"
  let iteratorKeyword = softKeyword "iterator"
  let caseKeyword = softKeyword "case"
  let itemKeyword = softKeyword "item"

  let rec identifiersMatch () =
    parser {
      let! id = singleIdentifier

      return!
        parser.Any
          [ parser {
              do! doubleColonOperator

              return!
                parser {
                  let! ids = identifiersMatch () |> parser.Map(NonEmptyList.ToList)
                  return NonEmptyList.OfList(id, ids)
                }
                |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
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
          let! id = singleIdentifier
          return Identifier.LocalScope id
        } ]

  let identifierWithLookups () =
    parser {
      let! id = identifierLocalOrFullyQualified ()

      let! lookups =
        parser.Many(
          parser {
            do! dotOperator
            let! prop = singleIdentifier
            return prop
          }
        )

      return id, lookups
    }

  let betweenBrackets p =
    parser {
      do! openRoundBracketOperator

      return!
        parser {
          let! res = p ()
          do! closeRoundBracketOperator
          return res
        }
        |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
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
        |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
    }

  let afterKeyword k p =
    parser {
      do! k
      return! p |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
    }
    |> parser.MapError(Errors<Location>.FilterHighestPriorityOnly)

  let intLiteralToken () =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.IntLiteral s -> s |> Some
      | _ -> None)
