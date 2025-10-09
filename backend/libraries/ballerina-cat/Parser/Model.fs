namespace Ballerina.Parser

[<AutoOpen>]
module Model =
  open Ballerina.Fun
  open Ballerina.Collections.Sum
  open Ballerina.Collections
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Reader.WithError


  open FSharp.Core
  open Ballerina.Collections.NonEmptyList

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
    | Parser of ((List<'sym> * 'loc) -> Sum<ParserResult<'a, 'sym, 'loc, 'err>, 'err * Option<List<'sym> * 'loc>>)

    static member Run (input: List<'sym>, loc: 'loc) (Parser p: Parser<'a, 'sym, 'loc, 'err>) = p (input, loc)

    static member Throw(e: 'err) : Parser<'a, 'sym, 'loc, 'err> = Parser(fun _ -> Right(e, None))

    static member Return(v: 'a) : Parser<'a, 'sym, 'loc, 'err> =
      Parser(fun _ -> Left(ParserResult.Return v))

    static member Map (f: 'a -> 'b) (p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'b, 'sym, 'loc, 'err> =
      Parser(fun (input: List<'sym>, loc: 'loc) -> p |> Parser.Run(input, loc) |> Sum.map (ParserResult.Map f))

    static member MapError (f: 'err -> 'err) (p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'a, 'sym, 'loc, 'err> =
      Parser(fun (input: List<'sym>, loc: 'loc) -> p |> Parser.Run(input, loc) |> Sum.mapRight (fun (e, y) -> f e, y))

    static member Flatten(p0: Parser<Parser<'a, 'sym, 'loc, 'err>, 'sym, 'loc, 'err>) : Parser<'a, 'sym, 'loc, 'err> =
      Parser(fun (s0: List<'sym> * 'loc) ->
        match p0 |> Parser.Run s0 with
        | Right e -> Right e
        | Left(ParserResult(p1, s1)) ->
          match Parser.Run (s1 |> Option.defaultValue s0) p1 with
          | Right e -> Right e
          | Left(ParserResult(v, s2)) -> Left(ParserResult(v, Option.orElse s1 s2)))

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
      Parser(fun (input: List<'sym>, loc0: 'loc) ->
        match input with
        | [] -> Left(ParserResult((), Some([], loc0)))
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
      Parser(fun (input: List<'sym>, loc0: 'loc) ->
        match input with
        | [] -> (err.UnexpectedEndOfFile loc0, None) |> Right
        | x :: xs ->
          if x = expected then
            let loc1 = loc0 |> loc.Step x
            Left(ParserResult(x, Some(xs, loc1)))
          else
            (err.UnexpectedSymbol loc0 x, None) |> Right)

    member _.Exactly(predicate: 'sym -> bool) : Parser<'sym, 'sym, 'loc, 'err> =
      Parser(fun (input: List<'sym>, loc0: 'loc) ->
        match input with
        | [] -> (err.UnexpectedEndOfFile loc0, None) |> Right
        | x :: xs ->
          if predicate x then
            let loc1 = loc0 |> loc.Step x
            Left(ParserResult(x, Some(xs, loc1)))
          else
            (err.UnexpectedSymbol loc0 x, None) |> Right)

    member _.Exactly(predicate: 'sym -> Option<'a>) : Parser<'a, 'sym, 'loc, 'err> =
      Parser(fun (input: List<'sym>, loc0: 'loc) ->
        match input with
        | [] -> (err.UnexpectedEndOfFile loc0, None) |> Right
        | x :: xs ->
          match predicate x with
          | Some res ->
            let loc1 = loc0 |> loc.Step x
            Left(ParserResult(res, Some(xs, loc1)))
          | None -> (err.UnexpectedSymbol loc0 x, None) |> Right)

    member _.Try(p: Parser<'a, 'sym, 'loc, 'err>) : Parser<Sum<'a, 'err>, 'sym, 'loc, 'err> =
      Parser(fun (input: List<'sym>, loc0: 'loc) ->
        match p |> Parser.Run(input, loc0) with
        | Left(ParserResult(v, s1)) -> Left(ParserResult(Left v, s1))
        | Right(e, s1) -> Left(ParserResult(Right e, s1)))

    member parser.All(ps: List<Parser<'a, 'sym, 'loc, 'err>>) : Parser<List<'a>, 'sym, 'loc, 'err> =
      Parser(fun s0 ->
        let mutable s = None
        let mutable res = ResizeArray<'a>()
        let mutable err_acc: Option<'err> = None

        for p in ps do
          if err_acc.IsSome then
            ()
          else
            let s0 = s |> Option.defaultValue s0

            match p |> Parser.Run s0 with
            | Left(ParserResult(v, s1)) ->
              res.Add v
              s <- s1 |> Option.orElse s
            | Right(e, s1) ->
              err_acc <-
                match err_acc with
                | Some err_acc -> err.Concat(err_acc, e) |> Some
                | None -> Some e

              s <- s1 |> Option.orElse s

        match err_acc with
        | None -> Left(ParserResult(res |> List.ofSeq, s)) // all succeeded
        | Some err -> Right(err, None))


    member parser.Any(ps: List<Parser<'a, 'sym, 'loc, 'err>>) : Parser<'a, 'sym, 'loc, 'err> =
      Parser(fun s0 ->
        let mutable s = None
        let mutable res = None
        let mutable err_acc: Option<'err> = None

        for p in ps do
          if res.IsSome then
            ()
          else
            let s0 = s |> Option.defaultValue s0

            match p |> Parser.Run s0 with
            | Left(ParserResult(v, s1)) ->
              res <- Some v
              s <- s1 |> Option.orElse s
            | Right(e, s1) ->
              err_acc <-
                match err_acc with
                | Some err_acc -> err.Concat(err_acc, e) |> Some
                | None -> Some e

              s <- s1 |> Option.orElse s

        match res, err_acc with
        | Some res, _ -> Left(ParserResult(res, s)) // all succeeded
        | _, Some err -> Right(err, None)
        | _ -> Right(err.AnyFailed(snd s0), None))
      |> parser.MapError err.FilterHighestPriorityOnly

    member parser.Stream: Parser<List<'sym>, 'sym, 'loc, 'err> =
      Parser(fun (input: List<'sym>, _) -> Left(ParserResult.Return input))

    member parser.State: Parser<List<'sym> * 'loc, 'sym, 'loc, 'err> =
      Parser(fun s -> Left(ParserResult.Return s))

    member parser.SetState(s: List<'sym> * 'loc) : Parser<Unit, 'sym, 'loc, 'err> =
      Parser(fun _ -> Left(ParserResult.FromState((), s)))

    member parser.Location: Parser<'loc, 'sym, 'loc, 'err> =
      Parser(fun (_, loc) -> Left(ParserResult.Return loc))

    member parser.MapError (f: 'err -> 'err) (p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'a, 'sym, 'loc, 'err> =
      Parser.MapError f p

    member parser.Map (f: 'a -> 'b) (p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'b, 'sym, 'loc, 'err> = Parser.Map f p

    member private parser.ManyAcc (l: List<'a>) (p: Parser<'a, 'sym, 'loc, 'err>) : Parser<List<'a>, 'sym, 'loc, 'err> =
      parser {
        let! x = p |> parser.Try

        match x with
        | Right _ -> return l |> List.rev
        | Left x -> return! parser.ManyAcc (x :: l) p
      }

    member parser.Many(p: Parser<'a, 'sym, 'loc, 'err>) : Parser<List<'a>, 'sym, 'loc, 'err> = parser.ManyAcc [] p

    member parser.Lookahead(p: Parser<'a, 'sym, 'loc, 'err>) : Parser<'a, 'sym, 'loc, 'err> =
      Parser(fun s0 ->
        match p |> Parser.Run s0 with
        | Left(ParserResult(v, _)) -> Left(ParserResult(v, None)) // restore original state
        | Right(e, _) -> Right(e, None))
    // parser {
    //   let! s = parser.State
    //   let! res = p
    //   do! parser.SetState s
    //   return res
    // }

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
