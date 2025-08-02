namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module ListTypeExpr =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  type TypeExpr with
    static member FromJsonList(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField "list" "list" (fromJsonRoot >>= (TypeExpr.List >> sum.Return))

    static member ToJsonList(rootToJson: TypeExpr -> JsonValue) : TypeExpr -> JsonValue =
      rootToJson >> Json.kind "list" "list"
