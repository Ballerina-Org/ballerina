namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Let =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns

  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "let"

  type Expr<'T> with
    static member FromJsonLet (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun letJson ->
        reader {
          let! (var, value, body) = letJson |> JsonValue.AsTriple |> reader.OfSum
          let! var = var |> JsonValue.AsString |> reader.OfSum
          let var = Var.Create var
          let! value = value |> fromRootJson
          let! body = body |> fromRootJson
          return Expr.Let(var, None, value, body)
        })

    static member ToJsonLet
      (rootToJson: ExprEncoder<'T>)
      (var: Var)
      (value: Expr<'T>)
      (body: Expr<'T>)
      : ExprEncoderReader<'T> =
      reader {
        let var = var.Name |> JsonValue.String
        let! value = value |> rootToJson
        let! body = body |> rootToJson
        return [| var; value; body |] |> JsonValue.Array |> Json.discriminator discriminator
      }
