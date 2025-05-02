import {
  FormLabel,
  simpleUpdater,
  Template,
  Value,
  ValueSum,
  View,
  Bindings,
  simpleUpdaterWithChildren,
  CommonAbstractRendererState,
  CommonAbstractRendererReadonlyContext,
  PredicateValue,
} from "../../../../../../../../main";
import { DispatchOnChange } from "../../../state";

export type SumAbstractRendererReadonlyContext = Value<ValueSum> &
  CommonAbstractRendererReadonlyContext;

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
