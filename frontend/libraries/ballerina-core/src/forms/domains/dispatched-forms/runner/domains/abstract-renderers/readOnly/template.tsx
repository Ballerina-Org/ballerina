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
  NestedRenderer,
  BaseFlags,
  replaceWith,
  ValueReadOnly,
  Updater,
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
  Flags extends BaseFlags = BaseFlags,
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
  ChildRenderer: NestedRenderer<any>,
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
      ) => {
        const labelContext =
          CommonAbstractRendererState.Operations.GetLabelContext(
            _.labelContext,
            ChildRenderer,
          );
        return {
          disabled: _.disabled || _.globallyDisabled,
          globallyDisabled: _.globallyDisabled,
          locked: _.locked,
          value: _.value.ReadOnly,
          ...(_.childFormState || GetDefaultChildState()),
          readOnly: true,
          globallyReadOnly: _.globallyReadOnly,
          bindings: _.bindings,
          extraContext: _.extraContext,
          type: _.type.arg,
          customPresentationContext: _.customPresentationContext,
          remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
          domNodeAncestorPath: _.domNodeAncestorPath + `[ReadOnly]`,
          legacy_domNodeAncestorPath:
            _.legacy_domNodeAncestorPath + "[readOnly]",
          predictionAncestorPath: _.predictionAncestorPath + "[ReadOnly]",
          layoutAncestorPath: _.layoutAncestorPath + "[readOnly]",
          typeAncestors: [_.type as DispatchParsedType<any>].concat(
            _.typeAncestors,
          ),
          lookupTypeAncestorNames: _.lookupTypeAncestorNames,
          preprocessedSpecContext: _.preprocessedSpecContext,
          labelContext,
          usePreprocessor: _.usePreprocessor,
        };
      },
    )
    .mapState((_) =>
      ReadOnlyAbstractRendererState.Updaters.Core.childFormState(_),
    )
    .mapForeignMutationsFromProps<{
      onChange: DispatchOnChange<PredicateValue, Flags>;
    }>(
      (
        props,
      ): {
        onChange: DispatchOnChange<PredicateValue, Flags>;
      } => ({
        onChange: (
          elementUpdater: Option<BasicUpdater<PredicateValue>>,
          nestedDelta: DispatchDelta<Flags>,
        ) => {
          const flags = { kind: "localOnly" };
          const delta: DispatchDelta<Flags> = {
            kind: "UnitReplace",
            replace: PredicateValue.Default.unit(),
            state: {},
            type: DispatchParsedType.Default.primitive("unit"),
            flags: flags as Flags,
            sourceAncestorLookupTypeNames:
              nestedDelta.sourceAncestorLookupTypeNames,
          };

          props.foreignMutations.onChange(
            elementUpdater.kind == "l"
              ? Option.Default.none()
              : Option.Default.some(
                  Updater<PredicateValue>((value) =>
                    ValueReadOnly.Updaters.ReadOnly(elementUpdater.value)(
                      value as ValueReadOnly,
                    ),
                  ),
                ),
            delta,
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
    const domNodeId = props.context.domNodeAncestorPath;
    const legacy_domNodeId =
      props.context.legacy_domNodeAncestorPath + "[readOnly]";

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
        <IdProvider domNodeId={props.context.usePreprocessor ? domNodeId : legacy_domNodeId}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId,
              legacy_domNodeId,
            }}
            embeddedTemplate={configuredChildTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
