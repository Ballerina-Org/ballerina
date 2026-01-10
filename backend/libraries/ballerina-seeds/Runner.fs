namespace Ballerina.Seeds

open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Types.Model
open Ballerina.Data.Schema.Model
open Ballerina.Data.Seeds

open Ballerina.Data.Spec.Model
open Ballerina.LocalizedErrors
open Ballerina.Reader.WithError
open Ballerina.Seeds

open Ballerina.State.WithError

module Runner =
  let _extensions, languageContext = stdExtensions

  let seed
    (schema: Schema<TypeValue<ValueExt>, ResolvedIdentifier, ValueExt>)
    : Reader<SpecData<TypeValue<ValueExt>, ValueExt>, SeedingContext, Errors> =
    reader {
      let! ctx = reader.GetContext()

      let! entities, _ =
        schema.Entities
        |> Map.map EntityDescriptor.seed
        |> state.AllMap
        |> State.Run(ctx, SeedingState.Default(languageContext.TypeCheckState))
        |> reader.OfSum
        |> reader.MapError fst


      let! lookups =
        schema.Lookups
        |> Map.map (fun _k -> LookupDescriptor.seed entities)
        |> Map.values
        |> Seq.toList
        |> sum.All
        |> reader.OfSum

      let lookupByKey =
        schema.Lookups
        |> Map.toList
        |> List.zip (Map.keys schema.Lookups |> Seq.toList)
        |> List.mapi (fun i (k, _) -> k, lookups[i])

      let! backwardsLookups =
        lookupByKey
        |> Seq.map (fun (k, lookup) ->
          sum {
            let descriptor = schema.Lookups[k]
            let! backwards = LookupDescriptor.tryFlip descriptor lookup

            match backwards with
            | Some(name, backwards) -> seq { name, backwards }
            | None -> Seq.empty
          })
        |> sum.All
        |> sum.Map(Seq.collect id)
        |> reader.OfSum

      let allLookups = lookupByKey |> Seq.append backwardsLookups |> Map.ofSeq

      return
        { Entities = entities
          Lookups = allLookups }
    }
