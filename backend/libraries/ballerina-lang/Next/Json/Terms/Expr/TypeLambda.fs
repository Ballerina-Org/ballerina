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

  type Expr<'T> with
    static member FromJsonTypeLambda(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "type-lambda" "type-lambda" (fun typeParamJson ->
        reader {
          let! (typeParam, body) = typeParamJson |> JsonValue.AsPair |> reader.OfSum
          let! typeParam = typeParam |> TypeParameter.FromJson |> reader.OfSum
          let! body = body |> fromRootJson
          return Expr.TypeLambda(typeParam, body)
        })

    static member ToJsonTypeLambda
      : ExprEncoder<'T> -> TypeParameter -> Expr<'T> -> Reader<JsonValue, JsonEncoder<'T>, Errors> =
      fun rootToJson typeParam body ->
        reader {
          let typeParamJson = typeParam |> TypeParameter.ToJson
          let! bodyJson = body |> rootToJson

          return
            [| typeParamJson; bodyJson |]
            |> JsonValue.Array
            |> Json.kind "type-lambda" "type-lambda"
        }
