﻿namespace Ballerina.Data.Seeds

open System
open System.Collections.Generic
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Terms.Model
open Ballerina.Data.Schema.Model
open Ballerina.LocalizedErrors
open Ballerina.DSL.Next.Types.Model
open Ballerina.Data.Arity.Model
open Ballerina.Seeds
open Ballerina.State.WithError

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
    : State<Map<Guid, Value<TypeValue, ValueExt>>, SeedingContext, SeedingState, Errors> =
    state {
      let! ctx = state.GetContext()
      let itemsToSeed = ctx.WantedCount |> Option.defaultValue (Random().Next() % 50 + 50)

      return!
        seq {
          for _ in 0 .. itemsToSeed - 1 do
            let value = Traverser.seed e.Type
            yield Guid.CreateVersion7(), value
        }
        |> Seq.map (fun (guid, value) ->
          state {
            let! v = value
            return guid, v
          })
        |> state.All
        |> state.Map Map.ofSeq
    }

module LookupDescriptor =
  let seed
    (entities: Map<EntityName, Map<Guid, Value<TypeValue, ValueExt>>>)
    (descriptor: LookupDescriptor)
    : Sum<Map<Guid, Set<Guid>>, Errors> =

    sum {
      let! sources =
        entities
        |> Map.tryFindWithError
          descriptor.Source
          descriptor.Source.EntityName
          "while seed lookup source descriptor"
          Location.Unknown

      let! targets =
        entities
        |> Map.tryFindWithError
          descriptor.Target
          descriptor.Target.EntityName
          "while seed lookup source descriptor"
          Location.Unknown

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
    : Sum<Option<LookupName * Map<Guid, Set<Guid>>>, Errors> =
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
