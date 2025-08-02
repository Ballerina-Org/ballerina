namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Apply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model

  type Expr<'T> with
    static member FromJsonApply(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "apply" "apply" (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> fromRootJson
          let! arg = arg |> fromRootJson
          return Expr.Apply(f, arg)
        })

    static member ToJsonApply(rootToJson: Expr<'T> -> JsonValue) : Expr<'T> * Expr<'T> -> JsonValue =
      fun (f, arg) ->
        let f = f |> rootToJson
        let arg = arg |> rootToJson
        [| f; arg |] |> JsonValue.Array |> Json.kind "apply" "apply"
