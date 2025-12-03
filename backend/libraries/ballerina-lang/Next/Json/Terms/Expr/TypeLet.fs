namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json
open Ballerina.Errors

[<AutoOpen>]
module TypeLet =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "type-let"

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member FromJsonTypeLet
      (fromRootJson: ExprParser<'T, 'Id, 'valueExt>)
      (value: JsonValue)
      : ExprParserReader<'T, 'Id, 'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun typeLetJson ->
        reader {
          let! (typeId, typeArg, body) = typeLetJson |> JsonValue.AsTriple |> reader.OfSum
          let! typeId = typeId |> JsonValue.AsString |> reader.OfSum
          let! ctx, _ = reader.GetContext()
          let! typeArg = typeArg |> ctx |> reader.OfSum
          let! body = body |> fromRootJson
          return Expr.TypeLet(typeId, typeArg, body)
        })

    static member ToJsonTypeLet
      (rootToJson: ExprEncoder<'T, 'Id, 'valueExt>)
      (typeId: string)
      (typeArg: 'T)
      (body: Expr<'T, 'Id, 'valueExt>)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let! ctx, _ = reader.GetContext()
        let typeId = typeId |> JsonValue.String
        let! body = rootToJson body

        return
          [| typeId; ctx typeArg; body |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
