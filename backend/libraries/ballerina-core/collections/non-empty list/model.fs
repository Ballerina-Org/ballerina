namespace Ballerina.Collections

module NonEmptyList =
  open Ballerina.Fun

  type NonEmptyList<'e> =
    | One of 'e
    | Many of 'e * NonEmptyList<'e>

    interface System.Collections.Generic.IEnumerable<'e> with
      member l.GetEnumerator() : System.Collections.Generic.IEnumerator<'e> =
        (l |> NonEmptyList.ToSeq).GetEnumerator()

      member l.GetEnumerator() : System.Collections.IEnumerator =
        (l |> NonEmptyList.ToSeq).GetEnumerator()

    member l.Head =
      match l with
      | One h -> h
      | Many(h, _) -> h

    member l.Tail =
      match l with
      | One _ -> []
      | Many(_, t) -> t |> NonEmptyList.ToList

    static member map (f: 'e -> 'b) (l: NonEmptyList<'e>) =
      match l with
      | One h -> One(f h)
      | Many(h, t) -> Many(f h, t |> NonEmptyList.map f)

    static member reduce (f: 'e -> 'e -> 'e) (l: NonEmptyList<'e>) =
      match l with
      | One e -> e
      | Many(h, t) -> f h (NonEmptyList.reduce f t)

    static member ToList(l: NonEmptyList<'e>) =
      match l with
      | One e -> [ e ]
      | Many(h, t) -> h :: (NonEmptyList.ToList t)

    static member ToSeq(l: NonEmptyList<'e>) =
      seq {
        match l with
        | One e -> yield e
        | Many(h, t) ->
          yield h
          yield! NonEmptyList.ToSeq t
      }

    static member OfList(head: 'e, tail) =
      match tail with
      | [] -> One head
      | x :: xs -> Many(head, NonEmptyList.OfList(x, xs))

    static member TryOfList(l: List<'e>) =
      match l with
      | x :: xs -> NonEmptyList.OfList(x, xs) |> Some
      | _ -> None
