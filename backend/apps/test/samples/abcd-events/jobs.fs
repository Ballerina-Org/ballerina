module abcdsample.eventLoop

open System
open System.Linq
open positions.model
open Ballerina.Fun
open Ballerina.Collections.Map
open Ballerina.Collections.Sum
open Ballerina.Coroutines
open Ballerina.DSL.Model
open Ballerina.DSL.Execute
open context
open Ballerina.DSL.TransitiveExecution
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Expr.Eval
open Ballerina.DSL.Preprocessor
open Ballerina.DSL.Predicate
open Microsoft.FSharp.Quotations

let abcdEventLoop () =
  let mutable context: Context = init_abcdContext ()
  let co = CoroutineBuilder()

  let processABCD: Coroutine<Unit, JobsState, Context, ABCDEvent> =
    co.Repeat(
      co {
        let! businessRule =
          co.On (function
            | ABCDEvent.Edit br -> Option.Some br
          // | _ ->
          //   Option.None
          )

        do! co.Wait(TimeSpan.FromSeconds 0.0)

        let! modifiedFields =
          co.Do(fun ctx ->
            let schema = ctx.Schema
            let results = Expr.eval None schema Map.empty businessRule.Condition

            let allModifiedFields =
              seq {
                for (vars, result) in results do
                  match result with
                  | Value.ConstBool true ->
                    for a in businessRule.Actions do
                      yield! execute schema vars a
                  | _ -> ()
              }

            allModifiedFields |> Seq.fold (Map.merge EntitiesIdentifiers.merge) Map.empty
          // execute ctx.Schema vars e.Self.Assignment
          )

        do printfn "modifiedFields %A" modifiedFields
        do Console.ReadLine() |> ignore

        do!
          co.Do(fun ctx ->
            let businessRulesExecutionContext =
              { AllRules = ctx.BusinessRules
                Schema = ctx.Schema }

            let businessRulesExecutionState =
              { AllExecutedRules = Map.empty
                CurrentExecutedRules = Map.empty
                CurrentModifiedFields = modifiedFields
                Trace = [] }

            match executeRulesTransitively().run (businessRulesExecutionContext, businessRulesExecutionState) with
            | Left _ ->
              do printfn "Transitive execution completed successfully"
              do Console.ReadLine() |> ignore
            | Right e ->
              do printfn "Error %A, rule execution resulted in a possible loop that was interrupted" e
              do Console.ReadLine() |> ignore)
      }
    )

  let init () : EvaluatedCoroutines<_, _, _> =
    { active = [ Guid.CreateVersion7(), processABCD ] |> Map.ofSeq
      waiting = Map.empty
      waitingOrListening = Map.empty
      listening = Map.empty
      stopped = Set.empty }

  let getSnapshot () =
    // state, context, active events, () [= Db x Scope]
    { edits = Set.empty },
    context,
    context.ActiveEvents
    |> Seq.map (function
      | ABCDEvent.Edit br as e -> br.BusinessRuleId, e)
    |> Map.ofSeq,
    ()

  let updateEvents (_: Unit) events u_e =
    let events' = u_e events

    let added =
      events'
      |> Map.filter (fun e'_id _ -> events |> Map.containsKey e'_id |> not)
      |> Map.values
      |> List.ofSeq

    let removed =
      events
      |> Map.filter (fun e_id _ -> events' |> Map.containsKey e_id |> not)
      |> Map.toSeq
      |> Seq.map snd
      |> Set.ofSeq

    context <-
      { context with
          ActiveEvents =
            (context.ActiveEvents |> List.filter (fun e -> removed |> Set.contains e |> not))
            @ added }

    ()

  let updateState u_s =
    let _ = u_s { edits = Set.empty }
    // run the whole process of business rules based on the edits, with loop avoidance
    ()

  let log (_: Unit) =
    Console.Clear() |> ignore

    for ab in context.ABs() |> Map.values do
      printf
        """{|
  Id = %s
  A1 = %d; B1 = %d; Total1 = %d;
    CD = {| Id = %s; C = %d; D = %d 
            EF = {| Id = %s; E = %d; F = %d |}
         |}; 
  А2 = %d; Б2 = %d; Total2 = %d;
  Α3 = %d; Β3 = %d; Σ3 = %d;
|}
"""
        (ab.ABId.ToString().Substring(0, 4))
        ab.A1
        ab.B1
        ab.Total1
        (((context.CDs())[ab.CDId]).CDId.ToString().Substring(0, 4))
        ((context.CDs())[ab.CDId]).C
        ((context.CDs())[ab.CDId]).D
        (((context.EFs())[((context.CDs())[ab.CDId]).EFId]).EFId.ToString().Substring(0, 4))
        ((context.EFs())[((context.CDs())[ab.CDId]).EFId]).E
        ((context.EFs())[((context.CDs())[ab.CDId]).EFId]).F
        ab.А2
        ab.Б2
        ab.Весь2
        ab.Α3
        ab.Β3
        ab.Σ3

  let releaseSnapshot (_: Unit) = ()
  Ballerina.Coroutines.Runner.runLoop init getSnapshot updateState updateEvents log releaseSnapshot

  ()
