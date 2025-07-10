import { Map } from "immutable";

import {
  TupleAbstractRendererForeignMutationsExpected,
  TupleAbstractRendererReadonlyContext,
  TupleAbstractRendererState,
  TupleAbstractRendererView,
} from "./state";
import {
  BasicUpdater,
  Bindings,
  DispatchCommonFormState,
  DispatchDelta,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  Template,
  Updater,
  DispatchOnChange,
  ErrorRendererProps,
  Option,
  Unit,
  CommonAbstractRendererState,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererForeignMutationsExpected,
} from "../../../../../../../../main";
import {
  DispatchParsedType,
  StringSerializedType,
} from "../../../../deserializer/domains/specification/domains/types/state";

export const DispatchTupleAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  ItemFormStates: Map<number, () => CommonAbstractRendererState>,
  itemTemplates: Map<
    number,
    Template<
      CommonAbstractRendererReadonlyContext<
        DispatchParsedType<any>,
        PredicateValue,
        CustomPresentationContext,
        ExtraContext
      > &
        CommonAbstractRendererState,
      CommonAbstractRendererState,
      CommonAbstractRendererForeignMutationsExpected<Flags>
    >
  >,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const embeddedItemTemplates =
    (itemIndex: number) => (flags: Flags | undefined) =>
      itemTemplates
        .get(itemIndex)!
        .mapContext(
          (
            _: TupleAbstractRendererReadonlyContext<
              CustomPresentationContext,
              ExtraContext
            > &
              TupleAbstractRendererState,
          ) => ({
            ...(_.itemFormStates.get(itemIndex) ||
              ItemFormStates.get(itemIndex)!()),
            value: _.value.values.get(itemIndex)!,
            disabled: _.disabled,
            locked: _.locked,
            bindings: _.bindings,
            extraContext: _.extraContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            customPresentationContext: _.customPresentationContext,
            type: _.type.args[itemIndex],
            domNodeAncestorPath:
              _.domNodeAncestorPath + `[tuple][${itemIndex + 1}]`,
            typeAncestors: [_.type as DispatchParsedType<any>].concat(
              _.typeAncestors,
            ),
            lookupTypeAncestorNames: _.lookupTypeAncestorNames,
          }),
        )
        .mapState(
          (
            _: BasicUpdater<CommonAbstractRendererState>,
          ): Updater<TupleAbstractRendererState> =>
            TupleAbstractRendererState.Updaters.Template.upsertItemFormState(
              itemIndex,
              ItemFormStates.get(itemIndex)!,
              _,
            ),
        )
        .mapForeignMutationsFromProps<
          TupleAbstractRendererForeignMutationsExpected<Flags>
        >(
          (
            props,
          ): {
            onChange: DispatchOnChange<PredicateValue, Flags>;
          } => ({
            onChange: (elementUpdater, nestedDelta) => {
              const delta: DispatchDelta<Flags> = {
                kind: "TupleCase",
                item: [itemIndex, nestedDelta],
                tupleType: props.context.type,
                flags,
                sourceAncestorLookupTypeNames:
                  nestedDelta.sourceAncestorLookupTypeNames,
              };
              props.foreignMutations.onChange(
                elementUpdater.kind == "l"
                  ? Option.Default.none()
                  : Option.Default.some(
                      Updater((tuple) =>
                        tuple.values.has(itemIndex)
                          ? PredicateValue.Default.tuple(
                              tuple.values.update(
                                itemIndex,
                                PredicateValue.Default.unit(),
                                elementUpdater.value,
                              ),
                            )
                          : tuple,
                      ),
                    ),
                delta,
              );

              props.setState(
                TupleAbstractRendererState.Updaters.Core.commonFormState(
                  DispatchCommonFormState.Updaters.modifiedByUser(
                    replaceWith(true),
                  ),
                ).then(
                  TupleAbstractRendererState.Updaters.Template.upsertItemFormState(
                    itemIndex,
                    ItemFormStates.get(itemIndex)!,
                    (_) => ({
                      ..._,
                      commonFormState:
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        )(_.commonFormState),
                    }),
                  ),
                ),
              );
            },
          }),
        );

  return Template.Default<
    TupleAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      TupleAbstractRendererState,
    TupleAbstractRendererState,
    TupleAbstractRendererForeignMutationsExpected<Flags>,
    TupleAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[tuple]";

    if (!PredicateValue.Operations.IsTuple(props.context.value)) {
      console.error(
        `Tuple expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering tuple field\n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Tuple value expected but got ${JSON.stringify(
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
            }}
            embeddedItemTemplates={embeddedItemTemplates}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
