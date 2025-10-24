namespace Ballerina.Cat.Collections


module OrderedMap =
  (*
    Insertion order preserving map.

    Complexity:
    - insert and lookup: same as Map
    *)
  open Ballerina.Collections.Sum

  type OrderedMap<'K, 'V when 'K: comparison> =
    // private
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

    static member add (k: 'K) (v: 'V) (om: OrderedMap<'K, 'V>) : OrderedMap<'K, 'V> = OrderedMap<'K, 'V>.Add om k v

    static member remove (k: 'K) (om: OrderedMap<'K, 'V>) : OrderedMap<'K, 'V> =
      if om.data.ContainsKey k |> not then
        { reverseOrder = om.reverseOrder |> List.filter (fun key -> key <> k)
          data = om.data.Remove k }
      else
        om

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

    static member mergeSecondAfterFirst (om1: OrderedMap<'K, 'V>) (om2: OrderedMap<'K, 'V>) : OrderedMap<'K, 'V> =
      let mergedReverseOrder =
        (om2.reverseOrder |> List.filter (fun k -> not (om1.data.ContainsKey k)))
        @ om1.reverseOrder

      let mergedData =
        om1.data |> Map.fold (fun (acc: Map<'K, 'V>) k v -> acc.Add(k, v)) om2.data

      { reverseOrder = mergedReverseOrder
        data = mergedData }
