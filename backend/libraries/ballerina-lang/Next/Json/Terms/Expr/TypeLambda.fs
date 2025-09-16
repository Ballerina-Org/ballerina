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

  let private kindKey = "type-lambda"
  let private fieldKey = "type-lambda"

  type Expr<'T> with
    static member FromJsonTypeLambda (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      reader.AssertKindAndContinueWithField value kindKey fieldKey (fun typeParamJson ->
        reader {
          let! (typeParam, body) = typeParamJson |> JsonValue.AsPair |> reader.OfSum
          let! typeParam = typeParam |> TypeParameter.FromJson |> reader.OfSum
          let! body = body |> fromRootJson
          return Expr.TypeLambda(typeParam, body)
        })

    static member ToJsonTypeLambda
      (rootToJson: ExprEncoder<'T>)
      (typeParam: TypeParameter)
      (body: Expr<'T>)
      : ExprEncoderReader<'T> =
      reader {
        let typeParamJson = typeParam |> TypeParameter.ToJson
        let! bodyJson = body |> rootToJson

        return [| typeParamJson; bodyJson |] |> JsonValue.Array |> Json.kind kindKey fieldKey
      }
