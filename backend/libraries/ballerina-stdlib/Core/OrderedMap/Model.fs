namespace Ballerina.StdLib


module OrderPreservingMap =
  (*
    Insertion order preserving map.

    Complexity:
    - insert and lookup: same as Map
    *)
  open Ballerina.Errors
  open Ballerina.Collections.Sum

  type OrderedMap<'K, 'V when 'K: comparison> =
    private
      { reverseOrder: List<'K>
        data: Map<'K, 'V> }

    // same as Map
    static member empty: OrderedMap<'K, 'V> = { reverseOrder = []; data = Map.empty }
    static member IsEmpty(om: OrderedMap<'K, 'V>) = om.data.IsEmpty
    static member Count(om: OrderedMap<'K, 'V>) = om.data.Count

    static member ContainsKey (om: OrderedMap<'K, 'V>) (k: 'K) = om.data.ContainsKey k
    static member TryFind (om: OrderedMap<'K, 'V>) (k: 'K) = om.data.TryFind k

    static member Keys(om: OrderedMap<'K, 'V>) : List<'K> = om.reverseOrder |> List.rev

    static member Values(om: OrderedMap<'K, 'V>) : List<'V> =
      OrderedMap.Keys om |> List.map (fun k -> om.data.[k])

    static member map (f: 'K -> 'V -> 'U) (om: OrderedMap<'K, 'V>) : OrderedMap<'K, 'U> =
      { reverseOrder = OrderedMap.Keys om |> List.rev
        data = om.data |> Map.map f }

    static member Add (om: OrderedMap<'K, 'V>) (k: 'K) (v: 'V) : OrderedMap<'K, 'V> =
      if om.data.ContainsKey k then
        { om with data = om.data.Add(k, v) }
      else
        { reverseOrder = k :: om.reverseOrder
          data = om.data.Add(k, v) }

    // new methods
    static member AddIfNotExists (om: OrderedMap<'K, 'V>) (k: 'K) (v: 'V) : Option<OrderedMap<'K, 'V>> =
      if om.data.ContainsKey k then
        None
      else
        Some(OrderedMap.Add om k v)

    static member toList(om: OrderedMap<'K, 'V>) : List<'K * 'V> =
      OrderedMap.Keys om |> List.map (fun k -> k, om.data.[k])

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

    static member ofList(kvs: List<'K * 'V>) : OrderedMap<'K, 'V> =
      List.fold (fun acc (k, v) -> OrderedMap.Add acc k v) OrderedMap.empty kvs

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
