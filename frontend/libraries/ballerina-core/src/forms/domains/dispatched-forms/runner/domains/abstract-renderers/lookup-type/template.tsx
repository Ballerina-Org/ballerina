import React from "react";
import {
  IdWrapperProps,
  PredicateValue,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  Unit,
  CommonAbstractRendererState,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererForeignMutationsExpected,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";
import {
  DispatchParsedType,
  StringSerializedType,
} from "../../../../deserializer/domains/specification/domains/types/state";
import { LookupTypeAbstractRendererView } from "./state";

export const LookupTypeAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
>(
  embeddedTemplate: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<any>,
      PredicateValue,
      CustomPresentationContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>
  >,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  _ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
) => {
  return Template.Default<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<any>,
      PredicateValue,
      CustomPresentationContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>,
    LookupTypeAbstractRendererView<CustomPresentationContext, Flags>
  >((props) => {
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
              serializedTypeHierarchy,
            }}
            embeddedTemplate={embeddedTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
