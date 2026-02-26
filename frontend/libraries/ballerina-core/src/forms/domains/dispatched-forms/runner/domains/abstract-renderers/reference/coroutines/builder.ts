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
  ReferenceAbstractRendererForeignMutationsExpected,
  ReferenceAbstractRendererReadonlyContext,
  ReferenceAbstractRendererState,
} from "../../../../../../../../../main";

export const Co = <CustomPresentationContext = Unit, ExtraContext = Unit>() =>
  CoTypedFactory<
    ReferenceAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext>,
    ReferenceAbstractRendererState
  >();

export const InitializeCo = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  CoTypedFactory<
    ReferenceAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      Pick<ReferenceAbstractRendererForeignMutationsExpected, "onChange">,
    ReferenceAbstractRendererState
  >();

export const DebouncerCo = <
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
>() =>
  CoTypedFactory<
    ReferenceAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > & {
      onDebounce: SimpleCallback<void>;
    },
    ReferenceAbstractRendererState
  >();

export const DebouncedCo = CoTypedFactory<
  { onDebounce: SimpleCallback<void> },
  Value<[Map<string, string>, boolean]>
>();
