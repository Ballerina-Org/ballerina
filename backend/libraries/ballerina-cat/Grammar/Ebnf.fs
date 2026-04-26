namespace Ballerina.Grammar

module Ebnf =
  let private escapeTerminal (s: string) : string =
    s.Replace("\\", "\\\\").Replace("\"", "\\\"")

  let rec private serializeRule (rule: GrammarRule) : string =
    match rule with
    | Terminal s -> $"\"{escapeTerminal s}\""
    | NonTerminal s -> s
    | Seq rules ->
      rules
      |> List.map serializeRuleGrouped
      |> String.concat " , "
    | Alt rules ->
      rules
      |> List.map serializeRule
      |> String.concat " | "
    | Repeat rule -> "{ " + serializeRule rule + " }"
    | Repeat1 rule -> serializeRule rule + " , { " + serializeRule rule + " }"
    | Optional rule -> "[ " + serializeRule rule + " ]"
    | Empty -> ""

  and private serializeRuleGrouped (rule: GrammarRule) : string =
    match rule with
    | Alt _ -> "( " + serializeRule rule + " )"
    | _ -> serializeRule rule

  let serialize (rules: NamedRule list) : string =
    rules
    |> List.map (fun r -> $"{r.Name} = {serializeRule r.Rule} ;")
    |> String.concat "\n"
