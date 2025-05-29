namespace Ballerina.DSL.Expr.Types

module TypeCheck =
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.Unification
  open Ballerina.Errors
  open Ballerina.DSL.Model
  open Ballerina.Core.Object

  type TypeName = string

  type Expr with
    static member typeCheck
      (typeBindings: TypeBindings)
      (schema: Schema)
      (vars: VarTypes)
      (e: Expr)
      : Sum<ExprType * VarTypes, Errors> =
      let lookup t =
        sum {
          match t with
          | ExprType.LookupType lookupTypeId ->
            let! lookupType =
              typeBindings
              |> Map.tryFindWithError lookupTypeId "type id" lookupTypeId.TypeName

            return Some lookupTypeId.TypeName, lookupType
          | _ -> return None, t
        }

      let rec eval (vars: VarTypes) (e: Expr) : Sum<Option<TypeName> * ExprType * VarTypes, Errors> =
        let notImplementedError exprName =
          sum.Throw(Errors.Singleton $"Error: not implemented Expr type checker for expression {exprName}")

        let result =
          match e with
          | Expr.Exists(varName, entityDescriptor, condition) ->
            sum {
              let vars' = vars |> Map.add varName (ExprType.SchemaLookupType entityDescriptor)
              return! eval vars' condition
            }
          | Expr.VarLookup v ->
            sum {
              let! varType = vars |> Map.tryFindWithError v "var" v.VarName
              return (None, varType, vars)
            }
          | Expr.RecordFieldLookup(e, field) ->
            sum {
              let! eTypeName, eType, vars' = eval vars e

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

                return None, fieldDescriptorType, vars'
              | _ ->
                return!
                  sum.Throw(
                    $$"""Error: cannot access field {{field}} on value {{e.ToString()}} because it's not a record"""
                    |> Errors.Singleton
                  )
            }
          | Expr.IsCase(caseName, e) ->
            sum {
              let! _, eType, vars' = eval vars e

              match eType with
              | UnionType cases ->
                let! _ =
                  cases
                  |> Map.tryFind { CaseName = caseName }
                  |> Sum.fromOption (fun () ->
                    $$"""Error: invalid case name {{caseName}} on {{eType}}""" |> Errors.Singleton)

                return None, ExprType.PrimitiveType PrimitiveType.BoolType, vars'
              | t ->
                return!
                  sum.Throw(
                    sprintf "Error: unexpected case check on type %A when typechecking expression %A" t e
                    |> Errors.Singleton
                  )
            }
          | Expr.MatchCase(e, caseHandlers) ->
            sum {
              let! _, eType, vars' = eval vars e

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
                        let vars'' = vars' |> Map.add varName case.Fields
                        let! _, bodyType, _ = eval vars'' body
                        return bodyType
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
                        (sum { return x, UnificationConstraints.Zero() })

                    return None, ``type`` |> fst, vars'
              | t ->
                return!
                  sum.Throw(
                    sprintf "Error: unexpected matchCase on type %A when typechecking expression %A" t e
                    |> Errors.Singleton
                  )
            }
          | Expr.FieldLookup(e, field) ->
            sum {
              let! _, eType, vars' = eval vars e

              match eType with
              | SchemaLookupType _ ->
                let! fieldDescriptor =
                  schema.tryFindField field
                  |> Sum.fromOption (fun () ->
                    (sprintf "Error: cannot find field '%s'" field.FieldName) |> Errors.Singleton)

                return None, fieldDescriptor.Type(), vars'
              | t ->
                return!
                  sum.Throw(
                    sprintf "Error: unexpected lookup on type %A when typechecking expression %A" t e
                    |> Errors.Singleton
                  )
            }
          | Expr.Value v ->
            sum {
              match v with
              | Value.ConstInt _ -> return None, PrimitiveType PrimitiveType.IntType, vars
              | Value.ConstBool _ -> return None, PrimitiveType PrimitiveType.BoolType, vars
              | Value.ConstString _ -> return None, PrimitiveType PrimitiveType.StringType, vars
              | Value.Tuple items ->
                let! evaluatedItems = items |> List.map Expr.Value |> List.map (eval vars) |> sum.All
                let itemTypes = evaluatedItems |> List.map (fun (_, itemType, _) -> itemType)
                None, ExprType.TupleType itemTypes, vars
              | _ ->
                return!
                  sum.Throw(
                    $"not implemented type checker for value expression {e.ToString()}"
                    |> Errors.Singleton
                  )
            }
          | Expr.Binary(op, e1, e2) ->
            match op with
            | Or ->
              sum {
                let! _, t1, vars' = eval vars e1
                let! _, t2, vars'' = eval vars' e2

                match t1, t2 with
                | PrimitiveType BoolType, PrimitiveType BoolType -> return None, PrimitiveType BoolType, vars''
                | _ -> return! sum.Throw($$"""Error: invalid type of expression {{e}}""" |> Errors.Singleton)
              }
            | Equals ->
              sum {
                let! _, t1, vars' = eval vars e1
                let! _, t2, vars'' = eval vars' e2

                if t1 = t2 then
                  return None, PrimitiveType BoolType, vars''
                else
                  return!
                    sum.Throw(
                      $$"""Error: cannot compare different types {{t1}} and {{t2}}"""
                      |> Errors.Singleton
                    )
              }
            | Plus ->
              sum {
                let! _, t1, vars' = eval vars e1
                let! _, t2, vars'' = eval vars' e2

                match t1, t2 with
                | PrimitiveType IntType, PrimitiveType IntType -> return None, PrimitiveType IntType, vars''
                | _ ->
                  return!
                    sum.Throw(
                      $"not implemented type checker for binary expression {e.ToString()}"
                      |> Errors.Singleton
                    )
              }
            | And -> notImplementedError "Binary op: And"
            | Minus -> notImplementedError "Binary op: Minus"
            | Times -> notImplementedError "Binary op: Times"
            | GreaterThan -> notImplementedError "Binary op: GreaterThan"
            | GreaterThanEquals -> notImplementedError "Binary op: GreaterThanEquals"
            | DividedBy -> notImplementedError "Binary op: DividedBy"
          | Expr.Project(e, i) ->
            sum {
              let! _, t, vars' = eval vars e

              match t with
              | ExprType.TupleType itemTypes ->
                if i > 0 && i <= itemTypes.Length then
                  return None, itemTypes.[i - 1], vars'
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
          | Expr.SumBy(_, _, _) -> notImplementedError "SumBy"
          | Expr.Unary(_, _) -> notImplementedError "Unary"
          | Expr.Apply(_, _) -> notImplementedError "Apply"
          | Expr.MakeRecord _ -> notImplementedError "MakeRecord"
          | Expr.MakeTuple _ -> notImplementedError "MakeTuple"
          | Expr.MakeSet _ -> notImplementedError "MakeSet"
          | Expr.MakeCase(_, _) -> notImplementedError "MakeCase"

        sum {
          let! _, t, vars = result
          let! n, t = lookup t
          return n, t, vars
        }

      let result = eval vars e
      result |> Sum.map (fun (_, x, y) -> x, y)
