namespace Ballerina.DSL.Next.Types.TypeChecker

module Patterns =
  open Ballerina.Fun
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.EquivalenceClasses
  open Ballerina.DSL.Next.EquivalenceClasses
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  type TypeExprEvalSymbols with

    static member Empty =
      { Types = Map.empty
        ResolvedIdentifiers = Map.empty
        IdentifiersResolver = Map.empty
        UnionCases = Map.empty
        RecordFields = Map.empty }

    static member CreateFromTypeSymbols(symbols: Map<ResolvedIdentifier, TypeSymbol>) : TypeExprEvalSymbols =
      { TypeExprEvalSymbols.Empty with
          Types = symbols }


  type TypeExprEvalContext with
    static member Empty(assembly: string, _module: string) : TypeExprEvalContext =
      { Scope =
          { Assembly = assembly
            Module = _module
            Type = None } }

    static member Updaters =
      {| Scope = fun u (c: TypeExprEvalContext) -> { c with Scope = c.Scope |> u } |}

  type TypeExprEvalState with
    static member Empty: TypeExprEvalState =
      { Bindings = Map.empty
        UnionCases = Map.empty
        RecordFields = Map.empty
        Symbols = TypeExprEvalSymbols.Empty }

    static member Create(bindings: TypeBindings, symbols: TypeExprEvalSymbols) : TypeExprEvalState =
      { TypeExprEvalState.Empty with
          Bindings = bindings
          Symbols = symbols }

    static member CreateFromSymbols(symbols: TypeExprEvalSymbols) : TypeExprEvalState =
      { TypeExprEvalState.Empty with
          Symbols = symbols }

    static member CreateFromTypeSymbols(symbols: Map<ResolvedIdentifier, TypeSymbol>) : TypeExprEvalState =
      { TypeExprEvalState.Empty with
          Symbols =
            { TypeExprEvalSymbols.Empty with
                Types = symbols } }

    static member tryFindType
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeValue * Kind, TypeExprEvalState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Bindings
          |> Map.tryFindWithError v "bindings" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindUnionCaseConstructor
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeValue * OrderedMap<TypeSymbol, TypeValue>, TypeExprEvalState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.UnionCases
          |> Map.tryFindWithError v "union cases" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindRecordField
      (v: ResolvedIdentifier, loc: Location)
      : Reader<OrderedMap<TypeSymbol, TypeValue> * TypeValue, TypeExprEvalState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.RecordFields
          |> Map.tryFindWithError v "record fields" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindTypeSymbol
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeSymbol, TypeExprEvalState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.Types
          |> Map.tryFindWithError v "type symbols" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindRecordFieldSymbol
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeSymbol, TypeExprEvalState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.RecordFields
          |> Map.tryFindWithError v "record field symbols" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindUnionCaseSymbol
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeSymbol, TypeExprEvalState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.UnionCases
          |> Map.tryFindWithError v "union case symbols" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindResolvedIdentifier
      (v: TypeSymbol, loc: Location)
      : Reader<ResolvedIdentifier, TypeExprEvalState, Errors> =
      reader {
        let! ctx = reader.GetContext()

        return!
          ctx.Symbols.ResolvedIdentifiers
          |> Map.tryFindWithError v "resolved identifiers" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member Updaters =
      {| Bindings = fun u (c: TypeExprEvalState) -> { c with Bindings = c.Bindings |> u }
         UnionCases =
          fun u (c: TypeExprEvalState) ->
            { c with
                UnionCases = c.UnionCases |> u }
         RecordFields =
          fun u (c: TypeExprEvalState) ->
            { c with
                RecordFields = c.RecordFields |> u }
         Symbols =
          {| Types =
              fun u (c: TypeExprEvalState) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          Types = c.Symbols.Types |> u } }
             ResolvedIdentifiers =
              fun u (c: TypeExprEvalState) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          ResolvedIdentifiers = c.Symbols.ResolvedIdentifiers |> u } }
             IdentifiersResolver =
              fun u (c: TypeExprEvalState) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          IdentifiersResolver = c.Symbols.IdentifiersResolver |> u } }
             RecordFields =
              fun u (c: TypeExprEvalState) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          RecordFields = c.Symbols.RecordFields |> u } }
             UnionCases =
              fun u (c: TypeExprEvalState) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          UnionCases = c.Symbols.UnionCases |> u } } |} |}

    static member unbindType x =
      state { do! state.SetState(TypeExprEvalState.Updaters.Bindings(Map.remove x)) }

    static member bindType x t_x =
      state { do! state.SetState(TypeExprEvalState.Updaters.Bindings(Map.add x t_x)) }

    static member bindUnionCaseConstructor (x: ResolvedIdentifier) t_x =
      state {
        do! state.SetState(TypeExprEvalState.Updaters.UnionCases(Map.add x t_x))
        let x = x.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve
        do! state.SetState(TypeExprEvalState.Updaters.UnionCases(Map.add x t_x))
      }

    static member bindRecordField (x: ResolvedIdentifier) t_x =
      state {
        do! state.SetState(TypeExprEvalState.Updaters.RecordFields(Map.add x t_x))
        let x = x.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve
        do! state.SetState(TypeExprEvalState.Updaters.RecordFields(Map.add x t_x))
      }

    static member bindRecordFieldSymbol x t_x =
      state {
        do! state.SetState(TypeExprEvalState.Updaters.Symbols.ResolvedIdentifiers(Map.add t_x x))
        do! state.SetState(TypeExprEvalState.Updaters.Symbols.RecordFields(Map.add x t_x))
      }

    static member bindIdentifierToResolvedIdentifier x t_x =
      state { do! state.SetState(TypeExprEvalState.Updaters.Symbols.IdentifiersResolver(Map.add t_x x)) }

    static member bindUnionCaseSymbol x t_x =
      state {
        do! state.SetState(TypeExprEvalState.Updaters.Symbols.ResolvedIdentifiers(Map.add t_x x))
        do! state.SetState(TypeExprEvalState.Updaters.Symbols.UnionCases(Map.add x t_x))
      }

    static member bindTypeSymbol x t_x =
      state {
        do! state.SetState(TypeExprEvalState.Updaters.Symbols.ResolvedIdentifiers(Map.add t_x x))
        do! state.SetState(TypeExprEvalState.Updaters.Symbols.Types(Map.add x t_x))
      }

  type UnificationContext with

    static member Empty: UnificationContext =
      { EvalState = TypeExprEvalState.Empty
        Scope = TypeCheckScope.Empty }

    static member Create(x) : UnificationContext =
      { EvalState = TypeExprEvalState.Create x
        Scope = TypeCheckScope.Empty }

    static member Updaters =
      {| EvalState =
          fun f ctx ->
            { ctx with
                UnificationContext.EvalState = f ctx.EvalState }
         Scope =
          fun f ctx ->
            { ctx with
                UnificationContext.Scope = f ctx.Scope } |}

  type TypeCheckContext with
    static member Empty(asm, mod_) : TypeCheckContext =
      { Types = TypeExprEvalContext.Empty(asm, mod_)
        Values = Map.empty }

    static member Getters =
      {| Types = fun (c: TypeCheckContext) -> c.Types
         Values = fun (c: TypeCheckContext) -> c.Values |}

    static member TryFindVar(id: ResolvedIdentifier, loc: Location) : TypeCheckerResult<TypeValue * Kind> =
      state {
        let! ctx = state.GetContext()

        return!
          ctx.Values
          |> Map.tryFindWithError id "variables" id.ToFSharpString loc
          |> state.OfSum
      }

    static member Updaters =
      {| Types = fun u (c: TypeCheckContext) -> { c with Types = c.Types |> u }
         Values = fun u (c: TypeCheckContext) -> { c with Values = c.Values |> u } |}


  type TypeInstantiateContext with
    static member Empty =
      { Bindings = TypeExprEvalState.Empty
        VisitedVars = Set.empty
        Scope = TypeCheckScope.Empty }

    static member Updaters =
      {| Bindings = fun f (ctx: TypeInstantiateContext) -> { ctx with Bindings = f ctx.Bindings }
         VisitedVars =
          fun f (ctx: TypeInstantiateContext) ->
            { ctx with
                VisitedVars = f ctx.VisitedVars } |}


  type TypeCheckState with
    static member Empty: TypeCheckState =
      { Types = TypeExprEvalState.Empty
        Vars = UnificationState.Empty }

    static member Getters =
      {| Types = fun (c: TypeCheckState) -> c.Types
         Vars = fun (c: TypeCheckState) -> c.Vars |}

    static member ToInstantiationContext(ctx: TypeCheckState, scope: TypeCheckScope) : TypeInstantiateContext =
      { Bindings = ctx.Types
        VisitedVars = Set.empty
        Scope = scope }

    static member TryFindTypeSymbol(id: Identifier, loc: Location) : TypeCheckerResult<TypeSymbol> =
      state {
        let! s = state.GetState()
        let! ctx = state.GetContext()

        return!
          s.Types.Symbols.Types
          |> Map.tryFindWithError (id |> ctx.Types.Scope.Resolve) "symbols" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryResolveIdentifier(id: TypeSymbol, loc: Location) : TypeCheckerResult<ResolvedIdentifier> =
      state {
        let! s = state.GetState()

        return!
          s.Types.Symbols.ResolvedIdentifiers
          |> Map.tryFindWithError id "resolved identifier" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryResolveIdentifier(id: Identifier, loc: Location) : TypeCheckerResult<ResolvedIdentifier> =
      state {
        let! s = state.GetState()

        return!
          s.Types.Symbols.IdentifiersResolver
          |> Map.tryFindWithError id "identifier resolver" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindRecordFieldSymbol(id: ResolvedIdentifier, loc: Location) : TypeCheckerResult<TypeSymbol> =
      state {
        let! s = state.GetState()

        return!
          s.Types.Symbols.RecordFields
          |> Map.tryFindWithError id "record fields" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindUnionCaseSymbol(id: ResolvedIdentifier, loc: Location) : TypeCheckerResult<TypeSymbol> =
      state {
        let! s = state.GetState()

        return!
          s.Types.Symbols.UnionCases
          |> Map.tryFindWithError id "union cases" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindType(id: ResolvedIdentifier, loc: Location) : TypeCheckerResult<TypeValue * Kind> =
      state {
        let! s = state.GetState()

        return!
          s.Types.Bindings
          |> Map.tryFindWithError id "type bindings" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindUnionCaseConstructor
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<TypeValue * OrderedMap<TypeSymbol, TypeValue>> =
      state {
        let! s = state.GetState()

        return!
          s.Types.UnionCases
          |> Map.tryFindWithError id "union cases" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindRecordField
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<OrderedMap<TypeSymbol, TypeValue> * TypeValue> =
      state {
        let! s = state.GetState()

        return!
          s.Types.RecordFields
          |> Map.tryFindWithError id "record fields" id.ToFSharpString loc
          |> state.OfSum
      }

    static member Updaters =
      {| Types = fun u (c: TypeCheckState) -> { c with Types = c.Types |> u }
         Vars = fun (u: Updater<UnificationState>) (c: TypeCheckState) -> { c with Vars = c.Vars |> u } |}
