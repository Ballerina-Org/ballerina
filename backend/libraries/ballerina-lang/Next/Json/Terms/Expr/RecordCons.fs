namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module RecordCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "record-cons"

  type Expr<'T, 'Id when 'Id: comparison> with
    static member FromJsonRecordCons
      (fromRootJson: ExprParser<'T, 'Id>)
      (value: JsonValue)
      : ExprParserReader<'T, 'Id> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun fieldsJson ->
        reader {
          let! fields = fieldsJson |> JsonValue.AsArray |> reader.OfSum
          let! _, ctx = reader.GetContext()

          let! fields =
            fields
            |> Seq.map (fun field ->
              reader {
                let! (k, v) = field |> JsonValue.AsPair |> reader.OfSum
                let! k = k |> ctx |> reader.OfSum
                let! v = v |> fromRootJson
                return (k, v)
              })
            |> reader.All

          return Expr.RecordCons(fields)
        })

    static member ToJsonRecordCons
      (rootToJson: ExprEncoder<'T, 'Id>)
      (record: List<'Id * Expr<'T, 'Id>>)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let! _, ctx = reader.GetContext()

        let! all =
          record
          |> List.map (fun (field, expr) ->
            reader {
              let! expr = rootToJson expr
              let field = field |> ctx
              return [| field; expr |] |> JsonValue.Array
            })
          |> reader.All

        return all |> (List.toArray >> JsonValue.Array >> Json.discriminator discriminator)
      }
