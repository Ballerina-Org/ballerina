module Playground

open Ballerina.Collections.Option
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
open Ballerina.Lenses
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.StdLib.List
open Ballerina.DSL.Next.StdLib.Option
open Ballerina.DSL.Next.StdLib.Int32
open Ballerina.DSL.Next.StdLib.Int64
open Ballerina.DSL.Next.StdLib.Float32
open Ballerina.DSL.Next.StdLib.Float64
open Ballerina.DSL.Next.StdLib.Decimal
open Ballerina.DSL.Next.StdLib.DateOnly
open Ballerina.DSL.Next.StdLib.DateTime
open Ballerina.DSL.Next.StdLib.String
open Ballerina.DSL.Next.StdLib.Guid
open Ballerina.DSL.Next.StdLib.Bool
open Ballerina.DSL.Next.Terms.Eval
open Ballerina.State.WithError
open Ballerina.LocalizedErrors
open Ballerina.Parser
open Ballerina.StdLib.Object
open Ballerina.DSL.Next.Syntax

let private (!) = Identifier.LocalScope
let private (=>) t f = Identifier.FullyQualified([ t ], f)
let private (!!) = Identifier.LocalScope >> TypeExpr.Lookup
let private (=>>) = Identifier.FullyQualified >> TypeExpr.Lookup
do ignore (!)
do ignore (=>)
do ignore (!!)
do ignore (=>>)

type ValueExt =
  | ValueExt of Choice<ListExt, OptionExt, PrimitiveExt>

  static member Getters = {| ValueExt = fun (ValueExt e) -> e |}
  static member Updaters = {| ValueExt = fun u (ValueExt e) -> ValueExt(u e) |}

and ListExt =
  | ListOperations of ListOperations<ValueExt>
  | ListValues of ListValues<ValueExt>

and OptionExt =
  | OptionOperations of OptionOperations<ValueExt>
  | OptionValues of OptionValues<ValueExt>
  | OptionConstructors of OptionConstructors

and PrimitiveExt =
  | BoolOperations of BoolOperations<ValueExt>
  | Int32Operations of Int32Operations<ValueExt>
  | Int64Operations of Int64Operations<ValueExt>
  | Float32Operations of Float32Operations<ValueExt>
  | Float64Operations of Float64Operations<ValueExt>
  | DecimalOperations of DecimalOperations<ValueExt>
  | DateOnlyOperations of DateOnlyOperations<ValueExt>
  | DateTimeOperations of DateTimeOperations<ValueExt>
  | StringOperations of StringOperations<ValueExt>
  | GuidOperations of GuidOperations<ValueExt>

let listExtension =
  ListExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice1Of3(ListValues x) -> Some x
        | _ -> None)
      Set = ListValues >> Choice1Of3 >> ValueExt.ValueExt }
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice1Of3(ListOperations x) -> Some x
        | _ -> None)
      Set = ListOperations >> Choice1Of3 >> ValueExt.ValueExt }


let boolExtension =
  BoolExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(BoolOperations x) -> Some x
        | _ -> None)
      Set = BoolOperations >> Choice3Of3 >> ValueExt.ValueExt }

let int32Extension =
  Int32Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(Int32Operations x) -> Some x
        | _ -> None)
      Set = Int32Operations >> Choice3Of3 >> ValueExt.ValueExt }

let int64Extension =
  Int64Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(Int64Operations x) -> Some x
        | _ -> None)
      Set = Int64Operations >> Choice3Of3 >> ValueExt.ValueExt }

let float32Extension =
  Float32Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(Float32Operations x) -> Some x
        | _ -> None)
      Set = Float32Operations >> Choice3Of3 >> ValueExt.ValueExt }

let float64Extension =
  Float64Extension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(Float64Operations x) -> Some x
        | _ -> None)
      Set = Float64Operations >> Choice3Of3 >> ValueExt.ValueExt }

let decimalExtension =
  DecimalExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(DecimalOperations x) -> Some x
        | _ -> None)
      Set = DecimalOperations >> Choice3Of3 >> ValueExt.ValueExt }

let dateOnlyExtension =
  DateOnlyExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(DateOnlyOperations x) -> Some x
        | _ -> None)
      Set = DateOnlyOperations >> Choice3Of3 >> ValueExt.ValueExt }

let dateTimeExtension =
  DateTimeExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(DateTimeOperations x) -> Some x
        | _ -> None)
      Set = DateTimeOperations >> Choice3Of3 >> ValueExt.ValueExt }

let stringExtension =
  StringExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(StringOperations x) -> Some x
        | _ -> None)
      Set = StringOperations >> Choice3Of3 >> ValueExt.ValueExt }

let guidExtension =
  GuidExtension<ValueExt>
    { Get =
        ValueExt.Getters.ValueExt
        >> (function
        | Choice3Of3(GuidOperations x) -> Some x
        | _ -> None)
      Set = GuidOperations >> Choice3Of3 >> ValueExt.ValueExt }

let context =
  LanguageContext<ValueExt>.Empty
  |> (listExtension |> TypeExtension.RegisterLanguageContext)
  |> (boolExtension |> OperationsExtension.RegisterLanguageContext)
  |> (int32Extension |> OperationsExtension.RegisterLanguageContext)
  |> (int64Extension |> OperationsExtension.RegisterLanguageContext)
  |> (float32Extension |> OperationsExtension.RegisterLanguageContext)
  |> (float64Extension |> OperationsExtension.RegisterLanguageContext)
  |> (decimalExtension |> OperationsExtension.RegisterLanguageContext)
  |> (dateOnlyExtension |> OperationsExtension.RegisterLanguageContext)
  |> (dateTimeExtension |> OperationsExtension.RegisterLanguageContext)
  |> (stringExtension |> OperationsExtension.RegisterLanguageContext)
  |> (guidExtension |> OperationsExtension.RegisterLanguageContext)

[<EntryPoint>]
let main _args =

  let programs =
    [ """
let x = false || !true && false
in x
""" ]

  // test 1
  // let x = 10
  // in x

  // test 2
  // let x = 10.5
  // in x

  // test 3
  // let x = true
  // in x

  // test 4
  // false

  // test N
  // type ERP =
  //   | BC of ()
  //   | FandO of ()
  //   | SAP of ()

  // in type GlobalConfig = {
  //   SuperAdmin: bool;
  //   TenantERP: ERP;
  //   TenantID: string;
  // }

  // in type FieldsToFilter =
  //   | CommonField1 of ()
  //   | CommonField2 of ()
  //   | FieldForSuperAdmins of ()
  //   | FieldForSAP of ()
  //   | FieldForBC of ()

  // in let id = fun (x:GlobalConfig) -> x

  // in let config = {
  //   SuperAdmin=false;
  //   TenantERP=SAP();
  //   TenantID="12345678-1234-1234-1234-123456789012";
  // }

  // in let config = id config

  // in let cons = List::Cons [FieldsToFilter]
  // in let nil = List::Nil [FieldsToFilter] ()
  // in let fieldsToFilter = cons (CommonField1(), cons (CommonField2(), cons (FieldForSuperAdmins(), cons (FieldForSAP(), cons(FieldForBC(), nil)))))

  // in let filteringLogic = fun field ->
  //         (match field with
  //         | CommonField1 (_ -> true)
  //         | CommonField2 (_ -> true)
  //         | FieldForSuperAdmins (_ -> config.SuperAdmin)
  //         | FieldForSAP (_ -> match config.TenantERP with | SAP (_ -> true) | BC (_ -> config.SuperAdmin) | FandO (_ -> config.SuperAdmin))
  //         | FieldForBC (_ -> match config.TenantERP with | BC (_ -> true) | SAP (_ -> config.SuperAdmin) | FandO (_ -> config.SuperAdmin))
  //         )

  // in List::filter[FieldsToFilter]
  //       filteringLogic
  //       fieldsToFilter




  for program in programs do
    let initialLocation = Location.Initial "input"
    let parserStopwatch = System.Diagnostics.Stopwatch.StartNew()
    let actual = tokens |> Parser.Run(program |> Seq.toList, initialLocation)
    do parserStopwatch.Stop()

    match actual with
    | Right e -> Console.WriteLine $"Failed to tokenize program: {e.ToFSharpString}"
    | Left(ParserResult(actual, _)) ->
      for t in actual do
        do Console.Write $"{t.Token.ToString()} "

      do Console.WriteLine()

      do parserStopwatch.Start()
      let parsed = Parser.program |> Parser.Run(actual, initialLocation)
      do parserStopwatch.Stop()

      match parsed with
      | Right e -> Console.WriteLine $"Failed to parse program: {e.ToFSharpString}"
      | Left(ParserResult(program, _)) ->

        do Console.WriteLine $"Parsed program:\n{program.ToFSharpString}\n"
        do Console.ReadLine() |> ignore

        let typeCheckerStopwatch = System.Diagnostics.Stopwatch.StartNew()

        let typeCheckResult =
          Expr.TypeCheck program
          |> State.Run(context.TypeCheckContext, context.TypeCheckState)

        do typeCheckerStopwatch.Stop()

        match typeCheckResult with
        | Left((program, typeValue, _), typeCheckFinalState) ->
          Console.WriteLine $"Type checking succeeded: {typeValue.ToFSharpString}"
          // Console.WriteLine $"Type checked program: {program.ToFSharpString}"

          let evalStopwatch = System.Diagnostics.Stopwatch.StartNew()
          let evalContext = context.ExprEvalContext

          let typeCheckedSymbols =
            match typeCheckFinalState with
            | None -> []
            | Some s -> s.Types.Symbols |> Map.toList

          let unionCaseConstructors =
            match typeCheckFinalState with
            | None -> []
            | Some s ->
              s.Types.UnionCases
              |> Map.map (fun k _ ->
                Value<TypeValue, ValueExt>.Lambda("_" |> Var.Create, Expr.UnionCons(k, !"_" |> Expr.Lookup)))
              |> Map.toList

          let evalContext =
            { evalContext with
                Symbols = (evalContext.Symbols |> Map.toList) @ typeCheckedSymbols |> Map.ofList
                // Values: Map<Identifier, Value<TypeValue, 'valueExtension>>
                Values = (evalContext.Values |> Map.toList) @ unionCaseConstructors |> Map.ofList }

          let evalResult = Expr.Eval program |> Reader.Run evalContext
          do evalStopwatch.Stop()

          match evalResult with
          | Left value ->
            do
              Console.WriteLine
                $"Timing: \n  parsing {parserStopwatch.ElapsedMilliseconds}ms\n  type checking {typeCheckerStopwatch.ElapsedMilliseconds}ms\n  evaluation {evalStopwatch.ElapsedMilliseconds}ms\nand resulted in {value.ToFSharpString}"
          | Right e -> Console.WriteLine $"Evaluation failed: {e.ToFSharpString}"
        | Right(e, _) -> Console.WriteLine $"Type checking failed: {e.ToFSharpString}"

      do Console.WriteLine()

  0
