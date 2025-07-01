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
  Option,
  Unit,
  StringSerializedType,
} from "../../../../../../../../main";
import React from "react";

export const StringAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
) => {
  return Template.Default<
    StringAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    >,
    StringAbstractRendererState,
    StringAbstractRendererForeignMutationsExpected<Flags>,
    StringAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const completeSerializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

    const domNodeId = props.context.domNodeAncestorPath + "[string]";

    if (!PredicateValue.Operations.IsString(props.context.value)) {
      console.error(
        `String expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: String value expected but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

    return (
      <>
        <IdProvider domNodeId={domNodeId}>
          <props.view
            {...props}
            context={{
              ...props.context,
              completeSerializedTypeHierarchy,
              domNodeId,
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
