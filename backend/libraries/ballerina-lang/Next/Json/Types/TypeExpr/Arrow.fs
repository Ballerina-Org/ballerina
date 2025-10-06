﻿namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module ArrowTypeExpr =
  open FSharp.Data
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "arrow"

  type TypeExpr with
    static member FromJsonArrow(fromJsonRoot: JsonValue -> Sum<TypeExpr, Errors>) =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun arrowFields ->
        sum {
          let! (param, returnType) = arrowFields |> JsonValue.AsPair
          let! param = param |> fromJsonRoot
          let! returnType = returnType |> fromJsonRoot

          return TypeExpr.Arrow(param, returnType)
        })

    static member ToJsonArrow(rootToJson: TypeExpr -> JsonValue) : TypeExpr * TypeExpr -> JsonValue =
      fun (param, returnType) ->
        let paramJson = rootToJson param
        let returnTypeJson = rootToJson returnType

        JsonValue.Array [| paramJson; returnTypeJson |]
        |> Json.discriminator discriminator
