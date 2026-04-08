namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module SumCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "sum"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonSumCons
      (_fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue
        discriminator
        value
        (fun elementsJson ->
          reader {
            let! (case, count) =
              elementsJson |> JsonValue.AsPair |> reader.OfSum

            let! case = case |> JsonValue.AsInt |> reader.OfSum
            let! count = count |> JsonValue.AsInt |> reader.OfSum

            return
              TypeCheckedExpr.SumCons(
                { Case = case; Count = count },
                TypeValue.CreateUnit(),
                Kind.Star
              )
          })

    static member ToJsonSumCons
      (_rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (sel: SumConsSelector)
      : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let case = sel.Case |> decimal |> JsonValue.Number
        let count = sel.Count |> decimal |> JsonValue.Number

        return
          [| case; count |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
