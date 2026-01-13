namespace Ballerina.DSL.FormBuilder.Compiler

module FormCompiler =
  open Ballerina.LocalizedErrors
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.Parser
  open Ballerina.DSL.FormBuilder.Syntax.Parser
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.Runners
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.FormBuilder.Types.TypeChecker
  open Ballerina.State.WithError
  open System.IO
  open Ballerina.DSL.Next.Extensions

  type ProgramInput = { Program: string; Source: string }

  type FormCompilerInput<'valueExt when 'valueExt: comparison> =
    { Types: ProgramInput
      ApiTypes: Map<TypeValue<'valueExt>, string * TypeValue<'valueExt>>
      Forms: ProgramInput }

  let memoizeTypes (expr: Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>) =
    let rec memoizeTypeRec
      (expr: Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>)
      (table: Map<string, TypeValue<'valueExt>>)
      =
      sum {
        match expr.Expr with
        | TypeLet typeLet -> return! memoizeTypeRec typeLet.Body (table.Add(typeLet.Name, typeLet.TypeDef))
        | ExprRec.Primitive PrimitiveValue.Unit -> return table
        | _ ->
          return!
            Right(
              Errors.Singleton(Location.Unknown, $"Expected type let but {expr.Expr} was given")
              |> Errors.SetPriority ErrorPriority.High
            )
      }

    memoizeTypeRec expr Map.empty

  let deleteFileIfExists (file: string) =
    if File.Exists file then
      File.Delete file

  let compileForms<'valueExt when 'valueExt: comparison>
    (input: FormCompilerInput<'valueExt>)
    (languageContext: LanguageContext<'valueExt>)
    =
    sum {
      let formsInitialLocation = Location.Initial input.Forms.Source

      let! types, _, typeCheckState =
        Expr.TypeCheckString languageContext input.Types.Program
        |> Sum.mapRight (fun errors -> errors.AsFSharpString)

      let! ParserResult(formTokens, _) =
        Ballerina.DSL.FormBuilder.Syntax.Lexer.tokens
        |> Parser.Run(input.Forms.Program |> Seq.toList, formsInitialLocation)
        |> Sum.mapRight (fun errors -> errors.AsFSharpString)

      let! ParserResult(formDefinitions, _) =
        parseFormSpec ()
        |> Parser.Run(formTokens, formsInitialLocation)
        |> Sum.mapRight (fun errors -> errors.AsFSharpString)

      let! memoizedTypes = memoizeTypes types |> Sum.mapRight (fun errors -> errors.AsFSharpString)
      let formTypeCheckState = FormTypeCheckerState<'valueExt>.Init typeCheckState

      let formTypeCheckContext =
        FormTypeCheckingContext<ValueExt>.Init memoizedTypes languageContext.TypeCheckContext input.ApiTypes

      let! typeCheckedFormDefinitions, _ =
        checkFormDefinitions formDefinitions
        |> State.Run(formTypeCheckContext, formTypeCheckState)
        |> Sum.mapRight (fun (errors, _) -> errors.AsFSharpString)

      return typeCheckedFormDefinitions
    }
