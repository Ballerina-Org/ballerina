import React from "react";
import {
  IdWrapperProps,
  PredicateValue,
  ErrorRendererProps,
  Unit,
  CommonAbstractRendererState,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererForeignMutationsExpected,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { LookupTypeAbstractRendererView } from "./state";

export const LookupTypeAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  embeddedTemplate: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<any>,
      PredicateValue,
      CustomPresentationContext,
      ExtraContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>
  >,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  _ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<any>,
      PredicateValue,
      CustomPresentationContext,
      ExtraContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>,
    LookupTypeAbstractRendererView<
      CustomPresentationContext,
      Flags,
      ExtraContext
    >
  >((props) => {
    return (
      <>
        <IdProvider domNodeId={props.context.domNodeAncestorPath}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId: props.context.domNodeAncestorPath,
            }}
            embeddedTemplate={embeddedTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
