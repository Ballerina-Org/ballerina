namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module TypeValueApply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Types.Json

  let private discriminator = "application"

  let rec private fromJsonSymbolicTypeApplication
    (fromRootJson: JsonValue -> Sum<TypeValue<'valueExt>, Errors>)
    (json: JsonValue)
    : Sum<SymbolicTypeApplication<'valueExt>, Errors> =
    sum {
      let! f, a = json |> JsonValue.AsPair
      let! a = a |> fromRootJson

      return!
        sum.Any2
          (sum {
            let! id = f |> Identifier.FromJson
            return SymbolicTypeApplication.Lookup(id, a)
          })
          (sum {
            let! functionApp = f |> fromJsonSymbolicTypeApplication fromRootJson
            return SymbolicTypeApplication.Application(functionApp, a)
          })
    }

  let rec private toJsonSymbolicTypeApplication
    (rootToJson: TypeValue<'valueExt> -> JsonValue)
    (app: SymbolicTypeApplication<'valueExt>)
    : JsonValue =
    match app with
    | SymbolicTypeApplication.Lookup(f, a) -> JsonValue.Array [| Identifier.ToJson f; rootToJson a |]
    | SymbolicTypeApplication.Application(f, a) ->
      JsonValue.Array [| toJsonSymbolicTypeApplication rootToJson f; rootToJson a |]

  type TypeValue<'valueExt> with
    static member FromJsonApplication
      (fromRootJson: JsonValue -> Sum<TypeValue<'valueExt>, Errors>)
      : JsonValue -> Sum<SymbolicTypeApplication<'valueExt>, Errors> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun applyFields ->
        sum {
          let! symbolicApp = applyFields |> fromJsonSymbolicTypeApplication fromRootJson
          return symbolicApp
        })

    static member ToJsonApplication
      (rootToJson: TypeValue<'valueExt> -> JsonValue)
      : SymbolicTypeApplication<'valueExt> -> JsonValue =
      toJsonSymbolicTypeApplication rootToJson >> Json.discriminator discriminator
