namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Types

[<AutoOpen>]
module If =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "if"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonIf
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun ifJson ->
        reader {
          let! cond, thenBranch, elseBranch = ifJson |> JsonValue.AsTriple |> reader.OfSum
          let! cond = cond |> fromRootJson
          let! thenBranch = thenBranch |> fromRootJson
          let! elseBranch = elseBranch |> fromRootJson
          return TypeCheckedExpr.If(cond, thenBranch, elseBranch, TypeValue.CreateUnit(), Kind.Star)
        })

    static member ToJsonIf
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (cond: TypeCheckedExpr<'valueExt>)
      (thenBranch: TypeCheckedExpr<'valueExt>)
      (elseBranch: TypeCheckedExpr<'valueExt>)
      : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let! condJson = cond |> rootToJson
        let! thenJson = thenBranch |> rootToJson
        let! elseJson = elseBranch |> rootToJson

        return
          [| condJson; thenJson; elseJson |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
