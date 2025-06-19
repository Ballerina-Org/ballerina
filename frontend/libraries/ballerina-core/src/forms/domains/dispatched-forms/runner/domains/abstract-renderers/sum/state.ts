import {
  simpleUpdater,
  Template,
  ValueSum,
  View,
  simpleUpdaterWithChildren,
  CommonAbstractRendererState,
  CommonAbstractRendererReadonlyContext,
  DispatchOnChange,
  Unit,
} from "../../../../../../../../main";
import { SumType } from "../../../../deserializer/domains/specification/domains/types/state";

export type SumAbstractRendererReadonlyContext<CustomPresentationContext = Unit> =
  CommonAbstractRendererReadonlyContext<SumType<any>, ValueSum, CustomPresentationContext>;

export type SumAbstractRendererState = CommonAbstractRendererState & {
  customFormState: {
    left: CommonAbstractRendererState;
    right: CommonAbstractRendererState;
  };
};

export const SumAbstractRendererState = {
  Default: (
    customFormState: SumAbstractRendererState["customFormState"],
  ): SumAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
    customFormState,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<SumAbstractRendererState>()("commonFormState"),
      ...simpleUpdaterWithChildren<SumAbstractRendererState>()({
        ...simpleUpdater<SumAbstractRendererState["customFormState"]>()("left"),
        ...simpleUpdater<SumAbstractRendererState["customFormState"]>()(
          "right",
        ),
      })("customFormState"),
    },
    Template: {},
  },
};

export type SumAbstractRendererForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<ValueSum, Flags>;
};

export type SumAbstractRendererViewForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<ValueSum, Flags>;
};

export type SumAbstractRendererView<CustomPresentationContext = Unit, Flags = Unit> = View<
  SumAbstractRendererReadonlyContext<CustomPresentationContext> & SumAbstractRendererState,
  SumAbstractRendererState,
  SumAbstractRendererViewForeignMutationsExpected<Flags>,
  {
    embeddedLeftTemplate?: (
      flags: Flags | undefined,
    ) => Template<
      SumAbstractRendererReadonlyContext<CustomPresentationContext> &
        SumAbstractRendererState,
      SumAbstractRendererState,
      SumAbstractRendererForeignMutationsExpected<Flags>
    >;

    embeddedRightTemplate?: (
      flags: Flags | undefined,
    ) => Template<
      SumAbstractRendererReadonlyContext<CustomPresentationContext> &
        SumAbstractRendererState,
      SumAbstractRendererState,
      SumAbstractRendererForeignMutationsExpected<Flags>
    >;
  }
>;
