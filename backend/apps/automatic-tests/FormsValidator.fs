﻿module automatic_tests

open NUnit.Framework
open Ballerina.DSL.FormEngine.Runner
open Ballerina.Collections.Sum
open Ballerina.Errors

[<SetUp>]
let Setup () = ()

let rec iterateTestsOverFiles baseName start count printLastErrors =
  if count <= 0 then
    ()
  else
    let fileName = $"{baseName}{start}.json"
    do System.Console.WriteLine $"Filename = {fileName}"

    let actual =
      Ballerina.DSL.FormEngine.Runner.runSingle
        true
        FormsGenTarget.golang
        fileName
        "./generated-output/models"
        null
        null
        "./input-forms/go-config.json"

    match actual, count, printLastErrors with
    | Right err, 1, true -> Errors.Print "wrong predicate structure" err
    | _ -> ()

    Assert.That(actual.IsRight, Is.EqualTo(true))
    iterateTestsOverFiles baseName (start + 1) (count - 1) printLastErrors

[<Test>]
let CorrectSpec () =
  let actual =
    Ballerina.DSL.FormEngine.Runner.runSingle
      true
      FormsGenTarget.golang
      "./input-forms/person-config.json"
      "./generated-output/models"
      null
      null
      "./input-forms/go-config.json"

  match actual with
  | Right err -> Errors.Print "person-config" err
  | _ -> ()

  Assert.That(actual.IsLeft, Is.EqualTo(true))

[<Test>]
let MissingReference () =
  let actual =
    Ballerina.DSL.FormEngine.Runner.runSingle
      true
      FormsGenTarget.golang
      "./input-forms/with errors/missing reference.json"
      "./generated-output/models"
      null
      null
      "./input-forms/go-config.json"

  Assert.That(actual.IsRight, Is.EqualTo(true))

[<Test>]
let WrongEnumStructure () =
  iterateTestsOverFiles "./input-forms/with errors/wrong enum structure " 1 9 false

[<Test>]
let WrongTypeStructure () =
  iterateTestsOverFiles "./input-forms/with errors/wrong type structure " 1 9 false

[<Test>]
let WrongAPIStructure () =
  iterateTestsOverFiles "./input-forms/with errors/wrong api structure " 1 9 false

[<Test>]
let WrongFormStructure () =
  iterateTestsOverFiles "./input-forms/with errors/wrong form structure " 1 9 false

[<Test>]
let WrongPredicateStructure () =
  iterateTestsOverFiles "./input-forms/with errors/wrong predicate structure " 1 9 false
