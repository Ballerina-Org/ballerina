namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Kind =

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

  let rec kindDecl () =
    parser {
      let! first =
        parser.Any
          [ (starOperator |> parser.Map(fun _ -> Kind.Star))
            (parser {
              do! openRoundBracketOperator
              let! res = kindDecl ()
              do! closeRoundBracketOperator
              return res
            }) ]

      let! singleArrow = singleArrowOperator |> parser.Try

      match singleArrow with
      | Right _ -> return Kind.Star
      | _ ->
        let! rest = kindDecl ()
        return Kind.Arrow(first, rest)
    }
