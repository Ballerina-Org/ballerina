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
  ExtraContext = Unit,
>(
  GetDefaultElementState: () => CommonAbstractRendererState,
  GetDefaultElementValue: () => PredicateValue,
  elementTemplate: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<T>,
      PredicateValue,
      CustomPresentationContext,
      ExtraContext
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
            _: ListAbstractRendererReadonlyContext<
              CustomPresentationContext,
              ExtraContext
            > &
              ListAbstractRendererState,
          ) => ({
            disabled: _.disabled,
            value:
              _.value.values?.get(elementIndex) || GetDefaultElementValue(),
            ...(_.elementFormStates?.get(elementIndex) ||
              GetDefaultElementState()),
            bindings: _.bindings,
            extraContext: _.extraContext,
            type: _.type.args[0],
            customPresentationContext: _.customPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            domNodeTypeHierarchy: [`[${elementIndex}]`, SerializedType].concat(
              _.domNodeTypeHierarchy,
            ),
            serializedTypeHierarchy: [SerializedType].concat(
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
    ListAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      ListAbstractRendererState,
    ListAbstractRendererState,
    ListAbstractRendererForeignMutationsExpected<Flags>,
    ListAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const completeSerializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

    const domNodeTypeHierarchy = [SerializedType].concat(
      props.context.domNodeTypeHierarchy,
    );

    const domNodeId = domNodeTypeHierarchy.join(".");

    if (!PredicateValue.Operations.IsTuple(props.context.value)) {
      console.error(
        `Tuple value expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering list field\n...${SerializedType}`,
      );
      return (
        <ErrorRenderer
          message={`${SerializedType}: Tuple value expected for list but got ${JSON.stringify(
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
              completeSerializedTypeHierarchy,
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
