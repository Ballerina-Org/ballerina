namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module TypeApply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json

  let private kindKey = "type-apply"
  let private fieldKey = "type-apply"

  type Expr<'T> with
    static member FromJsonTypeApply (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      reader.AssertKindAndContinueWithField value kindKey fieldKey (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> fromRootJson
          let! ctx = reader.GetContext()
          let! arg = arg |> ctx |> reader.OfSum
          return Expr.TypeApply(f, arg)
        })

    static member ToJsonTypeApply (rootToJson: ExprEncoder<'T>) (f: Expr<'T>) (arg: 'T) : ExprEncoderReader<'T> =
      reader {
        let! ctx = reader.GetContext()
        let argJson = ctx arg
        let! fJson = rootToJson f
        return [| fJson; argJson |] |> JsonValue.Array |> Json.kind kindKey fieldKey
      }
