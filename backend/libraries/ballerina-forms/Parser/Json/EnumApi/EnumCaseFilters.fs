namespace Ballerina.DSL.FormEngine.Parser

type internal EnumCaseFilters =
  { Filters: EnumCaseFilter Set }

  static member Empty = { Filters = Set.empty }

[<AutoOpen>]
module EnumCaseFilters =
  open Ballerina.Collections.NonEmptySet
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Patterns
  open Ballerina.Errors
  open FSharp.Data

  let private validateSubjectCases (NonEmptySet existingCases) subjects =
    sum {
      let subjects = subjects |> Seq.map CaseName.Create |> Set.ofSeq

      if Set.isSubset subjects existingCases then
        return!
          subjects
          |> NonEmptySet.TryOfSet
          |> Sum.fromOption (fun () -> "Has to have at least one case defined!" |> Errors.Singleton)
      else
        return!
          $"Does not have {subjects - existingCases} cases defined"
          |> Errors.Singleton
          |> Right
    }

  let private getExistingCases unionType =
    sum {
      let! unionCases = unionType |> ExprType.GetCases

      return!
        unionCases.Keys
        |> NonEmptySet.TryOfSeq
        |> Sum.fromOption (fun () -> $"Type {unionType} has to have at least one case defined" |> Errors.Singleton)
    }

  let private getDuplicates (filters: List<_>) =
    seq {
      for i in 0 .. filters.Length - 2 do
        for j in i + 1 .. filters.Length - 1 do
          let intersection = filters[i].SubjectCases |> intersect filters[j].SubjectCases

          if not (Set.isEmpty intersection) then
            yield intersection
    }
    |> Set.unionMany

  type EnumCaseFilters with
    static member internal Parse (unionType: ExprType) (filtersJson: JsonValue array) =
      sum {
        let! existingCases = unionType |> getExistingCases

        let validateSubjectCases =
          validateSubjectCases existingCases
          >> (sum.WithErrorContext $"...when validating type {unionType} cases")

        let! filters =
          seq {
            for filterJson in filtersJson do
              yield EnumCaseFilter.Parse validateSubjectCases filterJson
          }
          |> sum.All

        let duplicates = filters |> getDuplicates

        if duplicates.IsEmpty then
          return { Filters = filters |> Set.ofList }
        else
          return!
            $"Only a single filter can be applied to a field! Fields in breach: {duplicates |> Set.map _.CaseName}"
            |> Errors.Singleton
            |> Right
      }
