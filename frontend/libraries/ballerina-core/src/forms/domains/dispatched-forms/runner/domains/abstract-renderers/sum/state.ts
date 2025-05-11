import {
  simpleUpdater,
  Template,
  Value,
  ValueSum,
  View,
  simpleUpdaterWithChildren,
  CommonAbstractRendererState,
  CommonAbstractRendererReadonlyContext,
  DispatchOnChange,
} from "../../../../../../../../main";
import { SumType } from "../../../../deserializer/domains/specification/domains/types/state";

export type SumAbstractRendererReadonlyContext =
  CommonAbstractRendererReadonlyContext<SumType<any>, ValueSum>;

export type SumAbstractRendererState<LeftFormState, RightFormState> =
  CommonAbstractRendererState & {
    customFormState: {
      left: LeftFormState;
      right: RightFormState;
    };
  };

export const SumAbstractRendererState = <LeftFormState, RightFormState>() => ({
  Default: (
    customFormState: SumAbstractRendererState<
      LeftFormState,
      RightFormState
    >["customFormState"],
  ): SumAbstractRendererState<LeftFormState, RightFormState> => ({
    ...CommonAbstractRendererState.Default(),
    customFormState,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<
        SumAbstractRendererState<LeftFormState, RightFormState>
      >()("commonFormState"),
      ...simpleUpdaterWithChildren<
        SumAbstractRendererState<LeftFormState, RightFormState>
      >()({
        ...simpleUpdater<
          SumAbstractRendererState<
            LeftFormState,
            RightFormState
          >["customFormState"]
        >()("left"),
        ...simpleUpdater<
          SumAbstractRendererState<
            LeftFormState,
            RightFormState
          >["customFormState"]
        >()("right"),
      })("customFormState"),
    },
    Template: {},
  },
});
export type SumAbstractRendererView<
  LeftFormState,
  RightFormState,
  SumAbstractRendererReadonlyContext,
  ForeignMutationsExpected,
> = View<
  SumAbstractRendererReadonlyContext &
    Value<ValueSum> &
    SumAbstractRendererState<LeftFormState, RightFormState>,
  SumAbstractRendererState<LeftFormState, RightFormState>,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueSum>;
  },
  {
    embeddedLeftTemplate?: Template<
      SumAbstractRendererReadonlyContext &
        Value<ValueSum> &
        SumAbstractRendererState<LeftFormState, RightFormState>,
      SumAbstractRendererState<LeftFormState, RightFormState>,
      ForeignMutationsExpected & {
        onChange: DispatchOnChange<ValueSum>;
      }
    >;

    embeddedRightTemplate?: Template<
      SumAbstractRendererReadonlyContext &
        Value<ValueSum> &
        SumAbstractRendererState<LeftFormState, RightFormState>,
      SumAbstractRendererState<LeftFormState, RightFormState>,
      ForeignMutationsExpected & {
        onChange: DispatchOnChange<ValueSum>;
      }
    >;
  }
>;
