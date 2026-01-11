namespace Ballerina.DSL.FormEngine.Parser

[<AutoOpen>]
module internal EnumApi =
  open Ballerina.DSL.Parser.Patterns
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Parser.ExprType
  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.Expr.Types.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Patterns
  open FSharp.Data
  open Ballerina.DSL.FormEngine.Parser.EnumFilteringProgram

  let private getUnderlyingUnion valueFieldName context (enumName: string) enumTypeRef =
    sum {
      let! enumType = ExprType.ResolveLookup context enumTypeRef
      let! fields = ExprType.GetFields enumType

      match fields with
      | [ (value, ExprType.LookupType underlyingUnion) ] when value = valueFieldName -> return underlyingUnion
      | _ ->
        return!
          $$"""Error: invalid enum reference type passed to enum '{{enumName}}'. Expected { {{valueFieldName}}:ENUM }, found {{fields}}."""
          |> Errors.Singleton
          |> Right
    }

  type EnumApi with
    static member ParsePlain<'ExprExtension, 'ValueExtension> valueFieldName context enumName enumTypeJson =
      sum {
        let! enumType = ExprType.Parse enumTypeJson
        let! enumTypeId = enumType |> ExprType.AsLookupId
        let! underlyingUnion = enumType |> getUnderlyingUnion valueFieldName context enumName

        return
          { EnumApi.TypeId = enumTypeId
            EnumName = enumName
            UnderlyingEnum = underlyingUnion
            Filter = EnumCaseFilterExpr.PassAll }
      }

    static member ParseWithFilters<'ExprExtension, 'ValueExtension> valueFieldName context enumName enumFiltersJson =
      sum {
        let! enumFiltersJson = enumFiltersJson |> JsonValue.AsRecord
        let! enumRefTypeNameJson = enumFiltersJson |> sum.TryFindField "TypeRef"
        let! filtersJson = enumFiltersJson |> sum.TryFindField "Filters"
        let! filters = filtersJson |> JsonValue.AsArray

        let! refType = ExprType.Parse enumRefTypeNameJson
        let! enumTypeId = refType |> ExprType.AsLookupId
        let! underlyingUnion = refType |> getUnderlyingUnion valueFieldName context enumName
        let! unionType = ExprType.Find context underlyingUnion
        let! filters = EnumCaseFilters.Parse unionType filters
        let! filteringExpr = buildFilteringExpr enumName filters context

        return
          { EnumApi.TypeId = enumTypeId
            EnumName = enumName
            UnderlyingEnum = underlyingUnion
            Filter = filteringExpr }
      }
