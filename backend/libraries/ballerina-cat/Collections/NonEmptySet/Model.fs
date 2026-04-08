namespace Ballerina.Collections

module NonEmptySet =
  type NonEmptySet<[<EqualityConditionalOn>] 'e when 'e: comparison> =
    private
    | NonEmptySet of 'e Set

    interface System.Collections.Generic.IEnumerable<'e> with
      member l.GetEnumerator() : System.Collections.Generic.IEnumerator<'e> =
        (l |> NonEmptySet.ToSeq).GetEnumerator()

      member l.GetEnumerator() : System.Collections.IEnumerator =
        (l |> NonEmptySet.ToSeq).GetEnumerator()

    interface System.Collections.Generic.IReadOnlyCollection<'e> with
      member this.Count = let (NonEmptySet set) = this in set.Count

    static member OfList(x: 'e, xs: 'e list) = x :: xs |> Set.ofList |> NonEmptySet

    static member TryOfArray(array: 'e array) =
      match array with
      | [||] -> None
      | _ -> Set.ofArray array |> NonEmptySet |> Some

    static member TryOfList(list: 'e list) =
      match list with
      | [] -> None
      | _ -> Set.ofList list |> NonEmptySet |> Some

    static member TryOfSeq(seq: 'e seq) =
      if seq |> Seq.isEmpty then
        None
      else
        Set.ofSeq seq |> NonEmptySet |> Some

    static member TryOfSet(set: 'e Set) =
      if set |> Set.isEmpty then
        None
      else
        set |> NonEmptySet |> Some

    static member ToSet(NonEmptySet set: 'e NonEmptySet) = set

    static member ToSeq(NonEmptySet set: 'e NonEmptySet) = Set.toSeq set

    static member ToList(NonEmptySet set: 'e NonEmptySet) = Set.toList set

  let (|NonEmptySet|) (NonEmptySet set: 'e NonEmptySet) = set

  let intersect (NonEmptySet s1) (NonEmptySet s2) = Set.intersect s1 s2

  let unionMany (sets: 'a NonEmptySet seq) =
    sets |> Seq.map (fun (NonEmptySet set) -> set) |> Set.unionMany |> NonEmptySet
