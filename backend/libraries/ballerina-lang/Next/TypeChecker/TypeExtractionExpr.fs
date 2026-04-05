namespace Ballerina.DSL.Next.Types.TypeChecker

module TypeExtractionExpr =
  open Ballerina.State.Simple
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.TypeExtraction

  type ListOps =
    { Nil: ResolvedIdentifier
      Cons: ResolvedIdentifier
      Append: ResolvedIdentifier
      Fold: ResolvedIdentifier }

  type MapOps = { MapToList: ResolvedIdentifier }

  type private CompileContext<'valueExt> =
    { ListOps: ListOps
      MapOps: MapOps
      Resolve: TypeSymbol -> ResolvedIdentifier }

  let private freshVar<'c> (prefix: string) : State<Var * ResolvedIdentifier, 'c, int> =
    state {
      let! current = state.GetState()
      let next = current + 1
      do! state.SetState(fun _ -> next)
      let name = sprintf "%s_%d" prefix next
      return Var.Create name, TypeCheckScope.Empty.Resolve(Identifier.LocalScope name)
    }

  // Compiles an extraction tree into an Expr that returns List<target>.
  let rec private compileTree<'valueExt>
    (tree: ExtractionTree)
    (current: TypeCheckedExpr<'valueExt>)
    : State<TypeCheckedExpr<'valueExt>, CompileContext<'valueExt>, int> =

    state {
      let! ctx = state.GetContext()

      let nilExpr: TypeCheckedExpr<'valueExt> =
        TypeCheckedExpr.Apply(TypeCheckedExpr.Lookup(ctx.ListOps.Nil), TypeCheckedExpr.Primitive(PrimitiveValue.Unit))

      let singleton value =
        TypeCheckedExpr.Apply(TypeCheckedExpr.Lookup(ctx.ListOps.Cons), TypeCheckedExpr.TupleCons([ value; nilExpr ]))

      let appendTwo left right =
        TypeCheckedExpr.Apply(TypeCheckedExpr.Apply(TypeCheckedExpr.Lookup(ctx.ListOps.Append), left), right)

      let appendMany (lists: TypeCheckedExpr<'valueExt> list) =
        match lists with
        | [] -> nilExpr
        | first :: rest -> rest |> List.fold appendTwo first

      let selfMatchExpr = if tree.SelfMatch then [ singleton current ] else []

      let compileUnionCase sym childTree =
        state {
          let! caseVar, caseId = freshVar "case"
          let! innerExpr = compileTree childTree (TypeCheckedExpr.Lookup caseId)
          let handlers = Map.ofList [ ctx.Resolve sym, (Some caseVar, innerExpr) ]
          let fallback = Some nilExpr
          return TypeCheckedExpr.Apply(TypeCheckedExpr.UnionDes(handlers, fallback), current)
        }

      let compileSumAlternative case count childTree =
        state {
          let! caseVar, caseId = freshVar "case"
          let! innerExpr = compileTree childTree (TypeCheckedExpr.Lookup caseId)
          let! dummyVar, _ = freshVar "unused"

          let handlers =
            // Sum selectors in runtime values are 1-based.
            [ 1..count ]
            |> List.map (fun i ->
              let selector = { Case = i; Count = count }

              if i = case + 1 then
                selector, (Some caseVar, innerExpr)
              else
                selector, (Some dummyVar, nilExpr))
            |> Map.ofList

          return TypeCheckedExpr.Apply(TypeCheckedExpr.SumDes(handlers), current)
        }

      let compileContainer kind childTree =
        let current', childTree' =
          match kind with
          | ImportedContainer(containerId, argIndex) when containerId.Name = "Map" ->
            let mapAsList =
              TypeCheckedExpr.Apply(TypeCheckedExpr.Lookup(ctx.MapOps.MapToList), current)

            let liftedTree =
              { SelfMatch = false
                Children = Map.ofList [ TupleElement argIndex, childTree ] }

            mapAsList, liftedTree
          | _ -> current, childTree

        state {
          let directMatches = if childTree'.SelfMatch then [ current' ] else []

          if Map.isEmpty childTree'.Children then
            return appendMany directMatches
          else
            let childOnlyTree = { childTree' with SelfMatch = false }

            let! accVar, accId = freshVar "acc"
            let! xVar, xId = freshVar "x"
            let! innerList = compileTree childOnlyTree (TypeCheckedExpr.Lookup xId)

            let appendExpr =
              TypeCheckedExpr.Apply(
                TypeCheckedExpr.Apply(TypeCheckedExpr.Lookup(ctx.ListOps.Append), TypeCheckedExpr.Lookup(accId)),
                innerList
              )

            let foldFn =
              TypeCheckedExpr.Lambda(
                accVar,
                TypeValue.CreatePrimitive(PrimitiveType.Unit),
                TypeCheckedExpr.Lambda(
                  xVar,
                  TypeValue.CreatePrimitive(PrimitiveType.Unit),
                  appendExpr,
                  TypeValue.CreatePrimitive(PrimitiveType.Unit)
                ),
                TypeValue.CreatePrimitive(PrimitiveType.Unit)
              )

            let foldedMatches =
              TypeCheckedExpr.Apply(
                TypeCheckedExpr.Apply(TypeCheckedExpr.Apply(TypeCheckedExpr.Lookup(ctx.ListOps.Fold), foldFn), nilExpr),
                current'
              )

            return appendMany (directMatches @ [ foldedMatches ])
        }

      let rec compileGroups (groups: (ExtractionStep * ExtractionTree) list) (acc: TypeCheckedExpr<'valueExt> list) =
        state {

          match groups with
          | [] -> return List.rev acc
          | (step, childTree) :: restGroups ->
            let! compiled =
              match step with
              | RecordField sym -> compileTree childTree (TypeCheckedExpr.RecordDes(current, ctx.Resolve sym))

              | TupleElement i -> compileTree childTree (TypeCheckedExpr.TupleDes(current, { Index = i + 1 }))

              | UnionCase sym -> compileUnionCase sym childTree

              | SumAlternative(case, count) -> compileSumAlternative case count childTree

              | ContainerElement kind -> compileContainer kind childTree

            return! compileGroups restGroups (compiled :: acc)
        }

      let! branchExprs = compileGroups (tree.Children |> Map.toList) []
      return appendMany (selfMatchExpr @ branchExprs)
    }

  let rec private collectSymbolsFromTree (tree: ExtractionTree) : Set<TypeSymbol> =
    let symbolsFromStep (step: ExtractionStep) =
      match step with
      | RecordField sym
      | UnionCase sym -> Set.singleton sym
      | _ -> Set.empty

    tree.Children
    |> Map.toList
    |> List.fold
      (fun acc (step, child) -> Set.unionMany [ acc; symbolsFromStep step; collectSymbolsFromTree child ])
      Set.empty

  // Builds a lambda Expr (host -> List<target>) from an extraction tree.
  // symbolResolver maps TypeSymbols (from type-level records/unions) to
  // ResolvedIdentifiers (used in value-level records/union cases).
  let buildExtractionExpr<'valueExt>
    (listOps: ListOps)
    (mapOps: MapOps)
    (symbolResolver: Map<TypeSymbol, ResolvedIdentifier>)
    (tree: ExtractionTree)
    : TypeCheckedExpr<'valueExt> =
    let missing =
      collectSymbolsFromTree tree
      |> Set.filter (fun sym -> not (Map.containsKey sym symbolResolver))

    if not (Set.isEmpty missing) then
      failwithf
        "TypeExtractionExpr.buildExtractionExpr: missing symbol mappings for %A"
        (missing |> Set.toList |> List.map string)

    let resolve (sym: TypeSymbol) = Map.find sym symbolResolver

    let compileCtx =
      { ListOps = listOps
        MapOps = mapOps
        Resolve = resolve }

    let expressionState =
      state {
        let! hostVar, hostId = freshVar "host"
        let! body = compileTree tree (TypeCheckedExpr.Lookup hostId)

        return
          TypeCheckedExpr.Lambda(
            hostVar,
            TypeValue.CreatePrimitive(PrimitiveType.Unit),
            body,
            TypeValue.CreatePrimitive(PrimitiveType.Unit)
          )
      }

    let (result, _) = expressionState.run (compileCtx, 0)
    result

  // Convenience: finds extraction tree and builds the extraction Expr in one step.
  let buildTypeExtractor<'valueExt when 'valueExt: comparison>
    (listOps: ListOps)
    (mapOps: MapOps)
    (isTarget: TypeValue<'valueExt> -> bool)
    (state: TypeCheckState<'valueExt>)
    (hostType: TypeValue<'valueExt>)
    : TypeCheckedExpr<'valueExt> =
    let tree = findExtractionTreeFromState isTarget state hostType

    buildExtractionExpr listOps mapOps state.Symbols.ResolvedIdentifiers tree
