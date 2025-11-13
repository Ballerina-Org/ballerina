namespace Ballerina.Data.Schema

open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Json
open Ballerina.Data.Schema.Model
open Ballerina.Data.Schema.Json
open Ballerina.Errors
open Ballerina.Reader.WithError
open Ballerina.VirtualFolders.Interactions
open Ballerina.VirtualFolders.Model
open Ballerina.VirtualFolders.Operations
open Ballerina.VirtualFolders.Patterns
open Ballerina.State.WithError
open Ballerina.Data.TypeEval
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.FormEngine.Model

module Extract =
  let _stdExtensions, langContext = stdExtensions

  let private entityNameFromLauncher (launcherName: LauncherName) (context: ParsedFormsContext<_, _>) =
    let launcher = context.Launchers |> Map.find launcherName

    match launcher.Mode with
    | FormLauncherMode.Edit apis
    | FormLauncherMode.Create apis -> apis.EntityApi.EntityName
    | Passthrough _configType ->
      let form = context.Forms |> Map.find launcher.Form.FormName

      let t =
        match form.Body with
        | Annotated a -> a.TypeId.VarName
        | Table t -> t.RowTypeId.VarName

      let key =
        context.Apis.Entities
        |> Map.tryFindKey (fun _ (api, _) -> api.TypeId.VarName = t)

      context.Apis.Entities[key.Value] |> fst |> _.EntityName
    | PassthroughTable pt -> pt.TableApi.TableName

  let rendererAndEntity<'ExprExt, 'ValExt>
    (launcherName: LauncherName)
    (formsContext: ParsedFormsContext<'ExprExt, 'ValExt>)
    (schema: Schema<TypeExpr, Identifier, ValueExt>)
    : Sum<Renderer<'ExprExt, 'ValExt> * EntityName * Schema<TypeValue, ResolvedIdentifier, ValueExt>, Errors> =
    sum {

      let entityName: EntityName =
        { EntityName = entityNameFromLauncher launcherName formsContext }

      let launcher = formsContext.Launchers |> Map.find launcherName
      let form = formsContext.Forms |> Map.find launcher.Form.FormName

      let! schema, _stateOpt =
        schema
        |> Schema.SchemaEval
        |> State.Run(langContext.TypeCheckContext.Types, langContext.TypeCheckState.Types)
        |> sum.MapError(fst >> _.Errors.Head.Message >> Errors.Singleton)

      let renderer =
        match form.Body with
        | Annotated annotation -> Some annotation.Renderer
        | Table table -> table.Details |> Option.map _.Renderer

      let! renderer = renderer |> sum.OfOption(Errors.Singleton("Table renderer is None"))
      return renderer, entityName, schema
    }

  let fromVirtualFolders (variant: WorkspaceVariant) (path: VirtualPath option) (node: VfsNode) =
    match variant with
    | Explore _ ->
      sum {
        let! path = path |> sum.OfOption(Errors.Singleton "Path is required")
        let schemaPath = withFileSuffix "_schema" path
        let typesPath = withFileSuffix "_typesV2" path

        let! schemaJson =
          tryFind schemaPath node
          |> sum.OfOption(Errors.Singleton("Can't find schema file"))

        let! typesJson =
          tryFind typesPath node
          |> sum.OfOption(Errors.Singleton("Can't find types file"))

        let! schemaJson = VfsNode.AsFile schemaJson
        let! typesJson = VfsNode.AsFile typesJson
        let! schemaJson = FileContent.AsJson schemaJson.Content
        let! typesJson = FileContent.AsJson typesJson.Content
        let! schemaJson = Schema.InsertTypesToSchema(schemaJson, typesJson)
        let! schema = Schema.FromJson schemaJson |> Reader.Run(TypeExpr.FromJson, Identifier.FromJson)
        return schema
      }
    | Compose -> sum.Throw(Errors.Singleton("Not implemented v2 files retrieval for compose spec"))
