namespace Ballerina.DSL.Next.Types.TypeChecker

module TypeLet =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
  open System
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.AdHocPolymorphicOperators
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Types.TypeChecker.Primitive
  open Ballerina.DSL.Next.Types.TypeChecker.Lookup
  open Ballerina.DSL.Next.Types.TypeChecker.Lambda
  open Ballerina.DSL.Next.Types.TypeChecker.Apply
  open Ballerina.DSL.Next.Types.TypeChecker.If
  open Ballerina.DSL.Next.Types.TypeChecker.Let
  open Ballerina.DSL.Next.Types.TypeChecker.RecordCons
  open Ballerina.DSL.Next.Types.TypeChecker.RecordWith
  open Ballerina.DSL.Next.Types.TypeChecker.RecordDes
  open Ballerina.DSL.Next.Types.TypeChecker.UnionDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumCons
  open Ballerina.DSL.Next.Types.TypeChecker.TupleDes
  open Ballerina.DSL.Next.Types.TypeChecker.TupleCons
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLambda
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal TypeCheckTypeLet
      (typeCheckExpr: TypeChecker, loc0: Location)
      : TypeChecker<ExprTypeLet<TypeExpr, Identifier>> =
      fun
          context_t
          ({ Name = typeIdentifier
             TypeDef = typeDefinition
             Body = rest }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          let! typeDefinition =
            TypeExpr.Eval (Some(ExprTypeLetBindingName typeIdentifier)) loc0 typeDefinition
            |> Expr.liftTypeEval
            |> state.MapContext(
              TypeCheckContext.Updaters.Types(
                TypeExprEvalContext.Updaters.Scope(TypeCheckScope.Updaters.Type(replaceWith (Some typeIdentifier)))
              )
            )

          let! scope = state.GetContext() |> state.Map(fun ctx -> ctx.Types.Scope)

          do!
            TypeExprEvalState.bindType (typeIdentifier |> Identifier.LocalScope |> scope.Resolve) typeDefinition
            |> Expr.liftTypeEval

          let scope = scope |> TypeCheckScope.Updaters.Type(replaceWith (Some typeIdentifier))

          let bind_component (v, t, k) : Updater<Map<ResolvedIdentifier, (TypeValue * Kind)>> = Map.add v (t, k)

          let! definition_cases =
            typeDefinition
            |> fst
            |> TypeValue.AsUnion
            |> ofSum
            |> state.Catch
            |> state.Map(Sum.toOption)
            |> state.Map(Option.map WithTypeExprSourceMapping.Getters.Value)

          let! bind_definition_cases =
            definition_cases
            |> Option.map (fun definition_cases ->
              definition_cases
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, argT) ->
                state {
                  do!
                    TypeExprEvalState.bindUnionCaseConstructor (k.Name |> scope.Resolve) (argT, definition_cases)
                    |> Expr.liftTypeEval

                  return
                    bind_component (
                      k.Name |> scope.Resolve,
                      TypeValue.CreateRecord(
                        OrderedMap.ofList
                          [ TypeSymbol.Create(Identifier.LocalScope "cons"),
                            TypeValue.CreateArrow(argT, TypeValue.CreateUnion(definition_cases))
                            TypeSymbol.Create(Identifier.LocalScope "map"),
                            TypeValue.CreateArrow(
                              TypeValue.CreateArrow(argT, argT),
                              TypeValue.CreateArrow(
                                TypeValue.CreateUnion(definition_cases),
                                TypeValue.CreateUnion(definition_cases)
                              )
                            ) ]
                      ),
                      //
                      Kind.Star
                    )
                })
              |> state.All)
            |> state.RunOption
            |> state.Map(Option.map (List.fold (>>) id) >> Option.defaultValue id)

          let! definition_fields =
            typeDefinition
            |> fst
            |> TypeValue.AsRecord
            |> ofSum
            |> state.Catch
            |> state.Map(Sum.toOption)
            |> state.Map(Option.map WithTypeExprSourceMapping.Getters.Value)

          let! bind_definition_fields =
            definition_fields
            |> Option.map (fun definition_fields ->
              definition_fields
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, argT) ->
                state {
                  do!
                    TypeExprEvalState.bindRecordField (k.Name |> scope.Resolve) (definition_fields, argT)
                    |> Expr.liftTypeEval

                  return
                    bind_component (
                      k.Name |> scope.Resolve,
                      TypeValue.CreateRecord(
                        OrderedMap.ofList
                          [ TypeSymbol.Create(Identifier.LocalScope "get"),
                            TypeValue.CreateArrow(TypeValue.CreateRecord(definition_fields), argT)
                            TypeSymbol.Create(Identifier.LocalScope "map"),
                            TypeValue.CreateArrow(
                              TypeValue.CreateArrow(argT, argT),
                              TypeValue.CreateArrow(
                                TypeValue.CreateRecord(definition_fields),
                                TypeValue.CreateRecord(definition_fields)
                              )
                            ) ]
                      ),
                      Kind.Star
                    )
                })
              |> state.All)
            |> state.RunOption
            |> state.Map(Option.map (List.fold (>>) id) >> Option.defaultValue id)

          let! rest, rest_t, rest_k =
            !rest
            |> state.MapContext(TypeCheckContext.Updaters.Values(bind_definition_cases >> bind_definition_fields))

          return
            Expr.TypeLet(
              typeIdentifier,
              typeDefinition |> fst,
              rest,
              loc0,
              ctx.Types.Scope
              |> TypeCheckScope.Updaters.Type(replaceWith (Some typeIdentifier))
            ),
            rest_t,
            rest_k
        }
