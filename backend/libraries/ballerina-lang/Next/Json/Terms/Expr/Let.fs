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
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "let"

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member FromJsonLet
      (fromRootJson: ExprParser<'T, 'Id, 'valueExt>)
      (value: JsonValue)
      : ExprParserReader<'T, 'Id, 'valueExt> =
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
      (rootToJson: ExprEncoder<'T, 'Id, 'valueExt>)
      (var: Var)
      (value: Expr<'T, 'Id, 'valueExt>)
      (body: Expr<'T, 'Id, 'valueExt>)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let var = var.Name |> JsonValue.String
        let! value = value |> rootToJson
        let! body = body |> rootToJson
        return [| var; value; body |] |> JsonValue.Array |> Json.discriminator discriminator
      }
