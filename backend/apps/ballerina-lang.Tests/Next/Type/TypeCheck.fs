module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.TypeCheck

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.Errors
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.KindCheck
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Types.TypeCheck
open Ballerina.DSL.Next.KitchenSink
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError
open Ballerina.Reader.WithError
open Ballerina.Fun

let private (!) = Identifier.LocalScope
let (=>) t f = Identifier.FullyQualified([ t ], f)

[<Test>]
let ``LangNext-TypeCheck let typechecks`` () =

  let program =
    Expr.Let(
      "x" |> Var.Create,
      Expr.Primitive(PrimitiveValue.Int 10),
      Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Int 5))
    )

  let initialContext = TypeCheckContext.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Int,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int, TypeValue.Primitive PrimitiveType.Int)
        ))
    )

  let actual =
    TypeExpr.TypeCheck program |> State.Run(initialContext, TypeCheckState.Empty)

  match actual with
  | Sum.Left((_, TypeValue.Primitive(PrimitiveType.Int)), _) -> Assert.Pass()
  | Sum.Left((_, t), _) -> Assert.Fail $"Expected typechecking to succeed with 'Int' but succeeded with: {t}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"


[<Test>]
let ``LangNext-TypeCheck lambda infers and typechecks`` () =
  let program =
    Expr.Lambda(
      "x" |> Var.Create,
      None,
      Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Int 5))
    )

  let initialContext = TypeCheckContext.Empty

  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Int,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int, TypeValue.Primitive PrimitiveType.Int)
        ))
    )

  let actual =
    TypeExpr.TypeCheck program |> State.Run(initialContext, TypeCheckState.Empty)

  match actual with
  | Sum.Left((_, TypeValue.Arrow(TypeValue.Primitive(PrimitiveType.Int), TypeValue.Primitive(PrimitiveType.Int))), _) ->
    Assert.Pass()
  | Sum.Left((_, t), _) -> Assert.Fail $"Expected typechecking to succeed with 'Int -> Int' but succeeded with: {t}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"

[<Test>]
let ``LangNext-TypeCheck record cons typechecks with declared symbols`` () =
  let program =
    Expr.RecordCons(
      [ !"X", Expr.Primitive(PrimitiveValue.Int 10)
        !"Y", Expr.Primitive(PrimitiveValue.Bool true) ]
    )

  let initialContext = TypeCheckContext.Empty

  let X = TypeSymbol.Create "X"
  let Y = TypeSymbol.Create "Y"

  let initialState =
    TypeCheckState.Empty
    |> TypeCheckState.Updaters.Types(
      TypeExprEvalState.Updaters.Symbols(replaceWith (Map.ofList [ (!"X", X); (!"Y", Y) ]))
    )

  let actual = TypeExpr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected =
    TypeValue.Record(
      [ (X, TypeValue.Primitive PrimitiveType.Int)
        (Y, TypeValue.Primitive PrimitiveType.Bool) ]
      |> Map.ofList
    )

  match actual with
  | Sum.Left((_, actual), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t), _) -> Assert.Fail $"Expected typechecking to succeed with 'Int -> Int' but succeeded with: {t}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"


[<Test>]
let ``LangNext-TypeCheck union des typechecks with declared symbols and inferred branch argument`` () =
  let program =
    Expr.UnionDes(
      [ (!"Case1Of3",
         ("x" |> Var.Create,
          Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Int 1))))
        (!"Case2Of3",
         ("y" |> Var.Create,
          Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"y"), Expr.Primitive(PrimitiveValue.Int 2))))
        (!"Case3Of3", ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Int 3))) ]
      |> Map.ofList
    )

  let Case1Of3 = TypeSymbol.Create "Case1Of3"
  let Case2Of3 = TypeSymbol.Create "Case2Of3"
  let Case3Of3 = TypeSymbol.Create "Case3Of3"

  let initialContext = TypeCheckContext.Empty

  let initialState =
    TypeCheckState.Empty
    |> TypeCheckState.Updaters.Types(
      TypeExprEvalState.Updaters.Symbols(
        replaceWith (
          Map.ofList
            [ (!Case1Of3.Name, Case1Of3)
              (!Case2Of3.Name, Case2Of3)
              (!Case3Of3.Name, Case3Of3) ]
        )
      )
    )


  let initialContext =
    initialContext
    |> TypeCheckContext.Updaters.Values(
      Map.add
        !"+"
        (TypeValue.Arrow(
          TypeValue.Primitive PrimitiveType.Int,
          TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int, TypeValue.Primitive PrimitiveType.Int)
        ))
    )

  let actual = TypeExpr.TypeCheck program |> State.Run(initialContext, initialState)

  match actual with
  | Sum.Left((_, TypeValue.Arrow(TypeValue.Union cases, TypeValue.Primitive PrimitiveType.Int)), _) when
    cases |> Seq.length = 3
    && cases |> Map.tryFind Case1Of3 = Some(TypeValue.Primitive PrimitiveType.Int)
    && cases |> Map.tryFind Case2Of3 = Some(TypeValue.Primitive PrimitiveType.Int)
    ->
    Assert.Pass()
  | Sum.Left((_, t), _) ->
    Assert.Fail
      $"Expected typechecking to succeed with '(Case1Of3 of Int | Case2Of3 of Int | Case3Of3 of _) -> Int' but succeeded with: {t}"
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
        ))
    )

  let actual = TypeExpr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected = TypeValue.Primitive PrimitiveType.Decimal

  match actual with
  | Sum.Left((_, actual), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t), _) ->
    Assert.Fail
      $"Expected typechecking to succeed with '(Case1Of3 of Int | Case2Of3 of Int | Case3Of3 of _) -> Int' but succeeded with: {t}"
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
        ))
    )

  let actual = TypeExpr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected = TypeValue.Primitive PrimitiveType.Decimal

  match actual with
  | Sum.Left((_, actual), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t), _) ->
    Assert.Fail
      $"Expected typechecking to succeed with '(Case1Of3 of Int | Case2Of3 of Int | Case3Of3 of _) -> Int' but succeeded with: {t}"
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
        ))
    )

  let actual = TypeExpr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected = TypeValue.Primitive PrimitiveType.Decimal

  match actual with
  | Sum.Left((_, actual), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t), _) ->
    Assert.Fail
      $"Expected typechecking to succeed with '(Case1Of3 of Int | Case2Of3 of Int | Case3Of3 of _) -> Int' but succeeded with: {t}"
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
        ))
    )

  let actual = TypeExpr.TypeCheck program |> State.Run(initialContext, initialState)
  let expected = TypeValue.Primitive PrimitiveType.String

  match actual with
  | Sum.Left((_, actual), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t), _) -> Assert.Fail $"Expected typechecking to succeed with 'String' but succeeded with: {t}"
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
          [ (0,
             ("x" |> Var.Create,
              Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"x"), Expr.Primitive(PrimitiveValue.Decimal 1.0M))))
            (1,
             ("y" |> Var.Create,
              Expr.Apply(Expr.Apply(Expr.Lookup !"+", Expr.Lookup !"y"), Expr.Primitive(PrimitiveValue.Decimal 2.0M))))
            (2, ("_" |> Var.Create, Expr.Primitive(PrimitiveValue.Decimal 3.0M))) ]
          |> Map.ofList
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
        ))
    )

  let actual = TypeExpr.TypeCheck program |> State.Run(initialContext, initialState)

  let expected = TypeValue.Primitive PrimitiveType.Decimal

  match actual with
  | Sum.Left((_, actual), _) when actual = expected -> Assert.Pass()
  | Sum.Left((_, t), _) ->
    Assert.Fail
      $"Expected typechecking to succeed with '(Case1Of3 of Int | Case2Of3 of Int | Case3Of3 of _) -> Int' but succeeded with: {t}"
  | Sum.Right err -> Assert.Fail $"Expected typechecking to succeed but failed with: {err}"
