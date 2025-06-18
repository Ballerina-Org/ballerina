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

export type SumAbstractRendererReadonlyContext<CustomContext = Unit> =
  CommonAbstractRendererReadonlyContext<SumType<any>, ValueSum, CustomContext>;

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

export type SumAbstractRendererView<CustomContext = Unit, Flags = Unit> = View<
  SumAbstractRendererReadonlyContext<CustomContext> & SumAbstractRendererState,
  SumAbstractRendererState,
  SumAbstractRendererViewForeignMutationsExpected<Flags>,
  {
    embeddedLeftTemplate?: (
      flags: Flags | undefined,
    ) => Template<
      SumAbstractRendererReadonlyContext<CustomContext> &
        SumAbstractRendererState,
      SumAbstractRendererState,
      SumAbstractRendererForeignMutationsExpected<Flags>
    >;

    embeddedRightTemplate?: (
      flags: Flags | undefined,
    ) => Template<
      SumAbstractRendererReadonlyContext<CustomContext> &
        SumAbstractRendererState,
      SumAbstractRendererState,
      SumAbstractRendererForeignMutationsExpected<Flags>
    >;
  }
>;
