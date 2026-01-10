namespace Ballerina.DSL.Next.Delta.Json

[<AutoOpen>]
module DeltaExt =
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Data.Delta.Extensions

  type DeltaExt with
    static member FromJson
      (valueParser: JsonValue -> Sum<Value<TypeValue<ValueExt>, ValueExt>, Errors>)
      : JsonParser<DeltaExt> =
      fun json ->
        Sum.assertDiscriminatorAndContinueWithValue
          "deltaExt"
          (fun json ->
            sum {
              let! container = JsonValue.AsRecordMap json

              let! discriminator =
                container
                |> Map.tryFindWithError "discriminator" "deltaExt" "Cant find discriminator in deltaExt"
                |> Sum.bind JsonValue.AsString

              let! value =
                container
                |> Map.tryFindWithError "value" "deltaExt" "Cant find value in deltaExt"

              let! value =
                match discriminator with
                | "list" ->
                  sum {
                    let! container = JsonValue.AsRecordMap value

                    let! discriminator =
                      container
                      |> Map.tryFindWithError "discriminator" "deltaExt" "Cant find discriminator in list deltaExt"
                      |> Sum.bind JsonValue.AsString

                    let! value =
                      match discriminator with
                      | "updateElementAt" ->
                        sum {
                          let! container = JsonValue.AsRecordMap value

                          let! value =
                            container
                            |> Map.tryFindWithError "value" "deltaExt" "Cant find value in list deltaExt"

                          let! value = JsonValue.AsRecordMap value

                          let! index =
                            value
                            |> Map.tryFindWithError "index" "deltaExt" "Cant find index in list deltaExt"
                            |> Sum.bind JsonValue.AsNumber

                          let! value =
                            value
                            |> Map.tryFindWithError "value" "deltaExt" "Cant find value in list deltaExt"

                          let! value = valueParser value
                          return DeltaExt.DeltaExt(Choice1Of3(UpdateElement(int index, value)))
                        }
                      | "appendElement" ->
                        sum {
                          let! value = JsonValue.AsRecordMap value

                          let! value =
                            value
                            |> Map.tryFindWithError "value" "deltaExt" "Cant find value in list deltaExt"

                          let! value = valueParser value
                          return DeltaExt.DeltaExt(Choice1Of3(ListDeltaExt.AppendElement(value)))
                        }
                      | "removeElementAt" ->
                        sum {
                          let! container = JsonValue.AsRecordMap value

                          let! index =
                            container
                            |> Map.tryFindWithError "index" "deltaExt" "Cant find index in list deltaExt"
                            |> Sum.bind JsonValue.AsNumber

                          return DeltaExt.DeltaExt(Choice1Of3(ListDeltaExt.RemoveElement(int index)))
                        }
                      | other -> sum.Throw(Errors.Singleton $"Unimplemented parser for deltaExt list op: {other}")

                    return value
                  }
                | other -> sum.Throw(Errors.Singleton $"Unimplemented parser for deltaExt{other} discriminator")

              return value
            })
          json

    static member ToJson
      (valueEncoder: JsonEncoderWithError<Value<TypeValue<ValueExt>, ValueExt>>)
      (deltaExt: DeltaExt)
      : Sum<JsonValue, Errors> =
      sum {

        let! value =
          match deltaExt with
          | DeltaExt.DeltaExt(Choice1Of3(UpdateElement(i, v))) ->
            sum {
              let! value = valueEncoder v

              return
                JsonValue.Record
                  [| "discriminator", JsonValue.String "list"
                     "value",
                     JsonValue.Record
                       [| "discriminator", JsonValue.String "updateElementAt"
                          "value", JsonValue.Record [| "index", JsonValue.Number(decimal i); "value", value |] |] |]
            }
          | DeltaExt.DeltaExt(Choice1Of3(ListDeltaExt.AppendElement(v))) ->
            sum {
              let! value = valueEncoder v

              return
                JsonValue.Record
                  [| "discriminator", JsonValue.String "list"
                     "value", JsonValue.Record [| "discriminator", JsonValue.String "appendElement"; "value", value |] |]
            }
          | DeltaExt.DeltaExt(Choice1Of3(ListDeltaExt.RemoveElement(i))) ->
            JsonValue.Record
              [| "discriminator", JsonValue.String "list"
                 "value",
                 JsonValue.Record
                   [| "discriminator", JsonValue.String "removeElementAt"
                      "index", JsonValue.Number(decimal i) |] |]
            |> sum.Return
          | DeltaExt.DeltaExt(Choice2Of3 _) ->
            sum.Throw(Errors.Singleton "Option in Delta extensions serializers not yet implemented")
          | DeltaExt.DeltaExt(Choice3Of3(OptionDeltaExt)) ->
            sum.Throw(Errors.Singleton "Option in Delta extensions serializers not yet implemented")

        return value |> Json.discriminator "deltaExt"
      }
