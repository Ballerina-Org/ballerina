namespace Ballerina.Collections

module NonEmptyList =
  // NoneEmptyList is the implementation of a free subgroup
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

    static member map (f: 'e -> 'b) (NonEmptyList(h, t): NonEmptyList<'e>) = NonEmptyList(f h, t |> List.map f)

    static member mapi (f: int -> 'e -> 'b) (NonEmptyList(h, t): NonEmptyList<'e>) : NonEmptyList<'b> =
      NonEmptyList(f 0 h, t |> List.mapi (fun i e -> f (i + 1) e))

    static member fold
      (f: 'state -> 'e -> 'state)
      (initial: 'e -> 'state)
      (NonEmptyList(h, t): NonEmptyList<'e>)
      : 'state =
      List.fold f (initial h) t


    static member reduce (f: 'e -> 'e -> 'e) (l: NonEmptyList<'e>) =
      match l with
      | NonEmptyList(h, t) -> List.reduce f (h :: t)

    static member append (NonEmptyList(head1, tail1): NonEmptyList<'e>) (NonEmptyList(head2, tail2): NonEmptyList<'e>) =
      NonEmptyList.OfList(head1, tail1 @ [ head2 ] @ tail2)

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

    static member prependList (l: List<'e>) (NonEmptyList(head, tail): NonEmptyList<'e>) =
      match l with
      | [] -> NonEmptyList(head, tail)
      | listHead :: listTail -> NonEmptyList.OfList(listHead, listTail @ [ head ] @ tail)
