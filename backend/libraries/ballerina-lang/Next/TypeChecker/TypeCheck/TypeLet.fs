namespace Ballerina.DSL.Next.Types.TypeChecker

module TypeLet =
  open Ballerina.StdLib.String
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
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

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckTypeLet<'valueExt when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprTypeLet<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun
          context_t
          ({ Name = typeIdentifier
             TypeDef = typeDefinition
             Body = rest }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          // do Console.WriteLine($"TypeLet binding evaluating typeDefinition {typeDefinition.ToFSharpString}")
          // do Console.ReadLine() |> ignore

          let! typeDefinition =
            TypeExpr.Eval () typeCheckExpr (Some(ExprTypeLetBindingName typeIdentifier)) loc0 typeDefinition
            |> Expr.liftTypeEval
            |> state.MapContext(
              TypeCheckContext.Updaters.Scope(TypeCheckScope.Updaters.Type(replaceWith (Some typeIdentifier)))
            )

          // do Console.WriteLine $"Evaluated to {(typeDefinition |> fst)}"
          // do Console.ReadLine() |> ignore

          let! scope = state.GetContext() |> state.Map(fun ctx -> ctx.Scope)

          do!
            TypeCheckState.bindType (typeIdentifier |> Identifier.LocalScope |> scope.Resolve) typeDefinition
            |> Expr.liftTypeEval

          let scope = scope |> TypeCheckScope.Updaters.Type(replaceWith (Some typeIdentifier))

          let bind_component (v, t, k) : Updater<Map<ResolvedIdentifier, (TypeValue<'valueExt> * Kind)>> =
            Map.add v (t, k)

          let! definition_cases =
            typeDefinition
            |> fst
            |> TypeValue.AsUnion
            |> ofSum
            |> state.Catch
            |> state.Map(Sum.toOption)

          // do Console.WriteLine $"Definition cases: {definition_cases}"
          // do Console.ReadLine() |> ignore

          let! bind_definition_cases =
            definition_cases
            |> Option.map (fun (type_parameters, definition_cases) ->
              definition_cases
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, arg_t) ->
                state {
                  let wrap_type_params arg_t =
                    arg_t
                    |> List.foldBack
                      (fun tp acc -> TypeValue.CreateLambda(tp, acc |> TypeExpr.FromTypeValue))
                      type_parameters

                  let wrap_type_params_kind arg_t =
                    arg_t
                    |> List.foldBack (fun (tp: TypeParameter) acc -> Kind.Arrow(tp.Kind, acc)) type_parameters

                  do!
                    TypeCheckState.bindUnionCaseConstructor
                      (k.Name |> scope.Resolve)
                      (arg_t |> wrap_type_params, type_parameters, definition_cases)
                    |> Expr.liftTypeEval

                  let constructor_t =
                    TypeValue.CreateArrow(arg_t, TypeValue.CreateUnion(definition_cases))
                    |> wrap_type_params

                  let constructor_k = Kind.Star |> wrap_type_params_kind

                  return
                    bind_component (k.Name |> scope.Resolve, constructor_t, constructor_k)
                    >> bind_component (k.Name |> TypeCheckScope.Empty.Resolve, constructor_t, constructor_k)

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

          let! bind_definition_fields =
            definition_fields
            |> Option.map (fun definition_fields ->
              definition_fields
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, (arg_t, _arg_k)) ->
                state {
                  do!
                    TypeCheckState.bindRecordField (k.Name |> scope.Resolve) (definition_fields, arg_t)
                    |> Expr.liftTypeEval

                  return
                    bind_component (
                      k.Name |> scope.Resolve,
                      TypeValue.CreateArrow(TypeValue.CreateRecord(definition_fields), arg_t),
                      Kind.Star
                    )
                })
              |> state.All)
            |> state.RunOption
            |> state.Map(Option.map (List.fold (>>) id) >> Option.defaultValue id)

          let! entities =
            typeDefinition
            |> fst
            |> TypeValue.AsSchema
            |> ofSum
            |> state.Catch
            |> state.Map(Sum.toOption)

          do!
            entities
            |> Option.map (fun schema ->
              schema.Entities
              |> OrderedMap.toSeq
              |> Seq.map (fun (entityName, entityDef) ->
                state {
                  let scope =
                    { TypeCheckScope.Empty with
                        Type = Some(typeIdentifier) }

                  do!
                    TypeCheckState.bindType
                      (entityName.Name |> Identifier.LocalScope |> scope.Resolve)
                      (entityDef.TypeWithProps, Kind.Star)
                    |> Expr.liftTypeEval

                  return ()
                })
              |> state.All)
            |> state.RunOption
            |> state.Ignore

          let! rest, rest_t, rest_k, ctx_rest =
            !rest
            |> state.MapContext(TypeCheckContext.Updaters.Values(bind_definition_cases >> bind_definition_fields))

          return
            Expr.TypeLet(
              typeIdentifier,
              typeDefinition |> fst,
              rest,
              loc0,
              ctx.Scope |> TypeCheckScope.Updaters.Type(replaceWith (Some typeIdentifier))
            ),
            rest_t,
            rest_k,
            ctx_rest
        }
