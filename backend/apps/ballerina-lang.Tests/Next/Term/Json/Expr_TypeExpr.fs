module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Json.Expr_TypeExpr

open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open NUnit.Framework
open FSharp.Data
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.Json
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.DSL.Next.Terms.Json

let private (!) = Identifier.LocalScope

let ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``
  (expression: Expr<TypeExpr, Identifier>)
  (expectedJson: JsonValue)
  =
  let toJson = Expr.ToJson >> Reader.Run(TypeExpr.ToJson, Identifier.ToJson)

  match toJson expression with
  | Right err -> Assert.Fail $"Encode failed: {err}"
  | Left json ->
    let toStr (j: JsonValue) =
      j.ToString(JsonSaveOptions.DisableFormatting)

    Assert.That(toStr json, Is.EqualTo(toStr expectedJson))

    let parser = Expr.FromJson >> Reader.Run(TypeExpr.FromJson, Identifier.FromJson)

    let parsed = parser expectedJson

    match parsed with
    | Right err -> Assert.Fail $"Parse failed: {err}"
    | Left result -> Assert.That(result, Is.EqualTo(expression))

[<Test>]
let ``Dsl:Terms:Expr.Lambda json round-trip`` () =
  let json =
    """{"discriminator":"lambda","value":["x",{"discriminator":"int32","value":"42"}]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.Lambda(Var.Create "x", None, Expr.Primitive(PrimitiveValue.Int32 42))

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.TypeLambda json round-trip`` () =
  let json =
    """{"discriminator":"type-lambda","value":[{"name":"T","kind":{"discriminator":"star"}},{"discriminator":"int32","value":"42"}]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.TypeLambda(TypeParameter.Create("T", Kind.Star), Expr.Primitive(PrimitiveValue.Int32 42))

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ```Dsl:Terms:Expr.TypeApply json round-trip`` () =
  let json =
    """{"discriminator":"type-apply","value":[{"discriminator":"lookup","value":{"discriminator":"id","value":"f"}}, {"discriminator":"int32"}]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.TypeApply(Expr.Lookup(!"f"), TypeExpr.Primitive PrimitiveType.Int32)

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.Apply json round-trip`` () =
  let json =
    """{"discriminator":"apply","value":[{"discriminator":"lambda","value":["x",{"discriminator":"int32","value":"1"}]}, {"discriminator":"int32","value":"2"}]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.Apply(
      Expr.Lambda(Var.Create "x", None, Expr.Primitive(PrimitiveValue.Int32 1)),
      Expr.Primitive(PrimitiveValue.Int32 2)
    )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.Let json round-trip`` () =
  let json =
    """{"discriminator":"let","value":["y", {"discriminator":"int32","value":"5"}, {"discriminator":"int32","value":"6"}]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.Let(Var.Create "y", None, Expr.Primitive(PrimitiveValue.Int32 5), Expr.Primitive(PrimitiveValue.Int32 6))

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.TypeLet json round-trip`` () =
  let json =
    """{"discriminator":"type-let","value":["T", {"discriminator":"int32"}, {"discriminator":"int32","value":"7"}]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.TypeLet("T", TypeExpr.Primitive PrimitiveType.Int32, Expr.Primitive(PrimitiveValue.Int32 7))

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.RecordCons json round-trip`` () =
  let json =
    """{"discriminator":"record-cons","value":[[{"discriminator":"id","value":"Bar"},{"discriminator":"int32","value":"1"}],[{"discriminator":"id","value":"Foo"},{"discriminator":"string","value":"baz"}]]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.RecordCons(
      [ !"Bar", Expr.Primitive(PrimitiveValue.Int32 1)
        !"Foo", Expr.Primitive(PrimitiveValue.String "baz") ]
    )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.TupleCons json round-trip`` () =
  let json =
    """{"discriminator":"tuple-cons","value":[{"discriminator":"int32","value":"1"},{"discriminator":"string","value":"two"}]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.TupleCons(
      [ Expr.Primitive(PrimitiveValue.Int32 1)
        Expr.Primitive(PrimitiveValue.String "two") ]
    )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.SumCons json round-trip`` () =
  let json = """{"discriminator":"sum","value":[3,5]}"""

  let expected: Expr<TypeExpr, Identifier> = Expr.SumCons({ Case = 3; Count = 5 })

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.RecordDes json round-trip`` () =
  let json =
    """{"discriminator":"record-field-lookup","value":[{"discriminator":"lookup","value":{"discriminator":"id","value":"myRecord"}},{"discriminator":"id","value":"field"}]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.RecordDes(Expr.Lookup("myRecord" |> Identifier.LocalScope), !"field")

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.UnionDes json round-trip`` () =
  let json =
    """{"discriminator":"union-match","value":[[{"discriminator":"id", "value":"Bar"},["y",{"discriminator":"int32","value":"2"}]],[{"discriminator":"id", "value":"Foo"},["x",{"discriminator":"int32","value":"1"}]]]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.UnionDes(
      Map.ofList
        [ !"Foo", (Var.Create "x", Expr.Primitive(PrimitiveValue.Int32 1))
          !"Bar", (Var.Create "y", Expr.Primitive(PrimitiveValue.Int32 2)) ],
      None
    )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.TupleDes json round-trip`` () =
  let json =
    """{"discriminator":"tuple-des","value":[{"discriminator":"lookup","value":{"discriminator":"id","value":"myTuple"}},1]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.TupleDes(Expr.Lookup("myTuple" |> Identifier.LocalScope), { Index = 1 })


  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.SumDes json round-trip`` () =
  let json =
    """{"discriminator":"sum-des","value":[[1,2,"a",{"discriminator":"int32","value":"1"}],[2,2,"b",{"discriminator":"int32","value":"2"}]]}"""

  let expected =
    Expr<TypeExpr, Identifier>
      .SumDes(
        [ { Case = 1; Count = 2 }, (Var.Create "a", Expr.Primitive(PrimitiveValue.Int32 1))
          { Case = 2; Count = 2 }, (Var.Create "b", Expr.Primitive(PrimitiveValue.Int32 2)) ]
        |> Map.ofList
      )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.If json round-trip`` () =
  let json =
    """{"discriminator":"if","value":[{"discriminator":"boolean","value":"true"},{"discriminator":"int32","value":"1"},{"discriminator":"int32","value":"2"}]}"""

  let expected: Expr<TypeExpr, Identifier> =
    Expr.If(
      Expr.Primitive(PrimitiveValue.Bool true),
      Expr.Primitive(PrimitiveValue.Int32 1),
      Expr.Primitive(PrimitiveValue.Int32 2)
    )

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.Primitives json round-trip`` () =
  let json = """{"discriminator":"int32","value":"123"}"""
  let expected: Expr<TypeExpr, Identifier> = Expr.Primitive(PrimitiveValue.Int32 123)

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``

[<Test>]
let ``Dsl:Terms:Expr.Lookup json round-trip`` () =
  let json =
    """{"discriminator":"lookup","value":{"discriminator":"id","value":"foo"}}"""

  let expected: Expr<TypeExpr, Identifier> = Expr.Lookup(!"foo")

  (expected, JsonValue.Parse json)
  ||> ``Assert Expr<TypeExpr> -> ToJson -> FromJson -> Expr<TypeExpr>``
