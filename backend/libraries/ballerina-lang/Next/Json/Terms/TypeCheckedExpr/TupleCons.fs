namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Types
open Ballerina.Errors

[<AutoOpen>]
module TupleCons =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "tuple-cons"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonTupleCons
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun elementsJson ->
        reader {
          let! elements = elementsJson |> JsonValue.AsArray |> reader.OfSum
          let! elements = elements |> Seq.map fromRootJson |> reader.All
          return TypeCheckedExpr.TupleCons(elements)
        })

    static member ToJsonTupleCons
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (tuple: List<TypeCheckedExpr<'valueExt>>)
      : TypeCheckedExprEncoderReader<'valueExt> =
      tuple
      |> List.map rootToJson
      |> reader.All
      |> reader.Map(Array.ofList >> JsonValue.Array >> Json.discriminator discriminator)
