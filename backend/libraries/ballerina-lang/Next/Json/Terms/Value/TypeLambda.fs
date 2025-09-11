namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module TypeLambda =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json

  type Value<'T, 'valueExtension> with
    static member FromJsonTypeLambda
      (_fromJsonRoot: JsonValue -> ExprParser<'T>)
      : JsonValue -> ValueParser<'T, 'valueExtension> =
      fun json ->
        reader {
          return!
            reader.AssertKindAndContinueWithField
              "type-lambda"
              "type-lambda"
              (fun typeParamJson ->
                reader {
                  let! typeParam, body = typeParamJson |> JsonValue.AsPair |> reader.OfSum
                  let! typeParam = typeParam |> TypeParameter.FromJson |> reader.OfSum
                  let! body = body |> Expr.FromJson
                  return Value.TypeLambda(typeParam, body)
                })
              json
        }

    static member ToJsonTypeLambda: ExprEncoder<'T> -> TypeParameter -> Expr<'T> -> JsonEncoder<'T, 'valueExtension> =
      fun root tp body ->
        reader {
          let tp = TypeParameter.ToJson tp
          let! bodyJson = root body |> reader.MapContext fst
          return [| tp; bodyJson |] |> JsonValue.Array |> Json.kind "type-lambda" "type-lambda"
        }
