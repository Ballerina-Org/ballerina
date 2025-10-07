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
open Ballerina.DSL.Next.Types.TypeCheck
open Ballerina.DSL.Next.Types.Eval
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
open Ballerina.DSL.Next.StdLib.List

let private (!) = Identifier.LocalScope
let private (=>) t f = Identifier.FullyQualified([ t ], f)
let private (!!) = Identifier.LocalScope >> TypeExpr.Lookup
let private (=>>) = Identifier.FullyQualified >> TypeExpr.Lookup
do ignore (!)
do ignore (=>)
do ignore (!!)
do ignore (=>>)

type ValueExt =
  | ValueExt of Choice<ListExt, OptionExt, PrimitiveExt>

  static member Getters = {| ValueExt = fun (ValueExt e) -> e |}
  static member Updaters = {| ValueExt = fun u (ValueExt e) -> ValueExt(u e) |}

and ListExt =
  | ListOperations of ListOperations<ValueExt>
  | ListValues of ListValues<ValueExt>

and OptionExt =
  | OptionOperations of OptionOperations<ValueExt>
  | OptionValues of OptionValues<ValueExt>
  | OptionConstructors of OptionConstructors

and PrimitiveExt =
  | BoolOperations of BoolOperations<ValueExt>
  | Int32Operations of Int32Operations<ValueExt>
  | Int64Operations of Int64Operations<ValueExt>
  | Float32Operations of Float32Operations<ValueExt>
  | Float64Operations of Float64Operations<ValueExt>
  | DecimalOperations of DecimalOperations<ValueExt>
  | DateOnlyOperations of DateOnlyOperations<ValueExt>
  | DateTimeOperations of DateTimeOperations<ValueExt>
  | TimeSpanOperations of TimeSpanOperations<ValueExt>
  | StringOperations of StringOperations<ValueExt>
  | GuidOperations of GuidOperations<ValueExt>

let listExtension =
  ListExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice1Of3(ListValues x) -> Some x
        | _ -> None)
      Set = ListValues >> Choice1Of3 >> ValueExt.ValueExt }
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice1Of3(ListOperations x) -> Some x
        | _ -> None)
      Set = ListOperations >> Choice1Of3 >> ValueExt.ValueExt }

let optionExtension =
  OptionExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of3(OptionValues x) -> Some x
        | _ -> None)
      Set = OptionValues >> Choice2Of3 >> ValueExt.ValueExt }
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of3(OptionConstructors x) -> Some x
        | _ -> None)
      Set = OptionConstructors >> Choice2Of3 >> ValueExt.ValueExt }
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of3(OptionOperations x) -> Some x
        | _ -> None)
      Set = OptionOperations >> Choice2Of3 >> ValueExt.ValueExt }

let boolExtension =
  BoolExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(BoolOperations x) -> Some x
        | _ -> None)
      Set = BoolOperations >> Choice3Of3 >> ValueExt.ValueExt }

let int32Extension =
  Int32Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(Int32Operations x) -> Some x
        | _ -> None)
      Set = Int32Operations >> Choice3Of3 >> ValueExt.ValueExt }

let int64Extension =
  Int64Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(Int64Operations x) -> Some x
        | _ -> None)
      Set = Int64Operations >> Choice3Of3 >> ValueExt.ValueExt }

let float32Extension =
  Float32Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(Float32Operations x) -> Some x
        | _ -> None)
      Set = Float32Operations >> Choice3Of3 >> ValueExt.ValueExt }

let float64Extension =
  Float64Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(Float64Operations x) -> Some x
        | _ -> None)
      Set = Float64Operations >> Choice3Of3 >> ValueExt.ValueExt }

let decimalExtension =
  DecimalExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(DecimalOperations x) -> Some x
        | _ -> None)
      Set = DecimalOperations >> Choice3Of3 >> ValueExt.ValueExt }

let dateOnlyExtension =
  DateOnlyExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(DateOnlyOperations x) -> Some x
        | _ -> None)
      Set = DateOnlyOperations >> Choice3Of3 >> ValueExt.ValueExt }

let dateTimeExtension =
  DateTimeExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(DateTimeOperations x) -> Some x
        | _ -> None)
      Set = DateTimeOperations >> Choice3Of3 >> ValueExt.ValueExt }

let timeSpanExtension =
  TimeSpanExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(TimeSpanOperations x) -> Some x
        | _ -> None)
      Set = TimeSpanOperations >> Choice3Of3 >> ValueExt.ValueExt }

let stringExtension =
  StringExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(StringOperations x) -> Some x
        | _ -> None)
      Set = StringOperations >> Choice3Of3 >> ValueExt.ValueExt }

let guidExtension =
  GuidExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(GuidOperations x) -> Some x
        | _ -> None)
      Set = GuidOperations >> Choice3Of3 >> ValueExt.ValueExt }

let context =
  LanguageContext<ValueExt>.Empty
  |> (listExtension |> TypeExtension.RegisterLanguageContext)
  |> (optionExtension |> TypeExtension.RegisterLanguageContext)
  |> (boolExtension |> OperationsExtension.RegisterLanguageContext)
  |> (int32Extension |> OperationsExtension.RegisterLanguageContext)
  |> (int64Extension |> OperationsExtension.RegisterLanguageContext)
  |> (float32Extension |> OperationsExtension.RegisterLanguageContext)
  |> (float64Extension |> OperationsExtension.RegisterLanguageContext)
  |> (decimalExtension |> OperationsExtension.RegisterLanguageContext)
  |> (dateOnlyExtension |> OperationsExtension.RegisterLanguageContext)
  |> (dateTimeExtension |> OperationsExtension.RegisterLanguageContext)
  |> (timeSpanExtension |> OperationsExtension.RegisterLanguageContext)
  |> (stringExtension |> OperationsExtension.RegisterLanguageContext)
  |> (guidExtension |> OperationsExtension.RegisterLanguageContext)

[<Test>]
let ``LangNext-ExprEval (generic) Apply of custom Option type succeeds`` () =
  let program =
    Expr.TypeApply(Expr.Lookup(Identifier.FullyQualified([ "Option" ], "Some")), TypeExpr.Primitive PrimitiveType.Int32)

  let initialContext = context.TypeCheckContext

  let initialState = context.TypeCheckState
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Left((program, _typeValue, _), _state) ->
    let initialContext = context.ExprEvalContext

    let actual = Expr.Eval program |> Reader.Run initialContext

    let expected: Value<TypeValue, ValueExt> =
      Choice2Of3(OptionConstructors Option_Some) |> ValueExt |> Ext

    match actual with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
    | Sum.Right err -> Assert.Fail $"Evaluation failed: {err}"

  | Right(e, _) -> Assert.Fail($"Type checking failed: {e.ToFSharpString}")


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

  let initialContext = context.TypeCheckContext

  let initialState = context.TypeCheckState
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Left((program, _typeValue, _), _state) ->
    let initialContext = context.ExprEvalContext

    let actual = Expr.Eval program |> Reader.Run initialContext

    let expected: Value<TypeValue, ValueExt> =
      Choice2Of3(OptionValues(Option(Some(Value.Primitive(PrimitiveValue.Int32 100)))))
      |> ValueExt
      |> Ext

    match actual with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
    | Sum.Right err -> Assert.Fail $"Evaluation failed: {err}"

  | Right(e, _) -> Assert.Fail($"Type checking failed: {e.ToFSharpString}")


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

  let initialContext = context.TypeCheckContext

  let initialState = context.TypeCheckState
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Left((program, _typeValue, _), _state) ->
    let initialContext = context.ExprEvalContext

    let actual = Expr.Eval program |> Reader.Run initialContext

    let expected: Value<TypeValue, ValueExt> =
      Choice2Of3(OptionValues(Option None)) |> ValueExt |> Ext

    match actual with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
    | Sum.Right err -> Assert.Fail $"Evaluation failed: {err}"

  | Right(e, _) -> Assert.Fail($"Type checking failed: {e.ToFSharpString}")


[<Test>]
let ``Int32 addition operation works`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Int32" ], "+")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateInt32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Int32" ], "*")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateInt32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Int32" ], "-")), Expr.Primitive(PrimitiveValue.Int32 10)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateInt32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Int32" ], "==")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 5)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Int32" ], "!=")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 5)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Int32" ], ">")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Int32" ], ">=")), Expr.Primitive(PrimitiveValue.Int32 5)),
      Expr.Primitive(PrimitiveValue.Int32 5)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Int64" ], "**")), Expr.Primitive(PrimitiveValue.Int64 12L)),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateInt64()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Int64" ], "%")), Expr.Primitive(PrimitiveValue.Int64 12L)),
      Expr.Primitive(PrimitiveValue.Int64 5L)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateInt64()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Float32" ], "+")),
        Expr.Primitive(PrimitiveValue.Float32 5.0f)
      ),
      Expr.Primitive(PrimitiveValue.Float32 3.0f)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateFloat32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
    Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Float32" ], "-")), Expr.Primitive(PrimitiveValue.Float32 5.0f))

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateFloat32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Float32" ], "/")),
        Expr.Primitive(PrimitiveValue.Float32 5.0f)
      ),
      Expr.Primitive(PrimitiveValue.Float32 3.0f)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateFloat32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Float32" ], "**")),
        Expr.Primitive(PrimitiveValue.Float32 5.0f)
      ),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateFloat32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Float32" ], "%")),
        Expr.Primitive(PrimitiveValue.Float32 5.0f)
      ),
      Expr.Primitive(PrimitiveValue.Float32 3.0f)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateFloat32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Float32" ], "==")),
        Expr.Primitive(PrimitiveValue.Float32 5.0f)
      ),
      Expr.Primitive(PrimitiveValue.Float32 5.0f)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Float32" ], "!=")),
        Expr.Primitive(PrimitiveValue.Float32 5.0f)
      ),
      Expr.Primitive(PrimitiveValue.Float32 5.0f)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Float32" ], ">")),
        Expr.Primitive(PrimitiveValue.Float32 5.0f)
      ),
      Expr.Primitive(PrimitiveValue.Float32 3.0f)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Float32" ], ">=")),
        Expr.Primitive(PrimitiveValue.Float32 5.0f)
      ),
      Expr.Primitive(PrimitiveValue.Float32 5.0f)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Decimal" ], "==")),
        Expr.Primitive(PrimitiveValue.Decimal 12.0M)
      ),
      Expr.Primitive(PrimitiveValue.Decimal 12.0M)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Decimal" ], "!=")),
        Expr.Primitive(PrimitiveValue.Decimal 12.0M)
      ),
      Expr.Primitive(PrimitiveValue.Decimal 12.0M)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Decimal" ], ">")),
        Expr.Primitive(PrimitiveValue.Decimal 12.0M)
      ),
      Expr.Primitive(PrimitiveValue.Decimal 1.0M)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Decimal" ], ">=")),
        Expr.Primitive(PrimitiveValue.Decimal 12.0M)
      ),
      Expr.Primitive(PrimitiveValue.Decimal 12.0M)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "Decimal" ], "**")),
        Expr.Primitive(PrimitiveValue.Decimal 3.5M)
      ),
      Expr.Primitive(PrimitiveValue.Int32 3)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateDecimal()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "String" ], "+")),
        Expr.Primitive(PrimitiveValue.String "Hello ")
      ),
      Expr.Primitive(PrimitiveValue.String "World")
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateString()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "String" ], "==")),
        Expr.Primitive(PrimitiveValue.String "Hello")
      ),
      Expr.Primitive(PrimitiveValue.String "Hello")
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "String" ], "!=")),
        Expr.Primitive(PrimitiveValue.String "Hello")
      ),
      Expr.Primitive(PrimitiveValue.String "World")
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "String" ], ">")),
        Expr.Primitive(PrimitiveValue.String "Hello")
      ),
      Expr.Primitive(PrimitiveValue.String "World")
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "String" ], ">=")),
        Expr.Primitive(PrimitiveValue.String "Hello")
      ),
      Expr.Primitive(PrimitiveValue.String "Hello")
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Bool" ], "&&")), Expr.Primitive(PrimitiveValue.Bool true)),
      Expr.Primitive(PrimitiveValue.Bool false)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Bool" ], "||")), Expr.Primitive(PrimitiveValue.Bool false)),
      Expr.Primitive(PrimitiveValue.Bool true)
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
    Expr.Apply(Expr.Lookup(Identifier.FullyQualified([ "Bool" ], "!")), Expr.Primitive(PrimitiveValue.Bool true))

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
        Expr.Lookup(Identifier.FullyQualified([ "Guid" ], "==")),
        Expr.Primitive(PrimitiveValue.Guid(System.Guid("88888888-4444-4444-4444-121212121212")))
      ),
      Expr.Primitive(PrimitiveValue.Guid(System.Guid("88888888-4444-4444-4444-121212121212")))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
        Expr.Lookup(Identifier.FullyQualified([ "Guid" ], "!=")),
        Expr.Primitive(PrimitiveValue.Guid(System.Guid("88888888-4444-4444-4444-121212121212")))
      ),
      Expr.Primitive(PrimitiveValue.Guid(System.Guid("88888888-4444-4444-4444-121212121212")))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Apply(
        Expr.Lookup(Identifier.FullyQualified([ "DateOnly" ], "-")),
        Expr.Primitive(PrimitiveValue.Date(System.DateOnly(1, 1, 2)))
      ),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(1, 1, 1)))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateTimeSpan()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
        Expr.Lookup(Identifier.FullyQualified([ "DateOnly" ], "toDateTime")),
        Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
      ),
      Expr.TupleCons
        [ Expr.Primitive(PrimitiveValue.Int32(2))
          Expr.Primitive(PrimitiveValue.Int32(10))
          Expr.Primitive(PrimitiveValue.Int32(40)) ]
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateDateTime()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Lookup(Identifier.FullyQualified([ "DateOnly" ], "getYear")),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateInt32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Lookup(Identifier.FullyQualified([ "DateOnly" ], "getMonth")),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateInt32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Lookup(Identifier.FullyQualified([ "DateOnly" ], "getDay")),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateInt32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Lookup(Identifier.FullyQualified([ "DateOnly" ], "getDayOfWeek")),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateInt32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Lookup(Identifier.FullyQualified([ "DateOnly" ], "getDayOfYear")),
      Expr.Primitive(PrimitiveValue.Date(System.DateOnly(2025, 10, 10)))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateInt32()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
      Expr.Lookup(Identifier.FullyQualified([ "DateTime" ], "toDateOnly")),
      Expr.Primitive(PrimitiveValue.DateTime(System.DateTime(2025, 10, 10, 10, 10, 10)))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateDateOnly()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
        Expr.Lookup(Identifier.FullyQualified([ "TimeSpan" ], "==")),
        Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 0, 0)))
      ),
      Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 0, 0)))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
        Expr.Lookup(Identifier.FullyQualified([ "TimeSpan" ], "!=")),
        Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 0, 0)))
      ),
      Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 0, 0)))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

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
        Expr.Lookup(Identifier.FullyQualified([ "TimeSpan" ], ">")),
        Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 12, 0)))
      ),
      Expr.Primitive(PrimitiveValue.TimeSpan(System.TimeSpan(1, 0, 44)))
    )

  let typeCheckResult =
    Expr.TypeCheck program
    |> State.Run(context.TypeCheckContext, context.TypeCheckState)

  match typeCheckResult with
  | Left((typedProgram, typeValue, _), _) ->
    Assert.That(typeValue, Is.EqualTo(TypeValue.CreateBool()))

    let evalResult = Expr.Eval typedProgram |> Reader.Run context.ExprEvalContext

    match evalResult with
    | Left result ->
      match result with
      | Value.Primitive(PrimitiveValue.Bool true) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected Bool true but got {result}"
    | Right err -> Assert.Fail $"Evaluation failed: {err}"
  | Right(err, _) -> Assert.Fail $"Type checking failed: {err}"
