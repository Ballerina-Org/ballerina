import {
  FormLabel,
  simpleUpdater,
  Template,
  Value,
  ValueSum,
  View,
  Bindings,
  BasicUpdater,
  simpleUpdaterWithChildren,
} from "../../../../../../../../main";
import { DispatchCommonFormState } from "../../../../built-ins/state";
import { DispatchOnChange } from "../../../state";

export type SumAbstractRendererState<LeftFormState, RightFormState> = {
  commonFormState: DispatchCommonFormState;
} & {
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
    commonFormState: DispatchCommonFormState.Default(),
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
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<ValueSum> &
    SumAbstractRendererState<LeftFormState, RightFormState>,
  SumAbstractRendererState<LeftFormState, RightFormState>,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueSum>;
  },
  {
    embeddedLeftTemplate?: Template<
      Context &
        Value<ValueSum> &
        SumAbstractRendererState<LeftFormState, RightFormState> & {
          bindings: Bindings;
          extraContext: any;
        },
      SumAbstractRendererState<LeftFormState, RightFormState>,
      ForeignMutationsExpected & {
        onChange: DispatchOnChange<ValueSum>;
      }
    >;

    embeddedRightTemplate?: Template<
      Context &
        Value<ValueSum> &
        SumAbstractRendererState<LeftFormState, RightFormState> & {
          bindings: Bindings;
          extraContext: any;
        },
      SumAbstractRendererState<LeftFormState, RightFormState>,
      ForeignMutationsExpected & {
        onChange: DispatchOnChange<ValueSum>;
      }
    >;
  }
>;
