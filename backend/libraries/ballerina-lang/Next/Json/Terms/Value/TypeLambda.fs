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

  let private kindKey = "type-lambda"
  let private fieldKey = "type-lambda"

  type Value<'T, 'valueExtension> with
    static member FromJsonTypeLambda(json: JsonValue) : ValueParserReader<'T, 'valueExtension> =
      reader.AssertKindAndContinueWithField json kindKey fieldKey (fun typeParamJson ->
        reader {
          let! exprFromJsonRoot, _ = reader.GetContext()
          let! typeParam, body = typeParamJson |> JsonValue.AsPair |> reader.OfSum
          let! typeParam = typeParam |> TypeParameter.FromJson |> reader.OfSum
          let! body = body |> exprFromJsonRoot |> reader.OfSum
          return Value.TypeLambda(typeParam, body)
        })

    static member ToJsonTypeLambda (typeParam: TypeParameter) (body: Expr<'T>) : ValueEncoderReader<'T> =
      reader {
        let! rootExprEncoder, _ = reader.GetContext()
        let tp = TypeParameter.ToJson typeParam
        let! bodyJson = body |> rootExprEncoder |> reader.OfSum
        return [| tp; bodyJson |] |> JsonValue.Array |> Json.kind kindKey fieldKey
      }
