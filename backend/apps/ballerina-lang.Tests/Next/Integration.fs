module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Integration

open NUnit.Framework
open Ballerina.StdLib.Object
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next
open Ballerina.DSL.Next.StdLib
open Ballerina.DSL.Next.Runners
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.List.Model
open Ballerina.DSL.Next.StdLib.Map.Model
open Ballerina.Cat.Collections.OrderedMap
open Ballerina.Collections.NonEmptyList

let context = Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Expr_Eval.context

let private run (program: string) =

  let typeCheckResult = Expr.TypeCheckString context program

  match typeCheckResult with
  | Left(program, typeValue, typeCheckFinalState) ->

    let evalContext = context.ExprEvalContext

    let typeCheckedSymbols: ExprEvalContextSymbols =
      (typeCheckFinalState.Symbols) |> ExprEvalContextSymbols.FromTypeChecker

    let evalContext =
      { evalContext with
          Scope =
            { evalContext.Scope with
                Symbols = ExprEvalContextSymbols.Append evalContext.Scope.Symbols typeCheckedSymbols }
      // Values: Map<Identifier, Value<TypeValue, 'valueExtension>>
      // Values =
      //   ((evalContext.Values |> Map.toList)
      //    @ unionCaseConstructors
      //    @ recordFieldDestructors)
      //   |> Map.ofList
      }

    let evalResult =
      Expr.Eval(NonEmptyList.prependList context.TypeCheckedPreludes (NonEmptyList.One program))
      |> Reader.Run evalContext

    match evalResult with
    | Left value -> Sum.Left(value, typeValue)
    | Right e -> Sum.Right $"Evaluation failed: {e.AsFSharpString}"
  | Right e -> Sum.Right $"Type checking failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration let over int succeeds`` () =
  let program = """let x = 10 in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> = Primitive(Int32 10)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")



[<Test>]
let ``LangNext-Integration let over decimal succeeds`` () =
  let program = """let x = 10.5 in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> = Primitive(Decimal 10.5M)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Decimal }) -> ()
    | _ -> Assert.Fail($"Expected Decimal type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")



[<Test>]
let ``LangNext-Integration let over bool succeeds`` () =
  let program = """let x = true in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> = Primitive(Bool true)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")



[<Test>]
let ``LangNext-Integration let over string succeeds`` () =
  let program = """let x = "Hello world!" in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> =
      Primitive(String "Hello world!")

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.String }) -> ()
    | _ -> Assert.Fail($"Expected String type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")


[<Test>]
let ``LangNext-Integration let over boolean expression succeeds`` () =
  let program = """let x = false || !true && false in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> = Primitive(Bool false)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")


[<Test>]
let ``LangNext-Integration let over integer expression succeeds`` () =
  let program = """let x = 3 + 5 * 10 - 10 / 2 in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> = Primitive(Int32 48)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")


[<Test>]
let ``LangNext-Integration let over conditional expression succeeds`` () =
  let program = """let x = if 1 < 2 then true else false in x"""

  let actual = program |> run

  match actual with
  | Left(value, typeValue) ->

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> = Primitive(Bool true)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")


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

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> = Primitive(Bool true)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Bool }) -> ()
    | _ -> Assert.Fail($"Expected Bool type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")


[<Test>]
let ``LangNext-Integration programs that should not typecheck fail`` () =
  let programs =
    [ """
let a = 1Of3 10
in match a with | 1Of3 (v -> v + 1) | 2Of3 (v -> 0)
            """

      """
let a = 1Of3 10
in match a with | 1Of3 (v -> v + 1) | 2Of3 (v -> "") | 3Of3 (v -> false)
            """

      """
let a = 1Of3 10
in match a with | 1Of3 (v -> v + 1) | 2Of3 (v -> "") | 3Of3 (v -> false)
            """

      """
type T = { A:int32; B:bool; }
in let t:T = { A=10; C=true; }
in t.A > 10 || t.B
    """

      """
type T = { A:int32; B:bool; }
in let t:T = { A=10; B=true; }
in t.C
    """

      """
type T = | A of int32 | B of string
in (fun (t:T) -> 
match t with
| B (_ -> 0)
)
      """

      """
type T = | A of int32 | B of string
in (fun (t:T) -> 
match t with
| A (i -> i |> string::length)
| B (_ -> 0)
)
      """

      """10 && false"""

      """if 10 then "hello" else "world" """

      """fun (y:int32) -> x + y"""

      """let x:int = false in x"""

      """
(1,2,3).0
      """

      """
(1,2,3).4
      """

      """
let f = (fun (x:string) -> x |> string::length)
in f 10
            """

      """
let f = (fun (x:string) -> x && true)
in f false
      """ ]

  for program in programs do
    let actual = program |> run

    match actual with
    | Left(value, typeValue) ->

      Assert.Fail(
        $"Type checking and evaluation succeeded with {(value, typeValue).AsFSharpString}, even though failure was expected"
      )

    | Right _e -> ()

  Assert.Pass()


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

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> =
      Tuple [ Primitive(Int32 1); Primitive(String "hello"); Primitive(Bool true) ]

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Tuple({ value = [ TypeValue.Primitive({ value = PrimitiveType.Int32 })
                                  TypeValue.Primitive({ value = PrimitiveType.String })
                                  TypeValue.Primitive({ value = PrimitiveType.Bool }) ] }) -> ()
    | _ -> Assert.Fail($"Expected Tuple type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")

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

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> = Primitive(Int32 123)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")


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

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> = Primitive(Int32 -1)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")



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

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> = Primitive(Int32 42)

    Assert.That(value, Is.EqualTo(expectedValue))

    match typeValue with
    | TypeValue.Primitive({ value = PrimitiveType.Int32 }) -> ()
    | _ -> Assert.Fail($"Expected Int32 type, got {typeValue.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")


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

    let expectedValue: Value<TypeValue<ValueExt>, ValueExt> = Primitive(Int32 -1)

    Assert.That(value, Is.EqualTo(expectedValue))

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")


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
    | Tuple [ Ext(ValueExt(Choice1Of6(_v1)), None); Ext(ValueExt(Choice1Of6(_v2)), None) ] -> Assert.Pass()
    | _ -> Assert.Fail($"Expected a tuple of two list values, got {value.AsFSharpString}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")


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
    | Tuple [ Ext(ValueExt(Choice1Of6(ListExt.ListValues(StdLib.List.Model.ListValues.List [ Value.Primitive(Int32 1)
                                                                                             Value.Primitive(Int32 -2)
                                                                                             Value.Primitive(Int32 3) ]))),
                  _)
              Ext(ValueExt(Choice1Of6(ListExt.ListValues(StdLib.List.Model.ListValues.List [ Value.Primitive(Bool true)
                                                                                             Value.Primitive(Bool false)
                                                                                             Value.Primitive(Bool true) ]))),
                  _)
              Ext(ValueExt(Choice1Of6(ListExt.ListValues(StdLib.List.Model.ListValues.List [ Value.Primitive(Int32 1)
                                                                                             Value.Primitive(Int32 3) ]))),
                  _)
              Value.Primitive(Int32 2)
              Value.Primitive(Bool false) ] -> Assert.Pass()
    | _ -> Assert.Fail($"Expected a tuple of two list values, got {value}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")




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

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")



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

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")





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
    | Ext(ValueExt(Choice1Of6(ListExt.ListValues(List.Model.ListValues.List [ Value.Primitive(String "hello")
                                                                              Value.Primitive(String " ")
                                                                              Value.Primitive(String "world")
                                                                              Value.Primitive(String "bonjour")
                                                                              Value.Primitive(String " ")
                                                                              Value.Primitive(String "monde") ]))),
          None) -> Assert.Pass()
    | _ -> Assert.Fail($"Expected a list with the appended values, got {value}")

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")





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

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")



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

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")


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

  | Right e -> Assert.Fail($"Run failed: {e.AsFSharpString}")

let PrimitiveOpTest
  (program: string, expectedValue: PrimitiveValue, errMsg: Value<TypeValue<ValueExt>, ValueExt> -> string)
  =
  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive res when res = expectedValue -> Assert.Pass $"Correctly evaluated to {expectedValue.ToString()}"
    | _ -> Assert.Fail(errMsg value)
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

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

[<Test>]
let ``LangNext-Integration DateOnly constructor builds a DateOnly value`` () =
  let program = "let x = dateOnly::new(\"2021-02-01\") in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Sum({ Case = 2; Count = 2 }, Value.Primitive(PrimitiveValue.Date(d))) when d = System.DateOnly(2021, 2, 1) ->
      Assert.Pass $"Correctly evaluated to 2021-02-01"
    | _ -> Assert.Fail $"Expected 2021-02-01, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration DateOnly (now) constructor builds a DateOnly value`` () =
  let program = "let x = dateOnly::now() in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(PrimitiveValue.Date _) -> Assert.Pass $"Correctly evaluated to the current date"
    | _ -> Assert.Fail $"Expected the current date, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration DateOnly (utcNow) constructor builds a DateOnly value`` () =
  let program = "let x = dateOnly::utcNow() in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(PrimitiveValue.Date _) -> Assert.Pass $"Correctly evaluated to the current utc date"
    | _ -> Assert.Fail $"Expected the current utc date, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"


[<Test>]
let ``LangNext-Integration DateOnly constructor fails with an error if the string is not a valid date`` () =
  let program = "let x = dateOnly::new(\"GHSBR\") in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Unit)) -> Assert.Pass()
    | _ -> Assert.Fail $"Expected an error, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration DateTime constructor builds a DateTime value`` () =
  let program = "let x = dateTime::new(\"2021-02-01T12:00:00\") in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Sum({ Case = 0; Count = 1 }, Value.Primitive(PrimitiveValue.DateTime(d))) when
      d = System.DateTime(2021, 2, 1, 12, 0, 0)
      ->
      Assert.Pass $"Correctly evaluated to 2021-02-01"
    | _ -> Assert.Fail $"Expected 2021-02-01, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration DateTime (now) constructor builds a DateTime value`` () =
  let program = "let x = dateTime::now() in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(PrimitiveValue.DateTime _) -> Assert.Pass $"Correctly evaluated to the current date"
    | _ -> Assert.Fail $"Expected the current date, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration DateTime (utcNow) constructor builds a DateTime value`` () =
  let program = "let x = dateTime::utcNow() in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(PrimitiveValue.DateTime _) -> Assert.Pass $"Correctly evaluated to the current utc date"
    | _ -> Assert.Fail $"Expected the current utc date, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"


[<Test>]
let ``LangNext-Integration DateTime constructor fails with an error if the string is not a valid date`` () =
  let program = "let x = dateTime::new(\"GHSBR\") in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Sum({ Case = 1; Count = 1 }, Value.Primitive(PrimitiveValue.Unit)) -> Assert.Pass()
    | _ -> Assert.Fail $"Expected an error, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration Guid constructor builds a Guid value`` () =
  let program = "let x = guid::new(\"123e4567-e89b-12d3-a456-426614174000\") in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Sum({ Case = 2; Count = 2 }, Value.Primitive(PrimitiveValue.Guid(g))) when
      g = System.Guid "123e4567-e89b-12d3-a456-426614174000"
      ->
      Assert.Pass $"Correctly evaluated to 123e4567-e89b-12d3-a456-426614174000"
    | _ -> Assert.Fail $"Expected 123e4567-e89b-12d3-a456-426614174000, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration Guid (v4) constructor builds a Guid value`` () =
  let program = "let x = guid::v4() in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(PrimitiveValue.Guid(g)) when g <> System.Guid.Empty ->
      Assert.Pass $"Correctly evaluated to a new Guid"
    | _ -> Assert.Fail $"Expected a new Guid, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration Guid constructor fails with an error if the string is not a valid guid`` () =
  let program = "let x = guid::new(\"GHSBR\") in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Unit)) -> Assert.Pass()
    | _ -> Assert.Fail $"Expected an error, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration TimeSpan constructor builds a TimeSpan value from "02:00:00"`` () =
  let program = "let x = timeSpan::new(\"02:00:00\") in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Sum({ Case = 2; Count = 2 }, Value.Primitive(PrimitiveValue.TimeSpan(t))) when
      t = System.TimeSpan(0, 2, 0, 0)
      ->
      Assert.Pass $"Correctly evaluated to 02:00:00"
    | _ -> Assert.Fail $"Expected 02:00:00, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration TimeSpan constructor builds a TimeSpan value from "6"`` () =
  let program = "let x = timeSpan::new(\"6\") in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Sum({ Case = 2; Count = 2 }, Value.Primitive(PrimitiveValue.TimeSpan(t))) when
      t = System.TimeSpan(6, 0, 0, 0)
      ->
      Assert.Pass $"Correctly evaluated to 6.00:00:00"
    | _ -> Assert.Fail $"Expected 6.00:00:00, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration TimeSpan constructor builds TimeSpan zero value`` () =
  let program = "let x = timeSpan::zero() in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(PrimitiveValue.TimeSpan(t)) when t = System.TimeSpan.Zero ->
      Assert.Pass $"Correctly evaluated to TimeSpan.Zero"
    | _ -> Assert.Fail $"Expected TimeSpan.Zero, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration TimeSpan constructor fails with an error if the string is not a valid time span`` () =
  let program = "let x = timeSpan::new(\"GHSBR\") in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Unit)) -> Assert.Pass()
    | _ -> Assert.Fail $"Expected an error, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration TimeSpan constructor fails with an error if the parsed value overflows`` () =
  let program = "let x = timeSpan::new(\"6:34:14:45\") in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Unit)) -> Assert.Pass()
    | _ -> Assert.Fail $"Expected an error, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration TimeSpan constructor fails with an error if value has a wrong format`` () =
  let program = "let x = timeSpan::new(\"6:12:14:45,3448\") in x"

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Sum({ Case = 1; Count = 2 }, Value.Primitive(PrimitiveValue.Unit)) -> Assert.Pass()
    | _ -> Assert.Fail $"Expected an error, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"


[<Test>]
let ``LangNext-Integration record construction inside a sum match expression constructs correctly`` () =
  let program =
    """
type R = {
  X: bool;
  Y: string;
}
in let cons = List::Cons [R]
in let nil = List::Nil [R] ()
in let (_:int32+string) = 2Of2 ""
in match _ with
  | 1Of2 (_ -> nil)
  | 2Of2 (_ -> cons ({ X = true; Y = "ABC"; }, nil))
      """


  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Ext(ValueExt.ValueExt(Choice1Of6(ListExt.ListValues(ListValues.List([ Value.Record _ ])))), None) ->
      Assert.Pass $"Correctly evaluated to a record"
    | _ -> Assert.Fail $"Expected a record, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"


[<Test>]
let ``LangNext-Integration record construction inside a union match expression constructs correctly`` () =
  let program =
    """
type R = {
  X: bool;
  Y: string;
}
in type T = | A of int32 | B of string
in let cons = List::Cons [R]
in let nil = List::Nil [R] ()
in let (_:T) = B ""
in match _ with
  | A (_ -> nil)
  | B (_ -> cons ({ X = true; Y = "ABC"; }, nil))
      """


  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Ext(ValueExt.ValueExt(Choice1Of6(ListExt.ListValues(ListValues.List([ Value.Record _ ])))), None) ->
      Assert.Pass $"Correctly evaluated to a record"
    | _ -> Assert.Fail $"Expected a record, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"


[<Test>]
let ``LangNext-Integration record destruction is used as a type hint according to scope`` () =
  let program =
    """
type T = { A:int32; B:int32; C:int32; }
in let f = fun x -> x.A + x.B + x.C
in f { A=10; B=20; C=30; }
      """


  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(PrimitiveValue.Int32 60) -> Assert.Pass $"Correctly evaluated to 60"
    | _ -> Assert.Fail $"Expected 60, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"


[<Test>]
let ``LangNext-Integration record destruction type hint unifies and fails accordingly`` () =
  let program =
    """
type T = { A:int32; B:int32; C:int32; }
in type U = { A:int32; B:int32; }
in let f = fun x -> x.A + x.B + x.C
in f, f { A=10; B=20; C=30; }
      """


  let actual = program |> run

  match actual with
  | Left _ -> Assert.Fail $"Expected typechecking error"
  | Right _ -> Assert.Pass()


[<Test>]
let ``LangNext-Integration union destruction is used as a type hint according to scope`` () =
  let program =
    """
type T = | A of int32 | B of int32 | C of int32
in let f = fun x -> match x with | A (v -> v) | B (v -> v) | C (v -> v)
in (f (A 10)) + (f (B 20)) + (f (C 30))
      """


  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(PrimitiveValue.Int32 60) -> Assert.Pass $"Correctly evaluated to 60"
    | _ -> Assert.Fail $"Expected 60, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"


[<Test>]
let ``LangNext-Integration union destruction type hint unifies and fails accordingly`` () =
  let program =
    """
type T = | A of int32 | B of int32 | C of int32
in type U = | A of int32 | B of int32
in let f = fun x -> match x with | A (v -> v) | B (v -> v) | C (v -> v)
in f (A 10), f (B 20), f (C 30)
      """


  let actual = program |> run

  match actual with
  | Left _ -> Assert.Fail $"Expected typechecking error"
  | Right _ -> Assert.Pass()


[<Test>]
let ``LangNext-Integration type lambda and application over lists succeds`` () =
  let program =
    """
let singleton = fun [a:*] (x:a) -> List::Cons [a] (x, List::Nil [a] ())
in singleton [int32] 10, singleton [bool] true
      """


  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Tuple [ Value.Ext(ValueExt.ValueExt(Choice1Of6(ListExt.ListValues(ListValues.List([ Value.Primitive(PrimitiveValue.Int32 10) ])))),
                              None)
                    Value.Ext(ValueExt.ValueExt(Choice1Of6(ListExt.ListValues(ListValues.List([ Value.Primitive(PrimitiveValue.Bool true) ])))),
                              None) ] -> Assert.Pass $"Correctly evaluated to (10, true)"
    | _ -> Assert.Fail $"Expected a record, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration type lambda and application over lists with implicit type parameter application succeds``
  ()
  =
  let program =
    """
let singleton = fun [a:*] (x:a) -> List::Cons [a] (x, List::Nil [a] ())
in singleton 10, singleton true
      """


  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Tuple [ Value.Ext(ValueExt.ValueExt(Choice1Of6(ListExt.ListValues(ListValues.List([ Value.Primitive(PrimitiveValue.Int32 10) ])))),
                              None)
                    Value.Ext(ValueExt.ValueExt(Choice1Of6(ListExt.ListValues(ListValues.List([ Value.Primitive(PrimitiveValue.Bool true) ])))),
                              None) ] -> Assert.Pass $"Correctly evaluated to (10, true)"
    | _ -> Assert.Fail $"Expected a record, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"



[<Test>]
let ``LangNext-Integration type lambda and application over tuples with explicit type parameter application succeds``
  ()
  =
  let program =
    """
let pair = fun [a:*] [b:*] (x:a) (y:b) -> (x,y)
in pair [int32] [bool] 10 true
      """


  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Tuple [ Value.Primitive(PrimitiveValue.Int32 10); Value.Primitive(PrimitiveValue.Bool true) ] ->
      Assert.Pass $"Correctly evaluated to (10, true)"
    | _ -> Assert.Fail $"Expected a record, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"


[<Test>]
let ``LangNext-Integration type lambda with record body succeeds`` () =
  let program =
    """
type Countainer = [a:*] -> { Value:a; Count:int32; }
in let getCount = fun [a:*] (x:Countainer[a]) -> x.Count
in let f = fun (x:Countainer[int32]) -> x.Value + x.Count
in let x:Countainer[int32] = { Value = 100; Count = 10}
in let y:Countainer[bool] = { Value = false; Count = 11}
in f x, getCount x, getCount y
      """


  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Tuple [ Value.Primitive(PrimitiveValue.Int32 110)
                    Value.Primitive(PrimitiveValue.Int32 10)
                    Value.Primitive(PrimitiveValue.Int32 11) ] -> Assert.Pass $"Correctly evaluated to (110, 10, 11)"
    | _ -> Assert.Fail $"Expected a record, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNextIntegrationTypeValueIsAugmentedWithSourceScopeInfo`` () =
  let program =
    """
type OCREvidence = {
  firstCellIndex: int32;
  lastCellIndex: int32;
}

in type SomethingWithEvidence = {
  evidence: OCREvidence;
  value: int32;
}

in let id = fun [a:*] (x:a) -> x
in id [SomethingWithEvidence]
      """

  let actual = program |> run

  match actual with
  | Left(value, typeValue: TypeValue<ValueExt>) ->
    match value with
    | Value.Lambda _ ->
      match typeValue with
      | TypeValue.Arrow({ value = (_, outputType) }) ->
        match outputType with
        | TypeValue.Record { value = fields: OrderedMap<TypeSymbol, (TypeValue<ValueExt> * Kind)>
                             typeCheckScopeSource = scope: TypeCheckScope } ->
          match scope.Type with
          | Some "SomethingWithEvidence" ->
            // keep only the name of the type symbol, so we can look it up without guid issues
            let fields =
              fields
              |> OrderedMap.toList
              |> List.map (fun (sym, v) -> sym.Name.ToString(), v)
              |> Map.ofList

            // the scope of the evidence type value should be its own (so should take precedence over the outer scope)
            match fields.TryFind "evidence" with
            | Some(evidenceTypeValue, _) ->
              match evidenceTypeValue with
              | TypeValue.Record { typeCheckScopeSource = evidenceScope: TypeCheckScope } ->
                match evidenceScope.Type with
                | Some "OCREvidence" -> Assert.Pass $"Correctly evaluated"
                | _ -> Assert.Fail $"Expected OCREvidence, got {evidenceScope.Type}"
              | _ -> Assert.Fail $"Expected a record, got {evidenceTypeValue}"
            | _ -> Assert.Fail $"Expected evidence field to be there, got {fields}"
          | _ -> Assert.Fail $"Expected SomethingWithEvidence, got {scope.Type}"
        | _ -> Assert.Fail $"Expected a record, got {outputType}"
      | _ -> Assert.Fail $"Expected an arrow, got {typeValue}"

      Assert.Pass $"Correctly evaluated"
    | _ -> Assert.Fail $"Expected a lambda, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration map set, empty, map operations succeed`` () =
  let program =
    """
let empty = Map::Empty [string][int32] ()
in let m = Map::set [string][int32] ("key1", 1) empty
in let n = Map::set [string][int32] ("key2", 2) m
in Map::map (fun (v:int32) -> v + 100) n
      """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Ext(ValueExt.ValueExt(Choice6Of6(MapExt.MapValues(MapValues.Map(map)))), _) ->
      let mapList = map |> Map.toList

      match mapList with
      | [ (Value.Primitive(PrimitiveValue.String "key1"), Value.Primitive(PrimitiveValue.Int32 101))
          (Value.Primitive(PrimitiveValue.String "key2"), Value.Primitive(PrimitiveValue.Int32 102)) ]
      | [ (Value.Primitive(PrimitiveValue.String "key2"), Value.Primitive(PrimitiveValue.Int32 102))
          (Value.Primitive(PrimitiveValue.String "key1"), Value.Primitive(PrimitiveValue.Int32 101)) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected map with key1->101 and key2->102, got {mapList}"
    | _ -> Assert.Fail $"Expected a map, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

[<Test>]
let ``LangNext-Integration map maptoList operation succeeds`` () =
  let program =
    """
let empty = Map::Empty [string][int32] ()
in let m = Map::set [string][int32] ("key1", 1) (Map::set [string][int32] ("key2", 2) empty)
in Map::maptoList [string][int32] m
      """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Ext(ValueExt.ValueExt(Choice1Of6(ListExt.ListValues(ListValues.List(listValues)))), None) ->
      let tuple1 =
        Value.Tuple
          [ Value.Primitive(PrimitiveValue.String "key1")
            Value.Primitive(PrimitiveValue.Int32 1) ]

      let tuple2 =
        Value.Tuple
          [ Value.Primitive(PrimitiveValue.String "key2")
            Value.Primitive(PrimitiveValue.Int32 2) ]

      match listValues with
      | [ v1; v2 ] when (v1 = tuple1 && v2 = tuple2) || (v1 = tuple2 && v2 = tuple1) -> Assert.Pass()
      | _ -> Assert.Fail $"Expected list with [(key1, 1), (key2, 2)] in any order, got {listValues}"
    | _ -> Assert.Fail $"Expected a list, got {value}"
  | Right e -> Assert.Fail $"Run failed: {e.AsFSharpString}"

(*
List size = 200 -------------------
BEFORE PARSING TOKENS                                                                   

Time taken: 00:00:00.1940570

PARSING TOKENS OK

Time taken: 00:00:10.6251000

PARSING PROGRAM OK
BEFORE TYPE CHECKING                                                                                            

Time taken: 00:00:00.4527620

List size = 400 --------------------
BEFORE PARSING TOKENS                 

Time taken: 00:00:00.4582300

PARSING TOKENS OK

Time taken: 00:00:29.0341010

PARSING PROGRAM OK
BEFORE TYPE CHECKING                

Time taken: 00:00:01.931802

List size = 800 --------------------
BEFORE PARSING TOKENS                                                                   

Time taken: 00:00:01.2275040

PARSING TOKENS OK

Time taken: 00:01:31.0587370

PARSING PROGRAM OK                                                                      
BEFORE TYPE CHECKING                                                                                       

Time taken: 00:00:10.2837940

List size = 1600 --------------

BEFORE PARSING TOKENS                                                                                         

Time taken: 00:00:03.9334170

PARSING TOKENS OK

Time taken: 00:05:13.1547770

PARSING PROGRAM OK
BEFORE TYPE CHECKING                                                                                        

Time taken: 00:01:07.7189350
*)

(*
Stackless monads

List size = 200
BEFORE PARSING TOKENS                                                                                          

Time taken: 00:00:00.2684860

PARSING TOKENS OK
BEFORE PARSING PROGRAM                                                                                         

Time taken: 00:00:06.2997870

PARSING PROGRAM OK
BEFORE TYPE CHECKING                                                                                          

Time taken: 00:00:00.4550920

List size = 400
BEFORE PARSING TOKENS                                                                                          

Time taken: 00:00:00.5031170

PARSING TOKENS OK

Time taken: 00:00:11.9504680

PARSING PROGRAM OK
BEFORE TYPE CHECKING                                                                                          

Time taken: 00:00:01.3342000

List size = 800
BEFORE PARSING TOKENS                                                                                          

Time taken: 00:00:01.0997350

PARSING TOKENS OK

Time taken: 00:00:22.9022750

PARSING PROGRAM OK
BEFORE TYPE CHECKING                                                                                          

Time taken: 00:00:05.3398620

List size = 1600
BEFORE PARSING TOKENS                                                                                          

Time taken: 00:00:03.0137700

PARSING TOKENS OK

Time taken: 00:00:45.9683190

PARSING PROGRAM OK
BEFORE TYPE CHECKING                                                                                          

Time taken: 00:00:23.6824260

List size = 3200
BEFORE PARSING TOKENS                                                                                            

Time taken: 00:00:09.0641730

PARSING TOKENS OK

Time taken: 00:01:30.5152150

PARSING PROGRAM OK
BEFORE TYPE CHECKING                                                                                          

Time taken: 00:01:57.1312520
*)

[<Test>]
let ``StackOverflow`` () =
  let expectedLength = 300

  let listProgram =
    [ 1..expectedLength ]
    |> List.rev
    |> List.fold (fun acc elem -> $"List::Cons [int32] ({elem}, {acc})") "List::Nil [int32] ()"

  let program = sprintf "let l = %s in List::length [int32] l" listProgram

  match program |> run with
  | Left(value, typeValue) ->
    match typeValue with
    | TypeValue.Primitive { value = PrimitiveType.Int32 } ->
      match value with
      | Value.Primitive(Int32 length) -> Assert.That(length, Is.EqualTo(expectedLength))
      | _ -> Assert.Fail $"Type was correct (Int32), but value was not an int. Got value: {value.AsFSharpString}"
    | _ -> Assert.Fail $"Expected type to be Int32, but got: {typeValue.AsFSharpString}. Value: {value.AsFSharpString}"
  | Right e -> Assert.Fail $"Run failed (possibly due to stack overflow). Error: {e.AsFSharpString}"
