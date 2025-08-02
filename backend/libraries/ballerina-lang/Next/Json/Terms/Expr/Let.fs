namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Let =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns

  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms.Patterns

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

    static member ToJsonLet(rootToJson: Expr<'T> -> JsonValue) : Var * Expr<'T> * Expr<'T> -> JsonValue =
      fun (var, value, body) ->
        let var = var.Name |> JsonValue.String
        let value = value |> rootToJson
        let body = body |> rootToJson
        [| var; value; body |] |> JsonValue.Array |> Json.kind "let" "let"
