namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module TupleCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model

  type Expr<'T> with
    static member FromJsonTupleCons(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "tuple-cons" "elements" (fun elementsJson ->
        reader {
          let! elements = elementsJson |> JsonValue.AsArray |> reader.OfSum
          let! elements = elements |> Seq.map fromRootJson |> reader.All
          return Expr.TupleCons(elements)
        })

    static member ToJsonTupleCons(rootToJson: Expr<'T> -> JsonValue) : List<Expr<'T>> -> JsonValue =
      List.map rootToJson
      >> Array.ofList
      >> JsonValue.Array
      >> Json.kind "tuple-cons" "elements"
