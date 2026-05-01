namespace Ballerina.Grammar

module Gbnf =
  let private sanitizeName (s: string) : string =
    s.Replace(" ", "-").Replace("::", "-").Replace(".", "-")

  let private escapeTerminal (s: string) : string =
    s.Replace("\\", "\\\\").Replace("\"", "\\\"")

  let rec private serializeRule (rule: GrammarRule) : string =
    match rule with
    | Terminal s -> $"\"{escapeTerminal s}\""
    | NonTerminal s -> sanitizeName s
    | Seq rules ->
      rules
      |> List.map serializeRuleGrouped
      |> String.concat " ows "
    | Alt rules ->
      rules
      |> List.map serializeRule
      |> String.concat " | "
    | Repeat rule -> "(" + serializeRule rule + ")*"
    | Repeat1 rule -> "(" + serializeRule rule + ")+"
    | Optional rule -> "(" + serializeRule rule + ")?"
    | Empty -> "\"\""

  and private serializeRuleGrouped (rule: GrammarRule) : string =
    match rule with
    | Alt _ -> "(" + serializeRule rule + ")"
    | _ -> serializeRule rule

  let private preamble =
    [ "# Whitespace rules (auto-generated)"
      "ows ::= [ \\t\\n\\r]? [ \\t\\n\\r]? [ \\t\\n\\r]? [ \\t\\n\\r]?"
      "ws ::= [ \\t\\n\\r] [ \\t\\n\\r]? [ \\t\\n\\r]? [ \\t\\n\\r]?" ]

  let serialize (rules: NamedRule list) : string =
    let body =
      rules
      |> List.map (fun r -> $"{sanitizeName r.Name} ::= {serializeRule r.Rule}")

    (preamble @ body) |> String.concat "\n"
