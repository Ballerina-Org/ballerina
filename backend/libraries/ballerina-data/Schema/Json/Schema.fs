namespace Ballerina.Data.Json

module Schema =

  open Ballerina.Reader.WithError
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Data.Model
  open Ballerina.Data.Arity.Model
  open Ballerina.Data
  open FSharp.Data
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Json
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Json

  type EntityMethod with
    static member FromJson(jsonValue: JsonValue) : Sum<EntityMethod, Errors> =
      sum {
        match! jsonValue |> JsonValue.AsString with
        | "get" -> return Get
        | "getMany" -> return GetMany
        | "create" -> return Create
        | "delete" -> return Delete
        | "update" -> return Update
        | _ -> return! $"Invalid entity method: {jsonValue}" |> Errors.Singleton |> sum.Throw
      }

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

  type Updater<'Type> with
    static member FromJson(jsonValue: JsonValue) : Reader<Updater<'Type>, JsonParser<'Type>, Errors> =
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

  type LookupMethod with
    static member FromJson(jsonValue: JsonValue) : Sum<LookupMethod, Errors> =
      sum {
        match! jsonValue |> JsonValue.AsString with
        | "get" -> return LookupMethod.Get
        | "getMany" -> return LookupMethod.GetMany
        | "create" -> return LookupMethod.Create
        | "delete" -> return LookupMethod.Delete
        | "update" -> return LookupMethod.Update
        | "link" -> return LookupMethod.Link
        | "unlink" -> return LookupMethod.Unlink
        | _ -> return! $"Invalid lookup method: {jsonValue}" |> Errors.Singleton |> sum.Throw
      }

  type JsonParser<'T> = JsonValue -> Sum<'T, Errors>

  type EntityDescriptor<'T> with
    static member FromJson(jsonValue: JsonValue) : Reader<EntityDescriptor<'T>, JsonParser<'T>, Errors> =
      reader {
        let! jsonValue = jsonValue |> JsonValue.AsRecordMap |> reader.OfSum
        let! typeValue = jsonValue |> Map.tryFindWithError "type" "entity" "type" |> reader.OfSum
        let! ctx = reader.GetContext()
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
        let! updaters = updaters |> Seq.map Updater<'T>.FromJson |> reader.All

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
              return name, descriptor
            })
          |> sum.RunOption

        return
          { Source = source
            Target = target
            Forward = forward
            Backward = backward }
      }

  type Schema<'T> with
    static member FromJson(jsonValue: JsonValue) : Reader<Schema<'T>, JsonParser<'T>, Errors> =
      reader {
        let! jsonValue = jsonValue |> JsonValue.AsRecordMap |> reader.OfSum

        let! entities = jsonValue |> Map.tryFindWithError "entities" "root" "entities" |> reader.OfSum
        let! entities = entities |> JsonValue.AsRecordMap |> reader.OfSum

        let! lookups = jsonValue |> Map.tryFindWithError "lookups" "root" "lookups" |> reader.OfSum
        let! lookups = lookups |> JsonValue.AsRecordMap |> reader.OfSum

        let! entitiesMap =
          entities
          |> Map.map (fun _ entityJson ->
            reader {
              let! entityDescriptor = EntityDescriptor<'T>.FromJson entityJson
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

        return
          { Entities = entitiesMap
            Lookups = lookupsMap }
      }
