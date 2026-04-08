module OAuth.Coroutine

open Ballerina.Coroutines
open Ballerina.Coroutines.Runner
open OAuth.Models
open System
open System.Threading.Tasks

let rec private refresh
  (refreshToken: 'refreshToken)
  : Coroutine<_, _, OAuthContext<'accessToken, 'refreshToken, 'error>, 'error> =
  co {
    match! co.Await(fun ctxt -> ctxt.RefreshToken refreshToken) with
    | Choice1Of2 error ->
      do! co.Wait(TimeSpan.FromSeconds(0.))
      do! co.Produce(Guid.NewGuid(), error)
    | Choice2Of2(accessToken, refreshToken) ->
      do! co.Wait(TimeSpan.FromSeconds(0.))
      do! co.Await(fun ctxt -> ctxt.SaveAccessToken accessToken)
      let! waitTime = co.Do(fun ctxt -> ctxt.GetExpiration accessToken)
      do! co.Wait(waitTime * 0.5)
      do! refresh refreshToken
  }

let processToken
  (retryGetTokenInterval: TimeSpan)
  : Coroutine<
      unit,
      unit,
      OAuthContext<'refreshToken, 'accessToken, 'error>,
      'error
     >
  =
  co.Repeat(
    co {
      match! co.Await(fun ctxt -> ctxt.GetToken()) with
      | Choice1Of2 error ->
        do! co.Wait(TimeSpan.FromSeconds(0.))
        do! co.Wait retryGetTokenInterval
        do! co.Produce(Guid.NewGuid(), error)
      | Choice2Of2(accessToken, refreshToken) ->
        do! co.Wait(TimeSpan.FromSeconds(0.))
        do! co.Await(fun ctxt -> ctxt.SaveAccessToken accessToken)
        do! refresh refreshToken
    }
  )
