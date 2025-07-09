namespace Ballerina.Collections

module Map =
  open Ballerina.Fun

  type Map<'k, 'v when 'k: comparison> with
    static member merge (deduplicate: 'v -> 'v -> 'v) (m2: Map<'k, 'v>) (m1: Map<'k, 'v>) : Map<'k, 'v> =
      let rec merge (deduplicate: 'v -> 'v -> 'v) (m2: Map<'k, 'v>) (m1: list<'k * 'v>) : Map<'k, 'v> =
        match m1 with
        | [] -> m2
        | (k1, v1) :: m1 ->
          match m2 |> Map.tryFind k1 with
          | None -> merge deduplicate m2 m1 |> Map.add k1 v1
          | Some v2 -> merge deduplicate m2 m1 |> Map.add k1 (deduplicate v1 v2)

      merge deduplicate m2 (m1 |> Seq.map (fun kv -> kv.Key, kv.Value) |> List.ofSeq)

    static member mergeMany ((+): 'v -> 'v -> 'v) (maps: seq<Map<'k, 'v>>) : Map<'k, 'v> =
      maps |> Seq.fold (fun m1 m2 -> m1 |> Map.merge (+) m2) Map.empty

    static member upsert (k: 'k) (z: Unit -> 'v) (u: Updater<'v>) (m: Map<'k, 'v>) : Map<'k, 'v> =
      match m |> Map.tryFind k with
      | None -> m |> Map.add k (z ())
      | Some v -> m |> Map.add k (u v)

    static member update k u m =
      match m |> Map.tryFind k with
      | None -> m
      | Some v -> m |> Map.add k (u v)
