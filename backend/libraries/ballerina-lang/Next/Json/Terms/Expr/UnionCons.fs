namespace Ballerina.DSL.Next.Terms.Json.Expr

[<AutoOpen>]
module UnionCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Json

  type Expr<'T> with
    static member FromJsonUnionCons(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "union-case" "union-case" (fun unionCaseJson ->
        reader {
          let! (k, v) = unionCaseJson |> JsonValue.AsPair |> reader.OfSum
          let! k = k |> JsonValue.AsString |> reader.OfSum
          let! v = v |> fromRootJson
          return Expr.UnionCons(k, v)
        })

    static member ToJsonUnionCons(rootToJson: Expr<'T> -> JsonValue) : string * Expr<'T> -> JsonValue =
      fun (k, v) ->
        let k = k |> JsonValue.String
        let v = v |> rootToJson
        [| k; v |] |> JsonValue.Array |> Json.kind "union-case" "union-case"
