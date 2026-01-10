namespace Ballerina.DSL.FormEngine.Parser

open Ballerina.Collections.NonEmptySet
open Ballerina.DSL.Expr.Model

type internal EnumCaseFilter =
  { Body: string
    SubjectCases: CaseName NonEmptySet }

[<AutoOpen>]
module EnumCaseFilter =
  open Ballerina.DSL.Parser.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Patterns
  open FSharp.Data

  let private getFilterBody filter =
    sum {
      let! fieldValue = filter |> sum.TryFindField "Expr"
      return! fieldValue |> JsonValue.AsString
    }

  let private getSubjects validateCaseNames filter =
    sum {
      let! fieldValue = filter |> sum.TryFindField "Fields"
      let! jsonArray = JsonValue.AsArray fieldValue
      let! subjects = jsonArray |> (Array.map JsonValue.AsString >> Sum.All)
      return! validateCaseNames subjects
    }

  type EnumCaseFilter with
    static member internal Parse validateCaseNames filterJson =
      sum {
        let! filter = filterJson |> JsonValue.AsRecord
        let! body = getFilterBody filter
        let! subjects = filter |> getSubjects validateCaseNames

        { Body = body; SubjectCases = subjects }
      }
