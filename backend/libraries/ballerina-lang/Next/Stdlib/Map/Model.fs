namespace Ballerina.DSL.Next.StdLib.Map

module Model =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Serialization.PocoObjects

  type MapOperations<'ext> =
    | Map_Set of Option<Value<TypeValue<'ext>, 'ext> * Value<TypeValue<'ext>, 'ext>>
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

  type MapKind =
    | Map = 1

  type MapValueDTO<'extDTO when 'extDTO: not null and 'extDTO: not struct> =
    { Kind: MapKind
      Map: (ValueDTO<'extDTO> * ValueDTO<'extDTO>) list }

    static member CreateMapFromList l : MapValueDTO<'extDTO> = { Kind = MapKind.Map; Map = l }
