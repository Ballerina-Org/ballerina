namespace Ballerina.DSL.Next.Runners

[<AutoOpen>]
module TypeCheck =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
  open System
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.AdHocPolymorphicOperators
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Types.TypeChecker.Primitive
  open Ballerina.DSL.Next.Types.TypeChecker.Lookup
  open Ballerina.DSL.Next.Types.TypeChecker.Lambda
  open Ballerina.DSL.Next.Types.TypeChecker.Apply
  open Ballerina.DSL.Next.Types.TypeChecker.If
  open Ballerina.DSL.Next.Types.TypeChecker.Let
  open Ballerina.DSL.Next.Types.TypeChecker.RecordCons
  open Ballerina.DSL.Next.Types.TypeChecker.RecordWith
  open Ballerina.DSL.Next.Types.TypeChecker.RecordDes
  open Ballerina.DSL.Next.Types.TypeChecker.UnionDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumCons
  open Ballerina.DSL.Next.Types.TypeChecker.TupleDes
  open Ballerina.DSL.Next.Types.TypeChecker.TupleCons
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLambda
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLet
  open Ballerina.DSL.Next.Types.TypeChecker.TypeApply
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.Parser
  open Ballerina.DSL.Next.Syntax

  type Expr<'T, 'Id when 'Id: comparison> with
    static member TypeCheckString (languageContext: LanguageContext<'ValueExt>) (program: string) =
      let initialLocation = Location.Initial "input"
      let actual = tokens |> Parser.Run(program |> Seq.toList, initialLocation)

      match actual with
      | Right(e, _) -> Right e
      | Left(ParserResult(actual, _)) ->
        let parsed = Parser.Expr.program |> Parser.Run(actual, initialLocation)

        match parsed with
        | Right(e, _) -> Right e
        | Left(ParserResult(program, _)) ->
          let typeCheckResult =
            Expr.TypeCheck None program
            |> State.Run(languageContext.TypeCheckContext, languageContext.TypeCheckState)

          match typeCheckResult with
          | Right(e, _) -> Right e
          | Left((program, programType, _), typeCheckFinalState) ->

            let typeCheckState =
              Option.defaultValue languageContext.TypeCheckState typeCheckFinalState

            Left(program, programType, typeCheckState)
