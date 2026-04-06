namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

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

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonTypeLet
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun typeLetJson ->
        reader {
          let! (typeId, typeArg, body) = typeLetJson |> JsonValue.AsTriple |> reader.OfSum
          let! typeId = typeId |> JsonValue.AsString |> reader.OfSum
          let! ctx, _ = reader.GetContext()
          let! typeArg = typeArg |> ctx |> reader.OfSum
          let! body = body |> fromRootJson
          return TypeCheckedExpr.TypeLet(typeId, typeArg, body, TypeValue.CreateUnit(), Kind.Star)
        })

    static member ToJsonTypeLet
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (typeId: string)
      (typeArg: TypeValue<'valueExt>)
      (body: TypeCheckedExpr<'valueExt>)
      : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let! ctx, _ = reader.GetContext()
        let typeId = typeId |> JsonValue.String
        let! body = rootToJson body

        return
          [| typeId; ctx typeArg; body |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
