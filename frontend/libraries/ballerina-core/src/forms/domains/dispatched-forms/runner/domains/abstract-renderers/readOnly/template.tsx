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
import {
  ReadOnlyAbstractRendererView,
  ReadOnlyAbstractRendererReadonlyContext,
  ReadOnlyAbstractRendererState,
} from "./state";

export const ReadOnlyAbstractRenderer = <
  T extends DispatchParsedType<T>,
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  GetDefaultChildState: () => CommonAbstractRendererState,
  embeddedTemplate: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<T>,
      PredicateValue,
      CustomPresentationContext,
      ExtraContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>
  >,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const configuredChildTemplate = embeddedTemplate
    .mapContext(
      (
        _: ReadOnlyAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        > &
          ReadOnlyAbstractRendererState,
      ) => ({
        disabled: _.disabled,
        locked: _.locked,
        value: _.value.ReadOnly,
        ...(_.childFormState || GetDefaultChildState()),
        bindings: _.bindings,
        extraContext: _.extraContext,
        type: _.type.arg,
        customPresentationContext: _.customPresentationContext,
        remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
        domNodeAncestorPath: _.domNodeAncestorPath + `[readOnly]`,
        typeAncestors: [_.type as DispatchParsedType<any>].concat(
          _.typeAncestors,
        ),
        lookupTypeAncestorNames: _.lookupTypeAncestorNames,
      }),
    )
    .mapState((_) =>
      ReadOnlyAbstractRendererState.Updaters.Core.childFormState(_),
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
          console.debug(
            "ReadOnly field onChange intercepted - no changes allowed",
          );
        },
      }),
    );

  return Template.Default<
    ReadOnlyAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      ReadOnlyAbstractRendererState,
    ReadOnlyAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>,
    ReadOnlyAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[readOnly]";
    if (!PredicateValue.Operations.IsReadOnly(props.context.value)) {
      console.error(
        `ReadOnly value expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: ReadOnly value expected for list but got ${JSON.stringify(
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
              domNodeId,
            }}
            embeddedTemplate={configuredChildTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
