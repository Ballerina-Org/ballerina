namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Terms.Model

[<AutoOpen>]
module TupleDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.Errors

  type Expr<'T> with
    static member FromJsonTupleDes(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "tuple-des" "tuple-des" (fun tupleDesJson ->
        reader {
          let! (expr, index) = tupleDesJson |> JsonValue.AsPair |> reader.OfSum
          let! expr = expr |> fromRootJson
          let! index = index |> JsonValue.AsInt |> reader.OfSum
          return Expr.TupleDes(expr, { Index = index })
        })

    static member ToJsonTupleDes
      : ExprEncoder<'T> -> Expr<'T> -> TupleDesSelector -> Reader<JsonValue, JsonEncoder<'T>, Errors> =
      fun rootToJson e sel ->
        reader {
          let! e = e |> rootToJson
          let index = sel.Index |> decimal |> JsonValue.Number

          return [| e; index |] |> JsonValue.Array |> Json.kind "tuple-des" "tuple-des"
        }
