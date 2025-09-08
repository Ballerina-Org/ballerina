namespace Ballerina.Data.Seeds

open System
open System.Collections.Generic
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Model
open Ballerina.Data.Seeds.Model
open Ballerina.Errors
open Ballerina.DSL.Next.Types.Model
open Ballerina.Data
open Ballerina.Data.Arity.Model
open Ballerina.Reader.WithError
open Ballerina.Seeds

module Updaters =
  type Seeds<'T> with
    static member Updaters =
      {| Entities = fun u (s: Seeds<'T>) -> { s with Seeds.Entities = u s.Entities }
         Lookups = fun u (s: Seeds<'T>) -> { s with Seeds.Lookups = u s.Lookups } |}

    static member Print(db: Seeds<'T>) =
      let entities () =
        db.Entities
        |> Map.toList
        |> List.groupBy fst
        |> List.map (fun (name, values) ->
          values
          |> List.indexed
          |> List.takeWhile (fun (i, _) -> i < 5)
          |> List.map (fun (_k, (_, v)) ->
            v
            |> Map.toList
            |> List.indexed
            |> List.takeWhile (fun (i, _) -> i < 5)
            |> List.map (fun (k, (id, _v)) -> $"{name} - {k + 1}: {id}")))

      let lookups () =
        db.Lookups
        |> Map.toList
        |> List.groupBy fst
        |> List.map (fun (_name, values) ->
          values
          |> List.indexed
          |> List.takeWhile (fun (i, _) -> i < 5)
          |> List.map (fun (_k, (_, v)) ->
            v
            |> Map.toList
            |> List.indexed
            |> List.takeWhile (fun (i, _) -> i < 5)
            |> List.map (fun (_k, (id, vs)) ->
              let ids = vs |> Set.toArray |> Seq.indexed |> Seq.takeWhile (fun (i, _) -> i < 5)

              ids
              |> Seq.iter (fun (_, targetId) ->
                let sourceName =
                  db.Entities
                  |> Map.tryFindKey (fun _ v -> v |> Map.containsKey id)
                  |> Option.defaultValue "Unknown"

                let targetName =
                  db.Entities
                  |> Map.tryFindKey (fun _ v -> v |> Map.containsKey targetId)
                  |> Option.defaultValue "Unknown"

                printfn $"{sourceName} ({id}) --- {_name} --> {targetName} ({targetId})"))))

      printfn $"{entities ()}"
      printfn $"{lookups ()}"

module Arity =
  let seedOneToOne (sources: 's list) (targets: 't list) : ('s * 't list) list =
    List.zip sources targets |> List.map (fun (s, t) -> s, [ t ])

  type CountedTarget<'t> = { Item: 't; mutable Count: int }

  let seedBidirectional<'s, 't when 't: comparison>
    (rng: Random)
    (sourceArity: LookupArity)
    (targetArity: LookupArity)
    (sources: 's list)
    (targets: 't list)
    : ('s * 't list) list =

    let targetCounts = Dictionary<'t, int>()

    for t in targets do
      targetCounts[t] <- 0

    let targetMax = targetArity.Max |> Option.defaultValue Int32.MaxValue
    let sourceMin = sourceArity.Min |> Option.defaultValue 0
    let sourceMax = sourceArity.Max |> Option.defaultValue Int32.MaxValue

    sources
    |> List.choose (fun source ->
      let availableTargets =
        targetCounts
        |> Seq.filter (fun kv -> kv.Value < targetMax)
        |> Seq.map (fun kv -> kv.Key)
        |> Seq.toList

      if availableTargets.Length < sourceMin then
        None
      else
        let upperBound = min sourceMax availableTargets.Length
        let count = rng.Next(sourceMin, upperBound + 1)

        let selected =
          availableTargets |> List.sortBy (fun _ -> rng.Next()) |> List.take count

        for t in selected do
          targetCounts[t] <- targetCounts[t] + 1

        Some(source, selected))

module EntityDescriptor =
  let seed
    (e: EntityDescriptor<TypeValue>)
    : Reader<Map<Guid, Value<TypeValue, Unit>>, SeedingContext<TypeValue>, Errors> =
    reader {
      let! ctx = reader.GetContext()
      let itemsToSeed = ctx.WantedCount |> Option.defaultValue (Random().Next() % 50 + 50)

      return!
        seq {
          for _ in 0 .. itemsToSeed - 1 do
            let value = Traverser.seed e.Type
            yield Guid.CreateVersion7(), value
        }
        |> Seq.map (fun (guid, value) ->
          reader {
            let! v = value
            return guid, v
          })
        |> reader.All
        |> reader.Map Map.ofList
    }

module LookupDescriptor =
  let seed<'T>
    (entities: Map<string, Map<Guid, Value<'T, Unit>>>)
    (descriptor: LookupDescriptor)
    : Sum<Map<Guid, Set<Guid>>, Errors> =

    sum {
      let! sources =
        entities
        |> Map.tryFindWithError descriptor.Source descriptor.Source "while seed lookup source descriptor"

      let! targets =
        entities
        |> Map.tryFindWithError descriptor.Target descriptor.Target "while seed lookup source descriptor"

      let sourceKeys = sources |> Map.toList |> List.map fst
      let targetKeys = targets |> Map.toList |> List.map fst

      let sourceArity = descriptor.Forward.Arity
      let targetArityOption = descriptor.Backward |> Option.map (snd >> _.Arity)

      let seededPairs =
        match targetArityOption with
        | Some targetArity -> Arity.seedBidirectional (Random()) sourceArity targetArity sourceKeys targetKeys
        | None -> Arity.seedOneToOne sourceKeys targetKeys

      return
        seededPairs
        |> Seq.groupBy fst
        |> Seq.map (fun (k, group) -> k, group |> Seq.collect snd |> Set.ofSeq)
        |> Map.ofSeq
    }

  let tryFlip
    (descriptor: LookupDescriptor)
    (lookup: Map<Guid, Set<Guid>>)
    : Sum<Option<string * Map<Guid, Set<Guid>>>, Errors> =
    sum {
      match descriptor.Backward with
      | None -> return None
      | Some(name, _) ->
        let flipped =
          lookup
          |> Map.toSeq
          |> Seq.collect (fun (k, values) -> values |> Seq.map (fun v -> v, k))
          |> Seq.groupBy fst
          |> Seq.map (fun (k, group) ->
            let values = group |> Seq.map snd |> Set.ofSeq
            k, values)
          |> Map.ofSeq

        return Some(name, flipped)
    }
