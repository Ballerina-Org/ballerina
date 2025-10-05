namespace Ballerina.Data.Spec

open System
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.Data.Schema.Model

module Model =
  type V2Format =
    { Schema: Schema<TypeExpr>
      TypesV2: TypesV2 }

  and TypesV2 = List<string * TypeExpr>

  type SpecName =
    | SpecName of string

    member this.Name =
      let (SpecName name) = this
      name

  type SpecData<'T, 'valueExtension> =
    { Entities: Map<EntityName, Map<Guid, Value<'T, 'valueExtension>>>
      Lookups: Map<LookupName, Map<Guid, Set<Guid>>> }
