namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module RecordDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "record-field-lookup"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonRecordDes
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue
        discriminator
        value
        (fun recordDesJson ->
          reader {
            let! expr, field =
              recordDesJson |> JsonValue.AsPair |> reader.OfSum

            let! expr = expr |> fromRootJson
            let! _, ctx = reader.GetContext()
            let! field = field |> ctx |> reader.OfSum

            return
              TypeCheckedExpr.RecordDes(
                expr,
                field,
                TypeValue.CreateUnit(),
                Kind.Star
              )
          })

    static member ToJsonRecordDes
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (expr: TypeCheckedExpr<'valueExt>)
      (field: ResolvedIdentifier)
      : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let! _, ctx = reader.GetContext()
        let! expr = rootToJson expr
        let field = field |> ctx

        return
          [| expr; field |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
