module Ballerina.DTO.Tests

open NUnit.Framework
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.Terms.Eval
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.Errors
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Runners
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.Serialization.ValueSerializer
open Ballerina.DSL.Next.Serialization.ValueDeserializer
open Ballerina.DSL.Next.StdLib.MutableMemoryDB
open Ballerina.DSL.Next.StdLib.String

let _, languageContext, query_type_sym, mk_query_type =
  stdExtensions<unit, MutableMemoryDB<unit, unit>> (StringTypeClass<_>.Console()) (db_ops ())

let primitive (v: string) =
  $"""
let x = {v}
in x
"""

let testInt = primitive "5"
let testInt64 = primitive "5l"
let testFloat = primitive "2.256f"
let testFloat64 = primitive "3.52d"
let testDecimal = primitive "3.52"
let testBool = primitive "true"
let testGuid = primitive "guid::v4()"
let testString = primitive $"\"hello world!\""
let testDateTime = primitive "dateTime::utcNow()"
let testDateOnly = primitive "dateOnly::utcNow()"
let testTimeSpan = primitive "timeSpan::fromSeconds 3.5"
let testUnit = primitive "()"
let testTuple = primitive "3, 2.5"

let testSum =
  """
  let s1 = 1Of3 5
  in s1
"""

let testRecord =
  """
type R = {
  A : int32;
  B : decimal;
  C : string;
  D : guid;
}
in let r : R = { 
  D = guid::v4(); 
  B = 4.22; 
  C = "Hi!"; 
  A = 5; 
}
in r
"""

let testUnion =
  """
type U =
| A of int32
| B of decimal
| C of float64
in let a : U = A 5
in let b : U = B 3.45676
in let c : U = C 4.3343d
in a, b, c
"""

let testEmptyList =
  """
let l = List::Nil()
in l
"""

let testList =
  """
let l = List::Cons(3, List::Cons(-6, List::Cons(22, List::Nil())))
in l
"""

let testComplexType =
  """
type U1 =
| A of int32
| B of decimal
| C of float64

in type U2 =
| A of guid
| B of string
| C of U1

in type R1 = {
  A : List [U1];
  B : guid;
  C : string + decimal;
}

in type R2 = {
  A : U2;
  B : R1;
}

in let r1 : R1 = { 
  C = 1Of2 "Hi!";
  B = guid::v4();
  A = List::Cons(U1::A 5, List::Cons(U1::B 3.15, List::Cons(U1::C 4.5d, List::Nil())));
}
in let r2 : R2 = {
  B = r1;
  A = U2::C (U1::B 3.11525);
}
in r2
"""

let build_cache =
  memcache (languageContext.TypeCheckContext, languageContext.TypeCheckState)

let typeCheckProgram (programName: string) (program: string) =
  sum {
    let project: ProjectBuildConfiguration =
      { Files = NonEmptyList.OfList(FileBuildConfiguration.FromFile($"{programName}.bl", program), []) }

    return! ProjectBuildConfiguration.BuildCached query_type_sym mk_query_type build_cache project
  }

let runProgram expr exprs (st: TypeCheckState<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>>) =
  sum {
    let evalContext = ExprEvalContext.Empty()

    let evalContext =
      ExprEvalContext.WithTypeCheckingSymbols (evalContext |> languageContext.ExprEvalContext) st.Symbols

    return!
      Expr.Eval(NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(expr, exprs)))
      |> Reader.Run evalContext
  }

let buildAndRunProgram (programName: string) (program: string) =
  sum {
    let! NonEmptyList(expr, exprs), _, _, (st: TypeCheckState<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>>) =
      typeCheckProgram programName program

    return! runProgram expr exprs st
  }

let roundtripConversion
  (value:
    Value<
      TypeValue<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>>,
      ValueExt<unit, MutableMemoryDB<unit, unit>, unit>
     >)
  =
  sum {
    let! valueDTO =
      valueToDTO value
      |> Reader.Run languageContext.SerializationContext.SerializationContext

    let! json =
      Value.JsonSerializeV2 value
      |> Reader.Run languageContext.SerializationContext.SerializationContext

    let! valueFromDTO =
      valueFromDTO valueDTO
      |> Reader.Run languageContext.SerializationContext.SerializationContext

    return value, json, valueFromDTO
  }

let runProgramAndConvert (programName: string) (program: string) =
  sum {
    let! value = buildAndRunProgram programName program

    return!
      roundtripConversion value
      |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
  }

let primitiveConversions =
  [ "int32-conversion", testInt
    "int64-conversion", testInt64
    "float-conversion", testFloat
    "float64-conversion", testFloat64
    "decimal-conversion", testDecimal
    "bool-conversion", testBool
    "guid-conversion", testGuid
    "string-conversion", testString
    "date-time-conversion", testDateTime
    "date-only-conversion", testDateOnly
    "time-span-conversion", testTimeSpan
    "unit-conversion", testUnit ]

let convertPrimitives () =
  primitiveConversions
  |> List.map (fun (name, program) -> runProgramAndConvert name program)
  |> sum.All

let checkConversion
  (testCategory: string)
  (conversion:
    Sum<
      Value<
        TypeValue<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>>,
        ValueExt<unit, MutableMemoryDB<unit, unit>, unit>
       > *
      string *
      Value<
        TypeValue<ValueExt<unit, MutableMemoryDB<unit, unit>, unit>>,
        ValueExt<unit, MutableMemoryDB<unit, unit>, unit>
       >,
      Errors<Location>
     >)
  =
  match conversion with
  | Left(givenValue, _, returnedValue) -> Assert.That(givenValue, Is.EqualTo returnedValue)
  | Right errors -> Assert.Fail $"Failed to convert {testCategory}: {errors.ToString()}"


[<Test>]
let ``Convert primitives and assert equality`` () =
  match convertPrimitives () with
  | Left results ->
    for givenValue, _, returnedValue in results do
      Assert.That(givenValue, Is.EqualTo returnedValue)
  | Right errors -> Assert.Fail $"Failed to convert primitives: {errors.ToString()}"

[<Test>]
let ``Convert tuple and assert equality`` () =
  checkConversion "tuple" (runProgramAndConvert "tuple-conversion" testTuple)

[<Test>]
let ``Convert sum and assert equality`` () =
  checkConversion "sum" (runProgramAndConvert "sum-conversion" testSum)

[<Test>]
let ``Convert record and assert equality`` () =
  checkConversion "record" (runProgramAndConvert "record-conversion" testRecord)

[<Test>]
let ``Convert union and assert equality`` () =
  checkConversion "union" (runProgramAndConvert "union-conversion" testUnion)

[<Test>]
let ``Convert empty list and assert equality`` () =
  checkConversion "empty list" (runProgramAndConvert "empty-list-conversion" testEmptyList)

[<Test>]
let ``Convert list and assert equality`` () =
  checkConversion "list" (runProgramAndConvert "list-conversion" testList)

[<Test>]
let ``Convert complex value and assert equality`` () =
  checkConversion "complex value" (runProgramAndConvert "complex-value-conversion" testComplexType)
