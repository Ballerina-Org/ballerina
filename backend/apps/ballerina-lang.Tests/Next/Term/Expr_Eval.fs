module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Expr_Eval

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
open Ballerina.DSL.Next.Types.TypeChecker.Expr
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.DSL.Next.Types.TypeChecker.Eval
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.DSL.Next.Terms
open Ballerina.State.WithError
open Ballerina.DSL.Next.StdLib.Option
open Ballerina.DSL.Next.StdLib.Int32
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.StdLib.DateOnly
open Ballerina.DSL.Next.StdLib.Int64
open Ballerina.DSL.Next.StdLib.Float32
open Ballerina.DSL.Next.StdLib.Float64
open Ballerina.DSL.Next.StdLib.Decimal
open Ballerina.DSL.Next.StdLib.DateTime
open Ballerina.DSL.Next.StdLib.Bool
open Ballerina.DSL.Next.StdLib.String
open Ballerina.DSL.Next.StdLib.Guid
open Ballerina.DSL.Next.StdLib.TimeSpan
open Ballerina.DSL.Next.StdLib
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.Collections.NonEmptyList

let private (!) = Identifier.LocalScope
let private (=>) t f = Identifier.FullyQualified([ t ], f)

let private (!!): string -> TypeExpr<ValueExt> =
  Identifier.LocalScope >> TypeExpr.Lookup

let private (=>>): List<string> * string -> TypeExpr<ValueExt> =
  Identifier.FullyQualified >> TypeExpr.Lookup

do ignore (!)
do ignore (=>)
do ignore (!!)
do ignore (=>>)

let ops, context = stdExtensions

let typeCheck = Expr.TypeCheck()

let private runTypeCheck (program: Expr<TypeExpr<ValueExt>, Identifier, ValueExt>) =
  typeCheck None program
  |> State.Run(context.TypeCheckContext, context.TypeCheckState)

let private eval (program: Expr<TypeValue<ValueExt>, ResolvedIdentifier, ValueExt>) =
  Expr.Eval(NonEmptyList.One program) |> Reader.Run context.ExprEvalContext

[<Test>]
let ``LangNext-ExprEval (generic) Apply of custom Option type succeeds`` () =
  let program =
    Expr.TypeApply(Expr.Lookup(Identifier.FullyQualified([ "Option" ], "Some")), TypeExpr.Primitive PrimitiveType.Int32)

  let actual = runTypeCheck program

  match actual with
  | Left((program, _typeValue, _, _), _state) ->
    let actual = eval program

    let expected: Value<TypeValue<ValueExt>, ValueExt> =
      Choice2Of6(OptionConstructors Option_Some) |> ValueExt |> Ext

    match actual with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
    | Sum.Right err -> Assert.Fail $"Evaluation failed: {err}"

  | Right(e, _) -> Assert.Fail($"Type checking failed: {e.AsFSharpString}")


[<Test>]
let ``LangNext-ExprEval construction of custom Option.Some succeeds`` () =
  let program =
    Expr.Apply(
      Expr.TypeApply(
        Expr.Lookup(Identifier.FullyQualified([ "Option" ], "Some")),
        TypeExpr.Primitive PrimitiveType.Int32
      ),
      Expr.Primitive(PrimitiveValue.Int32 100)
    )

  let actual = runTypeCheck program

  match actual with
  | Left((program, _typeValue, _, _), _state) ->
    let actual = eval program

    let expected: Value<TypeValue<ValueExt>, ValueExt> =
      Choice2Of6(OptionValues(Option(Some(Value.Primitive(PrimitiveValue.Int32 100)))))
      |> ValueExt
      |> Ext

    match actual with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
    | Sum.Right err -> Assert.Fail $"Evaluation failed: {err}"

  | Right(e, _) -> Assert.Fail($"Type checking failed: {e.AsFSharpString}")


[<Test>]
let ``LangNext-ExprEval construction of custom Option.None succeeds`` () =
  let program =
    Expr.Apply(
      Expr.TypeApply(
        Expr.Lookup(Identifier.FullyQualified([ "Option" ], "None")),
        TypeExpr.Primitive PrimitiveType.Int32
      ),
      Expr.Primitive(PrimitiveValue.Unit)
    )

  let actual = runTypeCheck program

  match actual with
  | Left((program, _typeValue, _, _), _state) ->
    let actual = eval program

    let expected: Value<TypeValue<ValueExt>, ValueExt> =
      Choice2Of6(OptionValues(Option None)) |> ValueExt |> Ext

    match actual with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
    | Sum.Right err -> Assert.Fail $"Evaluation failed: {err}"

  | Right(e, _) -> Assert.Fail($"Type checking failed: {e.AsFSharpString}")


[<Test>]
let ``Int32 addition operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("+")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateInt32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Int32 8) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Int32 8 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Int32 multiplication operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("*")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateInt32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Int32 15) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Int32 15 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Int32 subtraction operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("-")), Expr.Primitive(PrimitiveValue.Int32 10)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateInt32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Int32 7) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Int32 7 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Int32 equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("==")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 5)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Int32 not equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("!=")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 5)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool false) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool false but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Int32 greater than operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope(">")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Int32 greater than or equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope(">=")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 5)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Int64 power operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("**")), Expr.Primitive(PrimitiveValue.Int64 12L)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateInt64()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Int64 1728L) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Int64 1728 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Int64 mod operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("%")), Expr.Primitive(PrimitiveValue.Int64 12L)),
      Expr.Primitive(PrimitiveValue.Int64 5L)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateInt64()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Int64 2L) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Int64 2 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"


[<Test>]
let ``Float32 plus operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("+")), Expr.Primitive(PrimitiveValue.Float32 5.0f)),
      Expr.Primitive(PrimitiveValue.Float32 3.0f)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateFloat32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Float32 8.0f) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Float32 8.0 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Float32 minus operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("-")), Expr.Primitive(PrimitiveValue.Float32 0.0f)),
      Expr.Primitive(PrimitiveValue.Float32 5.0f)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateFloat32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Float32 -5.0f) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Float32 -5.0 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Float32 divide operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("/")), Expr.Primitive(PrimitiveValue.Float32 5.0f)),
      Expr.Primitive(PrimitiveValue.Float32 3.0f)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateFloat32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Float32 1.6666666666666667f) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Float32 1.6666666666666667 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Float32 power operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("**")), Expr.Primitive(PrimitiveValue.Float32 5.0f)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateFloat32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Float32 125.0f) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Float32 125.0 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Float32 mod operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("%")), Expr.Primitive(PrimitiveValue.Float32 5.0f)),
      Expr.Primitive(PrimitiveValue.Float32 3.0f)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateFloat32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Float32 2.0f) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Float32 2.0 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Float32 equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("==")), Expr.Primitive(PrimitiveValue.Float32 5.0f)),
      Expr.Primitive(PrimitiveValue.Float32 5.0f)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Float32 not equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("!=")), Expr.Primitive(PrimitiveValue.Float32 5.0f)),
      Expr.Primitive(PrimitiveValue.Float32 5.0f)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool false) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool false but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Float32 greater than operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope(">")), Expr.Primitive(PrimitiveValue.Float32 5.0f)),
      Expr.Primitive(PrimitiveValue.Float32 3.0f)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Float32 greater than or equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope(">=")), Expr.Primitive(PrimitiveValue.Float32 5.0f)),
      Expr.Primitive(PrimitiveValue.Float32 5.0f)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

let ``Decimal equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("==")), Expr.Primitive(PrimitiveValue.Decimal 12.0M)),
      Expr.Primitive(PrimitiveValue.Decimal 12.0M)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Decimal not equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("!=")), Expr.Primitive(PrimitiveValue.Decimal 12.0M)),
      Expr.Primitive(PrimitiveValue.Decimal 12.0M)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool false) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool false but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Decimal greater than operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope(">")), Expr.Primitive(PrimitiveValue.Decimal 12.0M)),
      Expr.Primitive(PrimitiveValue.Decimal 1.0M)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Decimal greater than or equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope(">=")), Expr.Primitive(PrimitiveValue.Decimal 12.0M)),
      Expr.Primitive(PrimitiveValue.Decimal 12.0M)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Decimal power operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("**")), Expr.Primitive(PrimitiveValue.Decimal 3.5M)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateDecimal()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Decimal 42.875M) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Decimal 42.875 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

let ``String concatenation operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("+")), Expr.Primitive(PrimitiveValue.String "Hello ")),
      Expr.Primitive(PrimitiveValue.String "World")
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateString()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.String "Hello World") -> Assert.Pass()
      | _ -> Assert.Fail $"Expected 'Hello World' but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``String equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("==")), Expr.Primitive(PrimitiveValue.String "Hello")),
      Expr.Primitive(PrimitiveValue.String "Hello")
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``String not equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("!=")), Expr.Primitive(PrimitiveValue.String "Hello")),
      Expr.Primitive(PrimitiveValue.String "World")
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``String greater than operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope(">")), Expr.Primitive(PrimitiveValue.String "Hello")),
      Expr.Primitive(PrimitiveValue.String "World")
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool false) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool false but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``String greater than or equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope(">=")), Expr.Primitive(PrimitiveValue.String "Hello")),
      Expr.Primitive(PrimitiveValue.String "Hello")
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Bool and operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("&&")), Expr.Primitive(PrimitiveValue.Bool true)),
      Expr.Primitive(PrimitiveValue.Bool false)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool false) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool false but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Bool or operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("||")), Expr.Primitive(PrimitiveValue.Bool false)),
      Expr.Primitive(PrimitiveValue.Bool true)
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Bool not operation works`` () =
  let program =
    Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "bool" ], "!")), Expr.Primitive(PrimitiveValue.Bool true))

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool false) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool false but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Guid equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(
        Expr.Lookup(Identifier.LocalScope("==")),
        Expr.Primitive(PrimitiveValue.Guid(System.Guid("88888888-4444-4444-4444-121212121212")))
      ),
      Expr.Primitive(PrimitiveValue.Guid(System.Guid("88888888-4444-4444-4444-121212121212")))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``Guid not equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(
        Expr.Lookup(Identifier.LocalScope("!=")),
        Expr.Primitive(PrimitiveValue.Guid(System.Guid("88888888-4444-4444-4444-121212121212")))
      ),
      Expr.Primitive(PrimitiveValue.Guid(System.Guid("88888888-4444-4444-4444-121212121212")))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool false) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool false but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``DateOnly diff operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.LocalScope("-")), Expr.Primitive(PrimitiveValue.Date(System.DateOnly(1, 1, 2)))),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(1, 1, 1)))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateTimeSpan()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.TimeSpan(ts)) when ts = System.TimeSpan(1, 0, 0, 0) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected TimeSpan but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``DateOnly toDateTime operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "dateOnly" ], "toDateTime")),
        Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
      ),
      Expr.TupleCons
        [ Expr.Primitive(PrimitiveValue.Int32(2))
          Expr.Primitive(PrimitiveValue.Int32(10))
          Expr.Primitive(PrimitiveValue.Int32(40)) ]
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateDateTime()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.DateTime(dt)) when dt = System.DateTime(2025, 10, 10, 2, 10, 40) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected DateTime (2025, 10, 10, 2, 10, 40) but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``DateOnly getYear operation works`` () =
  let program =
    Expr.Apply(
      Expr.Lookup(Identifier.FullyQualified([ "dateOnly" ], "getYear")),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateInt32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Int32(2025)) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Int32 2025 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``DateOnly getMonth operation works`` () =
  let program =
    Expr.Apply(
      Expr.Lookup(Identifier.FullyQualified([ "dateOnly" ], "getMonth")),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateInt32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Int32(10)) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Int32 10 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``DateOnly getDay operation works`` () =
  let program =
    Expr.Apply(
      Expr.Lookup(Identifier.FullyQualified([ "dateOnly" ], "getDay")),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateInt32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Int32(10)) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Int32 10 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``DateOnly getDayOfWeek operation works`` () =
  let program =
    Expr.Apply(
      Expr.Lookup(Identifier.FullyQualified([ "dateOnly" ], "getDayOfWeek")),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateInt32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Int32(5)) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Int32 5 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``DateOnly getDayOfYear operation works`` () =
  let program =
    Expr.Apply(
      Expr.Lookup(Identifier.FullyQualified([ "dateOnly" ], "getDayOfYear")),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateInt32()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Int32(283)) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Int32 283 but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``DateTime toDateOnly operation works`` () =
  let program =
    Expr.Apply(
      Expr.Lookup(Identifier.FullyQualified([ "dateTime" ], "toDateOnly")),
      Expr.Primitive(PrimitiveValue.DateTime(System.DateTime(2025, 10, 10, 10, 10, 10)))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateDateOnly()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Date(d)) when d = System.DateOnly(2025, 10, 10) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected DateOnly (2025, 10, 10) but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``TimeSpan equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(
        Expr.Lookup(Identifier.LocalScope("==")),
        Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 0, 0)))
      ),
      Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 0, 0)))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"


[<Test>]
let ``TimeSpan not equal operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(
        Expr.Lookup(Identifier.LocalScope("!=")),
        Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 0, 0)))
      ),
      Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 0, 0)))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool false) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool false but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``TimeSpan greater than operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(
        Expr.Lookup(Identifier.LocalScope(">")),
        Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 12, 0)))
      ),
      Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 0, 44)))
    )

  let typeCheckResult = runTypeCheck program

  match typeCheckResult with
  | Left((typedProgram, typeValue, _, _), _) ->
    Assert.That(typeValue, Is.EqualTo<TypeValue<ValueExt>>(TypeValue.CreateBool()))

    let evalResult = eval typedProgram

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"
