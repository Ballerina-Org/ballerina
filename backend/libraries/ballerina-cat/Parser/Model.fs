namespace Ballerina.Parser

[<AutoOpen>]
module Model =
  open Ballerina.Fun
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Collections
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Reader.WithError
  open Ballerina.Stackless.State.WithError.StacklessStateWithError


  open FSharp.Core
  open Ballerina.Collections.NonEmptyList
  open System

  type ParserResult<'a, 'sym, 'loc, 'err> =
    | ParserResult of 'a * Option<List<'sym> * 'loc>

    static member Return(v: 'a) : ParserResult<'a, 'sym, 'loc, 'err> = ParserResult(v, None)
    static member FromState(v: 'a, s: List<'sym> * 'loc) : ParserResult<'a, 'sym, 'loc, 'err> = ParserResult(v, Some s)

    static member Map
      (f: 'a -> 'b)
      (ParserResult(a, rest): ParserResult<'a, 'sym, 'loc, 'err>)
      : ParserResult<'b, 'sym, 'loc, 'err> =
      ParserResult(f a, rest)


  type Parser<'a, 'sym, 'loc, 'err> =
    | Parser of FreeNode<'a, unit, List<'sym> * 'loc, 'err>

    static member Run (input: List<'sym>, loc: 'loc) (Parser p: Parser<'a, 'sym, 'loc, 'err>) =
      match FreeNode.run () (input, loc) p with
      | Left(v, s1) -> Left(ParserResult(v, s1))
      | Right(e, s1) -> Right(e, s1)

    static member Throw(e: 'err) : Parser<'a, 'sym, 'loc, 'err> = Parser(FreeNode.throw e)

    static member Return(v: 'a) : Parser<'a, 'sym, 'loc, 'err> = Parser(FreeNode.Return v)

    static member Map (f: 'a -> 'b) (Parser p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'b, 'sym, 'loc, 'err> =
      Parser(FreeNode.bind p (fun a -> FreeNode.Return(f a)))

    static member MapError (f: 'err -> 'err) (Parser p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'a, 'sym, 'loc, 'err> =
      let mapped =
        FreeNode.bind (FreeNode.catch p) (fun res ->
          match res with
          | Left v -> FreeNode.Return v
          | Right e -> FreeNode.throw (f e))

      Parser mapped

    static member Flatten(p0: Parser<Parser<'a, 'sym, 'loc, 'err>, 'sym, 'loc, 'err>) : Parser<'a, 'sym, 'loc, 'err> =
      let (Parser p0) = p0
      Parser(FreeNode.bind p0 (fun (Parser p1) -> p1))

  let fromStep
    (f: List<'sym> * 'loc -> Sum<'a * Option<List<'sym> * 'loc>, 'err * Option<List<'sym> * 'loc>>)
    : Parser<'a, 'sym, 'loc, 'err> =
    Parser(FreeNode.fromStep (fun (_c, s) -> f s))

  type ParserBuilder<'sym, 'loc, 'err when 'sym: equality>
    (
      loc: {| Step: 'sym -> Updater<'loc> |},
      err:
        {| UnexpectedEndOfFile: 'loc -> 'err
           NotFailed: 'loc -> 'err
           AnyFailed: 'loc -> 'err
           UnexpectedSymbol: 'loc -> 'sym -> 'err
           FilterHighestPriorityOnly: 'err -> 'err
           Concat: 'err * 'err -> 'err |}
    ) =
    member _.Throw(e: 'err) : Parser<'a, 'sym, 'loc, 'err> = Parser.Throw e
    member _.Return(v: 'a) : Parser<'a, 'sym, 'loc, 'err> = Parser.Return v

    member _.EndOfStream() : Parser<Unit, 'sym, 'loc, 'err> =
      fromStep (fun (input: List<'sym>, loc0: 'loc) ->
        match input with
        | [] -> Left((), Some([], loc0))
        | x :: _ -> Right(err.UnexpectedSymbol loc0 x, None))

    member _.Bind
      (p: Parser<'a, 'sym, 'loc, 'err>, f: 'a -> Parser<'b, 'sym, 'loc, 'err>)
      : Parser<'b, 'sym, 'loc, 'err> =
      Parser.Flatten(Parser.Map f p)

    member _.Zero() : Parser<unit, 'sym, 'loc, 'err> = Parser.Return()

    member parser.Combine
      (p1: Parser<unit, 'sym, 'loc, 'err>, p2: Parser<'a, 'sym, 'loc, 'err>)
      : Parser<'a, 'sym, 'loc, 'err> =
      parser.Bind(p1, fun () -> p2)

    member _.Delay(f: unit -> Parser<'a, 'sym, 'loc, 'err>) : Parser<'a, 'sym, 'loc, 'err> = f ()

    member _.ReturnFrom(p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'a, 'sym, 'loc, 'err> = p
    member _.Run(p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'a, 'sym, 'loc, 'err> = p

    member _.Exactly(expected: 'sym) : Parser<'sym, 'sym, 'loc, 'err> =
      fromStep (fun (input: List<'sym>, loc0: 'loc) ->
        match input with
        | [] -> (err.UnexpectedEndOfFile loc0, None) |> Right
        | x :: xs ->
          if x = expected then
            let loc1 = loc0 |> loc.Step x
            Left(x, Some(xs, loc1))
          else
            (err.UnexpectedSymbol loc0 x, None) |> Right)

    member _.Exactly(predicate: 'sym -> bool) : Parser<'sym, 'sym, 'loc, 'err> =
      fromStep (fun (input: List<'sym>, loc0: 'loc) ->
        match input with
        | [] -> (err.UnexpectedEndOfFile loc0, None) |> Right
        | x :: xs ->
          if predicate x then
            let loc1 = loc0 |> loc.Step x
            Left(x, Some(xs, loc1))
          else
            (err.UnexpectedSymbol loc0 x, None) |> Right)

    member _.Exactly(predicate: 'sym -> Option<'a>) : Parser<'a, 'sym, 'loc, 'err> =
      fromStep (fun (input: List<'sym>, loc0: 'loc) ->
        match input with
        | [] -> (err.UnexpectedEndOfFile loc0, None) |> Right
        | x :: xs ->
          match predicate x with
          | Some res ->
            let loc1 = loc0 |> loc.Step x
            Left(res, Some(xs, loc1))
          | None -> (err.UnexpectedSymbol loc0 x, None) |> Right)

    member _.Try(p: Parser<'a, 'sym, 'loc, 'err>) : Parser<Sum<'a, 'err>, 'sym, 'loc, 'err> =
      let (Parser p') = p
      Parser(FreeNode.catch p')

    member parser.All(ps: List<Parser<'a, 'sym, 'loc, 'err>>) : Parser<List<'a>, 'sym, 'loc, 'err> =
      let ps = ps |> List.map (fun (Parser p) -> p)
      Parser(FreeNode.all ps)

    member parser.Any(ps: List<Parser<'a, 'sym, 'loc, 'err>>) : Parser<'a, 'sym, 'loc, 'err> =
      let ps = ps |> List.map (fun (Parser p) -> p)
      Parser(FreeNode.any err.Concat ps)

    member parser.Stream: Parser<List<'sym>, 'sym, 'loc, 'err> =
      fromStep (fun (input: List<'sym>, _) -> Left(input, None))

    member parser.State: Parser<List<'sym> * 'loc, 'sym, 'loc, 'err> =
      fromStep (fun s -> Left(s, None))

    member parser.SetState(s: List<'sym> * 'loc) : Parser<Unit, 'sym, 'loc, 'err> = fromStep (fun _ -> Left((), Some s))

    member parser.Location: Parser<'loc, 'sym, 'loc, 'err> =
      fromStep (fun (_input, loc) -> Left(loc, None))

    member parser.MapError (f: 'err -> 'err) (p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'a, 'sym, 'loc, 'err> =
      Parser.MapError f p

    member parser.Map (f: 'a -> 'b) (p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'b, 'sym, 'loc, 'err> = Parser.Map f p

    member private parser.ManyAcc
      (l: List<'a>)
      (p: int -> Parser<'a, 'sym, 'loc, 'err>)
      : Parser<List<'a>, 'sym, 'loc, 'err> =
      parser {
        let! x = (p l.Length) |> parser.Try

        match x with
        | Right _ -> return l |> List.rev
        | Left x -> return! parser.ManyAcc (x :: l) p
      }

    member parser.Many(p: Parser<'a, 'sym, 'loc, 'err>) : Parser<List<'a>, 'sym, 'loc, 'err> =
      parser.ManyAcc [] (fun _ -> p)

    member parser.ManyIndex(p: int -> Parser<'a, 'sym, 'loc, 'err>) : Parser<List<'a>, 'sym, 'loc, 'err> =
      parser.ManyAcc [] p

    member parser.Lookahead(p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'a, 'sym, 'loc, 'err> =
      parser {
        let! s = parser.State
        let! res = p |> parser.Try
        do! parser.SetState s
        return! parser.OfSum res
      }

    member parser.OfSum(p: Sum<'a, 'err>) : Parser<'a, 'sym, 'loc, 'err> =
      match p with
      | Left v -> parser.Return v
      | Right e -> parser.Throw e

    member parser.Not(p: Parser<'a, 'sym, 'loc, 'err>) : Parser<Unit, 'sym, 'loc, 'err> =
      parser {
        let! s = parser.State
        let! res = p |> parser.Try
        do! parser.SetState s

        match res with
        | Left _res ->
          let! loc = parser.Location
          return! parser.Throw(err.NotFailed loc)
        | Right _err -> return ()
      }

    member parser.AtLeastOne(p: Parser<'a, 'sym, 'loc, 'err>) : Parser<NonEmptyList<'a>, 'sym, 'loc, 'err> =
      parser {
        let! x = p
        let! xs = parser.Many p
        return NonEmptyList.OfList(x, xs)
      }

    member parser.Ignore(p: Parser<'a, 'sym, 'loc, 'err>) : Parser<Unit, 'sym, 'loc, 'err> = p |> parser.Map ignore

    member parser.DebugErrors
      (label: string)
      (print_errors: 'err -> string)
      (p: Parser<'a, 'sym, 'loc, 'err>)
      : Parser<'a, 'sym, 'loc, 'err> =
      parser {
        let! res = p |> parser.Try

        match res with
        | Right errors ->
          do Console.WriteLine $"{label} errors = {print_errors errors}"
          do Console.ReadLine() |> ignore
        | _ -> ()

        return! res |> parser.OfSum
      }
