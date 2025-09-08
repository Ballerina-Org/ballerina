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

let private (!) = Identifier.LocalScope
let private (=>) t f = Identifier.FullyQualified([ t ], f)
let private (!!) = Identifier.LocalScope >> TypeExpr.Lookup
let private (=>>) = Identifier.FullyQualified >> TypeExpr.Lookup
do ignore (!)
do ignore (=>)
do ignore (!!)
do ignore (=>>)


let optionSomeId = Identifier.FullyQualified([ "Option" ], "Some")
let optionNoneId = Identifier.FullyQualified([ "Option" ], "None")
let optionSomeSymbol = optionSomeId |> TypeSymbol.Create
let optionNoneSymbol = optionNoneId |> TypeSymbol.Create

let optionSomeType =
  TypeValue.Lambda(
    TypeParameter.Create("a", Kind.Star),
    TypeExpr.Arrow(
      TypeExpr.Lookup(Identifier.LocalScope "a"),
      TypeExpr.Apply(TypeExpr.Lookup(Identifier.LocalScope "Option"), TypeExpr.Lookup(Identifier.LocalScope "a"))
    )
  )

let optionNoneType =
  TypeValue.Lambda(
    TypeParameter.Create("a", Kind.Star),
    TypeExpr.Arrow(
      TypeExpr.Primitive PrimitiveType.Unit,
      TypeExpr.Apply(TypeExpr.Lookup(Identifier.LocalScope "Option"), TypeExpr.Lookup(Identifier.LocalScope "a"))
    )
  )

let initialTypeCheckingContext: TypeCheckContext =
  { TypeCheckContext.Empty with
      Values =
        [ (optionSomeId, (optionSomeType, Kind.Arrow(Kind.Star, Kind.Star)))
          (optionNoneId, (optionNoneType, Kind.Arrow(Kind.Star, Kind.Star))) ]
        |> Map.ofList }

let initialTypeCheckingState: TypeCheckState =
  { Types =
      { TypeExprEvalState.Bindings =
          Map.empty // Map<Identifier, TypeValue * Kind> // add Option:FUN a -> Union(Some a, None Unit)
          |> Map.add
            (Identifier.LocalScope "Option")
            (TypeValue.Lambda(
              TypeParameter.Create("a", Kind.Star),
              (TypeValue
                .Union(
                  [ optionSomeSymbol, TypeValue.Lookup(Identifier.LocalScope "a")
                    optionNoneSymbol, TypeValue.Primitive PrimitiveType.Unit ]
                  |> Map.ofList
                )
                .AsExpr)
             ),
             Kind.Arrow(Kind.Star, Kind.Star))

        Symbols =
          [ (Identifier.FullyQualified([ "Option" ], "Some"), optionSomeSymbol)
            (Identifier.FullyQualified([ "Option" ], "None"), optionNoneSymbol) ]
          |> Map.ofList }
    Vars = UnificationState.Empty }

type SampleExtensionValue =
  | Option_Some
  | Option_None
  | Option of Microsoft.FSharp.Core.Option<Value<TypeValue, SampleExtensionValue>>

let rec evalOps: ValueExtensionOps<SampleExtensionValue> =
  { Eval =
      fun (v: SampleExtensionValue) ->
        let eval = Expr.Eval<SampleExtensionValue>

        match v with
        | SampleExtensionValue.Option_None ->
          Applicable(fun _ ->
            reader { return Value<TypeValue, SampleExtensionValue>.Ext(SampleExtensionValue.Option(Option.None)) })
        | SampleExtensionValue.Option_Some ->
          Applicable(fun v ->
            reader { return Value<TypeValue, SampleExtensionValue>.Ext(SampleExtensionValue.Option(Option.Some v)) })
        | SampleExtensionValue.Option v ->
          Matchable(fun handlers ->
            reader {
              let! onSomeVar, onSomeBody =
                handlers
                |> Map.tryFindWithError (Identifier.FullyQualified([ "Option" ], "Some")) "handlers" "Option.Some"
                |> reader.OfSum

              let! onNoneVar, onNoneBody =
                handlers
                |> Map.tryFindWithError (Identifier.FullyQualified([ "Option" ], "None")) "handlers" "Option.None"
                |> reader.OfSum

              match v with
              | Option.Some value ->
                return!
                  onSomeBody
                  |> eval
                  |> reader.MapContext(
                    ExprEvalContext.Updaters.Values(Map.add (Identifier.LocalScope onSomeVar.Name) value)
                  )
              | Option.None ->
                return!
                  onNoneBody
                  |> eval
                  |> reader.MapContext(
                    ExprEvalContext.Updaters.Values(
                      Map.add
                        (Identifier.LocalScope onNoneVar.Name)
                        (Value<TypeValue, SampleExtensionValue>.Primitive PrimitiveValue.Unit)
                    )
                  )
            }) }

[<Test>]
let ``LangNext-ExprEval (generic) Apply of custom Option type succeeds`` () =
  let program =
    Expr.TypeApply(Expr.Lookup(Identifier.FullyQualified([ "Option" ], "Some")), TypeExpr.Primitive PrimitiveType.Int32)

  let initialContext = initialTypeCheckingContext

  let initialState = initialTypeCheckingState
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Left((program, _typeValue, _), _state) ->
    let initialContext: ExprEvalContext<SampleExtensionValue> =
      { Values =
          Map.empty
          |> Map.add (Identifier.FullyQualified([ "Option" ], "Some")) (Option_Some |> Ext)
          |> Map.add (Identifier.FullyQualified([ "Option" ], "None")) (Option_None |> Ext)
        ExtensionOps = evalOps
        Symbols = Map.empty }

    let actual = Expr.Eval program |> Reader.Run initialContext
    let expected: Value<TypeValue, SampleExtensionValue> = Ext(Option_Some)

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
      Expr.Primitive(PrimitiveValue.Int 100)
    )

  let initialContext = initialTypeCheckingContext

  let initialState = initialTypeCheckingState
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Left((program, _typeValue, _), _state) ->
    let initialContext: ExprEvalContext<SampleExtensionValue> =
      { Values =
          Map.empty
          |> Map.add (Identifier.FullyQualified([ "Option" ], "Some")) (Option_Some |> Ext)
          |> Map.add (Identifier.FullyQualified([ "Option" ], "None")) (Option_None |> Ext)
        ExtensionOps = evalOps
        Symbols = Map.empty }

    let actual = Expr.Eval program |> Reader.Run initialContext

    let expected: Value<TypeValue, SampleExtensionValue> =
      Ext(Option(Some(Value.Primitive(PrimitiveValue.Int 100))))

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

  let initialContext = initialTypeCheckingContext

  let initialState = initialTypeCheckingState
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Left((program, _typeValue, _), _state) ->
    let initialContext: ExprEvalContext<SampleExtensionValue> =
      { Values =
          Map.empty
          |> Map.add (Identifier.FullyQualified([ "Option" ], "Some")) (Option_Some |> Ext)
          |> Map.add (Identifier.FullyQualified([ "Option" ], "None")) (Option_None |> Ext)
        ExtensionOps = evalOps
        Symbols = Map.empty }

    let actual = Expr.Eval program |> Reader.Run initialContext

    let expected: Value<TypeValue, SampleExtensionValue> = Ext(Option(None))

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
            Identifier.FullyQualified([ "Option" ], "None"), ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Int -1)) ]
      ),
      Expr.Apply(
        Expr.TypeApply(
          Expr.Lookup(Identifier.FullyQualified([ "Option" ], "Some")),
          TypeExpr.Primitive PrimitiveType.Int32
        ),
        Expr.Primitive(PrimitiveValue.Int 100)
      )
    )

  let initialContext = initialTypeCheckingContext

  let initialState = initialTypeCheckingState
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Left((program, _typeValue, _), _state) ->
    let initialContext: ExprEvalContext<SampleExtensionValue> =
      { Values =
          Map.empty
          |> Map.add (Identifier.FullyQualified([ "Option" ], "Some")) (Option_Some |> Ext)
          |> Map.add (Identifier.FullyQualified([ "Option" ], "None")) (Option_None |> Ext)
        ExtensionOps = evalOps
        Symbols = Map.empty }

    let actual = Expr.Eval program |> Reader.Run initialContext

    let expected: Value<TypeValue, SampleExtensionValue> =
      Value.Primitive(PrimitiveValue.Int 100)

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
            Identifier.FullyQualified([ "Option" ], "None"), ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Int -1)) ]
      ),
      Expr.Apply(
        Expr.TypeApply(
          Expr.Lookup(Identifier.FullyQualified([ "Option" ], "None")),
          TypeExpr.Primitive PrimitiveType.Int32
        ),
        Expr.Primitive(PrimitiveValue.Unit)
      )
    )

  let initialContext = initialTypeCheckingContext

  let initialState = initialTypeCheckingState
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Left((program, _typeValue, _), _state) ->
    let initialContext: ExprEvalContext<SampleExtensionValue> =
      { Values =
          Map.empty
          |> Map.add (Identifier.FullyQualified([ "Option" ], "Some")) (Option_Some |> Ext)
          |> Map.add (Identifier.FullyQualified([ "Option" ], "None")) (Option_None |> Ext)
        ExtensionOps = evalOps
        Symbols = Map.empty }

    let actual = Expr.Eval program |> Reader.Run initialContext

    let expected: Value<TypeValue, SampleExtensionValue> =
      Value.Primitive(PrimitiveValue.Int -1)

    match actual with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
    | Sum.Right err -> Assert.Fail $"Evaluation failed: {err}"

  | Right(e, _) -> Assert.Fail($"Type checking failed: {e.ToFSharpString}")
