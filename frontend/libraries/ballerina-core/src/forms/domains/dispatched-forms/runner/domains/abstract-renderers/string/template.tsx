import { List } from "immutable";
import {
  StringAbstractRendererReadonlyContext,
  StringAbstractRendererForeignMutationsExpected,
  StringAbstractRendererState,
  StringAbstractRendererView,
} from "./state";
import {
  DispatchDelta,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  Template,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  Option,
  Unit,
} from "../../../../../../../../main";
import React from "react";

export const StringAbstractRenderer = <CustomContext = Unit, Flags = Unit>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    StringAbstractRendererReadonlyContext<CustomContext>,
    StringAbstractRendererState,
    StringAbstractRendererForeignMutationsExpected<Flags>,
    StringAbstractRendererView<CustomContext, Flags>
  >((props) => {
    if (!PredicateValue.Operations.IsString(props.context.value)) {
      console.error(
        `String expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering string field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: String value expected for string but got ${JSON.stringify(
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
              setNewValue: (value, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind:
                    props.context.type.kind == "primitive" &&
                    props.context.type.name == "string"
                      ? "StringReplace"
                      : "GuidReplace",
                  replace: value,
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(replaceWith(value)),
                  delta,
                );
              },
            }}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
