module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.TypeCheck

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.Errors
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Types.TypeCheck
open Ballerina.DSL.Next.KitchenSink
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError
open Ballerina.Reader.WithError
open Ballerina.Fun

let private (!) = Identifier.LocalScope
let private (=>) t f = Identifier.FullyQualified([ t ], f)
let private (!!) = Identifier.LocalScope >> TypeExpr.Lookup
let private (=>>) = Identifier.FullyQualified >> TypeExpr.Lookup

let private initialContext t =
  TypeCheckContext.Empty
  |> TypeCheckContext.Updaters.Values(
    Map.add
      !"+"
      (TypeValue.Arrow(TypeValue.Primitive t, TypeValue.Arrow(TypeValue.Primitive t, TypeValue.Primitive t)), Kind.Star)
  )

[<Test>]
let ``LangNext-TypeCheck let typechecks`` () =

  let program =
    Expr.Let(
      "x" |> Var.Create,
      Expr.Primitive(PrimitiveValue.Int32 10),
      Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Int32 5))
    )

  let initialContext = TypeCheckContext.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Int32,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Int32)
         ),
         Kind.Star)
    )

  let actual =
    Expr.TypeCheck program |> State.Run(initialContext, TypeCheckState.Empty)

  match actual with
  | Sum.Left((_, TypeValue.Primitive(PrimitiveType.Int32), Kind.Star), _) -> Assert.Pass()
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
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Int32,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Int32)
         ),
         Kind.Star)
    )

  let actual =
    Expr.TypeCheck program |> State.Run(initialContext, TypeCheckState.Empty)

  match actual with
  | Sum.Left((_,
              TypeValue.Arrow(TypeValue.Primitive(PrimitiveType.Int32), TypeValue.Primitive(PrimitiveType.Int32)),
              Kind.Star),
             _) -> Assert.Pass()
  | Sum.Left((_, t, k), _) ->
    Assert.Fail $"Expected typechecking to succeed with '(Int -> Int)::*' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"

[<Test>]
let ``LangNext-TypeCheck record cons typechecks with declared symbols`` () =
  let program =
    Expr.RecordCons(
      [ !"X", Expr.Primitive(PrimitiveValue.Int32 10)
        !"Y", Expr.Primitive(PrimitiveValue.Bool true) ]
    )

  let initialContext = TypeCheckContext.Empty

  let X = "X" |> Identifier.LocalScope |> TypeSymbol.Create
  let Y = "Y" |> Identifier.LocalScope |> TypeSymbol.Create

  let initialState =
    TypeCheckState.Empty
    |> TypeCheckState.Updaters.Types(
      TypeExprEvalState.Updaters.Symbols(replaceWith (Map.ofList [ (!"X", X); (!"Y", Y) ]))
    )

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected =
    TypeValue.Record(
      [ (X, TypeValue.Primitive PrimitiveType.Int32)
        (Y, TypeValue.Primitive PrimitiveType.Bool) ]
      |> Map.ofList
    )

  match actual with
  | Sum.Left((_, actual, Kind.Star), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t, k), _) ->
    Assert.Fail $"Expected typechecking to succeed with 'Int -> Int' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"


[<Test>]
let ``LangNext-TypeCheck union des typechecks with declared symbols and inferred branch argument`` () =
  let program =
    Expr.UnionDes(
      [ (!"Case1Of3",
         ("x" |> Var.Create,
          Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Int32 1))))
        (!"Case2Of3",
         ("y" |> Var.Create,
          Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"y"), Expr.Primitive(PrimitiveValue.Int32 2))))
        (!"Case3Of3", ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Int32 3))) ]
      |> Map.ofList
    )

  let Case1Of3 = "Case1Of3" |> Identifier.LocalScope |> TypeSymbol.Create
  let Case2Of3 = "Case2Of3" |> Identifier.LocalScope |> TypeSymbol.Create
  let Case3Of3 = "Case3Of3" |> Identifier.LocalScope |> TypeSymbol.Create

  let initialContext = TypeCheckContext.Empty

  let initialState =
    TypeCheckState.Empty
    |> TypeCheckState.Updaters.Types(
      TypeExprEvalState.Updaters.Symbols(
        replaceWith (
          Map.ofList
            [ (Case1Of3.Name, Case1Of3)
              (Case2Of3.Name, Case2Of3)
              (Case3Of3.Name, Case3Of3) ]
        )
      )
    )


  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Int32,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Int32)
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Sum.Left((_, TypeValue.Arrow(TypeValue.Union cases, TypeValue.Primitive PrimitiveType.Int32), Kind.Star), _) when
    cases |> Seq.length = 3
    && cases |> Map.tryFind Case1Of3 = Some(TypeValue.Primitive PrimitiveType.Int32)
    && cases |> Map.tryFind Case2Of3 = Some(TypeValue.Primitive PrimitiveType.Int32)
    ->
    Assert.Pass()
  | Sum.Left((_, t, k), _) ->
    Assert.Fail
      $"Expected typechecking to succeed with '(Case1Of3 of Int | Case2Of3 of Int | Case3Of3 of _) -> Int' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"


[<Test>]
let ``LangNext-TypeCheck record cons and record des typecheck with a new type declaration`` () =
  let program =
    Expr.TypeLet(
      "Vector3",
      TypeExpr.Let(
        "X",
        TypeExpr.NewSymbol "X",
        TypeExpr.Let(
          "Y",
          TypeExpr.NewSymbol "Y",
          TypeExpr.Let(
            "Z",
            TypeExpr.NewSymbol "Z",
            TypeExpr.Record(
              [ (TypeExpr.Lookup(!"X"), TypeExpr.Primitive PrimitiveType.Decimal)
                (TypeExpr.Lookup(!"Y"), TypeExpr.Primitive PrimitiveType.Decimal)
                (TypeExpr.Lookup(!"Z"), TypeExpr.Primitive PrimitiveType.Decimal) ]
            )
          )
        )
      ),
      Expr.Let(
        "v3" |> Var.Create,
        Expr.RecordCons(
          [ ("Vector3" => "X", Expr.Primitive(PrimitiveValue.Decimal 1.0M))
            ("Vector3" => "Y", Expr.Primitive(PrimitiveValue.Decimal 2.0M))
            ("Vector3" => "Z", Expr.Primitive(PrimitiveValue.Decimal 3.0M)) ]
        ),
        Expr.Apply(
          Expr.Apply(
            Expr.Lookup !"+",
            Expr.Apply(
              Expr.Apply(Expr.Lookup !"+", Expr.RecordDes(Expr.Lookup !"v3", "Vector3" => "X")),
              Expr.RecordDes(Expr.Lookup !"v3", !"Y")
            )
          ),
          Expr.RecordDes(Expr.Lookup !"v3", !"Z")
        )
      )
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Decimal,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Decimal)
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected = TypeValue.Primitive PrimitiveType.Decimal

  match actual with
  | Sum.Left((_, actual, Kind.Star), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t, k), _) ->
    Assert.Fail
      $"Expected typechecking to succeed with '(Case1Of3 of Int | Case2Of3 of Int | Case3Of3 of _) -> Int' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"



[<Test>]
let ``LangNext-TypeCheck union cons and des typecheck with new type and inferred branch argument`` () =
  let program =
    Expr.TypeLet(
      "Either3",
      TypeExpr.Let(
        "Choice1Of3",
        TypeExpr.NewSymbol "Choice1Of3",
        TypeExpr.Let(
          "Choice2Of3",
          TypeExpr.NewSymbol "Choice2Of3",
          TypeExpr.Let(
            "Choice3Of3",
            TypeExpr.NewSymbol "Choice3Of3",
            TypeExpr.Union(
              [ (TypeExpr.Lookup(!"Choice1Of3"), TypeExpr.Primitive PrimitiveType.Decimal)
                (TypeExpr.Lookup(!"Choice2Of3"), TypeExpr.Primitive PrimitiveType.Decimal)
                (TypeExpr.Lookup(!"Choice3Of3"), TypeExpr.Primitive PrimitiveType.String) ]
            )
          )
        )
      ),
      Expr.Let(
        "v" |> Var.Create,
        Expr.UnionCons(!"Choice1Of3", Expr.Primitive(PrimitiveValue.Decimal 1.0M)),
        Expr.Apply(
          Expr.UnionDes(
            [ (!"Choice1Of3",
               ("x" |> Var.Create,
                Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Decimal 1.0m))))
              (!"Choice2Of3",
               ("y" |> Var.Create,
                Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"y"), Expr.Primitive(PrimitiveValue.Decimal 2.0m))))
              (!"Choice3Of3", ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Decimal 3.0m))) ]
            |> Map.ofList
          ),
          Expr.Lookup !"v"
        )
      )
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Decimal,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Decimal)
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected = TypeValue.Primitive PrimitiveType.Decimal

  match actual with
  | Sum.Left((_, actual, Kind.Star), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t, k), _) ->
    Assert.Fail
      $"Expected typechecking to succeed with '(Case1Of3 of Int | Case2Of3 of Int | Case3Of3 of _) -> Int' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"


[<Test>]
let ``LangNext-TypeCheck tuple cons and des typecheck`` () =
  let program =
    Expr.Let(
      "v3" |> Var.Create,
      Expr.TupleCons(
        [ Expr.Primitive(PrimitiveValue.Decimal 1.0M)
          Expr.Primitive(PrimitiveValue.Decimal 2.0M)
          Expr.Primitive(PrimitiveValue.Decimal 3.0M) ]
      ),
      Expr.Apply(
        Expr.Apply(Expr.Lookup !"+", Expr.TupleDes(Expr.Lookup !"v3", { TupleDesSelector.Index = 0 })),
        Expr.TupleDes(Expr.Lookup !"v3", { TupleDesSelector.Index = 1 })
      )
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Decimal,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Decimal)
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected = TypeValue.Primitive PrimitiveType.Decimal

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
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Decimal,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Bool)
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected = TypeValue.Primitive PrimitiveType.String

  match actual with
  | Sum.Left((_, actual, Kind.Star), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t, k), _) -> Assert.Fail $"Expected typechecking to succeed with 'String' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"



[<Test>]
let ``LangNext-TypeCheck sum cons and des typecheck`` () =
  let program =
    Expr.Let(
      "oneOfThree" |> Var.Create,
      Expr.SumCons(
        { SumConsSelector.Case = 0
          SumConsSelector.Count = 3 },
        Expr.Primitive(PrimitiveValue.Decimal 1.0M)
      ),
      Expr.Apply(
        Expr.SumDes(
          [ ("x" |> Var.Create,
             Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Decimal 1.0M)))
            ("y" |> Var.Create,
             Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"y"), Expr.Primitive(PrimitiveValue.Decimal 2.0M)))
            ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Decimal 3.0M)) ]
        ),
        Expr.Lookup(!"oneOfThree")
      )
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Decimal,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Decimal)
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected = TypeValue.Primitive PrimitiveType.Decimal

  match actual with
  | Sum.Left((_, actual, Kind.Star), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t, k), _) ->
    Assert.Fail
      $"Expected typechecking to succeed with '(Case1Of3 of Int | Case2Of3 of Int | Case3Of3 of _) -> Int' but succeeded with: {t}::{k}"
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
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Decimal,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Decimal)
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Sum.Left((_, // note: the unions are all instances of `Option[Decimal]` here, omitted for brevity
              TypeValue.Arrow(TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Union(_)), // cons
                              TypeValue.Arrow(TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Unit, TypeValue.Union(_)), // nil
                                              TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Bool, // flag
                                                              TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, // x
                                                                              TypeValue.Union(_))))),
              Kind.Star),
             _) when true -> Assert.Pass()
  | Sum.Left((_, t, k), _) ->
    Assert.Fail $"Expected typechecking to succeed with 'Decimal -> Option[Decimal]' but succeeded with: {t}::{k}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"



[<Test>]
let ``LangNext-TypeCheck record cons fail when symbol does not exist`` () =
  let program =
    Expr.RecordCons(
      [ !"X", Expr.Primitive(PrimitiveValue.Int32 10)
        !"NON_EXISTENT_ID", Expr.Primitive(PrimitiveValue.Bool true) ]
    )

  let initialContext = TypeCheckContext.Empty

  let X = "X" |> Identifier.LocalScope |> TypeSymbol.Create
  let Y = "Y" |> Identifier.LocalScope |> TypeSymbol.Create

  let initialState =
    TypeCheckState.Empty
    |> TypeCheckState.Updaters.Types(
      TypeExprEvalState.Updaters.Symbols(replaceWith (Map.ofList [ (!"X", X); (!"Y", Y) ]))
    )

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Cannot find symbols"; "NON_EXISTENT_ID" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"



[<Test>]
let ``LangNext-TypeCheck record des fail when symbol does not exist`` () =
  let program =
    Expr.TypeLet(
      "Vector3",
      TypeExpr.Let(
        "X",
        TypeExpr.NewSymbol "X",
        TypeExpr.Let(
          "Y",
          TypeExpr.NewSymbol "Y",
          TypeExpr.Let(
            "Z",
            TypeExpr.NewSymbol "Z",
            TypeExpr.Record(
              [ (TypeExpr.Lookup(!"X"), TypeExpr.Primitive PrimitiveType.Decimal)
                (TypeExpr.Lookup(!"Y"), TypeExpr.Primitive PrimitiveType.Decimal)
                (TypeExpr.Lookup(!"Z"), TypeExpr.Primitive PrimitiveType.Decimal) ]
            )
          )
        )
      ),
      Expr.Let(
        "v3" |> Var.Create,
        Expr.RecordCons(
          [ ("Vector3" => "X", Expr.Primitive(PrimitiveValue.Decimal 1.0M))
            ("Vector3" => "Y", Expr.Primitive(PrimitiveValue.Decimal 2.0M))
            ("Vector3" => "Z", Expr.Primitive(PrimitiveValue.Decimal 3.0M)) ]
        ),
        Expr.Apply(
          Expr.Apply(
            Expr.Lookup !"+",
            Expr.Apply(
              Expr.Apply(Expr.Lookup !"+", Expr.RecordDes(Expr.Lookup !"v3", "Vector3" => "X")),
              Expr.RecordDes(Expr.Lookup !"v3", !"Y")
            )
          ),
          Expr.RecordDes(Expr.Lookup !"v3", !"NON_EXISTENT_FIELD")
        )
      )
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Decimal,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Decimal, TypeValue.Primitive PrimitiveType.Decimal)
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Cannot find symbols"; "NON_EXISTENT_FIELD" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"



[<Test>]
let ``LangNext-TypeCheck union cons fail when symbol does not exist`` () =
  let program =
    Expr.UnionCons(!"NON_EXISTENT_ID", Expr.Primitive(PrimitiveValue.Decimal 1.0M))


  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Cannot find symbols"; "NON_EXISTENT_ID" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"


[<Test>]
let ``LangNext-TypeCheck union des fails on non-existent case`` () =
  let program =
    Expr.UnionDes(
      [ (!"NON_EXISTENT_CASE",
         ("x" |> Var.Create,
          Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Int32 1))))
        (!"Case2Of3",
         ("y" |> Var.Create,
          Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"y"), Expr.Primitive(PrimitiveValue.Int32 2))))
        (!"Case3Of3", ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Int32 3))) ]
      |> Map.ofList
    )

  let Case1Of3 = "Case1Of3" |> Identifier.LocalScope |> TypeSymbol.Create
  let Case2Of3 = "Case2Of3" |> Identifier.LocalScope |> TypeSymbol.Create
  let Case3Of3 = "Case3Of3" |> Identifier.LocalScope |> TypeSymbol.Create

  let initialContext = TypeCheckContext.Empty

  let initialState =
    TypeCheckState.Empty
    |> TypeCheckState.Updaters.Types(
      TypeExprEvalState.Updaters.Symbols(
        replaceWith (
          Map.ofList
            [ (Case1Of3.Name, Case1Of3)
              (Case2Of3.Name, Case2Of3)
              (Case3Of3.Name, Case3Of3) ]
        )
      )
    )


  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Int32,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Int32)
         ),
         Kind.Star)
    )

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Cannot find symbols"; "NON_EXISTENT_CASE" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"



[<Test>]
let ``LangNext-TypeCheck union des fails when branches differ in return type`` () =
  let program =
    Expr.UnionDes(
      [ (!"Case1Of3",
         ("x" |> Var.Create,
          Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Int32 1))))
        (!"Case2Of3",
         ("y" |> Var.Create,
          Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"y"), Expr.Primitive(PrimitiveValue.Int32 2))))
        (!"Case3Of3", ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.String "not an int eh"))) ]
      |> Map.ofList
    )

  let Case1Of3 = "Case1Of3" |> Identifier.LocalScope |> TypeSymbol.Create
  let Case2Of3 = "Case2Of3" |> Identifier.LocalScope |> TypeSymbol.Create
  let Case3Of3 = "Case3Of3" |> Identifier.LocalScope |> TypeSymbol.Create

  let initialState =
    TypeCheckState.Empty
    |> TypeCheckState.Updaters.Types(
      TypeExprEvalState.Updaters.Symbols(
        replaceWith (
          Map.ofList
            [ (Case1Of3.Name, Case1Of3)
              (Case2Of3.Name, Case2Of3)
              (Case3Of3.Name, Case3Of3) ]
        )
      )
    )


  let actual =
    Expr.TypeCheck program
    |> State.Run(initialContext PrimitiveType.Int32, initialState)

  let expected_error_messages = [ "Cannot unify"; "Int32"; "String" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"


[<Test>]
let ``LangNext-TypeCheck lookup fails when variable is not bound to a term`` () =
  let program =
    Expr.Apply(
      Expr.Apply(Expr.Lookup !"+", Expr.Primitive(PrimitiveValue.Int32 1)),
      Expr.Primitive(PrimitiveValue.Int32 2)
    )

  let initialContext = TypeCheckContext.Empty

  let initialState = TypeCheckState.Empty


  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Cannot find variable"; "+" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
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
      Map.add
        !"++"
        (TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.Int32), Kind.Star)
    )

  let initialState = TypeCheckState.Empty

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "expected arrow type"; "Int32" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"



[<Test>]
let ``LangNext-TypeCheck tuple des fails with out of bounds (negative) index`` () =
  let program =
    Expr.Let(
      "v3" |> Var.Create,
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

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "cannot find item"; "-1" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"


[<Test>]
let ``LangNext-TypeCheck tuple des fails with out of bounds (too large) index`` () =
  let program =
    Expr.Let(
      "v3" |> Var.Create,
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

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "cannot find item"; "3" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
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

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Cannot unify types"; "Bool"; "Decimal" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
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

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Cannot unify types"; "String"; "Unit" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"



[<Test>]
let ``LangNext-TypeCheck sum des fails on mismatched types with the same arity`` () =
  let program =
    Expr.Let(
      "oneOfThree" |> Var.Create,
      Expr.SumCons(
        { SumConsSelector.Case = 0
          SumConsSelector.Count = 3 },
        Expr.Primitive(PrimitiveValue.String "not a decimal eh")
      ),
      Expr.Apply(
        Expr.SumDes(
          [ ("x" |> Var.Create,
             Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Decimal 1.0M)))
            ("y" |> Var.Create,
             Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"y"), Expr.Primitive(PrimitiveValue.Decimal 2.0M)))
            ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Decimal 3.0M)) ]
        ),
        Expr.Lookup(!"oneOfThree")
      )
    )

  let initialContext = initialContext PrimitiveType.Decimal
  let initialState = TypeCheckState.Empty

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected_error_messages = [ "Cannot unify types"; "Sum" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"


[<Test>]
let ``LangNext-TypeCheck sum des fails on missing cases`` () =
  let program =
    Expr.Let(
      "oneOfThree" |> Var.Create,
      Expr.SumCons(
        { SumConsSelector.Case = 0
          SumConsSelector.Count = 3 },
        Expr.Primitive(PrimitiveValue.Decimal 1.0M)
      ),
      Expr.Apply(
        Expr.SumDes(
          [ ("x" |> Var.Create,
             Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Decimal 1.0M)))
            ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Decimal 3.0M)) ]
        ),
        Expr.Lookup(!"oneOfThree")
      )
    )

  let initialContext = initialContext PrimitiveType.Decimal

  let initialState = TypeCheckState.Empty

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected_error_messages = [ "Cannot unify types"; "Sum" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"



[<Test>]
let ``LangNext-TypeCheck union des fails on case with wrong payload`` () =
  let program =
    Expr.TypeLet(
      "Either3",
      TypeExpr.Let(
        "Choice1Of3",
        TypeExpr.NewSymbol "Choice1Of3",
        TypeExpr.Let(
          "Choice2Of3",
          TypeExpr.NewSymbol "Choice2Of3",
          TypeExpr.Let(
            "Choice3Of3",
            TypeExpr.NewSymbol "Choice3Of3",
            TypeExpr.Union(
              [ (TypeExpr.Lookup(!"Choice1Of3"), TypeExpr.Primitive PrimitiveType.Decimal)
                (TypeExpr.Lookup(!"Choice2Of3"), TypeExpr.Primitive PrimitiveType.Decimal)
                (TypeExpr.Lookup(!"Choice3Of3"), TypeExpr.Primitive PrimitiveType.String) ]
            )
          )
        )
      ),
      Expr.Let(
        "v" |> Var.Create,
        Expr.UnionCons(!"Choice1Of3", Expr.Primitive(PrimitiveValue.String "not a decimal eh")),
        Expr.Apply(
          Expr.UnionDes(
            [ (!"Choice1Of3",
               ("x" |> Var.Create,
                Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Decimal 1.0m))))
              (!"Choice2Of3",
               ("y" |> Var.Create,
                Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"y"), Expr.Primitive(PrimitiveValue.Decimal 2.0m))))
              (!"Choice3Of3", ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Decimal 3.0m))) ]
            |> Map.ofList
          ),
          Expr.Lookup !"v"
        )
      )
    )

  let initialContext = initialContext PrimitiveType.Decimal

  let initialState = TypeCheckState.Empty

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Cannot unify types"; "String"; "Decimal" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"


[<Test>]
let ``LangNext-TypeCheck union des fails on missing case`` () =
  let program =
    Expr.TypeLet(
      "Either3",
      TypeExpr.Let(
        "Choice1Of3",
        TypeExpr.NewSymbol "Choice1Of3",
        TypeExpr.Let(
          "Choice2Of3",
          TypeExpr.NewSymbol "Choice2Of3",
          TypeExpr.Let(
            "Choice3Of3",
            TypeExpr.NewSymbol "Choice3Of3",
            TypeExpr.Union(
              [ (TypeExpr.Lookup(!"Choice1Of3"), TypeExpr.Primitive PrimitiveType.Decimal)
                (TypeExpr.Lookup(!"Choice2Of3"), TypeExpr.Primitive PrimitiveType.Decimal)
                (TypeExpr.Lookup(!"Choice3Of3"), TypeExpr.Primitive PrimitiveType.String) ]
            )
          )
        )
      ),
      Expr.Let(
        "v" |> Var.Create,
        Expr.UnionCons(!"Choice1Of3", Expr.Primitive(PrimitiveValue.Decimal 1.0m)),
        Expr.Apply(
          Expr.UnionDes(
            [ (!"Choice1Of3",
               ("x" |> Var.Create,
                Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Decimal 1.0m))))
              (!"Choice3Of3", ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Decimal 3.0m))) ]
            |> Map.ofList
          ),
          Expr.Lookup !"v"
        )
      )
    )

  let initialContext = initialContext PrimitiveType.Decimal

  let initialState = TypeCheckState.Empty

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Cannot unify types"; "Union" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
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
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "Expected arrow kind" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
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

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected_error_messages = [ "cannot remove variable"; "a"; "Decimal" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
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
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected = TypeValue.Primitive PrimitiveType.Decimal

  match actual with
  | Sum.Left((_, actual, Kind.Star), _) when actual = expected -> Assert.Pass()
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
  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected = TypeValue.Primitive PrimitiveType.Unit

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

  let actual = Expr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected_error_messages =
    [ "mismatched kind"; "expected Star"; "got Arrow (Star, Star)" ]

  match actual with
  | Sum.Left _ -> Assert.Fail $"Expected typechecking to fail but succeeded"
  | Sum.Right(err, _) when
    expected_error_messages
    |> Seq.forall (fun exp -> err.Errors |> Seq.exists (fun err -> err.Message.Contains exp))
    ->
    Assert.Pass()
  | Sum.Right(err, _) ->
    Assert.Fail $"Expected typechecking to fail with {expected_error_messages} but fail with: {err}"
