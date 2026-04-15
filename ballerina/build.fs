module Ballerina.Build

open System
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.StdLib.DB
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.MutableMemoryDB
open Ballerina.DSL.Next.Runners
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.Collections.Sum
open Ballerina.Errors
open Ballerina.LocalizedErrors
open Ballerina.Reader.WithError
open Ballerina.StdLib.String

type CustomExt = Unit
type ValueExt = ValueExt<unit, MutableMemoryDB<unit, unit>, unit>

let private buildContext, languageContext, typeCheckingConfig, buildCache =
  hddcacheWithStdExtensions<unit, MutableMemoryDB<unit, unit>>
    (Ballerina.DSL.Next.StdLib.String.Extension.StringTypeClass<_>.Console())
    (Ballerina.DSL.Next.StdLib.Email.Extension.EmailTypeClass<_>.Console())
    (db_ops ())
    id
    id

let buildProject (project: ProjectBuildConfiguration) =
  sum {
    let! buildResult =
      ProjectBuildConfiguration.BuildCached typeCheckingConfig buildCache project
      |> sum.MapError (fun errors ->
        let inputFiles =
          project.Files
          |> Seq.map (fun def -> def.FileName.Path, def.Content())
          |> Map.ofSeq

        for e in (Errors<_>.FilterHighestPriorityOnly errors).Errors() do
          let source =
            match inputFiles |> Map.tryFind e.Context.File with
            | Some file -> file
            | None -> ""

          let lines =
            source.Split '\n'
            |> Seq.skip (e.Context.Line - 1)
            |> Seq.mapi (fun i line ->
              let fmt = "000"
              $"{(e.Context.Line + i).ToString(fmt)} |   {line}")
            |> Seq.truncate 3
            |> String.join "\n"

          Console.ForegroundColor <- ConsoleColor.Red
          Console.WriteLine $"  Error: {e.Message} at line {e.Context.Line}:\n{lines}"
          Console.ResetColor ()

        errors)

    return languageContext, buildResult
  }

let buildProjectStreaming
  (project: ProjectBuildConfiguration)
  (onFileBuilt:
    FileBuildConfiguration
      -> TypeCheckContext<ValueExt>
      -> TypeCheckState<ValueExt>
      -> unit)
  =
  sum {
    let! buildResult =
      ProjectBuildConfiguration.BuildCachedWithFileOutputsStreaming
        typeCheckingConfig
        buildCache
        project
        onFileBuilt
      |> sum.MapError (fun errors ->
        let inputFiles =
          project.Files
          |> Seq.map (fun def -> def.FileName.Path, def.Content())
          |> Map.ofSeq

        for e in (Errors<_>.FilterHighestPriorityOnly errors).Errors() do
          let source =
            match inputFiles |> Map.tryFind e.Context.File with
            | Some file -> file
            | None -> ""

          let lines =
            source.Split '\n'
            |> Seq.skip (e.Context.Line - 1)
            |> Seq.mapi (fun i line ->
              let fmt = "000"
              $"{(e.Context.Line + i).ToString(fmt)} |   {line}")
            |> Seq.truncate 3
            |> String.join "\n"

          Console.ForegroundColor <- ConsoleColor.Red
          Console.WriteLine $"  Error: {e.Message} at line {e.Context.Line}:\n{lines}"
          Console.ResetColor ()

        errors)

    return languageContext, buildResult
  }

let build (files: NonEmptyList<string>) =
  sum {
    let! project: ProjectBuildConfiguration =
      match files |> NonEmptyList.toList with
      | [ singlePath ] when singlePath.EndsWith(".blproj", StringComparison.OrdinalIgnoreCase) ->
        ProjectBuildConfiguration.FromProjectFile(singlePath, System.IO.Path.GetDirectoryName singlePath)
      | _ ->
        let fileDefs =
          files
          |> NonEmptyList.map (fun path -> FileBuildConfiguration.FromFile(path, System.IO.File.ReadAllText path))

        sum.Return { Files = fileDefs }

    return! buildProject project
  }

let buildAndRun (files: NonEmptyList<string>) =
  sum {
    let! langCtx, (exprs, _typeValue, _typeCheckCtx, typeCheckState: TypeCheckState<ValueExt>) = build files

    let! runnableExprs =
      exprs
      |> NonEmptyList.toList
      |> List.map Conversion.convertExpression
      |> sum.All

    let runnableExprs =
      match runnableExprs with
      | head :: tail -> NonEmptyList.OfList(head, tail)
      | [] -> failwith "Expected at least one expression"

    let evalContext: ExprEvalContext<unit, ValueExt> = ExprEvalContext.Empty ()
    let evalContext = ExprEvalContext.WithTypeCheckingSymbols (evalContext |> langCtx.ExprEvalContext) typeCheckState.Symbols
    return!
      Expr.Eval (NonEmptyList.prependList langCtx.TypeCheckedPreludes runnableExprs)
      |> Reader.Run evalContext
  }
