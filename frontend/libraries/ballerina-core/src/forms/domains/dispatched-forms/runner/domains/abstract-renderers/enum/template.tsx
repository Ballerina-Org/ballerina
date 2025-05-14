import { CoTypedFactory } from "../../../../../../../coroutines/builder";
import { Template } from "../../../../../../../template/state";
import {
  AsyncState,
  DispatchDelta,
  FormLabel,
  IdWrapperProps,
  Guid,
  PredicateValue,
  replaceWith,
  Synchronize,
  Unit,
  ValueOption,
  ValueRecord,
  DispatchOnChange,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
} from "../../../../../../../../main";
import {
  EnumAbstractRendererState,
  EnumAbstractRendererView,
  DispatchBaseEnumContext,
} from "./state";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { Value } from "../../../../../../../value/state";
import { OrderedMap } from "immutable";

export const EnumAbstractRenderer = <
  Context extends FormLabel & DispatchBaseEnumContext,
  ForeignMutationsExpected,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const Co = CoTypedFactory<
    Context &
      Value<ValueOption> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    EnumAbstractRendererState
  >();
  return Template.Default<
    Context &
      Value<ValueOption> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    EnumAbstractRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueOption>;
    },
    EnumAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => {
    if (!PredicateValue.Operations.IsOption(props.context.value)) {
      console.error(
        `Option expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering enum field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Option value expected for enum but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }
    return (
      <>
        <IdProvider
          id={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
        />
        <props.view
          {...props}
          context={{
            ...props.context,
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
              const newSelection =
                props.context.customFormState.options.sync.value.get(_);
              if (newSelection == undefined) {
                const delta: DispatchDelta = {
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
                const delta: DispatchDelta = {
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
      </>
    );
  }).any([
    Co.Template<
      ForeignMutationsExpected & {
        onChange: DispatchOnChange<ValueOption>;
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
        runFilter: (props) => {
          return props.context.customFormState.shouldLoad;
        },
      },
    ),
  ]);
};
