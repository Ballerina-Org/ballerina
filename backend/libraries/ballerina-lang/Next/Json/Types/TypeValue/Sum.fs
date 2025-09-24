﻿namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Sum =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns

  let private kindKey = "sum"
  let private fieldKey = "sum"

  type TypeValue with
    static member FromJsonSum(fromRootJson: JsonValue -> Sum<TypeValue, Errors>) : JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fun sumFields ->
        sum {
          let! cases = sumFields |> JsonValue.AsArray
          let! caseTypes = cases |> Array.map (fun case -> case |> fromRootJson) |> sum.All
          return TypeValue.CreateSum(caseTypes) // FIXME: origin should be serialized and parsed
        })

    static member ToJsonSum(rootToJson: TypeValue -> JsonValue) : List<TypeValue> -> JsonValue =
      List.toArray
      >> Array.map rootToJson
      >> JsonValue.Array
      >> Json.kind kindKey fieldKey
