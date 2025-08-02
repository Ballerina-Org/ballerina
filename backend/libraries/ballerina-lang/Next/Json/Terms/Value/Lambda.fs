namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Lambda =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json

  type Value<'T> with
    static member FromJsonLambda(fromJsonRoot: JsonValue -> ExprParser<'T>) : JsonValue -> ValueParser<'T> =
      fun json ->
        reader {
          return!
            reader.AssertKindAndContinueWithField
              "lambda"
              "lambda"
              (fun lambdaJson ->
                reader {
                  let! (var, body) = lambdaJson |> JsonValue.AsPair |> reader.OfSum
                  let! var = var |> JsonValue.AsString |> reader.OfSum
                  let var = Var.Create var
                  let! body = body |> fromJsonRoot
                  return Value.Lambda(var, body)
                })
              (json)
        }

    static member ToJsonLambda(toRootJson: Expr<'T> -> JsonValue) : Var * Expr<'T> -> JsonValue =
      fun (var, body) ->
        let var = var.Name |> JsonValue.String
        let body = body |> toRootJson
        [| var; body |] |> JsonValue.Array |> Json.kind "lambda" "lambda"
