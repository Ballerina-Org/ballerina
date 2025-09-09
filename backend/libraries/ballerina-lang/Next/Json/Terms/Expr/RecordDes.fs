namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module RecordDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Json

  type Expr<'T> with
    static member FromJsonRecordDes(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "record-field-lookup" "record-field-lookup" (fun recordDesJson ->
        reader {
          let! (expr, field) = recordDesJson |> JsonValue.AsPair |> reader.OfSum
          let! expr = expr |> fromRootJson
          let! field = field |> Identifier.FromJson |> reader.OfSum
          return Expr.RecordDes(expr, field)
        })

    static member ToJsonRecordDes
      : ExprEncoder<'T> -> Expr<'T> -> Identifier -> Reader<JsonValue, JsonEncoder<'T>, Errors> =
      fun rootToJson expr field ->
        reader {
          let! expr = rootToJson expr
          let field = field |> Identifier.ToJson

          return
            [| expr; field |]
            |> JsonValue.Array
            |> Json.kind "record-field-lookup" "record-field-lookup"
        }
