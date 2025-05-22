import { Value } from "../../../../../../../value/state";
import { Template } from "../../../../../../../template/state";
import {
  DispatchDelta,
  FormLabel,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  DispatchOnChange,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
} from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { DateAbstractRendererState, DateAbstractRendererView } from "./state";

export const DateAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
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
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Date value expected for date but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }
    return (
      <>
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId: props.context.identifiers.withoutLauncher,
            }}
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
                    isWholeEntityMutation: false,
                  };
                  setTimeout(() => {
                    props.foreignMutations.onChange(
                      replaceWith(newValue),
                      delta,
                    );
                  }, 0);
                }
              },
            }}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
