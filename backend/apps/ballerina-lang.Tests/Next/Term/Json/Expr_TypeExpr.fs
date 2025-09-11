module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Json.Expr_TypeExpr

open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open NUnit.Framework
open FSharp.Data
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.Json
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Json

let private (!) = Identifier.LocalScope

let ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``
  (expression: Expr<TypeExpr>)
  (expectedJson: JsonValue)
  =
  let toJson = Expr.ToJson >> Reader.Run TypeExpr.ToJson

  match toJson expression with
  | Right err -> Assert.Fail $"Encode failed: {err}"
  | Left json ->
    let toStr (j: JsonValue) =
      j.ToString(JsonSaveOptions.DisableFormatting)

    Assert.That(toStr json, Is.EqualTo(toStr expectedJson))

    let parser = Expr.FromJson >> Reader.Run TypeExpr.FromJson

    let parsed = parser expectedJson

    match parsed with
    | Right err -> Assert.Fail $"Parse failed: {err}"
    | Left result -> Assert.That(result, Is.EqualTo(expression))

[<Test>]
let ``Dsl:Terms:Expr.Lambda json round-trip`` () =
  let json = """{"kind":"lambda","lambda":["x",{"kind":"int","int":"42"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.Lambda(Var.Create "x", None, Expr.Primitive(PrimitiveValue.Int 42))

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.TypeLambda json round-trip`` () =
  let json =
    """{"kind":"type-lambda","type-lambda":[{"name":"T","kind":{"kind":"star"}},{"kind":"int","int":"42"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.TypeLambda(TypeParameter.Create("T", Kind.Star), Expr.Primitive(PrimitiveValue.Int 42))

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ```Dsl:Terms:Expr.TypeApply json round-trip`` () =
  let json =
    """{"kind":"type-apply","type-apply":[{"kind":"lookup","name":"f"}, {"kind":"int32"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.TypeApply(Expr.Lookup("f" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32)

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.Apply json round-trip`` () =
  let json =
    """{"kind":"apply","apply":[{"kind":"lambda","lambda":["x",{"kind":"int","int":"1"}]}, {"kind":"int","int":"2"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.Apply(
      Expr.Lambda(Var.Create "x", None, Expr.Primitive(PrimitiveValue.Int 1)),
      Expr.Primitive(PrimitiveValue.Int 2)
    )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.Let json round-trip`` () =
  let json =
    """{"kind":"let","let":["y", {"kind":"int","int":"5"}, {"kind":"int","int":"6"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.Let(Var.Create "y", Expr.Primitive(PrimitiveValue.Int 5), Expr.Primitive(PrimitiveValue.Int 6))

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.TypeLet json round-trip`` () =
  let json =
    """{"kind":"type-let","type-let":["T", {"kind":"int32"}, {"kind":"int","int":"7"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.TypeLet("T", TypeExpr.Primitive PrimitiveType.Int32, Expr.Primitive(PrimitiveValue.Int 7))

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.RecordCons json round-trip`` () =
  let json =
    """{"kind":"record-cons","fields":[[{"kind":"id","name":"Bar"},{"kind":"int","int":"1"}],[{"kind":"id","name":"Foo"},{"kind":"string","string":"baz"}]]}"""

  let expected: Expr<TypeExpr> =
    Expr.RecordCons(
      [ !"Bar", Expr.Primitive(PrimitiveValue.Int 1)
        !"Foo", Expr.Primitive(PrimitiveValue.String "baz") ]
    )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.UnionCons json round-trip`` () =
  let json =
    """{"kind":"union-case","union-case":[{"kind":"id","name":"Foo"},{"kind":"int","int":"42"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.UnionCons(!"Foo", Expr.Primitive(PrimitiveValue.Int 42))

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.TupleCons json round-trip`` () =
  let json =
    """{"kind":"tuple-cons","elements":[{"kind":"int","int":"1"},{"kind":"string","string":"two"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.TupleCons(
      [ Expr.Primitive(PrimitiveValue.Int 1)
        Expr.Primitive(PrimitiveValue.String "two") ]
    )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.SumCons json round-trip`` () =
  let json = """{"kind":"sum","case":[3,5,{"kind":"int","int":"42"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.SumCons({ Case = 3; Count = 5 }, Expr.Primitive(PrimitiveValue.Int 42))

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.RecordDes json round-trip`` () =
  let json =
    """{"kind":"record-field-lookup","record-field-lookup":[{"kind":"lookup","name":"myRecord"},{"kind":"id","name":"field"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.RecordDes(Expr.Lookup("myRecord" |> Identifier.LocalScope), !"field")

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.UnionDes json round-trip`` () =
  let json =
    """{"kind":"union-match","union-match":[[{"kind":"id", "name":"Bar"},["y",{"kind":"int","int":"2"}]],[{"kind":"id", "name":"Foo"},["x",{"kind":"int","int":"1"}]]]}"""

  let expected: Expr<TypeExpr> =
    Expr.UnionDes(
      Map.ofList
        [ !"Foo", (Var.Create "x", Expr.Primitive(PrimitiveValue.Int 1))
          !"Bar", (Var.Create "y", Expr.Primitive(PrimitiveValue.Int 2)) ]
    )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.TupleDes json round-trip`` () =
  let json =
    """{"kind":"tuple-des","tuple-des":[{"kind":"lookup","name":"myTuple"},1]}"""

  let expected: Expr<TypeExpr> =
    Expr.TupleDes(Expr.Lookup("myTuple" |> Identifier.LocalScope), { Index = 1 })


  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.SumDes json round-trip`` () =
  let json =
    """{"kind":"sum-des","sum-des":[["a",{"kind":"int","int":"1"}],["b",{"kind":"int","int":"2"}]]}"""

  let expected =
    Expr<TypeExpr>
      .SumDes(
        [ (Var.Create "a", Expr.Primitive(PrimitiveValue.Int 1))
          (Var.Create "b", Expr.Primitive(PrimitiveValue.Int 2)) ]
      )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.If json round-trip`` () =
  let json =
    """{"kind":"if","if":[{"kind":"boolean","boolean":"true"},{"kind":"int","int":"1"},{"kind":"int","int":"2"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.If(
      Expr.Primitive(PrimitiveValue.Bool true),
      Expr.Primitive(PrimitiveValue.Int 1),
      Expr.Primitive(PrimitiveValue.Int 2)
    )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.Primitives json round-trip`` () =
  let json = """{"kind":"int","int":"123"}"""
  let expected: Expr<TypeExpr> = Expr.Primitive(PrimitiveValue.Int 123)

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.Lookup json round-trip`` () =
  let json = """{"kind":"lookup","name":"foo"}"""
  let expected: Expr<TypeExpr> = Expr.Lookup("foo" |> Identifier.LocalScope)

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``
