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

  type TypeExprEvalContext = { Scope: TypeCheckScope }

  type TypeExprEvalSymbols =
    { Types: TypeSymbols
      ResolvedIdentifiers: Map<TypeSymbol, ResolvedIdentifier>
      IdentifiersResolver: Map<Identifier, ResolvedIdentifier>
      UnionCases: TypeSymbols
      RecordFields: TypeSymbols }

  type TypeExprEvalState =
    { Bindings: TypeBindings
      UnionCases: UnionCaseConstructorBindings
      RecordFields: RecordFieldBindings
      Symbols: TypeExprEvalSymbols }

  type TypeExprEvalResult = State<TypeValue * Kind, TypeExprEvalContext, TypeExprEvalState, Errors>
  type TypeExprEval = Option<ExprTypeLetBindingName> -> Location -> TypeExpr -> TypeExprEvalResult
  type TypeExprSymbolEvalResult = State<TypeSymbol, TypeExprEvalContext, TypeExprEvalState, Errors>
  type TypeExprSymbolEval = Location -> TypeExpr -> TypeExprSymbolEvalResult

  type UnificationContext =
    { EvalState: TypeExprEvalState
      Scope: TypeCheckScope }

  type UnificationState = EquivalenceClasses<TypeVar, TypeValue>

  type TypeCheckContext =
    { Types: TypeExprEvalContext
      Values: Map<ResolvedIdentifier, TypeValue * Kind> }

  type TypeCheckState =
    { Types: TypeExprEvalState
      Vars: UnificationState }

  type TypeCheckerResult<'r> = State<'r, TypeCheckContext, TypeCheckState, Errors>

  type TypeChecker<'res> =
    Option<TypeValue> -> 'res -> TypeCheckerResult<Expr<TypeValue, ResolvedIdentifier> * TypeValue * Kind>

  type TypeChecker = TypeChecker<Expr<TypeExpr, Identifier>>

  type TypeInstantiateContext =
    { Bindings: TypeExprEvalState
      VisitedVars: Set<TypeVar>
      Scope: TypeCheckScope }
