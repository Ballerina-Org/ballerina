module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Eval

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.Eval
open Ballerina.State.WithError
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Patterns
open Ballerina.StdLib.OrderPreservingMap
open Ballerina.LocalizedErrors

[<Test>]
let ``LangNext-TypeEval lookup looks up existing types`` () =
  let t1 = TypeValue.CreateString()

  let actual =
    TypeExpr.Lookup(Identifier.LocalScope "T1")
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      TypeExprEvalState.Create(
        [ "T1" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve, (t1, Kind.Star) ]
        |> Map.ofSeq,
        TypeExprEvalSymbols.Empty
      )
    )

  match actual with
  | Sum.Left((actual, _), _) ->
    match actual with
    | TypeValue.Primitive { value = PrimitiveType.String } -> Assert.Pass()
    | _ -> Assert.Fail $"Expected 'string' but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of anonymous unions`` () =
  let A = TypeSymbol.Create("A" |> Identifier.LocalScope)
  let B = TypeSymbol.Create("B" |> Identifier.LocalScope)
  let C = TypeSymbol.Create("C" |> Identifier.LocalScope)
  let D = TypeSymbol.Create("D" |> Identifier.LocalScope)

  let t1 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 =
    TypeExpr.Union(
      [ "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal
        "D" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Bool ]
    )

  let actual =
    TypeExpr.Flatten(t1, t2)
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      [ A.Name |> TypeCheckScope.Empty.Resolve, A
        B.Name |> TypeCheckScope.Empty.Resolve, B
        C.Name |> TypeCheckScope.Empty.Resolve, C
        D.Name |> TypeCheckScope.Empty.Resolve, D ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromTypeSymbols
    )

  match actual with
  | Sum.Left(actual, _) ->
    match actual with
    | TypeValue.Union(cases), Kind.Star ->
      match cases.value |> OrderedMap.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ Identifier.LocalScope "C", TypeValue.Primitive { value = PrimitiveType.Decimal }
          Identifier.LocalScope "D", TypeValue.Primitive { value = PrimitiveType.Bool }
          Identifier.LocalScope "A", TypeValue.Primitive { value = PrimitiveType.Int32 }
          Identifier.LocalScope "B", TypeValue.Primitive { value = PrimitiveType.String } ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {cases}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of named unions`` () =
  let A = TypeSymbol.Create("A" |> Identifier.LocalScope)
  let B = TypeSymbol.Create("B" |> Identifier.LocalScope)
  let C = TypeSymbol.Create("C" |> Identifier.LocalScope)
  let D = TypeSymbol.Create("D" |> Identifier.LocalScope)

  let t1 =
    TypeValue.CreateUnion(OrderedMap.ofList [ A, TypeValue.CreateInt32(); B, TypeValue.CreateString() ])

  let t2 =
    TypeValue.CreateUnion(OrderedMap.ofList [ C, TypeValue.CreateDecimal(); D, TypeValue.CreateBool() ])

  let actual =
    TypeExpr.Flatten(TypeExpr.Lookup(Identifier.LocalScope "T1"), TypeExpr.Lookup(Identifier.LocalScope "T2"))
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      TypeExprEvalState.Create(
        [ "T1" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve, (t1, Kind.Star)
          "T2" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve, (t2, Kind.Star) ]
        |> Map.ofSeq,
        [ A.Name |> TypeCheckScope.Empty.Resolve, A
          B.Name |> TypeCheckScope.Empty.Resolve, B
          C.Name |> TypeCheckScope.Empty.Resolve, C
          D.Name |> TypeCheckScope.Empty.Resolve, D ]
        |> Map.ofList
        |> TypeExprEvalSymbols.CreateFromTypeSymbols
      )
    )

  match actual with
  | Sum.Left((actual, _), _) ->
    match actual with
    | TypeValue.Union(cases) ->
      match cases.value |> OrderedMap.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ (Identifier.LocalScope "C", TypeValue.Primitive { value = PrimitiveType.Decimal })
          (Identifier.LocalScope "D", TypeValue.Primitive { value = PrimitiveType.Bool })
          (Identifier.LocalScope "A", TypeValue.Primitive { value = PrimitiveType.Int32 })
          (Identifier.LocalScope "B", TypeValue.Primitive { value = PrimitiveType.String }) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {cases}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of named records`` () =
  let A = TypeSymbol.Create("A" |> Identifier.LocalScope)
  let B = TypeSymbol.Create("B" |> Identifier.LocalScope)
  let C = TypeSymbol.Create("C" |> Identifier.LocalScope)
  let D = TypeSymbol.Create("D" |> Identifier.LocalScope)

  let t1 =
    TypeValue.CreateRecord(OrderedMap.ofList [ A, TypeValue.CreateInt32(); B, TypeValue.CreateString() ])

  let t2 =
    TypeValue.CreateRecord(OrderedMap.ofList [ C, TypeValue.CreateDecimal(); D, TypeValue.CreateBool() ])

  let actual =
    TypeExpr.Flatten(TypeExpr.Lookup(Identifier.LocalScope "T1"), TypeExpr.Lookup(Identifier.LocalScope "T2"))
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      TypeExprEvalState.Create(
        [ "T1" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve, (t1, Kind.Star)
          "T2" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve, (t2, Kind.Star) ]
        |> Map.ofSeq,
        [ A.Name |> TypeCheckScope.Empty.Resolve, A
          B.Name |> TypeCheckScope.Empty.Resolve, B
          C.Name |> TypeCheckScope.Empty.Resolve, C
          D.Name |> TypeCheckScope.Empty.Resolve, D ]
        |> Map.ofList
        |> TypeExprEvalSymbols.CreateFromTypeSymbols
      )
    )

  match actual with
  | Sum.Left((actual, _), _) ->
    match actual with
    | TypeValue.Record(fields) ->
      match fields.value |> OrderedMap.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ (Identifier.LocalScope "C", TypeValue.Primitive { value = PrimitiveType.Decimal })
          (Identifier.LocalScope "D", TypeValue.Primitive { value = PrimitiveType.Bool })
          (Identifier.LocalScope "A", TypeValue.Primitive { value = PrimitiveType.Int32 })
          (Identifier.LocalScope "B", TypeValue.Primitive { value = PrimitiveType.String }) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {fields}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of incompatible types fails`` () =
  let A = TypeSymbol.Create("A" |> Identifier.LocalScope)
  let B = TypeSymbol.Create("B" |> Identifier.LocalScope)

  let t1 =
    TypeExpr.Union(
      [ TypeExpr.Lookup("A" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32
        TypeExpr.Lookup("B" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 = TypeExpr.Primitive PrimitiveType.Decimal

  let actual =
    TypeExpr.Flatten(t1, t2)
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      [ A.Name |> TypeCheckScope.Empty.Resolve, A
        B.Name |> TypeCheckScope.Empty.Resolve, B ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromTypeSymbols
    )

  match actual with
  | Sum.Right _err -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"

[<Test>]
let ``LangNext-TypeEval Keyof extracts record keys`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")

  let t1 =
    TypeExpr.Record(
      [ TypeExpr.Lookup("A" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32
        TypeExpr.Lookup("B" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.String
        TypeExpr.Lookup("C" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let actual =
    t1
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      [ A.Name |> TypeCheckScope.Empty.Resolve, A
        B.Name |> TypeCheckScope.Empty.Resolve, B
        C.Name |> TypeCheckScope.Empty.Resolve, C ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromTypeSymbols
    )

  let expected =
    TypeValue.Union
      { value =
          OrderedMap.ofList
            [ A, TypeValue.CreateUnit()
              B, TypeValue.CreateUnit()
              C, TypeValue.CreateUnit() ]
        source = TypeExprSourceMapping.OriginTypeExpr t1 }

  match actual with
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval flatten of Keyofs`` () =
  let A = TypeSymbol.Create("A" |> Identifier.LocalScope)
  let B = TypeSymbol.Create("B" |> Identifier.LocalScope)
  let C = TypeSymbol.Create("C" |> Identifier.LocalScope)
  let D = TypeSymbol.Create("D" |> Identifier.LocalScope)
  let E = TypeSymbol.Create("E" |> Identifier.LocalScope)

  let t1 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let t2 =
    TypeExpr.Record(
      [ "D" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "E" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )
    |> TypeExpr.KeyOf

  let t3 = TypeExpr.Flatten(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      [ A.Name |> TypeCheckScope.Empty.Resolve, A
        B.Name |> TypeCheckScope.Empty.Resolve, B
        C.Name |> TypeCheckScope.Empty.Resolve, C
        D.Name |> TypeCheckScope.Empty.Resolve, D
        E.Name |> TypeCheckScope.Empty.Resolve, E ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromTypeSymbols
    )

  match actual with
  | Sum.Left((actual, _), _) ->
    match actual with
    | TypeValue.Union(cases) ->
      match cases.value |> OrderedMap.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ (Identifier.LocalScope "D", TypeValue.Primitive { value = PrimitiveType.Unit })
          (Identifier.LocalScope "E", TypeValue.Primitive { value = PrimitiveType.Unit })
          (Identifier.LocalScope "A", TypeValue.Primitive { value = PrimitiveType.Unit })
          (Identifier.LocalScope "B", TypeValue.Primitive { value = PrimitiveType.Unit })
          (Identifier.LocalScope "C", TypeValue.Primitive { value = PrimitiveType.Unit })

        ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {cases}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Keyofs`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")
  let X = TypeSymbol.Create(Identifier.LocalScope "X")

  let t1 =
    TypeExpr.Record(
      [ A.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        B.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        C.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let t2 =
    TypeExpr.Record(
      [ A.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        X.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )
    |> TypeExpr.KeyOf

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      [ A.Name |> TypeCheckScope.Empty.Resolve, A
        B.Name |> TypeCheckScope.Empty.Resolve, B
        C.Name |> TypeCheckScope.Empty.Resolve, C
        X.Name |> TypeCheckScope.Empty.Resolve, X ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromTypeSymbols
    )

  let expected =
    TypeValue.Union
      { value = OrderedMap.ofList [ B, TypeValue.CreateUnit(); C, TypeValue.CreateUnit() ]
        source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Exclude(t1, t2)) }

  match actual with
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Records`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")
  let X = TypeSymbol.Create(Identifier.LocalScope "X")

  let t1 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "X" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      [ A.Name |> TypeCheckScope.Empty.Resolve, A
        B.Name |> TypeCheckScope.Empty.Resolve, B
        C.Name |> TypeCheckScope.Empty.Resolve, C
        X.Name |> TypeCheckScope.Empty.Resolve, X ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromTypeSymbols
    )

  let expected =
    TypeValue.Record
      { value =
          OrderedMap.ofList
            [ B, TypeValue.PrimitiveWithTrivialSource PrimitiveType.String
              C, TypeValue.PrimitiveWithTrivialSource PrimitiveType.Decimal ]
        source = TypeExprSourceMapping.OriginTypeExpr t3 }

  match actual with
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Unions`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")
  let X = TypeSymbol.Create(Identifier.LocalScope "X")

  let t1 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "X" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      [ A.Name |> TypeCheckScope.Empty.Resolve, A
        B.Name |> TypeCheckScope.Empty.Resolve, B
        C.Name |> TypeCheckScope.Empty.Resolve, C
        X.Name |> TypeCheckScope.Empty.Resolve, X ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromTypeSymbols
    )

  (*

n =
  OriginTypeExpr
    (Exclude
      (Union
          [(Lookup (LocalScope "A"), Primitive Int32);
          (Lookup (LocalScope "B"), Primitive String);
          (Lookup (LocalScope "C"), Primitive Decimal)],
        Union
          [(Lookup (LocalScope "A"), Primitive Int32);
          (Lookup (LocalScope "X"), Primitive String)])) }>  

  *)

  let expected =
    TypeValue.Union
      { value =
          OrderedMap.ofList
            [ B, TypeValue.PrimitiveWithTrivialSource PrimitiveType.String
              C, TypeValue.PrimitiveWithTrivialSource PrimitiveType.Decimal ]
        source = TypeExprSourceMapping.OriginTypeExpr t3 }

  match actual with
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude fails on incompatible types`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")
  let X = TypeSymbol.Create(Identifier.LocalScope "X")

  let t1 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "X" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      [ A.Name |> TypeCheckScope.Empty.Resolve, A
        B.Name |> TypeCheckScope.Empty.Resolve, B
        C.Name |> TypeCheckScope.Empty.Resolve, C
        X.Name |> TypeCheckScope.Empty.Resolve, X ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromTypeSymbols
    )

  match actual with
  | Sum.Left((actual, _), _) -> Assert.Fail $"Expected failure but got result: {actual}"
  | Sum.Right _err -> Assert.Pass()

[<Test>]
let ``LangNext-TypeEval Rotate from union to record`` () =
  let t =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.Rotate

  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")

  let actual =
    t
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      [ A.Name |> TypeCheckScope.Empty.Resolve, A
        B.Name |> TypeCheckScope.Empty.Resolve, B
        C.Name |> TypeCheckScope.Empty.Resolve, C ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromTypeSymbols
    )

  let expected =
    TypeValue.Record
      { value =
          OrderedMap.ofList
            [ A, TypeValue.PrimitiveWithTrivialSource PrimitiveType.Int32
              B, TypeValue.PrimitiveWithTrivialSource PrimitiveType.String
              C, TypeValue.PrimitiveWithTrivialSource PrimitiveType.Decimal ]
        source = TypeExprSourceMapping.OriginTypeExpr t }

  match actual with
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Rotate from record to union`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")

  let t =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.Rotate

  let actual =
    t
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      [ A.Name |> TypeCheckScope.Empty.Resolve, A
        B.Name |> TypeCheckScope.Empty.Resolve, B
        C.Name |> TypeCheckScope.Empty.Resolve, C ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromTypeSymbols
    )

  let expected =
    TypeValue.Union
      { value =
          OrderedMap.ofList
            [ A, TypeValue.PrimitiveWithTrivialSource PrimitiveType.Int32
              B, TypeValue.PrimitiveWithTrivialSource PrimitiveType.String
              C, TypeValue.PrimitiveWithTrivialSource PrimitiveType.Decimal ]
        source = TypeExprSourceMapping.OriginTypeExpr t }

  match actual with
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval (generic) Apply`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Star),
        TypeExpr.Tuple(
          [ TypeExpr.Primitive PrimitiveType.Int32
            TypeExpr.Lookup(Identifier.LocalScope "a") ]
        )
      ),
      TypeExpr.Primitive PrimitiveType.String
    )

  let actual =
    t
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(TypeExprEvalContext.Empty("", ""), TypeExprEvalState.Empty)

  let expected =
    TypeValue.Tuple
      { value =
          [ TypeValue.PrimitiveWithTrivialSource PrimitiveType.Int32
            TypeValue.PrimitiveWithTrivialSource PrimitiveType.String ]
        source =
          TypeExprSourceMapping.OriginTypeExpr(
            TypeExpr.Apply(
              TypeExpr.Lambda(
                TypeParameter.Create("a", Kind.Star),
                TypeExpr.Tuple
                  [ TypeExpr.Primitive PrimitiveType.Int32
                    TypeExpr.Lookup(Identifier.LocalScope "a") ]
              ),
              TypeExpr.Primitive PrimitiveType.String
            )
          ) }

  match actual with
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

// [<Test>]
// let ``LangNext-TypeEval (generic) Apply over symbol`` () =
//   let t =
//     TypeExpr.Apply(
//       TypeExpr.Lambda(
//         TypeParameter.Create("a", Kind.Symbol),
//         TypeExpr.Record([ TypeExpr.Lookup(Identifier.LocalScope "a"), TypeExpr.Primitive PrimitiveType.Int32 ])
//       ),
//       TypeExpr.Lookup(Identifier.LocalScope "Value")
//     )

//   let Value = TypeSymbol.Create(Identifier.LocalScope "Value"

//   let actual =
//     t
//     |> TypeExpr.Eval
//     |> State.Run(
//       TypeExprEvalContext.Empty("", ""),
//       TypeExprEvalState.Create(Map.empty, [ Value.Name, Value ] |> Map.ofList)
//     )

//   let expected =
//     TypeValCons.Record([ Value, TypeValCons.Primitive PrimitiveType.Int32 ] |> Map.ofList)

//   match actual with
//   | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
//   | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval (generic) Apply of type instead of symbol fails`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Symbol),
        TypeExpr.Record([ TypeExpr.Lookup(Identifier.LocalScope "a"), TypeExpr.Primitive PrimitiveType.Int32 ])
      ),
      TypeExpr.Primitive PrimitiveType.String
    )

  let actual =
    t
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      TypeExprEvalState.Create(
        Map.empty,
        [ "Value" ]
        |> Seq.map (fun s ->
          s |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve, s |> Identifier.LocalScope |> TypeSymbol.Create)
        |> Map.ofSeq
        |> TypeExprEvalSymbols.CreateFromTypeSymbols
      )
    )

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"


[<Test>]
let ``LangNext-TypeEval (generic) Apply of symbol instead of type fails`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Star),
        TypeExpr.Tuple(
          [ TypeExpr.Lookup(Identifier.LocalScope "a")
            TypeExpr.Primitive PrimitiveType.Int32 ]
        )
      ),
      TypeExpr.Lookup(Identifier.LocalScope "Value")
    )

  let actual =
    t
    |> TypeExpr.Eval None Location.Unknown
    |> State.Run(
      TypeExprEvalContext.Empty("", ""),
      TypeExprEvalState.Create(
        Map.empty,
        [ "Value" ]
        |> Seq.map (fun s ->
          s |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve, s |> Identifier.LocalScope |> TypeSymbol.Create)
        |> Map.ofSeq
        |> TypeExprEvalSymbols.CreateFromTypeSymbols
      )
    )

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"
