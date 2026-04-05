namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina.DSL.Next.Types

[<AutoOpen>]
module TypeApply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "type-apply"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonTypeApply
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> fromRootJson
          let! ctx, _ = reader.GetContext()
          let! arg = arg |> ctx |> reader.OfSum
          return TypeCheckedExpr.TypeApply(f, arg)
        })

    static member ToJsonTypeApply
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (f: TypeCheckedExpr<'valueExt>)
      (arg: TypeValue<'valueExt>)
      : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let! ctx, _ = reader.GetContext()
        let argJson = ctx arg
        let! fJson = rootToJson f
        return [| fJson; argJson |] |> JsonValue.Array |> Json.discriminator discriminator
      }
