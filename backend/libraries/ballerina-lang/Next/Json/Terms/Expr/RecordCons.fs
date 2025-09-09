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

  type Expr<'T> with
    static member FromJsonRecordCons(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "record-cons" "fields" (fun fieldsJson ->
        reader {
          let! fields = fieldsJson |> JsonValue.AsArray |> reader.OfSum

          let! fields =
            fields
            |> Seq.map (fun field ->
              reader {
                let! (k, v) = field |> JsonValue.AsPair |> reader.OfSum
                let! k = k |> Identifier.FromJson |> reader.OfSum
                let! v = v |> fromRootJson
                return (k, v)
              })
            |> reader.All

          return Expr.RecordCons(fields)
        })

    static member ToJsonRecordCons
      : ExprEncoder<'T> -> List<Identifier * Expr<'T>> -> Reader<JsonValue, JsonEncoder<'T>, Errors> =
      fun rootToJson record ->
        reader {
          let! all =
            record
            |> List.map (fun (field, expr) ->
              reader {
                let! expr = rootToJson expr
                let field = Identifier.ToJson field
                return [| field; expr |] |> JsonValue.Array
              })
            |> reader.All

          return all |> (List.toArray >> JsonValue.Array >> Json.kind "record-cons" "fields")
        }
