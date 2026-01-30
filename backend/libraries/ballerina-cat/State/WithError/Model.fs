namespace Ballerina.State

module WithError =
  open Ballerina.Fun
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Collections
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Reader.WithError

  open Ballerina.Stackless.State.WithError.StacklessStateWithError

  type State<'a, 'c, 's, 'e> =
    | State of FreeNode<'a, 'c, 's, 'e>

    member this.run(c, s) =
      let (State p) = this
      FreeNode.run c s p

    static member Run (input: 'c * 's) (p: State<'a, 'c, 's, 'e>) = p.run input

    // For the uncultured swine:
    // ∇ = narrowing
    // Δ = widening
    static member mapState<'sOuter>
      (``∇``: 'sOuter * 'c -> 's)
      (Δ: 's * 'c -> 'sOuter -> 'sOuter)
      ((State p): State<'a, 'c, 's, 'e>)
      : State<'a, 'c, 'sOuter, 'e> =
      State(
        FreeNode.fromStep (fun (c, s0: 'sOuter) ->
          match FreeNode.run c (``∇`` (s0, c)) p with
          | Left(res, u_s) -> Left(res, u_s |> Option.map (fun s -> Δ (s, c) s0))
          | Right(e, u_s) -> Right(e, u_s |> Option.map (fun s -> Δ (s, c) s0)))
      )

    static member mapContext<'cOuter>
      (``∇``: 'cOuter -> 'c)
      ((State p): State<'a, 'c, 's, 'e>)
      : State<'a, 'cOuter, 's, 'e> =
      State(FreeNode.fromStep (fun (c, s0) -> FreeNode.run (``∇`` c) s0 p))

    static member map<'b> (f: 'a -> 'b) ((State p): State<'a, 'c, 's, 'e>) : State<'b, 'c, 's, 'e> =
      State(FreeNode.bind p (fun a -> FreeNode.Return(f a)))

    static member mapError<'e1> (f: 'e -> 'e1) ((State p): State<'a, 'c, 's, 'e>) : State<'a, 'c, 's, 'e1> =
      State(
        FreeNode.fromStep (fun (c, s) ->
          match FreeNode.run c s p with
          | Left(v, s') -> Left(v, s')
          | Right(e, s') -> Right(f e, s'))
      )

    static member bind<'a, 'b, 'c, 's, 'e>
      (k: 'a -> State<'b, 'c, 's, 'e>)
      (p: State<'a, 'c, 's, 'e>)
      : State<'b, 'c, 's, 'e> =
      let (State p') = p
      State(FreeNode.bind p' (fun a -> let (State p'') = k a in p''))

  type StateBuilder() =
    member _.Ignore p = State.map ignore p
    member _.Map f p = State.map f p
    member _.MapContext f p = State.mapContext f p
    member _.MapError f p = State.mapError f p
    member _.Zero<'c, 's, 'e>() : State<unit, 'c, 's, 'e> = State(FreeNode.Return())
    member _.Return<'a, 'c, 's, 'e>(result: 'a) : State<'a, 'c, 's, 'e> = State(FreeNode.Return result)
    member _.Yield<'a, 'c, 's, 'e>(result: 'a) : State<'a, 'c, 's, 'e> = State(FreeNode.Return result)
    member _.Bind(p: State<'a, 'c, 's, 'e>, k: 'a -> State<'b, 'c, 's, 'e>) = State.bind k p
    member _.Combine(p: State<'b, 'c, 's, 'e>, k: State<'a, 'c, 's, 'e>) = State.bind (fun _ -> k) p
    member state.Repeat(p: State<'a, 'c, 's, 'e>) : State<'a, 'c, 's, 'e> = state.Bind(p, fun _ -> state.Repeat(p))

    member _.GetContext<'c, 's, 'e>() : State<'c, 'c, 's, 'e> =
      State(FreeNode.fromStep (fun (c: 'c, _) -> Left(c, None)))

    member _.GetState() =
      State(FreeNode<'s, 'c, 's, 'e>.getState)

    member _.SetState(u: U<'s>) =
      State(FreeNode<'s, 'c, 's, 'e>.setState u)

    member state.ReturnFrom(p: State<'a, 'c, 's, 'e>) =
      state {
        let! res = p
        return res
      }

    member _.Catch((State p): State<'a, 'c, 's, 'e>) : State<Sum<'a, 'e>, 'c, 's, 'e> = State(FreeNode.catch p)

    member _.Throw(e: 'e) = State(FreeNode.throw e)
    member state.Delay<'a, 'c, 's, 'e>(p: unit -> State<'a, 'c, 's, 'e>) : State<'a, 'c, 's, 'e> = p ()

    member state.AnyAcc<'a, 'c, 's, 'e>
      (e: {| concat: 'e * 'e -> 'e |}, e0: Option<'e>, l: NonEmptyList<State<'a, 'c, 's, 'e>>)
      : State<'a, 'c, 's, 'e> =
      state {
        match l with
        | NonEmptyList(p, ps) ->
          match! p |> state.Catch with
          | Left result -> return result
          | Right e1 ->
            let e1 =
              match e0 with
              | Some e0 -> e.concat (e0, e1)
              | None -> e1

            match ps with
            | [] -> return! e1 |> state.Throw
            | p' :: ps' -> return! state.AnyAcc(e, e1 |> Some, NonEmptyList.OfList(p', ps'))
      }

    member state.Any<'a, 'c, 's, 'e>
      (e: {| concat: 'e * 'e -> 'e |}, ps: NonEmptyList<State<'a, 'c, 's, 'e>>)
      : State<'a, 'c, 's, 'e> =
      let ps = ps |> NonEmptyList.ToList |> List.map (fun (State p) -> p)
      State(FreeNode.any e.concat ps)

    member inline state.Any<'a, 'c, 's, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>
      (ps: NonEmptyList<State<'a, 'c, 's, 'b>>)
      =
      state.Any({| concat = 'b.Concat |}, ps)

    member inline state.Any<'a, 'c, 's, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>
      (p: State<'a, 'c, 's, 'b>, ps: List<State<'a, 'c, 's, 'b>>)
      =
      NonEmptyList.OfList(p, ps) |> state.Any

    member state.All<'a, 'c, 's, 'e>(_e: {| concat: 'e * 'e -> 'e |}, ps: List<State<'a, 'c, 's, 'e>>) =
      let ps = ps |> List.map (fun (State p) -> p)
      State(FreeNode.all ps)

    member inline state.All<'a, 'c, 's, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>
      (ps: List<State<'a, 'c, 's, 'b>>)
      =
      state.All({| concat = 'b.Concat |}, ps)

    member inline state.All<'a, 'c, 's, 'b when 'b: (static member Concat: 'b * 'b -> 'b)>
      (ps: seq<State<'a, 'c, 's, 'b>>)
      =
      state.All({| concat = 'b.Concat |}, ps |> Seq.toList)

    member inline state.For(seq, body: _ -> State<Unit, _, _, _>) =
      seq |> Seq.map body |> state.All |> state.Ignore

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
      State(
        FreeNode.fromStep (fun (c, _s) ->
          match r c with
          | Sum.Left(res) -> Left(res, None)
          | Sum.Right(err) -> Right(err, None))
      )

    member state.OfStateReader<'a, 'c, 's, 'e>(Reader r: Reader<'a, 's, 'e>) : State<'a, 'c, 's, 'e> =
      State(
        FreeNode.fromStep (fun (_c, s) ->
          match r s with
          | Sum.Left(res) -> Left(res, None)
          | Sum.Right(err) -> Right(err, None))
      )

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

    member inline state.All4 p1 p2 p3 p4 =
      state.All2 p1 (state.All2 p2 (state.All2 p3 p4)) |> state.Map Tuple.fromNested4

    member inline state.All5 p1 p2 p3 p4 p5 =
      state.All2 p1 (state.All2 p2 (state.All2 p3 (state.All2 p4 p5)))
      |> state.Map Tuple.fromNested5

    member inline state.Either<'a, 'c, 's, 'e when 'e: (static member Concat: 'e * 'e -> 'e)>
      (p1: State<'a, 'c, 's, 'e>)
      (p2: State<'a, 'c, 's, 'e>)
      =
      state.Any({| concat = 'e.Concat |}, NonEmptyList.OfList(p1, [ p2 ]))

    member inline state.Either3 (p1: State<'a, 'c, 's, 'e>) (p2: State<'a, 'c, 's, 'e>) (p3: State<'a, 'c, 's, 'e>) =
      state.Either p1 (state.Either p2 p3)

    member inline state.Either4 p1 p2 p3 p4 =
      state.Either p1 (state.Either p2 (state.Either p3 p4))

    member inline state.Either5 p1 p2 p3 p4 p5 =
      state.Either p1 (state.Either p2 (state.Either p3 (state.Either p4 p5)))

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
