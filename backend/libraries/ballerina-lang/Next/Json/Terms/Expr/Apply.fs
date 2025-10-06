namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Apply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "apply"

  type Expr<'T> with
    static member FromJsonApply (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> fromRootJson
          let! arg = arg |> fromRootJson
          return Expr.Apply(f, arg)
        })

    static member ToJsonApply (rootToJson: ExprEncoder<'T>) (f: Expr<'T>) (arg: Expr<'T>) : ExprEncoderReader<'T> =
      reader {
        let! f = f |> rootToJson
        let! arg = arg |> rootToJson
        return [| f; arg |] |> JsonValue.Array |> Json.discriminator discriminator
      }
