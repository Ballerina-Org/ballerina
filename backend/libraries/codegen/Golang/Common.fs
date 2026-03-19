namespace Codegen.Golang

open System.Text.RegularExpressions

module Common =
  type CleanedUpLiteral =
    private
    | CleanedUpLiteral of string

    static member Value(CleanedUpLiteral literal) = literal

  let allowedStrings = Regex("^[A-Z_][a-zA-Z0-9_]*$")

  let cleanUpLiteral (literal: string) : CleanedUpLiteral * string =
    if allowedStrings.IsMatch literal |> not then
      // Create a cleanedUpLiteral that passes the regex for Go identifiers
      // Must start with uppercase letter or underscore, followed by letters/digits/underscores
      let firstChar =
        if literal.Length > 0 && Regex("^[A-Z_]").IsMatch(literal.Substring(0, 1)) then
          literal.Substring(0, 1)
        else if
          // If it doesn't start with uppercase letter or underscore, use uppercase version if possible
          literal.Length > 0 && Regex("^[a-z]").IsMatch(literal.Substring(0, 1))
        then
          literal.Substring(0, 1).ToUpper()
        else
          "_"

      let rest =
        if literal.Length > 0 then
          literal.Substring(1)
          |> String.collect (fun c ->
            if Regex("[a-zA-Z0-9_]").IsMatch(c.ToString()) then
              c.ToString()
            else
              "_")
        else
          ""

      let cleanedUpLiteral = firstChar + rest

      if cleanedUpLiteral.Length > 1 then
        (CleanedUpLiteral cleanedUpLiteral, literal)
      else
        (CleanedUpLiteral literal, literal)
    else
      (CleanedUpLiteral literal, literal)
