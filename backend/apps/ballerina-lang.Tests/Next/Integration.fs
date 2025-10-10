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

        let unionCaseConstructors =
          match typeCheckFinalState with
          | None -> []
          | Some s ->
            s.Types.UnionCases
            |> Map.map (fun k _ ->
              Value<TypeValue, ValueExt>
                .Lambda(
                  "_" |> Var.Create,
                  Expr.UnionCons(k, ("_" |> Identifier.LocalScope, Location.Unknown) |> Expr.Lookup, Location.Unknown),
                  Map.empty
                ))
            |> Map.toList

        let evalContext =
          { evalContext with
              Symbols = ExprEvalContextSymbols.Append evalContext.Symbols typeCheckedSymbols
              // Values: Map<Identifier, Value<TypeValue, 'valueExtension>>
              Values = (evalContext.Values |> Map.toList) @ unionCaseConstructors |> Map.ofList }

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
let ``LangNext-Integration same record and union type and field names disambiguate succesfully`` () =
  let program =
    """
type A = {
  A: int32;
  B: string;
  C: bool;
}

in type A =
  | A of int32
  | B of string
  | C of bool

in let a1 = { A=10; B="hello"; C=true; }
in let a2 = A(10)
in let a3 = { a1 with B="hello world"; }

in let f = fun x -> x.A + 10
in let x = f a3

in x
  """

  let actual = program |> run

  match actual with
  | Left(value, _typeValue) ->
    match value with
    | Value.Primitive(Int32 20) -> Assert.Pass()
    | _ -> Assert.Fail($"Expected 20, got {value}")

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
