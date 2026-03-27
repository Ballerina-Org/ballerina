import { Unit } from "../../../../../../../../../main";
import { SetupLazyOneRefetchCo as Co } from "./builder";

export const setupLazyOneRefetch = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  Co<CustomPresentationContext, ExtraContext>()
    .GetState()
    .then((current) => {
      return Co<CustomPresentationContext, ExtraContext>().Do(() => {
        if (current.lastOnChangePromise.kind == "l") {
          current.resetLazyOneRefetchHandlers();
          return;
        }

        current.lastOnChangePromise.value
          .then((_) => {
            current.reinitializeLazyOne();
          })
          // TODO: handle error case?
          .finally(() => {
            current.resetLazyOneRefetchHandlers();
          });
      });
    });
