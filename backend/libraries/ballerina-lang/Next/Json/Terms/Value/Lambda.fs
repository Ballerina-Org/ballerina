namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Lambda =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Types

  let private discriminator = "lambda"

  type Value<'T, 'valueExtension> with
    static member FromJsonLambda(json: JsonValue) : ValueParserReader<'T, ResolvedIdentifier, 'valueExtension> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator json (fun lambdaJson ->
        reader {
          let! exprFromJsonRoot, _, _ = reader.GetContext()
          let! (var, body) = lambdaJson |> JsonValue.AsPair |> reader.OfSum
          let! var = var |> JsonValue.AsString |> reader.OfSum
          let var = Var.Create var
          let! body = body |> exprFromJsonRoot |> reader.OfSum
          return Value.Lambda(var, body, Map.empty, TypeCheckScope.Empty)
        })

    static member ToJsonLambda
      (var: Var)
      (body: Expr<'T, ResolvedIdentifier, 'valueExtension>)
      : ValueEncoderReader<'T, 'valueExtension> =
      reader {
        let! rootExprEncoder, _ = reader.GetContext()
        let var = var.Name |> JsonValue.String
        let! body = body |> rootExprEncoder |> reader.OfSum
        return [| var; body |] |> JsonValue.Array |> Json.discriminator discriminator
      }
