namespace Ballerina.DSL.Next.StdLib.DateTime

[<AutoOpen>]
module Model =
  open System

  type DateTimeConstructors =
    | DateTime_New
    | DateTime_Now
    | DateTime_UTCNow

    override self.ToString() : string =
      match self with
      | DateTime_New -> "dateTime::new"
      | DateTime_Now -> "dateTime::now"
      | DateTime_UTCNow -> "dateTime::utcNow"

  type DateTimeOperations<'ext> =
    | Diff of {| v1: Option<DateTime> |}
    | Equal of {| v1: Option<DateTime> |}
    | NotEqual of {| v1: Option<DateTime> |}
    | GreaterThan of {| v1: Option<DateTime> |}
    | GreaterThanOrEqual of {| v1: Option<DateTime> |}
    | LessThan of {| v1: Option<DateTime> |}
    | LessThanOrEqual of {| v1: Option<DateTime> |}
    | ToDateOnly
    | Year
    | Month
    | Day
    | DayOfWeek
    | DayOfYear
