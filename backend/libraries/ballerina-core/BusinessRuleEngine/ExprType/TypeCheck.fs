namespace Ballerina.DSL.Expr.Types

module TypeCheck =
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.Unification
  open Ballerina.Errors
  open Ballerina.Core.Object

  type TypeName = string

  type TypeChecker<'source> = TypeBindings -> VarTypes -> 'source -> Sum<ExprType, Errors>

  type TypeCheck<'ExprExtension, 'ValueExtension> = TypeChecker<Expr<'ExprExtension, 'ValueExtension>>

  let private notImplementedError exprName =
    sum.Throw(Errors.Singleton $"Error: not implemented Expr type checker for expression {exprName}")

  type Expr<'ExprExtension, 'ValueExtension> with
    static member typeCheck
      : (TypeChecker<Expr<'ExprExtension, 'ValueExtension>>
          -> TypeChecker<Value<'ExprExtension, 'ValueExtension>>
          -> TypeChecker<'ExprExtension>)
          -> (TypeChecker<Expr<'ExprExtension, 'ValueExtension>>
            -> TypeChecker<Value<'ExprExtension, 'ValueExtension>>
            -> TypeChecker<'ValueExtension>)
          -> TypeChecker<Expr<'ExprExtension, 'ValueExtension>> =
      fun typeCheckExprExtension typeCheckValueExtension typeBindings vars e ->
        let lookup (t: ExprType) : Sum<ExprType, Errors> =
          sum {
            match t with
            | ExprType.LookupType lookupTypeId ->
              let! lookupType =
                typeBindings
                |> Map.tryFindWithError lookupTypeId "type id" lookupTypeId.TypeName

              return lookupType
            | _ -> return t
          }


        let typeCheckVarLookup (vars: VarTypes) (v: VarName) : Sum<ExprType, Errors> =
          sum { return! vars |> Map.tryFindWithError v "var" v.VarName }

        let rec typeCheckRecordFieldLookup
          (vars: VarTypes)
          (e: Expr<'ExprExtension, 'ValueExtension>)
          (field: string)
          : Sum<ExprType, Errors> =
          let (!) = eval vars

          sum {
            let! eType = !e

            match eType with
            | RecordType entityDescriptor ->
              let! fieldDescriptorType =
                entityDescriptor
                |> Map.tryFindWithError field "field" field
                |> sum.MapError(Errors.Map(fun e -> e + " in record" + eType.ToString()))

              fieldDescriptorType
            | _ ->
              return!
                sum.Throw(
                  $$"""Error: cannot access field {{field}} on value {{e.ToString()}} because it's not a record"""
                  |> Errors.Singleton
                )
          }

        and typeCheckMatchCase
          (vars: VarTypes)
          (e: Expr<'ExprExtension, 'ValueExtension>)
          (caseHandlers: Map<string, VarName * Expr<'ExprExtension, 'ValueExtension>>)
          : Sum<ExprType, Errors> =
          let (!) = eval vars

          sum {
            let! eType = !e

            match eType with
            | UnionType cases ->
              let handledCases = caseHandlers |> Seq.map (fun h -> h.Key) |> Set.ofSeq

              let expectedCases =
                cases |> Map.values |> Seq.map (fun h -> h.CaseName) |> Set.ofSeq

              if Set.isProperSuperset handledCases expectedCases then
                return! sum.Throw(Errors.Singleton $"Error: too many handlers {handledCases - expectedCases}")
              elif Set.isProperSuperset expectedCases handledCases then
                return!
                  sum.Throw(Errors.Singleton $"Error: not enough handlers, missing {expectedCases - handledCases}")
              else
                let! casesWithHandler =
                  cases
                  |> Map.values
                  |> Seq.map (fun case ->
                    caseHandlers
                    |> Map.tryFind case.CaseName
                    |> Option.map (fun (varName, body) -> case, varName, body)
                    |> Sum.fromOption (fun () ->
                      Errors.Singleton $"Error: missing case handler for case {case.CaseName}"
                      |> Errors.WithPriority ErrorPriority.High))
                  |> sum.All

                let! handlerTypes =
                  casesWithHandler
                  |> List.map (fun (case, varName, body) ->
                    sum {
                      let vars'' = vars |> Map.add varName case.Fields
                      let! bodyType = eval vars'' body
                      bodyType
                    })
                  |> sum.All

                match handlerTypes with
                | [] ->
                  return!
                    sum.Throw(
                      Errors.Singleton
                        $"Error: matchCase {e} has no case handlers. One case handler is required for each possible case."
                    )
                | x :: xs ->
                  let! ``type`` =
                    xs
                    |> List.fold
                      (fun unifications expr ->
                        sum {
                          let! prevExpr, _ = unifications

                          let! newUnifications = ExprType.Unify Map.empty typeBindings prevExpr expr

                          return expr, newUnifications
                        })
                      (sum { x, UnificationConstraints.Zero() })

                  ``type`` |> fst
            | t ->
              return!
                sum.Throw(
                  sprintf "Error: unexpected matchCase on type %A when typechecking expression %A" t e
                  |> Errors.Singleton
                )
          }

        and typeCheckValue
          (_: TypeBindings)
          (vars: VarTypes)
          (v: Value<'ExprExtension, 'ValueExtension>)
          : Sum<ExprType, Errors> =
          let (!) = eval vars

          sum {
            match v with
            | Value.Tuple items ->
              let! evaluatedItems = items |> List.map Expr.Value |> List.map (!) |> sum.All
              let itemTypes = evaluatedItems
              ExprType.TupleType itemTypes
            | Value.Extension e ->
              return!
                typeCheckValueExtension
                  (Expr.typeCheck typeCheckExprExtension typeCheckValueExtension)
                  typeCheckValue
                  typeBindings
                  vars
                  e
            | _ -> return! sum.Throw($"not implemented type checker for value {v.ToString()}" |> Errors.Singleton)
          }

        and typeCheckProjection
          (vars: VarTypes)
          (e: Expr<'ExprExtension, 'ValueExtension>)
          (i: int)
          : Sum<ExprType, Errors> =
          let (!) = eval vars

          sum {
            let! t = !e

            match t with
            | ExprType.TupleType itemTypes ->
              if i > 0 && i <= itemTypes.Length then
                itemTypes.[i - 1]
              else
                return!
                  sum.Throw(
                    $"Error: invalid lookup index {i} when looking up {e.ToString()}."
                    |> Errors.Singleton
                  )

            | _ ->
              return!
                sum.Throw(
                  $"Error: invalid lookup type {t.ToString()} when looking up {e.ToString()}.Item{i}."
                  |> Errors.Singleton
                )
          }

        and eval (vars: VarTypes) (e: Expr<'ExprExtension, 'ValueExtension>) : Sum<ExprType, Errors> =

          let result =
            match e with
            | Expr.VarLookup v -> typeCheckVarLookup vars v
            | Expr.RecordFieldLookup(e, field) -> typeCheckRecordFieldLookup vars e field
            | Expr.MatchCase(e, caseHandlers) -> typeCheckMatchCase vars e caseHandlers
            | Expr.Value v -> typeCheckValue typeBindings vars v
            | Expr.Project(e, i) -> typeCheckProjection vars e i
            | Expr.Apply(_, _) -> notImplementedError "Apply"
            | Expr.MakeRecord _ -> notImplementedError "MakeRecord"
            | Expr.MakeTuple _ -> notImplementedError "MakeTuple"
            | Expr.MakeSet _ -> notImplementedError "MakeSet"
            | Expr.MakeCase(_, _) -> notImplementedError "MakeCase"
            | Expr.GenericApply(_, _) -> notImplementedError "GenericApply"
            | Expr.Let(_, _, _) -> notImplementedError "Let"
            | Expr.LetType(_, _, _) -> notImplementedError "Let type"
            | Expr.Annotate(_, _) -> notImplementedError "Annotate"
            | Expr.Extension ext ->
              typeCheckExprExtension
                (Expr.typeCheck typeCheckExprExtension typeCheckValueExtension)
                typeCheckValue
                typeBindings
                vars
                ext

          sum {
            let! t = result
            return! lookup t
          }

        eval vars e
