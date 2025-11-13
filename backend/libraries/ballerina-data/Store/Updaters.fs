namespace Ballerina.Data.Store

open Ballerina.Data.Spec.Model

module Updaters =
  type SpecData<'T, 'valueExtension> with
    static member Updaters =
      {| Entities =
          fun u (state: SpecData<'T, 'valueExtension>) ->
            { state with
                Entities = u state.Entities }
         Lookups = fun u (state: SpecData<'T, 'valueExtension>) -> { state with Lookups = u state.Lookups } |}
