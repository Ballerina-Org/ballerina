namespace Ballerina.Coroutines

module Model =
  open Ballerina.Fun
  open Ballerina.Collections.Sum
  open System
  open System.Threading.Tasks

  type DeltaT = TimeSpan

  type Coroutine<'a, 's, 'c, 'event, 'err> =
    | Co of
      ('s * 'c * Map<Guid, 'event> * DeltaT
        -> Sum<CoroutineResult<'a, 's, 'c, 'event, 'err> * Option<U<'s>> * Option<U<Map<Guid, 'event>>>, 'err>)

    static member map<'a, 'b, 's, 'c, 'event, 'err>
      (f: ('a -> 'b))
      ((Co p): Coroutine<'a, 's, 'c, 'event, 'err>)
      : Coroutine<'b, 's, 'c, 'event, 'err> =
      Co(fun (s, c, e, dt) ->
        p (s, c, e, dt)
        |> Sum.map (fun (p_result, s_updater, e_updater) -> (CoroutineResult.map f p_result, s_updater, e_updater)))

  and CoroutineResult<'a, 's, 'c, 'event, 'err> =
    | Return of 'a
    | Any of List<Coroutine<'a, 's, 'c, 'event, 'err>>
    // | All of List<Coroutine<'a, 's, 'c, 'event, 'err>>
    | Spawn of Coroutine<Unit, 's, 'c, 'event, 'err>
    | Wait of TimeSpan * Coroutine<'a, 's, 'c, 'event, 'err>
    | On of ('event -> Option<'a>)
    | Do of ('c -> 'a)
    | Await of Task<'a>
    // | Awaiting of Guid * Async<'a> * Task<'a>
    | MapContext of (Updater<'c> * Coroutine<'a, 's, 'c, 'event, 'err>)
    | Then of (Coroutine<Coroutine<'a, 's, 'c, 'event, 'err>, 's, 'c, 'event, 'err>)
    | Combine of (Coroutine<Unit, 's, 'c, 'event, 'err> * Coroutine<'a, 's, 'c, 'event, 'err>)
    | Repeat of Coroutine<'a, 's, 'c, 'event, 'err>

    static member map<'a, 'b, 's, 'c, 'event, 'err>
      (f: ('a -> 'b))
      (p: CoroutineResult<'a, 's, 'c, 'event, 'err>)
      : CoroutineResult<'b, 's, 'c, 'event, 'err> =
      match p with
      | Return x -> x |> f |> Return
      | Any ps -> ps |> List.map (Coroutine.map f) |> Any
      // | All ps -> ps |> List.map(Coroutine.map f) |> All
      | Spawn p -> p |> Spawn
      | Do(g) -> Do(g >> f)
      | Wait(t, p) -> Wait(t, p |> Coroutine.map f)
      | On(e_predicate) -> On(fun e -> e |> e_predicate |> Option.map f)
      // | Awaiting (id,p,t) ->
      //     Awaiting(
      //       id,
      //       async{
      //         let! x = p
      //         return f x
      //       },
      //       t.ContinueWith(new Func<_,_>(fun (a:Task<'a>) -> f(a.Result)))
      //     )
      | Await(p) ->
        Await(
          task {
            let! x = p
            return f x
          }
        )
      | Then(p_p) -> p_p |> Coroutine.map (Coroutine.map f) |> CoroutineResult.Then
      // Then(p', k >> (Coroutine.map f))
      | Combine(p, k) -> Combine(p, k |> Coroutine.map f)
      | Repeat(p) -> Repeat(p |> Coroutine.map f)
      | MapContext(u_c, p) -> MapContext(u_c, p |> Coroutine.map f)

  type Coroutine<'a, 's, 'c, 'event, 'err> with
    static member bind(p: Coroutine<'a, 's, 'c, 'event, 'err>, k: 'a -> Coroutine<'b, 's, 'c, 'event, 'err>) =
      Co(fun _ -> Left(Then(p |> Coroutine.map k), None, None))

  type CoroutineBuilder() =
    member _.Zero() =
      Co(fun _ -> Left(CoroutineResult.Return(()), None, None))

    member _.Throw(err: 'err) = Co(fun _ -> Right(err))

    member _.Return(result: 'a) =
      Co(fun _ -> Left(CoroutineResult.Return(result), None, None))

    member _.Yield(result: 'a) =
      Co(fun _ -> Left(CoroutineResult.Return(result), None, None))

    member co.Yield() =
      Co(fun _ -> Left(CoroutineResult.Wait(TimeSpan.FromMilliseconds 0., co.Return()), None, None))

    member _.Bind(p: Coroutine<'a, 's, 'c, 'event, 'err>, k: 'a -> Coroutine<'b, 's, 'c, 'event, 'err>) =
      Coroutine.bind (p, k)

    member _.Combine(p: Coroutine<Unit, 's, 'c, 'event, 'err>, k: Coroutine<'a, 's, 'c, 'event, 'err>) =
      Co(fun _ -> Left(CoroutineResult.Combine(p, k), None, None))

    member _.Any(ps: List<Coroutine<'a, 's, 'c, 'event, 'err>>) =
      Co(fun _ -> Left(CoroutineResult.Any(ps), None, None))
    // member _.All(ps:List<Coroutine<'a, 's, 'c, 'event, 'err>>) =
    //   Co(fun _ -> CoroutineResult.Any(ps), None, None)
    member co.YieldAfter(p: Coroutine<_, _, _, _, _>) =
      // co.Bind(p, fun x -> co.Bind(co.Wait(TimeSpan.FromSeconds 0.0), fun _ -> co.Return(x)))
      p

    member co.On(p_e: 'e -> Option<'a>) =
      co.YieldAfter(Co(fun _ -> Left(CoroutineResult.On(p_e), None, None)))

    member co.For(seq, body) =
      let seq: seq<Coroutine<Unit, _, _, _, _>> = seq |> Seq.map body
      seq |> Seq.fold (fun acc p -> co.Combine(acc, p)) (co.Return())
    // Co(fun _ -> CoroutineResult.For(seq, body), None, None)
    member co.Wait(t) =
      Co(fun _ -> Left(CoroutineResult.Wait(t, co.Return()), None, None))

    member co.Do(f: 's -> 'a) =
      co.YieldAfter(Co(fun _ -> Left(CoroutineResult.Do(f), None, None)))

    member co.Await(f: 'c -> Task<'a>) : Coroutine<'a, 's, 'c, 'event, 'err> =
      co {
        let! task = co.Do f
        return! co.Await task
      }

    member co.Await(p: Task<'a>) =
      co.YieldAfter(Co(fun _ -> Left(CoroutineResult.Await(p), None, None)))

    member _.Spawn(p: Coroutine<Unit, 's, 'c, 'event, 'err>) =
      Co(fun _ -> Left(CoroutineResult.Spawn(p), None, None))

    member _.Repeat(p: Coroutine<'a, 's, 'c, 'event, 'err>) : Coroutine<'a, 's, 'c, 'event, 'err> =
      Co(fun _ -> Left(CoroutineResult.Repeat(p), None, None))

    member _.mapContext (f) p =
      Co(fun _ -> Left(CoroutineResult.MapContext(f, p), None, None))

    member _.GetContext() =
      Co(fun (_, c, _, _) -> Left(CoroutineResult.Return(c), None, None))

    member _.GetState() =
      Co(fun (s, _, _, _) -> Left(CoroutineResult.Return(s), None, None))

    member _.SetState(u: U<'s>) =
      Co(fun (_, _, _, _) -> Left(CoroutineResult.Return(), Some u, None))

    member co.Produce(new_event) =
      co.YieldAfter(
        Co(fun _ -> Left(CoroutineResult.Return(), None, Some(fun es -> let (id, e) = new_event in es |> Map.add id e)))
      )

    member co.ofSum(p: Sum<'a, 'err>) =
      co {
        match p with
        | Sum.Left res -> return res
        | Sum.Right err -> return! co.Throw err
      }

    member co.ReturnFrom(p: Coroutine<'a, 's, 'c, 'event, 'err>) =
      co {
        let! res = p
        return res
      }

  let co = CoroutineBuilder()

  type WaitingCoroutine<'a, 's, 'c, 'event, 'err> =
    { P: Coroutine<'a, 's, 'c, 'event, 'err>
      Until: DateTime }

  type EvaluatedCoroutine<'a, 's, 'c, 'event, 'err> =
    | Done of 'a * Option<U<'s>> * Option<U<Map<Guid, 'event>>>
    | Spawned of
      List<Coroutine<Unit, 's, 'c, 'event, 'err>> *
      Option<U<'s>> *
      Option<U<Map<Guid, 'event>>> *
      Option<Coroutine<'a, 's, 'c, 'event, 'err>>
    | Active of Coroutine<'a, 's, 'c, 'event, 'err> * Option<U<'s>> * Option<U<Map<Guid, 'event>>>
    | Listening of Coroutine<'a, 's, 'c, 'event, 'err> * Option<U<'s>> * Option<U<Map<Guid, 'event>>>
    | Waiting of WaitingCoroutine<'a, 's, 'c, 'event, 'err> * Option<U<'s>> * Option<U<Map<Guid, 'event>>>
    | WaitingOrListening of WaitingCoroutine<'a, 's, 'c, 'event, 'err> * Option<U<'s>> * Option<U<Map<Guid, 'event>>>
    | Error of 'err

    member this.After(u_s, u_e) =
      match this with
      | Done(x, u_s', u_e') -> Done(x, u_s >>? u_s', u_e >>? u_e')
      | Spawned(x, u_s', u_e', p) -> Spawned(x, u_s >>? u_s', u_e >>? u_e', p)
      | Active(x, u_s', u_e') -> Active(x, u_s >>? u_s', u_e >>? u_e')
      | Listening(x, u_s', u_e') -> Listening(x, u_s >>? u_s', u_e >>? u_e')
      | Waiting(x, u_s', u_e') -> Waiting(x, u_s >>? u_s', u_e >>? u_e')
      | WaitingOrListening(x, u_s', u_e') -> WaitingOrListening(x, u_s >>? u_s', u_e >>? u_e')
      | Error err -> Error err

  type EvaluatedCoroutines<'s, 'c, 'event, 'err> =
    { active: Map<Guid, Coroutine<Unit, 's, 'c, 'event, 'err>>
      stopped: Set<Guid>
      crashed: Map<Guid, 'err>
      waiting: Map<Guid, WaitingCoroutine<Unit, 's, 'c, 'event, 'err>>
      listening: Map<Guid, Coroutine<Unit, 's, 'c, 'event, 'err>>
      waitingOrListening: Map<Guid, WaitingCoroutine<Unit, 's, 'c, 'event, 'err>> }

  type Coroutine<'a, 's, 'c, 'event, 'err> with
    static member eval<'a>
      ((Co p): Coroutine<'a, 's, 'c, 'event, 'err>)
      (ctx: 's * 'c * Map<Guid, 'event> * DeltaT)
      : EvaluatedCoroutine<'a, 's, 'c, 'event, 'err> =
      let (s, c, es, dt) = ctx

      match p ctx with
      | Right err -> Error err
      | Left(step, u_s, u_e) ->

        match step with
        | MapContext(u_c, p') -> Coroutine.eval p' (s, c |> u_c, es, dt)
        | Then(p_p) ->
          match Coroutine.eval p_p ctx with
          | Error err -> Error err
          | Done(p, u_s, u_e) ->
            let res = Coroutine.eval p ctx
            res.After(u_s, u_e)
          | Spawned(p', u_s, u_e, rest: Option<Coroutine<Coroutine<'a, 's, 'c, 'event, 'err>, 's, 'c, 'event, 'err>>) ->
            Spawned(
              p',
              u_s,
              u_e,
              rest
              |> Option.map (fun rest_p -> Co(fun _ -> Left(CoroutineResult.Then(rest_p), None, None)))
            )
          | Active(p_p', u_s, u_e) -> Active(Co(fun _ -> Left(CoroutineResult.Then(p_p'), None, None)), u_s, u_e)
          | Listening(p_p', u_s, u_e) -> Listening(Co(fun _ -> Left(CoroutineResult.Then(p_p'), None, None)), u_s, u_e)
          | Waiting(w, u_s, u_e) ->
            Waiting(
              { P = Co(fun _ -> Left(CoroutineResult.Then(w.P), None, None))
                Until = w.Until },
              u_s,
              u_e
            )
          | WaitingOrListening(w, u_s, u_e) ->
            Waiting(
              { P = Co(fun _ -> Left(CoroutineResult.Then(w.P), None, None))
                Until = w.Until },
              u_s,
              u_e
            )
        | Combine(p, k) ->
          match Coroutine.eval p ctx with
          | Error err -> Error err
          | Done(_, u_s, u_e) ->
            let res = Coroutine.eval k ctx
            res.After(u_s, u_e)
          | Spawned(p', u_s, u_e, rest) ->
            Spawned(
              p',
              u_s,
              u_e,
              rest
              |> Option.map (fun p -> Coroutine.bind (p, fun () -> k))
              |> Option.orElse (Some k)
            )
          | Active(p', u_s, u_e) -> Active(co.Combine(p', k), u_s, u_e)
          | Listening(p', u_s, u_e) -> Listening(co.Combine(p', k), u_s, u_e)
          | Waiting(w, u_s, u_e) ->
            Waiting(
              { P = co.Combine(w.P, k)
                Until = w.Until },
              u_s,
              u_e
            )
          | WaitingOrListening(w, u_s, u_e) ->
            Waiting(
              { P = co.Combine(w.P, k)
                Until = w.Until },
              u_s,
              u_e
            )
        | Return res -> Done(res, u_s, u_e)
        | Any(ps) ->
          let res =
            ps
            |> List.fold
              (fun res p ->
                match res with
                | Choice3Of3 _ -> res
                | Choice2Of3 _ -> res
                | Choice1Of3(ps', spawned', u_s, u_e) ->
                  match Coroutine.eval p ctx with
                  | Error err -> Choice3Of3 err
                  | Done(res, u_s', u_e') -> Choice2Of3(res, u_s >>? u_s', u_e >>? u_e')
                  | Spawned(p', u_s', u_e', rest) ->
                    Choice1Of3(ps' @ Option.toList rest, p' @ spawned', u_s >>? u_s', u_e >>? u_e')
                  | Active(p', u_s', u_e')
                  | Listening(p', u_s', u_e') -> Choice1Of3(p' :: ps', spawned', u_s >>? u_s', u_e >>? u_e')
                  | Waiting({ P = p'; Until = until }, u_s', u_e')
                  | WaitingOrListening({ P = p'; Until = until }, u_s', u_e') ->
                    Choice1Of3(
                      Co(fun _ -> Left(CoroutineResult.Wait(until - DateTime.Now, p'), None, None))
                      :: ps',
                      spawned',
                      u_s >>? u_s',
                      u_e >>? u_e'
                    ))
              (Choice1Of3([], [], None, None))

          match res with
          | Choice1Of3(ps', [], u_s, u_e) -> Active(co.Any ps', u_s, u_e)
          | Choice1Of3(ps', spawned, u_s, u_e) -> Spawned(spawned, u_s, u_e, Some(co.Any ps'))
          | Choice2Of3(res, u_s, u_e) -> Done(res, u_s, u_e)
          | Choice3Of3 err -> Error err
        | Wait(timeSpan, p': Coroutine<'a, 's, 'c, 'event, 'err>) ->
          if timeSpan.TotalSeconds <= 0. then
            Active(p', None, None)
          else
            Waiting(
              { P = p'
                Until = DateTime.Now + timeSpan - dt },
              None,
              None
            )
        // let timeSpan' = timeSpan - dt
        // if timeSpan'.TotalMilliseconds <= 0 then
        //   Active(p', None, None)
        // else
        //   Active(co{
        //     do! co.Wait timeSpan'
        //     return! p'
        //   }, None, None)
        | On(p_e) ->
          match
            es
            |> Seq.map (fun e -> p_e e.Value, e)
            |> Seq.tryFind (function
              | Some _, _ -> true
              | _ -> false)
          with
          | Some(Some res, e) -> Done(res, None, Some(Map.remove e.Key))
          | _ -> Active(co.On p_e, None, None)
        | Spawn(p) -> Spawned([ p ], None, None, None)
        | Do(f) -> Done(f c, None, None)
        | Await(a: Task<'a>) ->
          let result = a |> Async.AwaitTask |> Async.RunSynchronously
          Done(result, None, None)
        // let id = Guid.CreateVersion7()
        // let task = a |> Async.StartAsTask
        // Active(co.Awaiting(id, a, task), None, None)
        // | Awaiting(id, a, task) ->
        //   do printfn "%A" task.Status
        //   if task.IsCompletedSuccessfully then
        //     Done(task.Result, None, None)
        //   else
        //     Active(co.Awaiting(id, a, task), None, None)
        | Repeat(p) -> Coroutine.eval (co.Bind(p, fun _ -> co.Repeat p)) ctx

    static member evalMany<'s, 'c, 'event, 'err>
      (ps: Map<Guid, Coroutine<Unit, 's, 'c, 'event, 'err>>)
      ((s, c, es, dt): 's * 'c * Map<Guid, 'event> * DeltaT)
      : EvaluatedCoroutines<'s, 'c, 'event, 'err> * Option<U<'s>> * Option<U<Map<Guid, 'event>>> =
      let ctx = (s, c, es, dt)
      let mutable u_s: Option<U<'s>> = None
      let mutable u_e: Option<U<Map<Guid, 'event>>> = None

      let mutable evaluated: EvaluatedCoroutines<'s, 'c, 'event, 'err> =
        { active = Map.empty
          stopped = Set.empty
          crashed = Map.empty
          waiting = Map.empty
          listening = Map.empty
          waitingOrListening = Map.empty }

      for p in ps do
        match Coroutine.eval p.Value ctx with
        | Error err ->
          evaluated <-
            { evaluated with
                crashed = evaluated.crashed |> Map.add p.Key err }
        | Done(_, u_s', u_e') ->
          evaluated <-
            { evaluated with
                stopped = evaluated.stopped.Add p.Key }

          u_s <- u_s >>? u_s'
          u_e <- u_e >>? u_e'
        | Spawned(spawned, u_s', u_e', rest) ->
          match rest with
          | Some p' ->
            evaluated <-
              { evaluated with
                  active = evaluated.active.Add(p.Key, p') }

            u_s <- u_s >>? u_s'
            u_e <- u_e >>? u_e'
          | _ -> ()

          for p' in spawned do
            evaluated <-
              { evaluated with
                  active = evaluated.active.Add(Guid.CreateVersion7(), p') }

            u_s <- u_s >>? u_s'
            u_e <- u_e >>? u_e'
        | Active(p', u_s', u_e') ->
          evaluated <-
            { evaluated with
                active = evaluated.active.Add(p.Key, p') }

          u_s <- u_s >>? u_s'
          u_e <- u_e >>? u_e'
        | Listening(p', u_s', u_e') ->
          evaluated <-
            { evaluated with
                listening = evaluated.listening.Add(p.Key, p') }

          u_s <- u_s >>? u_s'
          u_e <- u_e >>? u_e'
        | Waiting(p', u_s', u_e') ->
          evaluated <-
            { evaluated with
                waiting = evaluated.waiting.Add(p.Key, p') }

          u_s <- u_s >>? u_s'
          u_e <- u_e >>? u_e'
        | WaitingOrListening(p', u_s', u_e') ->
          evaluated <-
            { evaluated with
                waitingOrListening = evaluated.waitingOrListening.Add(p.Key, p') }

          u_s <- u_s >>? u_s'
          u_e <- u_e >>? u_e'

      evaluated, u_s, u_e

    static member evalSynchronously<'a>
      (onSpawnedError: unit -> 'err)
      ((s, c, events, dt): 's * 'c * Map<Guid, 'event> * DeltaT)
      ((Co p): Coroutine<'a, 's, 'c, 'event, 'err>)
      : Sum<'a * 's * Map<Guid, 'event>, 'err> =
      let step = Coroutine.eval (Co p) (s, c, events, dt)

      match step with
      | EvaluatedCoroutine.Done(result, u_s, u_e) ->
        let s = s |> (u_s |> Option.defaultValue id)
        let events = events |> (u_e |> Option.defaultValue id)
        Sum.Left(result, s, events)
      | EvaluatedCoroutine.Error err -> Sum.Right err
      | EvaluatedCoroutine.Spawned _ -> Sum.Right(onSpawnedError ())
      | EvaluatedCoroutine.Active(p, u_s, u_e)
      | EvaluatedCoroutine.Listening(p, u_s, u_e) ->
        let s = s |> (u_s |> Option.defaultValue id)
        let events = events |> (u_e |> Option.defaultValue id)
        Coroutine.evalSynchronously onSpawnedError (s, c, events, dt) p
      | EvaluatedCoroutine.Waiting(p, u_s, u_e)
      | EvaluatedCoroutine.WaitingOrListening(p, u_s, u_e) ->
        let now = System.DateTime.Now
        do Task.Delay(p.Until - now).Wait()
        let s = s |> (u_s |> Option.defaultValue id)
        let events = events |> (u_e |> Option.defaultValue id)
        Coroutine.evalSynchronously onSpawnedError (s, c, events, dt) p.P
