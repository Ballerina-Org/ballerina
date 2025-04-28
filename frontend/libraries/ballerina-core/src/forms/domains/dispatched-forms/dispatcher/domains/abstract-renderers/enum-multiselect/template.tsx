import React from "react";
import {
  AsyncState,
  DispatchDelta,
  Guid,
  replaceWith,
  Synchronize,
  Unit,
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
import { DispatchOnChange } from "../../../state";
import {
  DispatchBaseEnumContext,
  EnumAbstractRendererState,
} from "../enum/state";
import { EnumMultiselectAbstractRendererView } from "./state";
import { OrderedMap } from "immutable";

export const EnumMultiselectAbstractRenderer = <
  Context extends FormLabel & DispatchBaseEnumContext,
  ForeignMutationsExpected,
>() => {
  const Co = CoTypedFactory<
    Context &
      Value<ValueRecord> &
      EnumAbstractRendererState & {
        disabled: boolean;
        type: DispatchParsedType<any>;
      },
    EnumAbstractRendererState
  >();
  return Template.Default<
    Context &
      Value<ValueRecord> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
      },
    EnumAbstractRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueRecord>;
    },
    EnumMultiselectAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => {
    return (
      <>
        <props.view
          {...props}
          context={{
            ...props.context,
            selectedIds: props.context.value.fields.keySeq().toArray(),
            activeOptions: !AsyncState.Operations.hasValue(
              props.context.customFormState.options.sync,
            )
              ? "loading"
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
          }}
        />
      </>
    );
  }).any([
    Co.Template<
      ForeignMutationsExpected & {
        onChange: DispatchOnChange<ValueRecord>;
      }
    >(
      Co.GetState().then((current) =>
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
      ),
      {
        interval: 15,
        runFilter: (props) =>
          !AsyncState.Operations.hasValue(
            props.context.customFormState.options.sync,
          ),
      },
    ),
  ]);
};
