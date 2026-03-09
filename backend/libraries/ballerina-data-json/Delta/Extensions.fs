namespace Ballerina.DSL.Next.Delta.Json

[<AutoOpen>]
module DeltaExt =
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open FSharp.Data
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Next.StdLib.List.Model
  open Ballerina.Data.Delta

  type DeltaExt<'runtimeContext, 'db, 'customExtension when 'db: comparison and 'customExtension: comparison> with
    static member FromJson
      (valueParser:
        JsonValue
          -> Sum<
            Value<
              TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
              ValueExt<'runtimeContext, 'db, 'customExtension>
             >,
            Errors<unit>
           >)
      : JsonParser<DeltaExt<'runtimeContext, 'db, 'customExtension>> =
      fun json ->
        Sum.assertDiscriminatorAndContinueWithValue
          "deltaExt"
          (fun json ->
            sum {
              let! container = JsonValue.AsRecordMap json

              let! discriminator =
                container
                |> Map.tryFindWithError "discriminator" "deltaExt" (fun () -> "Cant find discriminator in deltaExt") ()
                |> Sum.bind JsonValue.AsString

              let! value =
                container
                |> Map.tryFindWithError "value" "deltaExt" (fun () -> "Cant find value in deltaExt") ()

              let! value =
                match discriminator with
                | "list" ->
                  sum {
                    let! container = JsonValue.AsRecordMap value

                    let! discriminator =
                      container
                      |> Map.tryFindWithError
                        "discriminator"
                        "deltaExt"
                        (fun () -> "Cant find discriminator in list deltaExt")
                        ()
                      |> Sum.bind JsonValue.AsString

                    let! value =
                      match discriminator with
                      | "updateElementAt" ->
                        sum {
                          let! container = JsonValue.AsRecordMap value

                          let! value =
                            container
                            |> Map.tryFindWithError
                              "value"
                              "deltaExt"
                              (fun () -> "Cant find value in list deltaExt")
                              ()

                          let! value = JsonValue.AsRecordMap value

                          let! index =
                            value
                            |> Map.tryFindWithError
                              "index"
                              "deltaExt"
                              (fun () -> "Cant find index in list deltaExt")
                              ()
                            |> Sum.bind JsonValue.AsNumber

                          let! value =
                            value
                            |> Map.tryFindWithError
                              "value"
                              "deltaExt"
                              (fun () -> "Cant find value in list deltaExt")
                              ()

                          let! value = valueParser value
                          return DeltaExt.DeltaExtension(Choice1Of4(UpdateElement(int index, Delta.Replace value)))
                        }
                      | "appendElement" ->
                        sum {
                          let! value = JsonValue.AsRecordMap value

                          let! value =
                            value
                            |> Map.tryFindWithError
                              "value"
                              "deltaExt"
                              (fun () -> "Cant find value in list deltaExt")
                              ()

                          let! value = valueParser value
                          return DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.AppendElement(value)))
                        }
                      | "removeElementAt" ->
                        sum {
                          let! container = JsonValue.AsRecordMap value

                          let! index =
                            container
                            |> Map.tryFindWithError
                              "index"
                              "deltaExt"
                              (fun () -> "Cant find index in list deltaExt")
                              ()
                            |> Sum.bind JsonValue.AsNumber

                          return DeltaExt.DeltaExtension(Choice1Of4(ListDeltaExt.RemoveElement(int index)))
                        }
                      | other ->
                        sum.Throw(
                          Errors.Singleton () (fun () -> $"Unimplemented parser for deltaExt list op: {other}")
                        )

                    return value
                  }
                | other ->
                  sum.Throw(Errors.Singleton () (fun () -> $"Unimplemented parser for deltaExt{other} discriminator"))

              return value
            })
          json

    static member ToJson
      (_valueEncoder:
        JsonEncoderWithError<
          Value<
            TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
            ValueExt<'runtimeContext, 'db, 'customExtension>
           >
         >)
      (_deltaExt: DeltaExt<'runtimeContext, 'db, 'customExtension>)
      : Sum<JsonValue, Errors<unit>> =
      sum {

        let! value = sum.Throw(Errors.Singleton () (fun () -> "This function is deprecated"))

        return value |> Json.discriminator "deltaExt"
      }
