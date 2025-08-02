namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Arrow =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Collections.Sum.Operators

  type TypeValue with
    static member FromJsonArrow
      (fromRootJson: JsonValue -> Sum<TypeValue, Errors>)
      : JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "arrow" "arrow" (fun arrowFields ->
        sum {
          let! arrowFields = arrowFields |> JsonValue.AsRecordMap

          let! param = arrowFields |> (Map.tryFindWithError "param" "arrow" "param" >>= fromRootJson)

          let! returnType =
            arrowFields
            |> (Map.tryFindWithError "returnType" "arrow" "returnType" >>= fromRootJson)

          return TypeValue.Arrow(param, returnType)
        })

    static member ToJsonArrow(rootToJson: TypeValue -> JsonValue) : TypeValue * TypeValue -> JsonValue =
      fun (param, jsonType) ->
        let param = param |> rootToJson
        let jsonType = jsonType |> rootToJson

        JsonValue.Record([| ("param", param); ("returnType", jsonType) |])
        |> Json.kind "arrow" "arrow"
