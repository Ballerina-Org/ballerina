namespace Ballerina.DSL.Next.Syntax

[<AutoOpen>]
module Lexer =
  open Ballerina.Parser
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open System

  type Symbol = char

  type Bracket =
    | Open
    | Close

  type Keyword =
    | Type
    | Of
    | Function
    | Fun
    | Let
    | In
    | Match
    | With
    | If
    | Then
    | Else

    override this.ToString() =
      match this with
      | Type -> "type"
      | Of -> "of"
      | Function -> "function"
      | Fun -> "fun"
      | Let -> "let"
      | In -> "in"
      | Match -> "match"
      | With -> "with"
      | If -> "if"
      | Then -> "then"
      | Else -> "else"

  type Operator =
    | Equals
    | CurlyBracket of Bracket
    | SquareBracket of Bracket
    | RoundBracket of Bracket
    | DoubleColon
    | Colon
    | SemiColon
    | DoublePipe
    | DoubleAmpersand
    | Pipe
    | SingleArrow
    | DoubleArrow
    | Minus
    | GreaterThan
    | LessThan
    | Dot
    | Comma
    | At
    | Times

    override this.ToString() =
      match this with
      | Equals -> "="
      | CurlyBracket Open -> "{"
      | CurlyBracket Close -> "}"
      | SquareBracket Open -> "["
      | SquareBracket Close -> "]"
      | RoundBracket Open -> "("
      | RoundBracket Close -> ")"
      | DoubleColon -> "::"
      | Colon -> ":"
      | SemiColon -> ";"
      | DoublePipe -> "||"
      | DoubleAmpersand -> "&&"
      | Pipe -> "|"
      | SingleArrow -> "->"
      | DoubleArrow -> "=>"
      | Minus -> "-"
      | GreaterThan -> ">"
      | LessThan -> "<"
      | Dot -> "."
      | Comma -> ","
      | At -> "@"
      | Times -> "*"

  type Token =
    | Keyword of Keyword
    | Operator of Operator
    | Identifier of string
    | StringLiteral of string
    | BoolLiteral of bool

    override this.ToString() =
      match this with
      | Keyword k -> $"`{k.ToString()}`"
      | Operator o -> o.ToString()
      | Identifier id -> id
      | StringLiteral s -> $"\"{s}\""
      | BoolLiteral b -> if b then "true" else "false"

  type LocalizedToken =
    { Token: Token
      Location: Location }

    static member FromKeyword keyword location =
      { Token = keyword |> Token.Keyword
        Location = location }

    static member FromOperator operator location =
      { Token = operator |> Token.Operator
        Location = location }

    static member FromIdentifier identifier location =
      { Token = identifier |> Token.Identifier
        Location = location }

    static member FromStringLiteral literal location =
      { Token = literal |> Token.StringLiteral
        Location = location }

    static member FromBoolLiteral literal location =
      { Token = literal |> Token.BoolLiteral
        Location = location }

  let tokenizer =
    ParserBuilder<Symbol, Location, Errors>(
      {| Step = Location.Step |},
      {| UnexpectedEndOfFile = fun loc -> (loc, $"Unexpected end of file at {loc}") |> Errors.Singleton
         AnyFailed = fun loc -> (loc, "No matching token") |> Errors.Singleton
         NotFailed = fun loc -> (loc, $"Expected token not found at {loc}") |> Errors.Singleton
         UnexpectedSymbol = fun loc c -> (loc, $"Unexpected symbol: {c}") |> Errors.Singleton
         FilterHighestPriorityOnly = Errors.HighestPriority
         Concat = Errors.Concat |}
    )

  let newline = '\n' |> tokenizer.Exactly
  let space = ' ' |> tokenizer.Exactly
  let tab = '\t' |> tokenizer.Exactly
  let cr = '\r' |> tokenizer.Exactly

  let whitespace =
    tokenizer.Any([ space; tab; newline; cr ])
    |> tokenizer.AtLeastOne
    |> tokenizer.Ignore

  let eos = tokenizer.EndOfStream()

  let word (s: string) =
    tokenizer {
      do! tokenizer.All(s |> Seq.toList |> List.map tokenizer.Exactly) |> tokenizer.Ignore

      return! tokenizer.Location
    }

  let keyword =
    tokenizer {
      let! res =
        tokenizer.Any
          [ word (Keyword.Type.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Keyword.Type)
            word (Keyword.Of.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Keyword.Of)
            word (Keyword.In.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Keyword.In)
            word (Keyword.Function.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Keyword.Function)
            word (Keyword.Fun.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Keyword.Fun)
            word (Keyword.Let.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Keyword.Let)
            word (Keyword.Match.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Keyword.Match)
            word (Keyword.With.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Keyword.With)
            word (Keyword.If.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Keyword.If)
            word (Keyword.Then.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Keyword.Then)
            word (Keyword.Else.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Keyword.Else)
            word "true" |> tokenizer.Map(LocalizedToken.FromBoolLiteral true)
            word "false" |> tokenizer.Map(LocalizedToken.FromBoolLiteral false) ]

      do!
        tokenizer.Lookahead(
          tokenizer.Exactly((fun c -> c |> Char.IsLetter || c = '_') >> not)
          |> tokenizer.Ignore
        )

      return res
    }

  let operator =
    tokenizer.Any
      [ word "=" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Equals)
        word "->" |> tokenizer.Map(LocalizedToken.FromOperator Operator.SingleArrow)
        word "=>" |> tokenizer.Map(LocalizedToken.FromOperator Operator.DoubleArrow)
        word "." |> tokenizer.Map(LocalizedToken.FromOperator Operator.Dot)
        word "," |> tokenizer.Map(LocalizedToken.FromOperator Operator.Comma)
        word "@" |> tokenizer.Map(LocalizedToken.FromOperator Operator.At)
        word ">" |> tokenizer.Map(LocalizedToken.FromOperator Operator.GreaterThan)
        word "<" |> tokenizer.Map(LocalizedToken.FromOperator Operator.LessThan)
        word "-" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Minus)
        word "("
        |> tokenizer.Map(LocalizedToken.FromOperator(Operator.RoundBracket Open))
        word ")"
        |> tokenizer.Map(LocalizedToken.FromOperator(Operator.RoundBracket Close))
        word "{"
        |> tokenizer.Map(LocalizedToken.FromOperator(Operator.CurlyBracket Open))
        word "}"
        |> tokenizer.Map(LocalizedToken.FromOperator(Operator.CurlyBracket Close))
        word "["
        |> tokenizer.Map(LocalizedToken.FromOperator(Operator.SquareBracket Open))
        word "]"
        |> tokenizer.Map(LocalizedToken.FromOperator(Operator.SquareBracket Close))
        word "::" |> tokenizer.Map(LocalizedToken.FromOperator Operator.DoubleColon)
        word ":" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Colon)
        word ";" |> tokenizer.Map(LocalizedToken.FromOperator Operator.SemiColon)
        word "||" |> tokenizer.Map(LocalizedToken.FromOperator Operator.DoublePipe)
        word "&&" |> tokenizer.Map(LocalizedToken.FromOperator Operator.DoubleAmpersand)
        word "|" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Pipe)
        word "*" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Times) ]

  let letter = tokenizer.Exactly Char.IsLetter
  let digit = tokenizer.Exactly Char.IsDigit
  let underscore = tokenizer.Exactly '_'

  let identifier =
    tokenizer {
      let! start = [ letter; underscore ] |> tokenizer.Any

      let! rest = [ letter; digit; underscore ] |> tokenizer.Any |> tokenizer.Many

      let id = String.Concat(start :: rest)
      let! loc = tokenizer.Location

      return LocalizedToken.FromIdentifier id loc
    }

  let literal =
    tokenizer {
      do! tokenizer.Exactly '\"' |> tokenizer.Ignore
      let! literal = tokenizer.Many(tokenizer.Exactly(fun c -> c <> '\"'))
      do! tokenizer.Exactly '\"' |> tokenizer.Ignore

      let! loc = tokenizer.Location
      return LocalizedToken.FromStringLiteral (String.Concat(literal)) loc
    }

  let rec token =
    tokenizer {
      do! whitespace |> tokenizer.Try |> tokenizer.Ignore
      let! t = tokenizer.Any [ keyword; operator; identifier; literal ]
      do! tokenizer.Any [ whitespace; eos ] |> tokenizer.Try |> tokenizer.Ignore
      return t
    }

  let rec tokens =
    tokenizer {
      let! res = token |> tokenizer.Many
      do! eos
      return res
    }
