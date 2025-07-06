import { ValueInfiniteStreamState } from "../state";
import { ValueStreamCo } from "./builder";
import { ValueInfiniteStreamLoader } from "./infiniteLoader";

export const ValueStreamDataLoader = <foreignMutations>() =>
  ValueStreamCo().Template<foreignMutations>(ValueInfiniteStreamLoader(), {
    runFilter: (props) =>
      ValueInfiniteStreamState.Operations.shouldCoroutineRun(props.context),
  });
