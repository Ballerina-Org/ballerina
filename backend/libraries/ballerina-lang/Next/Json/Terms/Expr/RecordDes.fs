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
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "record-field-lookup"

  type Expr<'T, 'Id when 'Id: comparison> with
    static member FromJsonRecordDes (fromRootJson: ExprParser<'T, 'Id>) (value: JsonValue) : ExprParserReader<'T, 'Id> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun recordDesJson ->
        reader {
          let! expr, field = recordDesJson |> JsonValue.AsPair |> reader.OfSum
          let! expr = expr |> fromRootJson
          let! _, ctx = reader.GetContext()
          let! field = field |> ctx |> reader.OfSum
          return Expr.RecordDes(expr, field)
        })

    static member ToJsonRecordDes
      (rootToJson: ExprEncoder<'T, 'Id>)
      (expr: Expr<'T, 'Id>)
      (field: 'Id)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let! _, ctx = reader.GetContext()
        let! expr = rootToJson expr
        let field = field |> ctx

        return [| expr; field |] |> JsonValue.Array |> Json.discriminator discriminator
      }
