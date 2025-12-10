namespace Ballerina.Collections

module NonEmptySet =
  type NonEmptySet<'e when 'e: comparison> =
    private
    | NonEmptySet of Set<'e>

    interface System.Collections.Generic.IEnumerable<'e> with
      member l.GetEnumerator() : System.Collections.Generic.IEnumerator<'e> =
        (l |> NonEmptySet.ToSeq).GetEnumerator()

      member l.GetEnumerator() : System.Collections.IEnumerator =
        (l |> NonEmptySet.ToSeq).GetEnumerator()

    static member TryOfArray(array: 'e array) =
      match array with
      | [||] -> None
      | _ -> Set.ofArray array |> NonEmptySet |> Some

    static member TryOfList(list: 'e list) =
      match list with
      | [] -> None
      | _ -> Set.ofList list |> NonEmptySet |> Some

    static member ToSeq(NonEmptySet set: NonEmptySet<'e>) = Set.toSeq set

    static member ToList(NonEmptySet set: NonEmptySet<'e>) = Set.toList set
