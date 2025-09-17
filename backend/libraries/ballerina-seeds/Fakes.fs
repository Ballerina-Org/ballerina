namespace Ballerina.Seeds.Fakes

open Bogus

open System
open System.Globalization

open Ballerina.DSL.Next.Terms.Model
open Ballerina.Seeds
open Ballerina.DSL.Next.Types.Model

type TextKind =
  | Word
  | Paragraph
  | Sentence
  | VarName of lastUsedIndex: int

type FakeValue =
  | Unsupervised
  | Supervised of hint: string * lastIndex: int

type FakeOrRealValue =
  | RealValue of obj
  | FakeValue of FakeValue

type PrimitiveGenerator<'T, 'valueExtension> = PrimitiveType -> FakeOrRealValue -> Value<'T, 'valueExtension>

[<AutoOpen>]
module private Patterns =
  let (|AsBool|AsString|AsInt|AsFloat|AsDecimal|AsDateTime|Other|) (o: obj) =
    match o with
    | :? bool as v -> AsBool v
    | :? string as v -> AsString v
    | :? int as v -> AsInt v
    | :? float as v -> AsFloat v
    | :? decimal as v -> AsDecimal v
    | :? DateTime as v -> AsDateTime v
    | x -> Other x

type BogusDataGenerator<'Value>(cultureCode: string) =
  let randomSeed = 1234
  let faker = Faker cultureCode
  let fakeFor p = Hints.``for`` faker p
  let random = Random randomSeed
  let culture = CultureInfo.GetCultureInfo cultureCode

  let fd (d: DateTime) =
    let fmt = CultureInfo.InvariantCulture.DateTimeFormat
    d.ToString(fmt.ShortDatePattern, culture)

  let tryParseBool (value: string) : bool option =
    match Boolean.TryParse(value) with
    | true, result -> Some result
    | false, _ -> None

  let letters = [ 'a' .. 'z' ]

  member this.InfiniteNames: seq<string> =
    seq {
      yield! letters |> Seq.map string

      for i in 1 .. Int32.MaxValue do
        for ch in letters do
          yield string ch + string i
    }

  member this.NextName(index: int) =
    let letterIndex = index % letters.Length
    let suffix = index / letters.Length

    if suffix = 0 then
      string letters[letterIndex]
    else
      string letters[letterIndex] + string suffix

  member t.RealString(value: string) =
    (PrimitiveType.String, (FakeOrRealValue.RealValue value))
    ||> t.PrimitiveValueCons

  member t.RealBool(value: bool) =
    (PrimitiveType.Bool, FakeOrRealValue.RealValue value) ||> t.PrimitiveValueCons

  member t.RealInt(value: int) =
    (PrimitiveType.Int32, FakeOrRealValue.RealValue value) ||> t.PrimitiveValueCons

  member t.RealFloat(value: float) =
    (PrimitiveType.Decimal, FakeOrRealValue.RealValue value)
    ||> t.PrimitiveValueCons

  member val Culture: CultureInfo = culture with get

  member t.String(v: TextKind) =
    match v with
    | Word -> faker.Lorem.Word()
    | Paragraph -> faker.Lorem.Paragraph()
    | Sentence -> faker.Lorem.Sentence()
    | VarName i -> t.NextName i

  member t.Suggestion(person, suggestion) = fakeFor person suggestion

  member t.InfiniteSource(suggestion: string) =
    let person i = t.Person(cultureCode, i)
    Seq.initInfinite (fun i -> t.Suggestion(person i, suggestion))

  member t.Bool() = random.Next(0, 2) = 1
  member t.Int32(min, max) = random.Next(min, max)
  member t.Int64(min, max) = random.NextInt64(min, max)

  member t.Float32(min, max) =
    let value = random.NextSingle() * (max - min) + min
    single value

  member t.Float64(min, max) =
    let value = random.NextDouble() * (max - min) + min
    double value

  member t.Guid() = Guid.CreateVersion7()

  member t.PrimitiveValueCons pt input =

    let result =
      match pt, input with
      | PrimitiveType.String, FakeValue Unsupervised -> PrimitiveValue.String(t.String Word)
      | PrimitiveType.String, FakeValue(Supervised(label, i)) ->
        PrimitiveValue.String(t.InfiniteSource label |> Seq.skip i |> Seq.head)
      | PrimitiveType.String, RealValue v -> PrimitiveValue.String(string v)

      | PrimitiveType.Int32, FakeValue _ -> PrimitiveValue.Int32(t.Int32(0, 1000))
      | PrimitiveType.Int32, RealValue v ->
        match v with
        | AsInt i -> PrimitiveValue.Int32 i
        | _ -> failwith "Expected int"

      | PrimitiveType.Int64, FakeValue _ -> PrimitiveValue.Int64(t.Int64(0L, 1000L))
      | PrimitiveType.Int64, RealValue v ->
        match v with
        | AsInt i -> PrimitiveValue.Int64(int64 i)
        | _ -> failwith "Expected int"

      | PrimitiveType.Float32, FakeValue _ -> PrimitiveValue.Float32(t.Float32(0.f, 1000.f) |> single)
      | PrimitiveType.Float32, RealValue v ->
        match v with
        | AsFloat i -> PrimitiveValue.Float32(single i)
        | _ -> failwith "Expected float"

      | PrimitiveType.Float64, FakeValue _ -> PrimitiveValue.Float64(t.Float64(0., 1000.))
      | PrimitiveType.Float64, RealValue v ->
        match v with
        | AsFloat i -> PrimitiveValue.Float64 i
        | _ -> failwith "Expected float"

      | PrimitiveType.Decimal, FakeValue _ -> PrimitiveValue.Decimal(t.Float64(0., 1000.) |> decimal)
      | PrimitiveType.Decimal, RealValue v ->
        match v with
        | AsDecimal i -> PrimitiveValue.Decimal i
        | _ -> failwith "Expected decimal"

      | PrimitiveType.Bool, FakeValue Unsupervised -> PrimitiveValue.Bool(t.Bool())
      | PrimitiveType.Bool, FakeValue(Supervised(label, _i)) ->
        PrimitiveValue.Bool(tryParseBool label |> Option.defaultValue true) //TODO: dangerous
      | PrimitiveType.Bool, RealValue value ->
        match value with
        | AsBool value -> PrimitiveValue.Bool value
        | _ -> failwith "Expected bool"

      | PrimitiveType.Guid, FakeValue _ -> PrimitiveValue.String(t.Guid() |> string)
      | PrimitiveType.Guid, RealValue v -> PrimitiveValue.String(string v)

      | PrimitiveType.DateTime, FakeValue _ ->
        DateTime.Now.AddDays(float (t.Int32(-365, 0))) |> fd |> PrimitiveValue.String
      | PrimitiveType.DateTime, RealValue v ->
        match v with
        | AsDateTime dt -> fd dt |> PrimitiveValue.String
        | _ -> failwith "Expected DateTime"

      | PrimitiveType.DateOnly, FakeValue _ ->
        DateTime.Now.AddDays(float (t.Int32(-365, 0))) |> fd |> PrimitiveValue.String
      | PrimitiveType.DateOnly, RealValue v ->
        match v with
        | AsDateTime d -> fd d |> PrimitiveValue.String
        | _ -> failwith "Expected DateTime"

      | PrimitiveType.Unit, RealValue _
      | PrimitiveType.Unit, FakeValue _ -> PrimitiveValue.Unit

    result |> Value.Primitive

  member this.Person(locale: string, index: int) =
    let faker = Faker(locale)
    faker.Random <- Randomizer(index)
    faker.Person

module Runner =
  let private culture = CultureInfo.CurrentCulture
  let private region = RegionInfo culture.Name

  let en () = BogusDataGenerator "en"

  let current () =
    BogusDataGenerator(region.TwoLetterISORegionName.ToLower())

  let custom (locale: string) = BogusDataGenerator locale
