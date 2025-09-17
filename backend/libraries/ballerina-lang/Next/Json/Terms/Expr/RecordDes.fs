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

  let private kindKey = "record-field-lookup"
  let private fieldKey = "record-field-lookup"

  type Expr<'T> with
    static member FromJsonRecordDes (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      reader.AssertKindAndContinueWithField value kindKey fieldKey (fun recordDesJson ->
        reader {
          let! expr, field = recordDesJson |> JsonValue.AsPair |> reader.OfSum
          let! expr = expr |> fromRootJson
          let! field = field |> Identifier.FromJson |> reader.OfSum
          return Expr.RecordDes(expr, field)
        })

    static member ToJsonRecordDes
      (rootToJson: ExprEncoder<'T>)
      (expr: Expr<'T>)
      (field: Identifier)
      : ExprEncoderReader<'T> =
      reader {
        let! expr = rootToJson expr
        let field = field |> Identifier.ToJson

        return [| expr; field |] |> JsonValue.Array |> Json.kind kindKey fieldKey
      }
