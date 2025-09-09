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

  type Expr<'T> with
    static member FromJsonLet(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "let" "let" (fun letJson ->
        reader {
          let! (var, value, body) = letJson |> JsonValue.AsTriple |> reader.OfSum
          let! var = var |> JsonValue.AsString |> reader.OfSum
          let var = Var.Create var
          let! value = value |> fromRootJson
          let! body = body |> fromRootJson
          return Expr.Let(var, value, body)
        })

    static member ToJsonLet
      : ExprEncoder<'T> -> Var -> Expr<'T> -> Expr<'T> -> Reader<JsonValue, JsonEncoder<'T>, Errors> =
      fun rootToJson var value body ->
        reader {
          let var = var.Name |> JsonValue.String
          let! value = value |> rootToJson
          let! body = body |> rootToJson
          return [| var; value; body |] |> JsonValue.Array |> Json.kind "let" "let"
        }
