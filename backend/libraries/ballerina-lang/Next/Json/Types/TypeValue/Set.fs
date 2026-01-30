namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Set =
  open FSharp.Data
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina
  open Ballerina.Collections.Sum.Operators
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json.Keys


  let private discriminator = "set"

  type TypeValue<'valueExt> with
    static member FromJsonSet
      (fromRootJson: JsonValue -> Sum<TypeValue<'valueExt>, Errors<unit>>)
      : JsonValue -> Sum<TypeValue<'valueExt>, Errors<unit>> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun elementType ->
        sum {
          let! elementType = elementType |> fromRootJson
          return elementType
        })

    static member ToJsonSet(toRootJson: TypeValue<'valueExt> -> JsonValue) : TypeValue<'valueExt> -> JsonValue =
      toRootJson >> Json.discriminator discriminator
