namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeLambda =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "type-lambda"

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member FromJsonTypeLambda
      (fromRootJson: ExprParser<'T, 'Id, 'valueExt>)
      (value: JsonValue)
      : ExprParserReader<'T, 'Id, 'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun typeParamJson ->
        reader {
          let! (typeParam, body) = typeParamJson |> JsonValue.AsPair |> reader.OfSum
          let! typeParam = typeParam |> TypeParameter.FromJson |> reader.OfSum
          let! body = body |> fromRootJson
          return Expr.TypeLambda(typeParam, body)
        })

    static member ToJsonTypeLambda
      (rootToJson: ExprEncoder<'T, 'Id, 'valueExt>)
      (typeParam: TypeParameter)
      (body: Expr<'T, 'Id, 'valueExt>)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let typeParamJson = typeParam |> TypeParameter.ToJson
        let! bodyJson = body |> rootToJson

        return
          [| typeParamJson; bodyJson |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
