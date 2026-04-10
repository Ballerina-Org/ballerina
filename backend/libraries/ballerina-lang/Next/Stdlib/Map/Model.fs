namespace Ballerina.DSL.Next.StdLib.Map

module Model =
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Serialization.PocoObjects
  open Ballerina.Data.Delta
  open Ballerina.Data.Delta.Serialization.DeltaDTO

  type MapOperations<'ext> =
    | Map_Set of
      Option<Value<TypeValue<'ext>, 'ext> * Value<TypeValue<'ext>, 'ext>>
    | Map_Empty
    | Map_Map of {| f: Option<Value<TypeValue<'ext>, 'ext>> |}
    | Map_MapToList

    override self.ToString() : string =
      match self with
      | Map_Set _ -> "Map::Set"
      | Map_Empty -> "Map::Empty"
      | Map_Map _ -> "Map::map"
      | Map_MapToList -> "Map::maptolist"

  type MapValues<'ext when 'ext: comparison> =
    | Map of Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>

    override self.ToString() : string =
      match self with
      | Map map ->
        let keyValueStr =
          map
          |> Map.toList
          |> List.map (fun (k, v) -> $"{k.ToString()}: {v.ToString()}")
          |> String.concat ", "

        $"{{{keyValueStr}}}"

  type MapValueDTO<'extDTO when 'extDTO: not null and 'extDTO: not struct> =
    (ValueDTO<'extDTO> * ValueDTO<'extDTO>)[]

  type MapDeltaExt<'valueExt, 'deltaExt> =
    | UpdateKey of
      oldKey: Model.Value<Model.TypeValue<'valueExt>, 'valueExt> *
      newKey: Model.Value<Model.TypeValue<'valueExt>, 'valueExt>
    | UpdateValue of
      key: Model.Value<Model.TypeValue<'valueExt>, 'valueExt> *
      delta: Delta<'valueExt, 'deltaExt>
    | AddItem of
      key: Model.Value<Model.TypeValue<'valueExt>, 'valueExt> *
      value: Model.Value<Model.TypeValue<'valueExt>, 'valueExt>
    | RemoveItem of key: Model.Value<Model.TypeValue<'valueExt>, 'valueExt>

  type UpdateMapKeyDTO<'extDTO when 'extDTO: not null and 'extDTO: not struct> =
    { OldKey: ValueDTO<'extDTO>
      NewKey: ValueDTO<'extDTO> }

  type UpdateMapValueDTO<'extDTO, 'deltaExtDTO
    when 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct> =
    { Key: ValueDTO<'extDTO>
      Value: DeltaDTO<'extDTO, 'deltaExtDTO> }

  type AddMapItemDTO<'extDTO when 'extDTO: not null and 'extDTO: not struct> =
    { Key: ValueDTO<'extDTO>
      Value: ValueDTO<'extDTO> }

  type MapDeltaExtDTO<'extDTO, 'deltaExtDTO
    when 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct> =
    { UpdateKey: UpdateMapKeyDTO<'extDTO> | null
      UpdateValue: UpdateMapValueDTO<'extDTO, 'deltaExtDTO> | null
      AddItem: AddMapItemDTO<'extDTO> | null
      RemoveItem: ValueDTO<'extDTO> | null }

    static member Empty =
      { UpdateKey = null
        UpdateValue = null
        AddItem = null
        RemoveItem = null }

    static member CreateUpdateKey
      oldKey
      newKey
      : MapDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { MapDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          UpdateKey = { OldKey = oldKey; NewKey = newKey } }

    static member CreateUpdateValue
      key
      delta
      : MapDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { MapDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          UpdateValue = { Key = key; Value = delta } }

    static member CreateAddItem
      key
      value
      : MapDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { MapDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          AddItem = { Key = key; Value = value } }

    static member CreateRemoveItem key : MapDeltaExtDTO<'extDTO, 'deltaExtDTO> =
      { MapDeltaExtDTO<'extDTO, 'deltaExtDTO>.Empty with
          RemoveItem = key }
