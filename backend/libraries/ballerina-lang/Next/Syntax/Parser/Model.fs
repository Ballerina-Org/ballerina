namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Model =

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

  let parser =
    ParserBuilder<LocalizedToken, Location, Errors>(
      {| Step = fun lt _ -> lt.Location |},
      {| UnexpectedEndOfFile = fun loc -> (loc, $"Unexpected end of file at {loc}") |> Errors.Singleton
         AnyFailed = fun loc -> (loc, "No matching token") |> Errors.Singleton
         NotFailed = fun loc -> (loc, $"Expected token not found at {loc}") |> Errors.Singleton
         UnexpectedSymbol = fun loc c -> (loc, $"Unexpected symbol: {c}") |> Errors.Singleton
         FilterHighestPriorityOnly = Errors.FilterHighestPriorityOnly
         Concat = Errors.Concat |}
    )
