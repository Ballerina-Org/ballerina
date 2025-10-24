﻿namespace Ballerina.DSL.Next.Terms.Json

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

  type Expr<'T, 'Id when 'Id: comparison> with
    static member FromJsonTypeApply (fromRootJson: ExprParser<'T, 'Id>) (value: JsonValue) : ExprParserReader<'T, 'Id> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> fromRootJson
          let! ctx, _ = reader.GetContext()
          let! arg = arg |> ctx |> reader.OfSum
          return Expr.TypeApply(f, arg)
        })

    static member ToJsonTypeApply
      (rootToJson: ExprEncoder<'T, 'Id>)
      (f: Expr<'T, 'Id>)
      (arg: 'T)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let! ctx, _ = reader.GetContext()
        let argJson = ctx arg
        let! fJson = rootToJson f
        return [| fJson; argJson |] |> JsonValue.Array |> Json.discriminator discriminator
      }
