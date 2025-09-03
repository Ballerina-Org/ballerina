namespace Ballerina.Data.Seeds

open System
open Ballerina.DSL.Next.Terms.Model

module Model =

  type Seeds<'T> =
    { Entities: Map<string, Map<Guid, Value<'T>>>
      Lookups: Map<string, Map<Guid, Set<Guid>>> }
