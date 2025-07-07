import { OrderedMap } from "immutable";
import { Guid, Unit, SynchronizationResult } from "../../main";
import { CoTypedFactory } from "../coroutines/builder";
import { Coroutine } from "../coroutines/state";
import { BasicFun } from "../fun/state";

export const QueueCoroutine = <Context, State>(
  removeItem: BasicFun<Guid, Coroutine<Context & State, State, Unit>>,
  getItemsToProcess: BasicFun<
    Context & State,
    OrderedMap<
      Guid,
      {
        preprocess: Coroutine<Context & State, State, Unit>;
        operation: Coroutine<Context & State, State, SynchronizationResult>;
        postprocess: BasicFun<
          SynchronizationResult,
          Coroutine<Context & State, State, Unit>
        >;
        reenqueue: Coroutine<Context & State, State, Unit>;
      }
    >
  >,
): Coroutine<Context & State, State, Unit> => {
  const Co = CoTypedFactory<Context, State>();

  return Co.Repeat(
    Co.GetState().then((current) => {
      let operations = getItemsToProcess(current);

      return Co.Seq([
        Co.All(
          operations.toArray().map(([id, k]) =>
            k.preprocess
              .then(() => k.operation)
              .then((_) =>
                // alert(`${JSON.stringify(k)} => ${_}\n`)
                k.postprocess(_).then(() => {
                  if (_ == "should be enqueued again") {
                    return k.reenqueue;
                  } else {
                    return Co.Return({});
                  }
                }),
              )
              .then(() => removeItem(id)),
          ),
        ),
      ]);
    }),
  );
};
