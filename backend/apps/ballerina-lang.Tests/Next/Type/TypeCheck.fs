module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.TypeCheck

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.Errors
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Types.TypeCheck
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError
open Ballerina.Reader.WithError
open Ballerina.Fun
open Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Patterns
open Ballerina.StdLib.OrderPreservingMap

let private (!) = Identifier.LocalScope
let private (=>) t f = Identifier.FullyQualified([ t ], f)
let private (!!) = Identifier.LocalScope >> TypeExpr.Lookup
let private (=>>) = Identifier.FullyQualified >> TypeExpr.Lookup

let private initialContext t =
  TypeCheckContext.Empty
  |> TypeCheckContext.Updaters.Values(
    Map.add
      !"+"
      (TypeValue.CreateArrow(
        TypeValue.CreatePrimitive t,
        TypeValue.CreateArrow(TypeValue.CreatePrimitive t, TypeValue.CreatePrimitive t)
       ),
       Kind.Star)
  )

[<Test>]
let ``LangNext-TypeCheck let typechecks`` () =

  let program =
    Expr.Let(
      "x" |> Var.Create,
      None,
      Expr.Primitive(PrimitiveValue.Int32 10),
      Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Int32 5))
    )

  let initialContext = TypeCheckContext.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.CreateArrow(
          TypeValue.CreateInt32(),
          TypeValue.CreateArrow(TypeValue.CreateInt32(), TypeValue.CreateInt32())
         ),
         Kind.Star)
    )

  let actual =
    Expr.TypeCheck None program |> State.Run(initialContext, TypeCheckState.Empty)

  match actual with
  | Sum.Left((_, TypeValue.Primitive({ value = PrimitiveType.Int32 }), Kind.Star), _) -> Assert.Pass()
  | Sum.Left((_, t, k), _) -> Assert.Fail $"Expected typechecking to succeed with 'Int::*' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"


[<Test>]
let ``LangNext-TypeCheck lambda infers and typechecks`` () =
  let program =
    Expr.Lambda(
      "x" |> Var.Create,
      None,
      Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Int32 5))
    )

  let initialContext = TypeCheckContext.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.CreateArrow(
          TypeValue.CreateInt32(),
          TypeValue.CreateArrow(TypeValue.CreateInt32(), TypeValue.CreateInt32())
         ),
         Kind.Star)
    )

  let actual =
    Expr.TypeCheck None program |> State.Run(initialContext, TypeCheckState.Empty)

  match actual with
  | Sum.Left((_,
              TypeValue.Arrow({ value = (TypeValue.Primitive({ value = PrimitiveType.Int32 }),
                                         TypeValue.Primitive({ value = PrimitiveType.Int32 })) }),
              Kind.Star),
             _) -> Assert.Pass()
  | Sum.Left((_, t, k), _) ->
    Assert.Fail $"Expected typechecking to succeed with '(Int -> Int)::*' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"



[<Test>]
let ``LangNext-TypeCheck tuple cons and des typecheck`` () =
  let program =
    Expr.Let(
      "v3" |> Var.Create,
      None,
      Expr.TupleCons(
        [ Expr.Primitive(PrimitiveValue.Decimal 1.0M)
          Expr.Primitive(PrimitiveValue.Decimal 2.0M)
          Expr.Primitive(PrimitiveValue.Decimal 3.0M) ]
      ),
      Expr.Apply(
        Expr.Apply(Expr.Lookup !"+", Expr.TupleDes(Expr.Lookup !"v3", { TupleDesSelector.Index = 1 })),
        Expr.TupleDes(Expr.Lookup !"v3", { TupleDesSelector.Index = 2 })
      )
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.CreateArrow(
          TypeValue.CreateDecimal(),
          TypeValue.CreateArrow(TypeValue.CreateDecimal(), TypeValue.CreateDecimal())
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)

  let expected = TypeValue.CreateDecimal()

  match actual with
  | Sum.Left((_, actual, Kind.Star), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t, k), _) ->
    Assert.Fail
      $"Expected typechecking to succeed with '(Case1Of3 of Int | Case2Of3 of Int | Case3Of3 of _) -> Int' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"


[<Test>]
let ``LangNext-TypeCheck if-then-else typechecks`` () =
  let program =
    Expr.If(
      Expr.Apply(
        Expr.Apply(Expr.Lookup !">", Expr.Primitive(PrimitiveValue.Decimal 1.0M)),
        Expr.Primitive(PrimitiveValue.Decimal 2.0M)
      ),
      Expr.Primitive(PrimitiveValue.String "yes"),
      Expr.Primitive(PrimitiveValue.String "no")
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !">"
        (TypeValue.CreateArrow(
          TypeValue.CreateDecimal(),
          TypeValue.CreateArrow(TypeValue.CreateDecimal(), TypeValue.CreateBool())
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)
  let expected = TypeValue.CreateString()

  match actual with
  | Sum.Left((_, actual, Kind.Star), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t, k), _) -> Assert.Fail $"Expected typechecking to succeed with 'String' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"





[<Test>]
let ``LangNext-TypeCheck HKTs over option typechecks`` () =
  let program =
    Expr.TypeLet(
      "Option",
      TypeExpr.Let(
        "Some",
        TypeExpr.NewSymbol "Some",
        TypeExpr.Let(
          "None",
          TypeExpr.NewSymbol "None",
          TypeExpr.Lambda(
            ("a", Kind.Star) |> TypeParameter.Create,
            TypeExpr.Union([ (!!"Some", !!"a"); (!!"None", TypeExpr.Primitive PrimitiveType.Unit) ])
          )
        )
      ),
      Expr.Let(
        "func" |> Var.Create,
        None,
        Expr.TypeLambda(
          ("f", Kind.Arrow(Kind.Star, Kind.Star)) |> TypeParameter.Create,
          Expr.TypeLambda(
            ("a", Kind.Star) |> TypeParameter.Create,
            Expr.Lambda(
              "cons" |> Var.Create,
              Some(TypeExpr.Arrow(!!"a", TypeExpr.Apply(!!"f", !!"a"))),
              Expr.Lambda(
                "nil" |> Var.Create,
                Some(TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Unit, TypeExpr.Apply(!!"f", !!"a"))),
                Expr.Lambda(
                  "flag" |> Var.Create,
                  Some(TypeExpr.Primitive PrimitiveType.Bool),
                  Expr.Lambda(
                    "x" |> Var.Create,
                    Some(!!"a"),
                    Expr.If(
                      Expr.Lookup !"flag",
                      Expr.Apply(Expr.Lookup !"cons", Expr.Lookup !"x"),
                      Expr.Apply(Expr.Lookup !"nil", Expr.Primitive PrimitiveValue.Unit)
                    )
                  )
                )
              )
            )
          )
        ),
        Expr.TypeApply(Expr.TypeApply(Expr.Lookup !"func", !!"Option"), TypeExpr.Primitive PrimitiveType.Decimal)
      )
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.CreateArrow(
          TypeValue.CreateDecimal(),
          TypeValue.CreateArrow(TypeValue.CreateDecimal(), TypeValue.CreateDecimal())
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)

  match actual with
  | Sum.Left((_,
              TypeValue.Arrow { value = (TypeValue.Arrow { value = (TypeValue.Primitive { value = PrimitiveType.Decimal },
                                                                    TypeValue.Union _) },
                                         TypeValue.Arrow { value = (TypeValue.Arrow { value = (TypeValue.Primitive { value = PrimitiveType.Unit },
                                                                                               TypeValue.Union _) },
                                                                    TypeValue.Arrow { value = (TypeValue.Primitive { value = PrimitiveType.Bool },
                                                                                               TypeValue.Arrow { value = (TypeValue.Primitive { value = PrimitiveType.Decimal },
                                                                                                                          TypeValue.Union _) }) }) }) },
              Kind.Star),
             _) when true -> Assert.Pass()

  | Sum.Left((_, t, k), _) ->
    Assert.Fail $"Expected typechecking to succeed with 'Decimal -> Option[Decimal]' but succeeded with: {t}::{k}"

  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"


[<Test>]
let ``LangNext-TypeCheck should preserve given names in expr type let bindings and all origin type exprs`` () =
  let documentNumberCaseSymbol =
    "DocumentNumberCase" |> Identifier.LocalScope |> TypeSymbol.Create

  let documentNumberTy =
    TypeExpr.Union [ !!"DocumentNumberCase", TypeExpr.Primitive PrimitiveType.Int32 ]

  let documentDateCaseSymbol =
    "DocumentDateCase" |> Identifier.LocalScope |> TypeSymbol.Create

  let documentDateTy =
    TypeExpr.Union [ !!"DocumentDateCase", TypeExpr.Primitive PrimitiveType.String ]

  let numberFieldSymbol = "number" |> Identifier.LocalScope |> TypeSymbol.Create
  let dateFieldSymbol = "date" |> Identifier.LocalScope |> TypeSymbol.Create

  let predictionTy =
    TypeExpr.Record [ !!"number", !!"DocumentNumber"; !!"date", !!"DocumentDate" ]

  let myId =
    Expr.TypeLambda(
      ("A", Kind.Star) |> TypeParameter.Create,
      Expr.Lambda("x" |> Var.Create, Some !!"A", Expr.Lookup !"x")
    )

  let program =
    Expr.TypeLet(
      "DocumentNumber",
      documentNumberTy,
      Expr.TypeLet(
        "DocumentDate",
        documentDateTy,
        Expr.TypeLet("Prediction", predictionTy, Expr.TypeApply(myId, !!"Prediction"))
      )
    )

  let initialState =
    TypeCheckState.Empty
    |> TypeCheckState.Updaters.Types(
      TypeExprEvalState.Updaters.Symbols.Types(
        replaceWith (
          Map.ofList
            [ !"number", numberFieldSymbol
              !"date", dateFieldSymbol
              !"DocumentNumberCase", documentNumberCaseSymbol
              !"DocumentDateCase", documentDateCaseSymbol ]
        )
      )
    )

  let actual =
    Expr.TypeCheck None program |> State.Run(TypeCheckContext.Empty, initialState)

  let docNumberCaseValue =
    TypeValue.Primitive
      { value = PrimitiveType.Int32
        source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Int32) }

  let docNumberValue =
    TypeValue.Union
      { value = OrderedMap.ofList [ documentNumberCaseSymbol, docNumberCaseValue ]
        source = TypeExprSourceMapping.OriginExprTypeLet(ExprTypeLetBindingName "DocumentNumber", documentNumberTy) }

  let docDateCaseValue =
    TypeValue.Primitive
      { value = PrimitiveType.String
        source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.String) }

  let docDateValue =
    TypeValue.Union
      { value = OrderedMap.ofList [ documentDateCaseSymbol, docDateCaseValue ]
        source = TypeExprSourceMapping.OriginExprTypeLet(ExprTypeLetBindingName "DocumentDate", documentDateTy) }

  let predictionValue =
    TypeValue.Record
      { value = OrderedMap.ofList [ numberFieldSymbol, docNumberValue; dateFieldSymbol, docDateValue ]
        source = TypeExprSourceMapping.OriginExprTypeLet(ExprTypeLetBindingName "Prediction", predictionTy) }

  let expected =
    TypeValue.Arrow
      { value = predictionValue, predictionValue
        source =
          TypeExprSourceMapping.OriginTypeExpr(
            TypeExpr.Apply(
              TypeExpr.Lambda(TypeParameter.Create("A", Kind.Star), TypeExpr.Arrow(!!"A", !!"A")),
              TypeExpr.Lookup !"Prediction"
            )
          ) }

  match actual with
  | Sum.Left((_, actual, Kind.Star), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Left((_, _t, _k), _) -> Assert.Fail $"Expected {expected} but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"



[<Test>]
let ``LangNext-TypeCheck lookup fails when variable is not bound to a term`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup !"+", Expr.Primitive(PrimitiveValue.Int32 1)),
      Expr.Primitive(PrimitiveValue.Int32 2)
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty


  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Cannot find variable"; "+" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp ->
      err.Errors
      |> Seq.exists (fun err -> err.Message.ToLower().Contains(exp.ToLower())))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"


[<Test>]
let ``LangNext-TypeCheck application fails when applicand is not an arrow`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup !"++", Expr.Primitive(PrimitiveValue.Int32 1)),
      Expr.Primitive(PrimitiveValue.Int32 2)
    )

  let initialContext =
    TypeCheckContext.Empty
    |> TypeCheckContext.Updaters.Values(
      Map.add !"++" (TypeValue.CreateArrow(TypeValue.CreateInt32(), TypeValue.CreateInt32()), Kind.Star)
    )

  let initialState = TypeCheckState.Empty

  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "expected arrow type"; "int" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp ->
      err.Errors
      |> Seq.exists (fun err -> err.Message.ToLower().Contains(exp.ToLower())))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"



[<Test>]
let ``LangNext-TypeCheck tuple des fails with out of bounds (negative) index`` () =
  let program =
    Expr.Let(
      "v3" |> Var.Create,
      None,
      Expr.TupleCons(
        [ Expr.Primitive(PrimitiveValue.Decimal 1.0M)
          Expr.Primitive(PrimitiveValue.Decimal 2.0M)
          Expr.Primitive(PrimitiveValue.Decimal 3.0M) ]
      ),
      Expr.Apply(
        Expr.Apply(Expr.Lookup !"+", Expr.TupleDes(Expr.Lookup !"v3", { TupleDesSelector.Index = 0 })),
        Expr.TupleDes(Expr.Lookup !"v3", { TupleDesSelector.Index = -1 })
      )
    )

  let initialState = TypeCheckState.Empty

  let initialContext = initialContext PrimitiveType.Decimal

  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "cannot find item"; "-1" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp ->
      err.Errors
      |> Seq.exists (fun err -> err.Message.ToLower().Contains(exp.ToLower())))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"


[<Test>]
let ``LangNext-TypeCheck tuple des fails with out of bounds (too large) index`` () =
  let program =
    Expr.Let(
      "v3" |> Var.Create,
      None,
      Expr.TupleCons(
        [ Expr.Primitive(PrimitiveValue.Decimal 1.0M)
          Expr.Primitive(PrimitiveValue.Decimal 2.0M)
          Expr.Primitive(PrimitiveValue.Decimal 3.0M) ]
      ),
      Expr.Apply(
        Expr.Apply(Expr.Lookup !"+", Expr.TupleDes(Expr.Lookup !"v3", { TupleDesSelector.Index = 0 })),
        Expr.TupleDes(Expr.Lookup !"v3", { TupleDesSelector.Index = 3 })
      )
    )

  let initialState = TypeCheckState.Empty

  let initialContext = initialContext PrimitiveType.Decimal

  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "cannot find item"; "3" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp ->
      err.Errors
      |> Seq.exists (fun err -> err.Message.ToLower().Contains(exp.ToLower())))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"


[<Test>]
let ``LangNext-TypeCheck if-then-else fails with non-boolean condition`` () =
  let program =
    Expr.If(
      Expr.Primitive(PrimitiveValue.Decimal 2.0M),
      Expr.Primitive(PrimitiveValue.String "yes"),
      Expr.Primitive(PrimitiveValue.String "no")
    )

  let initialContext = initialContext PrimitiveType.Decimal

  let initialState = TypeCheckState.Empty

  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "cannot unify types"; "bool"; "decimal" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp ->
      err.Errors
      |> Seq.exists (fun err -> err.Message.ToLower().Contains(exp.ToLower())))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"


[<Test>]
let ``LangNext-TypeCheck if-then-else fails with incompatible branches`` () =
  let program =
    Expr.If(
      Expr.Primitive(PrimitiveValue.Bool true),
      Expr.Primitive(PrimitiveValue.String "yes"),
      Expr.Primitive(PrimitiveValue.Unit)
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty

  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "cannot unify types"; "string"; "()" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp ->
      err.Errors
      |> Seq.exists (fun err -> err.Message.ToLower().Contains(exp.ToLower())))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"


[<Test>]
let ``LangNext-TypeCheck type apply fails when the left side is not an HKT`` () =
  let program =
    Expr.TypeApply(Expr.Primitive PrimitiveValue.Unit, TypeExpr.Primitive PrimitiveType.Decimal)

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty
  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Expected arrow kind" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp ->
      err.Errors
      |> Seq.exists (fun err -> err.Message.ToLower().Contains(exp.ToLower())))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"


[<Test>]
let ``LangNext-TypeCheck type lambda fails when the variable cannot be generalized`` () =
  let program =
    Expr.TypeLambda(
      ("a", Kind.Star) |> TypeParameter.Create,
      Expr.Lambda(
        "x" |> Var.Create,
        Some(!!"a"),
        Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Decimal 1.0m))
      )
    )

  let initialContext = initialContext PrimitiveType.Decimal

  let initialState = TypeCheckState.Empty

  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "cannot remove variable"; "a"; "Decimal" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp ->
      err.Errors
      |> Seq.exists (fun err -> err.Message.ToLower().Contains(exp.ToLower())))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"



[<Test>]
let ``LangNext-TypeCheck type lambda succeeds when concrete type is passed to *->*`` () =
  let program =
    Expr.TypeApply(
      Expr.TypeLambda(("x", Kind.Star) |> TypeParameter.Create, Expr.Lookup(!"x")),
      TypeExpr.Primitive PrimitiveType.Decimal
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty
  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)

  let expected = PrimitiveType.Decimal

  match actual with
  | Sum.Left((_, TypeValue.Primitive({ value = actual }), Kind.Star), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t, k), _) ->
    Assert.Fail $"Expected typechecking to succeed with 'Decimal' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"


[<Test>]
let ``LangNext-TypeCheck type lambda succeeds when *->* type is passed to (*->*)->(*->*)`` () =
  let program =
    Expr.TypeLet(
      "id",
      TypeExpr.Lambda(("x", Kind.Star) |> TypeParameter.Create, !!"x"),
      Expr.TypeLet(
        "idid",
        TypeExpr.Lambda(("f", Kind.Arrow(Kind.Star, Kind.Star)) |> TypeParameter.Create, !!"f"),
        Expr.TypeApply(Expr.TypeApply(Expr.Lookup !"idid", !!"id"), TypeExpr.Primitive PrimitiveType.Unit)
      )
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty
  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)

  let expected =
    TypeValue.Primitive
      { value = PrimitiveType.Unit
        source =
          TypeExprSourceMapping.OriginTypeExpr(
            TypeExpr.Apply(
              TypeExpr.Lambda(TypeParameter.Create("x", Kind.Star), !!"x"),
              TypeExpr.Primitive PrimitiveType.Unit
            )
          ) }

  match actual with
  | Sum.Left((_, actual, Kind.Star), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t, k), _) -> Assert.Fail $"Expected typechecking to succeed with 'Unit' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"


[<Test>]
let ``LangNext-TypeCheck type lambda fails when passing *->* to *->*`` () =
  let program =
    Expr.TypeLet(
      "id",
      TypeExpr.Lambda(("x", Kind.Star) |> TypeParameter.Create, !!"x"),
      Expr.TypeApply(Expr.Lookup !"id", !!"id")
    )

  let initialContext = initialContext PrimitiveType.Decimal

  let initialState = TypeCheckState.Empty

  let actual = Expr.TypeCheck None program |> State.Run(initialContext, initialState)

  let expected_error_messages =
    [ "mismatched kind"; "expected Star"; "got Arrow (Star, Star)" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp ->
      err.Errors
      |> Seq.exists (fun err -> err.Message.ToLower().Contains(exp.ToLower())))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"
