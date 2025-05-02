import { DispatchDelta, FormLabel, Value } from "../../../../../../../../main";
import { replaceWith, Template } from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { DispatchOnChange } from "../../../state";
import {
  Base64FileAbstractRendererState,
  Base64FileAbstractRendererView,
} from "./state";

export const Base64FileAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>() => {
  return Template.Default<
    Context &
      Value<string> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    Base64FileAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<string> },
    Base64FileAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => (
    <span
      className={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
    >
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
    </span>
  )).any([]);
};
