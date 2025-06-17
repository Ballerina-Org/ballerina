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
  Option,
  id,
  Unit,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";
import { Value } from "../../../../../../../value/state";
import { FormLabel } from "../../../../../singleton/domains/form-label/state";
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
  Flags = Unit,
>(
  GetDefaultElementState: () => any,
  GetDefaultElementValue: () => PredicateValue,
  elementTemplate: Template<
    Context &
      Value<PredicateValue> &
      any & { bindings: Bindings; extraContext: any },
    any,
    {
      onChange: DispatchOnChange<PredicateValue, Flags>;
    }
  >,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const embeddedElementTemplate = (elementIndex: number) => (flags: Flags | undefined) =>
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
          onChange: DispatchOnChange<ValueTuple, Flags>;
        }
      >(
        (
          props,
        ): {
          onChange: DispatchOnChange<PredicateValue, Flags>;
        } => ({
          onChange: (elementUpdater, nestedDelta) => {
            const delta: DispatchDelta<Flags> = {
              kind: "ArrayValue",
              value: [elementIndex, nestedDelta],
              flags,
            };
            props.foreignMutations.onChange(
              elementUpdater.kind == "l"
                ? Option.Default.none()
                : Option.Default.some(
                    Updater((list) =>
                      list.values.has(elementIndex)
                        ? PredicateValue.Default.tuple(
                            list.values.update(
                              elementIndex,
                              PredicateValue.Default.unit(),
                              elementUpdater.value,
                            ),
                          )
                        : list,
                    ),
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
      onChange: DispatchOnChange<ValueTuple, Flags>;
    },
    ListAbstractRendererView<Context, ForeignMutationsExpected, Flags>
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
              add: (flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "ArrayAdd",
                  value: GetDefaultElementValue(),
                  state: {
                    commonFormState: props.context.commonFormState,
                    elementFormStates: props.context.elementFormStates,
                  },
                  type: (props.context.type as ListType<any>).args[0],
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(
                    Updater((list) =>
                      PredicateValue.Default.tuple(
                        ListRepo.Updaters.push<PredicateValue>(
                          GetDefaultElementValue(),
                        )(list.values),
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
              remove: (_, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "ArrayRemoveAt",
                  index: _,
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(
                    Updater((list) =>
                      PredicateValue.Default.tuple(
                        ListRepo.Updaters.remove<PredicateValue>(_)(
                          list.values,
                        ),
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
              move: (index, to, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "ArrayMoveFromTo",
                  from: index,
                  to: to,
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(
                    Updater((list) =>
                      PredicateValue.Default.tuple(
                        ListRepo.Updaters.move<PredicateValue>(
                          index,
                          to,
                        )(list.values),
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
              duplicate: (_, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "ArrayDuplicateAt",
                  index: _,
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(
                    Updater((list) =>
                      PredicateValue.Default.tuple(
                        ListRepo.Updaters.duplicate<PredicateValue>(_)(
                          list.values,
                        ),
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
              insert: (_, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "ArrayAddAt",
                  value: [_, GetDefaultElementValue()],
                  elementState: GetDefaultElementState(),
                  elementType: (props.context.type as ListType<any>).args[0],
                  flags
                };
                props.foreignMutations.onChange(
                  Option.Default.some(
                    Updater((list) =>
                      PredicateValue.Default.tuple(
                        ListRepo.Updaters.insert<PredicateValue>(
                          _,
                          GetDefaultElementValue(),
                        )(list.values),
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
            }}
            embeddedElementTemplate={embeddedElementTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
