namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeApply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model

  type Expr<'T> with
    static member FromJsonTypeApply(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "type-apply" "type-apply" (fun application ->
        reader {
          let! f, arg = application |> JsonValue.AsPair |> reader.OfSum
          let! f = f |> fromRootJson
          let! ctx = reader.GetContext()
          let! arg = arg |> ctx |> reader.OfSum
          return Expr.TypeApply(f, arg)
        })

    static member ToJsonTypeApply(rootToJson: Expr<'T> -> JsonValue) : Expr<'T> * 'T -> JsonValue =
      fun (f, arg) ->
        match box arg with
        | :? TypeExpr as typeExpr ->
          let argJson = TypeExpr.ToJson typeExpr
          let fJson = rootToJson f
          [| fJson; argJson |] |> JsonValue.Array |> Json.kind "type-apply" "type-apply"
        | other -> failwith $"Expected a TypeExpr but got {other.GetType().Name}"
