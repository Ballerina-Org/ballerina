namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module ResolvedTypeIdentifier =
  open Ballerina.StdLib.Json.Sum
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json
  open FSharp.Data
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "id"

  type ResolvedIdentifier with
    static member FromJson =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun nameJson ->
        sum {
          let! assembly, module_, type_, name = nameJson |> JsonValue.AsQuadruple
          let! assembly = assembly |> JsonValue.AsString
          let! module_ = module_ |> JsonValue.AsString

          let! type_ =
            sum.Any2 (type_ |> JsonValue.AsString |> sum.Map Left) (type_ |> JsonValue.AsNull |> sum.Map Right)

          let! name = name |> JsonValue.AsString

          return
            { ResolvedIdentifier.Assembly = assembly
              Module = module_
              Type = type_ |> Sum.toOption
              Name = name }
        })

    static member ToJson: ResolvedIdentifier -> JsonValue =
      fun id ->
        [| id.Assembly |> JsonValue.String
           id.Module |> JsonValue.String
           (match id.Type with
            | Some t -> t |> JsonValue.String
            | None -> JsonValue.Null)
           id.Name |> JsonValue.String |]
        |> JsonValue.Array
        |> Json.discriminator discriminator
