import { List } from "immutable";
import {
  SecretAbstractRendererState,
  SecretAbstractRendererView,
} from "./state";
import {
  DispatchDelta,
  FormLabel,
  PredicateValue,
  replaceWith,
  Template,
  Value,
  DispatchOnChange,
} from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";

export const SecretAbstractRenderer = <
  Context extends FormLabel & {
    identifiers: { withLauncher: string; withoutLauncher: string };
  },
  ForeignMutationsExpected,
>() => {
  return Template.Default<
    Context &
      Value<string> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    SecretAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<string> },
    SecretAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => {
    if (!PredicateValue.Operations.IsString(props.context.value)) {
      console.error(
        `String expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering secret field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <p>
          {props.context.label && `${props.context.label}: `}RENDER ERROR:
          String value expected for secret but got something else
        </p>
      );
    }
    return (
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
    );
  }).any([]);
};
