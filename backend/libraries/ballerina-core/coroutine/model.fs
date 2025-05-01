namespace Ballerina.Coroutines

open Ballerina.Fun
open System
open System.Threading.Tasks

type DeltaT = TimeSpan

type Coroutine<'a, 's, 'c, 'e> =
  | Co of
    ('s * 'c * Map<Guid, 'e> * DeltaT -> CoroutineResult<'a, 's, 'c, 'e> * Option<U<'s>> * Option<U<Map<Guid, 'e>>>)

  static member map<'a, 'b, 's, 'c, 'e>
    (f: ('a -> 'b))
    ((Co p): Coroutine<'a, 's, 'c, 'e>)
    : Coroutine<'b, 's, 'c, 'e> =
    Co(fun (s, c, e, dt) ->
      let (p_result, s_updater, e_updater) = p (s, c, e, dt)
      (CoroutineResult.map f p_result, s_updater, e_updater))

and CoroutineResult<'a, 's, 'c, 'e> =
  | Return of 'a
  | Any of List<Coroutine<'a, 's, 'c, 'e>>
  // | All of List<Coroutine<'a, 's, 'c, 'e>>
  | Spawn of Coroutine<Unit, 's, 'c, 'e>
  | Wait of TimeSpan * Coroutine<'a, 's, 'c, 'e>
  | On of ('e -> Option<'a>)
  | Do of ('c -> 'a)
  | Await of Task<'a>
  // | Awaiting of Guid * Async<'a> * Task<'a>
  | Then of (Coroutine<Coroutine<'a, 's, 'c, 'e>, 's, 'c, 'e>)
  | Combine of (Coroutine<Unit, 's, 'c, 'e> * Coroutine<'a, 's, 'c, 'e>)
  | Repeat of Coroutine<'a, 's, 'c, 'e>

  static member map<'a, 'b, 's, 'c, 'e>
    (f: ('a -> 'b))
    (p: CoroutineResult<'a, 's, 'c, 'e>)
    : CoroutineResult<'b, 's, 'c, 'e> =
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

type Coroutine<'a, 's, 'c, 'e> with
  static member bind(p: Coroutine<'a, 's, 'c, 'e>, k: 'a -> Coroutine<'b, 's, 'c, 'e>) =
    Co(fun _ -> Then(p |> Coroutine.map k), None, None)

type CoroutineBuilder() =
  member _.Zero() =
    Co(fun _ -> CoroutineResult.Return(()), None, None)

  member _.Return(result: 'a) =
    Co(fun _ -> CoroutineResult.Return(result), None, None)

  member _.Yield(result: 'a) =
    Co(fun _ -> CoroutineResult.Return(result), None, None)

  member co.Yield() =
    Co(fun _ -> CoroutineResult.Wait(TimeSpan.FromMilliseconds 0., co.Return()), None, None)

  member _.Bind(p: Coroutine<'a, 's, 'c, 'e>, k: 'a -> Coroutine<'b, 's, 'c, 'e>) = Coroutine.bind (p, k)

  member _.Combine(p: Coroutine<Unit, 's, 'c, 'e>, k: Coroutine<'a, 's, 'c, 'e>) =
    Co(fun _ -> CoroutineResult.Combine(p, k), None, None)

  member _.Any(ps: List<Coroutine<'a, 's, 'c, 'e>>) =
    Co(fun _ -> CoroutineResult.Any(ps), None, None)
  // member _.All(ps:List<Coroutine<'a, 's, 'c, 'e>>) =
  //   Co(fun _ -> CoroutineResult.Any(ps), None, None)
  member co.YieldAfter(p: Coroutine<_, _, _, _>) =
    // co.Bind(p, fun x -> co.Bind(co.Wait(TimeSpan.FromSeconds 0.0), fun _ -> co.Return(x)))
    p

  member co.On(p_e: 'e -> Option<'a>) =
    co.YieldAfter(Co(fun _ -> CoroutineResult.On(p_e), None, None))

  [<CustomOperation("wait", MaintainsVariableSpaceUsingBind = true)>]
  member co.WaitOp(p: Coroutine<_, _, _, _>, [<ProjectionParameter>] t) =
    co.Bind(p, fun x -> Co(fun _ -> CoroutineResult.Wait(t x, co.Return()), None, None))

  member co.For(seq, body) =
    let seq: seq<Coroutine<Unit, _, _, _>> = seq |> Seq.map body
    seq |> Seq.fold (fun acc p -> co.Combine(acc, p)) (co.Return())
  // Co(fun _ -> CoroutineResult.For(seq, body), None, None)
  member co.Wait(t) =
    Co(fun _ -> CoroutineResult.Wait(t, co.Return()), None, None)

  member co.Do(f: 's -> 'a) =
    co.YieldAfter(Co(fun _ -> CoroutineResult.Do(f), None, None))

  member co.Await(f: 'c -> Task<'a>) : Coroutine<'a, 's, 'c, 'e> =
    co {
      let! task = co.Do f
      return! co.Await task
    }

  member co.Await(p: Task<'a>) =
    co.YieldAfter(Co(fun _ -> CoroutineResult.Await(p), None, None))
  // member _.Awaiting(id:Guid, p : Async<'a>, t:Task<'a>) =
  //   Co(fun _ -> CoroutineResult.Awaiting(id,p,t), None, None)
  member _.Spawn(p: Coroutine<Unit, 's, 'c, 'e>) =
    Co(fun _ -> CoroutineResult.Spawn(p), None, None)

  member _.Repeat(p: Coroutine<'a, 's, 'c, 'e>) : Coroutine<'a, 's, 'c, 'e> =
    Co(fun _ -> CoroutineResult.Repeat(p), None, None)

  member _.GetContext() =
    Co(fun (_, c, _, _) -> CoroutineResult.Return(c), None, None)

  member _.GetState() =
    Co(fun (s, _, _, _) -> CoroutineResult.Return(s), None, None)

  member _.SetState(u: U<'s>) =
    Co(fun (_, _, _, _) -> CoroutineResult.Return(), Some u, None)

  [<CustomOperation("produce", MaintainsVariableSpaceUsingBind = true, AllowIntoPattern = true)>]
  member co.ProduceOp(p: Coroutine<'a, 's, 'c, 'e>, [<ProjectionParameter>] new_event) =
    co.Bind(
      p,
      fun x ->
        Co(fun _ -> CoroutineResult.Return(), None, Some(fun es -> let (id, e) = new_event x in es |> Map.add id e))
    )

  member co.Produce(new_event) =
    co.YieldAfter(
      Co(fun _ -> CoroutineResult.Return(), None, Some(fun es -> let (id, e) = new_event in es |> Map.add id e))
    )

  member co.ReturnFrom(p: Coroutine<'a, 's, 'c, 'e>) =
    co {
      let! res = p
      return res
    }

type WaitingCoroutine<'a, 's, 'c, 'e> =
  { P: Coroutine<'a, 's, 'c, 'e>
    Until: DateTime }

type EvaluatedCoroutine<'a, 's, 'c, 'e> =
  | Done of 'a * Option<U<'s>> * Option<U<Map<Guid, 'e>>>
  | Spawned of
    List<Coroutine<Unit, 's, 'c, 'e>> *
    Option<U<'s>> *
    Option<U<Map<Guid, 'e>>> *
    Option<Coroutine<'a, 's, 'c, 'e>>
  | Active of Coroutine<'a, 's, 'c, 'e> * Option<U<'s>> * Option<U<Map<Guid, 'e>>>
  | Listening of Coroutine<'a, 's, 'c, 'e> * Option<U<'s>> * Option<U<Map<Guid, 'e>>>
  | Waiting of WaitingCoroutine<'a, 's, 'c, 'e> * Option<U<'s>> * Option<U<Map<Guid, 'e>>>
  | WaitingOrListening of WaitingCoroutine<'a, 's, 'c, 'e> * Option<U<'s>> * Option<U<Map<Guid, 'e>>>

  member this.After(u_s, u_e) =
    match this with
    | Done(x, u_s', u_e') -> Done(x, u_s >>? u_s', u_e >>? u_e')
    | Spawned(x, u_s', u_e', p) -> Spawned(x, u_s >>? u_s', u_e >>? u_e', p)
    | Active(x, u_s', u_e') -> Active(x, u_s >>? u_s', u_e >>? u_e')
    | Listening(x, u_s', u_e') -> Listening(x, u_s >>? u_s', u_e >>? u_e')
    | Waiting(x, u_s', u_e') -> Waiting(x, u_s >>? u_s', u_e >>? u_e')
    | WaitingOrListening(x, u_s', u_e') -> WaitingOrListening(x, u_s >>? u_s', u_e >>? u_e')

type EvaluatedCoroutines<'s, 'c, 'e> =
  { active: Map<Guid, Coroutine<Unit, 's, 'c, 'e>>
    stopped: Set<Guid>
    waiting: Map<Guid, WaitingCoroutine<Unit, 's, 'c, 'e>>
    listening: Map<Guid, Coroutine<Unit, 's, 'c, 'e>>
    waitingOrListening: Map<Guid, WaitingCoroutine<Unit, 's, 'c, 'e>> }

type Coroutine<'a, 's, 'c, 'e> with
  static member eval<'a>
    ((Co p): Coroutine<'a, 's, 'c, 'e>)
    (ctx: 's * 'c * Map<Guid, 'e> * DeltaT)
    : EvaluatedCoroutine<'a, 's, 'c, 'e> =
    let co = CoroutineBuilder()
    let (_, c, es, dt) = ctx
    let (step, u_s, u_e) = p ctx

    match step with
    | Then(p_p) ->
      match Coroutine.eval p_p ctx with
      | Done(p, u_s, u_e) ->
        let res = Coroutine.eval p ctx
        res.After(u_s, u_e)
      | Spawned(p', u_s, u_e, rest: Option<Coroutine<Coroutine<'a, 's, 'c, 'e>, 's, 'c, 'e>>) ->
        Spawned(
          p',
          u_s,
          u_e,
          rest
          |> Option.map (fun rest_p -> Co(fun _ -> CoroutineResult.Then(rest_p), None, None))
        )
      | Active(p_p', u_s, u_e) -> Active(Co(fun _ -> CoroutineResult.Then(p_p'), None, None), u_s, u_e)
      | Listening(p_p', u_s, u_e) -> Listening(Co(fun _ -> CoroutineResult.Then(p_p'), None, None), u_s, u_e)
      | Waiting(w, u_s, u_e) ->
        Waiting(
          { P = Co(fun _ -> CoroutineResult.Then(w.P), None, None)
            Until = w.Until },
          u_s,
          u_e
        )
      | WaitingOrListening(w, u_s, u_e) ->
        Waiting(
          { P = Co(fun _ -> CoroutineResult.Then(w.P), None, None)
            Until = w.Until },
          u_s,
          u_e
        )
    | Combine(p, k) ->
      match Coroutine.eval p ctx with
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
            | Choice2Of2 _ -> res
            | Choice1Of2(ps', spawned', u_s, u_e) ->
              match Coroutine.eval p ctx with
              | Done(res, u_s', u_e') -> Choice2Of2(res, u_s >>? u_s', u_e >>? u_e')
              | Spawned(p', u_s', u_e', rest) ->
                Choice1Of2(ps' @ Option.toList rest, p' @ spawned', u_s >>? u_s', u_e >>? u_e')
              | Active(p', u_s', u_e')
              | Listening(p', u_s', u_e') -> Choice1Of2(p' :: ps', spawned', u_s >>? u_s', u_e >>? u_e')
              | Waiting({ P = p'; Until = until }, u_s', u_e')
              | WaitingOrListening({ P = p'; Until = until }, u_s', u_e') ->
                Choice1Of2(
                  Co(fun _ -> CoroutineResult.Wait(until - DateTime.Now, p'), None, None) :: ps',
                  spawned',
                  u_s >>? u_s',
                  u_e >>? u_e'
                ))
          (Choice1Of2([], [], None, None))

      match res with
      | Choice1Of2(ps', [], u_s, u_e) -> Active(co.Any ps', u_s, u_e)
      | Choice1Of2(ps', spawned, u_s, u_e) -> Spawned(spawned, u_s, u_e, Some(co.Any ps'))
      | Choice2Of2(res, u_s, u_e) -> Done(res, u_s, u_e)
    | Wait(timeSpan, p': Coroutine<'a, 's, 'c, 'e>) ->
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

  static member evalMany<'s, 'c, 'e>
    (ps: Map<Guid, Coroutine<Unit, 's, 'c, 'e>>)
    ((s, c, es, dt): 's * 'c * Map<Guid, 'e> * DeltaT)
    : EvaluatedCoroutines<'s, 'c, 'e> * Option<U<'s>> * Option<U<Map<Guid, 'e>>> =
    let ctx = (s, c, es, dt)
    let mutable u_s: Option<U<'s>> = None
    let mutable u_e: Option<U<Map<Guid, 'e>>> = None

    let mutable evaluated: EvaluatedCoroutines<'s, 'c, 'e> =
      { active = Map.empty
        stopped = Set.empty
        waiting = Map.empty
        listening = Map.empty
        waitingOrListening = Map.empty }

    for p in ps do
      match Coroutine.eval p.Value ctx with
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
