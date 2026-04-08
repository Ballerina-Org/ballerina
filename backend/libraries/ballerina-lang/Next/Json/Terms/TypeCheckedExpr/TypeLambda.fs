namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

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
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "type-lambda"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonTypeLambda
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue
        discriminator
        value
        (fun typeParamJson ->
          reader {
            let! (typeParam, body) =
              typeParamJson |> JsonValue.AsPair |> reader.OfSum

            let! typeParam =
              typeParam |> TypeParameter.FromJson |> reader.OfSum

            let! body = body |> fromRootJson

            return
              TypeCheckedExpr.TypeLambda(
                typeParam,
                body,
                TypeValue.CreateUnit(),
                Kind.Star
              )
          })

    static member ToJsonTypeLambda
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (typeParam: TypeParameter)
      (body: TypeCheckedExpr<'valueExt>)
      : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let typeParamJson = typeParam |> TypeParameter.ToJson
        let! bodyJson = body |> rootToJson

        return
          [| typeParamJson; bodyJson |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
