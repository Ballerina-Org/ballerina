namespace Ballerina.Data.Spec

open System
open FSharp.Data
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.Data.Schema.Model

module Model =
  type TypesV2 = List<string * TypeExpr>

  type SpecFormat =
    { Schema: Schema<TypeExpr>
      TypesV2: TypesV2 }

  type SpecName =
    | SpecName of string

    member this.Name =
      let (SpecName name) = this
      name

  type Launcher = { LauncherName: string }

  type Spec = { Name: SpecName; Body: SpecFormat }

  type Config = JsonValue

  type SpecData<'T, 'valueExtension> =
    { Entities: Map<string, Map<Guid, Value<'T, 'valueExtension>>>
      Lookups: Map<string, Map<Guid, Set<Guid>>> }
