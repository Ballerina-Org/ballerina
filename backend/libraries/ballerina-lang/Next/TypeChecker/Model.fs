namespace Ballerina.DSL.Next.Types.TypeChecker

[<AutoOpen>]
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


  type TypeBindings<'valueExt> = Map<ResolvedIdentifier, TypeValue<'valueExt> * Kind>

  type UnionCaseConstructorBindings<'valueExt> =
    Map<ResolvedIdentifier, TypeValue<'valueExt> * List<TypeParameter> * OrderedMap<TypeSymbol, TypeValue<'valueExt>>>

  type RecordFieldBindings<'valueExt> =
    Map<ResolvedIdentifier, OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind> * TypeValue<'valueExt>>

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

  type TypeCheckContext<'valueExt> =
    { Scope: TypeCheckScope
      TypeVariables: TypeVariablesScope<'valueExt>
      TypeParameters: TypeParametersScope
      Values: Map<ResolvedIdentifier, TypeValue<'valueExt> * Kind> }

  type UnificationState<'valueExt when 'valueExt: comparison> =
    { Classes: EquivalenceClasses<TypeVar, TypeValue<'valueExt>> }

  type TypeCheckState<'valueExt when 'valueExt: comparison> =
    { Bindings: TypeBindings<'valueExt>
      UnionCases: UnionCaseConstructorBindings<'valueExt>
      RecordFields: RecordFieldBindings<'valueExt>
      Symbols: TypeExprEvalSymbols
      Vars: UnificationState<'valueExt> }

  type TypeValueKindEval<'valueExt when 'valueExt: comparison> =
    Option<ExprTypeLetBindingName>
      -> Location
      -> TypeValue<'valueExt>
      -> State<Kind, KindEvalContext, TypeCheckState<'valueExt>, Errors>

  type TypeExprKindEval<'valueExt when 'valueExt: comparison> =
    Option<ExprTypeLetBindingName>
      -> Location
      -> TypeExpr<'valueExt>
      -> State<Kind, KindEvalContext, TypeCheckState<'valueExt>, Errors>

  type UnificationContext<'valueExt when 'valueExt: comparison> =
    { EvalState: TypeCheckState<'valueExt>
      TypeParameters: TypeParametersScope
      Scope: TypeCheckScope }

  type TypeCheckerResult<'r, 'valueExt when 'valueExt: comparison> =
    State<'r, TypeCheckContext<'valueExt>, TypeCheckState<'valueExt>, Errors>

  type TypeChecker<'input, 'valueExt when 'valueExt: comparison> =
    Option<TypeValue<'valueExt>>
      -> 'input
      -> TypeCheckerResult<
        Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> *
        TypeValue<'valueExt> *
        Kind *
        TypeCheckContext<'valueExt>,
        'valueExt
       >

  type ExprTypeChecker<'valueExt when 'valueExt: comparison> =
    TypeChecker<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt>

  type TypeInstantiateContext<'valueExt when 'valueExt: comparison> =
    { VisitedVars: Set<TypeVar>
      Scope: TypeCheckScope
      TypeVariables: TypeVariablesScope<'valueExt>
      TypeParameters: TypeParametersScope
      Values: Map<ResolvedIdentifier, TypeValue<'valueExt> * Kind> }

  type TypeExprEvalResult<'valueExt when 'valueExt: comparison> =
    State<TypeValue<'valueExt> * Kind, TypeCheckContext<'valueExt>, TypeCheckState<'valueExt>, Errors>

  type TypeExprEvalPlain<'valueExt when 'valueExt: comparison> =
    Option<ExprTypeLetBindingName> -> Location -> TypeExpr<'valueExt> -> TypeExprEvalResult<'valueExt>

  type TypeExprEval<'valueExt when 'valueExt: comparison> =
    TypeChecker<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt>
      -> Option<ExprTypeLetBindingName>
      -> Location
      -> TypeExpr<'valueExt>
      -> TypeExprEvalResult<'valueExt>

  type TypeExprSymbolEvalResult<'valueExt when 'valueExt: comparison> =
    State<TypeSymbol, TypeCheckContext<'valueExt>, TypeCheckState<'valueExt>, Errors>

  type TypeExprSymbolEval<'valueExt when 'valueExt: comparison> =
    TypeChecker<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt>
      -> Location
      -> TypeExpr<'valueExt>
      -> TypeExprSymbolEvalResult<'valueExt>
