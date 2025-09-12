namespace Ballerina.DSL.Next.StdLib.String

[<AutoOpen>]
module Model =
  type StringOperations<'ext> =
    | Concat of {| v1: Option<string> |}
    | Equal of {| v1: Option<string> |}
    | NotEqual of {| v1: Option<string> |}
    | GreaterThan of {| v1: Option<string> |}
    | GreaterThanOrEqual of {| v1: Option<string> |}
    | LessThan of {| v1: Option<string> |}
    | LessThanOrEqual of {| v1: Option<string> |}
