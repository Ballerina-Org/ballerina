namespace Ballerina.State

module Seq =

  open Ballerina.Fun
  open Ballerina.Collections.Sum

  type SeqState<'a, 'c, 's, 'e> =
    | State of ('c * 's -> Sum<seq<'a> * Option<'s>, 'e * Option<'s>>)

    member this.run(c, s) = let (State p) = this in p (c, s)

    static member map<'b> (f: 'a -> 'b) ((State p): SeqState<'a, 'c, 's, 'e>) : SeqState<'b, 'c, 's, 'e> =
      State(fun s0 ->
        match p s0 with
        | Sum.Left(res, u_s) -> Sum.Left(res |> Seq.map f, u_s)
        | Sum.Right(e, u_s) -> Sum.Right(e, u_s))

    static member fromValue(res: 'a) = State(fun _ -> Sum.Left([ res ], None))
    static member zero() = State(fun _ -> Sum.Left([], None))

    static member flatten((State p): SeqState<SeqState<'a, 'c, 's, 'e>, 'c, 's, 'e>) : SeqState<'a, 'c, 's, 'e> =
      State(fun (c, s0) ->
        match p (c, s0) with
        | Sum.Left(ps, s1) ->
          // fold over ps and Sum.Left([],s1)
          let res =
            ps
            |> Seq.fold
              (function
              | Sum.Right e -> fun _ -> Sum.Right e
              | Sum.Left(_, s1) ->
                fun (State p') ->
                  match p' (c, s1) with
                  | Sum.Left(res, s2) ->
                    Sum.Left(
                      res,
                      match s2 with
                      | Some s2 -> s2
                      | None -> s1
                    )
                  | Sum.Right e -> Sum.Right e)
              (Sum.Left(
                [],
                (match s1 with
                 | None -> s0
                 | Some s1 -> s1)
              ))

          match res with
          | Sum.Right e -> Sum.Right e
          | Sum.Left(res, s) -> Sum.Left(res, Some s)
        | Sum.Right e -> Sum.Right e)

    static member bind (p: SeqState<'a, 'c, 's, 'e>) (k: 'a -> SeqState<'b, 'c, 's, 'e>) : SeqState<'b, 'c, 's, 'e> =
      SeqState.map k p |> SeqState.flatten


  type SeqStateBuilder() =
    member _.Map f p = SeqState.map f p
    member _.Zero<'a, 'c, 's, 'e>() = SeqState.zero<'a, 'c, 's, 'e> ()
    member _.Return<'a, 'c, 's, 'e>(result: 'a) = SeqState.fromValue<'c, 's, 'e> result
    member _.Yield(result: 'a) = SeqState.fromValue<'c, 's, 'e> result
    member _.Bind(p: SeqState<'a, 'c, 's, 'e>, k: 'a -> SeqState<'b, 'c, 's, 'e>) = SeqState.bind p k
    member _.Combine(p: SeqState<'b, 'c, 's, 'e>, k: SeqState<'a, 'c, 's, 'e>) = SeqState.bind p (fun _ -> k)
    //   member _.Any(ps:List<Coroutine<'a, 's, 'c, 'e>>) =
    //     Co(fun _ -> CoroutineResult.Any(ps), None, None)
    //   // member _.All(ps:List<Coroutine<'a, 's, 'c, 'e>>) =
    //   //   Co(fun _ -> CoroutineResult.Any(ps), None, None)
    member state.Repeat(p: SeqState<'a, 'c, 's, 'e>) : SeqState<'a, 'c, 's, 'e> =
      state.Bind(p, fun _ -> state.Repeat(p))

    member _.GetContext() = State(fun (c, _) -> Sum.Left(c, None))
    member _.GetState() = State(fun (_, s) -> Sum.Left(s, None))

    member _.SetState(u: U<'s>) =
      State(fun (_, s) -> Sum.Left([], Some(u s)))

    member state.ReturnFrom(p: SeqState<'a, 'c, 's, 'e>) =
      state {
        let! res = p
        return res
      }

    member _.Catch(State p) =
      State(fun cs ->
        match p cs with
        | Left(res, u_s) -> Left([ Left res ], u_s)
        | Right err -> Left([ Right err ], None))

    member _.Throw(e: 'e) = State(fun _ -> Sum.Right(e, None))
    member state.Delay p = state.Bind((state.Return()), p)

    member state.For(seq, body: _ -> SeqState<_, _, _, _>) =
      match seq |> Seq.tryHead with
      | Some first -> state.Combine(body first, state.For(seq |> Seq.tail, body))
      | None -> state.Zero()
    // member state.All<'a,'c,'s,'e>
    //   (e:{| concat:'e * 'e -> 'e |}, ps:List<State<'a,'c,'s,'e>>) =
    //   match ps with
    //   | [] -> state.Return []
    //   | p::ps ->
    //     state{
    //       let! p_res = p |> state.Catch
    //       let! ps_res = state.All(e,ps) |> state.Catch
    //       match p_res, ps_res with
    //       | Left r,Left rs ->
    //         return r::rs
    //       | Right (err:'e), Right (errs:'e) ->
    //         let allErrs = e.concat(err,errs)
    //         return! state.Throw(allErrs)
    //       | Right (err:'e),_ | _,Right (err:'e) ->
    //         return! state.Throw err
    //     }
    // member inline state.All<'a,'c,'s,'b
    //   when 'b : (static member Concat:'b * 'b -> 'b)>
    //   (ps:List<State<'a,'c,'s,'b>>) =
    //   state.All({| concat='b.Concat |}, ps)
    // member inline state.All<'a,'c,'s,'b
    //   when 'b : (static member Concat:'b * 'b -> 'b)>
    //   (ps:seq<State<'a,'c,'s,'b>>) =
    //   state.All({| concat='b.Concat |}, ps |> Seq.toList)

    member state.ToState(State p) =
      Ballerina.State.WithError.State(fun (c, s) ->
        match p (c, s) with
        | Sum.Left(res, u_s) -> Sum.Left(res, u_s)
        | Sum.Right e -> Sum.Right e)

    member state.OfSum s =
      match s with
      | Left res -> state.Return res
      | Right err -> state.Throw err

  let seqState = SeqStateBuilder()
