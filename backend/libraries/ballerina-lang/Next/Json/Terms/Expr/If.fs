namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module If =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "if"

  type Expr<'T> with
    static member FromJsonIf (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun ifJson ->
        reader {
          let! cond, thenBranch, elseBranch = ifJson |> JsonValue.AsTriple |> reader.OfSum
          let! cond = cond |> fromRootJson
          let! thenBranch = thenBranch |> fromRootJson
          let! elseBranch = elseBranch |> fromRootJson
          return Expr.If(cond, thenBranch, elseBranch)
        })

    static member ToJsonIf
      (rootToJson: ExprEncoder<'T>)
      (cond: Expr<'T>)
      (thenBranch: Expr<'T>)
      (elseBranch: Expr<'T>)
      : ExprEncoderReader<'T> =
      reader {
        let! condJson = cond |> rootToJson
        let! thenJson = thenBranch |> rootToJson
        let! elseJson = elseBranch |> rootToJson

        return
          [| condJson; thenJson; elseJson |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
