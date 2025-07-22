namespace Ballerina.Collections

module Sum =
  open Ballerina.Fun
  open System
  open System.Threading.Tasks
  open Ballerina.Collections.NonEmptyList

  type Sum<'a, 'b> =
    | Left of 'a
    | Right of 'b

    static member map<'a, 'b, 'c>(f: 'a -> 'c) : Sum<'a, 'b> -> Sum<'c, 'b> =
      function
      | Left a -> a |> f |> Left
      | Right b -> Right b

    static member swap<'a, 'b>(p: Sum<'a, 'b>) : Sum<'b, 'a> =
      match p with
      | Left a -> Right a
      | Right b -> Left b

    static member mapRight<'a, 'b, 'c>(f: 'b -> 'c) : Sum<'a, 'b> -> Sum<'a, 'c> = Sum.swap >> Sum.map f >> Sum.swap
    static member map2<'a, 'b, 'a1, 'b1> (f: 'a -> 'a1) (g: 'b -> 'b1) = Sum.map f >> Sum.mapRight g

    static member toOption: Sum<'a, 'b> -> _ =
      function
      | Left a -> Some a
      | _ -> None

    static member fromOption<'a, 'b>(rightPlaceholder: Unit -> 'b) : Option<'a> -> Sum<'a, 'b> =
      function
      | Some a -> Left a
      | _ -> Right(rightPlaceholder ())

    static member flatten<'a, 'b>(p: Sum<Sum<'a, 'b>, 'b>) : Sum<'a, 'b> =
      match p with
      | Left(Left a) -> Left a
      | Right b
      | Left(Right b) -> Right b

    static member bind<'a, 'b, 'c>(k: 'a -> Sum<'c, 'b>) : Sum<'a, 'b> -> Sum<'c, 'b> =
      Sum.map<'a, 'b, Sum<'c, 'b>> k >> Sum.flatten

  let inline (<+>) (f: 'a -> 'a1) (g: 'b -> 'b1) = Sum.map2<'a, 'b, 'a1, 'b1> f g

  type SumBuilder() =
    member _.MapError f p = p |> Sum.mapRight f
    member _.Map f p = p |> Sum.map f

    member _.Catch h p =
      match p with
      | Left _ -> p
      | Right _ -> Left(h ())

    member _.Throw(e) = Sum.Right e
    member _.Return(result: 'a) = Sum.Left result
    member sum.ReturnFrom(result: Sum<_, _>) = result
    member _.Yield(result: 'a) = Sum.Left result
    member _.Bind(p, k) = Sum.bind k p
    member _.Combine(p, k) = Sum.bind (fun _ -> k) p
    member _.OfOption (b: 'b) (p: Option<'a>) = Sum.fromOption (fun () -> b) p

    member inline _.Any<'a, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>(ps: NonEmptyList<Sum<'a, 'b>>) =
      let merge: Sum<'a, 'b> -> Sum<'a, 'b> -> Sum<'a, 'b> =
        function
        | Left a -> fun _ -> Left a
        | Right b1 ->
          function
          | Left a -> Left a
          | Right b2 -> Right('b.Concat(b1, b2))

      match ps with
      | One p -> p
      | Many(p, ps) -> ps |> Seq.fold merge p

    member inline _.Any2<'a, 'b when 'b: (static member Concat: 'b * 'b -> 'b)> (p1: Sum<'a, 'b>) (p2: Sum<'a, 'b>) =
      match p1, p2 with
      | Left v, _
      | _, Left v -> Left v
      | Right e1, Right e2 -> Right('b.Concat(e1, e2))

    member inline _.All<'a, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>(ps: List<Sum<'a, 'b>>) =
      let merge: Sum<List<'a>, 'b> -> Sum<'a, 'b> -> Sum<List<'a>, 'b> =
        function
        | Left a1 ->
          function
          | Left a2 -> Left(a1 @ [ a2 ])
          | Right b2 -> Right b2
        | Right b1 ->
          function
          | Right b2 -> Right('b.Concat(b1, b2))
          | _ -> Right b1

      ps |> Seq.fold merge (Left [])

    member inline sum.All<'a, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>
      (ps: seq<Sum<'a, 'b>>)
      : Sum<List<'a>, 'b> =
      ps |> List.ofSeq |> sum.All

    member inline _.All2<'a1, 'a2, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>
      (p1: Sum<'a1, 'b>)
      (p2: Sum<'a2, 'b>)
      =
      match p1, p2 with
      | Left v1, Left v2 -> Left(v1, v2)
      | Right e1, Right e2 -> Right('b.Concat(e1, e2))
      | Right e, _
      | _, Right e -> Right e

    member sum.Delay p = sum.Bind((sum.Return()), p)

    member sum.Lift2 (f: 'a -> 'b -> 'c) (p1: Sum<_, 's>) (p2: Sum<_, 's>) =
      sum {
        let! a = p1
        let! b = p2
        return f a b
      }

    member inline sum.AllMap<'k, 'v, 'b when 'k: comparison and 'b: (static member Concat: 'b * 'b -> 'b)>
      (ps: Map<'k, Sum<'v, 'b>>)
      : Sum<Map<'k, 'v>, 'b> =
      ps
      |> Map.map (fun k p ->
        sum {
          let! v = p
          return (k, v)
        })
      |> Map.values
      |> sum.All
      |> sum.Map(Map.ofList)

  let sum = SumBuilder()

  type Sum<'a, 'b> with
    static member (>>=)(p, q) =
      sum {
        let! x = p
        return! q x
      }
