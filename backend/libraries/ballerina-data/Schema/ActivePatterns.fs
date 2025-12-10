namespace Ballerina.Data.Schema

open Ballerina.Cat.Collections.OrderedMap
open Ballerina.DSL.Next.Types
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.Types.Patterns

module ActivePatterns =
  let (|CollectionReference|_|) (m: WithSourceMapping<OrderedMap<TypeSymbol, TypeValue * Kind>>) =
    match OrderedMap.toList m.value with
    | [ (k, _); (k2, _) ] when k.Name.LocalName = "DisplayValue" && k2.Name.LocalName = "Id" ->
      Some(CollectionReference m.value)
    | [ (k, _); (k2, _) ] when k2.Name.LocalName = "DisplayValue" && k.Name.LocalName = "Id" ->
      Some(CollectionReference m.value)
    | _ -> None

  let (|CollectionReferenceValue|_|) (m: Map<ResolvedIdentifier, Value<_, _>>) =
    match Map.toList m with
    | [ (k, _); (k2, _) ] when k.Name = "DisplayValue" && k2.Name = "Id" -> Some(CollectionReferenceValue m)
    | [ (k, _); (k2, _) ] when k2.Name = "DisplayValue" && k.Name = "Id" -> Some(CollectionReferenceValue m)
    | _ -> None
