namespace Ballerina.Collections

module NonEmptyList =
  open Ballerina.Fun

  type NonEmptyList<'e> =
    | NonEmptyList of 'e * List<'e>

    interface System.Collections.Generic.IEnumerable<'e> with
      member l.GetEnumerator() : System.Collections.Generic.IEnumerator<'e> =
        (l |> NonEmptyList.ToSeq).GetEnumerator()

      member l.GetEnumerator() : System.Collections.IEnumerator =
        (l |> NonEmptyList.ToSeq).GetEnumerator()

    member l.Head =
      match l with
      | NonEmptyList(h, _) -> h

    member l.Tail =
      match l with
      | NonEmptyList(_, t) -> t

    static member rev(l: NonEmptyList<'e>) =
      let l = l |> NonEmptyList.ToList |> List.rev
      NonEmptyList.OfList(l.Head, l.Tail)

    static member map (f: 'e -> 'b) (l: NonEmptyList<'e>) =
      match l with
      | NonEmptyList(h, t) -> NonEmptyList(f h, t |> List.map f)

    static member reduce (f: 'e -> 'e -> 'e) (l: NonEmptyList<'e>) =
      match l with
      | NonEmptyList(h, t) -> List.reduce f (h :: t)

    static member ToList(l: NonEmptyList<'e>) =
      match l with
      | NonEmptyList(h, t) -> h :: t

    static member ToSeq(l: NonEmptyList<'e>) =
      seq {
        match l with
        | NonEmptyList(h, t) ->
          yield h
          yield! t
      }

    static member OfList(head: 'e, tail: List<'e>) = NonEmptyList(head, tail)

    static member TryOfList(l: List<'e>) =
      match l with
      | x :: xs -> NonEmptyList.OfList(x, xs) |> Some
      | _ -> None

    static member One(e: 'e) = NonEmptyList(e, [])
