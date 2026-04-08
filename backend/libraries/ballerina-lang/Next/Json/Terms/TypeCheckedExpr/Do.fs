namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Types

[<AutoOpen>]
module Do =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns

  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "do"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonDo
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue
        discriminator
        value
        (fun doJson ->
          reader {
            let! (value, body) = doJson |> JsonValue.AsPair |> reader.OfSum
            let! value = value |> fromRootJson
            let! body = body |> fromRootJson

            return
              TypeCheckedExpr.Do(
                value,
                body,
                TypeValue.CreateUnit(),
                Kind.Star
              )
          })

    static member ToJsonDo
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (value: TypeCheckedExpr<'valueExt>)
      (body: TypeCheckedExpr<'valueExt>)
      : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let! value = value |> rootToJson
        let! body = body |> rootToJson

        return
          [| value; body |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
