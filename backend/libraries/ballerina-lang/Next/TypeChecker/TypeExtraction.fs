namespace Ballerina.DSL.Next.Types.TypeChecker

module TypeExtraction =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Cat.Collections.OrderedMap

  type ContainerKind =
    | SetContainer
    | ImportedContainer of containerId: ResolvedIdentifier * argIndex: int

  type ExtractionStep =
    | RecordField of TypeSymbol
    | TupleElement of int
    | UnionCase of TypeSymbol
    | SumAlternative of case: int * count: int
    | ContainerElement of ContainerKind

  type ExtractionTree =
    { SelfMatch: bool
      Children: Map<ExtractionStep, ExtractionTree> }

  let emptyExtractionTree: ExtractionTree =
    { SelfMatch = false
      Children = Map.empty }

  let findExtractionTree<'valueExt>
    (isTarget: TypeValue<'valueExt> -> bool)
    (bindings: TypeBindings<'valueExt>)
    (resolvedIdentifiers: Map<Identifier, ResolvedIdentifier>)
    (hostType: TypeValue<'valueExt>)
    : ExtractionTree =

    let resolveIdentifier (id: Identifier) : ResolvedIdentifier option = Map.tryFind id resolvedIdentifiers

    let mkTree (selfMatch: bool) (children: Map<ExtractionStep, ExtractionTree>) : ExtractionTree option =
      if selfMatch || not (Map.isEmpty children) then
        Some
          { SelfMatch = selfMatch
            Children = children }
      else
        None

    let rec find (visited: Set<ResolvedIdentifier>) (t: TypeValue<'valueExt>) : ExtractionTree option =
      let selfMatch = isTarget t

      let selfMatch', children =
        match t with
        | TypeValue.Lookup id ->
          match resolveIdentifier id with
          | Some resolvedId when not (Set.contains resolvedId visited) ->
            match Map.tryFind resolvedId bindings with
            | Some(typeValue, _) ->
              match find (Set.add resolvedId visited) typeValue with
              | Some resolvedTree -> selfMatch || resolvedTree.SelfMatch, resolvedTree.Children
              | None -> selfMatch, Map.empty
            | None -> selfMatch, Map.empty
          | _ -> selfMatch, Map.empty

        | TypeValue.Record { value = fields } ->
          selfMatch,
          fields
          |> OrderedMap.toList
          |> List.choose (fun (sym, (fieldType, _)) ->
            find visited fieldType |> Option.map (fun child -> RecordField sym, child))
          |> Map.ofList

        | TypeValue.Tuple { value = elements } ->
          selfMatch,
          elements
          |> List.indexed
          |> List.choose (fun (i, elemType) -> find visited elemType |> Option.map (fun child -> TupleElement i, child))
          |> Map.ofList

        | TypeValue.Union { value = cases } ->
          selfMatch,
          cases
          |> OrderedMap.toList
          |> List.choose (fun (sym, caseType) ->
            find visited caseType |> Option.map (fun child -> UnionCase sym, child))
          |> Map.ofList

        | TypeValue.Sum { value = alternatives } ->
          let count = List.length alternatives

          selfMatch,
          alternatives
          |> List.indexed
          |> List.choose (fun (i, altType) ->
            find visited altType
            |> Option.map (fun child -> SumAlternative(i, count), child))
          |> Map.ofList

        | TypeValue.Set { value = elementType } ->
          let children =
            match find visited elementType with
            | Some child -> Map.ofList [ ContainerElement SetContainer, child ]
            | None -> Map.empty

          selfMatch, children

        | TypeValue.Imported imported ->
          let containerId = imported.Id
          let arguments = imported.Arguments

          selfMatch,
          arguments
          |> List.indexed
          |> List.choose (fun (i, argType) ->
            find visited argType
            |> Option.map (fun child -> ContainerElement(ImportedContainer(containerId, i)), child))
          |> Map.ofList

        | TypeValue.Arrow _
        | TypeValue.Primitive _
        | TypeValue.Var _
        | TypeValue.Lambda _
        | TypeValue.Application _
        | TypeValue.Schema _
        | TypeValue.Entities _
        | TypeValue.Relations _
        | TypeValue.Entity _
        | TypeValue.Relation _
        | TypeValue.RelationLookupOption _
        | TypeValue.RelationLookupOne _
        | TypeValue.RelationLookupMany _
        | TypeValue.ForeignKeyRelation _
        | TypeValue.QueryTypeFunction
        | TypeValue.QueryRow _ -> selfMatch, Map.empty

      mkTree selfMatch' children

    find Set.empty hostType |> Option.defaultValue emptyExtractionTree

  // Finds a tree from hostType to positions matching the isTarget predicate,
  // traversing Records, Tuples, Unions, Sums, Sets, and Imported type arguments.
  // Lookup nodes are resolved via bindings/identifiersResolver, with cycle detection.
  // Matching nodes are included and traversal continues into nested structure.
  let findExtractionTreeByName<'valueExt>
    (targetId: ResolvedIdentifier)
    (bindings: TypeBindings<'valueExt>)
    (identifiersResolver: Map<Identifier, ResolvedIdentifier>)
    (hostType: TypeValue<'valueExt>)
    : ExtractionTree =
    let isTarget t =
      match t with
      | TypeValue.Lookup id ->
        match Map.tryFind id identifiersResolver with
        | Some resolvedId -> resolvedId = targetId
        | None -> false
      | _ -> false

    findExtractionTree isTarget bindings identifiersResolver hostType

  // Convenience wrapper that extracts the needed pieces from a TypeCheckState.
  let private tryFindBoundTypeByResolvedId<'valueExt>
    (bindings: TypeBindings<'valueExt>)
    (resolvedId: ResolvedIdentifier)
    : TypeValue<'valueExt> option =
    bindings |> Map.tryFind resolvedId |> Option.map fst

  let private tryFindResolvedIdByName<'valueExt>
    (bindings: TypeBindings<'valueExt>)
    (name: string)
    : ResolvedIdentifier option =
    bindings |> Map.toSeq |> Seq.map fst |> Seq.tryFind (fun rid -> rid.Name = name)

  let private tryResolveLookupToBoundType<'valueExt when 'valueExt: comparison>
    (state: TypeCheckState<'valueExt>)
    (id: Identifier)
    : TypeValue<'valueExt> option =
    let fallbackResolvedId = TypeCheckScope.Empty.Resolve id

    let resolvedIdOpt =
      state.Symbols.IdentifiersResolver
      |> Map.tryFind id
      |> Option.orElseWith (fun () -> tryFindResolvedIdByName state.Bindings fallbackResolvedId.Name)

    resolvedIdOpt |> Option.bind (tryFindBoundTypeByResolvedId state.Bindings)

  let findExtractionTreeFromState<'valueExt when 'valueExt: comparison>
    (isTarget: TypeValue<'valueExt> -> bool)
    (state: TypeCheckState<'valueExt>)
    (hostType: TypeValue<'valueExt>)
    : ExtractionTree =
    let hostType' =
      match hostType with
      | TypeValue.Lookup id -> tryResolveLookupToBoundType state id |> Option.defaultValue hostType
      | _ -> hostType

    findExtractionTree isTarget state.Bindings state.Symbols.IdentifiersResolver hostType'
