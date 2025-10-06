namespace Ballerina.StdLib


module OrderPreservingMap =
  (*
    Insertion order preserving map.

    Complexity:
    - insert and lookup: same as Map
    *)
  open Ballerina.Errors
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError

  let private withError (e: string) (o: Option<'res>) : Sum<'res, Errors> =
    o |> Sum.fromOption<'res, Errors> (fun () -> Errors.Singleton e)

  type OrderedMap<'K, 'V when 'K: comparison> =
    private
      { reverseOrder: List<'K>
        data: Map<'K, 'V> }

    // same as Map
    static member empty: OrderedMap<'K, 'V> = { reverseOrder = []; data = Map.empty }
    member this.IsEmpty = this.data.IsEmpty
    static member isEmpty(om: OrderedMap<'K, 'V>) = om.IsEmpty
    member this.Count = this.data.Count
    static member count(om: OrderedMap<'K, 'V>) = om.Count
    static member containsKey (om: OrderedMap<'K, 'V>) (k: 'K) = om.data.ContainsKey k
    static member tryFind (k: 'K) (om: OrderedMap<'K, 'V>) = om.data.TryFind k

    static member tryFindWithError
      (k: 'K)
      (k_category: string)
      (k_error: string)
      (m: OrderedMap<'K, 'V>)
      : Sum<'V, Errors> =
      OrderedMap.tryFind k m
      |> withError (sprintf "Cannot find %s '%s'" k_category k_error)

    static member tryFindByWithError
      (predicate: 'K * 'V -> bool)
      (k_category: string)
      (k_error: string)
      (m: OrderedMap<'K, 'V>)
      : Sum<'K * 'V, Errors> =
      OrderedMap.toSeq m
      |> Seq.map (fun (k, v) -> k, v)
      |> Seq.tryFind predicate
      |> withError (sprintf "Cannot find %s '%s'" k_category k_error)

    static member keys(om: OrderedMap<'K, 'V>) : List<'K> = om.reverseOrder |> List.rev

    static member values(om: OrderedMap<'K, 'V>) : List<'V> =
      OrderedMap.keys om |> List.map (fun k -> om.data.[k])

    static member map (f: 'K -> 'V -> 'U) (om: OrderedMap<'K, 'V>) : OrderedMap<'K, 'U> =
      { reverseOrder = OrderedMap.keys om |> List.rev
        data = om.data |> Map.map f }

    static member filter (f: 'K -> 'V -> bool) (om: OrderedMap<'K, 'V>) : OrderedMap<'K, 'V> =
      { reverseOrder = om.reverseOrder |> List.filter (fun k -> f k om.data.[k])
        data = om.data |> Map.filter f }

    static member Add (om: OrderedMap<'K, 'V>) (k: 'K) (v: 'V) : OrderedMap<'K, 'V> =
      if om.data.ContainsKey k then
        { om with data = om.data.Add(k, v) }
      else
        { reverseOrder = k :: om.reverseOrder
          data = om.data.Add(k, v) }

    static member ofList(kvs: List<'K * 'V>) : OrderedMap<'K, 'V> =
      List.fold (fun acc (k, v) -> OrderedMap.Add acc k v) OrderedMap.empty kvs

    static member ofSeq(kvs: seq<'K * 'V>) : OrderedMap<'K, 'V> =
      Seq.fold (fun acc (k, v) -> OrderedMap.Add acc k v) OrderedMap.empty kvs

    static member toList(om: OrderedMap<'K, 'V>) : List<'K * 'V> =
      OrderedMap.keys om |> List.map (fun k -> k, om.data.[k])

    static member toSeq(om: OrderedMap<'K, 'V>) : seq<'K * 'V> =
      OrderedMap.keys om |> Seq.map (fun k -> k, om.data.[k])

    static member toArray(om: OrderedMap<'K, 'V>) : array<'K * 'V> =
      OrderedMap.keys om |> List.toArray |> Array.map (fun k -> k, om.data.[k])

    // new methods
    static member addIfNotExists (om: OrderedMap<'K, 'V>) (k: 'K) (v: 'V) : Option<OrderedMap<'K, 'V>> =
      if om.data.ContainsKey k then
        None
      else
        Some(OrderedMap.Add om k v)

    static member ofListIfNoDuplicates(kvs: List<'K * 'V>) : Sum<OrderedMap<'K, 'V>, Errors> =
      let duplicateKeys =
        kvs
        |> List.groupBy fst
        |> List.filter (fun (_, vs) -> vs.Length > 1)
        |> List.map fst

      if duplicateKeys.IsEmpty then
        OrderedMap.ofList kvs |> Left
      else
        Errors.Singleton(sprintf "Duplicate keys: %A" duplicateKeys)
        |> Errors.WithPriority ErrorPriority.Medium
        |> Right

    static member mergeSecondAfterFirstIfNoDuplicates
      (om1: OrderedMap<'K, 'V>)
      (om2: OrderedMap<'K, 'V>)
      : Sum<OrderedMap<'K, 'V>, Errors> =
      let conflicts = om2.reverseOrder |> List.filter (fun k -> om1.data.ContainsKey k)

      if conflicts.IsEmpty then
        OrderedMap.mergeSecondAfterFirst om1 om2 |> Left
      else
        let errorMsg = sprintf "Key conflicts during merge: %A" conflicts
        Errors.Singleton errorMsg |> Errors.WithPriority ErrorPriority.Medium |> Right

    static member mergeSecondAfterFirst (om1: OrderedMap<'K, 'V>) (om2: OrderedMap<'K, 'V>) : OrderedMap<'K, 'V> =
      let mergedReverseOrder =
        (om2.reverseOrder |> List.filter (fun k -> not (om1.data.ContainsKey k)))
        @ om1.reverseOrder

      let mergedData =
        om1.data |> Map.fold (fun (acc: Map<'K, 'V>) k v -> acc.Add(k, v)) om2.data

      { reverseOrder = mergedReverseOrder
        data = mergedData }

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
