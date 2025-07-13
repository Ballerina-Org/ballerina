namespace Ballerina.DSL.Expr.Types

module TypeCheck =
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.Unification
  open Ballerina.DSL.Expr.Types.Patterns
  open Ballerina.Errors
  open Ballerina.StdLib.Object

  type TypeName = string

  type TypeChecker<'source> = TypeBindings -> VarTypes -> 'source -> Sum<ExprType, Errors>

  type TypeCheck<'ExprExtension, 'ValueExtension> = TypeChecker<Expr<'ExprExtension, 'ValueExtension>>

  let private notImplementedError exprName =
    sum.Throw(Errors.Singleton $"Error: not implemented Expr type checker for expression {exprName}")

  type Expr<'ExprExtension, 'ValueExtension> with
    static member typeCheck
      (typeCheckExprExtension:
        (TypeChecker<Expr<'ExprExtension, 'ValueExtension>>
          -> TypeChecker<Value<'ExprExtension, 'ValueExtension>>
          -> TypeChecker<'ExprExtension>))
      (typeCheckValueExtension:
        (TypeChecker<Expr<'ExprExtension, 'ValueExtension>>
          -> TypeChecker<Value<'ExprExtension, 'ValueExtension>>
          -> TypeChecker<'ValueExtension>))
      : TypeChecker<Expr<'ExprExtension, 'ValueExtension>> =
      fun (typeBindings: TypeBindings) (vars: VarTypes) (e: Expr<'ExprExtension, 'ValueExtension>) ->
        let lookup (t: ExprType) : Sum<ExprType, Errors> =
          sum {
            match t with
            | ExprType.LookupType lookupTypeId ->
              let! lookupType = typeBindings |> Map.tryFindWithError lookupTypeId "type id" lookupTypeId.VarName

              return lookupType
            | _ -> return t
          }

        let typeCheckVarLookup (vars: VarTypes) (_typeBindings: TypeBindings) (v: VarName) : Sum<ExprType, Errors> =
          sum { return! vars |> Map.tryFindWithError v "var" v.VarName }

        let rec typeCheckRecordFieldLookup
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          (e: Expr<'ExprExtension, 'ValueExtension>)
          (field: string)
          : Sum<ExprType, Errors> =
          let (!) = eval vars typeBindings

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
          (typeBindings: TypeBindings)
          (e: Expr<'ExprExtension, 'ValueExtension>)
          (caseHandlers: Map<string, VarName * Expr<'ExprExtension, 'ValueExtension>>)
          : Sum<ExprType, Errors> =
          let (!) = eval vars typeBindings

          sum {
            let! eType = !e

            match eType with
            | SumType(l, r) ->
              let! leftHandlerVarName, leftHandlerBody =
                caseHandlers
                |> Map.tryFind sumCaseNames.Left
                |> Sum.fromOption (fun () ->
                  Errors.Singleton $"Error: missing case handler for {sumCaseNames.Left}"
                  |> Errors.WithPriority ErrorPriority.High)

              let! rightHandlerVarName, rightHandlerBody =
                caseHandlers
                |> Map.tryFind sumCaseNames.Right
                |> Sum.fromOption (fun () ->
                  Errors.Singleton $"Error: missing case handler for {sumCaseNames.Right}"
                  |> Errors.WithPriority ErrorPriority.High)

              if caseHandlers.Count <> 2 then
                return!
                  sum.Throw(
                    Errors.Singleton
                      $"Error: matchCase {e} has {caseHandlers.Count} case handlers, but expected 2 ({sumCaseNames.Left} and {sumCaseNames.Right})."
                  )
              else
                let! leftHandler = eval (vars |> Map.add leftHandlerVarName l) typeBindings leftHandlerBody
                let! rightHandler = eval (vars |> Map.add rightHandlerVarName r) typeBindings rightHandlerBody

                do! ExprType.Unify vars typeBindings leftHandler rightHandler |> Sum.map ignore

                return leftHandler
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
                      let! bodyType = eval vars'' typeBindings body
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

        and typeCheckGenericLambda
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          (parameter: ExprTypeId)
          (body: Expr<'ExprExtension, 'ValueExtension>)
          : Sum<ExprType, Errors> =

          sum {
            let! bodyType = eval (vars |> Map.add parameter (ExprType.VarType parameter)) typeBindings body

            return ExprType.GenericType(parameter, ExprTypeKind.Star, bodyType)

          }

        and typeCheckLambda
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          ((x, x_t, returnType): VarName * option<ExprType> * option<ExprType>)
          (body: Expr<'ExprExtension, 'ValueExtension>)
          : Sum<ExprType, Errors> =
          sum {
            let x_t_generic =
              System.Guid.CreateVersion7().ToString() |> VarName.Create |> VarType

            let returnTypeGeneric =
              System.Guid.CreateVersion7().ToString() |> VarName.Create |> VarType

            let x_t = x_t |> Option.defaultValue x_t_generic
            let returnType = returnType |> Option.defaultValue returnTypeGeneric

            let! ret = eval (vars |> Map.add x x_t) typeBindings body
            do! ExprType.Unify vars typeBindings returnType ret |> sum.Map ignore
            return ExprType.ArrowType(x_t, returnType)
          }

        and typeCheckValue
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          (v: Value<'ExprExtension, 'ValueExtension>)
          : Sum<ExprType, Errors> =
          let (!) = eval vars typeBindings

          sum {
            match v with
            | Value.Tuple items ->
              let! evaluatedItems = items |> List.map Expr.Value |> List.map (!) |> sum.All
              let itemTypes = evaluatedItems
              ExprType.TupleType itemTypes
            | Value.Lambda(x, t, returnType, body) -> return! typeCheckLambda vars typeBindings (x, t, returnType) body
            | Value.Extension e ->
              return!
                typeCheckValueExtension
                  (Expr.typeCheck typeCheckExprExtension typeCheckValueExtension)
                  typeCheckValue
                  typeBindings
                  vars
                  e
            | Value.GenericLambda(parameter, body) -> return! typeCheckGenericLambda vars typeBindings parameter body
            | _ -> return! sum.Throw($"not implemented type checker for value {v.ToString()}" |> Errors.Singleton)
          }

        and typeCheckProjection
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          (e: Expr<'ExprExtension, 'ValueExtension>)
          (i: int)
          : Sum<ExprType, Errors> =
          let (!) = eval vars typeBindings

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

        and typeCheckGenericApplication
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          (f: Expr<'ExprExtension, 'ValueExtension>)
          (arg: ExprType)
          : Sum<ExprType, Errors> =
          let (!) = eval vars typeBindings

          sum {
            let! f = !f

            match f with
            | ExprType.GenericType(parameter, _, bodyType) ->
              let res = ExprType.Substitute ([ parameter, arg ] |> Map.ofSeq) bodyType
              return res
            | _ ->
              return!
                sum.Throw(
                  Errors.Singleton $"Error: type {f.ToString()} cannot be applied to generic argument {arg.ToString()}"
                )
          }

        and typeCheckApplication
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          (f: Expr<'ExprExtension, 'ValueExtension>)
          (arg: Expr<'ExprExtension, 'ValueExtension>)
          : Sum<ExprType, Errors> =
          let (!) = eval vars typeBindings

          sum {
            let! arg = !arg

            match f with
            | Expr.Value(Value.Lambda(x, x_t, returnType, body)) ->
              let x_t_generic =
                System.Guid.CreateVersion7().ToString() |> VarName.Create |> VarType

              let returnTypeGeneric =
                System.Guid.CreateVersion7().ToString() |> VarName.Create |> VarType

              let x_t = x_t |> Option.defaultValue x_t_generic
              let returnType = returnType |> Option.defaultValue returnTypeGeneric

              do! ExprType.Unify vars typeBindings x_t arg |> sum.Map ignore
              let! ret = eval (vars |> Map.add x x_t) typeBindings body
              do! ExprType.Unify vars typeBindings returnType ret |> sum.Map ignore
              return returnType
            | _ ->
              let! f = !f
              let! i, o = f |> ExprType.AsLambda
              do! ExprType.Unify vars typeBindings i arg |> sum.Map ignore
              return o
          }

        and typeCheckLet
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          (x: VarName)
          (e: Expr<'ExprExtension, 'ValueExtension>)
          (rest: Expr<'ExprExtension, 'ValueExtension>)
          : Sum<ExprType, Errors> =
          let (!) = eval vars typeBindings

          sum {
            let! e = !e
            return! eval (vars |> Map.add x e) typeBindings rest
          }

        and typeCheckMakeCase
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          (caseName: string)
          (value: Expr<'ExprExtension, 'ValueExtension>)
          : Sum<ExprType, Errors> =
          let (!) = eval vars typeBindings

          sum {
            let! value = !value

            match caseName with
            | caseName when caseName = sumCaseNames.Left ->
              let other =
                System.Guid.CreateVersion7().ToString() |> VarName.Create |> ExprType.VarType

              return ExprType.SumType(value, other)
            | caseName when caseName = sumCaseNames.Right ->
              let other =
                System.Guid.CreateVersion7().ToString() |> VarName.Create |> ExprType.VarType

              return ExprType.SumType(other, value)
            | _ ->
              return!
                sum.Throw(
                  Errors.Singleton $"Error: not implemented type checker for case {caseName} with value {value}"
                )
          }

        and typeCheckLetType
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          (typeName: VarName)
          (typeDef: ExprType)
          (rest: Expr<'ExprExtension, 'ValueExtension>)
          : Sum<ExprType, Errors> =
          sum {
            let newTypeBindings = typeBindings |> Map.add typeName typeDef
            let! rest = eval vars newTypeBindings rest
            return rest
          }

        and typeCheckMakeRecord
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          (record: Map<string, Expr<'ExprExtension, 'ValueExtension>>)
          : Sum<ExprType, Errors> =
          let (!) = eval vars typeBindings

          sum {
            // we evaluate each field of the record to get its type as a sum and then we create a record type from it
            let! evaluatedFields =
              record
              |> Map.toSeq
              |> Seq.map (fun (fieldName, expr) ->
                sum {
                  let! fieldType = !expr
                  return fieldName, fieldType
                })
              |> Seq.toList
              |> sum.All

            return ExprType.RecordType(evaluatedFields |> Map.ofSeq)
          }

        and eval
          (vars: VarTypes)
          (typeBindings: TypeBindings)
          (e: Expr<'ExprExtension, 'ValueExtension>)
          : Sum<ExprType, Errors> =

          let result =
            match e with
            | Expr.VarLookup v -> typeCheckVarLookup vars typeBindings v
            | Expr.RecordFieldLookup(e, field) -> typeCheckRecordFieldLookup vars typeBindings e field
            | Expr.MatchCase(e, caseHandlers) -> typeCheckMatchCase vars typeBindings e caseHandlers
            | Expr.Value v -> typeCheckValue vars typeBindings v
            | Expr.Project(e, i) -> typeCheckProjection vars typeBindings e i
            | Expr.Apply(f, arg) -> typeCheckApplication vars typeBindings f arg
            | Expr.MakeRecord record -> typeCheckMakeRecord vars typeBindings record
            | Expr.MakeTuple _ -> notImplementedError "MakeTuple"
            | Expr.MakeSet _ -> notImplementedError "MakeSet"
            | Expr.MakeCase(caseName, value) -> typeCheckMakeCase vars typeBindings caseName value
            | Expr.GenericApply(f, arg) -> typeCheckGenericApplication vars typeBindings f arg
            | Expr.Let(x, e, rest) -> typeCheckLet vars typeBindings x e rest
            | Expr.LetType(typeName, typeDef, rest) -> typeCheckLetType vars typeBindings typeName typeDef rest
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
            return! sum.Any2 (lookup t) (sum { t })
          }

        eval vars typeBindings e
