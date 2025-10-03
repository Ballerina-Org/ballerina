namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module SumCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "sum"

  type Expr<'T> with
    static member FromJsonSumCons (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun elementsJson ->
        reader {
          let! (case, count, v) = elementsJson |> JsonValue.AsTriple |> reader.OfSum
          let! case = case |> JsonValue.AsInt |> reader.OfSum
          let! count = count |> JsonValue.AsInt |> reader.OfSum
          let! v = v |> fromRootJson
          return Expr.SumCons({ Case = case; Count = count }, v)
        })

    static member ToJsonSumCons
      (rootToJson: ExprEncoder<'T>)
      (sel: SumConsSelector)
      (e: Expr<'T>)
      : ExprEncoderReader<'T> =
      reader {
        let case = sel.Case |> decimal |> JsonValue.Number
        let count = sel.Count |> decimal |> JsonValue.Number
        let! e = e |> rootToJson
        return [| case; count; e |] |> JsonValue.Array |> Json.discriminator discriminator
      }
