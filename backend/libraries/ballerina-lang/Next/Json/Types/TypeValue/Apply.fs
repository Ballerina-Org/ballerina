namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module TypeValueApply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns

  let private kindKey = "apply"
  let private fieldKey = "apply"

  type TypeValue with
    static member FromJsonApply
      (fromRootJson: JsonValue -> Sum<TypeValue, Errors>)
      : JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fun applyFields ->
        sum {
          let! (var, arg) = applyFields |> JsonValue.AsPair
          let! varType = var |> fromRootJson
          let! varType = varType |> TypeValue.AsVar
          let! argType = arg |> fromRootJson
          return TypeValue.Apply(varType, argType)
        })

    static member ToJsonApply(toRootJson: TypeValue -> JsonValue) : TypeVar * TypeValue -> JsonValue =
      fun (var, arg) ->
        let varJson = var |> TypeValue.Var |> toRootJson
        let argJson = arg |> toRootJson
        JsonValue.Array [| varJson; argJson |] |> Json.kind kindKey fieldKey
