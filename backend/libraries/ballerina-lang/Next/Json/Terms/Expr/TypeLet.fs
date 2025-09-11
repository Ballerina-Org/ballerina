namespace Ballerina.DSL.Next.Terms.Json.Expr

open Ballerina.DSL.Next.Json
open Ballerina.Errors

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

    static member ToJsonTypeLet
      : ExprEncoder<'T> -> string -> 'T -> Expr<'T> -> Reader<JsonValue, JsonEncoder<'T>, Errors> =
      fun rootToJson typeId typeArg body ->
        reader {
          let! ctx = reader.GetContext()
          let typeId = typeId |> JsonValue.String
          let! body = rootToJson body

          return
            [| typeId; ctx typeArg; body |]
            |> JsonValue.Array
            |> Json.kind "type-let" "type-let"
        }
