import React from "react";
import {
  AsyncState,
  DispatchDelta,
  Guid,
  IdWrapperProps,
  replaceWith,
  Synchronize,
  Unit,
  DispatchOnChange,
  getLeafIdentifierFromIdentifier,
  ErrorRendererProps,
} from "../../../../../../../../main";
import { CoTypedFactory } from "../../../../../../../coroutines/builder";
import { Template } from "../../../../../../../template/state";
import { Value } from "../../../../../../../value/state";
import {
  PredicateValue,
  ValueRecord,
} from "../../../../../parser/domains/predicates/state";
import { FormLabel } from "../../../../../singleton/domains/form-label/state";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import {
  DispatchBaseEnumContext,
  EnumAbstractRendererState,
} from "../enum/state";
import { EnumMultiselectAbstractRendererView } from "./state";
import { OrderedMap } from "immutable";

export const EnumMultiselectAbstractRenderer = <
  Context extends FormLabel & DispatchBaseEnumContext,
  ForeignMutationsExpected,
>(
  IdWrapper: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const Co = CoTypedFactory<
    Context &
      Value<ValueRecord> &
      EnumAbstractRendererState & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    EnumAbstractRendererState
  >();
  return Template.Default<
    Context &
      Value<ValueRecord> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    EnumAbstractRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueRecord>;
    },
    EnumMultiselectAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => {
    if (!PredicateValue.Operations.IsRecord(props.context.value)) {
      console.error(
        `Record expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering enum multiselect field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Record value expected for enum multiselect but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }
    return (
      <IdWrapper
        id={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
      >
        <props.view
          {...props}
          context={{
            ...props.context,
            selectedIds: props.context.value.fields.keySeq().toArray(),
            activeOptions: !AsyncState.Operations.hasValue(
              props.context.customFormState.options.sync,
            )
              ? "unloaded"
              : props.context.customFormState.options.sync.value
                  .valueSeq()
                  .toArray(),
          }}
          foreignMutations={{
            ...props.foreignMutations,
            setNewValue: (_) => {
              if (
                !AsyncState.Operations.hasValue(
                  props.context.customFormState.options.sync,
                )
              )
                return;
              const options = props.context.customFormState.options.sync.value;
              const newSelection = _.flatMap((_) => {
                const selectedItem = options.get(_);
                if (selectedItem != undefined) {
                  const item: [string, ValueRecord] = [_, selectedItem];
                  return [item];
                }
                return [];
              });
              const delta: DispatchDelta = {
                kind: "SetReplace",
                replace: PredicateValue.Default.record(
                  OrderedMap(newSelection),
                ),
                state: {
                  commonFormState: props.context.commonFormState,
                  customFormState: props.context.customFormState,
                },
                type: props.context.type,
              };
              props.foreignMutations.onChange(
                replaceWith(
                  PredicateValue.Default.record(OrderedMap(newSelection)),
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
      </IdWrapper>
    );
  }).any([
    Co.Template<
      ForeignMutationsExpected & {
        onChange: DispatchOnChange<ValueRecord>;
      }
    >(
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
