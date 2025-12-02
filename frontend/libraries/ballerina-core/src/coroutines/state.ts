import { Map, Collection, OrderedMap } from "immutable";
import { Sum } from "../collections/domains/sum/state";
import { id } from "../fun/domains/id/state";
import { Unit } from "../fun/domains/unit/state";
import { BasicUpdater } from "../fun/domains/updater/state";
import { BasicFun, Fun } from "../fun/state";
import {Guid, Predicate, SimpleCallback} from "../../main";
export type DeltaT = number;
export type Coroutine<context, state, result> = {
  ([context, deltaT]: [context, DeltaT]): CoroutineStep<context, state, result>;
  // map: <nextResult>(
  //   f: Fun<result, nextResult>
  // ) => Coroutine<context, state, nextResult>;
  // mapState: (
  //   f: Fun<Updater<state> | undefined, Updater<state> | undefined>
  // ) => Coroutine<context, state, result>;
  embed: <parentContext, parentState>(
    narrow: BasicFun<parentContext, context | undefined>,
    widen: BasicFun<BasicUpdater<state>, BasicUpdater<parentState>>,
  ) => Coroutine<parentContext, parentState, result>;
  then: <nextResult>(
    f: BasicFun<result, Coroutine<context, state, nextResult>>,
  ) => Coroutine<context, state, nextResult>;
  seq: <nextResult>(
    k: Coroutine<context, state, nextResult>,
  ) => Coroutine<context, state, nextResult>;
};

const callMaybe =
  <a, b>(
    f: BasicFun<BasicUpdater<a>, BasicUpdater<b>>,
  ): BasicFun<BasicUpdater<a> | undefined, BasicUpdater<b> | undefined> =>
  (x) =>
    x == undefined ? undefined : f(x);

const thenMaybe = <state>(
  f: BasicUpdater<state> | undefined,
  g: BasicUpdater<state> | undefined,
): BasicUpdater<state> | undefined =>
  f == undefined ? g : g == undefined ? f : (x) => g(f(x));

export const Coroutine = {
  Create: <context, state, result>(
    p: BasicFun<[context, DeltaT], CoroutineStep<context, state, result>>,
  ): Coroutine<context, state, result> => {
    const co = p as unknown as Coroutine<context, state, result>;
    // co.map = function <nextResult>(
    //   f: Fun<result, nextResult>
    // ): Coroutine<context, state, nextResult> {
    //   return Coroutine.Map(this, f);
    // };
    // co.mapState = function (
    //   f: Fun<Updater<state> | undefined, Updater<state> | undefined>
    // ): Coroutine<context, state, result> {
    //   return Coroutine.MapState(this, f);
    // };
    co.embed = function <parentContext, parentState>(
      this: Coroutine<context, state, result>,
      narrow: BasicFun<parentContext, context | undefined>,
      widen: BasicFun<BasicUpdater<state>, BasicUpdater<parentState>>,
    ): Coroutine<parentContext, parentState, result> {
      return Coroutine.Embed<
        context,
        state,
        result,
        parentContext,
        parentState
      >(this, narrow, widen);
    };
    co.then = function <nextResult>(
      f: BasicFun<result, Coroutine<context, state, nextResult>>,
    ): Coroutine<context, state, nextResult> {
      // return Coroutine.Join(Coroutine.Map(this, f));
      return Coroutine.Create((_) => CoroutineStep.Then(undefined, this, f));
    };
    co.seq = function <nextResult>(
      f: Coroutine<context, state, nextResult>,
    ): Coroutine<context, state, nextResult> {
      return this.then(() => f);
    };
    return co;
  },
  UpdateState: <context, state>(
    stateUpdater: BasicFun<state, BasicUpdater<state>>,
  ): Coroutine<context, state, Unit> =>
    Coroutine.Create(([__, _]) =>
      CoroutineStep.Result((state) => stateUpdater(state)(state), {}),
    ),
  SetState: <context, state>(
    stateUpdater: BasicUpdater<state>,
  ): Coroutine<context, state, Unit> =>
    Coroutine.Yield(
      Coroutine.Create(([__, _]) => CoroutineStep.Result(stateUpdater, {})),
    ),
  GetState: <context, state>(): Coroutine<context, state, context> =>
    Coroutine.Create(([context, __]) =>
      CoroutineStep.Result(undefined, context),
    ),
  Start: <context, state>(): Coroutine<context, state, Unit> =>
    Coroutine.Create(([__, _]) => CoroutineStep.Result(undefined, {})),
  Return: <context, state, result>(
    result: result,
  ): Coroutine<context, state, result> =>
    Coroutine.Create(([__, _]) => CoroutineStep.Result(undefined, result)),
  Yield: <context, state, result>(
    next: Coroutine<context, state, result>,
  ): Coroutine<context, state, result> =>
    Coroutine.Create(([__, _]) => CoroutineStep.Yield(undefined, next)),
  Waiting: <context, state, result>(
    msLeft: number,
    next: Coroutine<context, state, result>,
  ): Coroutine<context, state, result> =>
    Coroutine.Create(([__, _]) =>
      CoroutineStep.Waiting(undefined, msLeft, next),
    ),
  // Map: <context, state, result1, result2>(
  //   p: Coroutine<context, state, result1>,
  //   f: Fun<result1, result2>
  // ): Coroutine<context, state, result2> =>
  //   Coroutine.Create((state) => CoroutineStep.Map(p(state), f)),
  MapContext: <childContext, result, parentContext, state>(
    p: Coroutine<childContext, state, result>,
    narrow: BasicFun<parentContext, childContext | undefined>,
  ): Coroutine<parentContext, state, result> =>
    Coroutine.Create(([parentContext, deltaT]) => {
      const childContext = narrow(parentContext);
      if (childContext != undefined)
        return CoroutineStep.MapContext(p([childContext, deltaT]), narrow);
      return CoroutineStep.Yield(undefined, Coroutine.MapContext(p, narrow));
    }),
  Embed: <childContext, childState, result, parentContext, parentState>(
    p: Coroutine<childContext, childState, result>,
    narrow: BasicFun<parentContext, childContext | undefined>,
    widen: BasicFun<BasicUpdater<childState>, BasicUpdater<parentState>>,
  ): Coroutine<parentContext, parentState, result> =>
    Coroutine.Create(([parentContext, deltaT]) => {
      const childContext = narrow(parentContext);
      if (childContext != undefined)
        return CoroutineStep.Embed(p([childContext, deltaT]), narrow, widen);
      return CoroutineStep.Yield(undefined, Coroutine.Embed(p, narrow, widen));
    }),
  MapState: <context, state, newState, result>(
    p: Coroutine<context, state, result>,
    f: BasicFun<BasicUpdater<state>, BasicUpdater<newState>>,
  ): Coroutine<context, newState, result> =>
    Coroutine.Create((state) => CoroutineStep.MapState(p(state), f)),
  // Join: <context, state, result>(
  //   p: Coroutine<context, state, Coroutine<context, state, result>>
  // ): Coroutine<context, state, result> =>
  //   Coroutine.Create<context, state, result>(([state, deltaT]) => {
  //     const step = p([state, deltaT]);
  //     if (step.kind == "result")
  //       return CoroutineStep.MapState(step
  //         .result([(step.newState || id)(state), deltaT]),
  //         (_) =>
  //           _ == undefined
  //             ? step.newState
  //             : step.newState == undefined
  //               ? _
  //               : then(step.newState, _)
  //         );
  //     if (step.kind == "yield")
  //       return CoroutineStep.Yield(step.newState, Coroutine.Join(step.next));
  //     if (step.kind == "waiting") {
  //       return CoroutineStep.Waiting(
  //         step.newState,
  //         step.msLeft,
  //         Coroutine.Join(step.next)
  //       );
  //     }
  //     return CoroutineStep.WaitingForEvent(step.newState, (e) => {
  //       const next = step.next(e);
  //       return next == "no match" ? next : Coroutine.Join(next);
  //     });
  //   }),
  Tick: <context, state, result>(
    context: context,
    p: Coroutine<context, state, result>,
    deltaT: number,
  ):
    | {
        kind: "continuing";
        excludeOthers?: true;
        state: BasicUpdater<state> | undefined;
        next: Coroutine<context, state, result>;
      }
    | {
        kind: "done";
        state: BasicUpdater<state> | undefined;
        result: result;
      } => {
    // debugger
    const step = p([context, deltaT]);
    if (step.kind == "result")
      return { kind: "done", state: step.newState, result: step.result };
    if (step.kind == "yield")
      return { kind: "continuing", state: step.newState, next: step.next };
    if (step.kind == "waiting" && step.msLeft <= deltaT)
      return { kind: "continuing", state: step.newState, next: step.next };
    if (step.kind == "waiting")
      return {
        kind: "continuing",
        state: step.newState,
        next: Coroutine.Waiting(step.msLeft - deltaT, step.next),
      };
    const next = Coroutine.Tick(context, step.p, deltaT);
    if (next.kind == "done")
      return {
        kind: "continuing",
        state: thenMaybe(step.newState, next.state),
        next: step.k(next.result),
      };
    else
      return {
        kind: "continuing",
        state: next.state,
        next: next.next.then(step.k),
      };
  },
  Nothing: <context, state, result>(): Coroutine<context, state, result> =>
    Coroutine.Create(([__, _]) =>
      CoroutineStep.Yield(undefined, Coroutine.Nothing()),
    ),
  Any: <context, state, result>(
    ps: Array<Coroutine<context, state, result>>,
  ): Coroutine<context, state, result> =>
    Coroutine.Create(([context, deltaT]) => {
      const ps1: Array<Coroutine<context, state, result>> = [];
      let nextState: BasicUpdater<state> | undefined = undefined;
      for (const p of ps) {
        const step = Coroutine.Tick(context, p, deltaT);
        nextState = thenMaybe(nextState, step.state);
        if (step.kind == "done")
          return CoroutineStep.Result(nextState, step.result);
        else {
          if (step.excludeOthers) {
            // console.log(
            //   `exclude others detected, killing ${ps.length - 1
            //   } other coroutines`
            // );
            return CoroutineStep.MapState(
              step.next([context, deltaT]),
              (_) => thenMaybe(nextState, _) || id,
            );
          } else ps1.push(step.next);
        }
      }
      return CoroutineStep.Yield(nextState, Coroutine.Any(ps1));
    }),
  All: <context, state, result>(
    ps: Array<Coroutine<context, state, result>>,
  ): Coroutine<context, state, Array<result>> =>
    Coroutine.Create(([context, deltaT]) => {
      const ps1: Array<Coroutine<context, state, result>> = [];
      let nextState: BasicUpdater<state> | undefined = undefined;
      let stillRunning = false;
      const results: Array<result> = [];
      for (const p of ps) {
        const step = Coroutine.Tick(context, p, deltaT);
        nextState = thenMaybe(nextState, step.state);
        if (step.kind == "done") {
          results.push(step.result);
          ps1.push(Coroutine.Return(step.result));
        } else {
          stillRunning = true;
          ps1.push(step.next);
        }
      }
      return stillRunning
        ? CoroutineStep.Yield(nextState, Coroutine.All(ps1))
        : CoroutineStep.Result(nextState, results);
    }),
  Repeat: <context, state>(
    p: Coroutine<context, state, Unit>,
  ): Coroutine<context, state, Unit> =>
    p.then((_) => Coroutine.Yield(Coroutine.Repeat(p))),
  Seq: <context, state>(
    ps: Array<Coroutine<context, state, Unit>>,
  ): Coroutine<context, state, Unit> =>
    ps.length <= 0
      ? Coroutine.Return<context, state, Unit>({})
      : ps[0].then(() => Coroutine.Seq<context, state>(ps.slice(1))),
  For: <context, state, element>(
    collection: Collection<number, element>,
    p: BasicFun<element, Coroutine<context, state, Unit>>,
  ): Coroutine<context, state, Unit> =>
    collection.isEmpty()
      ? Coroutine.Return({})
      : Coroutine.Seq([
          p(collection.first()!),
          Coroutine.For(collection.skip(1), p),
        ]),
  While: <context, state>(
    predicate: BasicFun<[context], boolean>,
    p: Coroutine<context, state, Unit>,
  ): Coroutine<context, state, Unit> =>
    Coroutine.Create(([context, deltaT]) => {
      if (predicate([context])) {
        return p.then((_) => Coroutine.While<context, state>(predicate, p))([
          context,
          deltaT,
        ]);
      } else return CoroutineStep.Result(undefined, {});
    }),
  Wait: <context, state>(ms: number): Coroutine<context, state, Unit> =>
    Coroutine.Waiting(ms, Coroutine.Return({})),
  Await: <context, state, result, error>(
    promise: BasicFun<Unit, Promise<result>>,
    onCatch: BasicFun<any, error>,
    debugName?: string,
  ): Coroutine<context, state, Sum<result, error>> =>
    Coroutine.Create(([_, __]) => {
      let promiseResult:
        | { kind: "resolve"; result: result }
        | { kind: "reject"; error: error }
        | undefined = undefined;
      // const started = Date.now();
      // if (SharedLayoutConstants.LogCoroutineTicks)
      //   console.log(`co::await::launched ${debugName}`);
      setTimeout(() =>
        promise({})
          .then((result) => {
            promiseResult = { kind: "resolve", result: result };
          })
          .catch((_) => {
            promiseResult = { kind: "reject", error: onCatch(_) };
          }),
      );
      // if (SharedLayoutConstants.LogCoroutineTicks)
      //   console.log(
      //     `co::await::creating awaiter ${debugName} (deltaT = ${
      //       Date.now() - started
      //     })`
      //   );
      const awaiter = (): CoroutineStep<context, state, Sum<result, error>> => {
        // if (SharedLayoutConstants.LogCoroutineTicks)
        //   console.log(
        //     `co::await::checking awaiter ${debugName}`,
        //     promiseResult
        //   );
        return promiseResult == undefined
          ? CoroutineStep.Yield(
              undefined,
              Coroutine.Create(([_, __]) => awaiter()),
            )
          : promiseResult.kind == "resolve"
            ? CoroutineStep.Result(
                undefined,
                Sum<result, error>().Default.left(promiseResult.result),
              )
            : CoroutineStep.Result(
                undefined,
                Sum<result, error>().Default.right(promiseResult.error),
              );
      };
      return CoroutineStep.Yield(
        undefined,
        Coroutine.Create(([_, __]) => awaiter()),
      );
    }),
  On: <
    eventKind,
    event,
    context extends { inboundEvents: Map<eventKind, OrderedMap<Guid, event>> },
    state extends { inboundEvents: Map<eventKind, OrderedMap<Guid, event>> },
    matchedEvent extends InboundKindFromContext<context>,
  >(
    kind: matchedEvent,
    filter?: BasicFun<
      [
        InboundEventFromContext<context & state> & { kind: matchedEvent },
        context & state,
      ],
      boolean
    >,
  ): Coroutine<
    context & state,
    state,
    InboundEventFromContext<context & state> & { kind: matchedEvent }
  > =>
    Coroutine.GetState<context & state, state>().then((context) => {
      // { inboundEvents:Map<InboundKindFromContext<c & s>, OrderedMap<Guid, InboundEventFromContext<c & s>>> }
      let inboundEvents = (context as any).inboundEvents ?? Map();
      let eventsOfKind = ((context as any).inboundEvents ?? Map()).get(
        kind as any,
      );
      if (eventsOfKind == undefined || eventsOfKind.isEmpty()) {
        return Coroutine.Yield<
          context & state,
          state,
          InboundEventFromContext<context & state> & { kind: matchedEvent }
        >(
          Coroutine.On<eventKind, event, context, state, matchedEvent>(
            kind,
            filter,
          ),
        );
      } else {
        const firstEventOfKind =
          filter == undefined
            ? eventsOfKind?.first()
            : eventsOfKind
                .filter((e: any) => {
                  return filter([e, context]);
                })
                .first();
        if (firstEventOfKind == undefined) {
          return Coroutine.Yield<
            context & state,
            state,
            InboundEventFromContext<context & state> & { kind: matchedEvent }
          >(
            Coroutine.On<eventKind, event, context, state, matchedEvent>(
              kind,
              filter,
            ),
          );
        }
        eventsOfKind = eventsOfKind.remove(firstEventOfKind.id);
        if (eventsOfKind.isEmpty()) inboundEvents = inboundEvents.remove(kind);
        else inboundEvents = inboundEvents.set(kind, eventsOfKind);
        return Coroutine.SetState<context & state, state>((_) => ({
          ..._,
          inboundEvents: inboundEvents,
        })).then(() =>
          Coroutine.Return<
            context & state,
            state,
            InboundEventFromContext<context & state> & { kind: matchedEvent }
          >(firstEventOfKind),
        );
      }
    }),
  Trigger: <
    context,
    state extends { outboundEvents: Map<kind, OrderedMap<Guid, event>> },
    event extends { id: Guid; kind: kind },
    kind extends string,
  >(
    event: event,
  ): Coroutine<context & state, state, Unit> =>
    Coroutine.SetState<context & state, state>((_) => ({
      ..._,
      outboundEvents: _.outboundEvents.set(
        event.kind,
        (_.outboundEvents.get(event.kind) ?? OrderedMap()).set(event.id, event),
      ),
    })),
  Do: <context, state>(
    action: SimpleCallback<void>,
  ): Coroutine<context, state, Unit> =>
    Coroutine.Create(([_, __]) => {
      action();
      return CoroutineStep.Yield(undefined, Coroutine.Return({}));
    }),
};

export type CoroutineStep<context, state, result> = {
  newState: BasicUpdater<state> | undefined;
} & (
  | { kind: "result"; result: result }
  | {
      kind: "then";
      p: Coroutine<context, state, any>;
      k: BasicFun<any, Coroutine<context, state, result>>;
    }
  | { kind: "yield"; next: Coroutine<context, state, result> }
  | {
      kind: "waiting";
      msLeft: number;
      next: Coroutine<context, state, result>;
    }
) & {
    // map: <nextResult>(
    //   f: Fun<result, nextResult>
    // ) => CoroutineStep<context, state, nextResult>;
    // mapState: (
    //   f: Fun<Updater<state> | undefined, Updater<state> | undefined>
    // ) => CoroutineStep<context, state, result>;
  };
export const CoroutineStep = {
  Result: <context, state, result>(
    newState: BasicUpdater<state> | undefined,
    result: result,
  ): CoroutineStep<context, state, result> => ({
    newState,
    kind: "result",
    result: result,
    // map: function <nextResult>(f: Fun<result, nextResult>) {
    //   return CoroutineStep.Map(this, f);
    // },
    // mapState: function (
    //   f: Fun<Updater<state> | undefined, Updater<state> | undefined>
    // ) {
    //   return CoroutineStep.MapState(this, f);
    // },
  }),
  Then: <context, state, result>(
    newState: BasicUpdater<state> | undefined,
    p: Coroutine<context, state, any>,
    k: BasicFun<any, Coroutine<context, state, result>>,
  ): CoroutineStep<context, state, result> => ({
    kind: "then",
    newState: newState,
    p: p,
    k: k,
    // map: function <nextResult>(f: Fun<result, nextResult>) {
    //   return CoroutineStep.Map(this, f);
    // },
    // mapState: function (
    //   f: Fun<Updater<state> | undefined, Updater<state> | undefined>
    // ) {
    //   return CoroutineStep.MapState(this, f);
    // },
  }),
  Yield: <context, state, result>(
    newState: BasicUpdater<state> | undefined,
    next: Coroutine<context, state, result>,
  ): CoroutineStep<context, state, result> => ({
    newState,
    kind: "yield",
    next: next,
    // map: function <nextResult>(f: Fun<result, nextResult>) {
    //   return CoroutineStep.Map(this, f);
    // },
    // mapState: function (
    //   f: Fun<Updater<state> | undefined, Updater<state> | undefined>
    // ) {
    //   return CoroutineStep.MapState(this, f);
    // },
  }),
  Waiting: <context, state, result>(
    newState: BasicUpdater<state> | undefined,
    msLeft: number,
    next: Coroutine<context, state, result>,
  ): CoroutineStep<context, state, result> => ({
    newState,
    kind: "waiting",
    msLeft: msLeft,
    next: next,
    // map: function <nextResult>(f: Fun<result, nextResult>) {
    //   return CoroutineStep.Map(this, f);
    // },
    // mapState: function (
    //   f: Fun<Updater<state> | undefined, Updater<state> | undefined>
    // ) {
    //   return CoroutineStep.MapState(this, f);
    // },
  }),
  // Map: <context, state, result1, result2>(
  //   p: CoroutineStep<context, state, result1>,
  //   f: Fun<result1, result2>
  // ): CoroutineStep<context, state, result2> =>
  //   p.kind == "result"
  //     ? CoroutineStep.Result(p.newState, f(p.result))
  //     : p.kind == "yield"
  //       ? CoroutineStep.Yield(p.newState, Coroutine.Map(p.next, f))
  //       : p.kind == "waiting"
  //         ? CoroutineStep.Waiting(p.newState, p.msLeft, Coroutine.Map(p.next, f))
  //         : CoroutineStep.WaitingForEvent(p.newState, (e) => {
  //             const next = p.next(e);
  //             return next == "no match" ? next : Coroutine.Map(next, f)
  //           }),
  MapContext: <childContext, parentContext, state, result>(
    p: CoroutineStep<childContext, state, result>,
    narrow: BasicFun<parentContext, childContext | undefined>,
  ): CoroutineStep<parentContext, state, result> =>
    p.kind == "result"
      ? p
      : p.kind == "yield"
        ? CoroutineStep.Yield(p.newState, Coroutine.MapContext(p.next, narrow))
        : p.kind == "waiting"
          ? CoroutineStep.Waiting(
              p.newState,
              p.msLeft,
              Coroutine.MapContext(p.next, narrow),
            )
          : CoroutineStep.Then(
              p.newState,
              Coroutine.MapContext(p.p, narrow),
              Fun(p.k).then((_) => Coroutine.MapContext(_, narrow)),
            ),
  MapState: <context, state, newState, result>(
    p: CoroutineStep<context, state, result>,
    f: BasicFun<BasicUpdater<state>, BasicUpdater<newState>>,
  ): CoroutineStep<context, newState, result> =>
    p.kind == "result"
      ? CoroutineStep.Result(callMaybe(f)(p.newState), p.result)
      : p.kind == "yield"
        ? CoroutineStep.Yield(
            callMaybe(f)(p.newState),
            Coroutine.MapState(p.next, f),
          )
        : p.kind == "waiting"
          ? CoroutineStep.Waiting(
              callMaybe(f)(p.newState),
              p.msLeft,
              Coroutine.MapState(p.next, f),
            )
          : CoroutineStep.Then(
              callMaybe(f)(p.newState),
              Coroutine.MapState(p.p, f),
              Fun(p.k).then((_) => Coroutine.MapState(_, f)),
            ),
  Embed: <childContext, childState, result, parentContext, parentState>(
    p: CoroutineStep<childContext, childState, result>,
    narrow: BasicFun<parentContext, childContext | undefined>,
    widen: BasicFun<BasicUpdater<childState>, BasicUpdater<parentState>>,
  ): CoroutineStep<parentContext, parentState, result> =>
    CoroutineStep.MapState(CoroutineStep.MapContext(p, narrow), widen),
};

export type InboundEventFromContext<s> = s extends {
  inboundEvents: Map<infer _kind, OrderedMap<Guid, infer event>>;
}
  ? event
  : never;

export type InboundKindFromContext<s> = s extends {
  inboundEvents: Map<infer kind, OrderedMap<Guid, infer _event>>;
}
  ? kind
  : never;
