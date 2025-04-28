import { BoolAbstractRendererView } from "./state";
import { Template } from "../../../../../../../template/state";
import { DispatchDelta, FormLabel, Value } from "../../../../../../../../main";
import { replaceWith } from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { DispatchOnChange } from "../../../state";
import { BoolAbstractRendererState } from "./state";

export const BoolAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>() => {
  return Template.Default<
    Context &
      Value<boolean> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
      },
    BoolAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<boolean> },
    BoolAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => (
    <>
      <props.view
        {...props}
        foreignMutations={{
          ...props.foreignMutations,
          setNewValue: (_) => {
            const delta: DispatchDelta = {
              kind: "BoolReplace",
              replace: _,
              state: {
                commonFormState: props.context.commonFormState,
                customFormState: props.context.customFormState,
              },
              type: props.context.type,
            };
            props.foreignMutations.onChange(replaceWith(_), delta);
          },
        }}
      />
    </>
  )).any([]);
};
