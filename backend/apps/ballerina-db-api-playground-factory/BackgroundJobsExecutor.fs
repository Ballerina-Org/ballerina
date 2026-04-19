namespace ballerina_db_api_playground

open System
open System.Threading
open System.Threading.Tasks
open Ballerina
open Ballerina.API
open Ballerina.API.MemoryDB.Model
open Ballerina.Cat.Collections.OrderedMap
open Ballerina.DSL.Next.StdLib
open Ballerina.DSL.Next.StdLib.FileDB
open Ballerina.DSL.Next.StdLib.MutableMemoryDB
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.Types
open Ballerina.Reader.WithError
open Microsoft.Extensions.Hosting
open Ballerina.Collections.Sum
open Ballerina.Errors
open Ballerina.LocalizedErrors
open ballerinalang.Runners.BackgroundJob
open Ballerina.Collections.Option

type MemoryDBBackgroundJobExecutor
  (
    descriptorFetcher:
      Guid
        -> string
        -> Sum<
          DbDescriptor<
            FileDBRuntimeContext,
            MutableMemoryDB.MutableMemoryDB<FileDBRuntimeContext, _>,
            unit
           >,
          Errors<Location>
         >,
    injectBackgroundContext: Updater<ExprEvalContext<FileDBRuntimeContext, _>>,
    dbFileConfig: DbFileConfig,
    timeProvider: unit -> DateTimeOffset,
    stream: SchemaId IObservable
  ) =
  inherit BackgroundService()

  let mutable schemas = Set.empty
  let cleanup = stream.Subscribe(fun schema -> schemas <- schemas.Add schema)

  interface IDisposable with
    member _.Dispose() = cleanup.Dispose()

  override this.ExecuteAsync(stoppingToken: CancellationToken) : Task =
    task {
      use timer = new PeriodicTimer(TimeSpan.FromSeconds 2L)

      let updateBackgroundJobs = updateBackgroundJobs dbFileConfig

      try
        let mutable keepRunning = true

        while keepRunning do
          let! hasNextTick = timer.WaitForNextTickAsync(stoppingToken)

          if hasNextTick then
            // This should stop the execution flow of the app; however, for test memory db we do not care
            try
              for schema in schemas do
                sum {
                  let! descriptor =
                    descriptorFetcher
                      schema.TenantId
                      schema.SchemaName

                  descriptor.DbExtension.DB
                  |> updateFromFileSystem dbFileConfig
                  |> Reader.Run descriptor.EvalContext

                  let now = timeProvider ()
                  let dbio = descriptor.DbExtension

                  dbio.DB.backgroundJobs
                  |> Seq.filter (fun (KeyValue(_, job)) ->
                    (not job.HasStarted)
                    && job.ScheduledAt
                       |> Option.map (fun scheduledAt -> scheduledAt <= now)
                       |> Option.defaultValue false)
                  |> Seq.iter (fun (KeyValue(entity, job)) ->
                    option {
                      let! entityDef =
                        dbio.Schema.Entities
                        |> OrderedMap.tryFind entity.EntityName

                      let! backgroundJob = entityDef.Hooks.OnBackground

                      let! values =
                        dbio.DB.entities |> Map.tryFind entity.EntityName

                      let! value = values |> Map.tryFind entity.EntityId

                      Console.WriteLine $"Executing job for {entity}"

                      updateBackgroundJobs
                      <| Map.add entity { job with HasStarted = true }
                      |> Reader.Run descriptor.EvalContext

                      let result =
                        executeBackgroundJob
                          descriptor.DbExtension
                          descriptor.EvalContext.ExtensionOps
                          injectBackgroundContext
                          descriptor.EvalContext.RuntimeContext
                          backgroundJob
                          value
                          entity.EntityId

                      match result with
                      | Left delayBeforeNextExecution ->
                        Console.WriteLine "Successfully executed"

                        match delayBeforeNextExecution with
                        | Some delayBeforeNextExecution ->
                          let nextExecution =
                            (timeProvider ()).Add delayBeforeNextExecution

                          updateBackgroundJobs
                          <| Map.add
                            entity
                            (MemoryDBBackgroundJob.ScheduleAt nextExecution)
                          |> Reader.Run descriptor.EvalContext

                          Console.WriteLine
                            $"Next execution scheduled at {nextExecution}"
                        | None ->
                          updateBackgroundJobs
                          <| Map.add entity MemoryDBBackgroundJob.Finished
                          |> Reader.Run descriptor.EvalContext

                          Console.WriteLine "Job is finished"
                      | Right errors ->
                        updateBackgroundJobs
                        <| Map.add entity MemoryDBBackgroundJob.Finished
                        |> Reader.Run descriptor.EvalContext

                        Console.WriteLine $"Failed {errors}"

                      return ()
                    }
                    |> ignore)

                  return ()
                }
                |> ignore
            with _ ->
              ()
          else
            keepRunning <- false
      with :? OperationCanceledException ->
        ()
    }
