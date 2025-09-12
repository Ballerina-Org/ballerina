namespace Ballerina.DSL.Next.StdLib.Bool

[<AutoOpen>]
module Model =
  type BoolOperations<'ext> =
    | And of {| v1: Option<bool> |}
    | Or of {| v1: Option<bool> |}
    | Not of {| v1: Unit |}
