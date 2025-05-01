namespace Ballerina.DSL.Expr

module Eval =
  open System
  open System.Linq
  open Ballerina.Fun
  open Ballerina.Coroutines
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Model
  open Ballerina.DSL.Expr
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.TypeCheck

  type Expr with
    static member eval
      (variableRestriction: Option<VarName * (obj -> bool)>)
      (schema: Schema)
      (vars: Vars)
      : Expr -> list<Vars * Value> =
      let rec eval (vars: Vars) (e: Expr) : list<Vars * Value> =
        match e with
        | Exists(varName, entityDescriptorId, condition) ->
          let restriction =
            match variableRestriction with
            | Some(restrictedVarName, predicate) when varName = restrictedVarName -> predicate
            | _ -> fun _ -> true

          [ for entityDescriptor in schema.tryFindEntity entityDescriptorId |> Option.toList do
              let values = entityDescriptor.GetEntities() |> Seq.filter restriction

              yield!
                [ for value in values do
                    for valueId in entityDescriptor.GetId value |> Option.toList do
                      let vars' = vars |> Map.add varName (entityDescriptorId, One valueId)

                      for res in eval vars' condition do
                        match res with
                        | _, ConstBool true -> yield res
                        | _ -> () ] ]
        | VarLookup v when vars |> Map.containsKey v -> [ vars, (Value.Var(vars.[v])) ]
        | FieldLookup(e, field) ->
          [ for fieldDescriptor in schema.tryFindField field |> Option.toList do
              for res1 in eval vars e do
                match res1 with
                | (vars', Value.Var(_, One entityId))
                | (vars', Value.ConstGuid(entityId)) ->
                  for value in fieldDescriptor.Get entityId |> Option.toList do
                    yield (vars', value)
                | _ ->
                  do printfn "unsupported field lookup %A -> %A" ([ e, field ]) res1
                  do Console.ReadLine() |> ignore
                  () ]
        | Value v -> [ vars, v ]
        | Binary(Plus, e1, e2) ->
          [ for vars', (i1, i2) in eval2AsInt vars e1 e2 do
              yield vars', Value.ConstInt(i1 + i2) ]
        | Binary(Equals, e1, e2) ->
          [ for vars', res1 in eval vars e1 do
              for vars'', res2 in eval vars' e2 do
                yield vars'', Value.ConstBool(res1 = res2) ]
        | e ->
          printfn "not implemented Expr evaluator for %A" e
          []

      and eval2AsInt vars e1 e2 =
        [ for vars', v1 in eval vars e1 do
            for vars'', v2 in eval vars' e2 do
              match v1, v2 with
              | Value.ConstInt i1, Value.ConstInt i2 -> yield vars'', (i1, i2)
              | _ -> () ]

      eval vars

    static member lookedUpFieldDescriptors(e: Expr) =
      let (!) e = Expr.lookedUpFieldDescriptors e

      match e with
      | Expr.FieldLookup(e, f) ->
        seq {
          yield Set.singleton {| FieldDescriptorId = f |}
          yield !e
        }
        |> Set.unionMany
      | Expr.Binary(_, e1, e2) -> !e1 |> Set.union !e2
      | Expr.SumBy(_, _, e)
      | Expr.Exists(_, _, e) -> !e
      | _ -> Set.empty

    static member scope(e: Expr) =
      let (!) e = Expr.scope e

      match e with
      | Expr.Exists(varName, entityType, e) ->
        seq {
          yield
            {| varName = varName
               entityType = entityType |}

          yield! !e
        }
        |> Set.ofSeq
      | Expr.FieldLookup(e, _)
      | Expr.SumBy(_, _, e) -> !e
      | Expr.Binary(_, e1, e2) -> !e1 |> Set.union !e2
      | _ -> Set.empty

    static member scopeSeq(e: Expr) =
      let (!) e = Expr.scopeSeq e

      match e with
      | Expr.Exists(varName, entityType, e) ->
        seq {
          yield
            {| varName = varName
               entityType = entityType |}

          yield! !e
        }
      | Expr.FieldLookup(e, _)
      | Expr.SumBy(_, _, e) -> !e
      | Expr.Binary(_, e1, e2) ->
        seq {
          yield! !e1
          yield! !e2
        }
      | _ -> Seq.empty

    static member fieldLookups(e: Expr) =
      let (!) e = Expr.fieldLookups e

      match e with
      | Expr.FieldLookup((Expr.VarLookup varName), field) -> seq { yield (varName, [ field ]) } |> Set.ofSeq
      | Expr.FieldLookup(e, nextField) -> !e |> Set.map (fun (v, fs) -> v, fs @ [ nextField ])
      | Expr.Exists(_, _, e)
      | Expr.SumBy(_, _, e) -> !e
      | Expr.Binary(_, e1, e2) -> !e1 |> Set.union !e2
      | _ -> Set.empty

  type BusinessRule with
    member rule.Dependencies: Schema -> Set<FieldDescriptorId> -> RuleDependencies =
      fun schema changedFields ->
        // let variables = scope rule.Condition |> Seq.map (fun v -> v.varName, v.entityType) |> Map.ofSeq
        let conditionType = Expr.typeCheck Map.empty schema Map.empty rule.Condition

        match conditionType with
        | Right errors -> failwithf "Condition %A does not type check with errors %A" (rule.Condition) errors.Errors
        | Left(_, vars) ->
          let dependencies =
            [ for action in rule.Actions do
                let fieldLookups = Expr.fieldLookups action.Value
                // printfn "action.Value %A fieldLookups %A" action.Value fieldLookups
                // Console.ReadLine() |> ignore
                for (varName, fields) in fieldLookups do
                  match Expr.typeCheck Map.empty schema vars (Expr.VarLookup varName) with
                  | Left(SchemaLookupType varType, _) ->
                    let rec lookupPrefixes =
                      function
                      | [] -> []
                      | f :: fs ->
                        [ yield [], f
                          for prefix in lookupPrefixes fs do
                            let (prefix, lastElement) = prefix
                            yield f :: prefix, lastElement ]

                    let allLookups = lookupPrefixes fields
                    // do printfn "allLookups %A" allLookups
                    // do Console.ReadLine() |> ignore
                    for (lookupFields, lastField) in allLookups do
                      // do printfn "lookupFields %A" lookupFields
                      // do Console.ReadLine() |> ignore
                      // if lookupFields |> List.length >= 2 then
                      // do printfn "typeCheckFull of %A with %A" ((Expr.VarLookup varName =>> lookupFields)) vars
                      // do Console.ReadLine() |> ignore
                      match Expr.typeCheck Map.empty schema vars (Expr.VarLookup varName =>> lookupFields) with
                      | Left(SchemaLookupType lookupType, _) ->
                        let dependency: RuleDependency =
                          { ChangedEntityType = lookupType
                            ChangedField = lastField
                            RestrictedVariable = varName
                            RestrictedVariableType = varType
                            PathFromVariableToChange = lookupFields }

                        if changedFields |> Set.contains dependency.ChangedField then
                          yield dependency
                      | Left _ as e ->
                        failwithf
                          "Error %A\nUnexpected typecheck result in lookup path %A with lookupField %A and vars %A"
                          e
                          ((Expr.VarLookup varName =>> lookupFields))
                          lookupFields
                          vars
                      | Right e ->
                        failwithf
                          "Error %A\nCannot typecheck lookup path %A with lookupField %A and vars %A"
                          e.Errors
                          ((Expr.VarLookup varName =>> lookupFields))
                          lookupFields
                          vars
                    // and RuleDependency = { ChangedEntityType:EntityDescriptor; RestrictedVariable:string; RestrictedVariableType:EntityDescriptor; PathFromVariableToChange:List<FieldDescriptor> }
                    // and RuleDependencies = Map<EntityDescriptorId * FieldDescriptor, List<RuleDependency>>
                    // for each prefix of fields
                    // the type is the type of the varName + path, thus excluding the last fieldName
                    ()
                  | Left _ as e ->
                    failwithf
                      "Error\nUnexpected typecheck result %A in var lookup %A and vars %A"
                      e
                      (Expr.VarLookup varName)
                      vars
                  | Right e -> failwithf "Cannot typecheck varName %A with errors %A" (Expr.VarLookup varName) e.Errors ]

          let byEntityAndField: RuleDependencies =
            { dependencies =
                dependencies
                |> Seq.groupBy (fun dep -> dep.ChangedEntityType, dep.ChangedField)
                |> Map.ofSeq
                |> Map.map (fun _ -> Set.ofSeq) } in

          byEntityAndField
