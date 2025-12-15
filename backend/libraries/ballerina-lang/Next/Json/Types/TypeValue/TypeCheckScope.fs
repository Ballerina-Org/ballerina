namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Json.Json

[<AutoOpen>]
module TypeCheckScope =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.StdLib.Json.Patterns

  let private discriminator = "typeCheckScope"

  type TypeCheckScope with
    static member FromJson: JsonValue -> Sum<TypeCheckScope, Errors> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun scope ->
        sum {
          let! fields = scope |> JsonValue.AsRecord
          let fields = Map.ofArray fields


          let! ty = fields |> Map.tryFindWithError "type" "TypeCheckScope" "type"
          let ty = JsonValue.AsString ty |> Sum.toOption
          let! md = fields |> Map.tryFindWithError "module" "TypeCheckScope" "module"
          let! md = JsonValue.AsString md
          let! assembly = fields |> Map.tryFindWithError "assembly" "TypeCheckScope" "module"
          let! assembly = JsonValue.AsString assembly

          return
            { Assembly = assembly
              Module = md
              Type = ty }
        })

    static member ToJson: TypeCheckScope -> JsonValue =
      fun scope ->
        let assembly = JsonValue.String scope.Assembly
        let md = JsonValue.String scope.Module

        let ty =
          match scope.Type with
          | Some t -> JsonValue.String t
          | None -> JsonValue.Null

        JsonValue.Record [| "assembly", assembly; "module", md; "type", ty |]
        |> Json.discriminator discriminator
