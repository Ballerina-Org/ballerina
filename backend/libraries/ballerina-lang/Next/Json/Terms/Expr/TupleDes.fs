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

  let private kindKey = "tuple-des"
  let private fieldKey = "tuple-des"

  type Expr<'T> with
    static member FromJsonTupleDes (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      reader.AssertKindAndContinueWithField value kindKey fieldKey (fun tupleDesJson ->
        reader {
          let! (expr, index) = tupleDesJson |> JsonValue.AsPair |> reader.OfSum
          let! expr = expr |> fromRootJson
          let! index = index |> JsonValue.AsInt |> reader.OfSum
          return Expr.TupleDes(expr, { Index = index })
        })

    static member ToJsonTupleDes
      (rootToJson: ExprEncoder<'T>)
      (e: Expr<'T>)
      (sel: TupleDesSelector)
      : ExprEncoderReader<'T> =
      reader {
        let! e = e |> rootToJson
        let index = sel.Index |> decimal |> JsonValue.Number

        return [| e; index |] |> JsonValue.Array |> Json.kind kindKey fieldKey
      }
