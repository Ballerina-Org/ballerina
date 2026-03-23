module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Extraction.Expr

open System
open NUnit.Framework
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.DSL.Next.Types.TypeChecker.Eval
open Ballerina.DSL.Next.Types.TypeChecker.TypeExtraction
open Ballerina.DSL.Next.Types.TypeChecker.TypeExtractionExpr
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.StdLib
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.Reader.WithError
open Ballerina.Collections.Sum
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.StdLib.MutableMemoryDB
open Ballerina.Cat.Collections.OrderedMap

let private scope = TypeCheckScope.Empty

let private resolveId name =
  Identifier.LocalScope name |> scope.Resolve

let private makeSym name =
  TypeSymbol.Create(Identifier.LocalScope name)

let private listOps =
  { Nil = resolveId "List.Nil"
    Cons = resolveId "List.Cons"
    Append = resolveId "List.append"
    Fold = resolveId "List.fold" }

let private mapOps = { MapToList = resolveId "Map.mapToList" }

type private RuntimeExt = ValueExt<unit, MutableMemoryDB<unit, unit>, unit>

let private resolveFqId (modulePath: string list) (name: string) =
  Identifier.FullyQualified(modulePath, name) |> scope.Resolve

let private runtimeListOps =
  { Nil = resolveFqId [ "List" ] "Nil"
    Cons = resolveFqId [ "List" ] "Cons"
    Append = resolveFqId [ "List" ] "append"
    Fold = resolveFqId [ "List" ] "fold" }

let private runtimeMapOps = { MapToList = resolveFqId [ "Map" ] "mapToList" }

let private runtimeOps, runtimeContext, _db_query_sym, _make_db_query_type =
  db_ops ()
  |> stdExtensions (Ballerina.DSL.Next.StdLib.String.Extension.StringTypeClass<_>.Console())

do ignore runtimeOps

let private evalTyped (program: Expr<TypeValue<RuntimeExt>, ResolvedIdentifier, RuntimeExt>) =
  let evalContext = ExprEvalContext.Empty() |> runtimeContext.ExprEvalContext

  Expr.Eval(NonEmptyList.prependList runtimeContext.TypeCheckedPreludes (NonEmptyList.One program))
  |> Reader.Run evalContext

let private assertEvalInt (expected: int) (program: Expr<TypeValue<RuntimeExt>, ResolvedIdentifier, RuntimeExt>) =
  match evalTyped program with
  | Left(Value.Primitive(PrimitiveValue.Int32 actual)) -> Assert.That(actual, Is.EqualTo(expected))
  | Left other -> Assert.Fail $"Expected int32 {expected}, got {other}"
  | Right err -> Assert.Fail $"Evaluation failed: {err}"

let private treeFromSteps (paths: ExtractionStep list list) : ExtractionTree =
  let rec add (steps: ExtractionStep list) (tree: ExtractionTree) =
    match steps with
    | [] -> { tree with SelfMatch = true }
    | step :: rest ->
      let child =
        tree.Children |> Map.tryFind step |> Option.defaultValue emptyExtractionTree

      let child' = add rest child

      { tree with
          Children = tree.Children |> Map.add step child' }

  paths |> List.fold (fun acc steps -> add steps acc) emptyExtractionTree

[<Test>]
let ``buildExtractionExpr with empty tree returns lambda wrapping Nil`` () =
  let expr =
    buildExtractionExpr<IComparable> listOps mapOps Map.empty emptyExtractionTree

  match expr.Expr with
  | ExprRec.Lambda { Body = body } ->
    match body.Expr with
    | ExprRec.Apply { F = nilLookup; Arg = unitArg } ->
      match nilLookup.Expr with
      | ExprRec.Lookup { Id = id } -> Assert.That(id, Is.EqualTo(listOps.Nil))
      | other -> Assert.Fail $"Expected Nil lookup, got {other}"

      match unitArg.Expr with
      | ExprRec.Primitive PrimitiveValue.Unit -> Assert.Pass()
      | other -> Assert.Fail $"Expected Unit primitive arg for Nil, got {other}"
    | other -> Assert.Fail $"Expected Apply(Nil, Unit) in body, got {other}"
  | other -> Assert.Fail $"Expected Lambda, got {other}"

[<Test>]
let ``buildExtractionExpr with single RecordField branch generates RecordDes wrapped in Cons`` () =
  let fieldSym = makeSym "evidence"
  let fieldResolved = resolveId "evidence"

  let symbolResolver = Map.ofList [ fieldSym, fieldResolved ]
  let tree = treeFromSteps [ [ RecordField fieldSym ] ]

  let expr = buildExtractionExpr<IComparable> listOps mapOps symbolResolver tree

  match expr.Expr with
  | ExprRec.Lambda { Body = body } ->
    match body.Expr with
    | ExprRec.Apply { F = consLookup; Arg = tupleArg } ->
      match consLookup.Expr with
      | ExprRec.Lookup { Id = id } -> Assert.That(id, Is.EqualTo(listOps.Cons))
      | other -> Assert.Fail $"Expected Cons lookup, got {other}"

      match tupleArg.Expr with
      | ExprRec.TupleCons { Items = items } ->
        match items with
        | [ recordDes; nilExpr ] ->
          match recordDes.Expr with
          | ExprRec.RecordDes { Field = field } -> Assert.That(field, Is.EqualTo(fieldResolved))
          | other -> Assert.Fail $"Expected RecordDes, got {other}"

          match nilExpr.Expr with
          | ExprRec.Apply { F = nilLookup; Arg = unitArg } ->
            match nilLookup.Expr with
            | ExprRec.Lookup { Id = id } -> Assert.That(id, Is.EqualTo(listOps.Nil))
            | other -> Assert.Fail $"Expected Nil lookup, got {other}"

            match unitArg.Expr with
            | ExprRec.Primitive PrimitiveValue.Unit -> Assert.Pass()
            | other -> Assert.Fail $"Expected Unit primitive arg for Nil, got {other}"
          | other -> Assert.Fail $"Expected Apply(Nil, Unit), got {other}"
        | other -> Assert.Fail $"Expected tuple [record; nil], got {other}"
      | other -> Assert.Fail $"Expected TupleCons argument to Cons, got {other}"
    | other -> Assert.Fail $"Expected Apply(Cons, Tuple(record, nil)), got {other}"
  | other -> Assert.Fail $"Expected Lambda, got {other}"

[<Test>]
let ``buildExtractionExpr with ContainerElement self-match returns host directly`` () =
  let tree = treeFromSteps [ [ ContainerElement SetContainer ] ]

  let expr = buildExtractionExpr<IComparable> listOps mapOps Map.empty tree

  match expr.Expr with
  | ExprRec.Lambda { Body = body } ->
    match body.Expr with
    | ExprRec.Lookup _ -> Assert.Pass()
    | other -> Assert.Fail $"Expected Lookup(host) for pass-through, got {other}"
  | other -> Assert.Fail $"Expected Lambda, got {other}"

[<Test>]
let ``buildExtractionExpr with ContainerElement plus RecordField uses Fold`` () =
  let fieldSym = makeSym "evidence"
  let fieldResolved = resolveId "evidence"

  let symbolResolver = Map.ofList [ fieldSym, fieldResolved ]
  let tree = treeFromSteps [ [ ContainerElement SetContainer; RecordField fieldSym ] ]

  let expr = buildExtractionExpr<IComparable> listOps mapOps symbolResolver tree

  match expr.Expr with
  | ExprRec.Lambda { Body = body } ->
    match body.Expr with
    | ExprRec.Apply { F = foldApplied } ->
      match foldApplied.Expr with
      | ExprRec.Apply { F = foldWithInit } ->
        match foldWithInit.Expr with
        | ExprRec.Apply { F = foldLookup } ->
          match foldLookup.Expr with
          | ExprRec.Lookup { Id = id } -> Assert.That(id, Is.EqualTo(listOps.Fold))
          | other -> Assert.Fail $"Expected Fold lookup, got {other}"
        | other -> Assert.Fail $"Expected Apply(Fold, fn), got {other}"
      | other -> Assert.Fail $"Expected Apply(Apply(Fold, fn), init), got {other}"
    | other -> Assert.Fail $"Expected Apply(..., host), got {other}"
  | other -> Assert.Fail $"Expected Lambda, got {other}"

[<Test>]
let ``buildExtractionExpr with Map container uses MapToList before Fold`` () =
  let mapTypeId = resolveId "Map"
  let tree = treeFromSteps [ [ ContainerElement(ImportedContainer(mapTypeId, 1)) ] ]

  let expr = buildExtractionExpr<IComparable> listOps mapOps Map.empty tree

  match expr.Expr with
  | ExprRec.Lambda { Body = body } ->
    match body.Expr with
    | ExprRec.Apply { F = foldApplied; Arg = mappedHost } ->
      match foldApplied.Expr with
      | ExprRec.Apply { F = foldWithInit } ->
        match foldWithInit.Expr with
        | ExprRec.Apply { F = foldLookup } ->
          match foldLookup.Expr with
          | ExprRec.Lookup { Id = id } -> Assert.That(id, Is.EqualTo(listOps.Fold))
          | other -> Assert.Fail $"Expected Fold lookup, got {other}"
        | other -> Assert.Fail $"Expected Apply(Fold, fn), got {other}"
      | other -> Assert.Fail $"Expected Apply(Apply(Fold, fn), init), got {other}"

      match mappedHost.Expr with
      | ExprRec.Apply { F = mapToListLookup } ->
        match mapToListLookup.Expr with
        | ExprRec.Lookup { Id = id } -> Assert.That(id, Is.EqualTo(mapOps.MapToList))
        | other -> Assert.Fail $"Expected MapToList lookup, got {other}"
      | other -> Assert.Fail $"Expected Apply(MapToList, host), got {other}"
    | other -> Assert.Fail $"Expected Apply(Fold..., mappedHost), got {other}"
  | other -> Assert.Fail $"Expected Lambda, got {other}"

[<Test>]
let ``buildExtractionExpr with multiple branches uses Append`` () =
  let sym1 = makeSym "ev1"
  let sym2 = makeSym "ev2"

  let symbolResolver = Map.ofList [ sym1, resolveId "ev1"; sym2, resolveId "ev2" ]
  let tree = treeFromSteps [ [ RecordField sym1 ]; [ RecordField sym2 ] ]

  let expr = buildExtractionExpr<IComparable> listOps mapOps symbolResolver tree

  match expr.Expr with
  | ExprRec.Lambda { Body = body } ->
    match body.Expr with
    | ExprRec.Apply { F = appendApplied } ->
      match appendApplied.Expr with
      | ExprRec.Apply { F = appendLookup } ->
        match appendLookup.Expr with
        | ExprRec.Lookup { Id = id } -> Assert.That(id, Is.EqualTo(listOps.Append))
        | other -> Assert.Fail $"Expected Append lookup, got {other}"
      | other -> Assert.Fail $"Expected Apply(Append, list1), got {other}"
    | other -> Assert.Fail $"Expected Apply(Apply(Append, ...), list2), got {other}"
  | other -> Assert.Fail $"Expected Lambda, got {other}"

[<Test>]
let ``buildExtractionExpr with UnionCase branch generates UnionDes with fallback`` () =
  let caseSym = makeSym "HasEvidence"
  let caseResolved = resolveId "HasEvidence"

  let symbolResolver = Map.ofList [ caseSym, caseResolved ]
  let tree = treeFromSteps [ [ UnionCase caseSym ] ]

  let expr = buildExtractionExpr<IComparable> listOps mapOps symbolResolver tree

  match expr.Expr with
  | ExprRec.Lambda { Body = body } ->
    match body.Expr with
    | ExprRec.Apply { F = unionDes } ->
      match unionDes.Expr with
      | ExprRec.UnionDes { Handlers = handlers
                           Fallback = fallback } ->
        Assert.That(handlers |> Map.containsKey caseResolved, Is.True, "Should have handler for matching case")
        Assert.That(fallback.IsSome, Is.True, "Should have a fallback (Nil)")
      | other -> Assert.Fail $"Expected UnionDes, got {other}"
    | other -> Assert.Fail $"Expected Apply(UnionDes, host), got {other}"
  | other -> Assert.Fail $"Expected Lambda, got {other}"

[<Test>]
let ``buildExtractionExpr fails upfront when symbol resolver is missing entries`` () =
  let fieldSym = makeSym "evidence"

  let tree = treeFromSteps [ [ RecordField fieldSym ] ]

  Assert.Throws<System.Exception>(fun () -> buildExtractionExpr<IComparable> listOps mapOps Map.empty tree |> ignore)
  |> fun ex -> Assert.That(ex.Message, Does.Contain("missing symbol mappings"))

[<Test>]
let ``buildExtractionExpr runtime: extracts single int field to one-element list`` () =
  let evidenceSym = makeSym "evidence"
  let evidenceId = resolveId "evidence"
  let symbolResolver = Map.ofList [ evidenceSym, evidenceId ]
  let tree = treeFromSteps [ [ RecordField evidenceSym ] ]

  let extractor =
    buildExtractionExpr<RuntimeExt> runtimeListOps runtimeMapOps symbolResolver tree

  let hostType =
    TypeValue.CreateRecord(OrderedMap.ofList [ evidenceSym, (TypeValue.CreateInt32(), Kind.Star) ])

  let hostValue: Value<TypeValue<RuntimeExt>, RuntimeExt> =
    Value.Record(Map.ofList [ evidenceId, Value.Primitive(PrimitiveValue.Int32 42) ])

  let extracted =
    Expr.Apply(extractor, Expr.FromValue(hostValue, hostType, Kind.Star))

  let listLength = Expr.Apply(Expr.Lookup(resolveFqId [ "List" ] "length"), extracted)
  assertEvalInt 1 listLength

[<Test>]
let ``buildExtractionExpr runtime: extracts multiple int fields with correct aggregate`` () =
  let ev1Sym = makeSym "ev1"
  let ev2Sym = makeSym "ev2"
  let ev1Id = resolveId "ev1"
  let ev2Id = resolveId "ev2"
  let symbolResolver = Map.ofList [ ev1Sym, ev1Id; ev2Sym, ev2Id ]
  let tree = treeFromSteps [ [ RecordField ev1Sym ]; [ RecordField ev2Sym ] ]

  let extractor =
    buildExtractionExpr<RuntimeExt> runtimeListOps runtimeMapOps symbolResolver tree

  let hostType =
    TypeValue.CreateRecord(
      OrderedMap.ofList
        [ ev1Sym, (TypeValue.CreateInt32(), Kind.Star)
          ev2Sym, (TypeValue.CreateInt32(), Kind.Star) ]
    )

  let hostValue: Value<TypeValue<RuntimeExt>, RuntimeExt> =
    Value.Record(
      Map.ofList
        [ ev1Id, Value.Primitive(PrimitiveValue.Int32 3)
          ev2Id, Value.Primitive(PrimitiveValue.Int32 7) ]
    )

  let extracted =
    Expr.Apply(extractor, Expr.FromValue(hostValue, hostType, Kind.Star))

  let listLength = Expr.Apply(Expr.Lookup(resolveFqId [ "List" ] "length"), extracted)
  assertEvalInt 2 listLength

  let accVar = Var.Create "acc"
  let xVar, xId = Var.Create "x", resolveId "x"
  let foldFnBody = Expr.Lookup xId

  let foldFn =
    Expr.Lambda(accVar, None, Expr.Lambda(xVar, None, foldFnBody, None), None)

  let sumExpr =
    Expr.Apply(
      Expr.Apply(
        Expr.Apply(Expr.Lookup(resolveFqId [ "List" ] "fold"), foldFn),
        Expr.Primitive(PrimitiveValue.Int32 -1)
      ),
      extracted
    )

  assertEvalInt 7 sumExpr
