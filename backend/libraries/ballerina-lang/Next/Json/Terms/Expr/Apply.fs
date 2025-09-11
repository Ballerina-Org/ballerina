namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Apply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors

  type Expr<'T> with
    static member FromJsonApply(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "apply" "apply" (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> fromRootJson
          let! arg = arg |> fromRootJson
          return Expr.Apply(f, arg)
        })

    static member ToJsonApply: ExprEncoder<'T> -> Expr<'T> -> Expr<'T> -> Reader<JsonValue, JsonEncoder<'T>, Errors> =
      fun rootToJson f arg ->
        reader {
          let! f = f |> rootToJson
          let! arg = arg |> rootToJson
          return [| f; arg |] |> JsonValue.Array |> Json.kind "apply" "apply"
        }
