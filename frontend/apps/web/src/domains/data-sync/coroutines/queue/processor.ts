import { Coroutine, Unit, QueueCoroutine } from "ballerina-core";
import { v4 } from "uuid";
import {
  DataSyncReadonlyContext,
  DataSyncWritableState,
  DataSync,
} from "../../state";
import { Co } from "../builder";

export const QueueProcessor = (): Coroutine<
  DataSyncReadonlyContext & DataSyncWritableState,
  DataSyncWritableState,
  Unit
> => {
  return QueueCoroutine<DataSyncReadonlyContext, DataSyncWritableState>(
    (workerId) => Co.SetState(DataSync().Updaters.Core.queue.remove(workerId)),
    (context) => {
      let workersToUpdateNow = context.queue.take(1);
      context.queue
        .skip(1)
        .take(10)
        .forEach((value, key) => {
          if (!workersToUpdateNow.find((_) => _.entityId == value.entityId)) {
            workersToUpdateNow = workersToUpdateNow.set(key, value);
            if (workersToUpdateNow.count() >= 3) return false;
          }
        });
      return workersToUpdateNow.map((k) => ({
        preprocess: k.dirtySetter("dirty but being processed"),
        operation: k.operation.then(() => Co.Return({ kind: "completed" })),
        postprocess: (_) =>
          _.kind == "completed"
            ? Co.GetState().then((current) =>
                current.queue.count(
                  (mutation) => mutation.entityId == k.entityId,
                ) <= 1
                  ? k.dirtySetter("not dirty").then(() => Co.Return({}))
                  : Co.Return({}),
              )
            : Co.Return({}), // BasicFun<SynchronizationResult, Coroutine<Context & State, State, Unit>>
        reenqueue: Co.SetState(DataSync().Updaters.Core.queue.add([v4(), k])),
      }));
    },
  );
};
