namespace Ballerina.Reader

module WithError =
  open Ballerina.Fun
  open Ballerina.Collections.Sum
  open Ballerina.Collections
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Sum
  open Ballerina.Collections.NonEmptyList

  type ReaderWithError<'c, 'a, 'e> =
    | ReaderWithError of ('c -> Sum<'a, 'e>)

    static member ofSum(r: Sum<'a, 'e>) : ReaderWithError<'c, 'a, 'e> = ReaderWithError(fun _ -> r)

    static member Run (c: 'c) (ReaderWithError r: ReaderWithError<'c, 'a, 'e>) = c |> r

    static member map<'b>(f: 'a -> 'b) : (ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c, 'b, 'e>) =
      fun (ReaderWithError r) -> ReaderWithError(r >> Sum.map f)

    static member mapError<'e1>(f: 'e -> 'e1) : (ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c, 'a, 'e1>) =
      fun (ReaderWithError r) ->
        ReaderWithError(fun ctx ->
          match r ctx with
          | Left res -> Left res
          | Right err -> Right(f err))

    static member mapContext<'c1>(f: 'c1 -> 'c) : (ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c1, 'a, 'e>) =
      fun (ReaderWithError r) -> ReaderWithError(f >> r)

    static member join: ReaderWithError<'c, ReaderWithError<'c, 'a, 'e>, 'e> -> ReaderWithError<'c, 'a, 'e> =
      fun (ReaderWithError r) ->
        ReaderWithError(fun c ->
          sum {
            let! ReaderWithError r_r = r c
            return! r_r c
          })

    static member cons: 'a -> ReaderWithError<'c, 'a, 'e> =
      fun v -> ReaderWithError(fun _c -> sum { return v })

    static member throw: 'e -> ReaderWithError<'c, 'a, 'e> =
      fun e -> ReaderWithError(fun _c -> sum.Throw(e))

  type ReaderWithErrorBuilder() =
    member _.Return<'c, 'a, 'e>(v: 'a) : ReaderWithError<'c, 'a, 'e> = ReaderWithError.cons v
    member _.ReturnFrom<'c, 'a, 'e>(r: ReaderWithError<'c, 'a, 'e>) = r

    member _.Bind<'c, 'a, 'b, 'e>(r: ReaderWithError<'c, 'a, 'e>, f: 'a -> ReaderWithError<'c, 'b, 'e>) =
      r |> ReaderWithError.map f |> ReaderWithError.join

    member reader.Combine<'c, 'a, 'b, 'e>(r1: ReaderWithError<'c, 'a, 'e>, r2: ReaderWithError<'c, 'b, 'e>) =
      reader {
        let! _ = r1
        return! r2
      }

    member reader.Catch((ReaderWithError p): ReaderWithError<'c, 'a, 'e>) : ReaderWithError<'c, Sum<'a, 'e>, 'e> =
      ReaderWithError(fun cs ->
        match p cs with
        | Left(res: 'a) -> let result: Sum<'a, 'e> = Left res in Left(result)
        | Right(err: 'e) -> let result: Sum<'a, 'e> = Right err in Left(result))

    member reader.Throw<'c, 'a, 'e>(error: 'e) : ReaderWithError<'c, 'a, 'e> =
      ReaderWithError(fun _ -> sum.Throw(error))

    member inline _.All2<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (ReaderWithError r1: ReaderWithError<'c, 'a, 'e>)
      : ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c, 'a * 'a, 'e> =
      fun (ReaderWithError r2) -> ReaderWithError(fun (c: 'c) -> sum.All2 (r1 c) (r2 c))

    member inline _.Any2<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (ReaderWithError r1: ReaderWithError<'c, 'a, 'e>)
      : ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c, 'a, 'e> =
      fun (ReaderWithError r2) -> ReaderWithError(fun (c: 'c) -> sum.Any2 (r1 c) (r2 c))

    member inline _.All<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (readers: seq<ReaderWithError<'c, 'a, 'e>>)
      : ReaderWithError<'c, List<'a>, 'e> =
      ReaderWithError(fun (c: 'c) -> sum.All(readers |> Seq.map (fun (ReaderWithError r) -> r c)))

    member inline _.Any<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (readers: NonEmptyList<ReaderWithError<'c, 'a, 'e>>)
      : ReaderWithError<'c, 'a, 'e> =
      ReaderWithError(fun (c: 'c) -> sum.Any(readers |> NonEmptyList.map (fun (ReaderWithError r) -> r c)))

    member inline reader.Any<'c, 'a, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (p: ReaderWithError<'c, 'a, 'e>, ps: ReaderWithError<'c, 'a, 'e> list)
      : ReaderWithError<'c, 'a, 'e> =
      reader.Any(NonEmptyList.OfList(p, ps))

    member inline reader.AllMap<'c, 'a, 'e, 'k when 'k: comparison and 'e: (static member Concat: 'e * 'e -> 'e)>
      (readers: Map<'k, ReaderWithError<'c, 'a, 'e>>)
      : ReaderWithError<'c, Map<'k, 'a>, 'e> =
      ReaderWithError(fun (c: 'c) ->
        sum {
          let! (results: Map<'k, 'a>) = readers |> Map.map (fun _k (ReaderWithError p) -> p c) |> sum.AllMap
          return results
        })

    member _.OfSum(s: Sum<'a, 'e>) : ReaderWithError<'c, 'a, 'e> = ReaderWithError.ofSum s

    member _.MapContext<'c1, 'c, 'a, 'e>(f: 'c1 -> 'c) : ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c1, 'a, 'e> =
      ReaderWithError.mapContext f

    member _.Map<'c, 'a, 'b, 'e>(f: 'a -> 'b) : ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c, 'b, 'e> =
      ReaderWithError.map f

    member _.MapError<'c, 'a, 'e, 'e1>(f: 'e -> 'e1) : ReaderWithError<'c, 'a, 'e> -> ReaderWithError<'c, 'a, 'e1> =
      ReaderWithError.mapError f

    member _.Ignore<'c, 'a, 'e>(r: ReaderWithError<'c, 'a, 'e>) : ReaderWithError<'c, Unit, 'e> =
      ReaderWithError.map (fun _ -> ()) r

    member _.GetContext() : ReaderWithError<'c, 'c, 'e> =
      ReaderWithError(fun c -> sum { return c })

  let reader = ReaderWithErrorBuilder()

  module Operators =
    let (>>=) f g = fun x -> reader.Bind(f x, g)

  type Reader<'a, 'c, 'e> = ReaderWithError<'c, 'a, 'e>
