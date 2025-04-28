import {
  BasicFun,
  DispatchCommonFormState,
  FormLabel,
  simpleUpdater,
  Template,
  Value,
  ValueUnionCase,
  View,
} from "../../../../../../../../main";
import { DispatchOnChange } from "../../../state";

export type UnionAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
} & {
  customFormState: {
    selectedCase: string;
    caseState: any;
  };
};

export const UnionAbstractRendererState = () => ({
  Default: (
    customFormState: UnionAbstractRendererState["customFormState"],
  ): UnionAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState,
  }),
  Updaters: {
    Core: {
      ...simpleUpdater<UnionAbstractRendererState>()("customFormState"),
    },
    Template: {},
  },
});
export type UnionAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context & Value<ValueUnionCase> & UnionAbstractRendererState,
  UnionAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueUnionCase>;
  },
  {
    embeddedCaseTemplate: BasicFun<
      string,
      Template<
        Context & Value<ValueUnionCase> & UnionAbstractRendererState,
        UnionAbstractRendererState,
        ForeignMutationsExpected & {
          onChange: DispatchOnChange<ValueUnionCase>;
        }
      >
    >;
  }
>;
