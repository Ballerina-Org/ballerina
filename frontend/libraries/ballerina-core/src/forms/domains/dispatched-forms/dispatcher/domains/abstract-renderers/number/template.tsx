import {
  DispatchDelta,
  FormLabel,
  replaceWith,
  Template,
  Value,
} from "../../../../../../../../main";
import { DispatchOnChange } from "../../../state";
import {
  NumberAbstractRendererState,
  NumberAbstractRendererView,
} from "./state";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";

export const NumberAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>() => {
  return Template.Default<
    Context &
      Value<number> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
      },
    NumberAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<number> },
    NumberAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => (
    <>
      <props.view
        {...props}
        foreignMutations={{
          ...props.foreignMutations,
          setNewValue: (_) => {
            const delta: DispatchDelta = {
              kind: "NumberReplace",
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
