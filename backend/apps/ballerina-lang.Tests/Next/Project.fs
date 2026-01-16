module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Project

open NUnit.Framework
open Ballerina.StdLib.Object
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.Runners.Project
open Ballerina.Collections.NonEmptyList


let private fileFromNameAndContent (name: string) (content: string) : FileBuildConfiguration =
  { FileName = { Path = name }
    Content = fun () -> content
    Checksum = Checksum.Compute content }

let private buildAndEval (files: NonEmptyList<string * string>) =
  let project: ProjectBuildConfiguration =
    { Files =
        files
        |> NonEmptyList.map (fun (name, content) -> fileFromNameAndContent name content) }

  let context = Term.Expr_Eval.context
  let cache = memcache (context.TypeCheckContext, context.TypeCheckState)

  let buildResult = ProjectBuildConfiguration.BuildCached cache project

  match buildResult with
  | Left(exprs, _, finalState) ->
    let evalContext =
      ExprEvalContext.WithTypeCheckingSymbols context.ExprEvalContext finalState.Symbols

    let evalResult = Expr.Eval exprs |> Reader.Run evalContext

    match evalResult with
    | Left value -> Left(value, NonEmptyList.ToList exprs |> List.length)
    | Right e -> Right $"Evaluation failed: {e.AsFSharpString}"
  | Right e -> Right $"Build failed: {e.AsFSharpString}"


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
  | Left(value, exprCount) ->
    Assert.That(exprCount, Is.EqualTo(3), "Should have 3 expressions")

    match value with
    | Value.Primitive(Int32 15) -> Assert.Pass "Correctly evaluated to 15 (0 + 10 + 5)"
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
  | Left(value, _) -> Assert.Fail $"Expected evaluation to fail, got {value}"
  | Right e -> Assert.That(e, Does.Contain("x"), "Error should mention missing x variable")
