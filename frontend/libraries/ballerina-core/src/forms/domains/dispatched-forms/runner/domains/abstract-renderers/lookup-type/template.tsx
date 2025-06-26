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
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import {
  LookupTypeAbstractRendererForeignMutationsExpected,
  LookupTypeAbstractRendererReadonlyContext,
  LookupTypeAbstractRendererState,
  LookupTypeAbstractRendererView,
} from "./state";

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
) => {
  return Template.Default<
    LookupTypeAbstractRendererReadonlyContext<CustomPresentationContext> &
      LookupTypeAbstractRendererState,
    LookupTypeAbstractRendererState,
    LookupTypeAbstractRendererForeignMutationsExpected<Flags>,
    LookupTypeAbstractRendererView<CustomPresentationContext, Flags>
  >((props) => {
    return (
      <>
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={props.context}
            foreignMutations={props.foreignMutations}
            embeddedTemplate={embeddedTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
