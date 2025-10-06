module Ballerina.Data.Tests.Seeds.EndToEnd

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
open Ballerina.Errors
open Ballerina.Reader.WithError
open Ballerina.Seeds
open Ballerina.State.WithError
open Ballerina.StdLib.Json.Patterns
open Ballerina.Seeds.Test
open NUnit.Framework
open FSharp.Data
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.List

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
        let! typesJson = JsonValue.AsRecord json |> state.OfSum

        let! types =
          typesJson
          |> List.ofArray
          |> List.map (fun (name, value) -> TypeExpr.FromJson value |> sum.Map(fun typeExpr -> name, typeExpr))
          |> sum.All
          |> state.OfSum

        do!
          types
          |> List.map (fun (name, expr) ->
            state {
              let! tv = TypeExpr.Eval None expr
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

        let fieldKey =
          personRec |> Map.findKey (fun key _ -> key.Name = LocalScope "Private")

        let field = personRec |> Map.find fieldKey
        let! nestedRec = Value.AsRecord field

        let nestedKey =
          nestedRec |> Map.findKey (fun key _ -> key.Name = LocalScope "Email")

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
        let fieldKey = personRec |> Map.findKey (fun key _ -> key.Name = LocalScope "Email")
        let field = personRec |> Map.find fieldKey
        let! emailStr = Value.AsPrimitive field
        let! email = PrimitiveValue.AsString emailStr
        return email, personRec
      }
      |> function
        | Right _ -> Assert.Fail("Verification of the email in the types failed")
        | Left(email, personRec) ->
          let actualFields =
            personRec |> Map.keys |> Seq.map _.Name.ToString() |> Seq.toList |> List.sort

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

        let fieldKey =
          companyRec |> Map.findKey (fun key _ -> key.Name = LocalScope "Emails")

        let field = companyRec |> Map.find fieldKey

        let! ext = field |> Value.AsExt
        let choice = ValueExt.Getters.ValueExt ext

        let! listValues =
          match choice with
          | Choice1Of3(ListExt.ListValues v) -> sum.Return v
          | _ -> sum.Throw(Errors.Singleton "Expected List, got other ext")

        let (List values) = listValues

        let! values = values |> Seq.map Value.AsPrimitive |> sum.All
        let! strings = values |> Seq.map PrimitiveValue.AsString |> sum.All
        return strings
      }
      |> function
        | Right er -> Assert.Fail($"Verification of the email in the types failed :{er}")
        | Left values -> Assert.That(List.forall isEmail values, Is.True)
