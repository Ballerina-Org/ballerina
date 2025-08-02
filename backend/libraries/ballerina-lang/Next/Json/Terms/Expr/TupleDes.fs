namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Terms.Model

[<AutoOpen>]
module TupleDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader

  type Expr<'T> with
    static member FromJsonTupleDes(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "tuple-des" "tuple-des" (fun tupleDesJson ->
        reader {
          let! (expr, index, count) = tupleDesJson |> JsonValue.AsTriple |> reader.OfSum
          let! expr = expr |> fromRootJson
          let! index = index |> JsonValue.AsInt |> reader.OfSum
          let! count = count |> JsonValue.AsInt |> reader.OfSum
          return Expr.TupleDes(expr, { Index = index; Count = count })
        })

    static member ToJsonTupleDes(rootToJson: Expr<'T> -> JsonValue) : Expr<'T> * TupleDesSelector -> JsonValue =
      fun (e, sel) ->
        let e = e |> rootToJson
        let index = sel.Index |> decimal |> JsonValue.Number
        let count = sel.Count |> decimal |> JsonValue.Number

        [| e; index; count |] |> JsonValue.Array |> Json.kind "tuple-des" "tuple-des"
