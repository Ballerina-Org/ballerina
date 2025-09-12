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

let private (!) = Identifier.LocalScope
let private (=>) t f = Identifier.FullyQualified([ t ], f)
let private (!!) = Identifier.LocalScope >> TypeExpr.Lookup
let private (=>>) = Identifier.FullyQualified >> TypeExpr.Lookup
do ignore (!)
do ignore (=>)
do ignore (!!)
do ignore (=>>)

type private PrimitiveExt =
  | Int32Operations of Int32Operations<ValueExt>
  | Int64Operations of Int64Operations<ValueExt>
  | Float32Operations of Float32Operations<ValueExt>
  | Float64Operations of Float64Operations<ValueExt>
  | DecimalOperations of DecimalOperations<ValueExt>
  | DateOnlyOperations of DateOnlyOperations<ValueExt>
  | DateTimeOperations of DateTimeOperations<ValueExt>
  | StringOperations of StringOperations<ValueExt>
  | GuidOperations of GuidOperations<ValueExt>

and private OptionExt =
  | OptionOperations of OptionOperations<ValueExt>
  | OptionValues of OptionValues<ValueExt>
  | OptionConstructors of OptionConstructors

and private ValueExt =
  | ValueExt of Choice<OptionExt, PrimitiveExt>

  static member Getters = {| ValueExt = fun (ValueExt e) -> e |}
  static member Updaters = {| ValueExt = fun u (ValueExt e) -> ValueExt(u e) |}

let private optionExtension =
  OptionExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice1Of2(OptionValues x) -> Some x
        | _ -> None)
      Set = OptionValues >> Choice1Of2 >> ValueExt.ValueExt }
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice1Of2(OptionConstructors x) -> Some x
        | _ -> None)
      Set = OptionConstructors >> Choice1Of2 >> ValueExt.ValueExt }
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice1Of2(OptionOperations x) -> Some x
        | _ -> None)
      Set = OptionOperations >> Choice1Of2 >> ValueExt.ValueExt }

let private int32Extension =
  Int32Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of2(Int32Operations x) -> Some x
        | _ -> None)
      Set = Int32Operations >> Choice2Of2 >> ValueExt.ValueExt }

let private int64Extension =
  Int64Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of2(Int64Operations x) -> Some x
        | _ -> None)
      Set = Int64Operations >> Choice2Of2 >> ValueExt.ValueExt }

let private float32Extension =
  Float32Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of2(Float32Operations x) -> Some x
        | _ -> None)
      Set = Float32Operations >> Choice2Of2 >> ValueExt.ValueExt }

let private float64Extension =
  Float64Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of2(Float64Operations x) -> Some x
        | _ -> None)
      Set = Float64Operations >> Choice2Of2 >> ValueExt.ValueExt }

let private decimalExtension =
  DecimalExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of2(DecimalOperations x) -> Some x
        | _ -> None)
      Set = DecimalOperations >> Choice2Of2 >> ValueExt.ValueExt }

let private dateOnlyExtension =
  DateOnlyExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of2(DateOnlyOperations x) -> Some x
        | _ -> None)
      Set = DateOnlyOperations >> Choice2Of2 >> ValueExt.ValueExt }

let private dateTimeExtension =
  DateTimeExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of2(DateTimeOperations x) -> Some x
        | _ -> None)
      Set = DateTimeOperations >> Choice2Of2 >> ValueExt.ValueExt }

let private stringExtension =
  StringExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of2(StringOperations x) -> Some x
        | _ -> None)
      Set = StringOperations >> Choice2Of2 >> ValueExt.ValueExt }

let private guidExtension =
  GuidExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice2Of2(GuidOperations x) -> Some x
        | _ -> None)
      Set = GuidOperations >> Choice2Of2 >> ValueExt.ValueExt }

let private context =
  LanguageContext<ValueExt>.Empty
  |> (optionExtension |> TypeExtension.ToLanguageContext)
  |> (int32Extension |> OperationsExtension.ToLanguageContext)
  |> (int64Extension |> OperationsExtension.ToLanguageContext)
  |> (float32Extension |> OperationsExtension.ToLanguageContext)
  |> (float64Extension |> OperationsExtension.ToLanguageContext)
  |> (dateOnlyExtension |> OperationsExtension.ToLanguageContext)
  |> (dateTimeExtension |> OperationsExtension.ToLanguageContext)
  |> (stringExtension |> OperationsExtension.ToLanguageContext)
  |> (guidExtension |> OperationsExtension.ToLanguageContext)

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
      Choice1Of2(OptionConstructors Option_Some) |> ValueExt |> Ext

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
      Choice1Of2(OptionValues(Option(Some(Value.Primitive(PrimitiveValue.Int32 100)))))
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
      Choice1Of2(OptionValues(Option None)) |> ValueExt |> Ext

    match actual with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
    | Sum.Right err -> Assert.Fail $"Evaluation failed: {err}"

  | Right(e, _) -> Assert.Fail($"Type checking failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-ExprEval construction of matching over custom (Option) succeeds with Some`` () =
  let program =
    Expr.Apply(
      Expr.UnionDes(
        Map.ofList
          [ Identifier.FullyQualified([ "Option" ], "Some"), ("x" |> Var.Create, Expr.Lookup(Identifier.LocalScope "x"))
            Identifier.FullyQualified([ "Option" ], "None"),
            ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Int32 -1)) ]
      ),
      Expr.Apply(
        Expr.TypeApply(
          Expr.Lookup(Identifier.FullyQualified([ "Option" ], "Some")),
          TypeExpr.Primitive PrimitiveType.Int32
        ),
        Expr.Primitive(PrimitiveValue.Int32 100)
      )
    )

  let initialContext = context.TypeCheckContext

  let initialState = context.TypeCheckState
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Left((program, _typeValue, _), _state) ->
    let initialContext = context.ExprEvalContext

    let actual = Expr.Eval program |> Reader.Run initialContext

    let expected: Value<TypeValue, ValueExt> = Value.Primitive(PrimitiveValue.Int32 100)

    match actual with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
    | Sum.Right err -> Assert.Fail $"Evaluation failed: {err}"

  | Right(e, _) -> Assert.Fail($"Type checking failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-ExprEval construction of matching over custom (Option) succeeds with None`` () =
  let program =
    Expr.Apply(
      Expr.UnionDes(
        Map.ofList
          [ Identifier.FullyQualified([ "Option" ], "Some"), ("x" |> Var.Create, Expr.Lookup(Identifier.LocalScope "x"))
            Identifier.FullyQualified([ "Option" ], "None"),
            ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Int32 -1)) ]
      ),
      Expr.Apply(
        Expr.TypeApply(
          Expr.Lookup(Identifier.FullyQualified([ "Option" ], "None")),
          TypeExpr.Primitive PrimitiveType.Int32
        ),
        Expr.Primitive(PrimitiveValue.Unit)
      )
    )

  let initialContext = context.TypeCheckContext

  let initialState = context.TypeCheckState
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Left((program, _typeValue, _), _state) ->
    let initialContext = context.ExprEvalContext

    let actual = Expr.Eval program |> Reader.Run initialContext

    let expected: Value<TypeValue, ValueExt> = Value.Primitive(PrimitiveValue.Int32 -1)

    match actual with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
    | Sum.Right err -> Assert.Fail $"Evaluation failed: {err}"

  | Right(e, _) -> Assert.Fail($"Type checking failed: {e.ToFSharpString}")
