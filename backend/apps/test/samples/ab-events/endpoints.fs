module absample.endpoints

open Ballerina
open System
open Microsoft.AspNetCore.Builder
open Microsoft.EntityFrameworkCore
open absample.efmodels
open absample.repositories
open Ballerina.CRUD

type WebApplication with
  member app.UseABSample<'DB>
    (
      repoAB: 'DB -> CrudSeq<AB>,
      repoABEvent: 'DB -> CrudSeq<ABEvent>,
      repoAEvent: 'DB -> CrudSeq<AEvent>,
      repoBEvent: 'DB -> CrudSeq<BEvent>
    ) =
    app
      .MapGet(
        "/ABs",
        new Func<_, _, _, _>(fun (db: 'DB) (skip: int) (take: int) ->
          (repoAB db).getN
            <@ fun _ -> true @>
            <@ fun e -> e.ABId @>
            (Ballerina.Range.WithinReason(skip, take)))
      )
      .WithOpenApi()
    |> ignore

    app
      .MapPost(
        "/ABs",
        new Func<_, _, _>(fun (db: 'DB) ([<FromBody>] msg: AB) ->
          let AB = (repoAB db)
          AB.create msg)
      )
      .WithOpenApi()
    |> ignore

    app
      .MapGet(
        "/ABEvents",
        new Func<_, _, _, _>(fun (db: 'DB) (skip: int) (take: int) ->
          let values =
            (repoABEvent db).getN
              <@ fun _ -> true @>
              <@ fun e -> e.ABEventId @>
              (Ballerina.Range.WithinReason(skip, take))

          values.Include(fun x -> x.AB))
      )
      .WithOpenApi()
    |> ignore

    app
      .MapGet(
        "/AEvents",
        new Func<_, _, _, _>(fun (db: 'DB) (skip: int) (take: int) ->
          (repoAEvent db).getN
            <@ fun _ -> true @>
            <@ fun e -> e.ABEventId @>
            (Ballerina.Range.WithinReason(skip, take)))
      )
      .WithOpenApi()
    |> ignore

    app
      .MapGet(
        "/BEvents",
        new Func<_, _, _, _>(fun (db: 'DB) (skip: int) (take: int) ->
          (repoBEvent db).getN
            <@ fun _ -> true @>
            <@ fun e -> e.ABEventId @>
            (Ballerina.Range.WithinReason(skip, take)))
      )
      .WithOpenApi()
    |> ignore

    app
      .MapPost(
        "/ABEvents",
        new Func<_, _, _>(fun (db: 'DB) ([<FromBody>] msg: ABEvent) ->
          let ABEvent = repoABEvent db
          msg.CreatedAt <- DateTime.UtcNow
          msg.ProcessingStatus <- ABEventStatus.Enqueued
          ABEvent.create msg)
      )
      .WithOpenApi()
    |> ignore

    app
