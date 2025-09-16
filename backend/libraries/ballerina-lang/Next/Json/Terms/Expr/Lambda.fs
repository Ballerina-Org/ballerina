namespace Ballerina.DSL.Next.Terms.Json.Expr

[<AutoOpen>]
module Lambda =
  open FSharp.Data
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Json

  let private kindKey = "lambda"
  let private fieldKey = "lambda"

  type Expr<'T> with
    static member FromJsonLambda (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      reader.AssertKindAndContinueWithField value kindKey fieldKey (fun lambdaJson ->
        reader {
          let! var, body = lambdaJson |> JsonValue.AsPair |> reader.OfSum
          let! var = var |> JsonValue.AsString |> reader.OfSum
          let var = Var.Create var
          let! body = body |> fromRootJson
          return Expr.Lambda(var, None, body)
        })

    static member ToJsonLambda (rootToJson: ExprEncoder<'T>) (var: Var) (body: Expr<'T>) : ExprEncoderReader<'T> =
      reader {
        let typeParamJson = var.Name |> JsonValue.String
        let! bodyJson = body |> rootToJson
        return [| typeParamJson; bodyJson |] |> JsonValue.Array |> Json.kind kindKey fieldKey
      }
