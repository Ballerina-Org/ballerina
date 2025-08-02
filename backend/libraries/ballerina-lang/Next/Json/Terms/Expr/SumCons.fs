namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module SumCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model

  type Expr<'T> with
    static member FromJsonSumCons(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "sum" "case" (fun elementsJson ->
        reader {
          let! (case, count, v) = elementsJson |> JsonValue.AsTriple |> reader.OfSum
          let! case = case |> JsonValue.AsInt |> reader.OfSum
          let! count = count |> JsonValue.AsInt |> reader.OfSum
          let! v = v |> fromRootJson
          return Expr.SumCons({ Case = case; Count = count }, v)
        })

    static member ToJsonSumCons(rootToJson: Expr<'T> -> JsonValue) : SumConsSelector * Expr<'T> -> JsonValue =
      fun (sel, e) ->
        let case = sel.Case |> decimal |> JsonValue.Number
        let count = sel.Count |> decimal |> JsonValue.Number
        let e = e |> rootToJson
        [| case; count; e |] |> JsonValue.Array |> Json.kind "sum" "case"
