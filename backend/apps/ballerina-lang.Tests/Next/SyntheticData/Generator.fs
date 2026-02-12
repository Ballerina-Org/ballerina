module Ballerina.Cat.Tests.BusinessRuleEngine.Next.SyntheticData.Generator

open System
open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.Cat.Collections.OrderedMap
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.SyntheticData
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.DSL.Next.Types.TypeChecker.Patterns
open Ballerina.DSL.Next.Runners
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.SyntheticData.Generator
open Ballerina.Cat.Tests.BusinessRuleEngine.Next.SyntheticData.ImportedTypesConfig
open Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Expr_Eval
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.List.Model
open Ballerina.Errors

let private emptyContext<'valueExt when 'valueExt: comparison> () : TypeCheckContext<'valueExt> =
  TypeCheckContext.Empty("", "")

let private emptyState<'valueExt when 'valueExt: comparison> () : TypeCheckState<'valueExt> = TypeCheckState.Empty

let private listImportedGenerators<'valueExt when 'valueExt: comparison>
  ()
  : Map<ResolvedIdentifier, ImportedGenerator<ValueExt<'valueExt>, ListConfig>> =
  let stdlib, _ = stdExtensions
  let listTypeId = stdlib.List.TypeName |> fst

  let generator =
    { ImportedGenerator.Id = listTypeId
      Generate =
        fun (config: GeneratorConfig<ListConfig>) generate imported ->
          match imported.Arguments with
          | [ elementType ] ->
            let maxLength =
              config.ImportedConfig
              |> Option.map (fun c -> c.MaxLength)
              |> Option.defaultValue GENERATED_MAX_COLLECTION_LENGTH

            let length = config.Random.Next(0, maxLength + 1)

            let items = [ for _ in 1..length -> generate elementType ] |> sum.All

            items
            |> sum.Map(fun values ->
              let listValues = ListValues.List values
              let extValue = ValueExt.ValueExt(Choice1Of7(ListExt.ListValues listValues))
              Value.Ext(extValue, None))
          | _ ->
            (fun () -> $"Expected 1 list type argument, got {imported.Arguments.Length}")
            |> Errors.Singleton()
            |> sum.Throw }

  Map.ofList [ listTypeId, generator ]

let private assertPrimitive expected actual =
  match expected, actual with
  | PrimitiveType.Int32, Value.Primitive(PrimitiveValue.Int32 _) -> ()
  | PrimitiveType.String, Value.Primitive(PrimitiveValue.String _) -> ()
  | PrimitiveType.Bool, Value.Primitive(PrimitiveValue.Bool _) -> ()
  | PrimitiveType.Guid, Value.Primitive(PrimitiveValue.Guid _) -> ()
  | PrimitiveType.DateTime, Value.Primitive(PrimitiveValue.DateTime _) -> ()
  | PrimitiveType.DateOnly, Value.Primitive(PrimitiveValue.Date _) -> ()
  | PrimitiveType.TimeSpan, Value.Primitive(PrimitiveValue.TimeSpan _) -> ()
  | PrimitiveType.Decimal, Value.Primitive(PrimitiveValue.Decimal _) -> ()
  | PrimitiveType.Float32, Value.Primitive(PrimitiveValue.Float32 _) -> ()
  | PrimitiveType.Float64, Value.Primitive(PrimitiveValue.Float64 _) -> ()
  | PrimitiveType.Int64, Value.Primitive(PrimitiveValue.Int64 _) -> ()
  | PrimitiveType.Unit, Value.Primitive PrimitiveValue.Unit -> ()
  | _ -> Assert.Fail $"Expected primitive {expected} but got {actual}"

let private logGenerated (typeValue: TypeValue<_>) (value: Value<TypeValue<_>, _>) =
  Console.WriteLine $"Generated value: {value}"
  Console.WriteLine $"Generated type: {typeValue}"

[<Test>]
let ``SyntheticData generates primitive values`` () =
  let config = configWithRandom 1 None

  let context: TypeCheckContext<unit> * TypeCheckState<unit> =
    emptyContext (), emptyState ()

  let importedGenerators = Map.empty

  let actual =
    Ballerina.DSL.Next.SyntheticData.Generator.Generate config context importedGenerators (TypeValue.CreateInt32())

  match actual with
  | Sum.Left value ->
    logGenerated (TypeValue.CreateInt32()) value
    assertPrimitive PrimitiveType.Int32 value
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``SyntheticData resolves lookups`` () =
  let t = TypeValue.CreateString()
  let lookupId = Identifier.LocalScope "MyString"
  let resolved = TypeCheckScope.Empty.Resolve lookupId

  let context: TypeCheckContext<unit> =
    emptyContext ()
    |> TypeCheckContext.Updaters.Values(fun _ -> Map.ofList [ resolved, (t, Kind.Star) ])

  let config = configWithRandom 2 None
  let context: TypeCheckContext<unit> * TypeCheckState<unit> = context, emptyState ()
  let importedGenerators = Map.empty

  let actual =
    Ballerina.DSL.Next.SyntheticData.Generator.Generate config context importedGenerators (TypeValue.Lookup lookupId)

  match actual with
  | Sum.Left value ->
    logGenerated (TypeValue.Lookup lookupId) value
    assertPrimitive PrimitiveType.String value
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``SyntheticData resolves type variables`` () =
  let tVar = TypeVar.Create "a"

  let context: TypeCheckContext<unit> =
    emptyContext ()
    |> TypeCheckContext.Updaters.TypeVariables(fun _ -> Map.ofList [ tVar.Name, (TypeValue.CreateBool(), Kind.Star) ])

  let config = configWithRandom 3 None
  let context: TypeCheckContext<unit> * TypeCheckState<unit> = context, emptyState ()
  let importedGenerators = Map.empty

  let actual =
    Ballerina.DSL.Next.SyntheticData.Generator.Generate config context importedGenerators (TypeValue.Var tVar)

  match actual with
  | Sum.Left value ->
    logGenerated (TypeValue.Var tVar) value
    assertPrimitive PrimitiveType.Bool value
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``SyntheticData builds records and tuples`` () =
  let fieldA = TypeSymbol.Create(Identifier.LocalScope "A")
  let fieldB = TypeSymbol.Create(Identifier.LocalScope "B")

  let recordType =
    OrderedMap.ofList
      [ fieldA, (TypeValue.CreateInt32(), Kind.Star)
        fieldB, (TypeValue.CreateString(), Kind.Star) ]
    |> TypeValue.CreateRecord

  let tupleType =
    TypeValue.CreateTuple [ TypeValue.CreateBool(); TypeValue.CreateGuid() ]

  let config = configWithRandom 4 None

  let context: TypeCheckContext<unit> * TypeCheckState<unit> =
    emptyContext (), emptyState ()

  let importedGenerators = Map.empty

  let recordValue =
    Ballerina.DSL.Next.SyntheticData.Generator.Generate config context importedGenerators recordType

  let tupleValue =
    Ballerina.DSL.Next.SyntheticData.Generator.Generate config context importedGenerators tupleType

  match recordValue with
  | Sum.Left(Value.Record fields) as recordResult ->
    let typeValue = recordType

    match recordResult with
    | Sum.Left value -> logGenerated typeValue value
    | _ -> ()

    let scope = TypeCheckScope.Empty
    let fieldAId = scope.Resolve fieldA.Name
    let fieldBId = scope.Resolve fieldB.Name

    match Map.tryFind fieldAId fields, Map.tryFind fieldBId fields with
    | Some vA, Some vB ->
      assertPrimitive PrimitiveType.Int32 vA
      assertPrimitive PrimitiveType.String vB
    | _ -> Assert.Fail $"Expected record with fields A and B but got {fields}"
  | Sum.Left other -> Assert.Fail $"Expected record value but got {other}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

  match tupleValue with
  | Sum.Left(Value.Tuple items) as tupleResult ->
    let typeValue = tupleType

    match tupleResult with
    | Sum.Left value -> logGenerated typeValue value
    | _ -> ()

    match items with
    | [ v1; v2 ] ->
      assertPrimitive PrimitiveType.Bool v1
      assertPrimitive PrimitiveType.Guid v2
    | _ -> Assert.Fail $"Expected tuple of length 2 but got {items}"
  | Sum.Left other -> Assert.Fail $"Expected tuple value but got {other}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``SyntheticData builds union and sum cases`` () =
  let caseA = TypeSymbol.Create(Identifier.LocalScope "A")
  let caseB = TypeSymbol.Create(Identifier.LocalScope "B")

  let unionType =
    OrderedMap.ofList [ caseA, TypeValue.CreateInt32(); caseB, TypeValue.CreateString() ]
    |> TypeValue.CreateUnion

  let sumType =
    TypeValue.CreateSum([ TypeValue.CreateInt32(); TypeValue.CreateString() ])

  let config = configWithRandom 5 None

  let context: TypeCheckContext<unit> * TypeCheckState<unit> =
    emptyContext (), emptyState ()

  let importedGenerators = Map.empty

  let unionValue =
    Ballerina.DSL.Next.SyntheticData.Generator.Generate config context importedGenerators unionType

  let sumValue =
    Ballerina.DSL.Next.SyntheticData.Generator.Generate config context importedGenerators sumType

  match unionValue with
  | Sum.Left(Value.UnionCase(caseId, value)) ->
    logGenerated unionType (Value.UnionCase(caseId, value))
    let scope = TypeCheckScope.Empty
    let caseAId = scope.Resolve caseA.Name
    let caseBId = scope.Resolve caseB.Name

    match caseId with
    | id when id = caseAId -> assertPrimitive PrimitiveType.Int32 value
    | id when id = caseBId -> assertPrimitive PrimitiveType.String value
    | _ -> Assert.Fail $"Unexpected union case id {caseId}"
  | Sum.Left other -> Assert.Fail $"Expected union value but got {other}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

  match sumValue with
  | Sum.Left(Value.Sum(selector, value)) ->
    logGenerated sumType (Value.Sum(selector, value))
    Assert.That(selector.Count, Is.EqualTo 2)

    match selector.Case with
    | 1 -> assertPrimitive PrimitiveType.Int32 value
    | 2 -> assertPrimitive PrimitiveType.String value
    | _ -> Assert.Fail $"Unexpected sum selector {selector}"
  | Sum.Left other -> Assert.Fail $"Expected sum value but got {other}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``SyntheticData delegates imported generators to build arguments`` () =
  let typeId = ResolvedIdentifier.Create "Box"
  let symbol = TypeSymbol.Create(Identifier.LocalScope "Box")

  let importedType =
    { ImportedTypeValue.Id = typeId
      Sym = symbol
      Parameters = []
      Arguments = [ TypeValue.CreateString(); TypeValue.CreateInt32() ] }

  let generator =
    { ImportedGenerator.Id = typeId
      Generate = fun _ generate imported -> imported.Arguments |> List.map generate |> sum.All |> sum.Map Value.Tuple }

  let config = configWithRandom 6 None

  let context: TypeCheckContext<unit> * TypeCheckState<unit> =
    emptyContext (), emptyState ()

  let importedGenerators = Map.ofList [ typeId, generator ]

  let actual =
    Ballerina.DSL.Next.SyntheticData.Generator.Generate
      config
      context
      importedGenerators
      (TypeValue.Imported importedType)

  match actual with
  | Sum.Left(Value.Tuple values) as tupleResult ->
    match tupleResult with
    | Sum.Left value -> logGenerated (TypeValue.Imported importedType) value
    | _ -> ()

    match values with
    | [ v1; v2 ] ->
      assertPrimitive PrimitiveType.String v1
      assertPrimitive PrimitiveType.Int32 v2
    | _ -> Assert.Fail $"Expected tuple with two values but got {values}"
  | Sum.Left other -> Assert.Fail $"Expected tuple value but got {other}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``SyntheticData generates values from parsed program type`` () =
  let program =
    """
type T = { A:int32; B:bool; }
in let t:T = { A=10; B=true; }
in t
    """

  let typeCheckResult = Expr.TypeCheckString context program

  match typeCheckResult with
  | Left(_expr, typeValue, finalState) ->
    let config = configWithRandom 7 None
    let importedGenerators = Map.empty

    let generated =
      Generator.Generate config (context.TypeCheckContext, finalState) importedGenerators typeValue

    match generated with
    | Sum.Left(Value.Record fields) as generatedResult ->
      match generatedResult with
      | Sum.Left value -> logGenerated typeValue value
      | _ -> ()

      let fieldNames = fields |> Map.keys |> Seq.map (fun k -> k.Name) |> Set.ofSeq

      Assert.That(fieldNames, Is.SupersetOf([ "A"; "B" ]))
    | Sum.Left other -> Assert.Fail $"Expected record value but got {other}"
    | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | Right err -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``SyntheticData generates values for parsed complex program types`` () =
  let program =
    """
type R = { A:int32; B:bool; C:string; }
in type U = | A of int32 | B of string | C of bool
in type T = int32 * bool * string
in type S = int32 + string + bool
in type L = List [int32]
in let r:R = { A=1; B=true; C="x"; }
in let u:U = A 1
in let t:T = (1, true, "x")
in let s:S = 1Of3 1
in let l:L = List::Nil [int32] ()
in (r, u, t, s, l)
    """

  match Expr.TypeCheckString context program with
  | Left(_expr, _typeValue, finalState) ->
    let config = configWithRandom 9 (Some ListConfig.Default)
    let ctx = context.TypeCheckContext, finalState
    let importedGenerators = listImportedGenerators ()

    let generate name =
      let typeValue = TypeValue.Lookup(Identifier.LocalScope name)
      Generator.Generate<ValueExt<unit>, ListConfig> config ctx importedGenerators typeValue

    let assertRecord typeValue result =
      match result with
      | Sum.Left(Value.Record fields) ->
        match result with
        | Sum.Left value -> logGenerated typeValue value
        | _ -> ()

        let fieldNames = fields |> Map.keys |> Seq.map (fun k -> k.Name) |> Set.ofSeq
        Assert.That(fieldNames, Is.SupersetOf([ "A"; "B"; "C" ]))
      | Sum.Left other -> Assert.Fail $"Expected record value but got {other}"
      | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

    let assertUnion typeValue result =
      match result with
      | Sum.Left(Value.UnionCase(_, _)) ->
        match result with
        | Sum.Left value -> logGenerated typeValue value
        | _ -> ()

        Assert.Pass()
      | Sum.Left other -> Assert.Fail $"Expected union value but got {other}"
      | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

    let assertTuple typeValue result =
      match result with
      | Sum.Left(Value.Tuple items) ->
        match result with
        | Sum.Left value -> logGenerated typeValue value
        | _ -> ()

        Assert.That(items.Length, Is.EqualTo 3)
      | Sum.Left other -> Assert.Fail $"Expected tuple value but got {other}"
      | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

    let assertSum typeValue result =
      match result with
      | Sum.Left(Value.Sum(selector, _)) ->
        match result with
        | Sum.Left value -> logGenerated typeValue value
        | _ -> ()

        Assert.That(selector.Count, Is.EqualTo 3)
      | Sum.Left other -> Assert.Fail $"Expected sum value but got {other}"
      | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

    let assertList typeValue result =
      match result with
      | Sum.Left(Value.Ext(ValueExt.ValueExt(Choice1Of7(ListExt.ListValues(ListValues.List items))), None)) ->
        match result with
        | Sum.Left value -> logGenerated typeValue value
        | _ -> ()

        Assert.That(items.Length, Is.LessThanOrEqualTo GENERATED_MAX_COLLECTION_LENGTH)
      | Sum.Left other -> Assert.Fail $"Expected list value but got {other}"
      | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

    for _ in 1..3 do
      let rType = TypeValue.Lookup(Identifier.LocalScope "R")
      let uType = TypeValue.Lookup(Identifier.LocalScope "U")
      let tType = TypeValue.Lookup(Identifier.LocalScope "T")
      let sType = TypeValue.Lookup(Identifier.LocalScope "S")
      let lType = TypeValue.Lookup(Identifier.LocalScope "L")

      generate "R" |> assertRecord rType
      generate "U" |> assertUnion uType
      generate "T" |> assertTuple tType
      generate "S" |> assertSum sType
      generate "L" |> assertList lType
  | Right err -> Assert.Fail $"Type checking failed: {err}"

[<Test>]
let ``SyntheticData generates nested records from parsed program types`` () =
  let program =
    """
type Inner = { X:int32; Y:bool; }
in type U = | A of int32 | B of string
in type S = int32 + string
in type Outer = { Inner:Inner; Tuple:int32 * string; Choice:S; Union:U; }
in let o:Outer = { Inner={ X=1; Y=true; }; Tuple=(1, "a"); Choice=1Of2 1; Union=A 1; }
in o
    """

  match Expr.TypeCheckString context program with
  | Left(_expr, _typeValue, finalState) ->
    let config = configWithRandom 10 None
    let ctx = context.TypeCheckContext, finalState
    let importedGenerators = Map.empty

    let generated =
      Generator.Generate config ctx importedGenerators (TypeValue.Lookup(Identifier.LocalScope "Outer"))

    match generated with
    | Sum.Left(Value.Record fields) as generatedResult ->
      match generatedResult with
      | Sum.Left value -> logGenerated (TypeValue.Lookup(Identifier.LocalScope "Outer")) value
      | _ -> ()

      let fieldNames = fields |> Map.keys |> Seq.map (fun k -> k.Name) |> Set.ofSeq
      Assert.That(fieldNames, Is.SupersetOf [ "Inner"; "Tuple"; "Choice"; "Union" ])
    | Sum.Left other -> Assert.Fail $"Expected record value but got {other}"
    | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | Right err -> Assert.Fail $"Type checking failed: {err}"


[<Test>]
let ``SyntheticData: primitive generation`` () =
  let actual =
    Generate
      GeneratorConfig<unit>.Empty
      (TypeCheckContext.Empty("", ""), TypeCheckState.Empty)
      Map.empty
      (TypeValue.CreateInt32())

  match actual with
  | Sum.Left(Value.Primitive(PrimitiveValue.Int32 _)) -> Assert.Pass()
  | Sum.Left other -> Assert.Fail $"Expected int32 primitive but got {other}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``SyntheticData: missing lookup fails`` () =
  let actual =
    Generate
      GeneratorConfig<unit>.Empty
      (TypeCheckContext.Empty("", ""), TypeCheckState.Empty)
      Map.empty
      (TypeValue.Lookup(Identifier.LocalScope "Missing"))

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left other -> Assert.Fail $"Expected failure but got {other}"
