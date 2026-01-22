namespace Ballerina.DSL.FormBuilder.Syntax

module Lexer =
  open System
  open Ballerina.DSL.Next.Syntax.Lexer
  open Ballerina.LocalizedErrors

  type Symbol = char

  type Bracket =
    | Open
    | Close


  type Keyword =
    | View
    | From
    | As
    | String
    | Int32
    | Int64
    | Float32
    | Float64
    | Date
    | DateOnly
    | StringId
    | Guid
    | Bool
    | Base64
    | Secret
    | Unit
    | Table
    | Tab
    | Column
    | Group
    | With
    | In
    | Enum
    | Single
    | Multi
    | Renderer
    | Disable
    | Tuple
    | Stream
    | List
    | Union
    | Option
    | Record
    | Tooltip
    | Create
    | Edit
    | Passthrough
    | Launcher
    | Details
    | Map
    | ReadOnly
    | Sum
    | Highlights
    | EntryPoint
    | One
    | Many
    | Preview
    | Linked
    | Unlinked
    | Element

    override this.ToString() : string =
      match this with
      | View -> "view"
      | From -> "from"
      | As -> "as"
      | String -> "string"
      | Int32 -> "int32"
      | Int64 -> "int64"
      | Float32 -> "float32"
      | Float64 -> "float64"
      | Date -> "date"
      | DateOnly -> "dateonly"
      | StringId -> "stringid"
      | Guid -> "guid"
      | Bool -> "bool"
      | Base64 -> "base64"
      | Secret -> "secret"
      | Unit -> "unit"
      | Table -> "table"
      | Tab -> "tab"
      | Column -> "column"
      | Group -> "group"
      | With -> "with"
      | In -> "in"
      | Enum -> "enum"
      | Single -> "single"
      | Multi -> "multi"
      | Renderer -> "renderer"
      | Disable -> "disable"
      | Tuple -> "tuple"
      | Stream -> "stream"
      | List -> "list"
      | Union -> "union"
      | Option -> "option"
      | Record -> "record"
      | Tooltip -> "tooltip"
      | Create -> "create"
      | Edit -> "edit"
      | Passthrough -> "passthrough"
      | Launcher -> "launcher"
      | Details -> "details"
      | Map -> "map"
      | ReadOnly -> "readonly"
      | Sum -> "sum"
      | Highlights -> "highlights"
      | EntryPoint -> "entrypoint"
      | One -> "one"
      | Many -> "many"
      | Preview -> "preview"
      | Linked -> "linked"
      | Unlinked -> "unlinked"
      | Element -> "element"

  type Operator =
    | Colon
    | Comma
    | CurlyBracket of Bracket
    | SquareBracket of Bracket
    | RoundBracket of Bracket
    | Equals
    | Pipe
    | Semicolon
    | Arrow
    | Choice1
    | Choice2
    | Dot

    override this.ToString() : string =
      match this with
      | Colon -> ":"
      | Comma -> ","
      | CurlyBracket Open -> "{"
      | CurlyBracket Close -> "}"
      | SquareBracket Open -> "["
      | SquareBracket Close -> "]"
      | RoundBracket Open -> "("
      | RoundBracket Close -> ")"
      | Equals -> "="
      | Pipe -> "|"
      | Semicolon -> ";"
      | Arrow -> "->"
      | Choice1 -> "1of2"
      | Choice2 -> "2of2"
      | Dot -> "."

  type Token =
    | Keyword of Keyword
    | Operator of Operator
    | Comment of string
    | Identifier of string
    | StringLiteral of string

    override this.ToString() : string =
      match this with
      | Keyword k -> $"`{k.ToString()}`"
      | Operator o -> o.ToString()
      | Identifier id -> id
      | Comment s -> $"// {s}"
      | StringLiteral s -> $"\"{s}\""

  type LocalizedToken =
    { Token: Token
      Location: Location }

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

  let keyword =
    tokenizer {
      let! res =
        tokenizer.Any
          [ word (View.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword View)
            word (From.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword From)
            word (As.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword As)
            word (Keyword.StringId.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword StringId)
            word (Keyword.String.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword String)
            word (Keyword.Int32.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Int32)
            word (Keyword.Int64.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Int64)
            word (Keyword.Float32.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Float32)
            word (Keyword.Float64.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Float64)
            word (Keyword.Date.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Date)
            word (Keyword.DateOnly.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword DateOnly)
            word (Keyword.Guid.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Guid)
            word (Keyword.Bool.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Bool)
            word (Keyword.Base64.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Base64)
            word (Keyword.Secret.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Secret)
            word (Keyword.Unit.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Unit)
            word (Keyword.Table.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Table)
            word (Tab.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Tab)
            word (Column.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Column)
            word (Group.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Group)
            word (With.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword With)
            word (In.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword In)
            word (Keyword.Enum.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Enum)
            word (Keyword.Single.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Single)
            word (Multi.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Multi)
            word (Renderer.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Renderer)
            word (Disable.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Disable)
            word (Keyword.Tuple.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Tuple)
            word (Stream.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Stream)
            word (Keyword.List.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword List)
            word (Union.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Union)
            word (Keyword.Option.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Option)
            word (Record.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Record)
            word (Sum.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Sum)
            word (Tooltip.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Tooltip)
            word (Create.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Create)
            word (Edit.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Edit)
            word (Passthrough.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Passthrough)
            word (Launcher.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Launcher)
            word (Details.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Details)
            word (Keyword.Map.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Map)
            word (ReadOnly.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword ReadOnly)
            word (Highlights.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword Highlights)
            word (EntryPoint.ToString())
            |> tokenizer.Map(LocalizedToken.FromKeyword EntryPoint)
            word (One.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword One)
            word (Many.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Many)
            word (Preview.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Preview)
            word (Linked.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Linked)
            word (Unlinked.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Unlinked)
            word (Element.ToString()) |> tokenizer.Map(LocalizedToken.FromKeyword Element) ]

      do!
        tokenizer.Lookahead(
          tokenizer.Exactly((fun c -> c |> Char.IsLetter || c = '_') >> not)
          |> tokenizer.Ignore
        )

      return res
    }

  let operator =
    tokenizer.Any
      [ word ":" |> tokenizer.Map(LocalizedToken.FromOperator Colon)
        word "," |> tokenizer.Map(LocalizedToken.FromOperator Comma)
        word "{" |> tokenizer.Map(LocalizedToken.FromOperator(CurlyBracket Open))
        word "}" |> tokenizer.Map(LocalizedToken.FromOperator(CurlyBracket Close))
        word "[" |> tokenizer.Map(LocalizedToken.FromOperator(SquareBracket Open))
        word "]" |> tokenizer.Map(LocalizedToken.FromOperator(SquareBracket Close))
        word "(" |> tokenizer.Map(LocalizedToken.FromOperator(RoundBracket Open))
        word ")" |> tokenizer.Map(LocalizedToken.FromOperator(RoundBracket Close))
        word "=" |> tokenizer.Map(LocalizedToken.FromOperator Equals)
        word "|" |> tokenizer.Map(LocalizedToken.FromOperator Pipe)
        word ";" |> tokenizer.Map(LocalizedToken.FromOperator Semicolon)
        word (Arrow.ToString()) |> tokenizer.Map(LocalizedToken.FromOperator Arrow)
        word (Choice1.ToString()) |> tokenizer.Map(LocalizedToken.FromOperator Choice1)
        word (Choice2.ToString()) |> tokenizer.Map(LocalizedToken.FromOperator Choice2)
        word (Dot.ToString()) |> tokenizer.Map(LocalizedToken.FromOperator Dot) ]

  let identifier =
    tokenizer {
      let! start = [ letter; underscore ] |> tokenizer.Any

      let! rest = [ letter; digit; underscore ] |> tokenizer.Any |> tokenizer.Many

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
      let! literal = tokenizer.Many(tokenizer.Exactly(fun c -> c <> '\"'))
      do! tokenizer.Exactly '\"' |> tokenizer.Ignore

      let! loc = tokenizer.Location
      return LocalizedToken.FromStringLiteral (String.Concat(literal)) loc
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
