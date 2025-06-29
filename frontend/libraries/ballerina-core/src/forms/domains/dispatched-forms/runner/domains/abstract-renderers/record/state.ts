import { Map, Set } from "immutable";

import {
  BasicUpdater,
  MapRepo,
  simpleUpdater,
  Template,
  Updater,
  ValueRecord,
  View,
  DispatchOnChange,
  FieldName,
  Unit,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  FormLayout,
  RecordType,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";

export type RecordAbstractRendererReadonlyContext<CustomPresentationContext> =
  CommonAbstractRendererReadonlyContext<
    RecordType<any>,
    ValueRecord,
    CustomPresentationContext
  >;

export type RecordAbstractRendererState = CommonAbstractRendererState & {
  fieldStates: Map<string, CommonAbstractRendererState>;
};

export const RecordAbstractRendererState = {
  Default: {
    zero: (): RecordAbstractRendererState => ({
      ...CommonAbstractRendererState.Default(),
      fieldStates: Map(),
    }),
    fieldState: (
      fieldStates: RecordAbstractRendererState["fieldStates"],
    ): RecordAbstractRendererState => ({
      ...CommonAbstractRendererState.Default(),
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

export type RecordAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<ValueRecord, Flags>;
};

export type RecordAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
> = View<
  RecordAbstractRendererReadonlyContext<CustomPresentationContext> &
    RecordAbstractRendererState & {
      layout: FormLayout;
    } & CommonAbstractRendererViewOnlyReadonlyContext,
  RecordAbstractRendererState,
  RecordAbstractRendererForeignMutationsExpected<Flags>,
  {
    EmbeddedFields: Map<
      string,
      (
        flags: Flags | undefined,
      ) => Template<
        RecordAbstractRendererReadonlyContext<CustomPresentationContext> &
          RecordAbstractRendererState,
        RecordAbstractRendererState,
        RecordAbstractRendererForeignMutationsExpected<Flags>
      >
    >;
    VisibleFieldKeys: Set<string>;
    DisabledFieldKeys: Set<string>;
    FieldLabels: Map<FieldName, string | undefined>;
  }
>;
