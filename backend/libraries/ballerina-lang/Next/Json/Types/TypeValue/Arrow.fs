namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Arrow =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina
  open Ballerina.Collections.Sum.Operators
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "arrow"

  type TypeValue<'valueExt> with
    static member FromJsonArrow<'valueExt>
      (fromRootJson: JsonValue -> Sum<TypeValue<'valueExt>, Errors<unit>>)
      : JsonValue -> Sum<TypeValue<'valueExt> * TypeValue<'valueExt>, Errors<unit>> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun arrowFields ->
        sum {
          let! arrowFields = arrowFields |> JsonValue.AsRecordMap

          let! param =
            arrowFields
            |> (Map.tryFindWithError "param" "arrow" (fun () -> "param") () >>= fromRootJson)

          let! returnType =
            arrowFields
            |> (Map.tryFindWithError "returnType" "arrow" (fun () -> "returnType") ()
                >>= fromRootJson)

          return param, returnType
        })

    static member ToJsonArrow
      (rootToJson: TypeValue<'valueExt> -> JsonValue)
      : TypeValue<'valueExt> * TypeValue<'valueExt> -> JsonValue =
      fun (param, jsonType) ->
        let param = param |> rootToJson
        let jsonType = jsonType |> rootToJson

        JsonValue.Record([| ("param", param); ("returnType", jsonType) |])
        |> Json.discriminator discriminator
