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

  type Kind with
    static member private FromJsonSymbol: JsonValue -> Sum<Kind, Errors> =
      sum.AssertKindAndContinue "symbol" (fun _ -> sum { return Kind.Symbol })

    static member private ToJsonSymbol: JsonValue =
      JsonValue.Record([| "kind", JsonValue.String "symbol" |])

    static member private FromJsonStar: JsonValue -> Sum<Kind, Errors> =
      sum.AssertKindAndContinue "star" (fun _ -> sum { return Kind.Star })

    static member private ToJsonStar: JsonValue =
      JsonValue.Record([| "kind", JsonValue.String "star" |])

    static member private FromJsonArrow: JsonValue -> Sum<Kind, Errors> =
      sum.AssertKindAndContinueWithField "arrow" "arrow" (fun arrowFields ->
        sum {
          let! arrowFields = arrowFields |> JsonValue.AsRecordMap
          let! param = arrowFields |> (Map.tryFindWithError "param" "arrow" "param" >>= Kind.FromJson)

          let! returnType =
            arrowFields
            |> (Map.tryFindWithError "returnType" "arrow" "returnType" >>= Kind.FromJson)

          return Kind.Arrow(param, returnType)
        })

    static member private ToJsonArrow: Kind * Kind -> JsonValue =
      fun (param, returnType) ->
        JsonValue.Record
          [| "kind", JsonValue.String "arrow"
             "arrow", JsonValue.Record [| "param", Kind.ToJson param; "returnType", Kind.ToJson returnType |] |]

    static member FromJson(json: JsonValue) : Sum<Kind, Errors> =
      sum.Any(Kind.FromJsonStar(json), [ Kind.FromJsonSymbol(json); Kind.FromJsonArrow(json) ])
      |> sum.MapError(Errors.HighestPriority)

    static member ToJson: Kind -> JsonValue =
      fun kind ->
        match kind with
        | Kind.Symbol -> Kind.ToJsonSymbol
        | Kind.Star -> Kind.ToJsonStar
        | Kind.Arrow(param, returnType) -> Kind.ToJsonArrow(param, returnType)
