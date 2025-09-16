namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module SetTypeExpr =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  let private kindKey = "set"
  let private fieldKey = "set"

  type TypeExpr with
    static member FromJsonSet(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fromJsonRoot >>= (TypeExpr.Set >> sum.Return))

    static member ToJsonSet(rootToJson: TypeExpr -> JsonValue) : TypeExpr -> JsonValue =
      rootToJson >> Json.kind kindKey fieldKey
