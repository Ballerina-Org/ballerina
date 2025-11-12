namespace Ballerina.StdLib.Formats

module Iso8601 =
  open System
  open System.Globalization

  [<Literal>]
  let private dateOnlyFormat = "yyyy-MM-dd"

  let private formats =
    [| dateOnlyFormat
       "yyyy-MM-dd'T'HH:mm"
       "yyyy-MM-dd'T'HH:mm:ss"
       "yyyy-MM-dd'T'HH:mm:ss.FFF"
       "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF"
       "yyyy-MM-dd'T'HH:mmK"
       "yyyy-MM-dd'T'HH:mm:ssK"
       "yyyy-MM-dd'T'HH:mm:ss.FFFK"
       "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK" |]

  // DateTimeStyles.RoundtripKind assumes DateTimeKind.Local if any time zone is specified (except Z),
  // meaning the timeZone is lost in the most cases
  let private styles =
    DateTimeStyles.AssumeUniversal ||| DateTimeStyles.AdjustToUniversal

  [<RequireQualifiedAccess>]
  module DateOnly =
    let print (date: DateOnly) =
      date.ToString("o", CultureInfo.InvariantCulture)

    let tryParse (input: string) =
      match DateOnly.TryParseExact(input, dateOnlyFormat, CultureInfo.InvariantCulture, DateTimeStyles.None) with
      | true, value -> Some value
      | false, _ -> None

  [<RequireQualifiedAccess>]
  module DateTime =
    // ToUniversalTime treat Unspecified as Local, adjusting it accordingly
    let printUtc (date: DateTime) =
      date.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)

    let tryParse (input: string) =
      match DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, styles) with
      | true, value -> Some value
      | false, _ -> None
