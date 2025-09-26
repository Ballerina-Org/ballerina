namespace Ballerina.DSL.Next.StdLib.TimeSpan

[<AutoOpen>]
module Model =
  open System

  type TimeSpanOperations<'ext> =
    | Plus of {| v1: Option<TimeSpan> |}
    | Minus of {| v1: unit |}
    | Equal of {| v1: Option<TimeSpan> |}
    | NotEqual of {| v1: Option<TimeSpan> |}
    | GreaterThan of {| v1: Option<TimeSpan> |}
    | GreaterThanOrEqual of {| v1: Option<TimeSpan> |}
    | LessThan of {| v1: Option<TimeSpan> |}
    | LessThanOrEqual of {| v1: Option<TimeSpan> |}
    | Days
    | Hours
    | Minutes
    | Seconds
    | Milliseconds
    | TotalDays
    | TotalHours
    | TotalMinutes
    | TotalSeconds
    | TotalMilliseconds
