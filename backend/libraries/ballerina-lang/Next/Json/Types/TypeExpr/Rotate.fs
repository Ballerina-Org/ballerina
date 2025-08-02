namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Rotate =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  type TypeExpr with
    static member FromJsonRotate(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField "rotate" "rotate" (fromJsonRoot >>= (TypeExpr.Rotate >> sum.Return))

    static member ToJsonRotate(rootToJson: TypeExpr -> JsonValue) : TypeExpr -> JsonValue =
      rootToJson >> Json.kind "rotate" "rotate"
