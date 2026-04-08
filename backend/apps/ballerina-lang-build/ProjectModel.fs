namespace Ballerina.DSL.Next.Runners

module ProjectModel =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Unification
  open Ballerina.Parser
  open Ballerina.DSL.Next.Syntax
  open System.IO
  open System.Text.Json
  open System.Text.Json.Serialization
  open Ballerina.Collections.NonEmptyList

  type ProjectBuildConfiguration =
    { Files: NonEmptyList<FileBuildConfiguration> }

  and FileTypeCheckedOutput<'valueExt when 'valueExt: comparison> =
    { File: FileBuildConfiguration
      Expr: TypeCheckedExpr<'valueExt>
      TypeValue: TypeValue<'valueExt> }

  and FileName = { Path: string }

  and Checksum =
    { Value: string }

    static member Compute(s: string) =
      use md5 = System.Security.Cryptography.MD5.Create()
      { Checksum.Value = BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s))) }

  and FileBuildConfiguration =
    { FileName: FileName
      Content: Unit -> string
      Checksum: Checksum }

    static member FromFile(fileName: string, fileContent: string) =
      { FileName = { Path = fileName }
        Content = fun () -> fileContent
        Checksum = Checksum.Compute fileContent }

  and ProjectCache<'valueExt when 'valueExt: comparison> =
    { Fold:
        NonEmptyList<FileBuildConfiguration>
          -> (FileBuildConfiguration * int
            -> State<
              TypeCheckedExpr<'valueExt> * TypeValue<'valueExt>,
              Unit,
              TypeCheckContext<'valueExt> * TypeCheckState<'valueExt>,
              Errors<Location>
             >)
          -> Sum<
            NonEmptyList<TypeCheckedExpr<'valueExt> * TypeValue<'valueExt>> *
            TypeCheckContext<'valueExt> *
            TypeCheckState<'valueExt>,
            Errors<Location>
           > }

  type InlayHint<'valueExt when 'valueExt: comparison> with
    member this.AsString() =
      $"%s{this.Identifier}: %s{this.Type.ToString()}"

  type TypeCheckState<'valueExt when 'valueExt: comparison> with
    static member InstantiateInlayHints
      (config: TypeCheckingConfig<'valueExt>)
      : State<TypeCheckState<'valueExt>, TypeCheckContext<'valueExt>, TypeCheckState<'valueExt>, Errors<Location>> =
      state {
        let typeCheckExpr =
          Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>.TypeCheck config

        let! initialState = state.GetState()

        let! inlayHints =
          initialState.InlayHints
          |> Map.toList
          |> List.map (fun (location, hint) ->
            state {
              let! currentState = state.GetState()
              let! currentContext = state.GetContext()

              let instantiatedHint =
                hint.Type
                |> TypeValue.Instantiate () (TypeExpr.Eval config typeCheckExpr) location
                |> State.Run(TypeInstantiateContext.FromEvalContext(currentContext), currentState)

              match instantiatedHint with
              | Left(instantiatedType, nextState) ->
                do!
                  nextState
                  |> Option.map (fun updatedState -> state.SetState(replaceWith updatedState))
                  |> state.RunOption
                  |> state.Map ignore

                let hint = { hint with Type = instantiatedType }
                return location, hint
              | Right _ -> return location, hint
            })
          |> state.All
          |> state.Map Map.ofList

        let! currentState = state.GetState()

        let nextState =
          currentState |> TypeCheckState.Updaters.InlayHints(replaceWith inlayHints)

        do! state.SetState(replaceWith nextState)
        return nextState
      }

    static member RenderInlayHints(state: TypeCheckState<'valueExt>) : Map<Location, string> =
      state.InlayHints |> Map.map (fun _ hint -> hint.AsString())

  type ProjectBuildConfiguration with
    static member BuildCachedWithFileOutputs<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (cache: ProjectCache<'valueExt>)
      (project: ProjectBuildConfiguration)
      : Sum<
          NonEmptyList<FileTypeCheckedOutput<'valueExt>> * TypeCheckContext<'valueExt> * TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =
      sum {
        let! expressionsWithTypes, finalContext, finalState =
          cache.Fold project.Files (fun (file, index) ->
            state {
              let! ParserResult(program, _) =
                file
                |> ProjectBuildConfiguration.ParseFile
                |> sum.WithErrorContext(fun () -> $"...while parsing {file.FileName.Path}")
                |> state.OfSum

              let! ctx, st = state.GetState()

              let! (typeCheckedExpr, ctx'), st' =
                Expr.TypeCheck config None program
                |> State.Run(ctx, st)
                |> sum.MapError fst
                |> sum.WithErrorContext(fun () -> $"...while typechecking {file.FileName.Path}")
                |> state.OfSum

              let typeValue = typeCheckedExpr.Type

              if index < project.Files.Tail.Length then
                match typeValue with
                | TypeValue.Primitive({ value = PrimitiveType.Unit }) -> return ()
                | _ ->
                  return!
                    state.Throw(
                      Errors.Singleton Location.Unknown (fun () ->
                        $"Expected returned unit type for {file.FileName.Path} but got {typeValue}. Intermediate files in the project should always return `()`, otherwise we discard possibly useful information by accident!")
                    )

              let st' = st' |> Option.defaultValue st
              do! state.SetState(replaceWith (ctx', st'))

              typeCheckedExpr, typeValue
            })

        let expressionsWithTypesInOrder =
          expressionsWithTypes |> NonEmptyList.rev |> NonEmptyList.ToList

        let! finalState =
          TypeCheckState.InstantiateInlayHints config
          |> State.Run(finalContext, finalState)
          |> sum.MapError fst
          |> sum.Map fst

        let filesInOrder = project.Files |> NonEmptyList.ToList

        let fileOutputs =
          List.zip filesInOrder expressionsWithTypesInOrder
          |> List.map (fun (file, (expr, typeValue)) ->
            { File = file
              Expr = expr
              TypeValue = typeValue })

        match fileOutputs with
        | outputHead :: outputTail -> return NonEmptyList.OfList(outputHead, outputTail), finalContext, finalState
        | [] -> return! sum.Throw(Errors.Singleton Location.Unknown (fun () -> "Expected at least one file output"))
      }

    static member ParseFile<'valueExt when 'valueExt: comparison>
      (file: FileBuildConfiguration)
      : Sum<
          ParserResult<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>>,
          Errors<Location>
         >
      =
      sum {
        let initialLocation = Location.Initial file.FileName.Path

        let! ParserResult(actual, _) =
          tokens
          |> Parser.Run(file.Content() |> Seq.toList, initialLocation)
          |> sum.MapError fst

        let! (parserResult:
          ParserResult<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>>) =
          Parser.Expr.program ()
          |> Parser.Run(actual, initialLocation)
          |> sum.MapError fst

        parserResult
      }

    static member BuildCached<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (cache: ProjectCache<'valueExt>)
      (project: ProjectBuildConfiguration)
      : Sum<
          NonEmptyList<TypeCheckedExpr<'valueExt>> *
          TypeValue<'valueExt> *
          TypeCheckContext<'valueExt> *
          TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =

      sum {
        let! fileOutputs, finalContext, finalState =
          ProjectBuildConfiguration.BuildCachedWithFileOutputs config cache project

        let expressions = fileOutputs |> NonEmptyList.map _.Expr

        let lastTypeValue =
          fileOutputs |> NonEmptyList.ToList |> List.last |> (fun x -> x.TypeValue)

        return expressions, lastTypeValue, finalContext, finalState
      }

  [<CLIMutable>]
  type ProjectFileDto =
    { [<JsonPropertyName "name">]
      Name: string
      [<JsonPropertyName "sources">]
      Sources: string array
      [<JsonPropertyName "inputProjects">]
      InputProjects: string array }

  type ProjectFile =
    { Name: string
      Sources: string list
      InputProjects: string list }

    static member FromDto(dto: ProjectFileDto) =
      { Name = dto.Name
        Sources = dto.Sources |> List.ofArray
        InputProjects = dto.InputProjects |> List.ofArray }

    static member FromJsonFile(path: string) : Sum<ProjectFile, Errors<Location>> =
      try
        use stream = File.OpenRead path
        let options = JsonFSharpOptions.Default().ToJsonSerializerOptions()
        let dto = JsonSerializer.Deserialize<ProjectFileDto>(stream, options)
        Left(ProjectFile.FromDto dto)
      with ex ->
        Right(Errors.Singleton Location.Unknown (fun () -> $"Failed to read project file '{path}': {ex.Message}"))

  type ProjectBuildConfiguration with
    static member FromProjectFile
      (projectFilePath: string, _projectDir: string)
      : Sum<ProjectBuildConfiguration, Errors<Location>> =
      sum {
        let loadSourceFile (baseDir: string) (sourcePath: string) : Sum<FileBuildConfiguration, Errors<Location>> =
          sum {
            let fullPath = Path.GetFullPath(Path.Combine(baseDir, sourcePath))

            if not (File.Exists fullPath) then
              return! sum.Throw(Errors.Singleton Location.Unknown (fun () -> $"Source file not found: {fullPath}"))

            let content = File.ReadAllText fullPath
            let fileName = Path.GetFileName sourcePath
            return FileBuildConfiguration.FromFile(fileName, content)
          }

        let rec loadProjectFiles
          (visited: Set<string>)
          (projectPath: string)
          : Sum<FileBuildConfiguration list, Errors<Location>> =
          sum {
            let normalizedProjectPath = Path.GetFullPath projectPath

            if not (File.Exists normalizedProjectPath) then
              return!
                sum.Throw(
                  Errors.Singleton Location.Unknown (fun () -> $"Project file not found: {normalizedProjectPath}")
                )

            if Set.contains normalizedProjectPath visited then
              return!
                sum.Throw(
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Circular project reference detected: {normalizedProjectPath}")
                )

            let visited = Set.add normalizedProjectPath visited
            let projectBaseDir = Path.GetDirectoryName normalizedProjectPath
            let! projectFile = ProjectFile.FromJsonFile normalizedProjectPath

            let! inputProjectFiles =
              projectFile.InputProjects
              |> List.map (fun inputProjectPath ->
                let fullInputProjectPath =
                  Path.GetFullPath(Path.Combine(projectBaseDir, inputProjectPath))

                loadProjectFiles visited fullInputProjectPath)
              |> sum.All
              |> sum.Map List.concat

            let! sourceFiles = projectFile.Sources |> List.map (loadSourceFile projectBaseDir) |> sum.All

            return inputProjectFiles @ sourceFiles
          }

        let! sourceFiles = loadProjectFiles Set.empty (Path.GetFullPath projectFilePath)

        match sourceFiles with
        | head :: tail -> return { Files = NonEmptyList.OfList(head, tail) }
        | [] ->
          return!
            sum.Throw(
              Errors.Singleton Location.Unknown (fun () -> "Project file must specify at least one source file")
            )
      }

    static member FromSingleFile(filePath: string) : Sum<ProjectBuildConfiguration, Errors<Location>> =
      sum {
        let fullPath = Path.GetFullPath filePath

        if not (File.Exists fullPath) then
          return! sum.Throw(Errors.Singleton Location.Unknown (fun () -> $"Source file not found: {fullPath}"))

        let content = File.ReadAllText fullPath
        let fileName = Path.GetFileName filePath
        let fileBuildConfig = FileBuildConfiguration.FromFile(fileName, content)

        return { Files = NonEmptyList.OfList(fileBuildConfig, []) }
      }
