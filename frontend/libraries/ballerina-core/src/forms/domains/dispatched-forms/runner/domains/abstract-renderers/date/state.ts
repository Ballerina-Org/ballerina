import {
  simpleUpdaterWithChildren,
  simpleUpdater,
  View,
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  Unit,
  CommonAbstractRendererReadonlyContext,
  DispatchPrimitiveType,
  CommonAbstractRendererState,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";
import { Maybe } from "../../../../../../../collections/domains/maybe/state";

export type DateAbstractRendererReadonlyContext<CustomPresentationContext> =
  CommonAbstractRendererReadonlyContext<
    DispatchPrimitiveType<any>,
    Date,
    CustomPresentationContext
  >;

export type DateAbstractRendererState = CommonAbstractRendererState & {
  customFormState: { possiblyInvalidInput: Maybe<string> };
};

export const DateAbstractRendererState = {
  Default: (): DateAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
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

export type DateAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<Date, Flags>;
  setNewValue: ValueCallbackWithOptionalFlags<Maybe<string>, Flags>;
};

export type DateAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
> = View<
  DateAbstractRendererReadonlyContext<CustomPresentationContext> &
    DateAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  DateAbstractRendererState,
  DateAbstractRendererForeignMutationsExpected<Flags>
>;
