import { BasicFun } from "../../../fun/state";

export const PromiseRepo = {
  Default: {
    mock: <T>(
      value: BasicFun<void, T>,
      error: BasicFun<void, any> = () => "error",
      probabilityOfSuccess: number = 1,
      averageDelay: number = 0,
    ) => {
      const λ = 1 / averageDelay;
      const delay = Math.log(1 - Math.random()) / -λ; // sample the exponential distribution
      return new Promise<T>((resolve, reject) =>
        setTimeout(
          () =>
            Math.random() <= probabilityOfSuccess
              ? resolve(value())
              : reject(error()),
          delay * 1000,
        ),
      );
    },
  },
};
