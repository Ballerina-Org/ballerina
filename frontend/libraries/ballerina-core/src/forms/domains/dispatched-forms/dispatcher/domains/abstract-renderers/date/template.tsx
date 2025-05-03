import { Value } from "../../../../../../../value/state";
import { Template } from "../../../../../../../template/state";
import {
  DispatchDelta,
  FormLabel,
  PredicateValue,
  replaceWith,
} from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { DispatchOnChange } from "../../../state";
import { DateAbstractRendererState, DateAbstractRendererView } from "./state";

export const DateAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>() => {
  return Template.Default<
    Context &
      Value<Date> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    DateAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<Date> },
    DateAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => {
    if (!PredicateValue.Operations.IsDate(props.context.value)) {
      console.error(
        `Date expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering date field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <p>
          {props.context.label && `${props.context.label}: `}RENDER ERROR: Date
          value expected for date but got something else
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
              props.setState(
                DateAbstractRendererState.Updaters.Core.customFormState.children.possiblyInvalidInput(
                  replaceWith(_),
                ),
              );
              const newValue = _ == undefined ? _ : new Date(_);

              if (!(newValue == undefined || isNaN(newValue.getTime()))) {
                const delta: DispatchDelta = {
                  kind: "TimeReplace",
                  replace: newValue.toISOString(),
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                };
                setTimeout(() => {
                  props.foreignMutations.onChange(replaceWith(newValue), delta);
                }, 0);
              }
            },
          }}
        />
      </span>
    );
  }).any([]);
};
