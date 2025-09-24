﻿namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Set =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns


  let private kindKey = "set"
  let private fieldKey = "set"

  type TypeValue with
    static member FromJsonSet(fromRootJson: JsonValue -> Sum<TypeValue, Errors>) : JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fun elementType ->
        sum {
          let! elementType = elementType |> fromRootJson
          return TypeValue.CreateSet(elementType) // FIXME: origin should be serialized and parsed
        })

    static member ToJsonSet(toRootJson: TypeValue -> JsonValue) : TypeValue -> JsonValue =
      toRootJson >> Json.kind kindKey fieldKey
