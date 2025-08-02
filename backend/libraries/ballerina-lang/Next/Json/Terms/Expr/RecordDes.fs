namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module RecordDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model

  type Expr<'T> with
    static member FromJsonRecordDes(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "record-field-lookup" "record-field-lookup" (fun recordDesJson ->
        reader {
          let! (expr, field) = recordDesJson |> JsonValue.AsPair |> reader.OfSum
          let! expr = expr |> fromRootJson
          let! field = field |> JsonValue.AsString |> reader.OfSum
          return Expr.RecordDes(expr, field)
        })

    static member ToJsonRecordDes(rootToJson: Expr<'T> -> JsonValue) : Expr<'T> * string -> JsonValue =
      fun (expr, field) ->

        let expr = rootToJson expr
        let field = JsonValue.String field

        [| expr; field |]
        |> JsonValue.Array
        |> Json.kind "record-field-lookup" "record-field-lookup"
