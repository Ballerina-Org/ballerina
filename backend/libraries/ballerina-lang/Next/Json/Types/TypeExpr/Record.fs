namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module RecordTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  type TypeExpr with
    static member FromJsonRecord(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField "record" "record" (fun recordFields ->
        sum {
          let! fields = recordFields |> JsonValue.AsArray

          let! fieldTypes =
            fields
            |> Array.map (fun field ->
              sum {
                let! (fieldKey, fieldValue) = field |> JsonValue.AsPair
                let! fieldType = fromJsonRoot fieldValue
                let! fieldKey = fromJsonRoot fieldKey
                return (fieldKey, fieldType)
              })
            |> sum.All

          return TypeExpr.Record(fieldTypes)
        })

    static member ToJsonRecord(rootToJson: TypeExpr -> JsonValue) : List<TypeExpr * TypeExpr> -> JsonValue =
      fun fields ->
        let fieldPairs =
          fields
          |> Seq.map (fun (fieldKey, fieldType) ->
            let fieldKeyJson = rootToJson fieldKey
            let fieldTypeJson = rootToJson fieldType
            JsonValue.Array [| fieldKeyJson; fieldTypeJson |])

        JsonValue.Array(fieldPairs |> Array.ofSeq) |> Json.kind "record" "record"
