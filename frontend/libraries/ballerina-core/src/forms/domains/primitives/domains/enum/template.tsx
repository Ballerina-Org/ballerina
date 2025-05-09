import { List, OrderedMap } from "immutable";
import {
  AsyncState,
  BasicFun,
  CoTypedFactory,
  Delta,
  Guid,
  ParsedType,
  PredicateValue,
  replaceWith,
  Synchronize,
  Unit,
  ValidateRunner,
  ValueOption,
  ValueRecord,
} from "../../../../../../main";
import { Template } from "../../../../../template/state";
import { Value } from "../../../../../value/state";
import { FormLabel } from "../../../singleton/domains/form-label/state";
import {
  FieldValidation,
  FieldValidationWithPath,
  OnChange,
} from "../../../singleton/state";
import { BaseEnumContext, EnumFormState, EnumView } from "./state";

export const EnumForm = <
  Context extends FormLabel & BaseEnumContext,
  ForeignMutationsExpected,
>(
  validation?: BasicFun<ValueOption, Promise<FieldValidation>>,
) => {
  const Co = CoTypedFactory<
    Context &
      Value<ValueOption> & {
        disabled: boolean;
        visible: boolean;
        type: ParsedType<any>;
      },
    EnumFormState
  >();
  return Template.Default<
    Context &
      Value<ValueOption> & {
        disabled: boolean;
        visible: boolean;
        type: ParsedType<any>;
      },
    EnumFormState,
    ForeignMutationsExpected & {
      onChange: OnChange<ValueOption>;
    },
    EnumView<Context, ForeignMutationsExpected>
  >((props) => {
    return (
      <>
        <props.view
          {...props}
          context={{
            ...props.context,
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
              const newSelection =
                props.context.customFormState.options.sync.value.get(_);
              if (newSelection == undefined) {
                const delta: Delta = {
                  kind: "OptionReplace",
                  replace: PredicateValue.Default.option(
                    false,
                    PredicateValue.Default.unit(),
                  ),
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                };
                return props.foreignMutations.onChange(
                  replaceWith(
                    PredicateValue.Default.option(
                      false,
                      PredicateValue.Default.unit(),
                    ),
                  ),
                  delta,
                );
              } else {
                const delta: Delta = {
                  kind: "OptionReplace",
                  replace: PredicateValue.Default.option(true, newSelection),
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                };
                return props.foreignMutations.onChange(
                  replaceWith(
                    PredicateValue.Default.option(true, newSelection),
                  ),
                  delta,
                );
              }
            },
          }}
        />
      </>
    );
  }).any([
    ValidateRunner<
      Context & { disabled: boolean; visible: boolean; type: ParsedType<any> },
      EnumFormState,
      ForeignMutationsExpected,
      ValueOption
    >(
      validation
        ? (_) =>
            validation(_).then(
              FieldValidationWithPath.Default.fromFieldValidation,
            )
        : undefined,
    ),
    Co.Template<
      ForeignMutationsExpected & {
        onChange: OnChange<ValueOption>;
      }
    >(
      Co.GetState().then((current) => {
        return Synchronize<Unit, OrderedMap<Guid, ValueRecord>>(
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
        );
      }),
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
