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
  CommonAbstractRendererViewOnlyReadonlyContext,
  ValueUnit,
} from "../../../../../../../../main";
import { SumType } from "../../../../deserializer/domains/specification/domains/types/state";

export type SumAbstractRendererReadonlyContext<
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = CommonAbstractRendererReadonlyContext<
  SumType<any>,
  ValueSum | ValueUnit,
  CustomPresentationContext,
  ExtraContext
>;

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

export type SumAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  SumAbstractRendererReadonlyContext<CustomPresentationContext, ExtraContext> &
    SumAbstractRendererState &
    CommonAbstractRendererViewOnlyReadonlyContext,
  SumAbstractRendererState,
  SumAbstractRendererViewForeignMutationsExpected<Flags>,
  {
    embeddedLeftTemplate?: (
      flags: Flags | undefined,
    ) => Template<
      SumAbstractRendererReadonlyContext<
        CustomPresentationContext,
        ExtraContext
      > &
        SumAbstractRendererState,
      SumAbstractRendererState,
      SumAbstractRendererForeignMutationsExpected<Flags>
    >;

    embeddedRightTemplate?: (
      flags: Flags | undefined,
    ) => Template<
      SumAbstractRendererReadonlyContext<
        CustomPresentationContext,
        ExtraContext
      > &
        SumAbstractRendererState,
      SumAbstractRendererState,
      SumAbstractRendererForeignMutationsExpected<Flags>
    >;
  }
>;
