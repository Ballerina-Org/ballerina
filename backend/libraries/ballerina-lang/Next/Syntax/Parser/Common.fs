namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Common =

  open System
  open Ballerina.Collections.Option
  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Syntax
  open Model

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

  let binaryOperator =
    parser.Any
      [ parseOperator Operator.Times |> parser.Map(fun () -> BinaryOperator.Times)
        parseOperator Operator.Div |> parser.Map(fun () -> BinaryOperator.Div)
        parseOperator Operator.Percentage |> parser.Map(fun () -> BinaryOperator.Mod)
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
  let ifKeyword = parseKeyword Keyword.If
  let thenKeyword = parseKeyword Keyword.Then
  let elseKeyword = parseKeyword Keyword.Else

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
