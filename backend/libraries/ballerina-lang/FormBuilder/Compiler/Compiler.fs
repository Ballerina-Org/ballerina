namespace Ballerina.DSL.FormBuilder.Compiler

module FormCompiler =
  open System
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Parser
  open Ballerina.DSL.FormBuilder.Syntax.Parser
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.Runners
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
              Errors.Singleton Location.Unknown (fun () -> $"Expected type let but {expr.Expr} was given")
              |> Errors.MapPriority(replaceWith ErrorPriority.High)
            )
      }

    memoizeTypeRec expr Map.empty

  let deleteFileIfExists (file: string) =
    if File.Exists file then
      File.Delete file

  let compileForms<'valueExt when 'valueExt: comparison>
    (input: FormCompilerInput<'valueExt>)
    (languageContext: LanguageContext<'valueExt>)
    (stdExtensions: StdExtensions<'valueExt>)
    =
    sum {
      let formsInitialLocation = Location.Initial input.Forms.Source

      // Append "in ()" if the types program doesn't already end with it
      let typesProgram =
        let trimmed = input.Types.Program.TrimEnd()

        if trimmed.EndsWith("in ()") || trimmed.EndsWith("in()") then
          input.Types.Program
        else
          $"{input.Types.Program}\nin ()"

      let! types, _, typeCheckState = Expr.TypeCheckString languageContext typesProgram |> Sum.mapRight _.ToString()

      // lexing
      let! ParserResult(formTokens, _) =
        Ballerina.DSL.FormBuilder.Syntax.Lexer.tokens
        |> Parser.Run(input.Forms.Program |> Seq.toList, formsInitialLocation)
        |> Sum.mapRight _.ToString()

      // parsing
      let! ParserResult(formDefinitions, _) =
        parseFormSpec ()
        |> Parser.Run(formTokens, formsInitialLocation)
        |> Sum.mapRight _.ToString()

      // to check if it has parsed correctly:   
      //Console.WriteLine("Parsed form definitions: " + formDefinitions.ToString())
 
      let! memoizedTypes = memoizeTypes types |> Sum.mapRight _.ToString()
      let formTypeCheckState = FormTypeCheckerState<'valueExt>.Init typeCheckState

      let formTypeCheckContext =
        FormTypeCheckingContext<ValueExt>.Init memoizedTypes languageContext.TypeCheckContext input.ApiTypes

      let! typeCheckedFormDefinitions, _ =
        checkFormDefinitions formDefinitions stdExtensions
        |> State.Run(formTypeCheckContext, formTypeCheckState)
        |> Sum.mapRight (fun (errors, _) -> Errors.ToString(errors, "\n"))

      return typeCheckedFormDefinitions
    }
