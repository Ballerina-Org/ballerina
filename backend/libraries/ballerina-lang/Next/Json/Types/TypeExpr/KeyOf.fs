namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module KeyOf =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  let private kindKey = "keyOf"
  let private fieldKey = "keyOf"

  type TypeExpr with
    static member FromJsonKeyOf(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fromJsonRoot >>= (TypeExpr.KeyOf >> sum.Return))

    static member ToJsonKeyOf(rootToJson: TypeExpr -> JsonValue) : TypeExpr -> JsonValue =
      rootToJson >> Json.kind kindKey fieldKey
