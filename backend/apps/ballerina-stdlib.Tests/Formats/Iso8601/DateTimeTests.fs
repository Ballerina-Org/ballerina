module Ballerina.StdLib.Tests.Formats.Iso8601.DateTimeTests

open System
open Ballerina.StdLib.Formats
open NUnit.Framework

[<Test;
  TestCase(2023, 9, 15, 14, 30, 45, 0, 0, "2023-09-15T14:30:45.0000000Z");
  TestCase(2023, 9, 15, 14, 30, 45, 123, 0, "2023-09-15T14:30:45.1230000Z");
  TestCase(2023, 9, 15, 14, 30, 45, 123, 4567, "2023-09-15T14:30:45.1234567Z")>]
let ``Expect printUtc to return an Iso6801 date string``
  year
  month
  day
  hours
  minutes
  seconds
  ms
  ticks
  (expected: string)
  =
  let date =
    DateTime(year, month, day, hours, minutes, seconds, ms, DateTimeKind.Utc).AddTicks ticks

  let actual = Iso8601.DateTime.printUtc date
  Assert.That(actual, Is.EqualTo expected)

[<Test;
  TestCase(2023, 9, 15, 14, 30, 45, 0, 0, "2023-09-15T14:30:45Z");
  TestCase(2023, 9, 15, 12, 30, 45, 0, 0, "2023-09-15T14:30:45+02:00");
  TestCase(2023, 9, 15, 19, 30, 45, 0, 0, "2023-09-15T14:30:45-05:00");
  TestCase(2023, 9, 15, 14, 30, 45, 123, 0, "2023-09-15T14:30:45.123");
  TestCase(2023, 9, 15, 14, 30, 45, 123, 0, "2023-09-15T14:30:45.123Z");
  TestCase(2023, 9, 15, 14, 30, 45, 123, 4567, "2023-09-15T14:30:45.1234567Z");
  TestCase(2023, 9, 15, 12, 30, 45, 123, 0, "2023-09-15T14:30:45.123+02:00");
  TestCase(2023, 9, 15, 19, 30, 45, 123, 4567, "2023-09-15T14:30:45.1234567-05:00")>]
let ``Expect tryParse to return Some date`` year month day hours minutes seconds ms ticks input =
  let expected =
    DateTime(year, month, day, hours, minutes, seconds, ms, DateTimeKind.Utc).AddTicks ticks

  match Iso8601.DateTime.tryParse input with
  | Some actual -> Assert.That(actual, Is.EqualTo expected)
  | None -> Assert.Fail $"Couldn't parse {input}"

[<Test;
  TestCase "15/09/2023 14:30:45.123";
  TestCase "09-15-2023 14:30:45.123";
  TestCase "2023/09/15 14:30:45.123";
  TestCase "15-09-2023 14:30:45.123+02:00";
  TestCase "2023.09.15 14:30:45.123-05:00";
  TestCase "15.09.2023 14:30:45.123Z";
  TestCase "09 15 2023 14:30:45.123+02:00";
  TestCase "Friday, 15 September 2023 14:30:45.123Z";
  TestCase "Sep 15, 2023 14:30:45.123-05:00">]
let ``Expect tryParse to return None on wrong date formats`` input =
  match Iso8601.DateTime.tryParse input with
  | Some _ -> Assert.Fail $"Shouldn't have parsed {input}"
  | None -> Assert.Pass $"Cannot parse {input} as expected"

[<Test;
  TestCase(2023, 9, 15, 14, 30, 45, 0, 0);
  TestCase(2023, 9, 15, 14, 30, 45, 123, 0);
  TestCase(2023, 9, 15, 14, 30, 45, 123, 4567)>]
let ``Expect printUtc then tryParse to return Some initial value`` year month day hours minutes seconds ms ticks =
  let expected =
    DateTime(year, month, day, hours, minutes, seconds, ms, DateTimeKind.Utc).AddTicks ticks

  match Iso8601.DateTime.printUtc >> Iso8601.DateTime.tryParse <| expected with
  | Some actual -> Assert.That(actual, Is.EqualTo expected)
  | None -> Assert.Fail $"Cannot parse {expected} after serialization"

[<Test;
  TestCase "2023-09-15T14:30:45.0000000Z";
  TestCase "2023-09-15T14:30:45.1230000Z";
  TestCase "2023-09-15T14:30:45.1234567Z">]
let ``Expect tryParse then printUtc to return Some initial value`` expected =
  match Iso8601.DateTime.tryParse >> Option.map Iso8601.DateTime.printUtc <| expected with
  | Some actual -> Assert.That(actual, Is.EqualTo expected)
  | None -> Assert.Fail $"Cannot parse {expected}"
