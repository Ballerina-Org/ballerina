module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Extraction.Tree

open System
open NUnit.Framework
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.DSL.Next.Types.TypeChecker.TypeExtraction
open Ballerina.Cat.Collections.OrderedMap

let private scope = TypeCheckScope.Empty

let private resolveId name =
  Identifier.LocalScope name |> scope.Resolve

let private makeSym name =
  TypeSymbol.Create(Identifier.LocalScope name)

let private makeRecord (fields: (string * TypeValue<IComparable>) list) =
  let om =
    fields
    |> List.map (fun (name, tv) -> makeSym name, (tv, Kind.Star))
    |> OrderedMap.ofList

  TypeValue.CreateRecord om

let private makeUnion (cases: (string * TypeValue<IComparable>) list) =
  let om =
    cases |> List.map (fun (name, tv) -> makeSym name, tv) |> OrderedMap.ofList

  TypeValue.CreateUnion om

let private makeLookup name =
  TypeValue.Lookup(Identifier.LocalScope name)

let private emptyBindings: TypeBindings<IComparable> = Map.empty
let private emptyResolver: Map<Identifier, ResolvedIdentifier> = Map.empty

let private makeBindingsAndResolver
  (types: (string * TypeValue<IComparable>) list)
  =
  let bindings: TypeBindings<IComparable> =
    types
    |> List.map (fun (n, tv) -> resolveId n, (tv, Kind.Star))
    |> Map.ofList

  let resolver =
    types
    |> List.map (fun (n, _) -> Identifier.LocalScope n, resolveId n)
    |> Map.ofList

  bindings, resolver

let private isTargetNamed name (t: TypeValue<IComparable>) =
  match t with
  | TypeValue.Lookup id ->
    match id with
    | Identifier.LocalScope n -> n = name
    | _ -> false
  | _ -> false

let rec private extractionStepLists
  (tree: ExtractionTree)
  : ExtractionStep list list =
  let self = if tree.SelfMatch then [ [] ] else []

  let nested =
    tree.Children
    |> Map.toList
    |> List.collect (fun (step, child) ->
      extractionStepLists child |> List.map (fun rest -> step :: rest))

  self @ nested

[<Test>]
let ``findExtractionTree returns empty for type with no target`` () =
  let host =
    makeRecord [ "name", TypeValue.CreatePrimitive PrimitiveType.String ]

  let tree =
    findExtractionTree
      (isTargetNamed "Evidence")
      emptyBindings
      emptyResolver
      host

  let paths = extractionStepLists tree
  Assert.That(paths, Is.Empty)

[<Test>]
let ``findExtractionTree finds direct Lookup field`` () =
  let host =
    makeRecord
      [ "evidence", makeLookup "Evidence"
        "name", TypeValue.CreatePrimitive PrimitiveType.String ]

  let bindings, resolver =
    makeBindingsAndResolver
      [ "Evidence", TypeValue.CreatePrimitive PrimitiveType.Bool ]

  let tree =
    findExtractionTree (isTargetNamed "Evidence") bindings resolver host

  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(1))

  match paths.[0] with
  | [ RecordField sym ] ->
    Assert.That(sym.Name.ToString(), Is.EqualTo("evidence"))
  | other -> Assert.Fail $"Unexpected path: {other}"

[<Test>]
let ``findExtractionTree finds nested field through Lookup resolution`` () =
  let inner = makeRecord [ "target", makeLookup "Evidence" ]

  let host =
    makeRecord
      [ "nested", makeLookup "Inner"
        "name", TypeValue.CreatePrimitive PrimitiveType.String ]

  let bindings, resolver =
    makeBindingsAndResolver
      [ "Inner", inner
        "Evidence", TypeValue.CreatePrimitive PrimitiveType.Bool ]

  let tree =
    findExtractionTree (isTargetNamed "Evidence") bindings resolver host

  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(1))

  match paths.[0] with
  | [ RecordField outerSym; RecordField innerSym ] ->
    Assert.That(outerSym.Name.ToString(), Is.EqualTo("nested"))
    Assert.That(innerSym.Name.ToString(), Is.EqualTo("target"))
  | other -> Assert.Fail $"Unexpected path: {other}"

[<Test>]
let ``findExtractionTree continues traversal after a match`` () =
  let isLookup (t: TypeValue<IComparable>) =
    match t with
    | TypeValue.Lookup _ -> true
    | _ -> false

  let inner = makeRecord [ "target", makeLookup "Evidence" ]
  let host = makeRecord [ "nested", makeLookup "Inner" ]

  let bindings, resolver =
    makeBindingsAndResolver
      [ "Inner", inner
        "Evidence", TypeValue.CreatePrimitive PrimitiveType.Bool ]

  let tree = findExtractionTree isLookup bindings resolver host
  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(2))

  let containsPath expected =
    paths
    |> List.exists (fun p ->
      match p, expected with
      | [ RecordField a ], [ "nested" ] -> a.Name.ToString() = "nested"
      | [ RecordField a; RecordField b ], [ "nested"; "target" ] ->
        a.Name.ToString() = "nested" && b.Name.ToString() = "target"
      | _ -> false)

  Assert.That(
    containsPath [ "nested" ],
    Is.True,
    "Should include the matched lookup itself"
  )

  Assert.That(
    containsPath [ "nested"; "target" ],
    Is.True,
    "Should also include nested lookup matches"
  )

[<Test>]
let ``findExtractionTree finds multiple target fields`` () =
  let host =
    makeRecord
      [ "evidence1", makeLookup "Evidence"
        "evidence2", makeLookup "Evidence"
        "name", TypeValue.CreatePrimitive PrimitiveType.String ]

  let bindings, resolver =
    makeBindingsAndResolver
      [ "Evidence", TypeValue.CreatePrimitive PrimitiveType.Bool ]

  let tree =
    findExtractionTree (isTargetNamed "Evidence") bindings resolver host

  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(2))

[<Test>]
let ``findExtractionTree finds target inside union case`` () =
  let union =
    makeUnion
      [ "HasEvidence", makeLookup "Evidence"
        "NoEvidence", TypeValue.CreatePrimitive PrimitiveType.Unit ]

  let host = makeRecord [ "data", union ]

  let bindings, resolver =
    makeBindingsAndResolver
      [ "Evidence", TypeValue.CreatePrimitive PrimitiveType.Bool ]

  let tree =
    findExtractionTree (isTargetNamed "Evidence") bindings resolver host

  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(1))

  match paths.[0] with
  | [ RecordField dataSym; UnionCase caseSym ] ->
    Assert.That(dataSym.Name.ToString(), Is.EqualTo("data"))
    Assert.That(caseSym.Name.ToString(), Is.EqualTo("HasEvidence"))
  | other -> Assert.Fail $"Unexpected path: {other}"

[<Test>]
let ``findExtractionTree finds target inside sum alternative`` () =
  let sum =
    TypeValue.CreateSum
      [ TypeValue.CreatePrimitive PrimitiveType.Unit; makeLookup "Evidence" ]

  let host = makeRecord [ "data", sum ]

  let bindings, resolver =
    makeBindingsAndResolver
      [ "Evidence", TypeValue.CreatePrimitive PrimitiveType.Bool ]

  let tree =
    findExtractionTree (isTargetNamed "Evidence") bindings resolver host

  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(1))

  match paths.[0] with
  | [ RecordField dataSym; SumAlternative(case, count) ] ->
    Assert.That(dataSym.Name.ToString(), Is.EqualTo("data"))
    Assert.That(case, Is.EqualTo(1))
    Assert.That(count, Is.EqualTo(2))
  | other -> Assert.Fail $"Unexpected path: {other}"

[<Test>]
let ``findExtractionTree finds target inside tuple`` () =
  let tuple =
    TypeValue.CreateTuple
      [ TypeValue.CreatePrimitive PrimitiveType.String; makeLookup "Evidence" ]

  let host = makeRecord [ "pair", tuple ]

  let bindings, resolver =
    makeBindingsAndResolver
      [ "Evidence", TypeValue.CreatePrimitive PrimitiveType.Bool ]

  let tree =
    findExtractionTree (isTargetNamed "Evidence") bindings resolver host

  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(1))

  match paths.[0] with
  | [ RecordField pairSym; TupleElement idx ] ->
    Assert.That(pairSym.Name.ToString(), Is.EqualTo("pair"))
    Assert.That(idx, Is.EqualTo(1))
  | other -> Assert.Fail $"Unexpected path: {other}"

[<Test>]
let ``findExtractionTree finds target inside Imported type argument`` () =
  let listOfEvidence =
    TypeValue.Imported
      { Id = resolveId "List"
        Sym = makeSym "List"
        Parameters = [ { Name = "a"; Kind = Kind.Star } ]
        Arguments = [ makeLookup "Evidence" ] }

  let host = makeRecord [ "items", listOfEvidence ]

  let bindings, resolver =
    makeBindingsAndResolver
      [ "Evidence", TypeValue.CreatePrimitive PrimitiveType.Bool ]

  let tree =
    findExtractionTree (isTargetNamed "Evidence") bindings resolver host

  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(1))

  match paths.[0] with
  | [ RecordField itemsSym; ContainerElement(ImportedContainer(containerId, 0)) ] ->
    Assert.That(itemsSym.Name.ToString(), Is.EqualTo("items"))
    Assert.That(containerId.Name, Is.EqualTo("List"))
  | other -> Assert.Fail $"Unexpected path: {other}"

[<Test>]
let ``findExtractionTree handles recursive types without infinite loop`` () =
  let host =
    makeRecord
      [ "self", makeLookup "Recursive"; "target", makeLookup "Evidence" ]

  let bindings, resolver =
    makeBindingsAndResolver
      [ "Recursive", makeRecord [ "self", makeLookup "Recursive" ]
        "Evidence", TypeValue.CreatePrimitive PrimitiveType.Bool ]

  let tree =
    findExtractionTree (isTargetNamed "Evidence") bindings resolver host

  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(1))

  match paths.[0] with
  | [ RecordField sym ] ->
    Assert.That(sym.Name.ToString(), Is.EqualTo("target"))
  | other -> Assert.Fail $"Unexpected path: {other}"

[<Test>]
let ``findExtractionTree with predicate matching primitive type`` () =
  let isString (t: TypeValue<IComparable>) =
    match t with
    | TypeValue.Primitive { value = PrimitiveType.String } -> true
    | _ -> false

  let host =
    makeRecord
      [ "name", TypeValue.CreatePrimitive PrimitiveType.String
        "age", TypeValue.CreatePrimitive PrimitiveType.Int32
        "email", TypeValue.CreatePrimitive PrimitiveType.String ]

  let tree = findExtractionTree isString emptyBindings emptyResolver host
  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(2))

[<Test>]
let ``findExtractionTree finds target inside Set element type`` () =
  let setOfEvidence = TypeValue.CreateSet(makeLookup "Evidence")
  let host = makeRecord [ "tags", setOfEvidence ]

  let bindings, resolver =
    makeBindingsAndResolver
      [ "Evidence", TypeValue.CreatePrimitive PrimitiveType.Bool ]

  let tree =
    findExtractionTree (isTargetNamed "Evidence") bindings resolver host

  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(1))

  match paths.[0] with
  | [ RecordField tagsSym; ContainerElement SetContainer ] ->
    Assert.That(tagsSym.Name.ToString(), Is.EqualTo("tags"))
  | other -> Assert.Fail $"Unexpected path: {other}"

[<Test>]
let ``findExtractionTree tracks argument index for multi-arg Imported`` () =
  let mapType =
    TypeValue.Imported
      { Id = resolveId "Map"
        Sym = makeSym "Map"
        Parameters =
          [ { Name = "k"; Kind = Kind.Star }; { Name = "v"; Kind = Kind.Star } ]
        Arguments =
          [ TypeValue.CreatePrimitive PrimitiveType.String
            makeLookup "Evidence" ] }

  let host = makeRecord [ "lookup", mapType ]

  let bindings, resolver =
    makeBindingsAndResolver
      [ "Evidence", TypeValue.CreatePrimitive PrimitiveType.Bool ]

  let tree =
    findExtractionTree (isTargetNamed "Evidence") bindings resolver host

  let paths = extractionStepLists tree

  Assert.That(paths |> List.length, Is.EqualTo(1))

  match paths.[0] with
  | [ RecordField lookupSym
      ContainerElement(ImportedContainer(containerId, argIdx)) ] ->
    Assert.That(lookupSym.Name.ToString(), Is.EqualTo("lookup"))
    Assert.That(containerId.Name, Is.EqualTo("Map"))

    Assert.That(
      argIdx,
      Is.EqualTo(1),
      "Evidence is the second type argument (index 1)"
    )
  | other -> Assert.Fail $"Unexpected path: {other}"
