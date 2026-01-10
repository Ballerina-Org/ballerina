namespace Ballerina.DSL.Next.StdLib.Decimal

[<AutoOpen>]
module Model =
  type DecimalOperations<'ext> =
    | String
    | Plus of {| v1: Option<decimal> |}
    | Minus of {| v1: Option<decimal> |}
    | Divide of {| v1: Option<decimal> |}
    | Times of {| v1: Option<decimal> |}
    | Power of {| v1: Option<decimal> |}
    | Mod of {| v1: Option<decimal> |}
    | Equal of {| v1: Option<decimal> |}
    | NotEqual of {| v1: Option<decimal> |}
    | GreaterThan of {| v1: Option<decimal> |}
    | GreaterThanOrEqual of {| v1: Option<decimal> |}
    | LessThan of {| v1: Option<decimal> |}
    | LessThanOrEqual of {| v1: Option<decimal> |}
