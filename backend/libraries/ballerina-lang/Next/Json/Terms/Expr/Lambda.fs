namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json


[<AutoOpen>]
module Lambda =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns

  type Expr<'T> with
    static member FromJsonLambda(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "lambda" "lambda" (fun lambdaJson ->
        reader {
          let! (var, body) = lambdaJson |> JsonValue.AsPair |> reader.OfSum
          let! var = var |> JsonValue.AsString |> reader.OfSum
          let var = Var.Create var
          let! body = body |> fromRootJson
          return Expr.Lambda(var, None, body)
        })

    static member ToJsonLambda(rootToJson: Expr<'T> -> JsonValue) : Var * Expr<'T> -> JsonValue =
      fun (var, body) ->
        let typeParamJson = var.Name |> JsonValue.String
        let bodyJson = body |> rootToJson
        [| typeParamJson; bodyJson |] |> JsonValue.Array |> Json.kind "lambda" "lambda"
