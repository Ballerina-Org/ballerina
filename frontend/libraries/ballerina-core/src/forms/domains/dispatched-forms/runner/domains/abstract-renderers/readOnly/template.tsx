import React from "react";
import {
  IdWrapperProps,
  PredicateValue,
  ErrorRendererProps,
  Unit,
  CommonAbstractRendererState,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererForeignMutationsExpected,
  DispatchOnChange,
  Option,
  BasicUpdater,
  DispatchDelta,
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
    const configuredChildTemplate = embeddedTemplate
      .mapContext(
        (
          _: CommonAbstractRendererReadonlyContext<
            DispatchParsedType<any>,
            PredicateValue,
            CustomPresentationContext,
            ExtraContext
          > &
            CommonAbstractRendererState,
        ) => ({
          ..._,
          value: props.context.value,
          type: readOnlyType.args[0],
          domNodeAncestorPath: props.context.domNodeAncestorPath + `[readOnly]`,
          typeAncestors: [props.context.type as DispatchParsedType<any>].concat(
            props.context.typeAncestors,
          ),
        }),
      )
      .mapForeignMutationsFromProps<{
        onChange: DispatchOnChange<PredicateValue, Flags>;
      }>(
        (): {
          onChange: DispatchOnChange<PredicateValue, Flags>;
        } => ({
          onChange: (
            elementUpdater: Option<BasicUpdater<PredicateValue>>,
            nestedDelta: DispatchDelta<Flags>,
          ) => {
            console.debug("ReadOnly field onChange intercepted - no changes allowed");
          },
        }),
      );

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
            embeddedTemplate={configuredChildTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
}; 