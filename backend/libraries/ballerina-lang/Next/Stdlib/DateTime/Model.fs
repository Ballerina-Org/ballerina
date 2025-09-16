namespace Ballerina.DSL.Next.StdLib.DateTime

[<AutoOpen>]
module Model =
  open System

  type DateTimeOperations<'ext> =
    | Equal of {| v1: Option<DateTime> |}
    | NotEqual of {| v1: Option<DateTime> |}
    | GreaterThan of {| v1: Option<DateTime> |}
    | GreaterThanOrEqual of {| v1: Option<DateTime> |}
    | LessThan of {| v1: Option<DateTime> |}
    | LessThanOrEqual of {| v1: Option<DateTime> |}
