import {
  UnitAbstractRendererReadonlyContext,
  UnitAbstractRendererState,
  UnitAbstractRendererView,
  UnitAbstractRendererForeignMutationsExpected,
} from "./state";
import {
  DispatchDelta,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  IdWrapperProps,
  PredicateValue,
  Template,
  Unit,
  Option,
  replaceWith,
  StringSerializedType,
} from "../../../../../../../../main";

export const UnitAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
) =>
  Template.Default<
    UnitAbstractRendererReadonlyContext<CustomPresentationContext> &
      UnitAbstractRendererState,
    UnitAbstractRendererState,
    UnitAbstractRendererForeignMutationsExpected<Flags>,
    UnitAbstractRendererView<CustomPresentationContext, Flags>
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

    const serializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

    return (
      <>
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId: props.context.identifiers.withoutLauncher,
              serializedTypeHierarchy,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              set: (flags: Flags | undefined) => {
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
