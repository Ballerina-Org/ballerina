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
  Option,
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
  Flags = Unit,
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
      onChange: DispatchOnChange<ValueOption, Flags>;
    },
    EnumAbstractRendererView<Context, ForeignMutationsExpected, Flags>
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
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId: props.context.identifiers.withoutLauncher,
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
              setNewValue: (value, flags) => {
                if (
                  !AsyncState.Operations.hasValue(
                    props.context.customFormState.options.sync,
                  )
                )
                  return;
                const newSelection =
                  props.context.customFormState.options.sync.value.get(value);
                if (newSelection == undefined) {
                  const delta: DispatchDelta<Flags> = {
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
                    flags,
                  };
                  return props.foreignMutations.onChange(
                    Option.Default.some(
                      replaceWith(
                        PredicateValue.Default.option(
                          false,
                          PredicateValue.Default.unit(),
                        ),
                      ),
                    ),
                    delta,
                  );
                } else {
                  const delta: DispatchDelta<Flags> = {
                    kind: "OptionReplace",
                    replace: PredicateValue.Default.option(true, newSelection),
                    state: {
                      commonFormState: props.context.commonFormState,
                      customFormState: props.context.customFormState,
                    },
                    type: props.context.type,
                    flags,
                  };
                  return props.foreignMutations.onChange(
                    Option.Default.some(
                      replaceWith(
                        PredicateValue.Default.option(true, newSelection),
                      ),
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
        </IdProvider>
      </>
    );
  }).any([
    Co.Template<
      ForeignMutationsExpected & {
        onChange: DispatchOnChange<ValueOption, Flags>;
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
