namespace Ballerina.Stackless.State.WithError

module StacklessStateWithError =
  open Ballerina.Collections.Sum

  type private Existential = obj

  // models a state monad: type State<'a, 'c, 's, 'e> = State of ('c * 's -> Sum<'a * Option<'s>, 'e * Option<'s>>)
  type private MonadResult<'a, 's, 'e> = Sum<'a * Option<'s>, 'e * Option<'s>>

  type Node<'c, 's, 'e> =
    | Ret of ('c * 's -> MonadResult<Existential, 's, 'e>)
    | Bind of Node<'c, 's, 'e> * SuccessContinuation<'c, 's, 'e>
    | Catch of Node<'c, 's, 'e> * ErrorContinuation<'c, 's, 'e>

  and private SuccessContinuation<'c, 's, 'e> = Existential -> Node<'c, 's, 'e>
  and private ErrorContinuation<'c, 's, 'e> = 'e * Option<'s> -> Node<'c, 's, 'e>

  type BindScope = int

  // needed for typing (Node cannot have the value type info because it's existential)
  type FreeNode<'a, 'c, 's, 'e> =
    | FreeNodeCons of Node<'c, 's, 'e>

    static member Return(a: 'a) : FreeNode<'a, 'c, 's, 'e> =
      FreeNodeCons(Ret(fun _ -> Left(box a, None)))

    static member fromStep(f: 'c * 's -> MonadResult<'a, 's, 'e>) : FreeNode<'a, 'c, 's, 'e> =
      FreeNodeCons(
        Ret(fun cs ->
          match f cs with
          | Left(v, s') -> Left(box v, s')
          | Right(e, s') -> Right(e, s'))
      )

    static member setState(f: 's -> 's) : FreeNode<unit, 'c, 's, 'e> =
      FreeNodeCons(Ret(fun (_c, s) -> Left(box (), Some(f s))))

    static member getState: FreeNode<'s, 'c, 's, 'e> =
      FreeNodeCons(Ret(fun (_c, s) -> Left(box s, None)))

    static member bind (m: FreeNode<'a, 'c, 's, 'e>) (f: 'a -> FreeNode<'b, 'c, 's, 'e>) : FreeNode<'b, 'c, 's, 'e> =
      let (FreeNodeCons node) = m

      // we cannot know the type here, because the program can have many different types at any point, but we must fit them all into a single Node type
      // nonetheless, we know for sure that this is well-typed because we're executing an originally well-typed program
      let k (o: Existential) =
        let (FreeNodeCons node2) = f (unbox<'a> o)
        node2

      FreeNodeCons(Bind(node, k))

    static member catch(m: FreeNode<'a, 'c, 's, 'e>) : FreeNode<Sum<'a, 'e>, 'c, 's, 'e> =
      // catch returns a type Sum<'a, 'e>, which is the result of the actual computation
      // the key is that this is then always returned as a Left (as far as the monad understands)
      // such that regardless of the actual result, a continuation can be applied
      // note that the catch continuation is essentially a hardcoded always-success continuation
      let (FreeNodeCons node) =
        FreeNode.bind m (fun v ->
          let res: Sum<'a, 'e> = Left v // wrap in the catch return type
          FreeNode.Return res) // lift back to the monad

      FreeNodeCons(
        Catch(
          node,
          fun (e, s') ->
            Ret(fun _ ->
              let res: Sum<'a, 'e> = Right e // wrap in the catch return type
              Left(box res, s')) // lift back to the monad -> as success!
        )
      )

    static member run (c: 'c) (s0: 's) (m: FreeNode<'a, 'c, 's, 'e>) : MonadResult<'a, 's, 'e> =
      let (FreeNodeCons start) = m
      let mutable cur = start
      let mutable s = s0
      let mutable latestState: 's option = None
      let mutable curBindScope: BindScope = 0

      // function call stack
      let mutable bindStack: SuccessContinuation<'c, 's, 'e> list = []

      // error continuations/handlers, each paired with the bindStack (pointer) and state that must be restored on failure
      let mutable catchStack
        : (ErrorContinuation<'c, 's, 'e> * SuccessContinuation<'c, 's, 'e> list * 's * 's option * BindScope) list =
        []

      let popExpiredCatches () =
        // why is tracking the bind depth enough?
        // because a catch handler expires as soon as we've returned to the bind scope it was registered in
        let mutable keepPopping = true

        while keepPopping do
          match catchStack with
          | (_, _, _, _, savedScope) :: rest when savedScope = curBindScope -> catchStack <- rest
          | _ -> keepPopping <- false

      let rec loop () =
        match cur with
        | Ret step ->
          match step (c, s) with
          | Left(v, s') ->

            match s' with
            | Some s' ->
              s <- s'
              latestState <- Some s'
            | None -> ()

            match bindStack with
            | [] ->
              popExpiredCatches ()
              Left(unbox<'a> v, latestState)

            | k :: rest ->
              bindStack <- rest
              curBindScope <- curBindScope - 1
              popExpiredCatches ()
              cur <- k v
              loop ()

          | Right(e, s') ->
            match s' with
            | Some s' -> s <- s'
            | None -> ()

            match catchStack with
            | [] -> Right(e, s')
            | (err_k, savedBinds, savedS, savedLatestState, savedScope) :: rest ->
              catchStack <- rest // pop off current err handler
              bindStack <- savedBinds // restore the program to where it was before the current subroutine was started
              // restore state as well
              s <- savedS
              latestState <- savedLatestState
              curBindScope <- savedScope
              cur <- err_k (e, s')
              loop ()

        | Bind(sub, k) ->
          curBindScope <- curBindScope + 1
          bindStack <- k :: bindStack
          cur <- sub
          loop ()

        | Catch(sub, err_k) ->
          catchStack <- (err_k, bindStack, s, latestState, curBindScope) :: catchStack
          cur <- sub
          loop ()

      loop ()

    static member throw(e: 'e) : FreeNode<'a, 'c, 's, 'e> =
      FreeNodeCons(Ret(fun (_c, _s) -> Right(e, None)))

    static member any (concat: 'e * 'e -> 'e) (ps: List<FreeNode<'a, 'c, 's, 'e>>) : FreeNode<'a, 'c, 's, 'e> =
      let mutable acc: FreeNode<Sum<'a, 'e> option * 'e option, 'c, 's, 'e> =
        FreeNode.Return(None, None)

      for p in ps do
        acc <-
          FreeNode.bind acc (fun (current, errors) ->
            match current with
            | Some(Left v) -> FreeNode.Return(Some(Left v), errors) // Already have success, short-circuit
            | _ ->
              FreeNode.bind (FreeNode.catch p) (fun res ->
                match res with
                | Left v -> FreeNode.Return(Some(Left v), errors)
                | Right err ->
                  let newErrors =
                    match errors with
                    | Some errs -> Some(concat (errs, err))
                    | None -> Some err

                  FreeNode.Return(None, newErrors)))

      FreeNode.bind acc (fun (result, errors) ->
        match result with
        | Some(Left v) -> FreeNode.Return v
        | _ ->
          match errors with
          | Some err -> FreeNode.throw err
          | None -> failwith "Unreachable: Any requires at least one element")

    static member all(ps: List<FreeNode<'a, 'c, 's, 'e>>) : FreeNode<List<'a>, 'c, 's, 'e> =
      let mutable acc: FreeNode<List<'a>, 'c, 's, 'e> = FreeNode.Return []

      for p in ps do
        acc <-
          FreeNode.bind acc (fun results ->
            FreeNode.bind (FreeNode.catch p) (fun res ->
              match res with
              | Left v -> FreeNode.Return(v :: results)
              | Right err -> FreeNode.throw err))

      FreeNode.bind acc (fun results -> FreeNode.Return(results |> List.rev))
