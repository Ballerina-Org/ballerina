namespace Ballerina.DSL.Next.Types.Json

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
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "sum"

  type TypeValue<'valueExt> with
    static member FromJsonSum
      (fromRootJson: JsonValue -> Sum<TypeValue<'valueExt>, Errors>)
      : JsonValue -> Sum<List<TypeValue<'valueExt>>, Errors> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun sumFields ->
        sum {
          let! cases = sumFields |> JsonValue.AsArray
          let! caseTypes = cases |> Array.map (fun case -> case |> fromRootJson) |> sum.All
          return caseTypes
        })

    static member ToJsonSum(rootToJson: TypeValue<'valueExt> -> JsonValue) : List<TypeValue<'valueExt>> -> JsonValue =
      List.toArray
      >> Array.map rootToJson
      >> JsonValue.Array
      >> Json.discriminator discriminator
