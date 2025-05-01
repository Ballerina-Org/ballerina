namespace Ballerina.DSL

module TransitiveExecution =
  open System
  open System.Linq
  open Ballerina.Fun
  open Ballerina.Collections.Map
  open Ballerina.Coroutines
  open Ballerina.State.WithError
  open Ballerina.DSL.Model
  open Ballerina.DSL.Model
  open Ballerina.DSL.Predicate
  open Ballerina.DSL.Execute
  open Ballerina.DSL.Preprocessor
  open Ballerina.DSL.Expr.Eval
  open Ballerina.DSL.Expr.Model

  type TraceFrame =
    { ExecutedRules: Map<BusinessRuleId, EntitiesIdentifiers>
      ModifiedFields: Map<FieldDescriptorId, EntitiesIdentifiers> }

  type BusinessRuleExecutionError = Loop of List<TraceFrame>

  type BusinessRuleExecutionContext =
    { AllRules: Map<Guid, BusinessRule>
      Schema: Schema }

  type BusinessRuleExecutionState =
    { AllExecutedRules: Map<BusinessRuleId, EntitiesIdentifiers>
      CurrentExecutedRules: Map<BusinessRuleId, EntitiesIdentifiers>
      CurrentModifiedFields: Map<FieldDescriptorId, EntitiesIdentifiers>
      Trace: List<TraceFrame> }

    static member trace rules fields =
      fun s ->
        { s with
            Trace =
              { ExecutedRules = rules
                ModifiedFields = fields }
              :: s.Trace }

    static member clear =
      fun s ->
        { s with
            CurrentExecutedRules = Map.empty
            CurrentModifiedFields = Map.empty }

    static member addCurrentExecutedRule businessRuleId allModifiedTargets =
      fun s ->
        { s with
            CurrentExecutedRules = s.CurrentExecutedRules |> Map.add businessRuleId allModifiedTargets }

    static member addCurrentModifiedFields k v =
      fun s ->
        { s with
            CurrentModifiedFields =
              s.CurrentModifiedFields
              |> Map.upsert k (fun () -> v) (EntitiesIdentifiers.merge v) }

    static member updaters =
      {| CurrentExecutedRules =
          fun u s ->
            { s with
                CurrentExecutedRules = u (s.CurrentExecutedRules) }
         AllExecutedRules =
          fun u s ->
            { s with
                AllExecutedRules = u (s.AllExecutedRules) } |}

  let rec executeRulesTransitively
    ()
    : State<_, BusinessRuleExecutionContext, BusinessRuleExecutionState, BusinessRuleExecutionError> =
    state {
      let! { AllRules = allBusinessRules
             Schema = schema } = state.GetContext()

      let! { AllExecutedRules = executedRules
             CurrentModifiedFields = modifiedFields } = state.GetState()
      // do printfn "executedRules = %A" executedRules
      // do Console.ReadLine() |> ignore
      // do printfn "modifiedFields = %A" modifiedFields
      // do Console.ReadLine() |> ignore
      let candidateRules =
        BusinessRule.getCandidateRules allBusinessRules (modifiedFields |> Map.keys |> Set.ofSeq)
      // do printfn "candidateRules = %A" (candidateRules |> Seq.map (fun (r,f) -> r.BusinessRuleId,r.Name,f))
      // do Console.ReadLine() |> ignore
      do! state.SetState BusinessRuleExecutionState.clear
      // let mutable modifiedFields':Map<FieldDescriptorId, EntitiesIdentifiers> = Map.empty
      // let mutable executedRules':Map<BusinessRuleId, EntitiesIdentifiers> = Map.empty
      for (businessRule, relevantModifiedFieldIds) in candidateRules do
        let! _ = state.GetState()
        let businessRuleId = businessRule.ToBusinessRuleId

        let ruleDependencies =
          businessRule.Dependencies schema (modifiedFields |> Map.keys |> Set.ofSeq)
        // do printfn "ruleDependencies = %A" ruleDependencies
        // do Console.ReadLine() |> ignore
        let changedIds =
          seq {
            for relevantModifiedFieldId in relevantModifiedFieldIds do
              yield! modifiedFields |> Map.tryFind relevantModifiedFieldId |> Option.toList
          }
          |> Seq.fold EntitiesIdentifiers.merge (EntitiesIdentifiers.Multiple Set.empty)
        // do printfn "changedIds = %A" changedIds
        // do Console.ReadLine() |> ignore
        let predicatesByRestrictedVariable =
          match changedIds with
          | All -> Map.empty
          | Multiple changedIds -> ruleDependencies.PredicatesByRestrictedVariable schema changedIds
        // do printfn "predicatesByRestrictedVariable = %A" predicatesByRestrictedVariable
        // do Console.ReadLine() |> ignore
        let firstVar = Expr.scopeSeq businessRule.Condition |> Seq.tryHead
        // do printfn "firstVar = %A" firstVar
        // do Console.ReadLine() |> ignore
        let firstRestriction: Option<VarName * (obj -> bool)> =
          firstVar
          |> Option.map (fun v ->
            predicatesByRestrictedVariable
            |> Map.tryFind v.varName
            |> Option.map (fun predicate -> v.varName, predicate))
          |> Option.flatten
        // do printfn "firstRestriction = %A" firstRestriction
        // do Console.ReadLine() |> ignore
        let results = Expr.eval firstRestriction schema Map.empty businessRule.Condition
        // do printfn "results = %A" results
        // do Console.ReadLine() |> ignore
        for (vars, result) in results do
          match result with
          | Value.ConstBool true ->
            for a in businessRule.Actions do
              let modifiedFieldsByRuleVariants: list<Map<FieldDescriptorId, EntitiesIdentifiers>> =
                execute schema vars a

              for modifiedFieldsByRule in modifiedFieldsByRuleVariants do
                let! { CurrentExecutedRules = executedRules' } = state.GetState()

                let allModifiedTargets =
                  seq {
                    if executedRules' |> Map.containsKey businessRuleId then
                      yield executedRules'.[businessRuleId]

                    for modifiedField in modifiedFieldsByRule do
                      yield modifiedField.Value
                  }
                  |> Seq.reduce EntitiesIdentifiers.merge
                // do printfn "executedRules'= %A" executedRules'
                // do Console.ReadLine() |> ignore
                // do printfn "adding %A %A" businessRuleId allModifiedTargets
                // do Console.ReadLine() |> ignore
                do! state.SetState(BusinessRuleExecutionState.addCurrentExecutedRule businessRuleId allModifiedTargets)

                let! _ = state.GetState()
                // do printfn "executedRules' after adding = %A" executedRules'
                // do Console.ReadLine() |> ignore
                for modifiedField in modifiedFieldsByRule |> List.ofSeq do
                  do!
                    state.SetState(
                      BusinessRuleExecutionState.addCurrentModifiedFields modifiedField.Key modifiedField.Value
                    )

                  let! _ = state.GetState()
                  return ()
          // do printfn "modifiedFields' = %A" modifiedFields'
          // do Console.ReadLine() |> ignore
          | _ -> return ()

      let! { CurrentExecutedRules = executedRules' } = state.GetState()
      let! { CurrentModifiedFields = modifiedFields' } = state.GetState()
      do! state.SetState(BusinessRuleExecutionState.trace executedRules' modifiedFields')
      // do printfn "final executedRules' = %A" executedRules'
      // do Console.ReadLine() |> ignore
      if BusinessRule.overlap executedRules executedRules' then
        let! { Trace = trace } = state.GetState()
        return! state.Throw(Loop trace)
      else if
        // do printfn "final modifiedFields' = %A" modifiedFields'
        // do Console.ReadLine() |> ignore
        modifiedFields' |> Map.isEmpty
      then
        return ()
      else
        do!
          state.SetState(
            BusinessRuleExecutionState.updaters.AllExecutedRules(BusinessRule.mergeExecutedRules executedRules')
          )

        return! executeRulesTransitively ()
    }
