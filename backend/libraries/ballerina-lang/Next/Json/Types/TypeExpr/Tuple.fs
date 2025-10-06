﻿namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TupleTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "tuple"

  type TypeExpr with
    static member FromJsonTuple(fromJsonRoot: TypeExprParser) : TypeExprParser =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun tupleFields ->
        sum {
          let! elements = tupleFields |> JsonValue.AsArray
          let! elementTypes = elements |> Array.map (fun element -> element |> fromJsonRoot) |> sum.All
          return TypeExpr.Tuple(elementTypes)
        })

    static member ToJsonTuple(rootToJson: TypeExpr -> JsonValue) : List<TypeExpr> -> JsonValue =
      List.map rootToJson
      >> List.toArray
      >> JsonValue.Array
      >> Json.discriminator discriminator
