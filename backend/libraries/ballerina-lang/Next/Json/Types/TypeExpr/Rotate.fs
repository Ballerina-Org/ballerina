namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Rotate =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  let private kindKey = "rotate"
  let private fieldKey = "rotate"

  type TypeExpr with
    static member FromJsonRotate(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fromJsonRoot >>= (TypeExpr.Rotate >> sum.Return))

    static member ToJsonRotate(rootToJson: TypeExpr -> JsonValue) : TypeExpr -> JsonValue =
      rootToJson >> Json.kind kindKey fieldKey
