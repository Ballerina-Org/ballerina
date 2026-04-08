namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Terms.Model

[<AutoOpen>]
module TupleDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "tuple-des"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonTupleDes
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue
        discriminator
        value
        (fun tupleDesJson ->
          reader {
            let! (expr, index) =
              tupleDesJson |> JsonValue.AsPair |> reader.OfSum

            let! expr = expr |> fromRootJson
            let! index = index |> JsonValue.AsInt |> reader.OfSum

            return
              TypeCheckedExpr.TupleDes(
                expr,
                { Index = index },
                TypeValue.CreateUnit(),
                Kind.Star
              )
          })

    static member ToJsonTupleDes
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (e: TypeCheckedExpr<'valueExt>)
      (sel: TupleDesSelector)
      : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let! e = e |> rootToJson
        let index = sel.Index |> decimal |> JsonValue.Number

        return
          [| e; index |] |> JsonValue.Array |> Json.discriminator discriminator
      }
