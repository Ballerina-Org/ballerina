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

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member FromJsonIf
      (fromRootJson: ExprParser<'T, 'Id, 'valueExt>)
      (value: JsonValue)
      : ExprParserReader<'T, 'Id, 'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun ifJson ->
        reader {
          let! cond, thenBranch, elseBranch = ifJson |> JsonValue.AsTriple |> reader.OfSum
          let! cond = cond |> fromRootJson
          let! thenBranch = thenBranch |> fromRootJson
          let! elseBranch = elseBranch |> fromRootJson
          return Expr.If(cond, thenBranch, elseBranch)
        })

    static member ToJsonIf
      (rootToJson: ExprEncoder<'T, 'Id, 'valueExt>)
      (cond: Expr<'T, 'Id, 'valueExt>)
      (thenBranch: Expr<'T, 'Id, 'valueExt>)
      (elseBranch: Expr<'T, 'Id, 'valueExt>)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let! condJson = cond |> rootToJson
        let! thenJson = thenBranch |> rootToJson
        let! elseJson = elseBranch |> rootToJson

        return
          [| condJson; thenJson; elseJson |]
          |> JsonValue.Array
          |> Json.discriminator discriminator
      }
