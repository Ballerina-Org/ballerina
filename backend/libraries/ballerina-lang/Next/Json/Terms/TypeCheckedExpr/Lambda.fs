namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina.DSL.Next.Types

[<AutoOpen>]
module Lambda =
  open FSharp.Data
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "lambda"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonLambda
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun lambdaJson ->
        reader {
          let! var, body = lambdaJson |> JsonValue.AsPair |> reader.OfSum
          let! var = var |> JsonValue.AsString |> reader.OfSum
          let var = Var.Create var
          let! body = body |> fromRootJson

          return
            TypeCheckedExpr.Lambda(
              var,
              Unchecked.defaultof<TypeValue<'valueExt>>,
              body,
              Unchecked.defaultof<TypeValue<'valueExt>>
            )
        })

    static member ToJsonLambda
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (var: Var)
      (body: TypeCheckedExpr<'valueExt>)
      : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let typeParamJson = var.Name |> JsonValue.String
        let! bodyJson = body |> rootToJson

        return
          [| typeParamJson; bodyJson |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
