namespace Ballerina.DSL.Next.StdLib.Map

[<AutoOpen>]
module Extension =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Types
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.Map.Model
  open Ballerina.DSL.Next.StdLib.Map.Patterns
  open Ballerina.DSL.Next.StdLib.List.Model
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina
  open Ballerina.DSL.Next.Serialization.PocoObjects
  open Ballerina.DSL.Next.Serialization
  open Ballerina.DSL.Next.Serialization.ValueSerializer
  open Ballerina.DSL.Next.Serialization.ValueDeserializer


  let MapExtension<'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct>
    (valueLens: PartialLens<'ext, MapValues<'ext>>)
    (operationLens: PartialLens<'ext, MapOperations<'ext>>)
    (listValueLens: Option<PartialLens<'ext, ListValues<'ext>>>)
    (valueDTOLens: PartialLens<'extDTO, MapValueDTO<'extDTO>>)
    : TypeExtension<'ext, 'extDTO, 'deltaExt, 'deltaExtDTO, Unit, MapValues<'ext>, MapOperations<'ext>> =
    let mapId = Identifier.LocalScope "Map"
    let mapSymbolId = mapId |> TypeSymbol.Create
    let kVar, kKind = TypeVar.Create("k"), Kind.Star
    let vVar, vKind = TypeVar.Create("v"), Kind.Star
    let mapId = mapId |> TypeCheckScope.Empty.Resolve

    let mapOf (keyArgName: string) (valueArgName: string) =
      TypeExpr.Apply(
        TypeExpr.Apply(TypeExpr.Lookup(Identifier.LocalScope "Map"), TypeExpr.Lookup(Identifier.LocalScope keyArgName)),
        TypeExpr.Lookup(Identifier.LocalScope valueArgName)
      )

    let mapMapId =
      Identifier.FullyQualified([ "Map" ], "map") |> TypeCheckScope.Empty.Resolve

    let mapSetId =
      Identifier.FullyQualified([ "Map" ], "set") |> TypeCheckScope.Empty.Resolve

    let mapEmptyId =
      Identifier.FullyQualified([ "Map" ], "Empty") |> TypeCheckScope.Empty.Resolve

    let mapMapToListId =
      Identifier.FullyQualified([ "Map" ], "maptoList")
      |> TypeCheckScope.Empty.Resolve

    let getValueAsMap
      (v: Value<TypeValue<'ext>, 'ext>)
      : Sum<Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>, Errors<Unit>> =
      sum {
        let! v, _ = v |> Value.AsExt

        let! v =
          valueLens.Get v
          |> sum.OfOption((fun () -> $"cannot get map value") |> Errors<Unit>.Singleton())

        let! v = v |> MapValues.AsMap
        v
      }

    let mapOperation: ResolvedIdentifier * TypeOperationExtension<'ext, Unit, MapValues<'ext>, MapOperations<'ext>> =
      mapMapId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("k", kKind),
            TypeExpr.Lambda(
              TypeParameter.Create("v", vKind),
              TypeExpr.Lambda(
                TypeParameter.Create("v'", Kind.Star),
                TypeExpr.Arrow(
                  TypeExpr.Arrow(
                    TypeExpr.Lookup(Identifier.LocalScope "v"),
                    TypeExpr.Lookup(Identifier.LocalScope "v'")
                  ),
                  TypeExpr.Arrow(mapOf "k" "v", mapOf "k" "v'")
                )
              )
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))
        Operation = Map_Map {| f = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Map_Map v -> Some(Map_Map v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MapOperations.AsMap
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application (value function)
                return
                  (MapOperations.Map_Map({| f = Some v |}) |> operationLens.Set, Some mapMapId)
                  |> Ext
              | Some f -> // the closure has the value function - second step in the application (the map)
                let! map =
                  getValueAsMap v
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! newMap =
                  map
                  |> Map.toList
                  |> List.map (fun (k, v) ->
                    reader {
                      let! newValue = Expr.EvalApply loc0 [] (f, v)
                      return (k, newValue)
                    })
                  |> reader.All

                return (MapValues.Map(Map.ofList newMap) |> valueLens.Set, None) |> Ext
            } }

    let setOperation: ResolvedIdentifier * TypeOperationExtension<'ext, Unit, MapValues<'ext>, MapOperations<'ext>> =
      mapSetId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("k", kKind),
            TypeExpr.Lambda(
              TypeParameter.Create("v", vKind),
              TypeExpr.Arrow(
                TypeExpr.Tuple(
                  [ TypeExpr.Lookup(Identifier.LocalScope "k")
                    TypeExpr.Lookup(Identifier.LocalScope "v") ]
                ),
                TypeExpr.Arrow(mapOf "k" "v", mapOf "k" "v")
              )
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
        Operation = Map_Set None
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Map_Set v -> Some(Map_Set v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MapOperations.AsSet
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application (key, value tuple)
                let! items =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match items with
                | [ key; value ] ->
                  return
                    (MapOperations.Map_Set(Some(key, value)) |> operationLens.Set, Some mapSetId)
                    |> Ext
                | _ ->
                  return!
                    (fun () -> "Error: expected pair (key, value)")
                    |> Errors.Singleton loc0
                    |> reader.Throw
              | Some(key, value) -> // the closure has the key-value pair - second step in the application (the map)
                let! map, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! map =
                  map
                  |> valueLens.Get
                  |> sum.OfOption((fun () -> "Error: expected map") |> Errors.Singleton loc0)
                  |> reader.OfSum

                let! map =
                  map
                  |> MapValues.AsMap
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return (MapValues.Map(Map.add key value map) |> valueLens.Set, None) |> Ext
            } }

    let emptyOperation: ResolvedIdentifier * TypeOperationExtension<'ext, Unit, MapValues<'ext>, MapOperations<'ext>> =
      mapEmptyId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("k", kKind),
            TypeExpr.Lambda(
              TypeParameter.Create("v", vKind),
              TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Unit, mapOf "k" "v")
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
        Operation = Map_Empty
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Map_Empty -> Some Map_Empty
            | _ -> None)
        Apply =
          fun loc0 _rest (op, _) ->
            reader {
              do!
                op
                |> MapOperations.AsEmpty
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              return (MapValues.Map(Map.empty) |> valueLens.Set, None) |> Ext
            } }

    let maptolistOperation
      : ResolvedIdentifier * TypeOperationExtension<'ext, Unit, MapValues<'ext>, MapOperations<'ext>> =
      mapMapToListId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("k", kKind),
            TypeExpr.Lambda(
              TypeParameter.Create("v", vKind),
              TypeExpr.Arrow(
                mapOf "k" "v",
                TypeExpr.Apply(
                  TypeExpr.Lookup(Identifier.LocalScope "List"),
                  TypeExpr.Tuple(
                    [ TypeExpr.Lookup(Identifier.LocalScope "k")
                      TypeExpr.Lookup(Identifier.LocalScope "v") ]
                  )
                )
              )
            )
          )
        Kind = Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
        Operation = Map_MapToList
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Map_MapToList -> Some Map_MapToList
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> MapOperations.AsMapToList
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! map =
                getValueAsMap v
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let tupleList = map |> Map.toList |> List.map (fun (k, v) -> Value.Tuple([ k; v ]))

              match listValueLens with
              | Some listLens -> return (ListValues.List tupleList |> listLens.Set, None) |> Ext
              | None ->
                return!
                  (fun () -> "Error: Map::maptolist requires List extension value lens")
                  |> Errors.Singleton loc0
                  |> reader.Throw
            } }

    let mapToDTO
      (value: 'ext)
      (applicableId: Option<ResolvedIdentifierDTO>)
      : Reader<ValueDTO<'extDTO>, SerializationContext<'ext, 'extDTO>, Ballerina.Errors.Errors<unit>> =
      reader {
        let! Map mapValue =
          value
          |> valueLens.Get
          |> sum.OfOption(Ballerina.Errors.Errors.Singleton () (fun _ -> "Expected map value in mapToDTO."))
          |> reader.OfSum

        let! pairs =
          mapValue
          |> Map.toList
          |> List.map (fun (k, v) ->
            reader {
              let! kDTO = valueToDTO k
              let! vDTO = valueToDTO v
              return (kDTO, vDTO)
            })
          |> reader.All

        return
          MapValueDTO.CreateMapFromList pairs
          |> valueDTOLens.Set
          |> ValueDTO.CreateExt applicableId
      }

    let DTOToMap
      (valueDTO: 'extDTO)
      (applicableId: Option<ResolvedIdentifier>)
      : Reader<Value<TypeValue<'ext>, 'ext>, SerializationContext<'ext, 'extDTO>, Ballerina.Errors.Errors<unit>> =
      reader {
        let! mapValueDTO =
          valueDTO
          |> valueDTOLens.Get
          |> sum.OfOption(Ballerina.Errors.Errors.Singleton () (fun _ -> "Expected map value DTO in DTOToMap."))
          |> reader.OfSum

        let! mapElements =
          mapValueDTO.Map
          |> List.map (fun (kDTO, vDTO) ->
            reader {
              let! k = valueFromDTO kDTO
              let! v = valueFromDTO vDTO
              return (k, v)
            })
          |> reader.All

        return Ext(MapValues.Map(mapElements |> Map.ofList) |> valueLens.Set, applicableId)
      }

    { TypeName = mapId, mapSymbolId
      TypeVars = [ (kVar, kKind); (vVar, vKind) ]
      Cases = Map.empty
      Operations = [ setOperation; emptyOperation; mapOperation; maptolistOperation ] |> Map.ofList
      // Deconstruct =
      //   fun (v: MapValues<'ext>) ->
      //     match v with
      //     | MapValues.Map map when not (Map.isEmpty map) ->
      //       let (firstKey, firstValue) = map |> Map.toList |> List.head
      //       let rest = map |> Map.remove firstKey |> MapValues.Map
      //       Value<TypeValue<'ext>, 'ext>.Tuple([ firstKey; firstValue; (rest |> valueLens.Set, None) |> Ext ])
      //     | _ -> Value<TypeValue<'ext>, 'ext>.Primitive PrimitiveValue.Unit
      Serialization =
        Some
          { SerializationContext = { ToDTO = mapToDTO; FromDTO = DTOToMap }
            ToDTO = fun _ -> reader.Throw(Errors.Singleton () (fun _ -> "Unsupported delta conversion to DTO for Map."))
            FromDTO =
              fun _ -> reader.Throw(Errors.Singleton () (fun _ -> "Unsupported delta conversion from DTO for Map.")) } }
