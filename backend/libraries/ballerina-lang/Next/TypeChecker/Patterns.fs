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

    static member CreateFromTypeSymbols(symbols: Map<ResolvedIdentifier, TypeSymbol>) : TypeExprEvalSymbols =
      { TypeExprEvalSymbols.Empty with
          Types = symbols }


  type TypeCheckContext<'valueExt> with
    static member Empty(assembly: string, _module: string) : TypeCheckContext<'valueExt> =
      { Scope =
          { Assembly = assembly
            Module = _module
            Type = None }
        TypeVariables = Map.empty
        TypeParameters = Map.empty
        Values = Map.empty }

    static member Updaters =
      {| Scope = fun u (c: TypeCheckContext<'valueExt>) -> { c with Scope = c.Scope |> u }
         TypeVariables =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with
                TypeVariables = c.TypeVariables |> u }
         TypeParameters =
          fun u (c: TypeCheckContext<'valueExt>) ->
            { c with
                TypeParameters = c.TypeParameters |> u }
         Values = fun u (c: TypeCheckContext<'valueExt>) -> { c with Values = c.Values |> u } |}

    static member FromInstantiateContext<'ve when 've: comparison>
      (ctx: TypeInstantiateContext<'ve>)
      : TypeCheckContext<'ve> =
      { Scope = ctx.Scope
        TypeVariables = ctx.TypeVariables
        TypeParameters = ctx.TypeParameters
        Values = ctx.Values }

    static member tryFindTypeVariable
      (v: string, loc: Location)
      : Reader<TypeValue<'valueExt> * Kind, TypeCheckContext<'valueExt>, Errors<Location>> =
      reader {
        let! s = reader.GetContext()

        return!
          s.TypeVariables
          |> Map.tryFindWithError v "type variables" (fun () -> v.AsFSharpString) loc
          |> reader.OfSum
      }

    static member tryFindTypeParameter
      (v: string, loc: Location)
      : Reader<Kind, TypeCheckContext<'valueExt>, Errors<Location>> =
      reader {
        let! s = reader.GetContext()

        return!
          s.TypeParameters
          |> Map.tryFindWithError v "type parameters" (fun () -> v.AsFSharpString) loc
          |> reader.OfSum
      }

  type UnificationState<'valueExt when 'valueExt: comparison> with
    static member Empty: UnificationState<'valueExt> = { Classes = EquivalenceClasses.Empty }

    static member Create(classes: EquivalenceClasses<TypeVar, TypeValue<'valueExt>>) : UnificationState<'valueExt> =
      { Classes = classes }

    static member EnsureVariableExists var (classes: UnificationState<'valueExt>) =
      EquivalenceClasses.EnsureVariableExists var classes.Classes
      |> UnificationState.Create

    static member Updaters =
      {| Classes =
          fun (u: Updater<EquivalenceClasses<TypeVar, TypeValue<'valueExt>>>) (c: UnificationState<'valueExt>) ->
            { c with Classes = c.Classes |> u } |}

  type TypeCheckState<'valueExt when 'valueExt: comparison> with
    static member Empty: TypeCheckState<'valueExt> =
      { Bindings = Map.empty
        UnionCases = Map.empty
        RecordFields = Map.empty
        Symbols = TypeExprEvalSymbols.Empty
        Vars = UnificationState.Empty }

    static member Create(bindings: TypeBindings<'valueExt>, symbols: TypeExprEvalSymbols) : TypeCheckState<'valueExt> =
      { TypeCheckState.Empty with
          Bindings = bindings
          Symbols = symbols }

    static member CreateFromUnificationState(vars: UnificationState<'valueExt>) : TypeCheckState<'valueExt> =
      { TypeCheckState.Empty with
          Vars = vars }

    static member CreateFromSymbols(symbols: TypeExprEvalSymbols) : TypeCheckState<'valueExt> =
      { TypeCheckState.Empty with
          Symbols = symbols }

    static member CreateFromTypeSymbols(symbols: Map<ResolvedIdentifier, TypeSymbol>) : TypeCheckState<'valueExt> =
      { TypeCheckState.Empty with
          Symbols =
            { TypeExprEvalSymbols.Empty with
                Types = symbols } }

    static member tryFindType
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeValue<'valueExt> * Kind, TypeCheckState<'valueExt>, Errors<Location>> =
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
          TypeValue<'valueExt> * List<TypeParameter> * OrderedMap<TypeSymbol, TypeValue<'valueExt>>,
          TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =
      reader {
        let! s = reader.GetContext()

        return!
          s.UnionCases
          |> Map.tryFindWithError v "union cases" (fun () -> v.AsFSharpString) loc
          |> reader.OfSum
      }

    static member tryFindRecordField
      (v: ResolvedIdentifier, loc: Location)
      : Reader<
          OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind> * TypeValue<'valueExt>,
          TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =
      reader {
        let! s = reader.GetContext()

        return!
          s.RecordFields
          |> Map.tryFindWithError v "record fields" (fun () -> v.AsFSharpString) loc
          |> reader.OfSum
      }

    static member tryFindTypeSymbol
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeSymbol, TypeCheckState<'valueExt>, Errors<Location>> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.Types
          |> Map.tryFindWithError v "type symbols" (fun () -> v.AsFSharpString) loc
          |> reader.OfSum
      }

    static member tryFindRecordFieldSymbol
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeSymbol, TypeCheckState<'valueExt>, Errors<Location>> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.RecordFields
          |> Map.tryFindWithError v "record field symbols" (fun () -> v.AsFSharpString) loc
          |> reader.OfSum
      }

    static member tryFindUnionCaseSymbol
      (v: ResolvedIdentifier, loc: Location)
      : Reader<TypeSymbol, TypeCheckState<'valueExt>, Errors<Location>> =
      reader {
        let! s = reader.GetContext()

        return!
          s.Symbols.UnionCases
          |> Map.tryFindWithError v "union case symbols" (fun () -> v.AsFSharpString) loc
          |> reader.OfSum
      }

    static member tryFindResolvedIdentifier
      (v: TypeSymbol, loc: Location)
      : Reader<ResolvedIdentifier, TypeCheckState<'valueExt>, Errors<Location>> =
      reader {
        let! ctx = reader.GetContext()

        return!
          ctx.Symbols.ResolvedIdentifiers
          |> Map.tryFindWithError v "resolved identifiers" (fun () -> v.AsFSharpString) loc
          |> reader.OfSum
      }

    static member Updaters =
      {| Vars =
          fun (u: Updater<UnificationState<'valueExt>>) (c: TypeCheckState<'valueExt>) -> { c with Vars = c.Vars |> u }
         Bindings = fun u (c: TypeCheckState<'valueExt>) -> { c with Bindings = c.Bindings |> u }
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
                          ResolvedIdentifiers = c.Symbols.ResolvedIdentifiers |> u } }
             IdentifiersResolver =
              fun u (c: TypeCheckState<'valueExt>) ->
                { c with
                    Symbols =
                      { c.Symbols with
                          IdentifiersResolver = c.Symbols.IdentifiersResolver |> u } }
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
          |> Map.tryFindWithError id "variables" (fun () -> id.AsFSharpString) loc
          |> state.OfSum
      }


  type TypeInstantiateContext<'valueExt when 'valueExt: comparison> with
    static member FromEvalContext(ctx: TypeCheckContext<'valueExt>) : TypeInstantiateContext<'valueExt> =
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
         Scope = fun f (ctx: TypeInstantiateContext<'valueExt>) -> { ctx with Scope = f ctx.Scope }
         Values = fun f (ctx: TypeInstantiateContext<'valueExt>) -> { ctx with Values = f ctx.Values } |}

  type TypeCheckState<'valueExt when 'valueExt: comparison> with
    // static member ToInstantiationContext
    //   (scope: TypeCheckScope, typeVariables: TypeVariablesScope, typeParameters: TypeParametersScope)
    //   : TypeInstantiateContext =
    //   { VisitedVars = Set.empty
    //     Scope = scope
    //     TypeVariables = typeVariables
    //     TypeParameters = typeParameters
    //     Values = Map.empty }

    static member TryFindTypeSymbol(id: Identifier, loc: Location) : TypeCheckerResult<TypeSymbol, 'valueExt> =
      state {
        let! s = state.GetState()
        let! ctx = state.GetContext()

        return!
          s.Symbols.Types
          |> Map.tryFindWithError (id |> ctx.Scope.Resolve) "symbols" (fun () -> id.AsFSharpString) loc
          |> state.OfSum
      }

    static member TryResolveIdentifier
      (id: TypeSymbol, loc: Location)
      : TypeCheckerResult<ResolvedIdentifier, 'valueExt> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.ResolvedIdentifiers
          |> Map.tryFindWithError id "resolved identifier" (fun () -> id.AsFSharpString) loc
          |> state.OfSum
      }

    static member TryResolveIdentifier
      (id: Identifier, loc: Location)
      : TypeCheckerResult<ResolvedIdentifier, 'valueExt> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.IdentifiersResolver
          |> Map.tryFindWithError id "identifier resolver" (fun () -> id.AsFSharpString) loc
          |> state.OfSum
      }

    static member TryFindRecordFieldSymbol
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<TypeSymbol, 'valueExt> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.RecordFields
          |> Map.tryFindWithError id "record fields" (fun () -> id.AsFSharpString) loc
          |> state.OfSum
      }

    static member TryFindUnionCaseSymbol
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<TypeSymbol, 'valueExt> =
      state {
        let! s = state.GetState()

        return!
          s.Symbols.UnionCases
          |> Map.tryFindWithError id "union cases" (fun () -> id.AsFSharpString) loc
          |> state.OfSum
      }

    static member TryFindType
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<TypeValue<'valueExt> * Kind, 'valueExt> =
      state {
        let! s = state.GetState()

        return!
          s.Bindings
          |> Map.tryFindWithError id "type bindings" (fun () -> id.AsFSharpString) loc
          |> state.OfSum
      }

    static member TryFindUnionCaseConstructor
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<
          TypeValue<'valueExt> * List<TypeParameter> * OrderedMap<TypeSymbol, TypeValue<'valueExt>>,
          'valueExt
         >
      =
      state {
        let! s = state.GetState()

        return!
          s.UnionCases
          |> Map.tryFindWithError id "union cases" (fun () -> id.AsFSharpString) loc
          |> state.OfSum
      }

    static member TryFindRecordField
      (id: ResolvedIdentifier, loc: Location)
      : TypeCheckerResult<OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind> * TypeValue<'valueExt>, 'valueExt> =
      state {
        let! s = state.GetState()

        return!
          s.RecordFields
          |> Map.tryFindWithError id "record fields" (fun () -> id.AsFSharpString) loc
          |> state.OfSum
      }
