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
  Option,
  replaceWith,
  unit,
} from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";

export const UnitAbstractRenderer = <Context extends FormLabel, Flags = Unit>(
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
    { onChange: DispatchOnChange<ValueUnit, Flags> },
    UnitAbstractRendererView<Context, Flags>
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
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId: props.context.identifiers.withoutLauncher,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              set: (flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "UnitReplace",
                  replace: PredicateValue.Default.unit(),
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(
                    replaceWith(PredicateValue.Default.unit()),
                  ),
                  delta,
                );
              },
            }}
          />
        </IdProvider>
      </>
    );
  }).any([]);
