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
  ReferenceOneAbstractRendererForeignMutationsExpected,
  ReferenceOneAbstractRendererReadonlyContext,
  ReferenceOneAbstractRendererState,
} from "../../../../../../../../../main";

export const Co = <CustomPresentationContext = Unit, ExtraContext = Unit>() =>
  CoTypedFactory<
    ReferenceOneAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext>,
    ReferenceOneAbstractRendererState
  >();

export const InitializeCo = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  CoTypedFactory<
    ReferenceOneAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      Pick<ReferenceOneAbstractRendererForeignMutationsExpected, "onChange">,
    ReferenceOneAbstractRendererState
  >();

export const DebouncerCo = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  CoTypedFactory<
    ReferenceOneAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > & {
      onDebounce: SimpleCallback<void>;
    },
    ReferenceOneAbstractRendererState
  >();

export const DebouncedCo = CoTypedFactory<
  { onDebounce: SimpleCallback<void> },
  Value<[Map<string, string>, boolean]>
>();
