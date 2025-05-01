module Oauth.MSGraph

open System
open OAuth.Coroutine
open OAuth.Models
open Ballerina.Coroutines
open OAuth.MSGraph.API

let init () : EvaluatedCoroutines<_, _, _> =
  { active = Map.empty.Add(Guid.NewGuid(), processToken (TimeSpan.FromMinutes(1.)))
    waiting = Map.empty
    waitingOrListening = Map.empty
    listening = Map.empty
    stopped = Set.empty }

let getSnapshot (tenant: Guid) (clientId: Guid) secret =
  fun () ->
    (),
    { GetToken =
        fun () ->
          do printfn "Getting Access Token..."
          do Console.ReadLine() |> ignore
          requestToken tenant clientId secret
      SaveAccessToken =
        fun accessToken ->
          task {
            do printfn "Getting Entra users: %A" accessToken
            let! getResult = getUsers 50 accessToken.AccessToken

            match getResult with
            | Choice1Of2 statusCode -> do printfn "API error: %A" statusCode
            | Choice2Of2 users -> do printfn "%A" (users |> Array.toList)

            do Console.ReadLine() |> ignore
          }
      RefreshToken =
        fun _ ->
          do printfn "Refreshings with Refresh Token: %A" ()
          do Console.ReadLine() |> ignore
          requestToken tenant clientId secret
      GetExpiration =
        //NOTE: this is for testing purposes. You can read the actual expiration from the token and refresh with half that frequency.
        fun _ -> TimeSpan.FromSeconds(30.) },
    Map.empty,
    ()

let msGraphEventLoop (tenant: Guid) (clientId: Guid) (secret: string) =
  Ballerina.Coroutines.Runner.runLoop
    init
    (getSnapshot tenant clientId secret)
    ((fun _ -> ()))
    (fun _ _ _ -> ())
    (fun () ->
      Console.Clear()
      printfn "Tick: %A" (DateTime.UtcNow))
    (fun _ -> ())
