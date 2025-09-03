namespace Ballerina.Reader

module WithError =
  open Ballerina.Fun
  open Ballerina.Collections.Sum
  open Ballerina.Collections
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Sum
  open Ballerina.Collections.NonEmptyList

  type Reader<'a, 'c, 'e> =
    | Reader of ('c -> Sum<'a, 'e>)

    static member ofSum(r: Sum<'a, 'e>) : Reader<'a, 'c, 'e> = Reader(fun _ -> r)

    static member Run (c: 'c) (Reader r: Reader<'a, 'c, 'e>) = c |> r

    static member map<'b>(f: 'a -> 'b) : (Reader<'a, 'c, 'e> -> Reader<'b, 'c, 'e>) =
      fun (Reader r) -> Reader(r >> Sum.map f)

    static member mapError<'e1>(f: 'e -> 'e1) : (Reader<'a, 'c, 'e> -> Reader<'a, 'c, 'e1>) =
      fun (Reader r) ->
        Reader(fun ctx ->
          match r ctx with
          | Left res -> Left res
          | Right err -> Right(f err))

    static member mapContext<'c1>(f: 'c1 -> 'c) : (Reader<'a, 'c, 'e> -> Reader<'a, 'c1, 'e>) =
      fun (Reader r) -> Reader(f >> r)

    static member join: Reader<Reader<'a, 'c, 'e>, 'c, 'e> -> Reader<'a, 'c, 'e> =
      fun (Reader r) ->
        Reader(fun c ->
          sum {
            let! Reader r_r = r c
            return! r_r c
          })

    static member cons: 'a -> Reader<'a, 'c, 'e> = fun v -> Reader(fun _c -> sum { return v })

    static member throw: 'e -> Reader<'a, 'c, 'e> = fun e -> Reader(fun _c -> sum.Throw(e))

  type ReaderBuilder() =
    member _.Return<'c, 'a, 'e>(v: 'a) : Reader<'a, 'c, 'e> = Reader.cons v
    member _.ReturnFrom<'c, 'a, 'e>(r: Reader<'a, 'c, 'e>) = r

    member _.Bind<'c, 'a, 'b, 'e>(r: Reader<'a, 'c, 'e>, f: 'a -> Reader<'b, 'c, 'e>) = r |> Reader.map f |> Reader.join

    member reader.Combine<'c, 'a, 'b, 'e>(r1: Reader<'a, 'c, 'e>, r2: Reader<'b, 'c, 'e>) =
      reader {
        let! _ = r1
        return! r2
      }

    member reader.Catch((Reader p): Reader<'a, 'c, 'e>) : Reader<Sum<'a, 'e>, 'c, 'e> =
      Reader(fun cs ->
        match p cs with
        | Left(res: 'a) -> let result: Sum<'a, 'e> = Left res in Left(result)
        | Right(err: 'e) -> let result: Sum<'a, 'e> = Right err in Left(result))

    member reader.Throw<'c, 'a, 'e>(error: 'e) : Reader<'a, 'c, 'e> = Reader(fun _ -> sum.Throw(error))

    member inline _.All2<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (Reader r1: Reader<'a, 'c, 'e>)
      : Reader<'a, 'c, 'e> -> Reader<'a * 'a, 'c, 'e> =
      fun (Reader r2) -> Reader(fun (c: 'c) -> sum.All2 (r1 c) (r2 c))

    member inline _.Any2<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (Reader r1: Reader<'a, 'c, 'e>)
      : Reader<'a, 'c, 'e> -> Reader<'a, 'c, 'e> =
      fun (Reader r2) -> Reader(fun (c: 'c) -> sum.Any2 (r1 c) (r2 c))

    member inline _.All<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (readers: seq<Reader<'a, 'c, 'e>>)
      : Reader<List<'a>, 'c, 'e> =
      Reader(fun (c: 'c) -> sum.All(readers |> Seq.map (fun (Reader r) -> r c)))

    member inline _.Any<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (readers: NonEmptyList<Reader<'a, 'c, 'e>>)
      : Reader<'a, 'c, 'e> =
      Reader(fun (c: 'c) -> sum.Any(readers |> NonEmptyList.map (fun (Reader r) -> r c)))

    member inline reader.Any<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (p: Reader<'a, 'c, 'e>, ps: Reader<'a, 'c, 'e> list)
      : Reader<'a, 'c, 'e> =
      reader.Any(NonEmptyList.OfList(p, ps))

    member inline reader.AllMap<'c, 'a, 'e, 'k when 'k: comparison and 'e: (static member Concat: 'e * 'e -> 'e)>
      (readers: Map<'k, Reader<'a, 'c, 'e>>)
      : Reader<Map<'k, 'a>, 'c, 'e> =
      Reader(fun (c: 'c) ->
        sum {
          let! (results: Map<'k, 'a>) = readers |> Map.map (fun _k (Reader p) -> p c) |> sum.AllMap
          return results
        })

    member _.OfSum(s: Sum<'a, 'e>) : Reader<'a, 'c, 'e> = Reader.ofSum s

    member _.MapContext<'c1, 'c, 'a, 'e>(f: 'c1 -> 'c) : Reader<'a, 'c, 'e> -> Reader<'a, 'c1, 'e> = Reader.mapContext f

    member _.Map<'c, 'a, 'b, 'e>(f: 'a -> 'b) : Reader<'a, 'c, 'e> -> Reader<'b, 'c, 'e> = Reader.map f

    member _.MapError<'c, 'a, 'e, 'e1>(f: 'e -> 'e1) : Reader<'a, 'c, 'e> -> Reader<'a, 'c, 'e1> = Reader.mapError f

    member _.Ignore<'c, 'a, 'e>(r: Reader<'a, 'c, 'e>) : Reader<Unit, 'c, 'e> = Reader.map (fun _ -> ()) r

    member _.GetContext() : Reader<'c, 'c, 'e> = Reader(fun c -> sum { return c })

  let reader = ReaderBuilder()

  module Operators =
    let (>>=) f g = fun x -> reader.Bind(f x, g)
