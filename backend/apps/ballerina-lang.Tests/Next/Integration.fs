module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Integration

open NUnit.Framework
open FSharp.Data
open Ballerina.StdLib.Object
open Ballerina.StdLib.String
open Ballerina.Collections.Sum
open Ballerina.Coroutines.Model
open Ballerina.Reader.WithError
open Ballerina.Errors
open System
open Ballerina.DSL.Next.Unification
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.TypeCheck
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Terms
open Ballerina.State.WithError
open Ballerina.DSL.Next.StdLib.Option
open Ballerina.DSL.Next.Extensions
open Ballerina.LocalizedErrors
open Ballerina.Parser
open Ballerina.Parser
open Ballerina.StdLib.Object
open Ballerina.DSL.Next.Syntax
open Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Expr_Eval
open Ballerina.DSL.Next
open Ballerina.DSL.Next.StdLib
open Ballerina.StdLib.OrderPreservingMap

let context = Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Expr_Eval.context

let private run program =
  let initialLocation = Location.Initial "input"

  let actual =
    Ballerina.DSL.Next.Syntax.Lexer.tokens
    |> Parser.Run(program |> Seq.toList, initialLocation)

  match actual with
  | Right e -> Right $"Failed to tokenize program: {e.ToFSharpString}"
  | Left(ParserResult(actual, _)) ->

    let parsed = Parser.Expr.program |> Parser.Run(actual, initialLocation)

    match parsed with
    | Right e -> Right $"Failed to parse program: {e.ToFSharpString}"
    | Left(ParserResult(program, _)) ->

      let typeCheckResult =
        Expr.TypeCheck None program
        |> State.Run(context.TypeCheckContext, context.TypeCheckState)

      match typeCheckResult with
      | Left((program, typeValue, _), typeCheckFinalState) ->

        let evalContext = context.ExprEvalContext

        let typeCheckedSymbols: ExprEvalContextSymbols =
          (match typeCheckFinalState with
           | None -> TypeExprEvalSymbols.Empty
           | Some s -> s.Types.Symbols)
          |> ExprEvalContextSymbols.FromTypeChecker

        let evalContext =
          { evalContext with
              Symbols = ExprEvalContextSymbols.Append evalContext.Symbols typeCheckedSymbols
          // Values: Map<Identifier, Value<TypeValue, 'valueExtension>>
          // Values =
          //   ((evalContext.Values |> Map.toList)
          //    @ unionCaseConstructors
          //    @ recordFieldDestructors)
          //   |> Map.ofList
          }

        let evalResult = Expr.Eval program |> Reader.Run evalContext

        match evalResult with
        | Left value -> Sum.Left(value, typeValue)
        | Right e -> Sum.Right $"Evaluation failed: {e.ToFSharpString}"
      | Right(e, _) -> Sum.Right $"Type checking failed: {e.ToFSharpString}"

[<Test>]
let ``LangNext-Integration let over int succeeds`` () =
  let program = """let x = 10 in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Int32 10)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")



[<Test>]
let ``LangNext-Integration let over decimal succeeds`` () =
  let program = """let x = 10.5 in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Decimal 10.5M)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Decimal }) -> ()
    | _ -> Assert.Fail($"Expected Decimal type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")



[<Test>]
let ``LangNext-Integration let over bool succeeds`` () =
  let program = """let x = true in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Bool true)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")



[<Test>]
let ``LangNext-Integration let over string succeeds`` () =
  let program = """let x = "Hello world!" in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(String "Hello world!")

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.String }) -> ()
    | _ -> Assert.Fail($"Expected String type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration let over boolean expression succeeds`` () =
  let program = """let x = false || !true && false in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Bool false)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration let over integer expression succeeds`` () =
  let program = """let x = 3 + 5 * 10 - 10 / 2 in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Int32 48)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration let over conditional expression succeeds`` () =
  let program = """let x = if 1 < 2 then true else false in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Bool true)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration let over conditional composite expression succeeds`` () =
  let program =
    """// complex expression with all operators
let x = if true then 1 + 3 * 2 < 5 * -3 / 2 || 10 - 2 < 20 && !false else false
// return x, not really needed
in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Bool true)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration let with annotation fails`` () =
  let program = """let x:int = false in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    Assert.Fail(
      $"Type checking and evaluation succeeded with {(value, typeValue).ToFSharpString}, even though failure was expected"
    )

  | Right _e -> Assert.Pass()



[<Test>]
let ``LangNext-Integration tuple construction and destruction succeeds`` () =
  let program =
    """
let v = (1, "hello", true)
in v.1, v.2, v.3
  """

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> =
      Tuple [ Primitive(Int32 1); Primitive(String "hello"); Primitive(Bool true) ]

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Tuple({ value = [ TypeValue.Primitive({ value = PrimitiveType.Int32 })
                                  TypeValue.Primitive({ value = PrimitiveType.String })
                                  TypeValue.Primitive({ value = PrimitiveType.Bool }) ] }) -> ()
    | _ -> Assert.Fail($"Expected Tuple type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")

[<Test>]
let ``LangNext-Integration tuple construction and destruction fails`` () =
  let program =
    """
type T = int32 * List [int32] * bool
in let t: T = 123, List::Cons [int32] (1, List::Nil [int32] ()), false
in t.1
  """

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Int32 123)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration sum construction and destruction succeeds`` () =
  let program =
    """
type T = int32 + List [int32] + bool
in let t: T = 2Of3(List::Nil [int32] ())
in match t with
| 1Of3 (x -> x)
| 2Of3 (x -> -1)
| 3Of3 (x -> 0)
  """

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Int32 -1)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")



[<Test>]
let ``LangNext-Integration sum and tuple construction and destruction succeeds`` () =
  let program =
    """
type T = int32 + List [int32] * int32 + bool
in let t: T = 2Of3(List::Nil [int32] (), 42)
in let item2 = fun (x:List [int32] * int32) -> x.2
in match t with
| 1Of3 (x -> x)
| 2Of3 (x -> item2 x)
| 3Of3 (x -> 0)
  """

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Int32 42)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration sum and arrow composition succeeds`` () =
  let program =
    """
type T = int32 + bool -> List [int32] + string

in let t1: T = 2Of3(fun flag -> if flag then List::Cons [int32] (1, List::Nil [int32] ()) else List::Nil [int32] ())
in let res1 = match t1 with
| 1Of3 (x -> x)
| 2Of3 (x -> -1)
| 3Of3 (x -> 0)

in res1
  """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->

    let expectedValue: Value<TypeValue, ValueExt> = Primitive(Int32 -1)

    Assert.That(value, Is.EqualTo(expectedValue))

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration arrows construction and destruction succeeds`` () =
  let program =
    """
type T = bool -> List [int32]

in let f: T = fun flag -> if flag then List::Cons [int32] (1, List::Nil [int32] ()) else List::Nil [int32] ()
in f true, f false
  """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Tuple [ Ext(ValueExt(Choice1Of3(_v1))); Ext(ValueExt(Choice1Of3(_v2))) ] -> Assert.Pass()
    | _ -> Assert.Fail($"Expected a tuple of two list values, got {value.ToFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration list cons, nil, map, filter, length, fold succeeds`` () =
  let program =
    """
let l = List::Cons [int32] (1, List::Cons [int32] (-2, List::Cons [int32] (3, List::Nil [int32] ())))
in let l1 = List::map [int32][bool] (fun (v:int32) -> v > 0) l
in let l2 = List::filter [int32] (fun (v:int32) -> v > 0) l
in let l3 = List::length [int32] l2
in let l4 = List::fold [int32] [bool] (fun (acc:bool) -> fun (v:int32) -> acc && v > 0) true l
in l,l1,l2,l3,l4
  """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Tuple [ Ext(ValueExt(Choice1Of3(ListExt.ListValues(StdLib.List.Model.ListValues.List [ Value.Primitive(Int32 1)
                                                                                             Value.Primitive(Int32 -2)
                                                                                             Value.Primitive(Int32 3) ]))))
              Ext(ValueExt(Choice1Of3(ListExt.ListValues(StdLib.List.Model.ListValues.List [ Value.Primitive(Bool true)
                                                                                             Value.Primitive(Bool false)
                                                                                             Value.Primitive(Bool true) ]))))
              Ext(ValueExt(Choice1Of3(ListExt.ListValues(StdLib.List.Model.ListValues.List [ Value.Primitive(Int32 1)
                                                                                             Value.Primitive(Int32 3) ]))))
              Value.Primitive(Int32 2)
              Value.Primitive(Bool false) ] -> Assert.Pass()
    | _ -> Assert.Fail($"Expected a tuple of two list values, got {value}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")




[<Test>]
let ``LangNext-Integration scoped function with closures succeeds`` () =
  let program =
    """
  let x = 7
  in let f = fun (a:int32) -> fun (b:int32) -> a + b + x
  in f 5 6
  """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(Int32 18) -> Assert.Pass()
    | _ -> Assert.Fail($"Expected a tuple of two list values, got {value}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")



[<Test>]
let ``LangNext-Integration union des with wildcard and ambiguous name succeeds`` () =
  let program =
    """
type T = 
| T of int32
| CaseA of int32
| CaseB of string
| CaseC of bool

in let t:T = CaseC false
in match t with
  | T (x -> x + 100)
  | CaseC (b -> if b then 1 else 0)
  | (* -> -1000)
  """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(Int32 0) -> Assert.Pass()
    | _ -> Assert.Fail($"Expected 18, got {value}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")





[<Test>]
let ``LangNext-Integration list append succeeds`` () =
  let program =
    """
let cons = List::Cons [string]
in let nil = List::Nil [string] ()
in let l1 = cons("hello", cons(" ", cons("world", nil)))
in let l2 = cons("bonjour", cons(" ", cons("monde", nil)))
in List::append [string] l1 l2
  """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Ext(ValueExt(Choice1Of3(ListExt.ListValues(List.Model.ListValues.List [ Value.Primitive(String "hello")
                                                                              Value.Primitive(String " ")
                                                                              Value.Primitive(String "world")
                                                                              Value.Primitive(String "bonjour")
                                                                              Value.Primitive(String " ")
                                                                              Value.Primitive(String "monde") ])))) ->
      Assert.Pass()
    | _ -> Assert.Fail($"Expected a list with the appended values, got {value}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")





[<Test>]
let ``LangNext-Integration same record field names disambiguate succesfully thanks to type annotations`` () =
  let program =
    """
type T1 = {
  A: int32;
  B: string;
  C: bool;
}    

in type T2 = {    
  A: string;
  B: int32;
  C: bool;
}

in let a1:T1 = { A=10; B="hello"; C=true; }
in let a2:T2 = { A="world"; B=20; C=false; }

in let f = fun (x:T1) -> x.A
in let x = f a1 + string::length (a1.B + "???")

in x
  """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(Int32 18) -> Assert.Pass()
    | _ -> Assert.Fail($"Expected 18, got {value}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")



[<Test>]
let ``LangNext-Integration sum and lambda cons and des with annotations`` () =
  let program =
    """
let f = (fun (x:string + int32) -> match x with | 1Of2 (v -> string::length ("!!!" + v)) | 2Of2 (v -> v))
in f (1Of2 "hello"), f (2Of2 10)
  """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Tuple [ Value.Primitive(Int32 8); Value.Primitive(Int32 10) ] -> Assert.Pass()
    | _ -> Assert.Fail($"Expected (8, 10), got {value}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")


[<Test>]
let ``LangNext-Integration tuple and lambda cons and des with annotations`` () =
  let program =
    """
let f = (fun (x:int32 * string) -> x.1)
in f (1, "hello")
  """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(Int32 1) -> Assert.Pass()
    | _ -> Assert.Fail($"Expected 1, got {value}")

  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")

let PrimitiveOpTest (program: string, expectedValue: PrimitiveValue, errMsg: Value<TypeValue, ValueExt> -> string) =
  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive res when res = expectedValue -> Assert.Pass()
    | _ -> Assert.Fail(errMsg value)
  | Right e -> Assert.Fail($"Run failed: {e.ToFSharpString}")

[<Test>]
let ``LangNext-Integration int32 literals work`` () =
  let programs =
    [ "let x = 42 in x", Int32 42, fun v -> $"Expected 42, got {v}"
      "let x = -42 in x", Int32 -42, fun v -> $"Expected -42, got {v}" ]

  programs
  |> List.iter (fun (program, expectedValue, errMsg) -> PrimitiveOpTest(program, expectedValue, errMsg))

[<Test>]
let ``LangNext-Integration Int64 literals work`` () =
  let programs =
    [ "let x = 42l in x", Int64 42L, fun v -> $"Expected 42 (Int64), got {v}"
      "let x = -42l in x", Int64 -42L, fun v -> $"Expected -42 (Int64), got {v}" ]

  programs
  |> List.iter (fun (program, expectedValue, errMsg) -> PrimitiveOpTest(program, expectedValue, errMsg))

[<Test>]
let ``LangNext-Integration Float32 literals work`` () =
  let programs =
    [ "let x = 94.53f in x", Float32 94.53f, fun v -> $"Expected 94.53f, got {v}"
      "let x = -94.53f in x", Float32 -94.53f, fun v -> $"Expected -94.53f, got {v}" ]

  programs
  |> List.iter (fun (program, expectedValue, errMsg) -> PrimitiveOpTest(program, expectedValue, errMsg))

[<Test>]
let ``LangNext-Integration Float64 literals work`` () =
  let programs =
    [ "let x = 123456789.123d in x", Float64 123456789.123, fun v -> $"Expected 123456789.123, got {v}"
      "let x = -123456789.123d in x", Float64 -123456789.123, fun v -> $"Expected -123456789.123, got {v}" ]

  programs
  |> List.iter (fun (program, expectedValue, errMsg) -> PrimitiveOpTest(program, expectedValue, errMsg))

[<Test>]
let ``LangNext-Integration Decimal literals work`` () =
  let programs =
    [ "let x = 12.123 in x", Decimal 12.123M, fun v -> $"Expected 12.123, got {v}"
      "let x = -12.123 in x", Decimal -12.123M, fun v -> $"Expected -12.123, got {v}" ]

  programs
  |> List.iter (fun (program, expectedValue, errMsg) -> PrimitiveOpTest(program, expectedValue, errMsg))

[<Test>]
let ``LangNext-Integration Int32 operations work`` () =
  let int32Ops =
    [ "let x = 100 - 50 in x", Int32 50, fun v -> $"Expected 50, got {v}"
      "let x = 100 * 2 in x", Int32 200, fun v -> $"Expected 200, got {v}"
      "let x = 100 / 2 in x", Int32 50, fun v -> $"Expected 50, got {v}"
      "let x = 100 % 3 in x", Int32 1, fun v -> $"Expected 1, got {v}"
      "let x = 100 ^ 2 in x", Int32 10000, fun v -> $"Expected 10000, got {v}" ]

  int32Ops
  |> List.iter (fun (program, expectedValue, errMsg) -> PrimitiveOpTest(program, expectedValue, errMsg))

[<Test>]
let ``LangNext-Integration Int64 operations work`` () =
  let int64Ops =
    [ "let x = 100l - 50l in x", Int64 50L, fun v -> $"Expected 50, got {v}"
      "let x = 100l * 2l in x", Int64 200L, fun v -> $"Expected 200, got {v}"
      "let x = 100l / 2l in x", Int64 50L, fun v -> $"Expected 50, got {v}"
      "let x = 100l % 3l in x", Int64 1L, fun v -> $"Expected 1, got {v}"
      "let x = 100l ^ 2l in x", Int64 10000L, fun v -> $"Expected 10000, got {v}" ]

  int64Ops
  |> List.iter (fun (program, expectedValue, errMsg) -> PrimitiveOpTest(program, expectedValue, errMsg))

[<Test>]
let ``LangNext-Integration Float32 operations work`` () =
  let float32Ops =
    [ "let x = 10.0f - 5.0f in x", Float32 5.0f, fun v -> $"Expected 5.0f, got {v}"
      "let x = 10.0f * 2.0f in x", Float32 20.0f, fun v -> $"Expected 20.0f, got {v}"
      "let x = 10.0f / 2.0f in x", Float32 5.0f, fun v -> $"Expected 5.0f, got {v}"
      "let x = 10.0f % 3.0f in x", Float32 1.0f, fun v -> $"Expected 1.0f, got {v}"
      "let x = 10.0f ^ 2.0f in x", Float32 100.0f, fun v -> $"Expected 100.0f, got {v}" ]

  float32Ops
  |> List.iter (fun (program, expectedValue, errMsg) -> PrimitiveOpTest(program, expectedValue, errMsg))

[<Test>]
let ``LangNext-Integration Float64 operations work`` () =
  let float64Ops =
    [ "let x = 10.0d - 5.0d in x", Float64 5.0, fun v -> $"Expected 5.0, got {v}"
      "let x = 10.0d * 2.0d in x", Float64 20.0, fun v -> $"Expected 20.0, got {v}"
      "let x = 10.0d / 2.0d in x", Float64 5.0, fun v -> $"Expected 5.0, got {v}"
      "let x = 10.0d % 3.0d in x", Float64 1.0, fun v -> $"Expected 1.0, got {v}"
      "let x = 10.0d ^ 2.0d in x", Float64 100.0, fun v -> $"Expected 100.0, got {v}" ]

  float64Ops
  |> List.iter (fun (program, expectedValue, errMsg) -> PrimitiveOpTest(program, expectedValue, errMsg))

[<Test>]
let ``LangNext-Integration Decimal operations work`` () =
  let decimalOps =
    [ "let x = 10.0 - 5.0 in x", Decimal 5.0M, fun v -> $"Expected 5.0, got {v}"
      "let x = 10.0 * 2.0 in x", Decimal 20.0M, fun v -> $"Expected 20.0, got {v}"
      "let x = 10.0 / 2.0 in x", Decimal 5.0M, fun v -> $"Expected 5.0, got {v}"
      "let x = 10.0 % 3.0 in x", Decimal 1.0M, fun v -> $"Expected 1.0, got {v}"
      "let x = 10.0 ^ 2.0 in x", Decimal 100.0M, fun v -> $"Expected 100.0, got {v}" ]

  decimalOps
  |> List.iter (fun (program, expectedValue, errMsg) -> PrimitiveOpTest(program, expectedValue, errMsg))
