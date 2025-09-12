namespace Ballerina.DSL.Next.StdLib.Float32

[<AutoOpen>]
module Model =
  type Float32Operations<'ext> =
    | Plus of {| v1: Option<float32> |}
    | Minus of {| v1: unit |}
    | Divide of {| v1: Option<float32> |}
    | Power of {| v1: Option<float32> |}
    | Mod of {| v1: Option<float32> |}
    | Equal of {| v1: Option<float32> |}
    | NotEqual of {| v1: Option<float32> |}
    | GreaterThan of {| v1: Option<float32> |}
    | GreaterThanOrEqual of {| v1: Option<float32> |}
    | LessThan of {| v1: Option<float32> |}
    | LessThanOrEqual of {| v1: Option<float32> |}
