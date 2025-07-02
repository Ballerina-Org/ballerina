import { CoTypedFactory } from "../../../../../../../coroutines/builder";
import { Template } from "../../../../../../../template/state";
import {
  AsyncState,
  DispatchDelta,
  IdWrapperProps,
  Guid,
  PredicateValue,
  replaceWith,
  Synchronize,
  Unit,
  ValueRecord,
  ErrorRendererProps,
  Option,
  StringSerializedType,
} from "../../../../../../../../main";
import {
  EnumAbstractRendererState,
  EnumAbstractRendererView,
  EnumAbstractRendererReadonlyContext,
  EnumAbstractRendererForeignMutationsExpected,
} from "./state";
import { OrderedMap } from "immutable";

export const EnumAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const Co = CoTypedFactory<
    EnumAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      EnumAbstractRendererState,
    EnumAbstractRendererState
  >();
  return Template.Default<
    EnumAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      EnumAbstractRendererState,
    EnumAbstractRendererState,
    EnumAbstractRendererForeignMutationsExpected<Flags>,
    EnumAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[enum]";

    if (!PredicateValue.Operations.IsOption(props.context.value)) {
      console.error(
        `Option expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Option value expected but got ${JSON.stringify(
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
    Co.Template<EnumAbstractRendererForeignMutationsExpected<Flags>>(
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
