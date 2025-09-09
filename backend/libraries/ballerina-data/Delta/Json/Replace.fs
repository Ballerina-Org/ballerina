namespace Ballerina.DSL.Next.Delta.Json

[<AutoOpen>]
module Replace =
  open Ballerina.Errors
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open Ballerina.DSL.Next.Terms.Json
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open FSharp.Data

  type Delta<'valueExtension> with
    static member FromJsonReplace: DeltaParser<'valueExtension> =
      reader.AssertKindAndContinueWithField<ValueParser<'valueExtension>, _> "replace" "replace" (fun json ->
        reader {
          let! ctx = reader.GetContext()
          let! value = json |> ctx |> reader.OfSum
          return value |> Delta.Replace
        })

    static member ToJsonReplace
      : Value<TypeValue, 'valueExtension>
          -> Reader<JsonValue, JsonEncoder<TypeValue> * JsonEncoder<'valueExtension>, Errors> =
      Value<TypeValue, 'valueExtension>.ToJson
      >> reader.Map(Json.kind "replace" "replace")
