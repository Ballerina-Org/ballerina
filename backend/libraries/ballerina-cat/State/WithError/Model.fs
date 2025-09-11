namespace Ballerina.State

module WithError =
  open Ballerina.Fun
  open Ballerina.Collections.Sum
  open Ballerina.Collections
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Reader.WithError

  type State<'a, 'c, 's, 'e> =
    | State of ('c * 's -> Sum<'a * Option<'s>, 'e * Option<'s>>)

    member this.run(c, s) = let (State p) = this in p (c, s)

    static member Run (input: 'c * 's) (p: State<'a, 'c, 's, 'e>) = p.run input

    // For the uncultured swine:
    // ∇ = narrowing
    // Δ = widening
    static member mapState<'sOuter>
      (``∇``: 'sOuter -> 's)
      (Δ: 's -> 'sOuter -> 'sOuter)
      ((State p): State<'a, 'c, 's, 'e>)
      : State<'a, 'c, 'sOuter, 'e> =
      State(fun (c, s0: 'sOuter) ->
        match p (c, (``∇`` s0)) with
        | Sum.Left(res, u_s) -> Sum.Left(res, u_s |> Option.map (fun s -> Δ s s0))
        | Sum.Right(e, u_s) -> Sum.Right(e, u_s |> Option.map (fun s -> Δ s s0)))

    static member mapContext<'cOuter>
      (``∇``: 'cOuter -> 'c)
      ((State p): State<'a, 'c, 's, 'e>)
      : State<'a, 'cOuter, 's, 'e> =
      State(fun (c, s0) -> p (``∇`` c, s0))

    static member map<'b> (f: 'a -> 'b) ((State p): State<'a, 'c, 's, 'e>) : State<'b, 'c, 's, 'e> =
      State(fun s0 ->
        match p s0 with
        | Sum.Left(res, u_s) -> Sum.Left(f res, u_s)
        | Sum.Right e -> Sum.Right e)

    static member mapError<'e1> (f: 'e -> 'e1) ((State p): State<'a, 'c, 's, 'e>) : State<'a, 'c, 's, 'e1> =
      State(fun s0 ->
        match p s0 with
        | Sum.Left l -> Sum.Left l
        | Sum.Right(e, s1) -> Sum.Right(f e, s1))

    static member fromValue(res: 'a) = State(fun _ -> Sum.Left(res, None))

    static member flatten((State p): State<State<'a, 'c, 's, 'e>, 'c, 's, 'e>) : State<'a, 'c, 's, 'e> =
      State(fun (c, s0) ->
        match p (c, s0) with
        | Sum.Left(State p', s1) ->
          match
            p' (
              match s1 with
              | None -> c, s0
              | Some s1 -> c, s1
            )
          with
          | Sum.Left(res, s2) ->
            Sum.Left(
              res,
              match s2, s1 with
              | Some _, _ -> s2
              | None, Some _ -> s1
              | _ -> None
            )
          | Sum.Right e -> Sum.Right e
        | Sum.Right e -> Sum.Right e)

    static member bind (k: 'a -> State<'b, 'c, 's, 'e>) (p: State<'a, 'c, 's, 'e>) : State<'b, 'c, 's, 'e> =
      State.map k p |> State.flatten


  type StateBuilder() =
    member _.Ignore p = State.map ignore p
    member _.Map f p = State.map f p
    member _.MapContext f p = State.mapContext f p
    member _.MapError f p = State.mapError f p
    member _.Zero<'c, 's, 'e>() = State.fromValue<'c, 's, 'e> ()
    member _.Return<'a, 'c, 's, 'e>(result: 'a) = State.fromValue<'c, 's, 'e> result
    member _.Yield(result: 'a) = State.fromValue<'c, 's, 'e> result
    member _.Bind(p: State<'a, 'c, 's, 'e>, k: 'a -> State<'b, 'c, 's, 'e>) = State.bind k p
    member _.Combine(p: State<'b, 'c, 's, 'e>, k: State<'a, 'c, 's, 'e>) = State.bind (fun _ -> k) p
    member state.Repeat(p: State<'a, 'c, 's, 'e>) : State<'a, 'c, 's, 'e> = state.Bind(p, fun _ -> state.Repeat(p))
    member _.GetContext() = State(fun (c, _) -> Sum.Left(c, None))
    member _.GetState() = State(fun (_, s) -> Sum.Left(s, None))

    member _.SetState(u: U<'s>) =
      State(fun (_, s) -> Sum.Left((), Some(u s)))

    member state.ReturnFrom(p: State<'a, 'c, 's, 'e>) =
      state {
        let! res = p
        return res
      }

    member _.Catch((State p): State<'a, 'c, 's, 'e>) : State<Sum<'a, 'e>, 'c, 's, 'e> =
      State(fun cs ->
        match p cs with
        | Left(res: 'a, u_s: Option<'s>) -> let result: Sum<'a, 'e> = Left res in Left(result, u_s)
        | Right(err: 'e, u_s: Option<'s>) -> let result: Sum<'a, 'e> = Right err in Left(result, u_s))

    member _.Throw(e: 'e) = State(fun _ -> Sum.Right(e, None))
    member state.Delay p = state.Bind((state.Return()), p)

    member state.For(seq, body: _ -> State<Unit, _, _, _>) =
      match seq |> Seq.tryHead with
      | Some first -> state.Combine(body first, state.For(seq |> Seq.tail, body))
      | None -> state { return () }

    member state.Any<'a, 'c, 's, 'e>
      (e: {| concat: 'e * 'e -> 'e |}, l: NonEmptyList<State<'a, 'c, 's, 'e>>)
      : State<'a, 'c, 's, 'e> =
      state {
        match l with
        | NonEmptyList(p, ps) ->
          match! p |> state.Catch with
          | Left result -> result
          | Right error ->
            match ps with
            | [] -> return! error |> state.Throw
            | p' :: ps' ->
              match! state.Any(e, NonEmptyList.OfList(p', ps')) |> state.Catch with
              | Left result -> result
              | Right error' ->
                let finalError = e.concat (error, error')
                return! finalError |> state.Throw
      }

    member inline state.Any<'a, 'c, 's, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>
      (ps: NonEmptyList<State<'a, 'c, 's, 'b>>)
      =
      state.Any({| concat = 'b.Concat |}, ps)

    member inline state.Any<'a, 'c, 's, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>
      (p: State<'a, 'c, 's, 'b>, ps: List<State<'a, 'c, 's, 'b>>)
      =
      NonEmptyList.OfList(p, ps) |> state.Any

    member state.All<'a, 'c, 's, 'e>(e: {| concat: 'e * 'e -> 'e |}, ps: List<State<'a, 'c, 's, 'e>>) =
      match ps with
      | [] -> state.Return []
      | p :: ps ->
        state {
          let! p_res = p |> state.Catch
          let! ps_res = state.All(e, ps) |> state.Catch

          match p_res, ps_res with
          | Left r, Left rs -> return r :: rs
          | Right(err: 'e), Right(errs: 'e) ->
            let allErrs = e.concat (err, errs)
            return! state.Throw(allErrs)
          | Right(err: 'e), _
          | _, Right(err: 'e) -> return! state.Throw err
        }

    member inline state.All<'a, 'c, 's, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>
      (ps: List<State<'a, 'c, 's, 'b>>)
      =
      state.All({| concat = 'b.Concat |}, ps)

    member inline state.All<'a, 'c, 's, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>
      (ps: seq<State<'a, 'c, 's, 'b>>)
      =
      state.All({| concat = 'b.Concat |}, ps |> Seq.toList)

    member inline state.AllMap<'k, 'a, 'c, 's, 'b when 'k: comparison and 'b: (static member Concat: 'b * 'b -> 'b)>
      (ps: Map<'k, State<'a, 'c, 's, 'b>>)
      =
      state.All(
        {| concat = 'b.Concat |},
        ps
        |> Map.toSeq
        |> Seq.map (fun (k, p) ->
          state {
            let! v = p
            return k, v
          })
        |> Seq.toList
      )
      |> state.Map(Map.ofSeq)

    member state.OfReader<'a, 'c, 's, 'e>(Reader r: Reader<'a, 'c, 'e>) : State<'a, 'c, 's, 'e> =
      State(fun (c, _s) ->
        match r c with
        | Sum.Left(res) -> Sum.Left(res, None)
        | Sum.Right(err) -> Sum.Right(err, None))

    member state.OfStateReader<'a, 'c, 's, 'e>(Reader r: Reader<'a, 's, 'e>) : State<'a, 'c, 's, 'e> =
      State(fun (_c, s) ->
        match r s with
        | Sum.Left(res) -> Sum.Left(res, None)
        | Sum.Right(err) -> Sum.Right(err, None))

    member state.ToReader<'a, 'c, 's, 'e>(State p: State<'a, 'c, 's, 'e>) : Reader<'a, 'c * 's, 'e> =
      Reader(fun (c, s) ->
        match p (c, s) with
        | Sum.Left(res, _) -> Sum.Left(res)
        | Sum.Right(err, _) -> Sum.Right(err))

    member state.OfSum s =
      match s with
      | Left res -> state.Return res
      | Right err -> state.Throw err

    member inline state.All2<'a1, 'a2, 'c, 's, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (p1: State<'a1, 'c, 's, 'e>)
      (p2: State<'a2, 'c, 's, 'e>)
      =
      state {
        let! v1 = p1 |> state.Catch
        let! v2 = p2 |> state.Catch

        match v1, v2 with
        | Left v1, Left v2 -> return v1, v2
        | Right e1, Right e2 -> return! state.Throw('e.Concat(e1, e2))
        | Right e, _
        | _, Right e -> return! state.Throw e
      }

    member inline state.All3 p1 p2 p3 =
      state.All2 p1 (state.All2 p2 p3) |> state.Map Tuple.fromNested3

    member inline state.All4 p1 p2 p3 p4 =
      state.All2 p1 (state.All2 p2 (state.All2 p3 p4)) |> state.Map Tuple.fromNested4

    member inline state.All5 p1 p2 p3 p4 p5 =
      state.All2 p1 (state.All2 p2 (state.All2 p3 (state.All2 p4 p5)))
      |> state.Map Tuple.fromNested5

    member inline state.Either<'a, 'c, 's, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (p1: State<'a, 'c, 's, 'e>)
      (p2: State<'a, 'c, 's, 'e>)
      =
      state {
        let! v1 = p1 |> state.Catch

        match v1 with
        | Left v -> return v
        | Right e1 ->
          let! v2 = p2 |> state.Catch

          match v2 with
          | Left v -> return v
          | Right e2 -> return! state.Throw('e.Concat(e1, e2))
      }

    member inline state.Either3 (p1: State<'a, 'c, 's, 'e>) (p2: State<'a, 'c, 's, 'e>) (p3: State<'a, 'c, 's, 'e>) =
      state.Either p1 (state.Either p2 p3)

    member inline state.Either4 p1 p2 p3 p4 =
      state.Either p1 (state.Either p2 (state.Either p3 p4))

    member inline state.Either5 p1 p2 p3 p4 p5 =
      state.Either p1 (state.Either p2 (state.Either p3 (state.Either p4 p5)))

    member inline state.Memo (get: 'i -> 's -> 'o option, set: 'i * 'o -> Updater<'s>) (f: 'i -> State<'o, _, 's, _>) =
      fun (i: 'i) ->
        state {
          let! s = state.GetState()

          match get i s with
          | Some(v) -> return v
          | None ->
            let! v = f i
            do! set (i, v) |> state.SetState
            return v
        }

    member state.RunOption(p: Option<State<'a, 'c, 's, 'e>>) =
      state {
        match p with
        | Some p ->
          let! a = p
          return Some a
        | None -> return None
      }

  let state = StateBuilder()

  type State<'a, 'c, 's, 'e> with
    static member (>>=)(p, q) =
      state {
        let! x = p
        return! q x
      }
