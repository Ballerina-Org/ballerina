namespace Ballerina.DSL.Next.StdLib.DateOnly

[<AutoOpen>]
module Model =
  open System

  type DateOnlyConstructors<'ext> = | DateOnly_New

  type DateOnlyValues<'ext> = DateOnly of DateOnly

  type DateOnlyOperations<'ext> =
    | Diff of {| v1: Option<DateOnly> |}
    | Equal of {| v1: Option<DateOnly> |}
    | NotEqual of {| v1: Option<DateOnly> |}
    | GreaterThan of {| v1: Option<DateOnly> |}
    | GreaterThanOrEqual of {| v1: Option<DateOnly> |}
    | LessThan of {| v1: Option<DateOnly> |}
    | LessThanOrEqual of {| v1: Option<DateOnly> |}
    | ToDateTime of {| v1: Option<DateOnly> |}
    | Year
    | Month
    | Day
    | DayOfWeek
    | DayOfYear
