namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module RecordCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model

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
                let! k = k |> JsonValue.AsString |> reader.OfSum
                let! v = v |> fromRootJson
                return (k, v)
              })
            |> reader.All

          return Expr.RecordCons(fields)
        })

    static member ToJsonRecordCons(rootToJson: Expr<'T> -> JsonValue) : List<string * Expr<'T>> -> JsonValue =
      List.map (fun (field, expr) ->
        let expr = rootToJson expr
        let field = JsonValue.String field
        [| field; expr |] |> JsonValue.Array)
      >> List.toArray
      >> JsonValue.Array
      >> Json.kind "record-cons" "fields"
