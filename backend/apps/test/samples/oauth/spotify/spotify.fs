module Oauth.Spotify

open System
open OAuth.Coroutine
open OAuth.Models
open Ballerina.Coroutines
open OAuth.Spotify.API

let init () : EvaluatedCoroutines<_, _, _> =
  { active = Map.empty.Add(Guid.NewGuid(), processToken (TimeSpan.FromMinutes(1.)))
    waiting = Map.empty
    waitingOrListening = Map.empty
    listening = Map.empty
    stopped = Set.empty }

let getSnapshot (clientId: string) (clientSecret: string) (authorizationCode: string) =
  fun () ->
    (),
    { GetToken =
        fun () ->
          do printfn "Getting Access Token..."
          do Console.ReadLine() |> ignore
          requestToken clientId clientSecret authorizationCode
      SaveAccessToken =
        fun accessToken ->
          task {
            do printfn "Getting Latest Albums: %A" accessToken
            let! getResult = getNewReleases 0 10 (accessToken.AccessToken)

            match getResult with
            | Choice1Of2 statusCode -> do printfn "API error: %A" statusCode
            | Choice2Of2 newReleases -> do printfn "%A" newReleases

            do Console.ReadLine() |> ignore
          }
      RefreshToken =
        fun token ->
          do printfn "Refreshings with Refresh Token: %A" token
          do Console.ReadLine() |> ignore
          refreshToken clientId clientSecret token
      GetExpiration =
        //NOTE: this is for testing purposes. You can read the actual expiration from the token and refresh with half that frequency.
        fun _ -> TimeSpan.FromSeconds(30.) },
    Map.empty,
    ()

let spotifyEventLoop (clientId: string) (clientSecret: string) (authorizationCode: string) =
  Ballerina.Coroutines.Runner.runLoop
    init
    (getSnapshot clientId clientSecret authorizationCode)
    ((fun _ -> ()))
    (fun _ _ _ -> ())
    (fun () ->
      Console.Clear()
      printfn "Tick: %A" (DateTime.UtcNow))
    (fun _ -> ())
