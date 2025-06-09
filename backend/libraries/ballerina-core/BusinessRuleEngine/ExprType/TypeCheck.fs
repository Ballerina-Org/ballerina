namespace Ballerina.DSL.Expr.Types

module TypeCheck =
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.Unification
  open Ballerina.Errors
  open Ballerina.Core.Object

  type TypeName = string

  let private notImplementedError exprName =
    sum.Throw(Errors.Singleton $"Error: not implemented Expr type checker for expression {exprName}")

  type Expr<'ExprExtension, 'ValueExtension> with
    static member typeCheck
      (typeBindings: TypeBindings)
      (vars: VarTypes)
      (e: Expr<'ExprExtension, 'ValueExtension>)
      : Sum<ExprType, Errors> =
      let lookup (t: ExprType) : Sum<Option<TypeName> * ExprType, Errors> =
        sum {
          match t with
          | ExprType.LookupType lookupTypeId ->
            let! lookupType =
              typeBindings
              |> Map.tryFindWithError lookupTypeId "type id" lookupTypeId.TypeName

            return Some lookupTypeId.TypeName, lookupType
          | _ -> return None, t
        }


      let typeCheckVarLookup (vars: VarTypes) (v: VarName) : Sum<Option<TypeName> * ExprType, Errors> =
        sum {
          let! varType = vars |> Map.tryFindWithError v "var" v.VarName
          None, varType
        }

      let rec typeCheckRecordFieldLookup
        (vars: VarTypes)
        (e: Expr<'ExprExtension, 'ValueExtension>)
        (field: string)
        : Sum<Option<TypeName> * ExprType, Errors> =
        let (!) = eval vars

        sum {
          let! eTypeName, eType = !e

          match eType with
          | RecordType entityDescriptor ->
            let! fieldDescriptorType =
              entityDescriptor
              |> Map.tryFindWithError field "field" field
              |> sum.MapError(
                Errors.Map(fun e ->
                  e
                  + " in record "
                  + match eTypeName with
                    | Some n -> n
                    | _ -> eType.ToString())
              )

            None, fieldDescriptorType
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
        : Sum<Option<TypeName> * ExprType, Errors> =
        let (!) = eval vars

        sum {
          let! _, eType = !e

          match eType with
          | UnionType cases ->
            let handledCases = caseHandlers |> Seq.map (fun h -> h.Key) |> Set.ofSeq

            let expectedCases =
              cases |> Map.values |> Seq.map (fun h -> h.CaseName) |> Set.ofSeq

            if Set.isProperSuperset handledCases expectedCases then
              return! sum.Throw(Errors.Singleton $"Error: too many handlers {handledCases - expectedCases}")
            elif Set.isProperSuperset expectedCases handledCases then
              return! sum.Throw(Errors.Singleton $"Error: not enough handlers, missing {expectedCases - handledCases}")
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
                    let! _, bodyType = eval vars'' body
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

                None, ``type`` |> fst
          | t ->
            return!
              sum.Throw(
                sprintf "Error: unexpected matchCase on type %A when typechecking expression %A" t e
                |> Errors.Singleton
              )
        }

      and typeCheckValue
        (vars: VarTypes)
        (v: Value<'ExprExtension, 'ValueExtension>)
        : Sum<Option<TypeName> * ExprType, Errors> =
        let (!) = eval vars

        sum {
          match v with
          | Value.ConstInt _ -> None, PrimitiveType PrimitiveType.IntType
          | Value.ConstBool _ -> None, PrimitiveType PrimitiveType.BoolType
          | Value.ConstString _ -> None, PrimitiveType PrimitiveType.StringType
          | Value.Tuple items ->
            let! evaluatedItems = items |> List.map Expr.Value |> List.map (!) |> sum.All
            let itemTypes = evaluatedItems |> List.map (fun (_, itemType) -> itemType)
            None, ExprType.TupleType itemTypes
          | _ ->
            return!
              sum.Throw(
                $"not implemented type checker for value expression {e.ToString()}"
                |> Errors.Singleton
              )
        }

      and typeCheckBinaryOperator
        (vars: VarTypes)
        (op: BinaryOperator)
        (e1: Expr<'ExprExtension, 'ValueExtension>)
        (e2: Expr<'ExprExtension, 'ValueExtension>)
        : Sum<Option<TypeName> * ExprType, Errors> =
        let (!) = eval vars

        sum {
          match op with
          | Or ->
            let! _, t1 = !e1
            let! _, t2 = !e2

            match t1, t2 with
            | PrimitiveType BoolType, PrimitiveType BoolType -> None, PrimitiveType BoolType
            | _ -> return! sum.Throw($$"""Error: invalid type of expression {{e}}""" |> Errors.Singleton)
          | Equals ->
            let! _, t1 = !e1
            let! _, t2 = !e2

            if t1 = t2 then
              None, PrimitiveType BoolType
            else
              return!
                sum.Throw(
                  $$"""Error: cannot compare different types {{t1}} and {{t2}}"""
                  |> Errors.Singleton
                )
          | Plus ->
            let! _, t1 = !e1
            let! _, t2 = !e2

            match t1, t2 with
            | PrimitiveType IntType, PrimitiveType IntType -> None, PrimitiveType IntType
            | _ ->
              return!
                sum.Throw(
                  $"not implemented type checker for binary expression {e.ToString()}"
                  |> Errors.Singleton
                )
          | And -> return! notImplementedError "Binary op: And"
          | Minus -> return! notImplementedError "Binary op: Minus"
          | Times -> return! notImplementedError "Binary op: Times"
          | GreaterThan -> return! notImplementedError "Binary op: GreaterThan"
          | GreaterThanEquals -> return! notImplementedError "Binary op: GreaterThanEquals"
          | DividedBy -> return! notImplementedError "Binary op: DividedBy"
        }

      and typeCheckProject
        (vars: VarTypes)
        (e: Expr<'ExprExtension, 'ValueExtension>)
        (i: int)
        : Sum<Option<TypeName> * ExprType, Errors> =
        let (!) = eval vars

        sum {
          let! _, t = !e

          match t with
          | ExprType.TupleType itemTypes ->
            if i > 0 && i <= itemTypes.Length then
              None, itemTypes.[i - 1]
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

      and eval (vars: VarTypes) (e: Expr<'ExprExtension, 'ValueExtension>) : Sum<Option<TypeName> * ExprType, Errors> =

        let result =
          match e with
          | Expr.VarLookup v -> typeCheckVarLookup vars v
          | Expr.RecordFieldLookup(e, field) -> typeCheckRecordFieldLookup vars e field
          | Expr.MatchCase(e, caseHandlers) -> typeCheckMatchCase vars e caseHandlers
          | Expr.Value v -> typeCheckValue vars v
          | Expr.Binary(op, e1, e2) -> typeCheckBinaryOperator vars op e1 e2
          | Expr.Project(e, i) -> typeCheckProject vars e i
          | Expr.Unary(_, _) -> notImplementedError "Unary"
          | Expr.Apply(_, _) -> notImplementedError "Apply"
          | Expr.MakeRecord _ -> notImplementedError "MakeRecord"
          | Expr.MakeTuple _ -> notImplementedError "MakeTuple"
          | Expr.MakeSet _ -> notImplementedError "MakeSet"
          | Expr.MakeCase(_, _) -> notImplementedError "MakeCase"
          | Expr.GenericApply(_, _) -> notImplementedError "GenericApply"
          | Expr.Let(_, _, _) -> notImplementedError "Let"
          | Expr.LetType(_, _, _) -> notImplementedError "Let type"
          | Expr.Annotate(_, _) -> notImplementedError "Annotate"
          | Expr.Extension _ -> notImplementedError "Extension"

        sum {
          let! _, t = result
          let! n, t = lookup t
          n, t
        }

      let result = eval vars e
      result |> Sum.map (fun (_, x) -> x)
