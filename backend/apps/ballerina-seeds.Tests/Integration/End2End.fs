module Ballerina.Data.Tests.Seeds.EndToEnd

open System
open System.Text.RegularExpressions
open Ballerina.Collections.NonEmptyList
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Json
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.DSL.Next.Terms.Json
open Ballerina.LocalizedErrors
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.Reader.WithError
open Ballerina.Seeds
open Ballerina.State.WithError
open Ballerina.StdLib.Json.Patterns
open Ballerina.Seeds.Test
open NUnit.Framework
open FSharp.Data
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib
open Ballerina.Data.Spec.Json
open Ballerina.Data.Spec.Model
open Ballerina.Data.Schema.Model
open Ballerina.Data.TypeEval
open Ballerina.Data.Schema

let extensions, languageContext = stdExtensions

let valueEncoderRoot =
  Json.buildRootEncoder<TypeValue, ValueExt> (NonEmptyList.OfList(Value.ToJson, [ extensions.List.Encoder ]))

let rootExprEncoder = Expr.ToJson >> Reader.Run TypeValue.ToJson
let encoder = valueEncoderRoot >> Reader.Run(rootExprEncoder, TypeValue.ToJson)

let emailRgx = Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)
let isEmail (s: string) = emailRgx.IsMatch s

let seedCtx = SeedingContext.Default()
let seedState = SeedingState.Default(languageContext.TypeCheckState.Types)

let private evalJsonAndSeed (str: string) =
  sum {

    let! bindings, _evalStateOpt =
      state {
        let json = str |> JsonValue.Parse

        let! typesJson =
          JsonValue.AsRecord json
          |> Sum.mapRight (Errors.FromErrors Location.Unknown)
          |> state.OfSum

        let! types =
          typesJson
          |> List.ofArray
          |> List.map (fun (name, value) -> TypeExpr.FromJson value |> sum.Map(fun typeExpr -> name, typeExpr))
          |> sum.All
          |> Sum.mapRight (Errors.FromErrors Location.Unknown)
          |> state.OfSum

        do!
          types
          |> List.map (fun (name, expr) ->
            state {
              let! tv = TypeExpr.Eval None Location.Unknown expr
              do! TypeExprEvalState.bindType name tv
              return ()
            })
          |> state.All
          |> state.Map(fun _ -> ())

        let! s = state.GetState()
        return s.Bindings
      }
      |> State.Run(languageContext.TypeCheckContext.Types, languageContext.TypeCheckState.Types)
      |> sum.MapError fst

    let! seeds, _seedStateOpt =
      bindings
      |> Map.filter (fun _i (t, _k) -> not t.IsLambda)
      |> Map.map (fun key (typeValue, kind) -> Traverser.seed typeValue |> state.Map(fun seed -> key, kind, seed))
      |> state.AllMap
      |> State.Run(seedCtx, seedState)
      |> sum.MapError fst

    return seeds
  }

[<Test>]
let ``Seeds: Json with nested records (Spec fragment) -> Parse TypeExpr -> Eval -> TypeValue -> Traverser -> Seeded with real email``
  ()
  =

  evalJsonAndSeed SampleData.Records.nested
  |> function
    | Right e -> Assert.Fail(e.Errors.Head.Message)
    | Left seeds ->

      let _, _, person = seeds |> Map.find (Identifier.LocalScope "Person")

      sum {
        let! _json =
          encoder person
          |> sum.Map(fun json -> json.ToString JsonSaveOptions.DisableFormatting)

        let! personRec = Value.AsRecord person

        let fieldKey = personRec |> Map.findKey (fun key _ -> key = LocalScope "Private")

        let field = personRec |> Map.find fieldKey
        let! nestedRec = Value.AsRecord field

        let nestedKey = nestedRec |> Map.findKey (fun key _ -> key = LocalScope "Email")

        let nested = nestedRec |> Map.find nestedKey
        let! nestedValue = Value.AsPrimitive nested
        let! email = PrimitiveValue.AsString nestedValue
        return email
      }
      |> function
        | Right _ -> Assert.Fail("Verification of the email in the types failed")
        | Left email ->
          Assert.That(
            isEmail email,
            Is.True,
            """if we defined a nested field named 'email', 
                                              it is parsed, evaluated and seeded with sth that is a faked email,
                                              then the end to end flow works"""
          )

[<Test>]
let ``Seeds: Json with Flatten expression (Spec fragment) -> Parse TypeExpr -> Eval -> TypeValue -> Traverser -> Seeded with real email``
  ()
  =

  let _why =
    """
        in v1 lang we have 'extends' keyword to inherit and hence not repeat same data fields
        in v2, instead or relying on the parser/validator, we can use Flatten TypeExpr
        this test verifies if such a structure is seeded as expected """

  evalJsonAndSeed SampleData.Records.flatten
  |> function
    | Right e -> Assert.Fail(e.Errors.Head.Message)
    | Left seeds ->

      let _, _, person = seeds |> Map.find (Identifier.LocalScope "Person")

      sum {
        let! _json =
          encoder person
          |> sum.Map(fun json -> json.ToString JsonSaveOptions.DisableFormatting)

        let! personRec = Value.AsRecord person
        let fieldKey = personRec |> Map.findKey (fun key _ -> key = LocalScope "Email")
        let field = personRec |> Map.find fieldKey
        let! emailStr = Value.AsPrimitive field
        let! email = PrimitiveValue.AsString emailStr
        return email, personRec
      }
      |> function
        | Right _ -> Assert.Fail("Verification of the email in the types failed")
        | Left(email, personRec) ->
          let actualFields =
            personRec |> Map.keys |> Seq.map _.ToString() |> Seq.toList |> List.sort

          Assert.That(actualFields, Is.EquivalentTo [ "Age"; "Email"; "Name" ])
          Assert.That(isEmail email, Is.True)

[<Test>]
let ``Seeds: List extension`` () =

  let _why =
    """
        in v1 lang we have ExprType.ListType for lists
        in v2, lists, options and primitive types are extensions and are preloaded as a lang context
        this test verifies if such a structure is seeded as expected """

  evalJsonAndSeed SampleData.Records.withList
  |> function
    | Right e -> Assert.Fail(e.Errors.Head.Message)
    | Left seeds ->

      let _, _, company = seeds |> Map.find (Identifier.LocalScope "Company")

      sum {
        let! _json =
          encoder company
          |> sum.Map(fun json -> json.ToString JsonSaveOptions.DisableFormatting)

        let! companyRec = Value.AsRecord company

        let fieldKey = companyRec |> Map.findKey (fun key _ -> key = LocalScope "Emails")

        let field = companyRec |> Map.find fieldKey

        let! ext = field |> Value.AsExt
        let choice = ValueExt.Getters.ValueExt ext

        let! listValues =
          match choice with
          | Choice1Of3(ListExt.ListValues v) -> sum.Return v
          | _ -> sum.Throw(Ballerina.Errors.Errors.Singleton "Expected List, got other ext")

        let (List.Model.ListValues.List values) = listValues

        let! values = values |> Seq.map Value.AsPrimitive |> sum.All
        let! strings = values |> Seq.map PrimitiveValue.AsString |> sum.All
        return strings
      }
      |> function
        | Right er -> Assert.Fail($"Verification of the email in the types failed :{er}")
        | Left values -> Assert.That(List.forall isEmail values, Is.True)

let analyze (data: Map<Guid, Set<Guid>>) =
  let counts = data |> Map.map (fun _ -> Set.count)

  let reverse =
    data
    |> Seq.collect (fun (KeyValue(k, set)) -> set |> Seq.map (fun v -> v, k))
    |> Seq.groupBy fst
    |> Seq.map (fun (v, pairs) -> v, pairs |> Seq.map snd |> Set.ofSeq |> Set.count)
    |> Map.ofSeq

  counts, reverse

let insert
  (ctx: SeedingContext)
  : Sum<Value<TypeValue, ValueExt> * Option<TypeExprEvalState>, Errors * Option<TypeExprEvalState>> =
  let json = SampleData.Specs.PersonGenders |> JsonValue.Parse

  state {
    let! spec =
      V2Format.FromJson json
      |> Sum.mapRight (Errors.FromErrors Location.Unknown)
      |> state.OfSum

    do! Ballerina.Data.Spec.Builder.typeContextFromSpecBody spec

    let lookupPath =
      spec.Schema.Lookups
      |> Map.toSeq
      |> Seq.head
      |> (fun (_name, data) -> data.Forward.Path)

    let! schema = spec.Schema |> Schema.SchemaEval

    let! seeds = Runner.seed schema |> Reader.Run ctx |> state.OfSum
    let lookups = seeds.Lookups |> Map.find { LookupName = "PeopleGenders" }
    let sampleOneToMany = lookups |> Map.toSeq |> Seq.head
    let counts, reverse = analyze lookups

    Assert.That(Map.forall (fun _ v -> v = 1) counts, Is.True, "Arity is preserved after seeding")
    Assert.That(Map.forall (fun _ v -> v = 1) reverse, Is.True, "Arity is preserved after seeding")

    let people =
      seeds.Entities
      |> Map.find { EntityName = "People" }
      |> Map.find (fst sampleOneToMany)

    let genders =
      seeds.Entities
      |> Map.find { EntityName = "Genders" }
      |> Map.find (snd sampleOneToMany |> Set.minElement)

    return!
      Value.insert genders people lookupPath
      |> Sum.mapRight (Errors.FromErrors Location.Unknown)
      |> state.OfSum
  }
  |> State.Run(languageContext.TypeCheckContext.Types, languageContext.TypeCheckState.Types)

let private tryExtractGender
  (value: Value<TypeValue, ValueExt>)
  : Sum<option<Identifier> * Map<Identifier, Value<TypeValue, ValueExt>>, Errors> =
  sum {
    let! record = Value.AsRecord value |> Sum.mapRight (Errors.FromErrors Location.Unknown)

    let! _, biology =
      record
      |> Map.tryFindByWithError
        (fun (k, _v) -> k = LocalScope "Biology")
        "record field"
        "biology field not found"
        Location.Unknown

    let! biology = biology |> Value.AsTuple |> Sum.mapRight (Errors.FromErrors Location.Unknown)
    let! _, biology = Value.AsUnion biology.Head |> Sum.mapRight (Errors.FromErrors Location.Unknown)
    let! fields = Value.AsRecord biology |> Sum.mapRight (Errors.FromErrors Location.Unknown)
    let genderKey = fields |> Map.tryFindKey (fun ts _ -> ts = LocalScope "Gender")
    return genderKey, fields
  }

[<Test>]
let ``Seed lookups, insert item via path, path satisfied results in item being inserted`` () =
  sum {
    let! result, _ =
      insert
        { SeedingContext.Default() with
            WantedCount = Some 10
            PickItemStrategy = First } // ensures Public union case (satisfies path) is seeded
      |> sum.MapError fst

    // check json content for curiosity
    let! _json =
      encoder result
      |> sum.Map _.ToString()
      |> Sum.mapRight (Errors.FromErrors Location.Unknown)

    let! genderKey, fields = tryExtractGender result

    let! genderKey =
      genderKey
      |> sum.OfOption(Errors.Singleton(Location.Unknown, "Gender not found"))

    let gender = fields |> Map.find genderKey
    let! gender = Value.AsTuple gender |> Sum.mapRight (Errors.FromErrors Location.Unknown)
    let gender = gender |> List.head
    let! ts, _ = Value.AsUnion gender |> Sum.mapRight (Errors.FromErrors Location.Unknown)
    return ts.Name.LocalName
  }
  |> function
    | Right error -> Assert.Fail error.Errors.Head.Message
    | Left gender -> Assert.That([ "F"; "M"; "X" ] |> List.contains gender, Is.True)

[<Test>]
let ``Seed lookups, insert item via path, path not satisfied results with no insert`` () =
  sum {
    let! result, _ =
      insert
        { SeedingContext.Default() with
            WantedCount = Some 10
            PickItemStrategy = Last } // ensures Secret union case (does not satisfy path) is seeded
      |> sum.MapError fst

    let! record = Value.AsRecord result |> Sum.mapRight (Errors.FromErrors Location.Unknown)

    let! _, biology =
      record
      |> Map.tryFindByWithError
        (fun (k, _v) -> k = LocalScope "Biology")
        "record field"
        "biology field not found"
        Location.Unknown

    let! biology = biology |> Value.AsTuple |> Sum.mapRight (Errors.FromErrors Location.Unknown)
    let head = biology |> List.exactlyOne
    let! unionCase = Value.AsUnion head |> Sum.mapRight (Errors.FromErrors Location.Unknown)
    return unionCase
  }
  |> function
    | Right error -> Assert.Fail $"Test assumes no errors but got {error.Errors.Head.Message}"
    | Left(_, unionCase) ->
      Assert.That(unionCase.IsPrimitive, Is.True, "biology remains a union case that has unit, not an inserted field")
