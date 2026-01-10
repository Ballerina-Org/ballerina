namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module SumCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "sum"

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member FromJsonSumCons
      (_fromRootJson: ExprParser<'T, 'Id, 'valueExt>)
      (value: JsonValue)
      : ExprParserReader<'T, 'Id, 'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun elementsJson ->
        reader {
          let! (case, count) = elementsJson |> JsonValue.AsPair |> reader.OfSum
          let! case = case |> JsonValue.AsInt |> reader.OfSum
          let! count = count |> JsonValue.AsInt |> reader.OfSum
          return Expr.SumCons({ Case = case; Count = count })
        })

    static member ToJsonSumCons
      (_rootToJson: ExprEncoder<'T, 'Id, 'valueExt>)
      (sel: SumConsSelector)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let case = sel.Case |> decimal |> JsonValue.Number
        let count = sel.Count |> decimal |> JsonValue.Number
        return [| case; count |] |> JsonValue.Array |> Json.discriminator discriminator
      }
