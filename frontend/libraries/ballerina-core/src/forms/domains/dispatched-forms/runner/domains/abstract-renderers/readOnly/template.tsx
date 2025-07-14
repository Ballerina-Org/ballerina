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
import {
  DispatchParsedType,
  ReadOnlyType,
} from "../../../../deserializer/domains/specification/domains/types/state";
import { ReadOnlyAbstractRendererView } from "./state";

export const ReadOnlyAbstractRenderer = <
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
  readOnlyType: ReadOnlyType<any>,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  _ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    CommonAbstractRendererReadonlyContext<
      ReadOnlyType<any>,
      PredicateValue,
      CustomPresentationContext,
      ExtraContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>,
    ReadOnlyAbstractRendererView<
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
              readOnlyType: readOnlyType,
            }}
            embeddedTemplate={embeddedTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
}; 