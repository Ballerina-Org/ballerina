namespace Ballerina.DSL.Next.Types.Json

open Ballerina.Errors
open Ballerina.LocalizedErrors

[<AutoOpen>]
module UnionTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "union"

  type TypeExpr<'valueExt> with
    static member FromJsonUnion(fromJsonRoot: TypeExprParser<'valueExt>) : TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun unionFields ->
        sum {
          let! cases = unionFields |> JsonValue.AsArray

          let! caseTypes =
            cases
            |> Array.map (fun case ->
              sum {
                let! (caseKey, caseValue) = case |> JsonValue.AsPair
                let! caseType = fromJsonRoot caseValue
                let! caseKey = fromJsonRoot caseKey
                return (caseKey, caseType)
              })
            |> sum.All

          let union = TypeExpr.Union(caseTypes)

          let! wrappedUnion =
            AutomaticSymbolCreation.wrapWithLet (union, caseTypes |> List.map fst, SymbolsKind.UnionConstructors)
            |> sum.MapError(Errors.MapContext(replaceWith ()))

          return wrappedUnion
        })

    static member ToJsonUnion
      (rootToJson: TypeExpr<'valueExt> -> JsonValue)
      : List<TypeExpr<'valueExt> * TypeExpr<'valueExt>> -> JsonValue =
      fun cases ->
        let caseTypes =
          cases
          |> Seq.map (fun (caseKey, caseType) ->
            let caseKeyJson = rootToJson caseKey
            let caseTypeJson = rootToJson caseType
            JsonValue.Array [| caseKeyJson; caseTypeJson |])

        JsonValue.Array(caseTypes |> Array.ofSeq) |> Json.discriminator discriminator
