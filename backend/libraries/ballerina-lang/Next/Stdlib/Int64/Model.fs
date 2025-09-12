namespace Ballerina.DSL.Next.StdLib.Int64

[<AutoOpen>]
module Model =
  type Int64Operations<'ext> =
    | Plus of {| v1: Option<int64> |}
    | Minus of {| v1: unit |}
    | Divide of {| v1: Option<int64> |}
    | Power of {| v1: Option<int64> |}
    | Mod of {| v1: Option<int64> |}
    | Equal of {| v1: Option<int64> |}
    | NotEqual of {| v1: Option<int64> |}
    | GreaterThan of {| v1: Option<int64> |}
    | GreaterThanOrEqual of {| v1: Option<int64> |}
    | LessThan of {| v1: Option<int64> |}
    | LessThanOrEqual of {| v1: Option<int64> |}
