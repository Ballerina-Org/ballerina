namespace Ballerina.DSL.Next.Syntax.Parser

[<AutoOpen>]
module Model =

  open System
  open Ballerina.Collections.Option
  open Ballerina.Parser
  open Ballerina.Collections.NonEmptyList
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Syntax

  let parser =
    ParserBuilder<LocalizedToken, Location, Errors<Location>>(
      {| Step = fun lt _ -> lt.Location |},
      {| UnexpectedEndOfFile = fun loc -> (fun () -> $"Unexpected end of file at {loc}") |> Errors.Singleton loc
         AnyFailed = fun loc -> (fun () -> "No matching token") |> Errors.Singleton loc
         NotFailed = fun loc -> (fun () -> $"Expected token not found at {loc}") |> Errors.Singleton loc
         UnexpectedSymbol = fun loc c -> (fun () -> $"Unexpected symbol: {c}") |> Errors.Singleton loc
         FilterHighestPriorityOnly = Errors<Location>.FilterHighestPriorityOnly
         Concat = Errors.Concat<Location> |}
    )
