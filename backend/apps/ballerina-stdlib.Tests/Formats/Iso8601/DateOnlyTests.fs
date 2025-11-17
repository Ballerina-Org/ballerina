module Ballerina.StdLib.Tests.Formats.Iso8601.DateOnlyTests

open System
open Ballerina.StdLib.Formats
open NUnit.Framework

[<Test; TestCase(1999, 10, 30, "1999-10-30"); TestCase(2000, 3, 4, "2000-03-04")>]
let ``Expect print to return a yyyy-MM-dd date string`` year month day (expected: string) =
  let date = DateOnly(year, month, day)
  let actual = Iso8601.DateOnly.print date
  Assert.That(actual, Is.EqualTo expected)

[<Test;
  TestCase(1, 1, 1, "0001-01-01");
  TestCase(9999, 12, 31, "9999-12-31");
  TestCase(1999, 10, 30, "1999-10-30");
  TestCase(2000, 3, 4, "2000-03-04")>]
let ``Expect tryParse to return Some date`` year month day input =
  let expected = DateOnly(year, month, day)

  match Iso8601.DateOnly.tryParse input with
  | Some actual -> Assert.That(actual, Is.EqualTo expected)
  | None -> Assert.Fail $"Couldn't parse {input}"

[<Test;
  TestCase "2023/09/15";
  TestCase "15/09/2023";
  TestCase "09-15-2023";
  TestCase "2023/09/15";
  TestCase "15-09-2023";
  TestCase "2023.09.15";
  TestCase "15.09.2023";
  TestCase "09 15 2023";
  TestCase "Friday, 15 September 2023";
  TestCase "Sep 15, 2023">]
let ``Expect tryParse to return None on wrong date formats`` input =
  match Iso8601.DateOnly.tryParse input with
  | Some _ -> Assert.Fail $"Shouldn't have parsed {input}"
  | None -> Assert.Pass $"Cannot parse {input} as expected"

[<Test; TestCase(1999, 10, 30); TestCase(2000, 3, 4)>]
let ``Expect print then tryParse to return Some initial value`` year month day =
  let expected = DateOnly(year, month, day)

  match Iso8601.DateOnly.print >> Iso8601.DateOnly.tryParse <| expected with
  | Some actual -> Assert.That(actual, Is.EqualTo expected)
  | None -> Assert.Fail $"Cannot parse {expected} after serialization"

[<Test; TestCase "1999-10-30"; TestCase "2000-03-04">]
let ``Expect tryParse then print to return Some initial value`` expected =
  match Iso8601.DateOnly.tryParse >> Option.map Iso8601.DateOnly.print <| expected with
  | Some actual -> Assert.That(actual, Is.EqualTo expected)
  | None -> Assert.Fail $"Cannot parse {expected}"
