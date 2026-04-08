namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

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

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonApply
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> fromRootJson
          let! arg = arg |> fromRootJson
          return TypeCheckedExpr.Apply(f, arg, TypeValue.CreateUnit(), Kind.Star)
        })

    static member ToJsonApply
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (f: TypeCheckedExpr<'valueExt>)
      (arg: TypeCheckedExpr<'valueExt>)
      : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let! f = f |> rootToJson
        let! arg = arg |> rootToJson
        return [| f; arg |] |> JsonValue.Array |> Json.discriminator discriminator
      }
