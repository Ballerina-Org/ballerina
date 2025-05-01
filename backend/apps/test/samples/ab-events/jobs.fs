module absample.eventLoop

open System
open System.Linq
open Microsoft.Extensions.DependencyInjection
open Ballerina.Coroutines
open Migrations
open Microsoft.EntityFrameworkCore
open absample.efmodels
open absample.repositories
open absample.coroutines.context
open absample.coroutines.jobs

let abEventLoop (createScope: Unit -> IServiceScope) =

  let init () : EvaluatedCoroutines<_, _, _> =
    use scope = createScope ()
    use db = scope.ServiceProvider.GetService<BallerinaContext>()
    db.ABEvents.RemoveRange(db.ABEvents)
    db.ABs.RemoveRange(db.ABs)

    db.ABs.Add(
      { ABId = Guid.NewGuid()
        ACount = 0
        BCount = 0
        AFailCount = 0
        BFailCount = 0 }
    )
    |> ignore

    db.ABs.Add(
      { ABId = Guid.NewGuid()
        ACount = 0
        BCount = 0
        AFailCount = 0
        BFailCount = 0 }
    )
    |> ignore

    db.SaveChanges() |> ignore
    db.ChangeTracker.Clear()

    { active =
        db.ABs.AsNoTracking()
        |> Seq.toArray
        |> Seq.map (fun e -> e.ABId)
        |> Seq.map (fun abId -> (abId, processAB abId))
        |> Map.ofSeq
      waiting = Map.empty
      waitingOrListening = Map.empty
      listening = Map.empty
      stopped = Set.empty }

  let getSnapshot () =
    let scope = createScope ()
    let db = scope.ServiceProvider.GetService<BallerinaContext>()

    ((),
     { ABs = abrepos.logical.AB db (db.ABs)
       ABEvents = abrepos.logical.ABEvent db (db.ABEvents) },
     db.ABEvents
       .AsNoTracking()
       .Where(fun e -> e.ProcessingStatus = ABEventStatus.Enqueued)
       .OrderBy(fun e -> (e.CreatedAt, e.ABEventId))
       .ToArray()
     |> Seq.map (fun e -> e.ABEventId, e |> absample.efmodels.ABEvent.ToUnion)
     |> Map.ofSeq,
     (db, scope))

  let updateEvents (dataSource: BallerinaContext * IServiceScope) events u_e =
    let db = dataSource |> fst
    let events' = u_e events

    let added =
      events'
      |> Map.filter (fun e'_id _ -> events |> Map.containsKey e'_id |> not)
      |> Map.toSeq
      |> Seq.map snd
      |> Seq.map (absample.efmodels.ABEvent.FromUnion)

    let removed =
      events
      |> Map.filter (fun e_id _ -> events' |> Map.containsKey e_id |> not)
      |> Map.toSeq
      |> Seq.map snd
      |> Seq.map (absample.efmodels.ABEvent.FromUnion)
      |> Seq.map (
        absample.efmodels.ABEvent.WithRecord(fun e ->
          { e with
              ProcessingStatus = ABEventStatus.Processed })
      )

    db.ABEvents.UpdateRange(removed) |> ignore
    db.ABEvents.AddRange(added) |> ignore
    db.SaveChanges() |> ignore

  let log (dataSource: BallerinaContext * IServiceScope) =
    Console.Clear()
    let db = dataSource |> fst

    printfn
      "ABs=%A ABEvents=%A"
      (db.ABs.OrderBy(fun e -> e.ABId).ToArray())
      (db.ABEvents.Where(fun e -> e.ProcessingStatus = ABEventStatus.Enqueued).OrderBy(fun e -> e.ABEventId).ToArray())

    ()

  let releaseSnapshot ((db, scope): BallerinaContext * IServiceScope) =
    db.Dispose()
    scope.Dispose()

  Ballerina.Coroutines.Runner.runLoop init getSnapshot (fun _ -> ()) updateEvents log releaseSnapshot
