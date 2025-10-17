import React from "react";
import {
  AsyncState,
  DispatchDelta,
  Guid,
  IdWrapperProps,
  replaceWith,
  Synchronize,
  Unit,
  ErrorRendererProps,
  Option,
  StringSerializedType,
} from "../../../../../../../../main";
import { CoTypedFactory } from "../../../../../../../coroutines/builder";
import { Template } from "../../../../../../../template/state";
import {
  PredicateValue,
  ValueRecord,
} from "../../../../../parser/domains/predicates/state";
import { EnumAbstractRendererState } from "../enum/state";
import {
  EnumMultiselectAbstractRendererState,
  EnumMultiselectAbstractRendererReadonlyContext,
  EnumMultiselectAbstractRendererView,
  EnumMultiselectAbstractRendererForeignMutationsExpected,
} from "./state";
import { OrderedMap } from "immutable";
import { useRegistryValueAtPath } from "../registry-store";

export const EnumMultiselectAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const Co = CoTypedFactory<
    EnumMultiselectAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      EnumMultiselectAbstractRendererState,
    EnumAbstractRendererState
  >();
  return Template.Default<
    EnumMultiselectAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      EnumMultiselectAbstractRendererState,
    EnumAbstractRendererState,
    EnumMultiselectAbstractRendererForeignMutationsExpected<Flags>,
    EnumMultiselectAbstractRendererView<
      CustomPresentationContext,
      Flags,
      ExtraContext
    >
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[enumMultiselect]";
    const value = useRegistryValueAtPath(props.context.path);
    if (!value) {
      return <></>;
    }
    if (!PredicateValue.Operations.IsRecord(value)) {
      console.error(
        `Record expected but got: ${JSON.stringify(
          value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Record value expected but got ${JSON.stringify(
            value,
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
              selectedIds: value.fields.keySeq().toArray(),
              activeOptions: !AsyncState.Operations.hasValue(
                props.context.customFormState.options.sync,
              )
                ? "unloaded"
                : props.context.customFormState.options.sync.value
                    .valueSeq()
                    .toArray(),
              value,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              setNewValue: (value, flags) => {
                if (
                  !AsyncState.Operations.hasValue(
                    props.context.customFormState.options.sync,
                  )
                )
                  return;
                const options =
                  props.context.customFormState.options.sync.value;
                const newSelection = value.flatMap((_) => {
                  const selectedItem = options.get(_);
                  if (selectedItem != undefined) {
                    const item: [string, ValueRecord] = [_, selectedItem];
                    return [item];
                  }
                  return [];
                });
                const delta: DispatchDelta<Flags> = {
                  kind: "SetReplace",
                  replace: PredicateValue.Default.record(
                    OrderedMap(newSelection),
                  ),
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                  flags,
                  sourceAncestorLookupTypeNames:
                    props.context.lookupTypeAncestorNames,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(
                    replaceWith(
                      PredicateValue.Default.record(OrderedMap(newSelection)),
                    ),
                  ),
                  delta,
                );
              },
              loadOptions: () => {
                props.setState((current) => ({
                  ...current,
                  customFormState: {
                    ...current.customFormState,
                    shouldLoad: true,
                  },
                }));
              },
            }}
          />
        </IdProvider>
      </>
    );
  }).any([
    Co.Template<EnumMultiselectAbstractRendererForeignMutationsExpected<Flags>>(
      Co.GetState().then((current) =>
        Co.Seq([
          Co.SetState((current) => ({
            ...current,
            activeOptions: "loading",
          })),
          Synchronize<Unit, OrderedMap<Guid, ValueRecord>>(
            current.getOptions,
            () => "transient failure",
            5,
            50,
          ).embed(
            (_) => _.customFormState.options,
            (_) => (current) => ({
              ...current,
              customFormState: {
                ...current.customFormState,
                options: _(current.customFormState.options),
              },
            }),
          ),
        ]),
      ),
      {
        interval: 15,
        runFilter: (props) => props.context.customFormState.shouldLoad,
      },
    ),
  ]);
};
