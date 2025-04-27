namespace Ballerina.DSL.FormEngine.Codegen.Golang.Generator

module Main =

  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.Patterns
  open Ballerina.State.WithError
  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.FormEngine.Parser
  open Ballerina.Errors
  open Ballerina.Collections.Sum
  open Ballerina.Core.Object
  open Ballerina.Core.String
  open Ballerina.Core.StringBuilder
  open Ballerina.Core.Json
  open System
  open System.Text.RegularExpressions
  open Ballerina.Fun
  open Ballerina.Collections
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.FormEngine.Codegen.Golang.Generator.Model
  open Ballerina.DSL.FormEngine.Codegen.Golang.LanguageConstructs
  open Ballerina.DSL.FormEngine.Codegen.Golang.LanguageConstructs.Enum
  open Ballerina.DSL.FormEngine.Codegen.Golang.LanguageConstructs.Union
  open Ballerina.DSL.FormEngine.Codegen.Golang.LanguageConstructs.Record
  open Ballerina.DSL.FormEngine.Codegen.Golang.LanguageConstructs.EnumGETters
  open Ballerina.DSL.FormEngine.Codegen.Golang.LanguageConstructs.EnumPOSTers
  open Ballerina.DSL.FormEngine.Codegen.Golang.LanguageConstructs.StreamGETters
  open Ballerina.DSL.FormEngine.Codegen.Golang.LanguageConstructs.StreamPOSTers
  open Ballerina.DSL.FormEngine.Codegen.Golang.LanguageConstructs.WritersAndDeltas
  open Ballerina.DSL.FormEngine.Codegen.Golang.LanguageConstructs.TypeAnnotations
  open Ballerina.DSL.FormEngine.Codegen.Golang.LanguageConstructs.DefaultValues

  type ParsedFormsContext with
    static member ToGolang
      (codegenConfig: CodeGenConfig)
      (ctx: ParsedFormsContext)
      (packageName: string)
      (formName: string)
      : Sum<StringBuilder, Errors> =
      let result =
        state {
          let identifierAllowedRegex = Regex codegenConfig.IdentifierAllowedRegex
          let (!) (s: string) = identifierAllowedRegex.Replace(s, "_")

          let launchersEnum: GolangEnum =
            { Name = $"{formName}LaunchersEnum"
              Cases =
                ctx.Launchers
                |> Map.values
                |> Seq.map (fun launcher ->
                  {| Name = $"{!launcher.LauncherName}Launcher"
                     Value = $"{launcher.LauncherName}" |})
                |> Seq.toList }

          let launchersEnum = GolangEnum.Generate () launchersEnum


          let! oneGETters =
            state {
              if ctx.Apis.Lookups |> Seq.exists (fun l -> l.Value.Ones |> Map.isEmpty |> not) then
                let! tuple2Config =
                  codegenConfig.Tuple
                  |> Seq.tryFind (fun t -> t.Ariety = 2)
                  |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find tuple 2.")
                  |> state.OfSum

                return
                  GolangOneGETters.Generate
                    ()
                    { FunctionName = $"{formName}OneGETter"
                      OneNotFoundErrorConstructor = codegenConfig.OneNotFoundError.Constructor
                      Tuple2Type = tuple2Config.GeneratedTypeName
                      Ones =
                        seq {
                          for l in ctx.Apis.Lookups do
                            let lookupTypeName = l.Key

                            for one in l.Value.Ones do
                              yield
                                {| OneName = one.Key
                                   OneLookupType = lookupTypeName
                                   OneType = (one.Value |> fst).TypeId.TypeName |}

                        }
                        |> List.ofSeq }
              else
                return StringBuilder.Many []
            }

          let! oneGETMANYUNLINKEDers =
            state {
              if ctx.Apis.Lookups |> Seq.exists (fun l -> l.Value.Ones |> Map.isEmpty |> not) then
                let! tuple2Config =
                  codegenConfig.Tuple
                  |> Seq.tryFind (fun t -> t.Ariety = 2)
                  |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find tuple 2.")
                  |> state.OfSum

                return
                  GolangOneGETMANYUNLINKEDers.Generate
                    ()
                    { FunctionName = $"{formName}OneGETMANYUNLINKEDer"
                      OneNotFoundErrorConstructor = codegenConfig.OneNotFoundError.Constructor
                      Tuple2Type = tuple2Config.GeneratedTypeName
                      TableType = codegenConfig.Table.GeneratedTypeName
                      Ones =
                        seq {
                          for l in ctx.Apis.Lookups do
                            let lookupTypeName = l.Key

                            for one in l.Value.Ones do
                              if one.Value |> snd |> Set.contains CrudMethod.GetManyUnlinked then
                                yield
                                  {| OneName = one.Key
                                     OneLookupType = lookupTypeName
                                     OneType = (one.Value |> fst).TypeId.TypeName |}

                        }
                        |> List.ofSeq }
              else
                return StringBuilder.Many []
            }

          let! onePATCHers =
            state {
              if ctx.Apis.Lookups |> Seq.exists (fun l -> l.Value.Ones |> Map.isEmpty |> not) then
                let! tuple2Config =
                  codegenConfig.Tuple
                  |> Seq.tryFind (fun t -> t.Ariety = 2)
                  |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find tuple 2.")
                  |> state.OfSum

                return
                  GolangOnePATCHers.Generate
                    ()
                    { FunctionName = $"{formName}OnePATCHer"
                      OneNotFoundErrorConstructor = codegenConfig.OneNotFoundError.Constructor
                      Tuple2Type = tuple2Config.GeneratedTypeName
                      DeltaOneType = codegenConfig.One.DeltaTypeName
                      DeltaBaseType = codegenConfig.DeltaBase.GeneratedTypeName
                      Ones =
                        seq {
                          for l in ctx.Apis.Lookups do
                            let lookupTypeName = l.Key

                            for one in l.Value.Ones do
                              if
                                one.Value
                                |> snd
                                |> Set.intersect (
                                  Set.ofList [ CrudMethod.Create; CrudMethod.Update; CrudMethod.Delete ]
                                )
                                |> Set.isEmpty
                                |> not
                              then
                                yield
                                  {| OneName = one.Key
                                     OneLookupType = lookupTypeName
                                     OneType = (one.Value |> fst).TypeId.TypeName |}

                        }
                        |> List.ofSeq }
              else
                return StringBuilder.Many []
            }

          let! streamGETMANYers =
            state {
              if ctx.Apis.Lookups |> Seq.exists (fun l -> l.Value.Streams |> Map.isEmpty |> not) then
                let! tuple2Config =
                  codegenConfig.Tuple
                  |> Seq.tryFind (fun t -> t.Ariety = 2)
                  |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find tuple 2.")
                  |> state.OfSum

                return
                  GolangStreamGETMANYers.Generate
                    ()
                    { FunctionName = $"{formName}StreamGETMANYer"
                      StreamNotFoundErrorConstructor = codegenConfig.LookupStreamNotFoundError.Constructor
                      Tuple2Type = tuple2Config.GeneratedTypeName
                      TableType = codegenConfig.Table.GeneratedTypeName
                      Ones =
                        seq {
                          for l in ctx.Apis.Lookups do
                            let lookupTypeName = l.Key

                            for stream in l.Value.Streams do
                              yield
                                {| StreamName = stream.Key
                                   StreamLookupType = lookupTypeName
                                   StreamType = stream.Value.TypeId.TypeName |}

                        }
                        |> List.ofSeq }
              else
                return StringBuilder.Many []
            }

          let! manyGETMANYers =
            state {
              if ctx.Apis.Lookups |> Seq.exists (fun l -> l.Value.Manys |> Map.isEmpty |> not) then
                let! tuple2Config =
                  codegenConfig.Tuple
                  |> Seq.tryFind (fun t -> t.Ariety = 2)
                  |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find tuple 2.")
                  |> state.OfSum

                return
                  GolangManyGETMANYers.Generate
                    ()
                    { FunctionName = $"{formName}ManyGETMANYer"
                      ManyNotFoundErrorConstructor = codegenConfig.ManyNotFoundError.Constructor
                      Tuple2Type = tuple2Config.GeneratedTypeName
                      TableType = codegenConfig.Table.GeneratedTypeName
                      Manys =
                        seq {
                          for l in ctx.Apis.Lookups do
                            let lookupTypeName = l.Key

                            for many in l.Value.Manys do
                              yield
                                {| ManyName = many.Key
                                   ManyLookupType = lookupTypeName
                                   ManyType = (many.Value |> fst).TypeId.TypeName |}

                        }
                        |> List.ofSeq }
              else
                return StringBuilder.Many []
            }

          let! manyPATCHers =
            state {
              if ctx.Apis.Lookups |> Seq.exists (fun l -> l.Value.Manys |> Map.isEmpty |> not) then
                let! tuple2Config =
                  codegenConfig.Tuple
                  |> Seq.tryFind (fun t -> t.Ariety = 2)
                  |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find tuple 2.")
                  |> state.OfSum

                return
                  GolangManyPATCHers.Generate
                    ()
                    { FunctionName = $"{formName}ManyPATCHer"
                      ManyNotFoundErrorConstructor = codegenConfig.ManyNotFoundError.Constructor
                      Tuple2Type = tuple2Config.GeneratedTypeName
                      DeltaManyType = codegenConfig.Table.DeltaTypeName
                      DeltaBaseType = codegenConfig.DeltaBase.GeneratedTypeName
                      Manys =
                        seq {
                          for l in ctx.Apis.Lookups do
                            let lookupTypeName = l.Key

                            for many in l.Value.Manys do
                              if
                                many.Value
                                |> snd
                                |> Set.intersect (
                                  Set.ofList [ CrudMethod.Create; CrudMethod.Update; CrudMethod.Delete ]
                                )
                                |> Set.isEmpty
                                |> not
                              then
                                yield
                                  {| ManyName = many.Key
                                     ManyLookupType = lookupTypeName
                                     ManyType = (many.Value |> fst).TypeId.TypeName |}
                        }
                        |> List.ofSeq }
              else
                return StringBuilder.Many []
            }

          let! manyGETMANYUNLINKEDers =
            state {
              if ctx.Apis.Lookups |> Seq.exists (fun l -> l.Value.Manys |> Map.isEmpty |> not) then
                let! tuple2Config =
                  codegenConfig.Tuple
                  |> Seq.tryFind (fun t -> t.Ariety = 2)
                  |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find tuple 2.")
                  |> state.OfSum

                return
                  GolangManyGETMANYUNLINKEDers.Generate
                    ()
                    { FunctionName = $"{formName}ManyGETMANYUNLINKEDer"
                      ManyNotFoundErrorConstructor = codegenConfig.ManyNotFoundError.Constructor
                      Tuple2Type = tuple2Config.GeneratedTypeName
                      TableType = codegenConfig.Table.GeneratedTypeName
                      Manys =
                        seq {
                          for l in ctx.Apis.Lookups do
                            let lookupTypeName = l.Key

                            for many in l.Value.Manys do
                              if many.Value |> snd |> Set.contains CrudMethod.GetManyUnlinked then
                                yield
                                  {| ManyName = many.Key
                                     ManyLookupType = lookupTypeName
                                     ManyType = (many.Value |> fst).TypeId.TypeName |}

                        }
                        |> List.ofSeq }
              else
                return StringBuilder.Many []
            }

          let tablesEnum: GolangEnum =
            { Name = $"{formName}TablesEnum"
              Cases =
                ctx.Apis.Tables
                |> Map.values
                |> Seq.map (fun entityApi ->
                  {| Name = $"{entityApi.TableName}Entity"
                     Value = $"{entityApi.TypeId.TypeName}" |})
                |> Seq.toList }

          let tablesEnum = GolangEnum.Generate () tablesEnum

          let tableGETters =
            GolangTableGETters.Generate
              ()
              { FunctionName = $"{formName}TableGETter"
                TableNotFoundErrorConstructor = codegenConfig.TableNotFoundError.Constructor
                Tables =
                  ctx.Apis.Tables
                  |> Map.values
                  // |> Seq.filter (snd >> Set.contains CrudMethod.Get)
                  // |> Seq.map fst
                  |> Seq.map (fun e ->
                    {| TableName = e.TableName
                       TableType = $"{codegenConfig.Table.GeneratedTypeName}[{e.TypeId.TypeName}]" |})
                  |> List.ofSeq }

          let tablePOSTters =
            GolangTablePOSTters.Generate
              ()
              { FunctionName = $"{formName}TablePOSTter"
                TableNotFoundErrorConstructor = codegenConfig.TableNotFoundError.Constructor
                Tables =
                  ctx.Apis.Tables
                  |> Map.values
                  // |> Seq.filter (snd >> Set.contains CrudMethod.Get)
                  // |> Seq.map fst
                  |> Seq.map (fun e ->
                    {| TableName = e.TableName
                       TableRowType = e.TypeId.TypeName |})
                  |> List.ofSeq }

          let tableDELETEters =
            GolangTableDELETEters.Generate
              ()
              { FunctionName = $"{formName}TableDELETEter"
                TableNotFoundErrorConstructor = codegenConfig.TableNotFoundError.Constructor
                Tables =
                  ctx.Apis.Tables
                  |> Map.values
                  // |> Seq.filter (snd >> Set.contains CrudMethod.Get)
                  // |> Seq.map fst
                  |> Seq.map (fun e -> {| TableName = e.TableName |})
                  |> List.ofSeq }

          let tablePATCHters =
            GolangTablePATCHters.Generate
              ()
              { FunctionName = $"{formName}TablePATCHter"
                TableNotFoundErrorConstructor = codegenConfig.TableNotFoundError.Constructor
                Tables =
                  ctx.Apis.Tables
                  |> Map.values
                  // |> Seq.filter (snd >> Set.contains CrudMethod.Get)
                  // |> Seq.map fst
                  |> Seq.map (fun e ->
                    {| TableName = e.TableName
                       TableDeltaType =
                        $"{codegenConfig.Table.DeltaTypeName}[{e.TypeId.TypeName}, Delta{e.TypeId.TypeName}]" |})
                  |> List.ofSeq }

          let entitiesEnum: GolangEnum =
            { Name = $"{formName}EntitiesEnum"
              Cases =
                ctx.Apis.Entities
                |> Map.values
                |> Seq.map fst
                |> Seq.map (fun entityApi ->
                  {| Name = $"{entityApi.EntityName}Entity"
                     Value = $"{entityApi.TypeId.TypeName}" |})
                |> Seq.toList }

          let entitiesEnum = GolangEnum.Generate () entitiesEnum

          let entityGETters =
            GolangEntityGETters.Generate
              ()
              { FunctionName = $"{formName}EntityGETter"
                EntityNotFoundErrorConstructor = codegenConfig.EntityNotFoundError.Constructor
                Entities =
                  ctx.Apis.Entities
                  |> Map.values
                  |> Seq.filter (snd >> Set.contains CrudMethod.Get)
                  |> Seq.map fst
                  |> Seq.map (fun e ->
                    {| EntityName = e.EntityName
                       EntityType = e.TypeId.TypeName |})
                  |> List.ofSeq }

          let entityDEFAULTers =
            let entities =
              { GolangEntityGETDEFAULTers.FunctionName = $"{formName}EntityDEFAULTer"
                Entities =
                  ctx.Apis.Entities
                  |> Map.values
                  |> Seq.filter (snd >> Set.contains CrudMethod.Default)
                  |> Seq.map fst
                  |> Seq.map (fun e ->
                    {| EntityName = e.EntityName
                       EntityType = e.TypeId.TypeName |})
                  |> List.ofSeq
                EntityNotFoundErrorConstructor = codegenConfig.EntityNotFoundError.Constructor }

            GolangEntityGETDEFAULTers.Generate () entities

          let entityPOSTers =
            let entities =
              { GolangEntityPOSTers.FunctionName = $"{formName}EntityPOSTer"
                Entities =
                  ctx.Apis.Entities
                  |> Map.values
                  |> Seq.filter (snd >> Set.contains CrudMethod.Create)
                  |> Seq.map fst
                  |> Seq.map (fun e ->
                    {| EntityName = e.EntityName
                       EntityType = e.TypeId.TypeName |})
                  |> List.ofSeq
                EntityNotFoundErrorConstructor = codegenConfig.EntityNotFoundError.Constructor }

            GolangEntityPOSTers.Generate () entities

          let enumCasesGETters =
            let getters =
              { GolangEnumGETters.FunctionName = $"{formName}EnumGETter"
                EnumNotFoundErrorConstructor = codegenConfig.EnumNotFoundError.Constructor
                Enums =
                  ctx.Apis.Enums
                  |> Map.values
                  |> Seq.map (fun e ->
                    {| EnumName = e.EnumName
                       EnumType = e.UnderlyingEnum.TypeName |})
                  |> List.ofSeq }

            GolangEnumGETters.Generate () getters

          let enumCasesPOSTters =
            let posters =
              { GolangEnumPOSTers.FunctionName = $"{formName}EnumPOSTter"
                InvalidEnumValueCombinationError = codegenConfig.InvalidEnumValueCombinationError.Constructor
                UnitType = codegenConfig.Unit.GeneratedTypeName
                Enums =
                  ctx.Apis.Enums
                  |> Map.values
                  |> Seq.map (fun e ->
                    {| EnumName = e.EnumName
                       EnumType = e.UnderlyingEnum.TypeName |})
                  |> List.ofSeq }

            GolangEnumPOSTers.Generate () posters

          let streamGETters =
            let getters =
              { GolangStreamGETters.FunctionName = $"{formName}StreamGETter"
                Streams =
                  ctx.Apis.Streams
                  |> Map.values
                  |> Seq.map (fun e ->
                    {| StreamName = e.StreamName
                       StreamType = e.TypeId.TypeName |})
                  |> List.ofSeq
                StreamNotFoundErrorConstructor = codegenConfig.StreamNotFoundError.Constructor }

            GolangStreamGETters.Generate () getters

          let streamPOSTters =
            let posters: GolangStreamPOSTers =
              { GolangStreamPOSTers.FunctionName = $"{formName}StreamPOSTter"
                Streams =
                  ctx.Apis.Streams
                  |> Map.values
                  |> Seq.map (fun e ->
                    {| StreamName = e.StreamName
                       StreamType = e.TypeId.TypeName |})
                  |> List.ofSeq
                GuidType = codegenConfig.Guid.GeneratedTypeName
                StreamNotFoundErrorConstructor = codegenConfig.StreamNotFoundError.Constructor }

            GolangStreamPOSTers.Generate () posters

          let customTypes = codegenConfig.Custom.Keys |> Set.ofSeq

          let typesToGenerate =
            ctx.Types
            |> Map.filter (fun k v -> customTypes |> Set.contains k |> not)
            |> Seq.map (fun t ->
              { TypeName = t.Key
                Type = t.Value.Type })
            |> List.ofSeq

          let! generatedTypes = GolangGeneratedType.Generate (ctx, codegenConfig, formName) typesToGenerate

          let customTypes: List<GolangCustomType> =
            codegenConfig.Custom
            |> Seq.map (fun t ->
              { TypeName = t.Key
                GeneratedTypeName = t.Value.GeneratedTypeName
                DefaultConstructor = t.Value.DefaultConstructor })
            |> List.ofSeq

          let customTypes = GolangCustomType.Generate () customTypes

          let entityAPIsWithUPDATE =
            ctx.Apis.Entities
            |> Map.values
            |> Seq.filter (snd >> Set.contains CrudMethod.Update)
            |> Seq.map fst

          let writersBuilder =
            state {
              for t in ctx.Types |> Map.values do
                do! ExprType.ToWriter { WriterName = t.TypeId.TypeName } t.Type |> state.Map ignore
            }

          match writersBuilder.run ((ctx, codegenConfig), Map.empty) with
          | Right(err: Errors, _) -> return! state.Throw err
          | Left(_, newWritersState) ->
            // do System.Console.WriteLine(newWritersState.ToFSharpString)
            let allWriters = newWritersState |> Option.defaultWith (fun () -> Map.empty)

            let! allCommittables =
              entityAPIsWithUPDATE
              |> Seq.map (fun e ->
                state {
                  let! t =
                    ctx.Types
                    |> Map.tryFindWithError e.TypeId.TypeName "type" e.TypeId.TypeName
                    |> state.OfSum

                  let t = t.Type

                  return!
                    allWriters
                    |> Map.tryFindWithError ({ WriterName = e.TypeId.TypeName }) "writer" e.TypeId.TypeName
                    |> Sum.map (fun w ->
                      {| Writer = w
                         EntityApiName = e.EntityName |})
                    |> state.OfSum
                })
              |> state.All

            let entityPATCHers =
              let entities =
                { GolangEntityPATCHers.FunctionName = $"{formName}EntityPATCHer"
                  Writers = allWriters
                  CommittableWriters = allCommittables
                  EntityNotFoundErrorConstructor = codegenConfig.EntityNotFoundError.Constructor }

              GolangEntityPATCHers.Generate (ctx, codegenConfig, formName) entities

            let! s = state.GetState()
            let imports: Set<string> = s.UsedImports

            let heading = Header.Generate (ctx, codegenConfig, packageName, formName) imports

            return
              StringBuilder.Many(
                seq {
                  yield heading
                  yield manyGETMANYers
                  yield manyPATCHers
                  yield manyGETMANYUNLINKEDers
                  yield oneGETters
                  yield oneGETMANYUNLINKEDers
                  yield onePATCHers
                  yield tablesEnum
                  yield tableGETters
                  yield tablePOSTters
                  yield tableDELETEters
                  yield tablePATCHters
                  yield launchersEnum
                  yield entitiesEnum
                  yield entityGETters
                  yield entityDEFAULTers
                  yield entityPOSTers
                  yield entityPATCHers
                  yield enumCasesGETters
                  yield enumCasesPOSTters
                  yield streamGETters
                  yield streamGETMANYers
                  yield streamPOSTters
                  yield! generatedTypes
                  yield customTypes
                }
              )
        }

      let result = result |> state.WithErrorContext $"...when generating Go code"

      match result.run (codegenConfig, { UsedImports = Set.empty }) with
      | Right(e, _) -> Right e
      | Left(res, s') -> Left res
