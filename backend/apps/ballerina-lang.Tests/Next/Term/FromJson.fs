module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.FromJson

open System
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open NUnit.Framework
open FSharp.Data
open Ballerina.Errors
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.Json
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.DSL.Next.Terms.Json.Primitive
open Ballerina.DSL.Next.Terms.Json.Value

[<Test>]
let ``LangNext-FromJson-Primitives parse`` () =
  let testCases =
    [ """{"kind": "int", "int":"123"}""", PrimitiveValue.Int 123
      """{"kind": "decimal", "decimal":"123.456"}""", PrimitiveValue.Decimal 123.456M
      """{"kind": "boolean", "boolean":"true"}""", PrimitiveValue.Bool true
      """{"kind": "guid", "guid":"00000000-0000-0000-0000-000000000001"}""",
      PrimitiveValue.Guid(System.Guid("00000000-0000-0000-0000-000000000001"))
      """{"kind": "string", "string":"hello"}""", PrimitiveValue.String "hello"
      """{"kind": "date", "date":"2023-10-01"}""", PrimitiveValue.Date(System.DateOnly(2023, 10, 1))
      """{"kind": "datetime", "datetime":"2023-10-01T12:00:00Z"}""",
      PrimitiveValue.DateTime(System.DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc))
      """{"kind": "unit"}""", PrimitiveValue.Unit ]

  for (json, expected) in testCases do
    match PrimitiveValue.FromJson(JsonValue.Parse json) with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
    | Sum.Right errors -> Assert.Fail($"Expected {expected}, but got errors: {errors}")


[<Test>]
let ``LangNext-FromJson-Values parse`` () =
  let foo =
    { TypeSymbol.Name = "foo"
      TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000001") }

  let bar =
    { TypeSymbol.Name = "bar"
      TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000002") }

  let testCases: List<string * Value<TypeValue>> =
    [ """{"kind": "var", "name":"myVar"}""", Var.Create "myVar" |> Value.Var
      """{"kind": "int", "int":"123"}""", PrimitiveValue.Int 123 |> Value.Primitive
      """{"kind": "decimal", "decimal":"123.456"}""", PrimitiveValue.Decimal 123.456M |> Value.Primitive
      """{"kind": "boolean", "boolean":"true"}""", PrimitiveValue.Bool true |> Value.Primitive
      """{"kind": "record", "fields":[[{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"kind":"int","int":"42"}], 
        [{"name":"bar","guid":"00000000-0000-0000-0000-000000000002"}, {"kind":"string","string":"baz"}]]}""",
      Value.Record(
        Map.ofList
          [ foo, PrimitiveValue.Int 42 |> Value.Primitive
            bar, PrimitiveValue.String "baz" |> Value.Primitive ]
      )
      """{"kind": "union-case", "union-case": [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"kind":"int","int":"42"}]}""",
      Value.UnionCase(foo, PrimitiveValue.Int 42 |> Value.Primitive)
      """{"kind": "tuple", "elements":[{"kind":"int","int":"1"},{"kind":"string","string":"two"}]}""",
      Value.Tuple(
        [ PrimitiveValue.Int 1 |> Value.Primitive
          PrimitiveValue.String "two" |> Value.Primitive ]
      )
      """{"kind": "sum", "case": [3, {"kind":"int","int":"42"}]}""",
      Value.Sum(3, PrimitiveValue.Int 42 |> Value.Primitive)
      """{"kind": "type-lambda", "type-lambda":[{"name":"T", "kind":{"kind":"star"}}, {"kind":"int","int":"42"}]}""",
      Value.TypeLambda({ Name = "T"; Kind = Kind.Star }, PrimitiveValue.Int 42 |> Expr.Primitive)
      """{"kind": "lambda", "lambda": ["x", {"kind":"int","int":"42"}]}""",
      Value.Lambda(Var.Create "x", PrimitiveValue.Int 42 |> Expr.Primitive) ]

  for (json, expected) in testCases do
    match Value<TypeValue>.FromJson(JsonValue.Parse json) |> Reader.Run TypeValue.FromJson with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
    | Sum.Right errors -> Assert.Fail($"Expected {expected}, but got errors: {errors}")





let parseExpr (json: string) =
  JsonValue.Parse json |> Expr<TypeExpr>.FromJson |> Reader.Run TypeExpr.FromJson

[<Test>]
let ``LangNext-FromJson-Expr - Lambda parses correctly`` () =
  let json = """{"kind":"lambda","lambda":["x",{"kind":"int","int":"42"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.Lambda(Var.Create "x", Expr.Primitive(PrimitiveValue.Int 42))

  match parseExpr json with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - TypeLambda parses correctly`` () =
  let json =
    """{"kind":"type-lambda","type-lambda":[{"name":"T","kind":{"kind":"star"}},{"kind":"int","int":"42"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.TypeLambda(TypeParameter.Create("T", Kind.Star), Expr.Primitive(PrimitiveValue.Int 42))

  match parseExpr json with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - TypeApply parses correctly`` () =
  let json =
    """{"kind":"type-apply","type-apply":[{"kind":"lookup","name":"f"}, {"kind":"int"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.TypeApply(Expr.Lookup "f", TypeExpr.Primitive PrimitiveType.Int)

  match parseExpr json with
  | Sum.Left(actual) -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - Apply parses correctly`` () =
  let json =
    """{"kind":"apply","apply":[{"kind":"lambda","lambda":["x",{"kind":"int","int":"1"}]}, {"kind":"int","int":"2"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.Apply(Expr.Lambda(Var.Create "x", Expr.Primitive(PrimitiveValue.Int 1)), Expr.Primitive(PrimitiveValue.Int 2))

  match parseExpr json with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - Let parses correctly`` () =
  let json =
    """{"kind":"let","let":["y", {"kind":"int","int":"5"}, {"kind":"int","int":"6"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.Let(Var.Create "y", Expr.Primitive(PrimitiveValue.Int 5), Expr.Primitive(PrimitiveValue.Int 6))

  match parseExpr json with
  | Sum.Left(actual) -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - TypeLet parses correctly`` () =
  let json =
    """{"kind":"type-let","type-let":["T", {"kind":"int"}, {"kind":"int","int":"7"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.TypeLet(TypeIdentifier.Create "T", TypeExpr.Primitive PrimitiveType.Int, Expr.Primitive(PrimitiveValue.Int 7))

  match parseExpr json with
  | Sum.Left(actual) -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - RecordCons parses correctly`` () =
  let json =
    """{"kind":"record-cons","fields":[["foo",{"kind":"int","int":"1"}],["bar",{"kind":"string","string":"baz"}]]}"""

  let expected: Expr<TypeExpr> =
    Expr.RecordCons(
      [ "foo", Expr.Primitive(PrimitiveValue.Int 1)
        "bar", Expr.Primitive(PrimitiveValue.String "baz") ]
    )

  match parseExpr json with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - UnionCons parses correctly`` () =
  let json =
    """{"kind":"union-case","union-case":["Foo",{"kind":"int","int":"42"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.UnionCons("Foo", Expr.Primitive(PrimitiveValue.Int 42))

  match parseExpr json with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - TupleCons parses correctly`` () =
  let json =
    """{"kind":"tuple-cons","elements":[{"kind":"int","int":"1"},{"kind":"string","string":"two"}]}"""

  let expected: Expr<TypeExpr> =
    Expr.TupleCons(
      [ Expr.Primitive(PrimitiveValue.Int 1)
        Expr.Primitive(PrimitiveValue.String "two") ]
    )

  match parseExpr json with
  | Sum.Left(actual) -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr-SumCons parses correctly`` () =
  let json = """{"kind":"sum","case":[3,5,{"kind":"int","int":"42"}]}"""

  match parseExpr json with
  | Sum.Left(Expr.SumCons({ Case = 3; Count = 5 }, Expr.Primitive(PrimitiveValue.Int 42))) -> Assert.Pass()
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - RecordDes parses correctly`` () =
  let json =
    """{"kind":"record-field-lookup","record-field-lookup":[{"kind":"lookup","name":"myRecord"},"field"]}"""

  let expected: Expr<TypeExpr> = Expr.RecordDes(Expr.Lookup "myRecord", "field")

  match parseExpr json with
  | Sum.Left(actual) -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr-UnionDes parses correctly`` () =
  let json =
    """{"kind":"union-match","union-match":[["Foo",["x",{"kind":"int","int":"1"}]],["Bar",["y",{"kind":"int","int":"2"}]]]}"""

  let expected: Expr<TypeExpr> =
    Expr.UnionDes(
      Map.ofList
        [ "Foo", (Var.Create "x", Expr.Primitive(PrimitiveValue.Int 1))
          "Bar", (Var.Create "y", Expr.Primitive(PrimitiveValue.Int 2)) ]
    )

  match parseExpr json with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - TupleDes parses correctly`` () =
  let json =
    """{"kind":"tuple-des","tuple-des":[{"kind":"lookup","name":"myTuple"},1,3]}"""

  let expected: Expr<TypeExpr> =
    Expr.TupleDes(Expr.Lookup "myTuple", { Index = 1; Count = 3 })


  match parseExpr json with
  | Sum.Left(actual) -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - SumDes parses correctly`` () =
  let json =
    """{"kind":"sum-des","sum-des":[[0,["a",{"kind":"int","int":"1"}]],[1,["b",{"kind":"int","int":"2"}]]]}"""

  let expected =
    Expr<TypeExpr>
      .SumDes(
        Map.ofList
          [ 0, (Var.Create "a", Expr.Primitive(PrimitiveValue.Int 1))
            1, (Var.Create "b", Expr.Primitive(PrimitiveValue.Int 2)) ]
      )

  match parseExpr json with
  | Sum.Left(actual) -> Assert.That(actual, Is.EqualTo expected)
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - If parses correctly`` () =
  let json =
    """{"kind":"if","if":[{"kind":"boolean","boolean":"true"},{"kind":"int","int":"1"},{"kind":"int","int":"2"}]}"""

  match parseExpr json with
  | Sum.Left(Expr.If(Expr.Primitive(PrimitiveValue.Bool true),
                     Expr.Primitive(PrimitiveValue.Int 1),
                     Expr.Primitive(PrimitiveValue.Int 2))) -> Assert.Pass()
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - Primitive parses correctly`` () =
  let json = """{"kind":"int","int":"123"}"""

  match parseExpr json with
  | Sum.Left(Expr.Primitive(PrimitiveValue.Int 123)) -> Assert.Pass()
  | other -> Assert.Fail $"Unexpected result: {other}"

[<Test>]
let ``LangNext-FromJson-Expr - Lookup parses correctly`` () =
  let json = """{"kind":"lookup","name":"foo"}"""

  match parseExpr json with
  | Sum.Left(Expr.Lookup "foo") -> Assert.Pass()
  | other -> Assert.Fail $"Unexpected result: {other}"
