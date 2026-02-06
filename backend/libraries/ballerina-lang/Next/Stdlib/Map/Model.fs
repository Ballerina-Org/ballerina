namespace Ballerina.DSL.Next.StdLib.Map

module Model =
  open Ballerina.DSL.Next.Types

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

  // map and maptolist of list

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
