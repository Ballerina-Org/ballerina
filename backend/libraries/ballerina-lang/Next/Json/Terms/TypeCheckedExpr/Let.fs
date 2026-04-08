namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Types

[<AutoOpen>]
module Let =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns

  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "let"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonLet
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue
        discriminator
        value
        (fun letJson ->
          reader {
            let! (var, value, body) =
              letJson |> JsonValue.AsTriple |> reader.OfSum

            let! var = var |> JsonValue.AsString |> reader.OfSum
            let var = Var.Create var
            let! value = value |> fromRootJson
            let! body = body |> fromRootJson

            return
              TypeCheckedExpr.Let(
                var,
                TypeValue.CreateUnit(),
                value,
                body,
                TypeValue.CreateUnit(),
                Kind.Star
              )
          })

    static member ToJsonLet
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (var: Var)
      (value: TypeCheckedExpr<'valueExt>)
      (body: TypeCheckedExpr<'valueExt>)
      : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let var = var.Name |> JsonValue.String
        let! value = value |> rootToJson
        let! body = body |> rootToJson

        return
          [| var; value; body |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
