import { Unit } from "../../../../../../../../../main";
import { SetupOnAfterChangeCo as Co } from "./builder";

export const setupOnAfterChange = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>()
    .GetState()
    .then((current) => {
      // this CO is run as soon as we have a lastOnChangePromise
      // (we are waiting for an onChange promise to be resolved)
      return Co<CustomPresentationContext, ExtraContext>().Do(() => {
        if (current.lastOnChangePromise.kind == "l") {
          return;
        }

        current.lastOnChangePromise.value
          .then((_) => {
            if (current.getApi != undefined) {
              // lazy one -> reinitialize it when the onChange promise resolves
              // we shouldn't reset the refetch handlers here since we are still waiting
              // for the updated one value to come in
              // and we want to keep the latest (local) change in the state
              // we will reset the refetch handlers in the initialize one coroutine
              current.reinitializeLazyOne();
            }
          })
          .catch((_) => {
            // on error -> reset the refetch handlers, regardless of whether the one is lazy or not
            current.resetLazyOneRefetchHandlers();
          })
          .finally(() => {
            if (current.getApi == undefined) {
              // if the one is not lazy, it means we already have the correct value in the state
              // and we do not need to refetch anything
              // so we can reset the refetch handlers here
              current.resetLazyOneRefetchHandlers();
            }
          });
      });
    });
