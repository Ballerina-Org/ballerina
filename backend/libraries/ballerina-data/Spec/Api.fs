namespace Ballerina.Data.Spec.Api

module Model =
  open Ballerina.Collections.Sum
  open Ballerina.Data.Spec.Model
  open Ballerina.Errors

  type SpecApi =
    { Get: SpecName -> Sum<Spec, Errors>
      Create: Spec -> Config -> Sum<unit, Errors>
      Delete: SpecName -> Sum<Unit, Errors>
      Update: Spec -> Sum<Unit, Errors>
      List: unit -> Sum<SpecName list, Errors> }
