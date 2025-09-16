namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module RecordTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  let private kindKey = "record"
  let private fieldKey = "record"

  type TypeExpr with
    static member FromJsonRecord(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fun recordFields ->
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

          let record = TypeExpr.Record(fieldTypes)
          let! wrappedRecord = AutomaticSymbolCreation.wrapWithLet (record, fieldTypes |> List.map fst)
          return wrappedRecord
        })

    static member ToJsonRecord(rootToJson: TypeExpr -> JsonValue) : List<TypeExpr * TypeExpr> -> JsonValue =
      fun fields ->
        let fieldPairs =
          fields
          |> Seq.map (fun (fieldKey, fieldType) ->
            let fieldKeyJson = rootToJson fieldKey
            let fieldTypeJson = rootToJson fieldType
            JsonValue.Array [| fieldKeyJson; fieldTypeJson |])

        JsonValue.Array(fieldPairs |> Array.ofSeq) |> Json.kind kindKey fieldKey
