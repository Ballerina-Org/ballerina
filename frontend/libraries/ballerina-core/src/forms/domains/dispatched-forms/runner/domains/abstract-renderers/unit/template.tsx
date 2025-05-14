import { UnitAbstractRendererState, UnitAbstractRendererView } from "./state";
import {
  DispatchDelta,
  DispatchOnChange,
  ErrorRendererProps,
  FormLabel,
  getLeafIdentifierFromIdentifier,
  IdWrapperProps,
  PredicateValue,
  Template,
  Unit,
  ValueUnit,
} from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";

export const UnitAbstractRenderer = <Context extends FormLabel>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) =>
  Template.Default<
    Context & {
      value: ValueUnit;
      disabled: boolean;
      type: DispatchParsedType<any>;
      identifiers: { withLauncher: string; withoutLauncher: string };
    },
    UnitAbstractRendererState,
    { onChange: DispatchOnChange<Unit> },
    UnitAbstractRendererView<Context>
  >((props) => {
    if (!PredicateValue.Operations.IsUnit(props.context.value)) {
      console.error(
        `Unit expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering unit field with label: ${JSON.stringify(
          props.context?.label,
        )}`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Unit value expected but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }
    return (
      <>
        <IdProvider
          id={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
        />
        <props.view
          {...props}
          foreignMutations={{
            ...props.foreignMutations,
            onChange: (_) => {
              const delta: DispatchDelta = {
                kind: "UnitReplace",
                replace: PredicateValue.Default.unit(),
                state: {
                  commonFormState: props.context.commonFormState,
                  customFormState: props.context.customFormState,
                },
                type: props.context.type,
              };
              props.foreignMutations.onChange(_, delta);
            },
          }}
        />
      </>
    );
  }).any([]);
