module Oauth.Mocked

open System
open OAuth.Coroutine
open OAuth.Models
open Ballerina.Coroutines
open System.Net.Http
open System.Threading.Tasks

let random = new Random()
let httpClient = new HttpClient()

let init () : EvaluatedCoroutines<_, _, _> =
  { active =
      Map.empty.Add(Guid.NewGuid(), processToken (TimeSpan.FromMinutes(1.)))
    waiting = Map.empty
    waitingOrListening = Map.empty
    listening = Map.empty
    stopped = Set.empty }

let getSnapshot () : unit * OAuthContext<_, _, _> * Map<Guid, Unit> * unit =
  (),
  { GetToken =
      fun () ->
        do printfn "Getting Access Token..."
        do Console.ReadLine() |> ignore

        if (random.NextDouble() < 0.1) then
          Choice1Of2() |> Task.FromResult
        else
          Choice2Of2(
            {| AccessToken = DateTime.UtcNow |},
            {| RefreshToken = DateTime.UtcNow |}
          )
          |> Task.FromResult
    SaveAccessToken =
      fun accessToken ->
        do printfn "Saving Access Token: %A" accessToken
        do Console.ReadLine() |> ignore
        Task.FromResult()
    RefreshToken =
      fun refreshToken ->
        do printfn "Refreshings with Refresh Token: %A" refreshToken
        do Console.ReadLine() |> ignore

        if (random.NextDouble() < 0.1) then
          Choice1Of2() |> Task.FromResult
        else
          Choice2Of2(
            {| AccessToken = DateTime.UtcNow |},
            {| RefreshToken = DateTime.UtcNow |}
          )
          |> Task.FromResult
    GetExpiration = fun _ -> TimeSpan.FromSeconds 30. },
  Map.empty,
  ()

let oauthEventLoop () =
  Ballerina.Coroutines.Runner.runLoop
    init
    getSnapshot
    ((fun _ -> ()))
    (fun _ _ _ -> ())
    (fun () ->
      Console.Clear()
      printfn "Tick: %A" (DateTime.UtcNow))
    (fun _ -> ())
