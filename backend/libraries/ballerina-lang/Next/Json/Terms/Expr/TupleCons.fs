namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json
open Ballerina.Errors

[<AutoOpen>]
module TupleCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model

  let private kindKey = "tuple-cons"
  let private fieldKey = "elements"

  type Expr<'T> with
    static member FromJsonTupleCons (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      reader.AssertKindAndContinueWithField value kindKey fieldKey (fun elementsJson ->
        reader {
          let! elements = elementsJson |> JsonValue.AsArray |> reader.OfSum
          let! elements = elements |> Seq.map fromRootJson |> reader.All
          return Expr.TupleCons(elements)
        })

    static member ToJsonTupleCons (rootToJson: ExprEncoder<'T>) (tuple: List<Expr<'T>>) : ExprEncoderReader<'T> =
      tuple
      |> List.map rootToJson
      |> reader.All
      |> reader.Map(Array.ofList >> JsonValue.Array >> Json.kind kindKey fieldKey)
