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

  let private kindKey = "lambda"
  let private fieldKey = "lambda"

  type Value<'T, 'valueExtension> with
    static member FromJsonLambda(json: JsonValue) : ValueParserReader<'T, 'valueExtension> =
      reader.AssertKindAndContinueWithField json kindKey fieldKey (fun lambdaJson ->
        reader {
          let! exprFromJsonRoot, _ = reader.GetContext()
          let! (var, body) = lambdaJson |> JsonValue.AsPair |> reader.OfSum
          let! var = var |> JsonValue.AsString |> reader.OfSum
          let var = Var.Create var
          let! body = body |> exprFromJsonRoot |> reader.OfSum
          return Value.Lambda(var, body)
        })

    static member ToJsonLambda (var: Var) (body: Expr<'T>) : ValueEncoderReader<'T> =
      reader {
        let! rootExprEncoder, _ = reader.GetContext()
        let var = var.Name |> JsonValue.String
        let! body = body |> rootExprEncoder |> reader.OfSum
        return [| var; body |] |> JsonValue.Array |> Json.kind kindKey fieldKey
      }
