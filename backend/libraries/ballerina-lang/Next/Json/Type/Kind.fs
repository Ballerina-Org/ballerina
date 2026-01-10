namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Kind =
  open Ballerina.StdLib.Json.Sum
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open FSharp.Data
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "arrow"
  let kindKey = "kind"

  type Kind with
    static member private FromJsonSymbol: JsonValue -> Sum<Kind, Errors> =
      Sum.assertDiscriminatorAndContinue "symbol" (fun _ -> sum { return Kind.Symbol })

    static member private ToJsonSymbol: JsonValue =
      JsonValue.Record([| discriminatorKey, JsonValue.String "symbol" |])

    static member private FromJsonStar: JsonValue -> Sum<Kind, Errors> =
      Sum.assertDiscriminatorAndContinue "star" (fun _ -> sum { return Kind.Star })

    static member private ToJsonStar: JsonValue =
      JsonValue.Record([| discriminatorKey, JsonValue.String "star" |])

    static member private FromJsonSchema: JsonValue -> Sum<Kind, Errors> =
      Sum.assertDiscriminatorAndContinue "schema" (fun _ -> sum { return Kind.Schema })

    static member private ToJsonSchema: JsonValue =
      JsonValue.Record([| discriminatorKey, JsonValue.String "schema" |])

    static member private FromJsonArrow =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun arrowFields ->
        sum {
          let! arrowFields = arrowFields |> JsonValue.AsRecordMap
          let! param = arrowFields |> (Map.tryFindWithError "param" "arrow" "param" >>= Kind.FromJson)

          let! returnType =
            arrowFields
            |> (Map.tryFindWithError "returnType" "arrow" "returnType" >>= Kind.FromJson)

          return Kind.Arrow(param, returnType)
        })

    static member private ToJsonArrow (param: Kind) (returnType: Kind) : JsonValue =
      JsonValue.Record
        [| discriminatorKey, JsonValue.String discriminator
           valueKey, JsonValue.Record [| "param", Kind.ToJson param; "returnType", Kind.ToJson returnType |] |]

    static member FromJson(json: JsonValue) : Sum<Kind, Errors> =
      sum.Any(
        Kind.FromJsonStar(json),
        [ Kind.FromJsonSymbol(json)
          Kind.FromJsonSchema(json)
          Kind.FromJsonArrow(json) ]
      )
      |> sum.MapError(Errors.HighestPriority)

    static member ToJson: Kind -> JsonValue =
      fun kind ->
        match kind with
        | Kind.Symbol -> Kind.ToJsonSymbol
        | Kind.Star -> Kind.ToJsonStar
        | Kind.Schema -> Kind.ToJsonSchema
        | Kind.Arrow(param, returnType) -> Kind.ToJsonArrow param returnType
