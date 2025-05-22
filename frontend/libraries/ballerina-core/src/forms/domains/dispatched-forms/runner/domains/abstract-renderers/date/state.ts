import {
  FormLabel,
  simpleUpdaterWithChildren,
  simpleUpdater,
  View,
  Value,
  SimpleCallback,
  DispatchCommonFormState,
  DispatchOnChange,
  DomNodeIdReadonlyContext,
} from "../../../../../../../../main";
import { Maybe } from "../../../../../../../collections/domains/maybe/state";

export type DateAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: { possiblyInvalidInput: Maybe<string> };
};

export const DateAbstractRendererState = {
  Default: (): DateAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: { possiblyInvalidInput: Maybe.Default(undefined) },
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<DateAbstractRendererState>()("commonFormState"),
      ...simpleUpdaterWithChildren<DateAbstractRendererState>()({
        ...simpleUpdater<DateAbstractRendererState["customFormState"]>()(
          "possiblyInvalidInput",
        ),
      })("customFormState"),
    },
  },
};
export type DateAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<Maybe<Date>> &
    DomNodeIdReadonlyContext &
    DateAbstractRendererState & { disabled: boolean },
  DateAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<Date>;
    setNewValue: SimpleCallback<Maybe<string>>;
  }
>;
