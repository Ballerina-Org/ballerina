namespace Ballerina.State

module Simple =
  open Ballerina.Fun

  type State<'a, 'c, 's> =
    | State of ('c * 's -> 'a * Option<'s>)

    member this.run = let (State p) = this in p

    static member map<'b> (f: 'a -> 'b) ((State p): State<'a, 'c, 's>) : State<'b, 'c, 's> =
      State(fun s0 ->
        let (res, u_s) = p s0
        (f res, u_s))

    static member fromValue(res: 'a) = State(fun _ -> (res, None))

    static member flatten((State p): State<State<'a, 'c, 's>, 'c, 's>) : State<'a, 'c, 's> =
      State(fun (c, s0) ->
        match p (c, s0) with
        | (State p', s1) ->
          match
            p' (
              match s1 with
              | None -> c, s0
              | Some s1 -> c, s1
            )
          with
          | (res, s2) ->
            (res,
             match s2, s1 with
             | Some _, _ -> s2
             | None, Some _ -> s1
             | _ -> None))

    static member bind (p: State<'a, 'c, 's>) (k: 'a -> State<'b, 'c, 's>) : State<'b, 'c, 's> =
      State.map k p |> State.flatten


  type StateBuilder() =
    member _.Zero<'c, 's>() = State.fromValue<'c, 's> ()
    member _.Return<'a, 'c, 's>(result: 'a) = State.fromValue<'c, 's> result
    member _.Yield(result: 'a) = State.fromValue<'c, 's> result
    member _.Bind(p: State<'a, 'c, 's>, k: 'a -> State<'b, 'c, 's>) = State.bind p k
    member _.Combine(p: State<'b, 'c, 's>, k: State<'a, 'c, 's>) = State.bind p (fun _ -> k)
    //   member _.Any(ps:List<Coroutine<'a, 's, 'c, 'e>>) =
    //     Co(fun _ -> CoroutineResult.Any(ps), None, None)
    //   // member _.All(ps:List<Coroutine<'a, 's, 'c, 'e>>) =
    //   //   Co(fun _ -> CoroutineResult.Any(ps), None, None)
    member state.Repeat(p: State<'a, 'c, 's>) : State<'a, 'c, 's> = state.Bind(p, fun _ -> state.Repeat(p))
    member _.GetContext() = State(fun (c, _) -> (c, None))
    member _.GetState() = State(fun (_, s) -> (s, None))
    member _.SetState(u: U<'s>) = State(fun (_, s) -> ((), Some(u s)))

    member state.ReturnFrom(p: State<'a, 'c, 's>) =
      state {
        let! res = p
        return res
      }

    member state.Delay p = state.Bind((state.Return()), p)

    member state.For(seq, body: _ -> State<Unit, _, _>) =
      match seq |> Seq.tryHead with
      | Some first -> state.Combine(body first, state.For(seq |> Seq.tail, body))
      | None -> state { return () }

  let state = StateBuilder()
