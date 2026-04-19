namespace Ballerina.DSL.Next.Syntax

[<AutoOpen>]
module Lexer =
  open Ballerina.Parser
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open System
  open Ballerina.Collections.NonEmptyList
  open Ballerina
  open Ballerina.Collections.Sum

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
    | Do
    | In
    | Match
    | With
    | If
    | Then
    | Else
    | Schema
    | Entity
    | Relation
    | Include
    | Query
    | From
    | Select
    | Where
    | Join
    | On
    | Can
    | And
    | OrderBy
    | Distinct
    | Count
    | Exists
    | Union
    | Array
    | Ascending
    | Descending

    override this.ToString() =
      match this with
      | Type -> "type"
      | Of -> "of"
      | Function -> "function"
      | Fun -> "fun"
      | Let -> "let"
      | Do -> "do"
      | In -> "in"
      | Match -> "match"
      | With -> "with"
      | If -> "if"
      | Then -> "then"
      | Else -> "else"
      | Schema -> "schema"
      | Entity -> "entity"
      | Relation -> "relation"
      | Include -> "include"
      | Query -> "query"
      | From -> "from"
      | Select -> "select"
      | Where -> "where"
      | Join -> "join"
      | On -> "on"
      | Can -> "can"
      | And -> "and"
      | OrderBy -> "orderby"
      | Distinct -> "distinct"
      | Count -> "count"
      | Exists -> "exists"
      | Union -> "union"
      | Array -> "array"
      | Ascending -> "asc"
      | Descending -> "desc"

  type Operator =
    | Equals
    | CurlyBracket of Bracket
    | SquareBracket of Bracket
    | RoundBracket of Bracket
    | DoubleColon
    | Colon
    | PipeGreaterThan
    | DoubleGreaterThan
    | SemiColon
    | DoublePipe
    | DoubleAmpersand
    | Pipe
    | SingleArrow
    | DoubleArrow
    | Minus
    | Equal
    | NotEqual
    | GreaterEqual
    | GreaterThan
    | LessThanOrEqual
    | LessThan
    | DoubleDot
    | Dot
    | Comma
    | At
    | Times
    | Plus
    | Div
    | Percentage
    | Bang

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
      | PipeGreaterThan -> "|>"
      | DoubleGreaterThan -> ">>"
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
      | DoubleDot -> ".."
      | Dot -> "."
      | Comma -> ","
      | At -> "@"
      | Times -> "*"
      | Plus -> "+"
      | Div -> "/"
      | Bang -> "!"
      | Equal -> "=="
      | NotEqual -> "!="
      | GreaterEqual -> ">="
      | LessThanOrEqual -> "<="
      | Percentage -> "%"

  let floatSuffixString = 'f'
  let doubleSuffixString = 'd'
  let longSuffixString = 'l'

  type Token =
    | Keyword of Keyword
    | Operator of Operator
    | Comment of string
    | Identifier of string
    | CaseLiteral of int * int
    | StringLiteral of string
    | BoolLiteral of bool
    | IntLiteral of int
    | Int64Literal of int64
    | DecimalLiteral of System.Decimal
    | Float32Literal of float32
    | Float64Literal of float

    override this.ToString() =
      match this with
      | Keyword k -> $"`{k.ToString()}`"
      | Operator o -> o.ToString()
      | Identifier id -> id
      | Comment s -> $"// {s}"
      | StringLiteral s -> $"\"{s}\""
      | CaseLiteral(i, n) -> $"{i}Of{n}"
      | BoolLiteral b -> if b then "true" else "false"
      | IntLiteral i -> i.ToString()
      | Int64Literal i -> $"{i.ToString()}" + longSuffixString.ToString()
      | Float32Literal f -> $"{f.ToString()}" + floatSuffixString.ToString()
      | Float64Literal f -> $"{f.ToString()}" + doubleSuffixString.ToString()
      | DecimalLiteral d -> d.ToString()

  type LocalizedToken =
    { Token: Token
      Location: Location }

    override this.ToString() = this.Token.ToString()

    static member FromKeyword keyword location =
      { Token = keyword |> Token.Keyword
        Location = location }

    static member FromOperator operator location =
      { Token = operator |> Token.Operator
        Location = location }

    static member FromComment comment location =
      { Token = comment |> Token.Comment
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

    static member FromIntLiteral literal location =
      { Token = literal |> Token.IntLiteral
        Location = location }

    static member FromInt64Literal literal location =
      { Token = literal |> Token.Int64Literal
        Location = location }

    static member FromCaseLiteral literal location =
      { Token = literal |> Token.CaseLiteral
        Location = location }

    static member FromDecimalLiteral literal location =
      { Token = literal |> Token.DecimalLiteral
        Location = location }

    static member FromFloat32Literal literal location =
      { Token = literal |> Token.Float32Literal
        Location = location }

    static member FromFloat64Literal literal location =
      { Token = literal |> Token.Float64Literal
        Location = location }


  let tokenizer =
    ParserBuilder<Symbol, Location, Errors<Location>>(
      {| Step = Location.Step |},
      {| UnexpectedEndOfFile =
          fun loc ->
            (fun () -> $"Unexpected end of file at {loc}")
            |> Errors.Singleton loc
         AnyFailed =
          fun loc -> (fun () -> "No matching token") |> Errors.Singleton loc
         NotFailed =
          fun loc ->
            (fun () -> $"Expected token not found at {loc}")
            |> Errors.Singleton loc
         UnexpectedSymbol =
          fun loc c ->
            (fun () -> $"Unexpected symbol: {c}") |> Errors.Singleton loc
         FilterHighestPriorityOnly = Errors<Location>.FilterHighestPriorityOnly
         Concat = Errors.Concat<Location> |}
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

  let dot = tokenizer.Exactly '.'

  let word (s: string) =
    tokenizer {
      do!
        tokenizer.All(s |> Seq.toList |> List.map tokenizer.Exactly)
        |> tokenizer.Ignore

      return! tokenizer.Location
    }

  let private keywords =
    [ Keyword.Include.ToString(), LocalizedToken.FromKeyword Keyword.Include
      Keyword.In.ToString(), LocalizedToken.FromKeyword Keyword.In
      Keyword.Type.ToString(), LocalizedToken.FromKeyword Keyword.Type
      Keyword.Of.ToString(), LocalizedToken.FromKeyword Keyword.Of
      Keyword.Function.ToString(), LocalizedToken.FromKeyword Keyword.Function
      Keyword.Fun.ToString(), LocalizedToken.FromKeyword Keyword.Fun
      Keyword.Let.ToString(), LocalizedToken.FromKeyword Keyword.Let
      Keyword.Do.ToString(), LocalizedToken.FromKeyword Keyword.Do
      Keyword.Match.ToString(), LocalizedToken.FromKeyword Keyword.Match
      Keyword.With.ToString(), LocalizedToken.FromKeyword Keyword.With
      Keyword.If.ToString(), LocalizedToken.FromKeyword Keyword.If
      Keyword.And.ToString(), LocalizedToken.FromKeyword Keyword.And
      Keyword.Then.ToString(), LocalizedToken.FromKeyword Keyword.Then
      Keyword.Else.ToString(), LocalizedToken.FromKeyword Keyword.Else
      Keyword.Schema.ToString(), LocalizedToken.FromKeyword Keyword.Schema
      Keyword.Entity.ToString(), LocalizedToken.FromKeyword Keyword.Entity
      Keyword.Relation.ToString(), LocalizedToken.FromKeyword Keyword.Relation
      Keyword.Query.ToString(), LocalizedToken.FromKeyword Keyword.Query
      Keyword.From.ToString(), LocalizedToken.FromKeyword Keyword.From
      Keyword.Select.ToString(), LocalizedToken.FromKeyword Keyword.Select
      Keyword.Where.ToString(), LocalizedToken.FromKeyword Keyword.Where
      Keyword.Join.ToString(), LocalizedToken.FromKeyword Keyword.Join
      Keyword.On.ToString(), LocalizedToken.FromKeyword Keyword.On
      Keyword.Can.ToString(), LocalizedToken.FromKeyword Keyword.Can
      Keyword.OrderBy.ToString(), LocalizedToken.FromKeyword Keyword.OrderBy
      Keyword.Distinct.ToString(), LocalizedToken.FromKeyword Keyword.Distinct
      Keyword.Count.ToString(), LocalizedToken.FromKeyword Keyword.Count
      Keyword.Exists.ToString(), LocalizedToken.FromKeyword Keyword.Exists
      Keyword.Union.ToString(), LocalizedToken.FromKeyword Keyword.Union
      Keyword.Array.ToString(), LocalizedToken.FromKeyword Keyword.Array
      Keyword.Ascending.ToString(), LocalizedToken.FromKeyword Keyword.Ascending
      Keyword.Descending.ToString(),
      LocalizedToken.FromKeyword Keyword.Descending
      "true", LocalizedToken.FromBoolLiteral true
      "false", LocalizedToken.FromBoolLiteral false ]
    |> List.sortByDescending (fun (w, _) -> w.Length)

  let keyword =
    tokenizer {
      let! res =
        keywords
        |> List.map (fun (w, t) -> w |> word |> tokenizer.Map t)
        |> tokenizer.Any

      do!
        tokenizer.Lookahead(
          tokenizer.Exactly((fun c -> c |> Char.IsLetter || c = '_') >> not)
          |> tokenizer.Ignore
        )

      return res
    }

  let operator =
    tokenizer.Any
      [ word "->"
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.SingleArrow)
        word "=>"
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.DoubleArrow)
        word ".."
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.DoubleDot)
        word "." |> tokenizer.Map(LocalizedToken.FromOperator Operator.Dot)
        word "," |> tokenizer.Map(LocalizedToken.FromOperator Operator.Comma)
        word "@" |> tokenizer.Map(LocalizedToken.FromOperator Operator.At)
        word "==" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Equal)
        word "=" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Equals)
        word "!="
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.NotEqual)
        word "|>"
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.PipeGreaterThan)
        word ">>"
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.DoubleGreaterThan)
        word ">="
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.GreaterEqual)
        word "<="
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.LessThanOrEqual)
        word ">"
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.GreaterThan)
        word "<" |> tokenizer.Map(LocalizedToken.FromOperator Operator.LessThan)
        word "-" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Minus)
        word "("
        |> tokenizer.Map(
          LocalizedToken.FromOperator(Operator.RoundBracket Open)
        )
        word ")"
        |> tokenizer.Map(
          LocalizedToken.FromOperator(Operator.RoundBracket Close)
        )
        word "{"
        |> tokenizer.Map(
          LocalizedToken.FromOperator(Operator.CurlyBracket Open)
        )
        word "}"
        |> tokenizer.Map(
          LocalizedToken.FromOperator(Operator.CurlyBracket Close)
        )
        word "["
        |> tokenizer.Map(
          LocalizedToken.FromOperator(Operator.SquareBracket Open)
        )
        word "]"
        |> tokenizer.Map(
          LocalizedToken.FromOperator(Operator.SquareBracket Close)
        )
        word "::"
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.DoubleColon)
        word ":" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Colon)
        word ";"
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.SemiColon)
        word "||"
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.DoublePipe)
        word "&&"
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.DoubleAmpersand)
        word "|" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Pipe)
        word "*" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Times)
        word "+" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Plus)
        word "/" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Div)
        word "!" |> tokenizer.Map(LocalizedToken.FromOperator Operator.Bang)
        word "%"
        |> tokenizer.Map(LocalizedToken.FromOperator Operator.Percentage) ]

  let letter = tokenizer.Exactly Char.IsLetter
  let digit = tokenizer.Exactly Char.IsDigit
  let underscore = tokenizer.Exactly '_'
  let tick = tokenizer.Exactly '\''

  let identifier =
    tokenizer {
      let! start = [ letter; underscore; tick ] |> tokenizer.Any

      let! rest =
        [ letter; digit; underscore; tick ] |> tokenizer.Any |> tokenizer.Many

      let id = String.Concat(start :: rest)
      let! loc = tokenizer.Location

      return LocalizedToken.FromIdentifier id loc
    }

  let comment =
    tokenizer {
      do! word "//" |> tokenizer.Ignore
      let! comment = tokenizer.Many(tokenizer.Exactly(fun c -> c <> '\n'))

      let! loc = tokenizer.Location
      return LocalizedToken.FromComment (String.Concat(comment)) loc
    }

  let stringLiteral =
    tokenizer {
      do! tokenizer.Exactly '\"' |> tokenizer.Ignore

      let stringChar =
        tokenizer.Any
          [ tokenizer {
              do! tokenizer.Exactly '\\' |> tokenizer.Ignore

              let! escaped = tokenizer.Exactly(fun _ -> true)

              return
                match escaped with
                | 'n' -> '\n'
                | 't' -> '\t'
                | 'r' -> '\r'
                | '\\' -> '\\'
                | '"' -> '"'
                | '0' -> '\000'
                | c -> c
            }
            tokenizer.Exactly(fun c -> c <> '\"' && c <> '\n' && c <> '\\') ]

      let! literal = stringChar |> tokenizer.Many

      do! tokenizer.Exactly '\"' |> tokenizer.Ignore

      do!
        tokenizer.Lookahead(
          tokenizer.Exactly(
            (fun c -> c |> Char.IsLetter || c = '_' || c = '\'') >> not
          )
          |> tokenizer.Ignore
        )

      let! loc = tokenizer.Location
      return LocalizedToken.FromStringLiteral (String.Concat(literal)) loc
    }

  let numberOrCaseLiteral =
    tokenizer {
      let! minus = tokenizer.Exactly '-' |> tokenizer.Try
      let minus = minus.IsLeft

      let! int_part =
        digit |> tokenizer.AtLeastOne |> tokenizer.Map NonEmptyList.ToList

      let! frac_part =
        tokenizer {
          do! dot |> tokenizer.Ignore
          let! digits = digit |> tokenizer.AtLeastOne

          do!
            tokenizer.Any [ dot |> tokenizer.Ignore ]
            |> tokenizer.Not
            |> tokenizer.Lookahead
            |> tokenizer.Ignore

          return digits
        }
        |> tokenizer.Try
        |> tokenizer.Map Sum.toOption

      let! loc = tokenizer.Location

      match frac_part with
      | Some frac ->
        let literal = String.Concat(int_part) + "." + String.Concat(frac)

        let! floatSuffix =
          floatSuffixString |> tokenizer.Exactly |> tokenizer.Try

        let isFloat = floatSuffix.IsLeft
        let mutable floatValue = 0f

        if isFloat then
          if not (System.Single.TryParse(literal, &floatValue)) then
            return!
              (fun () -> $"Cannot parse float literal {literal} at {loc}")
              |> Errors.Singleton loc
              |> tokenizer.Throw
          else
            let value = if minus then -floatValue else floatValue
            return LocalizedToken.FromFloat32Literal value loc
        else
          let! doubleSuffix =
            doubleSuffixString |> tokenizer.Exactly |> tokenizer.Try

          let isDouble = doubleSuffix.IsLeft
          let mutable doubleValue = 0.0

          if isDouble then
            if not (System.Double.TryParse(literal, &doubleValue)) then
              return!
                (fun () -> $"Cannot parse double literal {literal} at {loc}")
                |> Errors.Singleton loc
                |> tokenizer.Throw
            else
              let value = if minus then -doubleValue else doubleValue
              return LocalizedToken.FromFloat64Literal value loc
          else
            let mutable decimalValue = 0m

            if not (System.Decimal.TryParse(literal, &decimalValue)) then
              return!
                (fun () -> $"Cannot parse decimal literal {literal} at {loc}")
                |> Errors.Singleton loc
                |> tokenizer.Throw
            else
              let value = if minus then -decimalValue else decimalValue
              return LocalizedToken.FromDecimalLiteral value loc
      | None ->
        let longLiteral = String.Concat(int_part)
        let mutable longValue = 0L

        let! longSuffix = longSuffixString |> tokenizer.Exactly |> tokenizer.Try
        let isLong = longSuffix.IsLeft

        let literal = String.Concat(int_part)
        let mutable value = 0

        if isLong then
          if not (System.Int64.TryParse(longLiteral, &longValue)) then
            do
              Console.WriteLine
                $"Cannot parse int64 literal {longLiteral} at {loc}"

            return!
              (fun () -> $"Cannot parse int64 literal {longLiteral} at {loc}")
              |> Errors.Singleton loc
              |> tokenizer.Throw
          else
            let value = if minus then -longValue else longValue
            return LocalizedToken.FromInt64Literal value loc
        else if not (System.Int32.TryParse(literal, &value)) then
          return!
            (fun () -> $"Cannot parse int literal {literal} at {loc}")
            |> Errors.Singleton loc
            |> tokenizer.Throw
        else
          let! ofTotal =
            tokenizer {
              do! [ word "Of"; word "of" ] |> tokenizer.Any |> tokenizer.Ignore
              return! digit |> tokenizer.Many

            }
            |> tokenizer.Try
            |> tokenizer.Map Sum.toOption

          match ofTotal with
          | None ->
            let value = if minus then -value else value
            return LocalizedToken.FromIntLiteral value loc
          | Some ofTotal ->
            let ofTotal = String.Concat(ofTotal)
            let mutable total = 0

            if not (System.Int32.TryParse(ofTotal, &total)) then
              return!
                (fun () ->
                  $"Cannot parse case literal total {ofTotal} at {loc}")
                |> Errors.Singleton loc
                |> tokenizer.Throw
            else
              return LocalizedToken.FromCaseLiteral (value, total) loc
    }

  let tupleItem =
    tokenizer {
      do! dot |> tokenizer.Ignore
      let! digits = digit |> tokenizer.AtLeastOne
      let literal = String.Concat(digits)
      let mutable value = 0
      let! loc = tokenizer.Location

      if not (System.Int32.TryParse(literal, &value)) then
        return!
          (fun () -> $"Cannot parse tuple item literal {literal} at {loc}")
          |> Errors.Singleton loc
          |> tokenizer.Throw
      else
        return
          [ LocalizedToken.FromOperator Operator.Dot loc
            LocalizedToken.FromIntLiteral value loc ]
    }

  let rec token =
    tokenizer {
      do! whitespace |> tokenizer.Try |> tokenizer.Ignore

      let cons x = [ x ]

      let! t =
        tokenizer.Any
          [ keyword |> tokenizer.Map cons
            comment |> tokenizer.Map cons
            stringLiteral |> tokenizer.Map cons
            tupleItem
            numberOrCaseLiteral |> tokenizer.Map cons
            operator |> tokenizer.Map cons
            identifier |> tokenizer.Map cons ]

      do! tokenizer.Any [ whitespace; eos ] |> tokenizer.Try |> tokenizer.Ignore
      return t
    }

  let rec tokens =
    tokenizer {
      let! res = token |> tokenizer.Many
      let res = res |> List.concat

      let res =
        res
        |> List.filter (function
          | { Token = Comment _ } -> false
          | _ -> true)

      do! eos
      return res
    }
