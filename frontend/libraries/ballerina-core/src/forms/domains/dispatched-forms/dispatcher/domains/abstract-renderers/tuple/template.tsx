import { Map } from "immutable";

import { TupleAbstractRendererState, TupleAbstractRendererView } from "./state";
import {
  BasicUpdater,
  Bindings,
  DispatchDelta,
  FormLabel,
  MapRepo,
  PredicateValue,
  Template,
  Updater,
  Value,
  ValueTuple,
} from "../../../../../../../../main";
import { DispatchOnChange } from "../../../state";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";

export const DispatchTupleAbstractRenderer = <
  ItemFormStates extends Map<
    number,
    { commonFormState: { modifiedByUser: boolean } }
  >,
  Context extends FormLabel & {
    disabled: boolean;
    type: DispatchParsedType<any>;
  },
  ForeignMutationsExpected,
>(
  ItemFormStates: Map<number, () => any>,
  itemTemplates: Map<
    number,
    Template<
      Value<PredicateValue> & {
        commonFormState: { modifiedByUser: boolean };
        type: DispatchParsedType<any>;
        bindings: Bindings;
      },
      any,
      {
        onChange: DispatchOnChange<PredicateValue>;
      }
    >
  >,
) => {
  const embeddedItemTemplates = (itemIndex: number) =>
    itemTemplates
      .get(itemIndex)!
      .mapContext(
        (
          _: Context &
            Value<ValueTuple> &
            TupleAbstractRendererState & {
              bindings: Bindings;
              extraContext: any;
            },
        ): Value<PredicateValue> & {
          commonFormState: { modifiedByUser: boolean };
          type: DispatchParsedType<any>;
          bindings: Bindings;
        } => ({
          ...(_.itemFormStates.get(itemIndex) ||
            ItemFormStates.get(itemIndex)!()),
          value: _.value.values.get(itemIndex),
          disabled: _.disabled,
          type: _.type,
          bindings: _.bindings,
          extraContext: _.extraContext,
        }),
      )
      .mapState(
        (_: BasicUpdater<any>): Updater<TupleAbstractRendererState> =>
          TupleAbstractRendererState().Updaters.Template.upsertItemFormState(
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
            props.setState((_) => ({
              ..._,
              commonFormState: {
                ..._.commonFormState,
                modifiedByUser: true,
              },
              itemFormStates: MapRepo.Updaters.upsert(
                itemIndex,
                ItemFormStates.get(itemIndex)!,
                (__: any) => ({
                  ...__,
                  ...__.commonFormState,
                  modifiedByUser: true,
                }),
              )(_.itemFormStates) as unknown as ItemFormStates,
            }));
          },
        }),
      );

  return Template.Default<
    Context & Value<ValueTuple> & { disabled: boolean },
    TupleAbstractRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueTuple>;
    },
    TupleAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => {
    return (
      <>
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
