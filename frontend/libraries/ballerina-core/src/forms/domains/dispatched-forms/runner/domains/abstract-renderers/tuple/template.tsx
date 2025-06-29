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
  FormLabel,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  Template,
  Updater,
  Value,
  ValueTuple,
  DispatchOnChange,
  getLeafIdentifierFromIdentifier,
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
>(
  ItemFormStates: Map<number, () => CommonAbstractRendererState>,
  itemTemplates: Map<
    number,
    Template<
      CommonAbstractRendererReadonlyContext<
        DispatchParsedType<any>,
        PredicateValue,
        CustomPresentationContext
      > &
        CommonAbstractRendererState,
      CommonAbstractRendererState,
      CommonAbstractRendererForeignMutationsExpected<Flags>
    >
  >,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
) => {
  const embeddedItemTemplates =
    (itemIndex: number) => (flags: Flags | undefined) =>
      itemTemplates
        .get(itemIndex)!
        .mapContext(
          (
            _: TupleAbstractRendererReadonlyContext<CustomPresentationContext> &
              TupleAbstractRendererState,
          ) => ({
            ...(_.itemFormStates.get(itemIndex) ||
              ItemFormStates.get(itemIndex)!()),
            value: _.value.values.get(itemIndex)!,
            disabled: _.disabled,
            bindings: _.bindings,
            extraContext: _.extraContext,
            identifiers: {
              withLauncher: _.identifiers.withLauncher.concat(
                `[${itemIndex + 1}]`,
              ),
              withoutLauncher: _.identifiers.withoutLauncher.concat(
                `[${itemIndex + 1}]`,
              ),
            },
            domNodeId: _.identifiers.withoutLauncher.concat(
              `[${itemIndex + 1}]`,
            ),
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            CustomPresentationContext: _.CustomPresentationContext,
            type: _.type.args[itemIndex],
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
    TupleAbstractRendererReadonlyContext<CustomPresentationContext> &
      TupleAbstractRendererState,
    TupleAbstractRendererState,
    TupleAbstractRendererForeignMutationsExpected<Flags>,
    TupleAbstractRendererView<CustomPresentationContext, Flags>
  >((props) => {
    if (!PredicateValue.Operations.IsTuple(props.context.value)) {
      console.error(
        `Tuple expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering tuple field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Tuple value expected for tuple but got ${JSON.stringify(
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
            }}
            embeddedItemTemplates={embeddedItemTemplates}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
