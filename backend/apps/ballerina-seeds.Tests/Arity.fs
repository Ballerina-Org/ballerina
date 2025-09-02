module Ballerina.Data.Tests.Seeds.Arity

open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Types.Model
open Ballerina.Data.Model
open Ballerina.Seeds.Samples.DispatchPerson
open Ballerina.Seeds
open NUnit.Framework
open Ballerina.Seeds.Fakes

let ignoredEntityMethods: Set<EntityMethod> = Set.empty
let ignoredLookupMethods: Set<LookupMethod> = Set.empty

// [<Test>]
// let ``ArityLookups - backward arity has no duplications`` () =
//
//   let ctx: SeedingContext<TypeValue> =
//     { WantedCount = None
//       TypeContext =
//         { TypeExprEvalContext.Empty with
//             Bindings = context }
//       Generator = Runner.en ()
//       Label = Absent
//       InfinitiveVarNamesIndex = 0
//       Options = FullStructure }
//
//   let actual =
//     sum {
//       let specs = Map.empty
//       let! _api, db = Runner.createApi ctx familySchema specs
//       return db ()
//     }
//
//   match actual with
//   | Sum.Left seeds ->
//     let backward = seeds.Lookups |> Map.find "Admire"
//
//     let nrOfWeirdChildren =
//       backward |> Map.toList |> List.filter (fun (_k, v) -> Set.count v <> 2)
//
//     Assert.That(nrOfWeirdChildren.Length, Is.EqualTo 0, "There should not be any child with more than two parents.")
//   | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
