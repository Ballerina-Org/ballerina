import { Map, Set } from "immutable";

import {
  BasicUpdater,
  FormLabel,
  MapRepo,
  PredicateValue,
  simpleUpdater,
  Template,
  Updater,
  Value,
  ValueRecord,
  View,
  DispatchCommonFormState,
  DispatchOnChange,
  FieldName,
  DomNodeIdReadonlyContext,
  Unit,
} from "../../../../../../../../main";

export type RecordAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  fieldStates: Map<string, any>;
};

export const RecordAbstractRendererState = {
  Default: {
    zero: (): RecordAbstractRendererState => ({
      commonFormState: DispatchCommonFormState.Default(),
      fieldStates: Map(),
    }),
    fieldState: (
      fieldStates: RecordAbstractRendererState["fieldStates"],
    ): RecordAbstractRendererState => ({
      commonFormState: DispatchCommonFormState.Default(),
      fieldStates,
    }),
  },
  Updaters: {
    Core: {
      ...simpleUpdater<RecordAbstractRendererState>()("commonFormState"),
      fieldStates: (
        _: Updater<RecordAbstractRendererState["fieldStates"]>,
      ): Updater<RecordAbstractRendererState> =>
        Updater((state) => ({
          ...state,
          fieldStates: _(state.fieldStates),
        })),
    },
    Template: {
      upsertFieldState: (
        fieldName: string,
        defaultState: () => any,
        updater: BasicUpdater<any>,
      ): Updater<RecordAbstractRendererState> =>
        RecordAbstractRendererState.Updaters.Core.fieldStates(
          MapRepo.Updaters.upsert(fieldName, defaultState, updater),
        ),
    },
  },
};
export type RecordAbstractRendererView<
  Context,
  ForeignMutationsExpected,
  Flags = Unit,
> =
  View<
    Context &
      Value<ValueRecord> &
      RecordAbstractRendererState &
      DomNodeIdReadonlyContext,
    RecordAbstractRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueRecord, Flags>;
    },
    {
      EmbeddedFields: Map<
        string,
        (flags: Flags | undefined) => Template<
          Context & Value<PredicateValue> & any,
          any,
          ForeignMutationsExpected & {
            onChange: DispatchOnChange<ValueRecord, Flags>;
          }
        >
      >;
      VisibleFieldKeys: Set<string>;
      DisabledFieldKeys: Set<string>;
      FieldLabels: Map<FieldName, string | undefined>;
    }
  >;
