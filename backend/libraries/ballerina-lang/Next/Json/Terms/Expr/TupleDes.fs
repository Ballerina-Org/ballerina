namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Terms.Model

[<AutoOpen>]
module TupleDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "tuple-des"

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member FromJsonTupleDes
      (fromRootJson: ExprParser<'T, 'Id, 'valueExt>)
      (value: JsonValue)
      : ExprParserReader<'T, 'Id, 'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun tupleDesJson ->
        reader {
          let! (expr, index) = tupleDesJson |> JsonValue.AsPair |> reader.OfSum
          let! expr = expr |> fromRootJson
          let! index = index |> JsonValue.AsInt |> reader.OfSum
          return Expr.TupleDes(expr, { Index = index })
        })

    static member ToJsonTupleDes
      (rootToJson: ExprEncoder<'T, 'Id, 'valueExt>)
      (e: Expr<'T, 'Id, 'valueExt>)
      (sel: TupleDesSelector)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let! e = e |> rootToJson
        let index = sel.Index |> decimal |> JsonValue.Number

        return [| e; index |] |> JsonValue.Array |> Json.discriminator discriminator
      }
