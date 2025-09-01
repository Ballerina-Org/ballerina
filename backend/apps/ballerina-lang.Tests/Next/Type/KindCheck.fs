module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.KindCheck

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.Errors
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.KindCheck
open Ballerina.DSL.Next.KitchenSink
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError
open Ballerina.Reader.WithError

let private (!) = Identifier.LocalScope

[<Test>]
let ``LangNext-Unify kind check of star over symbol fails`` () =

  let a = TypeParameter.Create("a", Kind.Symbol)

  let program =
    TypeExpr.Apply(TypeExpr.Lambda(a, TypeExpr.Primitive(PrimitiveType.Int32)), TypeExpr.Primitive(PrimitiveType.Int32))

  let actual = TypeExpr.KindCheck program |> Reader.Run(KindCheckContext.Empty)

  match actual with
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"
  | Sum.Right _err -> Assert.Pass()

[<Test>]
let ``LangNext-Unify kind check of star=>star over star fails`` () =

  let f = TypeParameter.Create("f", Kind.Arrow(Kind.Star, Kind.Star))

  let program =
    TypeExpr.Apply(TypeExpr.Lambda(f, TypeExpr.Primitive(PrimitiveType.Int32)), TypeExpr.Primitive(PrimitiveType.Int32))

  let actual = TypeExpr.KindCheck program |> Reader.Run(KindCheckContext.Empty)

  match actual with
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"
  | Sum.Right _err -> Assert.Pass()

[<Test>]
let ``LangNext-Unify kind check of symbol over symbol succeeds as the identity`` () =

  let a = TypeParameter.Create("a", Kind.Symbol)

  let program =
    TypeExpr.Apply(
      TypeExpr.Lambda(a, TypeExpr.Lookup(a.Name |> Identifier.LocalScope)),
      TypeExpr.Lookup(a.Name |> Identifier.LocalScope)
    )

  let actual =
    TypeExpr.KindCheck program
    |> Reader.Run(KindCheckContext.Create([ !a.Name, Kind.Symbol ] |> Map.ofList))

  match actual with
  | Sum.Left(Kind.Symbol) -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected symbol kind but got: {res}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-Unify kind check of curried application succeeds`` () =

  // Lambda((f:Sym) (t:Star) => { f:t -> t }) "a" Int
  let f = TypeParameter.Create("f", Kind.Symbol)
  let t = TypeParameter.Create("t", Kind.Star)

  let program =
    TypeExpr.Apply(
      TypeExpr.Apply(
        TypeExpr.Lambda(
          f,
          TypeExpr.Lambda(
            t,
            TypeExpr.Record(
              [ TypeExpr.Lookup(f.Name |> Identifier.LocalScope),
                TypeExpr.Arrow(
                  TypeExpr.Lookup(t.Name |> Identifier.LocalScope),
                  TypeExpr.Lookup(t.Name |> Identifier.LocalScope)
                ) ]
            )
          )
        ),
        TypeExpr.Lookup("a" |> Identifier.LocalScope)
      ),
      TypeExpr.Primitive(PrimitiveType.Int32)
    )

  let actual =
    TypeExpr.KindCheck program
    |> Reader.Run(KindCheckContext.Create([ !"a", Kind.Symbol ] |> Map.ofList))

  match actual with
  | Sum.Left(Kind.Star) -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected star kind but got: {res}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"


[<Test>]
let ``LangNext-Unify kind check of curried application identity succeeds`` () =

  // (Lambda((f:* => *) => f) Option)Int
  let f = TypeParameter.Create("f", Kind.Arrow(Kind.Star, Kind.Star))

  let program =
    TypeExpr.Apply(
      TypeExpr.Apply(
        TypeExpr.Lambda(f, TypeExpr.Lookup(f.Name |> Identifier.LocalScope)),
        TypeExpr.Lookup("Option" |> Identifier.LocalScope)
      ),
      TypeExpr.Primitive(PrimitiveType.Int32)
    )

  let actual =
    TypeExpr.KindCheck program
    |> Reader.Run(KindCheckContext.Create([ !"Option", Kind.Arrow(Kind.Star, Kind.Star) ] |> Map.ofList))

  match actual with
  | Sum.Left(Kind.Star) -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected star kind but got: {res}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-Unify kind check of curried application eta-identity succeeds`` () =

  // Lambda((f:* => *) (t:*) => f[t]) Option Int
  let f = TypeParameter.Create("f", Kind.Arrow(Kind.Star, Kind.Star))
  let t = TypeParameter.Create("t", Kind.Star)

  let program =
    TypeExpr.Apply(
      TypeExpr.Apply(
        TypeExpr.Lambda(
          f,
          TypeExpr.Lambda(
            t,
            TypeExpr.Apply(
              TypeExpr.Lookup(f.Name |> Identifier.LocalScope),
              TypeExpr.Lookup(t.Name |> Identifier.LocalScope)
            )
          )
        ),
        TypeExpr.Lookup("Option" |> Identifier.LocalScope)
      ),
      TypeExpr.Primitive(PrimitiveType.Int32)
    )

  let actual =
    TypeExpr.KindCheck program
    |> Reader.Run(KindCheckContext.Create([ !"Option", Kind.Arrow(Kind.Star, Kind.Star) ] |> Map.ofList))

  match actual with
  | Sum.Left(Kind.Star) -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected star kind but got: {res}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
