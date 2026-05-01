namespace Ballerina.DSL.Next.Types.TypeChecker

[<AutoOpen>]
module Patterns =
  open Ballerina.Fun
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.EquivalenceClasses
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  type TypeExprEvalSymbols with

    static member Empty =
      { Types = Map.empty
        ResolvedIdentifiers = Map.empty
        IdentifiersResolver = Map.empty
        UnionCases = Map.empty
        RecordFields = Map.empty }

    static member CreateFromTypeSymbols
      (symbols: Map<ResolvedIdentifier, TypeSymbol>)
      : TypeExprEvalSymbols =
      { TypeExprEvalSymbols.Empty with
          Types = symbols }


  type TypeCheckContext<'valueExt> with
    static member Empty
      (assembly: string, _module: string)
      : TypeCheckContext<'valueExt> =
      { Scope =
          { Assembly = assembly
            Module = _module
            Type = None }
        IsTypeCheckingLetValue = false
        TypeVariables = Map.empty
        TypeParameters = Map.empty
        Values = Map.empty
        BackgroundHooksExtraScope = Map.empty
        PermissionHooksExtraScope = Map.empty
        ViewRejectedIdentifiers = Map.empty
        CoRejectedIdentifiers = Map.empty
        RejectedIdentifiers = Map.empty
        ViewAttributeSchemas = Map.empty }

    static member Updaters =
      {| Scope =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with Scope = c.Scope |> u }
         IsTypeCheckingLetValue =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with
                IsTypeCheckingLetValue = c.IsTypeCheckingLetValue |> u }
         TypeVariables =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with
                TypeVariables = c.TypeVariables |> u }
         TypeParameters =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with
                TypeParameters = c.TypeParameters |> u }
         Values =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with Values = c.Values |> u }
         BackgroundHooksExtraScope =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with
                BackgroundHooksExtraScope = c.BackgroundHooksExtraScope |> u }
         PermissionHooksExtraScope =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with
                PermissionHooksExtraScope = c.PermissionHooksExtraScope |> u }
         ViewRejectedIdentifiers =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with
                ViewRejectedIdentifiers = c.ViewRejectedIdentifiers |> u }
         CoRejectedIdentifiers =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with
                CoRejectedIdentifiers = c.CoRejectedIdentifiers |> u }
         RejectedIdentifiers =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with
                RejectedIdentifiers = c.RejectedIdentifiers |> u }
         ViewAttributeSchemas =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with
                ViewAttributeSchemas = c.ViewAttributeSchemas |> u } |}

    static member FromInstantiateContext<'ve when 've: comparison>
      (ctx: TypeInstantiateContext<'ve>)
      : TypeCheckContext<'ve> =
      { Scope = ctx.Scope
        IsTypeCheckingLetValue = false
        TypeVariables = ctx.TypeVariables
        TypeParameters = ctx.TypeParameters
        Values = ctx.Values
        BackgroundHooksExtraScope = ctx.BackgroundHooksExtraScope
        PermissionHooksExtraScope = ctx.PermissionHooksExtraScope
        ViewRejectedIdentifiers = Map.empty
        CoRejectedIdentifiers = Map.empty
        RejectedIdentifiers = Map.empty
        ViewAttributeSchemas = Map.empty }

    static member tryFindTypeVariable
      (v: string, loc: Location)
      : Reader<
          TypeValue<'valueExt> * Kind,
          TypeCheckContext<'valueExt>,
          Errors<Location>
         >
      =
      reader {
        let! s = reader.GetContext()

        return!
          s.TypeVariables
          |> Map.tryFindWithError
            v
            "type variables"
            (fun () -> v.AsFSharpString)
            loc
          |> reader.OfSum
      }

    static member tryFindTypeParameter
      (v: string, loc: Location)
      : Reader<Kind, TypeCheckContext<'valueExt>, Errors<Location>> =
      reader {
        let! s = reader.GetContext()

        return!
          s.TypeParameters
          |> Map.tryFindWithError
            v
            "type parameters"
            (fun () -> v.AsFSharpString)
            loc
          |> reader.OfSum
      }

  type UnificationState<'valueExt when 'valueExt: comparison> with
    static member Empty: UnificationState<'valueExt> =
      { Classes = EquivalenceClasses.Empty }

    static member Create
      (classes: EquivalenceClasses<TypeVar, TypeValue<'valueExt>>)
      : UnificationState<'valueExt> =
      { Classes = classes }

    static member EnsureVariableExists
      var
      (classes: UnificationState<'valueExt>)
      =
      EquivalenceClasses.EnsureVariableExists var classes.Classes
      |> UnificationState.Create

    static member Updaters =
      {| Classes =
          fun
              (u: Updater<EquivalenceClasses<TypeVar, TypeValue<'valueExt>>>)
              (c: UnificationState<'valueExt>) ->
            { c with Classes = c.Classes |> u } |}

  type TypeCheckState<'valueExt when 'valueExt: comparison> with
    static member Empty: TypeCheckState<'valueExt> =
      { Bindings = Map.empty
        UnionCases = Map.empty
        RecordFields = Map.empty
        Symbols = TypeExprEvalSymbols.Empty
        Vars = UnificationState.Empty
        InlayHints = Map.empty
        DotAccessHints = Map.empty
        ScopeAccessHints = Map.empty
        ScopePrefixHints = Map.empty
        VarsVersion = 0
        MemoInstantiateVar = Map.empty }

    static member Create
      (bindings: TypeBindings<'valueExt>, symbols: TypeExprEvalSymbols)
      : TypeCheckState<'valueExt> =
      { TypeCheckState.Empty with
          Bindings = bindings
          Symbols = symbols }

    static member CreateFromUnificationState
      (vars: UnificationState<'valueExt>)
      : TypeCheckState<'valueExt> =
      { TypeCheckState.Empty with
          Vars = vars }

    static member CreateFromSymbols
      (symbols: TypeExprEvalSymbols)
      : TypeCheckState<'valueExt> =
      { TypeCheckState.Empty with
          Symbols = symbols }

    static member CreateFromTypeSymbols
      (symbols: Map<ResolvedIdentifier, TypeSymbol>)
      : TypeCheckState<'valueExt> =
      { TypeCheckState.Empty with
          Symbols =
            { TypeExprEvalSymbols.Empty with
                Types = symbols } }

    static member tryFindType
      (v: ResolvedIdentifier, loc: Location)
      : Reader<
          TypeValue<'valueExt> * Kind,
          TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =
      reader {
        let! s = reader.GetContext()

        return!
          s.Bindings
          |> Map.tryFindWithError v "bindings" (fun () -> v.AsFSharpString) loc
          |> reader.OfSum
      }

    static member tryFindUnionCaseConstructor
      (v: ResolvedIdentifier, loc: Location)
      : Reader<
          TypeValue<'valueExt> *
          List<TypeParameter> *
          OrderedMap<TypeSymbol, TypeValue<'valueExt>>,
          TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =
      reader {
        let! s = reader.GetContext()

        return!
          s.UnionCases
          |> Map.tryFindWithError
            v
            "union cases"
            (fun () -> v.AsFSharpString)
            loc
          |> reader.OfSum
      }

    static member tryFindRecordField
      (v: ResolvedIdentifier, loc: Location)
      : Reader<
          OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind> *
          TypeValue<'valueExt>,
          TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =
      reader {
        let! s = reader.GetContext()

        return!
          s.RecordFields
          |> Map.tryFindWithError
            v
            "record fields"
            (fun () -> v.AsFSharpString)
            loc
          |> reader.OfSum
      }

    static member tryFindTypeSymbol
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeSymbol, TypeCheckState<'valueExt>, Errors<Location>> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.Types
          |> Map.tryFindWithError
            v
            "type symbols"
            (fun () -> v.AsFSharpString)
            loc
          |> reader.OfSum
      }

    static member tryFindRecordFieldSymbol
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeSymbol, TypeCheckState<'valueExt>, Errors<Location>> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.RecordFields
          |> Map.tryFindWithError
            v
            "record field symbols"
            (fun () -> v.AsFSharpString)
            loc
          |> reader.OfSum
      }

    static member tryFindUnionCaseSymbol
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeSymbol, TypeCheckState<'valueExt>, Errors<Location>> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.UnionCases
          |> Map.tryFindWithError
            v
            "union case symbols"
            (fun () -> v.AsFSharpString)
            loc
          |> reader.OfSum
      }

    static member tryFindResolvedIdentifier
      (v: TypeSymbol, loc: 'err_ctx)
      : Reader<ResolvedIdentifier, TypeCheckState<'valueExt>, Errors<'err_ctx>> =
      reader {
        let! ctx = reader.GetContext()

        return!
          ctx.Symbols.ResolvedIdentifiers
          |> Map.tryFindWithError
            v
            "resolved identifiers"
            (fun () -> v.AsFSharpString)
            loc
          |> reader.OfSum
      }

    static member Updaters =
      {| Vars =
          fun
              (u: Updater<UnificationState<'valueExt>>)
              (c: TypeCheckState<'valueExt>) ->
            { c with
                Vars = c.Vars |> u
                VarsVersion = c.VarsVersion + 1 }
         Bindings =
          fun u (c: TypeCheckState<'valueExt>) ->
            { c with Bindings = c.Bindings |> u }
         InlayHints =
          fun u (c: TypeCheckState<'valueExt>) ->
            { c with
                InlayHints = c.InlayHints |> u }
         DotAccessHints =
          fun u (c: TypeCheckState<'valueExt>) ->
            { c with
                DotAccessHints = c.DotAccessHints |> u }
         ScopeAccessHints =
          fun u (c: TypeCheckState<'valueExt>) ->
            { c with
                ScopeAccessHints = c.ScopeAccessHints |> u }
         ScopePrefixHints =
          fun u (c: TypeCheckState<'valueExt>) ->
            { c with
                ScopePrefixHints = c.ScopePrefixHints |> u }
         UnionCases =
          fun u (c: TypeCheckState<'valueExt>) ->
            { c with
                UnionCases = c.UnionCases |> u }
         RecordFields =
          fun u (c: TypeCheckState<'valueExt>) ->
            { c with
                RecordFields = c.RecordFields |> u }
         Symbols =
          {| Types =
              fun u (c: TypeCheckState<'valueExt>) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          Types = c.Symbols.Types |> u } }
             ResolvedIdentifiers =
              fun u (c: TypeCheckState<'valueExt>) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          ResolvedIdentifiers =
                            c.Symbols.ResolvedIdentifiers |> u } }
             IdentifiersResolver =
              fun u (c: TypeCheckState<'valueExt>) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          IdentifiersResolver =
                            c.Symbols.IdentifiersResolver |> u } }
             RecordFields =
              fun u (c: TypeCheckState<'valueExt>) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          RecordFields = c.Symbols.RecordFields |> u } }
             UnionCases =
              fun u (c: TypeCheckState<'valueExt>) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          UnionCases = c.Symbols.UnionCases |> u } } |}
         MemoInstantiateVar =
          fun u (c: TypeCheckState<'valueExt>) ->
            { c with
                MemoInstantiateVar = c.MemoInstantiateVar |> u } |}

    static member unbindType x =
      state {
        do! state.SetState(TypeCheckState.Updaters.Bindings(Map.remove x))
      }

    static member bindType x t_x =
      state {
        do! state.SetState(TypeCheckState.Updaters.Bindings(Map.add x t_x))
      }

    static member bindUnionCaseConstructor (x: ResolvedIdentifier) t_x =
      state {
        do! state.SetState(TypeCheckState.Updaters.UnionCases(Map.add x t_x))
        let x = x.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve
        do! state.SetState(TypeCheckState.Updaters.UnionCases(Map.add x t_x))
      }

    static member bindRecordField (x: ResolvedIdentifier) t_x =
      state {
        do! state.SetState(TypeCheckState.Updaters.RecordFields(Map.add x t_x))
        let x = x.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve
        do! state.SetState(TypeCheckState.Updaters.RecordFields(Map.add x t_x))
      }

    static member bindRecordFieldSymbol x t_x =
      state {
        do!
          state.SetState(
            TypeCheckState.Updaters.Symbols.ResolvedIdentifiers(Map.add t_x x)
          )

        do!
          state.SetState(
            TypeCheckState.Updaters.Symbols.RecordFields(Map.add x t_x)
          )
      }

    static member bindIdentifierToResolvedIdentifier x t_x =
      state {
        do!
          state.SetState(
            TypeCheckState.Updaters.Symbols.IdentifiersResolver(Map.add t_x x)
          )
      }

    static member bindUnionCaseSymbol x t_x =
      state {
        do!
          state.SetState(
            TypeCheckState.Updaters.Symbols.ResolvedIdentifiers(Map.add t_x x)
          )

        do!
          state.SetState(
            TypeCheckState.Updaters.Symbols.UnionCases(Map.add x t_x)
          )
      }

    static member bindTypeSymbol x t_x =
      state {
        do!
          state.SetState(
            TypeCheckState.Updaters.Symbols.ResolvedIdentifiers(Map.add t_x x)
          )

        do! state.SetState(TypeCheckState.Updaters.Symbols.Types(Map.add x t_x))
      }

    static member bindInlayHint
      (location: Location, identifier: string, hintType: TypeValue<'valueExt>)
      =
      state {
        do!
          state.SetState(
            TypeCheckState.Updaters.InlayHints(
              Map.add
                location
                { Identifier = identifier
                  Type = hintType }
            )
          )
      }

    static member unbindInlayHint(location: Location) =
      state {
        do!
          state.SetState(
            TypeCheckState.Updaters.InlayHints(Map.remove location)
          )
      }

    static member bindDotAccessHint
      (location: Location,
       objectType: TypeValue<'valueExt>,
       availableFields: Map<string, TypeValue<'valueExt>>)
      =
      state {
        do!
          state.SetState(
            TypeCheckState.Updaters.DotAccessHints(
              Map.add
                location
                { ObjectType = objectType
                  AvailableFields = availableFields }
            )
          )
      }

    static member bindScopeAccessHint
      (location: Location,
       prefix: string,
       availableSymbols: Map<string, string>)
      =
      state {
        do!
          state.SetState(
            TypeCheckState.Updaters.ScopeAccessHints(
              Map.add
                location
                { Prefix = prefix
                  AvailableSymbols = availableSymbols }
            )
          )
      }

    static member ComputeScopePrefixHints
      (ctx: TypeCheckContext<'valueExt>)
      (st: TypeCheckState<'valueExt>)
      : Map<string, Map<string, string>> =
      let fromValues =
        ctx.Values
        |> Map.toSeq
        |> Seq.choose (fun (rid, (tv, _)) ->
          match rid.Type with
          | Some prefix -> Some(prefix, rid.Name, tv.ToInlayString())
          | None -> None)

      let fromBindings =
        st.Bindings
        |> Map.toSeq
        |> Seq.choose (fun (rid, (tv, _)) ->
          match rid.Type with
          | Some prefix -> Some(prefix, rid.Name, tv.ToInlayString())
          | None -> None)

      Seq.append fromValues fromBindings
      |> Seq.groupBy (fun (prefix, _, _) -> prefix)
      |> Seq.map (fun (prefix, entries) ->
        prefix,
        entries
        |> Seq.map (fun (_, name, typeStr) -> name, typeStr)
        |> Map.ofSeq)
      |> Map.ofSeq

  type UnificationContext<'valueExt when 'valueExt: comparison> with

    static member Empty: UnificationContext<'valueExt> =
      { EvalState = TypeCheckState.Empty
        Scope = TypeCheckScope.Empty
        TypeParameters = Map.empty }

    static member Create(x) : UnificationContext<'valueExt> =
      { EvalState = TypeCheckState.Create x
        Scope = TypeCheckScope.Empty
        TypeParameters = Map.empty }

    static member Updaters =
      {| EvalState =
          fun f (ctx: UnificationContext<'valueExt>) ->
            { ctx with
                UnificationContext.EvalState = f ctx.EvalState }
         Scope =
          fun f (ctx: UnificationContext<'valueExt>) ->
            { ctx with
                UnificationContext.Scope = f ctx.Scope } |}

  type TypeCheckContext<'valueExt> with
    static member TryFindVar<'ve when 've: comparison>
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<TypeValue<'ve> * Kind, 've> =
      state {
        let! ctx = state.GetContext()

        return!
          ctx.Values
          |> Map.tryFindWithError
            id
            "variables"
            (fun () -> id.AsFSharpString)
            loc
          |> state.OfSum
      }


  type TypeInstantiateContext<'valueExt when 'valueExt: comparison> with
    static member FromEvalContext
      (ctx: TypeCheckContext<'valueExt>)
      : TypeInstantiateContext<'valueExt> =
      { VisitedVars = Set.empty
        Scope = ctx.Scope
        TypeVariables = ctx.TypeVariables
        TypeParameters = ctx.TypeParameters
        Values = ctx.Values
        BackgroundHooksExtraScope = ctx.BackgroundHooksExtraScope
        PermissionHooksExtraScope = ctx.PermissionHooksExtraScope }

    static member Empty: TypeInstantiateContext<'valueExt> =
      { VisitedVars = Set.empty
        Scope = TypeCheckScope.Empty
        TypeVariables = Map.empty
        TypeParameters = Map.empty
        Values = Map.empty
        BackgroundHooksExtraScope = Map.empty
        PermissionHooksExtraScope = Map.empty }

    static member Updaters =
      {| VisitedVars =
          fun f (ctx: TypeInstantiateContext<'valueExt>) ->
            { ctx with
                VisitedVars = f ctx.VisitedVars }
         TypeParameters =
          fun f (ctx: TypeInstantiateContext<'valueExt>) ->
            { ctx with
                TypeParameters = f ctx.TypeParameters }
         TypeVariables =
          fun f (ctx: TypeInstantiateContext<'valueExt>) ->
            { ctx with
                TypeVariables = f ctx.TypeVariables }
         Scope =
          fun f (ctx: TypeInstantiateContext<'valueExt>) ->
            { ctx with Scope = f ctx.Scope }
         Values =
          fun f (ctx: TypeInstantiateContext<'valueExt>) ->
            { ctx with Values = f ctx.Values }
         BackgroundHooksExtraScope =
          fun f (ctx: TypeInstantiateContext<'valueExt>) ->
            { ctx with
                BackgroundHooksExtraScope = f ctx.BackgroundHooksExtraScope }
         PermissionHooksExtraScope =
          fun f (ctx: TypeInstantiateContext<'valueExt>) ->
            { ctx with
                PermissionHooksExtraScope = f ctx.PermissionHooksExtraScope } |}

  type TypeValue<'valueExt> with
    /// Returns true when a TypeValue has no free type variables, lookups, or
    /// unevaluated applications — i.e. instantiation is guaranteed to be the
    /// identity function. Schemas are checked entity-by-entity.
    static member IsConcrete<'ve when 've: comparison>
      (tv: TypeValue<'ve>)
      : bool =
      match tv with
      | TypeValue.Var _ | TypeValue.Lookup _ | TypeValue.Application _ -> false
      | TypeValue.Primitive _ | TypeValue.QueryTypeFunction -> true
      | TypeValue.Arrow { value = l, r } ->
        TypeValue.IsConcrete l && TypeValue.IsConcrete r
      | TypeValue.Record { value = fields } ->
        fields |> OrderedMap.toSeq |> Seq.forall (fun (_, (v, _)) -> TypeValue.IsConcrete v)
      | TypeValue.Tuple { value = es } ->
        es |> List.forall TypeValue.IsConcrete
      | TypeValue.Union { value = es } ->
        es |> OrderedMap.toSeq |> Seq.forall (fun (_, v) -> TypeValue.IsConcrete v)
      | TypeValue.Sum { value = es } ->
        es |> List.forall TypeValue.IsConcrete
      | TypeValue.Set { value = v } -> TypeValue.IsConcrete v
      | TypeValue.Imported { Arguments = args } ->
        args |> List.forall TypeValue.IsConcrete
      | TypeValue.Lambda _ -> false
      | TypeValue.Schema s ->
        s.Entities |> OrderedMap.toSeq |> Seq.forall (fun (_, e) ->
          TypeValue.IsConcrete e.TypeOriginal
          && TypeValue.IsConcrete e.TypeWithProps
          && TypeValue.IsConcrete e.Id)
      | TypeValue.Entities s | TypeValue.Relations s ->
        s.Entities |> OrderedMap.toSeq |> Seq.forall (fun (_, e) ->
          TypeValue.IsConcrete e.TypeOriginal
          && TypeValue.IsConcrete e.TypeWithProps
          && TypeValue.IsConcrete e.Id)
      | TypeValue.Entity(s, e, e', eid) ->
        TypeValue.IsConcrete(TypeValue.Schema s)
        && TypeValue.IsConcrete e
        && TypeValue.IsConcrete e'
        && TypeValue.IsConcrete eid
      | TypeValue.Relation(s, _, _, f, f', fid, t, t', tid) ->
        TypeValue.IsConcrete(TypeValue.Schema s)
        && TypeValue.IsConcrete f && TypeValue.IsConcrete f'
        && TypeValue.IsConcrete fid
        && TypeValue.IsConcrete t && TypeValue.IsConcrete t'
        && TypeValue.IsConcrete tid
      | TypeValue.ForeignKeyRelation(s, _, f, f', fid, t, t', tid) ->
        TypeValue.IsConcrete(TypeValue.Schema s)
        && TypeValue.IsConcrete f && TypeValue.IsConcrete f'
        && TypeValue.IsConcrete fid
        && TypeValue.IsConcrete t && TypeValue.IsConcrete t'
        && TypeValue.IsConcrete tid
      | TypeValue.RelationLookupOption(s, sid, t', tid)
      | TypeValue.RelationLookupOne(s, sid, t', tid)
      | TypeValue.RelationLookupMany(s, sid, t', tid) ->
        TypeValue.IsConcrete(TypeValue.Schema s)
        && TypeValue.IsConcrete sid
        && TypeValue.IsConcrete t'
        && TypeValue.IsConcrete tid
      | TypeValue.QueryRow row ->
        match row with
        | TypeQueryRow.PrimaryKey t | TypeQueryRow.Json t -> TypeValue.IsConcrete t
        | TypeQueryRow.PrimitiveType _ -> true
        | TypeQueryRow.Tuple _ -> true
        | TypeQueryRow.Record _ -> true
        | TypeQueryRow.Array _ -> true

  type TypeCheckState<'valueExt when 'valueExt: comparison> with
    // static member ToInstantiationContext
    //   (scope: TypeCheckScope, typeVariables: TypeVariablesScope, typeParameters: TypeParametersScope)
    //   : TypeInstantiateContext =
    //   { VisitedVars = Set.empty
    //     Scope = scope
    //     TypeVariables = typeVariables
    //     TypeParameters = typeParameters
    //     Values = Map.empty }

    static member TryFindTypeSymbol
      (id: Identifier, loc: Location)
      : TypeCheckerResult<TypeSymbol, 'valueExt> =
      state {
        let! s = state.GetState()
        let! ctx = state.GetContext()

        return!
          s.Symbols.Types
          |> Map.tryFindWithError
            (id |> ctx.Scope.Resolve)
            "symbols"
            (fun () -> id.AsFSharpString)
            loc
          |> state.OfSum
      }

    static member TryResolveIdentifier
      (id: TypeSymbol, loc: Location)
      : TypeCheckerResult<ResolvedIdentifier, 'valueExt> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.ResolvedIdentifiers
          |> Map.tryFindWithError
            id
            "resolved identifier"
            (fun () -> id.AsFSharpString)
            loc
          |> state.OfSum
      }

    static member TryResolveIdentifier
      (id: Identifier, loc: Location)
      : TypeCheckerResult<ResolvedIdentifier, 'valueExt> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.IdentifiersResolver
          |> Map.tryFindWithError
            id
            "identifier resolver"
            (fun () -> id.AsFSharpString)
            loc
          |> state.OfSum
      }

    static member TryFindRecordFieldSymbol
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<TypeSymbol, 'valueExt> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.RecordFields
          |> Map.tryFindWithError
            id
            "record fields"
            (fun () -> id.AsFSharpString)
            loc
          |> state.OfSum
      }

    static member TryFindUnionCaseSymbol
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<TypeSymbol, 'valueExt> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.UnionCases
          |> Map.tryFindWithError
            id
            "union cases"
            (fun () -> id.AsFSharpString)
            loc
          |> state.OfSum
      }

    static member TryFindType
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<TypeValue<'valueExt> * Kind, 'valueExt> =
      state {
        let! s = state.GetState()

        return!
          s.Bindings
          |> Map.tryFindWithError
            id
            "type bindings"
            (fun () -> id.AsFSharpString)
            loc
          |> state.OfSum
      }

    static member TryFindUnionCaseConstructor
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<
          TypeValue<'valueExt> *
          List<TypeParameter> *
          OrderedMap<TypeSymbol, TypeValue<'valueExt>>,
          'valueExt
         >
      =
      state {
        let! s = state.GetState()

        return!
          s.UnionCases
          |> Map.tryFindWithError
            id
            "union cases"
            (fun () -> id.AsFSharpString)
            loc
          |> state.OfSum
      }

    static member TryFindRecordField
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<
          OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind> *
          TypeValue<'valueExt>,
          'valueExt
         >
      =
      state {
        let! s = state.GetState()

        return!
          s.RecordFields
          |> Map.tryFindWithError
            id
            "record fields"
            (fun () -> id.AsFSharpString)
            loc
          |> state.OfSum
      }
