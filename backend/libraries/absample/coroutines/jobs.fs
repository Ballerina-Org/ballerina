module absample.coroutines.jobs

open Ballerina.Coroutines
open Ballerina.Coroutines.Runner
open System
open context
open absample.models

let processAEvents
  (abId: System.Guid)
  : Coroutine<Unit, Unit, ABContext, absample.models.ABEvent> =
  co.Repeat(
    co.Any(
      [ co {
          let! a_e =
            co.On (function
              | absample.models.ABEvent.AEvent e when e.event.ABId = abId ->
                Some e
              | _ -> None)

          do! co.Wait(TimeSpan.FromSeconds 0.0)

          do!
            co.Do(fun ctx ->
              ctx.ABs.update abId (fun ab ->
                ({ ab with
                    ACount = ab.ACount + a_e.AStep })))

          do! co.Wait(TimeSpan.FromSeconds 0.0)
        }
        co {
          do! co.Wait(TimeSpan.FromSeconds 3.0)

          do!
            co.Do(fun ctx ->
              ctx.ABs.update abId (fun ab ->
                ({ ab with
                    AFailCount = ab.AFailCount + 1 })))

          do! co.Wait(TimeSpan.FromSeconds 0.0)
        } ]
    )
  )

let processBEvents
  (abId: Guid)
  : Coroutine<Unit, Unit, ABContext, absample.models.ABEvent> =
  co.Repeat(
    co.Any(
      [ co {
          let! b_e =
            co.On (function
              | absample.models.ABEvent.BEvent e when e.event.ABId = abId ->
                Some e
              | _ -> None)

          do! co.Wait(TimeSpan.FromSeconds 0.0)

          do!
            co.Do(fun ctx ->
              ctx.ABs.update abId (fun ab ->
                ({ ab with
                    BCount = ab.BCount + b_e.BStep })))

          do! co.Wait(TimeSpan.FromSeconds 0.0)
        }
        co {
          do! co.Wait(TimeSpan.FromSeconds 5.0)

          do!
            co.Produce(
              Guid.CreateVersion7(),
              absample.models.ABEvent.AEvent
                { event =
                    { ABEventId = Guid.Empty
                      ABId = abId
                      AB = Unchecked.defaultof<AB>
                      CreatedAt = DateTime.UtcNow
                      ProcessingStatus = ABEventStatus.Enqueued }
                  AStep = 1 }
            )

          do! co.Wait(TimeSpan.FromSeconds 0.0)

          do!
            co.Do(fun ctx ->
              ctx.ABs.update abId (fun ab ->
                ({ ab with
                    BFailCount = ab.BFailCount + 1 })))

          do! co.Wait(TimeSpan.FromSeconds 0.0)
        } ]
    )
  )

let processAB abId =
  co.Any([ processAEvents abId; processBEvents abId ])
