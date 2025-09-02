module Ballerina.Data.Tests.Seeds.Schema

open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Types.Model
open Ballerina.Seeds
open NUnit.Framework

[<Test; Ignore("sometimes fails with a child having one parent, needs investigation")>]
let ``Seeds, check arity`` () =

  let c: SeedingContext<TypeValue> =
    { WantedCount = None
      TypeContext =
        { TypeExprEvalState.Empty with
            Bindings = Samples.DispatchPerson.context }
      Generator = Ballerina.Seeds.Fakes.Runner.en ()
      Label = Absent
      InfinitiveVarNamesIndex = 0
      Options = FullStructure }

  let actual =
    sum {
      let! _api, seeds = Runner.createApi c Samples.DispatchPerson.familySchema Map.empty
      return seeds ()
    }

  match actual with
  | Sum.Left seeds ->

    let admires =
      seeds.Lookups
      |> Map.toList
      |> List.find (fun (name, _) -> name = "Admire")
      |> snd
      |> Map.toList
      |> List.map (snd >> Set.count)

    let arities = admires |> List.forall (fun items -> items >= 2 && items <= 2)

    Assert.That(arities, Is.True, "Children of Admire should have arity 2, always have two parents")
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
