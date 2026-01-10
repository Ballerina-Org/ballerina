namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Types

[<AutoOpen>]
module Apply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "apply"

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member FromJsonApply
      (fromRootJson: ExprParser<'T, 'Id, 'valueExt>)
      (value: JsonValue)
      : ExprParserReader<'T, 'Id, 'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> fromRootJson
          let! arg = arg |> fromRootJson
          return Expr.Apply(f, arg)
        })

    static member ToJsonApply
      (rootToJson: ExprEncoder<'T, 'Id, 'valueExt>)
      (f: Expr<'T, 'Id, 'valueExt>)
      (arg: Expr<'T, 'Id, 'valueExt>)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let! f = f |> rootToJson
        let! arg = arg |> rootToJson
        return [| f; arg |] |> JsonValue.Array |> Json.discriminator discriminator
      }
