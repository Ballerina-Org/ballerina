namespace Ballerina.Data.Schema

open System
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.DSL.FormEngine.Model
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.Types
open Ballerina.Data.Schema
open Ballerina.Data.Schema.Model
open Ballerina.Data.Spec.Model
open Ballerina.Errors

type LookupValue<'valueExt> =
  { Value: Value<TypeValue<'valueExt>, 'valueExt>
    Typed: TypeValue<'valueExt>
    Lookup:
      {| Name: LookupName
         Descriptor: LookupDescriptor |} }

module Entity =

  let expandLookups
    (entityId: Guid, entityName: EntityName)
    (_renderer: Renderer<_, _>)
    (seeds: SpecData<_, _>)
    (schema: Schema<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>)
    : Sum<Value<TypeValue<'valueExt>, 'valueExt> * LookupValue<'valueExt> list, 'e> =

    let source = seeds.Entities |> Map.find entityName |> Map.find entityId
    //FIXME: use find with errors
    //FIXME: this expands the entity with all matched lookups in the schema: use only those used by the renderer
    sum {
      let targets =
        schema.Lookups
        |> Map.toSeq
        |> Seq.filter (fun (_lookupName, lookup) -> lookup.Source = entityName)
        |> Seq.choose (fun (lookupName, lookup) ->
          schema.Entities
          |> Map.tryFind lookup.Target
          |> Option.map (fun entityDescriptor -> entityDescriptor, lookupName, lookup))
        |> Seq.map (fun (targetEntityDescriptor, lookupName, lookup) ->
          let targets = seeds.Lookups |> Map.find lookupName |> Map.find entityId
          let target = targets |> Set.toList |> List.head //Fixme: respect arity, return all if needed
          let value = seeds.Entities |> Map.find lookup.Target |> Map.find target

          { Value = value
            Typed = targetEntityDescriptor.Type
            Lookup =
              {| Name = lookupName
                 Descriptor = lookup |} })
        |> Seq.toList

      return source, targets
    }

  let insertLookupValues
    (sourceValue: Value<TypeValue<'valueExt>, 'valueExt>)
    (lookupValues: LookupValue<'valueExt> seq)
    : Sum<Value<TypeValue<'valueExt>, 'valueExt>, Errors<unit>> =
    lookupValues
    |> Seq.fold
      (fun acc lookupValue ->
        sum {
          let! sourceValue = acc

          let target =
            Value.enwrapArity lookupValue.Value lookupValue.Typed (lookupValue.Lookup.Descriptor |> _.Forward.Arity)

          return! Value.insert target sourceValue (lookupValue.Lookup.Descriptor |> _.Forward.Path)
        })
      (sum.Return sourceValue)
