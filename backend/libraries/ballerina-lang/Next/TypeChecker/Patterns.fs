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
  open Ballerina.Cat.Collections.OrderedMap
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


  type TypeCheckContext with
    static member Empty(assembly: string, _module: string) : TypeCheckContext =
      { Scope =
          { Assembly = assembly
            Module = _module
            Type = None }
        TypeVariables = Map.empty
        TypeParameters = Map.empty
        Values = Map.empty }

    static member Updaters =
      {| Scope = fun u (c: TypeCheckContext) -> { c with Scope = c.Scope |> u }
         TypeVariables =
          fun u (c: TypeCheckContext) ->
            { c with
                TypeVariables = c.TypeVariables |> u }
         TypeParameters =
          fun u (c: TypeCheckContext) ->
            { c with
                TypeParameters = c.TypeParameters |> u }
         Values = fun u (c: TypeCheckContext) -> { c with Values = c.Values |> u } |}

    static member FromInstantiateContext(ctx: TypeInstantiateContext) : TypeCheckContext =
      { Scope = ctx.Scope
        TypeVariables = ctx.TypeVariables
        TypeParameters = ctx.TypeParameters
        Values = ctx.Values }

    static member tryFindTypeVariable(v: string, loc: Location) : Reader<TypeValue * Kind, TypeCheckContext, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.TypeVariables
          |> Map.tryFindWithError v "type variables" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindTypeParameter(v: string, loc: Location) : Reader<Kind, TypeCheckContext, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.TypeParameters
          |> Map.tryFindWithError v "type parameters" v.ToFSharpString loc
          |> reader.OfSum
      }

  type TypeCheckState with
    static member Empty: TypeCheckState =
      { Bindings = Map.empty
        UnionCases = Map.empty
        RecordFields = Map.empty
        Symbols = TypeExprEvalSymbols.Empty
        Vars = UnificationState.Empty }

    static member Create(bindings: TypeBindings, symbols: TypeExprEvalSymbols) : TypeCheckState =
      { TypeCheckState.Empty with
          Bindings = bindings
          Symbols = symbols }

    static member CreateFromUnificationState(vars: UnificationState) : TypeCheckState =
      { TypeCheckState.Empty with
          Vars = vars }

    static member CreateFromSymbols(symbols: TypeExprEvalSymbols) : TypeCheckState =
      { TypeCheckState.Empty with
          Symbols = symbols }

    static member CreateFromTypeSymbols(symbols: Map<ResolvedIdentifier, TypeSymbol>) : TypeCheckState =
      { TypeCheckState.Empty with
          Symbols =
            { TypeExprEvalSymbols.Empty with
                Types = symbols } }

    static member tryFindType(v: ResolvedIdentifier, loc: Location) : Reader<TypeValue * Kind, TypeCheckState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Bindings
          |> Map.tryFindWithError v "bindings" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindUnionCaseConstructor
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeValue * OrderedMap<TypeSymbol, TypeValue>, TypeCheckState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.UnionCases
          |> Map.tryFindWithError v "union cases" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindRecordField
      (v: ResolvedIdentifier, loc: Location)
      : Reader<OrderedMap<TypeSymbol, TypeValue> * TypeValue, TypeCheckState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.RecordFields
          |> Map.tryFindWithError v "record fields" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindTypeSymbol(v: ResolvedIdentifier, loc: Location) : Reader<TypeSymbol, TypeCheckState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.Types
          |> Map.tryFindWithError v "type symbols" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindRecordFieldSymbol
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeSymbol, TypeCheckState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.RecordFields
          |> Map.tryFindWithError v "record field symbols" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindUnionCaseSymbol
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeSymbol, TypeCheckState, Errors> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.UnionCases
          |> Map.tryFindWithError v "union case symbols" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member tryFindResolvedIdentifier
      (v: TypeSymbol, loc: Location)
      : Reader<ResolvedIdentifier, TypeCheckState, Errors> =
      reader {
        let! ctx = reader.GetContext()

        return!
          ctx.Symbols.ResolvedIdentifiers
          |> Map.tryFindWithError v "resolved identifiers" v.ToFSharpString loc
          |> reader.OfSum
      }

    static member Updaters =
      {| Vars = fun (u: Updater<UnificationState>) (c: TypeCheckState) -> { c with Vars = c.Vars |> u }
         Bindings = fun u (c: TypeCheckState) -> { c with Bindings = c.Bindings |> u }
         UnionCases =
          fun u (c: TypeCheckState) ->
            { c with
                UnionCases = c.UnionCases |> u }
         RecordFields =
          fun u (c: TypeCheckState) ->
            { c with
                RecordFields = c.RecordFields |> u }
         Symbols =
          {| Types =
              fun u (c: TypeCheckState) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          Types = c.Symbols.Types |> u } }
             ResolvedIdentifiers =
              fun u (c: TypeCheckState) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          ResolvedIdentifiers = c.Symbols.ResolvedIdentifiers |> u } }
             IdentifiersResolver =
              fun u (c: TypeCheckState) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          IdentifiersResolver = c.Symbols.IdentifiersResolver |> u } }
             RecordFields =
              fun u (c: TypeCheckState) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          RecordFields = c.Symbols.RecordFields |> u } }
             UnionCases =
              fun u (c: TypeCheckState) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          UnionCases = c.Symbols.UnionCases |> u } } |} |}

    static member unbindType x =
      state { do! state.SetState(TypeCheckState.Updaters.Bindings(Map.remove x)) }

    static member bindType x t_x =
      state { do! state.SetState(TypeCheckState.Updaters.Bindings(Map.add x t_x)) }

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
        do! state.SetState(TypeCheckState.Updaters.Symbols.ResolvedIdentifiers(Map.add t_x x))
        do! state.SetState(TypeCheckState.Updaters.Symbols.RecordFields(Map.add x t_x))
      }

    static member bindIdentifierToResolvedIdentifier x t_x =
      state { do! state.SetState(TypeCheckState.Updaters.Symbols.IdentifiersResolver(Map.add t_x x)) }

    static member bindUnionCaseSymbol x t_x =
      state {
        do! state.SetState(TypeCheckState.Updaters.Symbols.ResolvedIdentifiers(Map.add t_x x))
        do! state.SetState(TypeCheckState.Updaters.Symbols.UnionCases(Map.add x t_x))
      }

    static member bindTypeSymbol x t_x =
      state {
        do! state.SetState(TypeCheckState.Updaters.Symbols.ResolvedIdentifiers(Map.add t_x x))
        do! state.SetState(TypeCheckState.Updaters.Symbols.Types(Map.add x t_x))
      }

  type UnificationContext with

    static member Empty: UnificationContext =
      { EvalState = TypeCheckState.Empty
        Scope = TypeCheckScope.Empty }

    static member Create(x) : UnificationContext =
      { EvalState = TypeCheckState.Create x
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
    static member TryFindVar(id: ResolvedIdentifier, loc: Location) : TypeCheckerResult<TypeValue * Kind> =
      state {
        let! ctx = state.GetContext()

        return!
          ctx.Values
          |> Map.tryFindWithError id "variables" id.ToFSharpString loc
          |> state.OfSum
      }


  type TypeInstantiateContext with
    static member FromEvalContext(ctx: TypeCheckContext) : TypeInstantiateContext =
      { VisitedVars = Set.empty
        Scope = ctx.Scope
        TypeVariables = ctx.TypeVariables
        TypeParameters = ctx.TypeParameters
        Values = ctx.Values }

    static member Empty =
      { VisitedVars = Set.empty
        Scope = TypeCheckScope.Empty
        TypeVariables = Map.empty
        TypeParameters = Map.empty
        Values = Map.empty }

    static member Updaters =
      {| VisitedVars =
          fun f (ctx: TypeInstantiateContext) ->
            { ctx with
                VisitedVars = f ctx.VisitedVars }
         TypeParameters =
          fun f (ctx: TypeInstantiateContext) ->
            { ctx with
                TypeParameters = f ctx.TypeParameters }
         TypeVariables =
          fun f (ctx: TypeInstantiateContext) ->
            { ctx with
                TypeVariables = f ctx.TypeVariables }
         Scope = fun f (ctx: TypeInstantiateContext) -> { ctx with Scope = f ctx.Scope }
         Values = fun f (ctx: TypeInstantiateContext) -> { ctx with Values = f ctx.Values } |}


  type TypeCheckState with
    // static member ToInstantiationContext
    //   (scope: TypeCheckScope, typeVariables: TypeVariablesScope, typeParameters: TypeParametersScope)
    //   : TypeInstantiateContext =
    //   { VisitedVars = Set.empty
    //     Scope = scope
    //     TypeVariables = typeVariables
    //     TypeParameters = typeParameters
    //     Values = Map.empty }

    static member TryFindTypeSymbol(id: Identifier, loc: Location) : TypeCheckerResult<TypeSymbol> =
      state {
        let! s = state.GetState()
        let! ctx = state.GetContext()

        return!
          s.Symbols.Types
          |> Map.tryFindWithError (id |> ctx.Scope.Resolve) "symbols" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryResolveIdentifier(id: TypeSymbol, loc: Location) : TypeCheckerResult<ResolvedIdentifier> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.ResolvedIdentifiers
          |> Map.tryFindWithError id "resolved identifier" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryResolveIdentifier(id: Identifier, loc: Location) : TypeCheckerResult<ResolvedIdentifier> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.IdentifiersResolver
          |> Map.tryFindWithError id "identifier resolver" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindRecordFieldSymbol(id: ResolvedIdentifier, loc: Location) : TypeCheckerResult<TypeSymbol> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.RecordFields
          |> Map.tryFindWithError id "record fields" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindUnionCaseSymbol(id: ResolvedIdentifier, loc: Location) : TypeCheckerResult<TypeSymbol> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.UnionCases
          |> Map.tryFindWithError id "union cases" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindType(id: ResolvedIdentifier, loc: Location) : TypeCheckerResult<TypeValue * Kind> =
      state {
        let! s = state.GetState()

        return!
          s.Bindings
          |> Map.tryFindWithError id "type bindings" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindUnionCaseConstructor
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<TypeValue * OrderedMap<TypeSymbol, TypeValue>> =
      state {
        let! s = state.GetState()

        return!
          s.UnionCases
          |> Map.tryFindWithError id "union cases" id.ToFSharpString loc
          |> state.OfSum
      }

    static member TryFindRecordField
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<OrderedMap<TypeSymbol, TypeValue> * TypeValue> =
      state {
        let! s = state.GetState()

        return!
          s.RecordFields
          |> Map.tryFindWithError id "record fields" id.ToFSharpString loc
          |> state.OfSum
      }
