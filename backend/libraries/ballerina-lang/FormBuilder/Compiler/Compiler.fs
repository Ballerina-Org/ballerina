namespace Ballerina.DSL.FormBuilder.Compiler

module FormCompiler =
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
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Collections.Map
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  type ProgramInput =
    { Preludes: NonEmptyList<FileBuildConfiguration>
      Source: string }

  type FormsInput = { Program: string; Source: string }

  type FormCompilerInput<'valueExt when 'valueExt: comparison> =
    { Types: ProgramInput
      ApiTypes: Map<TypeValue<'valueExt>, string * TypeValue<'valueExt>>
      Forms: FormsInput }

  let memoizeTypes (expr: Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>) =
    let rec memoizeTypeRec
      (expr: Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>)
      (table: Map<string, TypeValue<'valueExt>>)
      =
      sum {
        match expr.Expr with
        | TypeLet typeLet -> return! memoizeTypeRec typeLet.Body (table.Add(typeLet.Name, typeLet.TypeDef))
        | ExprRec.Let letExpr -> return! memoizeTypeRec letExpr.Rest table
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

  let compileForms<'runtimeContext, 'db, 'valueExt, 'valueExtDTO, 'deltaExt, 'deltaExtDTO, 'customExtension
    when 'db: comparison
    and 'valueExt: comparison
    and 'valueExtDTO: not null
    and 'valueExtDTO: not struct
    and 'deltaExt: comparison
    and 'customExtension: comparison
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct>
    (input: FormCompilerInput<'valueExt>)
    (cache: ProjectCache<'valueExt>)
    (languageContext: LanguageContext<'runtimeContext, 'valueExt, 'valueExtDTO, 'deltaExt, 'deltaExtDTO>)
    (stdExtensions: StdExtensions<'runtimeContext, 'valueExt, 'valueExtDTO, 'deltaExt, 'deltaExtDTO>)
    (config: TypeEvalConfig<'valueExt>)
    =
    sum {
      let formsInitialLocation = Location.Initial input.Forms.Source

      let! types, _, _, typeCheckState =
        let project: ProjectBuildConfiguration = { Files = input.Types.Preludes }

        ProjectBuildConfiguration.BuildCached config cache project
        |> Sum.mapRight _.ToString()

      let! ParserResult(formTokens, _) =
        Ballerina.DSL.FormBuilder.Syntax.Lexer.tokens
        |> Parser.Run(input.Forms.Program |> Seq.toList, formsInitialLocation)
        |> Sum.mapRight _.ToString()

      let! ParserResult(formDefinitions, _) =
        parseFormSpec ()
        |> Parser.Run(formTokens, formsInitialLocation)
        |> Sum.mapRight _.ToString()

      let! memoizedTypes =
        types
        |> NonEmptyList.ToList
        |> List.map memoizeTypes
        |> sum.All
        |> sum.Map(Map.mergeMany (fun _ v2 -> v2))
        |> Sum.mapRight _.ToString()

      let formTypeCheckState = FormTypeCheckerState<'valueExt>.Init typeCheckState

      let formTypeCheckContext =
        FormTypeCheckingContext<ValueExt<'runtimeContext, 'db, 'customExtension>>.Init
          memoizedTypes
          languageContext.TypeCheckContext
          input.ApiTypes

      let! typeCheckedFormDefinitions, _ =
        checkFormDefinitions formDefinitions stdExtensions
        |> State.Run(formTypeCheckContext, formTypeCheckState)
        |> Sum.mapRight (fun (errors, _) -> Errors.ToString(errors, "\n"))

      return typeCheckedFormDefinitions
    }
