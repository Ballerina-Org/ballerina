namespace Ballerina.StdLib.Json

module Reader =
  open FSharp.Data
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Map
  open Ballerina.Errors
  open Ballerina.Collections.Sum
  open Sum


  type ReaderBuilder with
    member _.AssertDiscriminatorAndContinue<'ctx, 'T>
      (discriminatorKey: string)
      (discriminatorValue: string)
      (k: Unit -> Reader<'T, 'ctx, Errors>)
      (json: JsonValue)
      : Reader<'T, 'ctx, Errors> =
      reader {
        let! ctx = reader.GetContext()

        let! result =
          sum.AssertDiscriminatorAndContinue discriminatorKey discriminatorValue (fun () -> k () |> Reader.Run ctx) json
          |> reader.OfSum

        return result
      }

    member reader.AssertDiscriminatorAndContinueWithValue<'ctx, 'T>
      (discriminatorKey: string)
      (valueKey: string)
      (discriminatorValue: string)
      (json: JsonValue)
      (k: JsonValue -> Reader<'T, 'ctx, Errors>)
      : Reader<'T, 'ctx, Errors> =
      reader {
        let! ctx = reader.GetContext()

        let! result =
          sum.AssertDiscriminatorAndContinueWithValue
            discriminatorKey
            valueKey
            discriminatorValue
            (fun valueJson -> k valueJson |> Reader.Run ctx)
            json
          |> reader.OfSum

        return result
      }
