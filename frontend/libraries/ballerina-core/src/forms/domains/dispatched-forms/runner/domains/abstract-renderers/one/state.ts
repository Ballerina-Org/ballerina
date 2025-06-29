import { Map } from "immutable";

import {
  BasicFun,
  BasicUpdater,
  SimpleCallback,
  Updater,
  ValueOption,
  View,
  simpleUpdater,
  simpleUpdaterWithChildren,
  ValueInfiniteStreamState,
  CommonAbstractRendererReadonlyContext,
  OneType,
  ValueOrErrors,
  Guid,
  Synchronized,
  Unit,
  Template,
  ValueRecord,
  RecordAbstractRendererState,
  ValueUnit,
  MapRepo,
  BasicFun2,
  Value,
  replaceWith,
  ValueCallbackWithOptionalFlags,
  VoidCallbackWithOptionalFlags,
  DispatchOnChange,
  CommonAbstractRendererState,
  DispatchDelta,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";
import { Debounced } from "../../../../../../../debounced/state";

export type OneAbstractRendererReadonlyContext<
  CustomPresentationContext,
  ExtraContext,
> = CommonAbstractRendererReadonlyContext<
  OneType<unknown>,
  ValueOption | ValueUnit,
  CustomPresentationContext,
  ExtraContext
> & {
  getApi: BasicFun<Guid, Promise<unknown>>;
  fromApiParser: (value: unknown) => ValueOrErrors<ValueRecord, string>;
  remoteEntityVersionIdentifier: string;
};

export type OneAbstractRendererState = CommonAbstractRendererState & {
  customFormState: {
    detailsState: RecordAbstractRendererState;
    selectedValue: Synchronized<
      ValueUnit,
      ValueOrErrors<ValueRecord | ValueUnit, string>
    >;
    streamParams: Debounced<Value<Map<string, string>>>;
    status: "open" | "closed";
    stream: ValueInfiniteStreamState;
    getChunkWithParams: BasicFun<
      string,
      BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
    >;
    initializationStatus: "not initialized" | "initialized" | "reinitializing";
    previousRemoteEntityVersionIdentifier: string;
    shouldReinitialize: boolean;
  };
};

export const OneAbstractRendererState = {
  Default: (
    getChunk: BasicFun<
      string,
      BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
    >,
  ): OneAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
    customFormState: {
      detailsState: RecordAbstractRendererState.Default.zero(),
      selectedValue: Synchronized.Default(ValueUnit.Default()),
      streamParams: Debounced.Default(Value.Default(Map())),
      status: "closed",
      getChunkWithParams: getChunk,
      stream: ValueInfiniteStreamState.Default(10, getChunk("")(Map())), // always overriden during initialisation to inject id
      initializationStatus: "not initialized",
      previousRemoteEntityVersionIdentifier: "",
      shouldReinitialize: false,
    },
  }),
  Updaters: {
    Core: {
      ...simpleUpdaterWithChildren<OneAbstractRendererState>()({
        ...simpleUpdater<OneAbstractRendererState["customFormState"]>()(
          "selectedValue",
        ),
        ...simpleUpdater<OneAbstractRendererState["customFormState"]>()(
          "status",
        ),
        ...simpleUpdater<OneAbstractRendererState["customFormState"]>()(
          "stream",
        ),
        ...simpleUpdater<OneAbstractRendererState["customFormState"]>()(
          "streamParams",
        ),
        ...simpleUpdater<OneAbstractRendererState["customFormState"]>()(
          "detailsState",
        ),
        ...simpleUpdater<OneAbstractRendererState["customFormState"]>()(
          "previousRemoteEntityVersionIdentifier",
        ),
        ...simpleUpdater<OneAbstractRendererState["customFormState"]>()(
          "shouldReinitialize",
        ),
        ...simpleUpdater<OneAbstractRendererState["customFormState"]>()(
          "initializationStatus",
        ),
      })("customFormState"),
      ...simpleUpdaterWithChildren<OneAbstractRendererState>()({
        ...simpleUpdater<OneAbstractRendererState["commonFormState"]>()(
          "modifiedByUser",
        ),
      })("commonFormState"),
    },
    Template: {
      streamParam: (
        key: string,
        _: BasicUpdater<string>,
      ): Updater<OneAbstractRendererState> =>
        OneAbstractRendererState.Updaters.Core.customFormState.children.streamParams(
          Debounced.Updaters.Template.value(
            Value.Updaters.value(MapRepo.Updaters.upsert(key, () => "", _)),
          ),
        ),
      shouldReinitialize: (_: boolean) =>
        OneAbstractRendererState.Updaters.Core.customFormState.children.shouldReinitialize(
          replaceWith(_),
        ),
    },
  },
};

export type OneAbstractRendererForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<ValueOption | ValueUnit, Flags>;
  clear?: () => void;
  delete?: (delta: DispatchDelta<Flags>) => void;
  select?: (
    updater: BasicUpdater<ValueOption | ValueUnit>,
    delta: DispatchDelta<Flags>,
  ) => void;
  create?: (
    updater: BasicUpdater<ValueOption | ValueUnit>,
    delta: DispatchDelta<Flags>,
  ) => void;
};

export type OneAbstractRendererViewForeignMutationsExpected<Flags = Unit> = {
  onChange: DispatchOnChange<ValueOption | ValueUnit, Flags>;
  toggleOpen: SimpleCallback<void>;
  setStreamParam: BasicFun2<string, string, void>;
  select: ValueCallbackWithOptionalFlags<ValueRecord, Flags>;
  create: ValueCallbackWithOptionalFlags<ValueRecord, Flags>;
  delete?: VoidCallbackWithOptionalFlags<Flags>;
  clear?: SimpleCallback<void>;
  loadMore: SimpleCallback<void>;
  reload: SimpleCallback<void>;
};

export type OneAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  (
    | (Omit<
        OneAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        >,
        "value"
      > & {
        value: ValueRecord | ValueUnit;
      } & OneAbstractRendererState & {
          kind: "initialized";
          hasMoreValues: boolean;
        } & CommonAbstractRendererViewOnlyReadonlyContext)
    | {
        kind: "uninitialized";
        domNodeId: string;
      }
  ) &
    OneAbstractRendererState,
  OneAbstractRendererState,
  | ({
      kind: "initialized";
    } & OneAbstractRendererViewForeignMutationsExpected<Flags>)
  | {
      kind: "uninitialized";
    },
  | {
      kind: "initialized";
      DetailsRenderer: (flags: Flags | undefined) => Template<
        Omit<
          OneAbstractRendererReadonlyContext<
            CustomPresentationContext,
            ExtraContext
          >,
          "value"
        > & {
          value: ValueRecord | ValueUnit;
        } & OneAbstractRendererState,
        OneAbstractRendererState,
        OneAbstractRendererViewForeignMutationsExpected<Flags>
      >;
      PreviewRenderer?: (value: ValueRecord) => (
        flags: Flags | undefined,
      ) => Template<
        Omit<
          OneAbstractRendererReadonlyContext<
            CustomPresentationContext,
            ExtraContext
          >,
          "value"
        > & {
          value: ValueRecord | ValueUnit;
        } & OneAbstractRendererState,
        OneAbstractRendererState,
        OneAbstractRendererViewForeignMutationsExpected<Flags>
      >;
    }
  | {
      kind: "uninitialized";
    }
>;
