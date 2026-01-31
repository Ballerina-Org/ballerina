namespace Ballerina.StdLib


module OrderPreservingMap =
  (*
    Insertion order preserving map.

    Complexity:
    - insert and lookup: same as Map
    *)
  open Ballerina.Errors
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina

  let private withError (e: unit -> string) (o: Option<'res>) : Sum<'res, Errors<Unit>> =
    o |> Sum.fromOption<'res, Errors<Unit>> (fun () -> Errors.Singleton () e)

  type OrderedMap<'K, 'V when 'K: comparison> with
    static member tryFindWithError
      (k: 'K)
      (k_category: string)
      (k_error: string)
      (m: OrderedMap<'K, 'V>)
      : Sum<'V, Errors<Unit>> =
      OrderedMap.tryFind k m
      |> withError (fun () -> sprintf "Cannot find %s '%s'" k_category k_error)

    static member tryFindByWithError
      (predicate: 'K * 'V -> bool)
      (k_category: string)
      (k_error: string)
      (m: OrderedMap<'K, 'V>)
      : Sum<'K * 'V, Errors<Unit>> =
      OrderedMap.toSeq m
      |> Seq.map (fun (k, v) -> k, v)
      |> Seq.tryFind predicate
      |> withError (fun () -> sprintf "Cannot find %s '%s'" k_category k_error)
    // new methods
    static member ofListIfNoDuplicates(kvs: List<'K * 'V>) : Sum<OrderedMap<'K, 'V>, Errors<Unit>> =
      let duplicateKeys =
        kvs
        |> List.groupBy fst
        |> List.filter (fun (_, vs) -> vs.Length > 1)
        |> List.map fst

      if duplicateKeys.IsEmpty then
        OrderedMap.ofList kvs |> Left
      else
        Errors.Singleton () (fun () -> sprintf "Duplicate keys: %A" duplicateKeys)
        |> Errors.MapPriority(replaceWith ErrorPriority.Medium)
        |> Right

    static member mergeSecondAfterFirstIfNoDuplicates
      (om1: OrderedMap<'K, 'V>)
      (om2: OrderedMap<'K, 'V>)
      : Sum<OrderedMap<'K, 'V>, Errors<Unit>> =
      let conflicts = om2.reverseOrder |> List.filter (fun k -> om1.data.ContainsKey k)

      if conflicts.IsEmpty then
        OrderedMap.mergeSecondAfterFirst om1 om2 |> Left
      else
        let errorMsg = sprintf "Key conflicts during merge: %A" conflicts

        Errors.Singleton () (fun () -> errorMsg)
        |> Errors.MapPriority(replaceWith ErrorPriority.Medium)
        |> Right

  type StateBuilder with
    member inline state.AllMapOrdered<'k, 'a, 'c, 's, 'e
      when 'k: comparison and 'e: (static member Concat: 'e * 'e -> 'e)>
      (ps: OrderedMap<'k, State<'a, 'c, 's, 'e>>)
      =
      state.All(
        {| concat = 'e.Concat |},
        ps
        |> OrderedMap.toSeq
        |> Seq.map (fun (k, p) ->
          state {
            let! v = p
            return k, v
          })
        |> Seq.toList
      )
      |> state.Map(OrderedMap.ofSeq)

  type SumBuilder with
    member inline sum.AllMapOrdered<'k, 'v, 'b when 'k: comparison and 'b: (static member Concat: 'b * 'b -> 'b)>
      (ps: OrderedMap<'k, Sum<'v, 'b>>)
      : Sum<OrderedMap<'k, 'v>, 'b> =
      ps
      |> OrderedMap.map (fun k p ->
        sum {
          let! v = p
          return k, v
        })
      |> OrderedMap.values
      |> sum.All
      |> sum.Map OrderedMap.ofSeq

  type ReaderBuilder with
    member inline _.AllMapOrdered<'c, 'a, 'e, 'k when 'k: comparison and 'e: (static member Concat: 'e * 'e -> 'e)>
      (readers: OrderedMap<'k, Reader<'a, 'c, 'e>>)
      : Reader<OrderedMap<'k, 'a>, 'c, 'e> =
      Reader(fun (c: 'c) ->
        sum {
          let! (results: OrderedMap<'k, 'a>) =
            readers |> OrderedMap.map (fun _k (Reader p) -> p c) |> sum.AllMapOrdered

          return results
        })
