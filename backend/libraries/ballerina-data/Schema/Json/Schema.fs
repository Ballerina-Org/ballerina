namespace Ballerina.Data.Schema

open Ballerina.VirtualFolders
open Ballerina.VirtualFolders.Interactions
open Ballerina.VirtualFolders.Operations

module Json =

  open Ballerina.Reader.WithError
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Data.Arity.Model
  open FSharp.Data
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Json
  open Ballerina.DSL.Next.Json
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.Data.Schema.Model
  open Ballerina.VirtualFolders.Model
  open Ballerina.VirtualFolders.Patterns
  open Ballerina.DSL.Next.Types

  type EntityMethod with
    static member FromJson(jsonValue: JsonValue) : Sum<EntityMethod, Errors> =
      sum {
        match! jsonValue |> JsonValue.AsString with
        | "get" -> return Get
        | "getMany" -> return GetMany
        | "create" -> return Create
        | "delete" -> return Delete
        | _ -> return! $"Invalid entity method: {jsonValue}" |> Errors.Singleton |> sum.Throw
      }

    static member ToJson(method: EntityMethod) : JsonValue =
      match method with
      | Get -> JsonValue.String "get"
      | GetMany -> JsonValue.String "getMany"
      | Create -> JsonValue.String "create"
      | Delete -> JsonValue.String "delete"

  type UpdaterPathStep with
    static member FromJson(jsonValue: JsonValue) : Sum<UpdaterPathStep, Errors> =
      sum {
        let! keyword, parameter = jsonValue |> JsonValue.AsPair
        let! keyword = keyword |> JsonValue.AsString

        match keyword with
        | "field" ->
          let! parameter = parameter |> JsonValue.AsString
          return UpdaterPathStep.Field parameter
        | "tupleItem" ->
          let! parameter = parameter |> JsonValue.AsInt
          return UpdaterPathStep.TupleItem(parameter)
        | "listItem" ->
          let! parameter = parameter |> JsonValue.AsString
          return UpdaterPathStep.ListItem(Var.Create parameter)
        | "unionCase" ->
          let! caseName, parameter = parameter |> JsonValue.AsPair
          let! caseName = caseName |> JsonValue.AsString
          let! parameter = parameter |> JsonValue.AsString
          return UpdaterPathStep.UnionCase(caseName, parameter |> Var.Create)
        | "sumCase" ->
          let! caseName, parameter = parameter |> JsonValue.AsPair
          let! caseName = caseName |> JsonValue.AsInt
          let! parameter = parameter |> JsonValue.AsString
          return UpdaterPathStep.SumCase(caseName, Var.Create parameter)
        | _ -> return! $"Invalid updater keyword: {keyword}" |> Errors.Singleton |> sum.Throw
      }

    static member ToJson(step: UpdaterPathStep) : JsonValue =
      match step with
      | UpdaterPathStep.Field name -> JsonValue.Array [| JsonValue.String "field"; JsonValue.String name |]
      | UpdaterPathStep.TupleItem index ->
        JsonValue.Array [| JsonValue.String "tupleItem"; JsonValue.Number(decimal index) |]
      | UpdaterPathStep.ListItem var -> JsonValue.Array [| JsonValue.String "listItem"; JsonValue.String var.Name |]
      | UpdaterPathStep.UnionCase(_str, var) ->
        JsonValue.Array
          [| JsonValue.String "unionCase"
             JsonValue.Array [| JsonValue.String "CaseName"; JsonValue.String var.Name |] |]
      | UpdaterPathStep.SumCase(index, var) ->
        JsonValue.Array
          [| JsonValue.String "sumCase"
             JsonValue.Array [| JsonValue.Number(decimal index); JsonValue.String var.Name |] |]

  type Updater<'Type, 'Id when 'Id: comparison> with
    static member FromJson
      (jsonValue: JsonValue)
      : Reader<Updater<'Type, 'Id>, JsonParser<'Type> * JsonParser<'Id>, Errors> =
      reader {
        let! path, condition, expr = jsonValue |> JsonValue.AsTriple |> reader.OfSum
        let! path = path |> JsonValue.AsArray |> reader.OfSum
        let! path = path |> Seq.map UpdaterPathStep.FromJson |> sum.All |> reader.OfSum
        let! condition = condition |> Expr.FromJson
        let! expr = expr |> Expr.FromJson

        return
          { Updater.Path = path
            Updater.Condition = condition
            Updater.Expr = expr }
      }

    static member ToJson
      (updater: Updater<'Type, 'Id>)
      : Reader<JsonValue, JsonEncoder<'Type> * JsonEncoder<'Id>, Errors> =
      let pathJson =
        updater.Path
        |> Seq.toArray
        |> Array.map UpdaterPathStep.ToJson
        |> JsonValue.Array

      reader {
        let! conditionJson = updater.Condition |> Expr.ToJson
        let! exprJson = updater.Expr |> Expr.ToJson
        return JsonValue.Array [| pathJson; conditionJson; exprJson |]
      }

  type LookupMethod with
    static member FromJson(jsonValue: JsonValue) : Sum<LookupMethod, Errors> =
      sum {
        match! jsonValue |> JsonValue.AsString with
        | "get" -> return LookupMethod.Get
        | "getMany" -> return LookupMethod.GetMany
        | "create" -> return LookupMethod.Create
        | "delete" -> return LookupMethod.Delete
        | "link" -> return LookupMethod.Link
        | "unlink" -> return LookupMethod.Unlink
        | _ -> return! $"Invalid lookup method: {jsonValue}" |> Errors.Singleton |> sum.Throw
      }

    static member ToJson(method: LookupMethod) : JsonValue =
      match method with
      | LookupMethod.Get -> JsonValue.String "get"
      | LookupMethod.GetMany -> JsonValue.String "getMany"
      | LookupMethod.Create -> JsonValue.String "create"
      | LookupMethod.Delete -> JsonValue.String "delete"
      | LookupMethod.Link -> JsonValue.String "link"
      | LookupMethod.Unlink -> JsonValue.String "unlink"

  type JsonParser<'T> = JsonValue -> Sum<'T, Errors>

  type EntityDescriptor<'T, 'Id when 'Id: comparison> with
    static member FromJson
      (jsonValue: JsonValue)
      : Reader<EntityDescriptor<'T, 'Id>, JsonParser<'T> * JsonParser<'Id>, Errors> =
      reader {
        let! jsonValue = jsonValue |> JsonValue.AsRecordMap |> reader.OfSum
        let! typeValue = jsonValue |> Map.tryFindWithError "type" "entity" "type" |> reader.OfSum
        let! ctx, _ = reader.GetContext()
        let! typeValue = typeValue |> ctx |> reader.OfSum

        let! methods = jsonValue |> Map.tryFindWithError "methods" "entity" "methods" |> reader.OfSum
        let! methods = methods |> JsonValue.AsArray |> reader.OfSum

        let! methods =
          methods
          |> Seq.map EntityMethod.FromJson
          |> sum.All
          |> sum.Map Set.ofSeq
          |> reader.OfSum

        let! updaters = jsonValue |> Map.tryFindWithError "updaters" "entity" "updaters" |> reader.OfSum
        let! updaters = updaters |> JsonValue.AsArray |> reader.OfSum
        let! updaters = updaters |> Seq.map Updater<'T, 'Id>.FromJson |> reader.All

        let! predicates =
          jsonValue
          |> Map.tryFindWithError "predicates" "entity" "predicates"
          |> reader.OfSum

        let! predicates = predicates |> JsonValue.AsRecordMap |> reader.OfSum
        let! predicates = predicates |> Map.map (fun _ -> Expr.FromJson) |> reader.AllMap

        return
          { Type = typeValue
            Methods = methods
            Updaters = updaters
            Predicates = predicates }
      }

    static member ToJson
      (entity: EntityDescriptor<'T, 'Id>)
      : Reader<JsonValue, JsonEncoder<'T> * JsonEncoder<'Id>, Errors> =
      reader {
        let! ctx, _ = reader.GetContext()
        let typeJson = entity.Type |> ctx

        let methodsJson =
          entity.Methods |> Seq.map EntityMethod.ToJson |> Seq.toArray |> JsonValue.Array

        let! updatersJson = entity.Updaters |> Seq.map Updater<'T, 'Id>.ToJson |> reader.All

        let! predicatesJson = entity.Predicates |> Map.map (fun _ -> Expr.ToJson) |> reader.AllMap

        return
          JsonValue.Record
            [| "type", typeJson
               "methods", methodsJson
               "updaters", updatersJson |> Seq.toArray |> JsonValue.Array
               "predicates", predicatesJson |> Map.toArray |> JsonValue.Record |]
      }

  type DirectedLookupDescriptor with
    static member FromJson(jsonValue: JsonValue) : Sum<DirectedLookupDescriptor, Errors> =
      sum {
        let! jsonValue = jsonValue |> JsonValue.AsRecordMap

        let! arity = jsonValue |> Map.tryFindWithError "arity" "lookup.directed" "arity"
        let! arity = arity |> JsonValue.AsRecordMap
        let! min = arity |> Map.tryFindWithError "min" "lookup.directed.arity" "min" |> sum.Catch
        let! min = min |> Option.map JsonValue.AsInt |> sum.RunOption
        let! max = arity |> Map.tryFindWithError "max" "lookup.directed.arity" "max" |> sum.Catch
        let! max = max |> Option.map JsonValue.AsInt |> sum.RunOption
        let arity = { Min = min; Max = max }

        let! methods = jsonValue |> Map.tryFindWithError "methods" "lookup.directed" "methods"
        let! methods = methods |> JsonValue.AsArray

        let! methods = methods |> Seq.map LookupMethod.FromJson |> sum.All |> sum.Map Set.ofSeq

        let! path = jsonValue |> Map.tryFindWithError "path" "entity" "path"
        let! path = path |> JsonValue.AsArray
        let! path = path |> Seq.map UpdaterPathStep.FromJson |> sum.All

        return
          { Arity = arity
            Methods = methods
            Path = path }
      }

    static member ToJson(descriptor: DirectedLookupDescriptor) : JsonValue =
      let arityJson =
        [| "min", descriptor.Arity.Min; "max", descriptor.Arity.Max |]
        |> Array.choose (fun (k, opt) -> opt |> Option.map (fun n -> k, JsonValue.Number(decimal n)))
        |> JsonValue.Record

      let methodsJson =
        descriptor.Methods
        |> Seq.map LookupMethod.ToJson
        |> Seq.toArray
        |> JsonValue.Array

      let path =
        descriptor.Path
        |> List.map UpdaterPathStep.ToJson
        |> Array.ofList
        |> JsonValue.Array

      JsonValue.Record [| "arity", arityJson; "methods", methodsJson; "path", path |]

  type LookupDescriptor with
    static member FromJson(jsonValue: JsonValue) : Sum<LookupDescriptor, Errors> =
      sum {
        let! jsonValue = jsonValue |> JsonValue.AsRecordMap

        let! source = jsonValue |> Map.tryFindWithError "source" "lookup" "source"
        let! source = source |> JsonValue.AsString
        let! target = jsonValue |> Map.tryFindWithError "target" "lookup" "target"
        let! target = target |> JsonValue.AsString

        let! forward = jsonValue |> Map.tryFindWithError "forward" "lookup" "forward"
        let! forward = DirectedLookupDescriptor.FromJson forward

        let! backward = jsonValue |> Map.tryFindWithError "backward" "lookup" "backward" |> sum.Catch

        let! backward =
          backward
          |> Option.map (fun b ->
            sum {
              let! b = b |> JsonValue.AsRecordMap
              let! name = b |> Map.tryFindWithError "name" "lookup.backward" "name"
              let! name = name |> JsonValue.AsString
              let! descriptor = b |> Map.tryFindWithError "descriptor" "lookup.backward" "descriptor"
              let! descriptor = DirectedLookupDescriptor.FromJson descriptor
              return { LookupName = name }, descriptor
            })
          |> sum.RunOption

        return
          { Source = { EntityName = source }
            Target = { EntityName = target }
            Forward = forward
            Backward = backward }
      }

    static member ToJson(lookup: LookupDescriptor) : JsonValue =
      let forwardJson = DirectedLookupDescriptor.ToJson lookup.Forward

      let backwardJson =
        lookup.Backward
        |> Option.map (fun (name, descriptor) ->
          JsonValue.Record
            [| "name", JsonValue.String name.LookupName
               "descriptor", DirectedLookupDescriptor.ToJson descriptor |])
        |> Option.defaultValue JsonValue.Null

      JsonValue.Record
        [| "source", JsonValue.String lookup.Source.EntityName
           "target", JsonValue.String lookup.Target.EntityName
           "forward", forwardJson
           "backward", backwardJson |]

  type Schema<'T, 'Id when 'Id: comparison> with
    static member FromJson(jsonValue: JsonValue) : Reader<Schema<'T, 'Id>, JsonParser<'T> * JsonParser<'Id>, Errors> =
      reader {
        let! jsonValue = jsonValue |> JsonValue.AsRecordMap |> reader.OfSum

        let! entities = jsonValue |> Map.tryFindWithError "entities" "root" "entities" |> reader.OfSum
        let! entities = entities |> JsonValue.AsRecordMap |> reader.OfSum

        let entities =
          entities
          |> Map.toList
          |> List.map (fun (k, v) -> { EntityName = k }, v)
          |> Map.ofList

        let! lookups = jsonValue |> Map.tryFindWithError "lookups" "root" "lookups" |> reader.OfSum
        let! lookups = lookups |> JsonValue.AsRecordMap |> reader.OfSum

        let lookups =
          lookups
          |> Map.toList
          |> List.map (fun (k, v) -> { LookupName = k }, v)
          |> Map.ofList

        let! entitiesMap =
          entities
          |> Map.map (fun _ entityJson ->
            reader {
              let! entityDescriptor = EntityDescriptor<'T, 'Id>.FromJson entityJson
              return entityDescriptor
            })
          |> reader.AllMap

        let! lookupsMap =
          lookups
          |> Map.map (fun _ lookupJson ->
            sum {
              let! lookupDescriptor = LookupDescriptor.FromJson lookupJson
              return lookupDescriptor
            })
          |> sum.AllMap
          |> reader.OfSum


        let! types = jsonValue |> Map.tryFindWithError "types" "root" "types" |> reader.OfSum
        let! types = JsonValue.AsRecord types |> reader.OfSum

        let! ctx, _ = reader.GetContext()

        let! types =
          types
          |> Array.toList
          |> List.map (fun (k, v) -> ctx v |> sum.Map(fun t -> Identifier.LocalScope k, t))
          |> sum.All
          |> reader.OfSum

        return
          { Types = OrderedMap.ofList types
            Entities = entitiesMap
            Lookups = lookupsMap }
      }

    static member ToJson(schema: Schema<'T, 'Id>) : Reader<JsonValue, JsonEncoder<'T> * JsonEncoder<'Id>, Errors> =

      reader {
        let! entitiesJson =
          schema.Entities
          |> Map.map (fun _ -> EntityDescriptor<'T, 'Id>.ToJson)
          |> reader.AllMap

        let lookupsJson =
          schema.Lookups
          |> Map.map (fun _ lookup -> LookupDescriptor.ToJson lookup)
          |> Map.toArray
          |> Array.map (fun (k, v) -> k.LookupName, v)
          |> JsonValue.Record

        let! ctx, _ = reader.GetContext()

        let typesJon =
          schema.Types
          |> OrderedMap.map (fun _k -> ctx)
          |> OrderedMap.toArray
          |> Array.map (fun (idf, v) -> idf.LocalName, v)

        return
          JsonValue.Record
            [| "types", JsonValue.Record typesJon
               "entities",
               entitiesJson
               |> Map.toArray
               |> Array.map (fun (k, v) -> k.EntityName, v)
               |> JsonValue.Record
               "lookups", lookupsJson |]
      }

    static member FromJsonVirtualFolder
      (variant: WorkspaceVariant)
      (root: FolderNode)
      : Sum<Schema<TypeExpr, Identifier>, Errors> =
      sum {
        let! merged =
          getWellKnownFile (Folder root) Merged
          |> sum.OfOption(Errors.Singleton "Attempt to get merged spec failed")

        let! schemaJson =
          match variant with
          | Compose ->
            sum {
              let! content = FileContent.AsJson merged.Content
              let! r = JsonValue.AsRecord content
              return! r |> Map.ofArray |> Map.tryFindWithError "schema" "api spec" "schema"
            }
          | Explore(_split, path) when Transient.has path ->
            sum {
              let path = Transient.value path
              let schemaPath = withFileSuffix "_schema" path.Value
              let typesPath = withFileSuffix "_typesV2" path.Value

              let! schemaContent =
                tryFind schemaPath (Folder root)
                |> sum.OfOption(Errors.Singleton $"Can't evaluate path {path} in vfs")

              let! schemaFile = VfsNode.AsFile schemaContent
              let! schemaJson = FileContent.AsJson schemaFile.Content

              let! typesContent =
                tryFind typesPath (Folder root)
                |> sum.OfOption(Errors.Singleton $"Can't evaluate path {path} in vfs")

              let! typesFile = VfsNode.AsFile typesContent
              let! typesJson = FileContent.AsJson typesFile.Content

              let! schema = JsonValue.AsRecordMap schemaJson
              let schema = Map.add "types" typesJson schema
              return JsonValue.Record(schema |> Map.toArray)
            }
          | Explore(_, _path) -> sum.Throw(Errors.Singleton $"Missing path in exploring vfs")

        let! schema = Schema.FromJson schemaJson |> Reader.Run(TypeExpr.FromJson, Identifier.FromJson)
        return schema
      }
