namespace Ballerina.DSL.Next.StdLib.DateOnly

[<AutoOpen>]
module Model =
  open System

  type DateOnlyOperations<'ext> =
    | Equal of {| v1: Option<DateOnly> |}
    | NotEqual of {| v1: Option<DateOnly> |}
    | GreaterThan of {| v1: Option<DateOnly> |}
    | GreaterThanOrEqual of {| v1: Option<DateOnly> |}
    | LessThan of {| v1: Option<DateOnly> |}
    | LessThanOrEqual of {| v1: Option<DateOnly> |}
    | ToDateTime of {| v1: Option<int * int * int> |}
