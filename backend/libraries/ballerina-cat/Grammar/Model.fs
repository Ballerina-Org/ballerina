namespace Ballerina.Grammar

[<AutoOpen>]
module Model =

  type GrammarRule =
    | Terminal of string
    | NonTerminal of string
    | Seq of GrammarRule list
    | Alt of GrammarRule list
    | Repeat of GrammarRule
    | Repeat1 of GrammarRule
    | Optional of GrammarRule
    | Empty

  type NamedRule = { Name: string; Rule: GrammarRule }
