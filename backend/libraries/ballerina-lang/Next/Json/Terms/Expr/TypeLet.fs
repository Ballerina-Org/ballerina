namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module TypeLet =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns

  type Expr<'T> with
    static member FromJsonTypeLet(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "type-let" "type-let" (fun typeLetJson ->
        reader {
          let! (typeId, typeArg, body) = typeLetJson |> JsonValue.AsTriple |> reader.OfSum
          let! typeId = typeId |> JsonValue.AsString |> reader.OfSum
          let! ctx = reader.GetContext()
          let! typeArg = typeArg |> ctx |> reader.OfSum
          let! body = body |> fromRootJson
          return Expr.TypeLet(typeId, typeArg, body)
        })

    static member ToJsonTypeLet(rootToJson: Expr<'T> -> JsonValue) : string * 'T * Expr<'T> -> JsonValue =
      fun (typeId, typeArg, body) ->
        let typeId = typeId |> JsonValue.String

        match box typeArg with
        | :? TypeExpr as typeExpr ->
          let argJson = TypeExpr.ToJson typeExpr
          let body = body |> rootToJson

          [| typeId; argJson; body |]
          |> JsonValue.Array
          |> Json.kind "type-let" "type-let"
        | other -> failwith $"Expected a TypeExpr but got {other.GetType().Name}"
