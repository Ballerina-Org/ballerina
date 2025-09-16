namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module UnionTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  let private kindKey = "union"
  let private fieldKey = "union"

  type TypeExpr with
    static member FromJsonUnion(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fun unionFields ->
        sum {
          let! cases = unionFields |> JsonValue.AsArray

          let! caseTypes =
            cases
            |> Array.map (fun case ->
              sum {
                let! (caseKey, caseValue) = case |> JsonValue.AsPair
                let! caseType = fromJsonRoot caseValue
                let! caseKey = fromJsonRoot caseKey
                return (caseKey, caseType)
              })
            |> sum.All

          let union = TypeExpr.Union(caseTypes)
          let! wrappedUnion = AutomaticSymbolCreation.wrapWithLet (union, caseTypes |> List.map fst)
          return wrappedUnion
        })

    static member ToJsonUnion(rootToJson: TypeExpr -> JsonValue) : List<TypeExpr * TypeExpr> -> JsonValue =
      fun cases ->
        let caseTypes =
          cases
          |> Seq.map (fun (caseKey, caseType) ->
            let caseKeyJson = rootToJson caseKey
            let caseTypeJson = rootToJson caseType
            JsonValue.Array [| caseKeyJson; caseTypeJson |])

        JsonValue.Array(caseTypes |> Array.ofSeq) |> Json.kind kindKey fieldKey
