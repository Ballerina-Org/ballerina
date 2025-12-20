namespace Ballerina.DSL.Next.Types.TypeChecker

module Model =
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
  open Ballerina.StdLib.Map


  type TypeBindings = Map<ResolvedIdentifier, TypeValue * Kind>

  type UnionCaseConstructorBindings =
    Map<ResolvedIdentifier, TypeValue * List<TypeParameter> * OrderedMap<TypeSymbol, TypeValue>>

  type RecordFieldBindings = Map<ResolvedIdentifier, OrderedMap<TypeSymbol, TypeValue * Kind> * TypeValue>

  type TypeSymbols = Map<ResolvedIdentifier, TypeSymbol>

  type TypeExprEvalSymbols =
    { Types: TypeSymbols
      ResolvedIdentifiers: Map<TypeSymbol, ResolvedIdentifier>
      IdentifiersResolver: Map<Identifier, ResolvedIdentifier>
      UnionCases: TypeSymbols
      RecordFields: TypeSymbols }

    static member Combine (s1: TypeExprEvalSymbols) (s2: TypeExprEvalSymbols) =
      { Types = s1.Types |> Map.merge s2.Types
        ResolvedIdentifiers = s1.ResolvedIdentifiers |> Map.merge s2.ResolvedIdentifiers
        IdentifiersResolver = s1.IdentifiersResolver |> Map.merge s2.IdentifiersResolver
        UnionCases = s1.UnionCases |> Map.merge s2.UnionCases
        RecordFields = s1.RecordFields |> Map.merge s2.RecordFields }

  type KindEvalContext = Map<string, Kind>

  type TypeCheckContext =
    { Scope: TypeCheckScope
      TypeVariables: TypeVariablesScope
      TypeParameters: TypeParametersScope
      Values: Map<ResolvedIdentifier, TypeValue * Kind> }

  type UnificationState = EquivalenceClasses<TypeVar, TypeValue>

  type TypeCheckState =
    { Bindings: TypeBindings
      UnionCases: UnionCaseConstructorBindings
      RecordFields: RecordFieldBindings
      Symbols: TypeExprEvalSymbols
      Vars: UnificationState }

  type TypeValueKindEval =
    Option<ExprTypeLetBindingName> -> Location -> TypeValue -> State<Kind, KindEvalContext, TypeCheckState, Errors>

  type TypeExprKindEval =
    Option<ExprTypeLetBindingName> -> Location -> TypeExpr -> State<Kind, KindEvalContext, TypeCheckState, Errors>

  type UnificationContext =
    { EvalState: TypeCheckState
      TypeParameters: TypeParametersScope
      Scope: TypeCheckScope }

  type TypeCheckerResult<'r> = State<'r, TypeCheckContext, TypeCheckState, Errors>

  type TypeChecker<'res, 'valueExt> =
    Option<TypeValue>
      -> 'res
      -> TypeCheckerResult<Expr<TypeValue, ResolvedIdentifier, 'valueExt> * TypeValue * Kind * TypeCheckContext>

  type ExprTypeChecker<'valueExt> = TypeChecker<Expr<TypeExpr, Identifier, 'valueExt>, 'valueExt>

  type TypeInstantiateContext =
    { VisitedVars: Set<TypeVar>
      Scope: TypeCheckScope
      TypeVariables: TypeVariablesScope
      TypeParameters: TypeParametersScope
      Values: Map<ResolvedIdentifier, TypeValue * Kind> }

  type TypeExprEvalResult = State<TypeValue * Kind, TypeCheckContext, TypeCheckState, Errors>
  type TypeExprEval = Option<ExprTypeLetBindingName> -> Location -> TypeExpr -> TypeExprEvalResult
  type TypeExprSymbolEvalResult = State<TypeSymbol, TypeCheckContext, TypeCheckState, Errors>
  type TypeExprSymbolEval = Location -> TypeExpr -> TypeExprSymbolEvalResult
