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

  type Value<'T, 'valueExtension> with
    static member FromJsonLambda
      (fromJsonRoot: JsonValue -> ExprParser<'T>)
      : JsonValue -> ValueParser<'T, 'valueExtension> =
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

    static member ToJsonLambda: ExprEncoder<'T> -> Var -> Expr<'T> -> JsonEncoder<'T, 'valueExtension> =
      fun root var body ->
        reader {
          let var = var.Name |> JsonValue.String
          let! body = body |> root |> reader.MapContext fst
          return [| var; body |] |> JsonValue.Array |> Json.kind "lambda" "lambda"
        }
