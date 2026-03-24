module DTOTests

open Ballerina
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open System
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.TypeChecker.Eval
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.Terms.Eval
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.State.WithError
open Ballerina.LocalizedErrors
open Ballerina.Errors
open Ballerina.Parser
open Ballerina.StdLib.Object
open Ballerina.DSL.Next.Syntax
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Runners
open Ballerina
open Ballerina.Collections.Option
open Ballerina.StdLib.String
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.Serialization.ValueSerializer
open Ballerina.DSL.Next.Serialization.ValueDeserializer
open Ballerina.Cat.Collections.OrderedMap
open Ballerina.DSL.Next.StdLib.MutableMemoryDB
open Ballerina.DSL.Next.Terms.Patterns
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

let tests =
  [ testInt
    testInt64
    testFloat
    testFloat64
    testDecimal
    testBool
    testGuid
    testString
    testDateTime
    testDateOnly
    testTimeSpan
    testUnit
    testTuple
    testSum
    testRecord
    testUnion
    testEmptyList
    testList
    testComplexType ]

let runTests () =
  tests
  |> List.mapi (fun i test -> runProgramAndConvert $"program_{i}" test)
  |> sum.All

let testFieldIndices () =
  sum {
    let! NonEmptyList(expr, exprs), typeValue, ctx, st = typeCheckProgram "testFieldIndices_complexType" testComplexType
    let! result = runProgram expr exprs st

    let! recordDTO =
      valueToDTO result
      |> Reader.Run languageContext.SerializationContext.SerializationContext
      |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

    let! positions, _ =
      recordDTO.GetRecordFieldPositions typeValue
      |> State.Run(ctx, st)
      |> sum.MapError fst

    return positions
  }

let run () =
  sum {
    let! results = runTests ()
    let! positions = testFieldIndices ()
    return results, positions
  }

[<EntryPoint>]
let main _ =
  match run () with
  | Left(results, positions) ->
    for value, json, valueFromDTO in results do
      Console.WriteLine $"PROGRAM EVALUATION:\n{value}\n\n{json}\n\n{valueFromDTO}\n============================="

    Console.WriteLine($"POSITIONS: {positions}")
    0
  | Right errors ->
    Console.Error.WriteLine $"ERROR: {errors.ToString()}"
    1
