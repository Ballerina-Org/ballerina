namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module TypeApply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json

  type Expr<'T> with
    static member FromJsonTypeApply(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "type-apply" "type-apply" (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> fromRootJson
          let! ctx = reader.GetContext()
          let! arg = arg |> ctx |> reader.OfSum
          return Expr.TypeApply(f, arg)
        })

    static member ToJsonTypeApply: ExprEncoder<'T> -> Expr<'T> -> 'T -> Reader<JsonValue, JsonEncoder<'T>, Errors> =
      fun rootToJson f arg ->
        reader {
          let! ctx = reader.GetContext()
          let argJson = ctx arg
          let! fJson = rootToJson f
          return [| fJson; argJson |] |> JsonValue.Array |> Json.kind "type-apply" "type-apply"
        }
