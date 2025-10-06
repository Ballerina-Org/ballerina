namespace Ballerina.DSL.Next.Terms.Json.Expr

[<AutoOpen>]
module UnionCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "union-case"

  type Expr<'T> with
    static member FromJsonUnionCons (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun unionCaseJson ->
        reader {
          let! (k, v) = unionCaseJson |> JsonValue.AsPair |> reader.OfSum
          let! k = k |> Identifier.FromJson |> reader.OfSum
          let! v = v |> fromRootJson
          return Expr.UnionCons(k, v)
        })

    static member ToJsonUnionCons (rootToJson: ExprEncoder<'T>) (k: Identifier) (v: Expr<'T>) : ExprEncoderReader<'T> =
      reader {
        let k = k |> Identifier.ToJson
        let! v = v |> rootToJson
        return [| k; v |] |> JsonValue.Array |> Json.discriminator discriminator
      }
