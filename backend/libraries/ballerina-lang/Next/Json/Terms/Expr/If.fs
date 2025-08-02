namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module If =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model

  type Expr<'T> with
    static member FromJsonIf(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "if" "if" (fun ifJson ->
        reader {
          let! (cond, thenBranch, elseBranch) = ifJson |> JsonValue.AsTriple |> reader.OfSum
          let! cond = cond |> fromRootJson
          let! thenBranch = thenBranch |> fromRootJson
          let! elseBranch = elseBranch |> fromRootJson
          return Expr.If(cond, thenBranch, elseBranch)
        })

    static member ToJsonIf(rootToJson: Expr<'T> -> JsonValue) : Expr<'T> * Expr<'T> * Expr<'T> -> JsonValue =
      fun (cond, thenBranch, elseBranch) ->
        let condJson = cond |> rootToJson
        let thenJson = thenBranch |> rootToJson
        let elseJson = elseBranch |> rootToJson
        [| condJson; thenJson; elseJson |] |> JsonValue.Array |> Json.kind "if" "if"
