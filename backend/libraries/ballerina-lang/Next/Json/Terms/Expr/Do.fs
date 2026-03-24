namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Types

[<AutoOpen>]
module Do =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns

  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "do"

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member FromJsonDo
      (fromRootJson: ExprParser<'T, 'Id, 'valueExt>)
      (value: JsonValue)
      : ExprParserReader<'T, 'Id, 'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun doJson ->
        reader {
          let! (value, body) = doJson |> JsonValue.AsPair |> reader.OfSum
          let! value = value |> fromRootJson
          let! body = body |> fromRootJson
          return Expr.Do(value, body)
        })

    static member ToJsonDo
      (rootToJson: ExprEncoder<'T, 'Id, 'valueExt>)
      (value: Expr<'T, 'Id, 'valueExt>)
      (body: Expr<'T, 'Id, 'valueExt>)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let! value = value |> rootToJson
        let! body = body |> rootToJson
        return [| value; body |] |> JsonValue.Array |> Json.discriminator discriminator
      }
