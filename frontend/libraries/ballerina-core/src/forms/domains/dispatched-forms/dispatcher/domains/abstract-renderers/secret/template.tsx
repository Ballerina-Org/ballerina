import { List } from "immutable";
import {
  SecretAbstractRendererState,
  SecretAbstractRendererView,
} from "./state";
import {
  DispatchDelta,
  FormLabel,
  replaceWith,
  Template,
  Value,
} from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { DispatchOnChange } from "../../../state";

export const SecretAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>() => {
  return Template.Default<
    Context &
      Value<string> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
      },
    SecretAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<string> },
    SecretAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => (
    <>
      <props.view
        {...props}
        foreignMutations={{
          ...props.foreignMutations,
          setNewValue: (_) => {
            const delta: DispatchDelta = {
              kind: "StringReplace",
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
