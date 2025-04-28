import { Value } from "../../../../../../../value/state";
import { Template } from "../../../../../../../template/state";
import {
  DispatchDelta,
  FormLabel,
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
      },
    DateAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<Date> },
    DateAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => (
    <>
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
    </>
  )).any([]);
};
