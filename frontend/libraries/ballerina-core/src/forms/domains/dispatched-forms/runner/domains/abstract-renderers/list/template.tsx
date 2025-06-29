import {
  DispatchCommonFormState,
  DispatchDelta,
  IdWrapperProps,
  ListRepo,
  MapRepo,
  PredicateValue,
  replaceWith,
  Updater,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  Option,
  Unit,
  CommonAbstractRendererState,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererForeignMutationsExpected,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";
import {
  DispatchParsedType,
  StringSerializedType,
} from "../../../../deserializer/domains/specification/domains/types/state";
import {
  ListAbstractRendererForeignMutationsExpected,
  ListAbstractRendererReadonlyContext,
  ListAbstractRendererState,
  ListAbstractRendererView,
} from "./state";

export const ListAbstractRenderer = <
  T,
  CustomPresentationContext = Unit,
  Flags = Unit,
>(
  GetDefaultElementState: () => CommonAbstractRendererState,
  GetDefaultElementValue: () => PredicateValue,
  elementTemplate: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<T>,
      PredicateValue,
      CustomPresentationContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>
  >,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
) => {
  const embeddedElementTemplate =
    (elementIndex: number) => (flags: Flags | undefined) =>
      elementTemplate
        .mapContext(
          (
            _: ListAbstractRendererReadonlyContext<CustomPresentationContext> &
              ListAbstractRendererState,
          ) => ({
            disabled: _.disabled,
            value:
              _.value.values?.get(elementIndex) || GetDefaultElementValue(),
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
            domNodeId: _.identifiers.withoutLauncher.concat(
              `[${elementIndex}]`,
            ),
            type: _.type.args[0],
            CustomPresentationContext: _.CustomPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            serializedTypeHierarchy: [`[${elementIndex}]`].concat(
              _.serializedTypeHierarchy,
            ),
          }),
        )
        .mapState((_) =>
          ListAbstractRendererState.Updaters.Core.elementFormStates(
            MapRepo.Updaters.upsert(
              elementIndex,
              () => GetDefaultElementState(),
              _,
            ),
          ),
        )
        .mapForeignMutationsFromProps<
          ListAbstractRendererForeignMutationsExpected<Flags>
        >((props) => ({
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
        }));

  return Template.Default<
    ListAbstractRendererReadonlyContext<CustomPresentationContext> &
      ListAbstractRendererState,
    ListAbstractRendererState,
    ListAbstractRendererForeignMutationsExpected<Flags>,
    ListAbstractRendererView<CustomPresentationContext, Flags>
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

    const serializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

    return (
      <>
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              serializedTypeHierarchy,
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
                  type: props.context.type.args[0],
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
                  elementType: props.context.type.args[0],
                  flags,
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
