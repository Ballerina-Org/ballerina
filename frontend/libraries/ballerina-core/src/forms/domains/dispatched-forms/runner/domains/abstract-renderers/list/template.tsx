import {
  BasicUpdater,
  Bindings,
  DispatchCommonFormState,
  DispatchDelta,
  IdWrapperProps,
  ListRepo,
  MapRepo,
  PredicateValue,
  replaceWith,
  Updater,
  ValueTuple,
  DispatchOnChange,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";
import { Value } from "../../../../../../../value/state";
import { FormLabel } from "../../../../../singleton/domains/form-label/state";
import { ListMethods } from "../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/list/state";
import {
  DispatchParsedType,
  ListType,
} from "../../../../deserializer/domains/specification/domains/types/state";
import { ListAbstractRendererState, ListAbstractRendererView } from "./state";

export const ListAbstractRenderer = <
  Context extends FormLabel & {
    type: DispatchParsedType<any>;
    disabled: boolean;
    identifiers: { withLauncher: string; withoutLauncher: string };
  },
  ForeignMutationsExpected,
>(
  GetDefaultElementState: () => any,
  GetDefaultElementValue: () => PredicateValue,
  elementTemplate: Template<
    Context &
      Value<PredicateValue> &
      any & { bindings: Bindings; extraContext: any },
    any,
    {
      onChange: DispatchOnChange<PredicateValue>;
    }
  >,
  methods: ListMethods,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const embeddedElementTemplate = (elementIndex: number) =>
    elementTemplate
      .mapContext(
        (
          _: Context &
            Value<ValueTuple> &
            ListAbstractRendererState & {
              bindings: Bindings;
              extraContext: any;
              identifiers: { withLauncher: string; withoutLauncher: string };
            },
        ): Value<ValueTuple> & any => ({
          ..._,
          disabled: _.disabled,
          value: _.value.values?.get(elementIndex) || GetDefaultElementValue(),
          ...(_.elementFormStates?.get(elementIndex) ||
            GetDefaultElementState()),
          bindings: _.bindings,
          extraContext: _.extraContext,
          identifiers: {
            withLauncher: _.identifiers.withLauncher.concat(
              `[${elementIndex}]`,
            ),
            withoutLauncher: _.identifiers.withoutLauncher.concat(
              `[${elementIndex}]`,
            ),
          },
        }),
      )
      .mapState(
        (_: BasicUpdater<any>): Updater<ListAbstractRendererState> =>
          ListAbstractRendererState.Updaters.Core.elementFormStates(
            MapRepo.Updaters.upsert(
              elementIndex,
              () => GetDefaultElementState(),
              _,
            ),
          ),
      )
      .mapForeignMutationsFromProps<
        ForeignMutationsExpected & {
          onChange: DispatchOnChange<ValueTuple>;
        }
      >(
        (
          props,
        ): {
          onChange: DispatchOnChange<PredicateValue>;
        } => ({
          onChange: (elementUpdater, nestedDelta) => {
            const delta: DispatchDelta = {
              kind: "ArrayValue",
              value: [elementIndex, nestedDelta],
              isWholeEntityMutation: false,
            };
            props.foreignMutations.onChange(
              Updater((list) =>
                list.values.has(elementIndex)
                  ? PredicateValue.Default.tuple(
                      list.values.update(
                        elementIndex,
                        PredicateValue.Default.unit(),
                        elementUpdater,
                      ),
                    )
                  : list,
              ),
              delta,
            );
            props.setState(
              ListAbstractRendererState.Updaters.Core.commonFormState(
                DispatchCommonFormState.Updaters.modifiedByUser(
                  replaceWith(true),
                ),
              ).then(
                ListAbstractRendererState.Updaters.Core.elementFormStates(
                  MapRepo.Updaters.upsert(
                    elementIndex,
                    () => GetDefaultElementState(),
                    (_) => ({
                      ..._,
                      commonFormState:
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        )(_.commonFormState),
                    }),
                  ),
                ),
              ),
            );
          },
        }),
      );

  return Template.Default<
    Context & Value<ValueTuple> & { disabled: boolean },
    ListAbstractRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueTuple>;
    },
    ListAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => {
    if (!PredicateValue.Operations.IsTuple(props.context.value)) {
      console.error(
        `Tuple value expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering list field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Tuple value expected for list but got ${JSON.stringify(
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
            }}
            foreignMutations={{
              ...props.foreignMutations,
              add: !methods.includes("add")
                ? undefined
                : (_) => {
                    const delta: DispatchDelta = {
                      kind: "ArrayAdd",
                      value: GetDefaultElementValue(),
                      state: {
                        commonFormState: props.context.commonFormState,
                        elementFormStates: props.context.elementFormStates,
                      },
                      type: (props.context.type as ListType<any>).args[0],
                      isWholeEntityMutation: true, // TODO: check
                    };
                    props.foreignMutations.onChange(
                      Updater((list) =>
                        PredicateValue.Default.tuple(
                          ListRepo.Updaters.push<PredicateValue>(
                            GetDefaultElementValue(),
                          )(list.values),
                        ),
                      ),
                      delta,
                    );
                    props.setState(
                      ListAbstractRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ),
                    );
                  },
              remove: !methods.includes("remove")
                ? undefined
                : (_) => {
                    const delta: DispatchDelta = {
                      kind: "ArrayRemoveAt",
                      index: _,
                      isWholeEntityMutation: true, // TODO: check
                    };
                    props.foreignMutations.onChange(
                      Updater((list) =>
                        PredicateValue.Default.tuple(
                          ListRepo.Updaters.remove<PredicateValue>(_)(
                            list.values,
                          ),
                        ),
                      ),
                      delta,
                    );
                    props.setState(
                      ListAbstractRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ),
                    );
                  },
              move: !methods.includes("move")
                ? undefined
                : (index, to) => {
                    const delta: DispatchDelta = {
                      kind: "ArrayMoveFromTo",
                      from: index,
                      to: to,
                      isWholeEntityMutation: true, // TODO: check
                    };
                    props.foreignMutations.onChange(
                      Updater((list) =>
                        PredicateValue.Default.tuple(
                          ListRepo.Updaters.move<PredicateValue>(
                            index,
                            to,
                          )(list.values),
                        ),
                      ),
                      delta,
                    );
                    props.setState(
                      ListAbstractRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ),
                    );
                  },
              duplicate: !methods.includes("duplicate")
                ? undefined
                : (_) => {
                    const delta: DispatchDelta = {
                      kind: "ArrayDuplicateAt",
                      index: _,
                      isWholeEntityMutation: true, // TODO: check
                    };
                    props.foreignMutations.onChange(
                      Updater((list) =>
                        PredicateValue.Default.tuple(
                          ListRepo.Updaters.duplicate<PredicateValue>(_)(
                            list.values,
                          ),
                        ),
                      ),
                      delta,
                    );
                    props.setState(
                      ListAbstractRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ),
                    );
                  },
              insert: !methods.includes("add")
                ? undefined
                : (_) => {
                    const delta: DispatchDelta = {
                      kind: "ArrayAddAt",
                      value: [_, GetDefaultElementValue()],
                      elementState: GetDefaultElementState(),
                      elementType: (props.context.type as ListType<any>)
                        .args[0],
                      isWholeEntityMutation: true, // TODO: check
                    };
                    props.foreignMutations.onChange(
                      Updater((list) =>
                        PredicateValue.Default.tuple(
                          ListRepo.Updaters.insert<PredicateValue>(
                            _,
                            GetDefaultElementValue(),
                          )(list.values),
                        ),
                      ),
                      delta,
                    );
                    props.setState(
                      ListAbstractRendererState.Updaters.Core.commonFormState(
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        ),
                      ),
                    );
                  },
            }}
            embeddedElementTemplate={embeddedElementTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
