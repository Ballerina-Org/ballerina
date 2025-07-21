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
import { ListMethods } from "../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/list/state";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import {
  ListAbstractRendererForeignMutationsExpected,
  ListAbstractRendererReadonlyContext,
  ListAbstractRendererState,
  ListAbstractRendererView,
} from "./state";

export const ListAbstractRenderer = <
  T extends DispatchParsedType<T>,
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
  methods: ListMethods,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
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
            locked: _.locked,
            value: PredicateValue.Operations.IsUnit(_.value)
              ? _.value
              : _.value.values.get(elementIndex) || GetDefaultElementValue(),
            ...(_.elementFormStates?.get(elementIndex) ||
              GetDefaultElementState()),
            bindings: _.bindings,
            extraContext: _.extraContext,
            type: _.type.args[0],
            customPresentationContext: _.customPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            domNodeAncestorPath:
              _.domNodeAncestorPath + `[list][${elementIndex}]`,
            typeAncestors: [_.type as DispatchParsedType<T>].concat(
              _.typeAncestors,
            ),
            lookupTypeAncestorNames: _.lookupTypeAncestorNames,
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
              sourceAncestorLookupTypeNames:
                nestedDelta.sourceAncestorLookupTypeNames,
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
    const domNodeId = props.context.domNodeAncestorPath + "[list]";

    if (
      !PredicateValue.Operations.IsTuple(props.context.value) &&
      !PredicateValue.Operations.IsUnit(props.context.value)
    ) {
      console.error(
        `Tuple or unit value expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Tuple or unit value expected for list but got ${JSON.stringify(
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
            }}
            foreignMutations={{
              ...props.foreignMutations,
              add: !methods.includes("add")
                ? undefined
                : (flags) => {
                    const delta: DispatchDelta<Flags> = {
                      kind: "ArrayAdd",
                      value: GetDefaultElementValue(),
                      state: {
                        commonFormState: props.context.commonFormState,
                        elementFormStates: props.context.elementFormStates,
                      },
                      type: props.context.type.args[0],
                      flags,
                      sourceAncestorLookupTypeNames:
                        props.context.lookupTypeAncestorNames,
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
              remove: !methods.includes("remove")
                ? undefined
                : (_, flags) => {
                    const delta: DispatchDelta<Flags> = {
                      kind: "ArrayRemoveAt",
                      index: _,
                      flags,
                      sourceAncestorLookupTypeNames:
                        props.context.lookupTypeAncestorNames,
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
              move: !methods.includes("move")
                ? undefined
                : (index, to, flags) => {
                    const delta: DispatchDelta<Flags> = {
                      kind: "ArrayMoveFromTo",
                      from: index,
                      to: to,
                      flags,
                      sourceAncestorLookupTypeNames:
                        props.context.lookupTypeAncestorNames,
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
              duplicate: !methods.includes("duplicate")
                ? undefined
                : (_, flags) => {
                    const delta: DispatchDelta<Flags> = {
                      kind: "ArrayDuplicateAt",
                      index: _,
                      flags,
                      sourceAncestorLookupTypeNames:
                        props.context.lookupTypeAncestorNames,
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
              insert: !methods.includes("add")
                ? undefined
                : (_, flags) => {
                    const delta: DispatchDelta<Flags> = {
                      kind: "ArrayAddAt",
                      value: [_, GetDefaultElementValue()],
                      elementState: GetDefaultElementState(),
                      elementType: props.context.type.args[0],
                      flags,
                      sourceAncestorLookupTypeNames:
                        props.context.lookupTypeAncestorNames,
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
