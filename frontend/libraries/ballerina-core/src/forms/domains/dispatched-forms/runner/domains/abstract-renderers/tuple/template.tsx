import { Map } from "immutable";

import { TupleAbstractRendererState, TupleAbstractRendererView } from "./state";
import {
  BasicUpdater,
  Bindings,
  DispatchCommonFormState,
  DispatchDelta,
  FormLabel,
  IdWrapperProps,
  MapRepo,
  PredicateValue,
  replaceWith,
  Template,
  Updater,
  Value,
  ValueTuple,
  DispatchOnChange,
  getLeafIdentifierFromIdentifier,
  ErrorRendererProps,
} from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";

export const DispatchTupleAbstractRenderer = <
  ItemFormState extends { commonFormState: DispatchCommonFormState },
  Context extends FormLabel & {
    disabled: boolean;
    type: DispatchParsedType<any>;
    identifiers: { withLauncher: string; withoutLauncher: string };
  },
  ForeignMutationsExpected,
>(
  ItemFormStates: Map<number, () => ItemFormState>,
  itemTemplates: Map<
    number,
    Template<
      Value<PredicateValue> & {
        commonFormState: { modifiedByUser: boolean };
        type: DispatchParsedType<any>;
        bindings: Bindings;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
      any,
      {
        onChange: DispatchOnChange<PredicateValue>;
      }
    >
  >,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const embeddedItemTemplates = (itemIndex: number) =>
    itemTemplates
      .get(itemIndex)!
      .mapContext(
        (
          _: Context &
            Value<ValueTuple> &
            TupleAbstractRendererState<ItemFormState> & {
              bindings: Bindings;
              extraContext: any;
              identifiers: { withLauncher: string; withoutLauncher: string };
            },
        ): Value<PredicateValue> & {
          commonFormState: { modifiedByUser: boolean };
          type: DispatchParsedType<any>;
          bindings: Bindings;
          identifiers: { withLauncher: string; withoutLauncher: string };
        } => ({
          ...(_.itemFormStates.get(itemIndex) ||
            ItemFormStates.get(itemIndex)!()),
          value: _.value.values.get(itemIndex)!,
          disabled: _.disabled,
          type: _.type,
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
        }),
      )
      .mapState(
        (
          _: BasicUpdater<ItemFormState>,
        ): Updater<TupleAbstractRendererState<ItemFormState>> =>
          TupleAbstractRendererState<ItemFormState>().Updaters.Template.upsertItemFormState(
            itemIndex,
            ItemFormStates.get(itemIndex)!,
            _,
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
              kind: "TupleCase",
              item: [itemIndex, nestedDelta],
              tupleType: props.context.type,
              isWholeEntityMutation: false,
            };
            props.foreignMutations.onChange(
              Updater((tuple) =>
                tuple.values.has(itemIndex)
                  ? PredicateValue.Default.tuple(
                      tuple.values.update(
                        itemIndex,
                        PredicateValue.Default.unit(),
                        elementUpdater,
                      ),
                    )
                  : tuple,
              ),
              delta,
            );

            props.setState(
              TupleAbstractRendererState<ItemFormState>()
                .Updaters.Core.commonFormState(
                  DispatchCommonFormState.Updaters.modifiedByUser(
                    replaceWith(true),
                  ),
                )
                .then(
                  TupleAbstractRendererState<ItemFormState>().Updaters.Template.upsertItemFormState(
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
    Context &
      Value<ValueTuple> & {
        disabled: boolean;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    TupleAbstractRendererState<ItemFormState>,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueTuple>;
    },
    TupleAbstractRendererView<ItemFormState, Context, ForeignMutationsExpected>
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
        <IdProvider
          id={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
        />
        <props.view
          {...props}
          context={{
            ...props.context,
          }}
          foreignMutations={{
            ...props.foreignMutations,
          }}
          embeddedItemTemplates={embeddedItemTemplates}
        />
      </>
    );
  }).any([]);
};
