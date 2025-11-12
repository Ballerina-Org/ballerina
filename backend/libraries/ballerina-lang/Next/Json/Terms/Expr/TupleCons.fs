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
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "tuple-cons"

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member FromJsonTupleCons
      (fromRootJson: ExprParser<'T, 'Id, 'valueExt>)
      (value: JsonValue)
      : ExprParserReader<'T, 'Id, 'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun elementsJson ->
        reader {
          let! elements = elementsJson |> JsonValue.AsArray |> reader.OfSum
          let! elements = elements |> Seq.map fromRootJson |> reader.All
          return Expr.TupleCons(elements)
        })

    static member ToJsonTupleCons
      (rootToJson: ExprEncoder<'T, 'Id, 'valueExt>)
      (tuple: List<Expr<'T, 'Id, 'valueExt>>)
      : ExprEncoderReader<'T, 'Id> =
      tuple
      |> List.map rootToJson
      |> reader.All
      |> reader.Map(Array.ofList >> JsonValue.Array >> Json.discriminator discriminator)
