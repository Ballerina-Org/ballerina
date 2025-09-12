namespace Ballerina.DSL.Next.StdLib.Int32

[<AutoOpen>]
module Model =
  type Int32Operations<'ext> =
    | Plus of {| v1: Option<int32> |}
    | Minus of {| v1: unit |}
    | Divide of {| v1: Option<int32> |}
    | Power of {| v1: Option<int32> |}
    | Mod of {| v1: Option<int32> |}
    | Equal of {| v1: Option<int32> |}
    | NotEqual of {| v1: Option<int32> |}
    | GreaterThan of {| v1: Option<int32> |}
    | GreaterThanOrEqual of {| v1: Option<int32> |}
    | LessThan of {| v1: Option<int32> |}
    | LessThanOrEqual of {| v1: Option<int32> |}
