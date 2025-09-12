import { Map } from "immutable";
import {
  Unit,
  Debounce,
  SimpleCallback,
  Value,
} from "../../../../../../../../../main";
import { OneAbstractRendererState } from "../state";
import { DebouncerCo, DebouncedCo } from "./builder";

export const debouncer = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  DebouncerCo<CustomPresentationContext, ExtraContext>().Repeat(
    DebouncerCo<CustomPresentationContext, ExtraContext>().Seq([
      Debounce<
        Value<[Map<string, string>, boolean]>,
        { onDebounce: SimpleCallback<void> }
      >(
        DebouncedCo.GetState()
          .then((current) => DebouncedCo.Do(() => current.onDebounce()))
          //.SetState(SearchNow.Updaters.reloadsRequested(_ => _ + 1))
          .then((_) => DebouncedCo.Return("success")),
        250,
      ).embed(
        (_) => ({
          ..._.customFormState.streamParams,
          onDebounce: _.onDebounce,
        }),
        OneAbstractRendererState.Updaters.Core.customFormState.children
          .streamParams,
      ),
      DebouncerCo<CustomPresentationContext, ExtraContext>().Wait(0),
    ]),
  );
