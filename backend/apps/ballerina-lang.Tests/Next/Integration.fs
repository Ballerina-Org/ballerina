module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Integration

open NUnit.Framework
open FSharp.Data
open Ballerina.StdLib.Object
open Ballerina.StdLib.String
open Ballerina.Collections.Sum
open Ballerina.Coroutines.Model
open Ballerina.Reader.WithError
open Ballerina.Errors
open System
open Ballerina.DSL.Next.Unification
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.TypeCheck
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Terms
open Ballerina.State.WithError
open Ballerina.DSL.Next.StdLib.Option
open Ballerina.DSL.Next.Extensions
open Ballerina.LocalizedErrors
open Ballerina.Parser
open Ballerina.Parser
open Ballerina.StdLib.Object
open Ballerina.DSL.Next.Syntax
open Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Expr_Eval

let context = Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Expr_Eval.context

let private run program =
  let initialLocation = Location.Initial "input"

  let actual =
    Ballerina.DSL.Next.Syntax.Lexer.tokens
    |> Parser.Run(program |> Seq.toList, initialLocation)

  match actual with
  | Right e -> Right $"Failed to tokenize program: {e.ToFSharpString}"
  | Left(ParserResult(actual, _)) ->

    let parsed = Parser.Expr.program |> Parser.Run(actual, initialLocation)

    match parsed with
    | Right e -> Right $"Failed to parse program: {e.ToFSharpString}"
    | Left(ParserResult(program, _)) ->

      let typeCheckResult =
        Expr.TypeCheck program
        |> State.Run(context.TypeCheckContext, context.TypeCheckState)

      match typeCheckResult with
      | Left((program, typeValue, _), typeCheckFinalState) ->

        let evalContext = context.ExprEvalContext

        let typeCheckedSymbols =
          match typeCheckFinalState with
          | None -> []
          | Some s -> s.Types.Symbols |> Map.toList

        let unionCaseConstructors =
          match typeCheckFinalState with
          | None -> []
          | Some s ->
            s.Types.UnionCases
            |> Map.map (fun k _ ->
              Value<TypeValue, ValueExt>
                .Lambda("_" |> Var.Create, Expr.UnionCons(k, "_" |> Identifier.LocalScope |> Expr.Lookup)))
            |> Map.toList

        let evalContext =
          { evalContext with
              Symbols = (evalContext.Symbols |> Map.toList) @ typeCheckedSymbols |> Map.ofList
              // Values: Map<Identifier, Value<TypeValue, 'valueExtension>>
              Values = (evalContext.Values |> Map.toList) @ unionCaseConstructors |> Map.ofList }

        let evalResult = Expr.Eval program |> Reader.Run evalContext

        match evalResult with
        | Left value -> Sum.Left(value, typeValue)
        | Right e -> Sum.Right $"Evaluation failed: {e.ToFSharpString}"
      | Right(e, _) -> Sum.Right $"Type checking failed: {e.ToFSharpString}"

[<Test>]
let ``LangNext-Integration let over int succeeds`` () =
  let program = """let x = 10 in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Int32 10)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")



[<Test>]
let ``LangNext-Integration let over decimal succeeds`` () =
  let program = """let x = 10.5 in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Decimal 10.5M)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Decimal }) -> ()
    | _ -> Assert.Fail($"Expected Decimal type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")



[<Test>]
let ``LangNext-Integration let over bool succeeds`` () =
  let program = """let x = true in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Bool true)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")



[<Test>]
let ``LangNext-Integration let over string succeeds`` () =
  let program = """let x = "Hello world!" in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(String "Hello world!")

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.String }) -> ()
    | _ -> Assert.Fail($"Expected String type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration let over boolean expression succeeds`` () =
  let program = """let x = false || !true && false in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Bool false)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration let over integer expression succeeds`` () =
  let program = """let x = 3 + 5 * 10 - 10 / 2 in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Int32 48)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration let over conditional expression succeeds`` () =
  let program = """let x = if 1 < 2 then true else false in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Bool true)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration let over conditional composite expression succeeds`` () =
  let program =
    """// complex expression with all operators
let x = if true then 1 + 3 * 2 < 5 * -3 / 2 || 10 - 2 < 20 && !false else false
// return x, not really needed
in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Bool true)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration let with annotation fails`` () =
  let program = """let x:int = false in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    Assert.Fail(
      $"Type checking and evaluation succeeded with {(value, typeValue).ToFSharpString}, even though failure was expected"
    )

  | Right _e -> Assert.Pass()
