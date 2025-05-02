import {
  BasicUpdater,
  Bindings,
  DispatchDelta,
  ListRepo,
  MapRepo,
  PredicateValue,
  Updater,
  ValueTuple,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";
import { Value } from "../../../../../../../value/state";
import { FormLabel } from "../../../../../singleton/domains/form-label/state";
import {
  DispatchParsedType,
  ListType,
} from "../../../../deserializer/domains/specification/domains/types/state";
import { DispatchOnChange } from "../../../state";
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
            props.setState((_) => ({
              ..._,
              commonFormState: {
                ..._.commonFormState,
                modifiedByUser: true,
              },
              elementFormStates: MapRepo.Updaters.upsert(
                elementIndex,
                () => GetDefaultElementState(),
                (__) => ({
                  ...__,
                  commonFormState: {
                    ...__.commonFormState,
                    modifiedByUser: true,
                  },
                }),
              )(_.elementFormStates),
            }));
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
    return (
      <span className={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}>
        <props.view
          {...props}
          context={{
            ...props.context,
          }}
          foreignMutations={{
            ...props.foreignMutations,
            add: (_) => {
              const delta: DispatchDelta = {
                kind: "ArrayAdd",
                value: GetDefaultElementValue(),
                state: {
                  commonFormState: props.context.commonFormState,
                  elementFormStates: props.context.elementFormStates,
                },
                type: (props.context.type as ListType<any>).args[0],
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
            },
            remove: (_) => {
              const delta: DispatchDelta = {
                kind: "ArrayRemoveAt",
                index: _,
              };
              props.foreignMutations.onChange(
                Updater((list) =>
                  PredicateValue.Default.tuple(
                    ListRepo.Updaters.remove<PredicateValue>(_)(list.values),
                  ),
                ),
                delta,
              );
            },
            move: (index, to) => {
              const delta: DispatchDelta = {
                kind: "ArrayMoveFromTo",
                from: index,
                to: to,
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
            },
            duplicate: (_) => {
              const delta: DispatchDelta = {
                kind: "ArrayDuplicateAt",
                index: _,
              };
              props.foreignMutations.onChange(
                Updater((list) =>
                  PredicateValue.Default.tuple(
                    ListRepo.Updaters.duplicate<PredicateValue>(_)(list.values),
                  ),
                ),
                delta,
              );
            },
            insert: (_) => {
              const delta: DispatchDelta = {
                kind: "ArrayAddAt",
                value: [_, GetDefaultElementValue()],
                elementState: GetDefaultElementState(),
                elementType: (props.context.type as ListType<any>).args[0],
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
            },
          }}
          embeddedElementTemplate={embeddedElementTemplate}
        />
      </span>
    );
  }).any([]);
};
