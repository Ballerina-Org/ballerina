import { Map } from "immutable";
import {
  SimpleCallback,
  Value,
  CoTypedFactory,
  Debounced,
  Unit,
  ValueOrErrors,
  ValueRecord,
  BaseFlags,
  Sum,
  OneAbstractRendererForeignMutationsExpected,
  OneAbstractRendererReadonlyContext,
  OneAbstractRendererState,
} from "../../../../../../../../../main";

export const Co = <CustomPresentationContext = Unit, ExtraContext = Unit>() =>
  CoTypedFactory<
    OneAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext>,
    OneAbstractRendererState
  >();

export const InitializeCo = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  CoTypedFactory<
    OneAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      Pick<OneAbstractRendererForeignMutationsExpected, "onChange">,
    OneAbstractRendererState
  >();

export const DebouncerCo = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  CoTypedFactory<
    OneAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > & {
      onDebounce: SimpleCallback<void>;
    },
    OneAbstractRendererState
  >();

export const DebouncedCo = CoTypedFactory<
  { onDebounce: SimpleCallback<void> },
  Value<Map<string, string>>
>();
