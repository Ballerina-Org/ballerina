import { OrderedMap } from "immutable";
import { Guid, Unit, SynchronizationOperationResult } from "../../main";
import { CoTypedFactory } from "../coroutines/builder";
import { Coroutine } from "../coroutines/state";
import { BasicFun } from "../fun/state";

export const QueueCoroutine = <
  Context,
  State,
  OperationResult extends
    SynchronizationOperationResult = SynchronizationOperationResult,
>(
  removeItem: BasicFun<Guid, Coroutine<Context & State, State, Unit>>,
  getItemsToProcess: BasicFun<
    Context & State,
    OrderedMap<
      Guid,
      {
        preprocess: Coroutine<Context & State, State, Unit>;
        operation: Coroutine<Context & State, State, OperationResult>;
        postprocess: BasicFun<
          OperationResult,
          Coroutine<Context & State, State, Unit>
        >;
        reenqueue: BasicFun<
          OperationResult,
          Coroutine<Context & State, State, Unit>
        >;
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
                k.postprocess(_).then(() => {
                  if (_.kind == "should be enqueued again") {
                    return k.reenqueue(_);
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
