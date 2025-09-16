namespace Ballerina.DSL.Next.StdLib.Float64

[<AutoOpen>]
module Model =
  type Float64Operations<'ext> =
    | Plus of {| v1: Option<float> |}
    | Minus of {| v1: unit |}
    | Divide of {| v1: Option<float> |}
    | Power of {| v1: Option<float> |}
    | Mod of {| v1: Option<float> |}
    | Equal of {| v1: Option<float> |}
    | NotEqual of {| v1: Option<float> |}
    | GreaterThan of {| v1: Option<float> |}
    | GreaterThanOrEqual of {| v1: Option<float> |}
    | LessThan of {| v1: Option<float> |}
    | LessThanOrEqual of {| v1: Option<float> |}
