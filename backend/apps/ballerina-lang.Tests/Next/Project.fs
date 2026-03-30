module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Project

open NUnit.Framework
open Ballerina.StdLib.Object
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.Runners.Project
open Ballerina.Collections.NonEmptyList
open Ballerina.Errors
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.MutableMemoryDB

type private ValueExt = ValueExt<unit, MutableMemoryDB<unit, unit>, unit>

let private fileFromNameAndContent (name: string) (content: string) : FileBuildConfiguration =
  { FileName = { Path = name }
    Content = fun () -> content
    Checksum = Checksum.Compute content
    RequireUnitTermination = true }

let private fileFromNameAndContentWithTermination
  (name: string)
  (content: string)
  (requireUnitTermination: bool)
  : FileBuildConfiguration =
  { FileName = { Path = name }
    Content = fun () -> content
    Checksum = Checksum.Compute content
    RequireUnitTermination = requireUnitTermination }

let private buildAndEvalFromConfiguredFiles
  (files: NonEmptyList<FileBuildConfiguration>)
  : Sum<Value<TypeValue<ValueExt>, ValueExt> * TypeValue<ValueExt> * int, string> =
  let project: ProjectBuildConfiguration = { Files = files }

  let context = Term.Expr_Eval.context
  let cache = memcache (context.TypeCheckContext, context.TypeCheckState)
  let typeEvalConfig = Term.Expr_Eval.typeEvalConfig

  let buildResult = ProjectBuildConfiguration.BuildCached typeEvalConfig cache project

  match buildResult with
  | Left(exprs, typeValue, _, finalState) ->
    let evalContext = ExprEvalContext.Empty() |> context.ExprEvalContext

    let evalContext =
      ExprEvalContext.WithTypeCheckingSymbols evalContext finalState.Symbols

    let evalResult =
      Expr.Eval(NonEmptyList.prependList context.TypeCheckedPreludes exprs)
      |> Reader.Run evalContext

    match evalResult with
    | Left value -> Left(value, typeValue, NonEmptyList.ToList exprs |> List.length)
    | Right(e: Errors.Errors<Patterns.Location>) ->
      let errString = Errors.ToString(e, "\n")
      Right(sprintf "Evaluation failed: %s" errString)
  | Right e ->
    let errString = Errors.ToString(e, "\n")
    Right(sprintf "Build failed: %s" errString)

let private buildAndEval
  (files: NonEmptyList<string * string>)
  : Sum<Value<TypeValue<ValueExt>, ValueExt> * TypeValue<ValueExt> * int, string> =
  files
  |> NonEmptyList.map (fun (name, content) -> fileFromNameAndContent name content)
  |> buildAndEvalFromConfiguredFiles


[<Test>]
let ``Project with three files each defining a type and term referenced by subsequent files`` () =
  let file1 =
    "file1.bl",
    """
type Point = { X: int32; Y: int32; }
in let origin: Point = { X = 0; Y = 0; }
in ()
    """

  let file2 =
    "file2.bl",
    """
type Line = { Start: Point; End: Point; }
in let horizontal: Line = { Start = origin; End = { X = 10; Y = 0; }; }
in ()
    """

  let file3 =
    "file3.bl",
    """
type Triangle = { A: Point; B: Point; C: Point; }
in let tri: Triangle = { A = horizontal.Start; B = horizontal.End; C = { X = 5; Y = 10; }; }
in tri.A.X + tri.B.X + tri.C.X
    """

  let result = buildAndEval (NonEmptyList.OfList(file1, [ file2; file3 ]))

  match result with
  | Left(value, typeValue, exprCount) ->
    Assert.That(exprCount, Is.EqualTo(3), "Should have 3 expressions")

    match value with
    | Value.Primitive(Int32 15) ->
      match typeValue with
      | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> Assert.Pass "Correctly evaluated to 15 (0 + 10 + 5)"
      | _ -> Assert.Fail $"Expected Int32 type, got {typeValue.AsFSharpString}"
    | _ -> Assert.Fail $"Expected 15, got {value}"


  | Right e -> Assert.Fail e


[<Test>]
let ``Project file order matters - wrong order fails`` () =
  let file1 =
    "file1.bl",
    """
let p: Point = { X = 0; Y = 0; }
in ()
    """

  let file2 =
    "file2.bl",
    """
type Point = { X: int32; Y: int32; }
in ()
    """

  let result = buildAndEval (NonEmptyList.OfList(file1, [ file2 ]))

  match result with
  | Left _ -> Assert.Fail "Should have failed because Point is not defined yet in file1"
  | Right e ->
    Assert.That(e, Does.Contain("Point"), "Error should mention missing Point type")
    Assert.Pass "Correctly failed due to wrong file order"

[<Test>]
let ``Preludes must evaluate to unit`` () =
  let wrongPrelude =
    "wrongPrelude.bl",
    """
let x: int32 = 10
in 1
    """

  let program =
    "program.bl",
    """
x * 2
    """

  let result = buildAndEval (NonEmptyList.OfList(wrongPrelude, [ program ]))

  match result with
  | Left(value, _, _) -> Assert.Fail $"Expected evaluation to fail, got {value}"
  | Right e -> Assert.That(e, Does.Contain("x"), "Error should mention missing x variable")

[<Test>]
let ``Non-last project files must terminate with constant unit`` () =
  let file1 =
    "file1.bl",
    """
1
    """

  let file2 =
    "file2.bl",
    """
2
    """

  let result = buildAndEval (NonEmptyList.OfList(file1, [ file2 ]))

  match result with
  | Left _ -> Assert.Fail "Expected build to fail because non-last file is not terminated by constant unit"
  | Right e ->
    Assert.That(e, Does.Contain("file1.bl"), "Error should point to the offending file")
    Assert.That(e, Does.Contain("terminated by constant unit"), "Error should mention constant unit termination")

[<Test>]
let ``Last project file can return non-unit`` () =
  let file1 =
    "file1.bl",
    """
()
    """

  let file2 =
    "file2.bl",
    """
2
    """

  let result = buildAndEval (NonEmptyList.OfList(file1, [ file2 ]))

  match result with
  | Left(value, typeValue, exprCount) ->
    Assert.That(exprCount, Is.EqualTo(2), "Should have 2 expressions")

    match value with
    | Value.Primitive(Int32 2) ->
      match typeValue with
      | TypeValue.Primitive({ value = PrimitiveType.Int32 }) ->
        Assert.Pass "Last file correctly returns a non-unit value"
      | _ -> Assert.Fail $"Expected Int32 type, got {typeValue.AsFSharpString}"
    | _ -> Assert.Fail $"Expected 2, got {value}"
  | Right e -> Assert.Fail e

[<Test>]
let ``Non-last project file allows non-unit when RequireUnitTermination is false`` () =
  let file1 = fileFromNameAndContentWithTermination "file1.bl" "1" false
  let file2 = fileFromNameAndContentWithTermination "file2.bl" "2" true

  let result = buildAndEvalFromConfiguredFiles (NonEmptyList.OfList(file1, [ file2 ]))

  match result with
  | Left(_, _, exprCount) ->
    Assert.That(exprCount, Is.EqualTo(2), "Should have 2 expressions")
    Assert.Pass "Non-last file with disabled termination requirement should succeed"
  | Right e -> Assert.Fail e
