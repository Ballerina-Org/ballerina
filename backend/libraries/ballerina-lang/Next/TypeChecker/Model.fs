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


  type TypeBindings = Map<ResolvedIdentifier, TypeValue * Kind>
  type UnionCaseConstructorBindings = Map<ResolvedIdentifier, TypeValue * OrderedMap<TypeSymbol, TypeValue>>
  type RecordFieldBindings = Map<ResolvedIdentifier, OrderedMap<TypeSymbol, TypeValue> * TypeValue>

  type TypeSymbols = Map<ResolvedIdentifier, TypeSymbol>

  type TypeExprEvalSymbols =
    { Types: TypeSymbols
      ResolvedIdentifiers: Map<TypeSymbol, ResolvedIdentifier>
      IdentifiersResolver: Map<Identifier, ResolvedIdentifier>
      UnionCases: TypeSymbols
      RecordFields: TypeSymbols }

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

  [<Obsolete("Use TypeCheckState instead")>]
  type TypeExprEvalState = TypeCheckState

  type TypeValueKindEval =
    Option<ExprTypeLetBindingName> -> Location -> TypeValue -> State<Kind, KindEvalContext, TypeCheckState, Errors>

  type TypeExprKindEval =
    Option<ExprTypeLetBindingName> -> Location -> TypeExpr -> State<Kind, KindEvalContext, TypeCheckState, Errors>

  type UnificationContext =
    { EvalState: TypeCheckState
      Scope: TypeCheckScope }

  type TypeCheckerResult<'r> = State<'r, TypeCheckContext, TypeCheckState, Errors>

  type TypeChecker<'res, 'valueExt> =
    Option<TypeValue> -> 'res -> TypeCheckerResult<Expr<TypeValue, ResolvedIdentifier, 'valueExt> * TypeValue * Kind>

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
