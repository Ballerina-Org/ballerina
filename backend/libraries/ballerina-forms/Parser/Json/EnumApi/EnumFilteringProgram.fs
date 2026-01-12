namespace Ballerina.DSL.FormEngine.Parser

open System
open System.Security.Cryptography
open Ballerina.Collections.NonEmptySet
open Ballerina.Collections.Sum
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.Expr.Types.Patterns
open Ballerina.DSL.FormEngine.Model
open Ballerina.DSL.Next.Runners
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Types
open System.Text
open Ballerina.Errors
open Ballerina.StdLib

type private Value = Value<TypeValue<ValueExt>, ValueExt>

module EnumFilteringProgram =
  [<Literal>]
  let UserContextTypeName = "UserContext"

  [<Literal>]
  let CasesTypeName = "CasesToFilter"

  [<Literal>]
  let ERPTypeName = "ERP"

  let private buildUnionCase (caseName: CaseName) = $"| {caseName.CaseName} of ()"

  let private buildUnionType (typeName: string) (NonEmptySet cases) =
    let sb = StringBuilder()
    sb.AppendLine $"type {typeName} =" |> ignore

    for case in cases do
      sb.AppendLine(buildUnionCase case) |> ignore

    sb.AppendLine()

  let private getERPCases (context: TypeContext) =
    context.TryFind ERPTypeName
    |> Sum.fromOption (fun () -> Errors.Singleton $"Type named {ERPTypeName} has to be defined!")
    |> Sum.bind (fun { Type = exprType } -> ExprType.AsUnion exprType)
    |> Sum.map _.Keys
    |> Sum.bind (fun cases ->
      NonEmptySet.TryOfSeq cases
      |> Sum.fromOption (fun () -> Errors.Singleton $"{ERPTypeName} union has to have at least one case defined!"))

  let private contextDefinition erpCases =
    StringBuilder()
      .Append(buildUnionType ERPTypeName erpCases)
      .Append(
        $"in type {UserContextTypeName} = {{
  IsSuperAdmin: bool;
  TenantERP: ERP;
}}

in ()"
      )
      .ToString()

  let private buildEnumFilteringProgram filters =
    let sb = StringBuilder()
    let cases = filters |> Seq.map _.SubjectCases |> unionMany
    sb.Append(buildUnionType CasesTypeName cases) |> ignore

    sb.AppendLine
      $"in let filteringLogic = fun (context:{UserContextTypeName}) -> fun (field:{CasesTypeName}) ->
(match field with"
    |> ignore

    for filter in filters do
      for subject in filter.SubjectCases do
        sb.AppendLine $"| {subject.CaseName} (_ -> {filter.Body})" |> ignore

    sb.AppendLine "| (* -> true))" |> ignore

    sb.AppendLine
      $"in fun (context:{UserContextTypeName}) ->
fun (fields:List [{CasesTypeName}]) ->
List::filter[{CasesTypeName}]
  (filteringLogic context)
  fields"
    |> ignore

    sb.ToString()

  let private buildCache =
    let languageContext = snd stdExtensions
    hardDriveCache (languageContext.TypeCheckContext, languageContext.TypeCheckState)

  let private getChecksum (input: string) =
    input |> Encoding.UTF8.GetBytes |> SHA256.HashData |> Convert.ToHexString

  let private getCommonFile context =
    sum {
      let! erpCases = getERPCases context
      let contextDefinition = contextDefinition erpCases

      return
        { FileName = { Path = "enum-filtering-common.bl" }
          Content = fun () -> contextDefinition
          Checksum = { Value = getChecksum contextDefinition } }
    }

  let private getCommonFileMem = memoize getCommonFile

  let private filterExpr enumName context enumCaseFilters =
    let filterProgram = buildEnumFilteringProgram enumCaseFilters

    sum {
      let! commonFile = getCommonFileMem context

      let project =
        { Files =
            [ commonFile
              { FileName = { Path = $"{enumName}-filter.bl" }
                Content = fun () -> filterProgram
                Checksum = { Value = getChecksum filterProgram } } ] }

      let buildResult = ProjectBuildConfiguration.BuildCached buildCache project

      match buildResult with
      | Right errors -> return! $"Enum filtering build errors:\n{errors}" |> Errors.Singleton |> Right
      | Left([], _, _) ->
        return!
          "Tried to compile enum filtering but no source files were found"
          |> Errors.Singleton
          |> Right
      | Left(exprs, _, st) -> return EnumCaseFilterExpr.Filter(exprs, st)
    }

  let internal buildFilteringExpr enumName { Filters = filters } typeContext =
    if filters.IsEmpty then
      Left EnumCaseFilterExpr.PassAll
    else
      filterExpr enumName typeContext filters
