namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json
open Ballerina.Errors

[<AutoOpen>]
module TupleCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "tuple-cons"

  type Expr<'T> with
    static member FromJsonTupleCons (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun elementsJson ->
        reader {
          let! elements = elementsJson |> JsonValue.AsArray |> reader.OfSum
          let! elements = elements |> Seq.map fromRootJson |> reader.All
          return Expr.TupleCons(elements)
        })

    static member ToJsonTupleCons (rootToJson: ExprEncoder<'T>) (tuple: List<Expr<'T>>) : ExprEncoderReader<'T> =
      tuple
      |> List.map rootToJson
      |> reader.All
      |> reader.Map(Array.ofList >> JsonValue.Array >> Json.discriminator discriminator)
