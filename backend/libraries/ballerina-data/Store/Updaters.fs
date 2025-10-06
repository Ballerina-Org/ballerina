namespace Ballerina.Data.Store

open Ballerina.Data.Spec.Model

module Updaters =
  type SpecData<'T, 'valueExtension> with
    static member Updaters =
      {| Entities =
          fun u (s: SpecData<'T, 'valueExtension>) ->
            { s with
                SpecData.Entities = u s.Entities }
         Lookups =
          fun u (s: SpecData<'T, 'valueExtension>) ->
            { s with
                SpecData.Lookups = u s.Lookups } |}
