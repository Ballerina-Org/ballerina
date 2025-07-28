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
  Unit,
  Template,
  ValueRecord,
  RecordAbstractRendererState,
  ValueUnit,
  MapRepo,
  BasicFun2,
  Value,
  ValueCallbackWithOptionalFlags,
  VoidCallbackWithOptionalFlags,
  DispatchOnChange,
  CommonAbstractRendererState,
  DispatchDelta,
  CommonAbstractRendererViewOnlyReadonlyContext,
  BaseFlags,
  Sum,
  PredicateValue,
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
  getApi?: BasicFun<Guid, Promise<unknown>>;
  fromApiParser: (value: unknown) => ValueOrErrors<ValueRecord, string>;
  remoteEntityVersionIdentifier: string;
};

export type OneAbstractRendererState = CommonAbstractRendererState & {
  customFormState: {
    detailsState: RecordAbstractRendererState;
    previewStates: Map<string, RecordAbstractRendererState>;
    streamParams: Debounced<Value<Map<string, string>>>;
    status: "open" | "closed";
    stream: Sum<ValueInfiniteStreamState, "not initialized">;
    getChunkWithParams:
      | BasicFun<
          string,
          BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
        >
      | undefined;
  };
};

export const OneAbstractRendererState = {
  Default: (
    getChunk:
      | BasicFun<
          string,
          BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
        >
      | undefined,
  ): OneAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
    customFormState: {
      detailsState: RecordAbstractRendererState.Default.zero(),
      previewStates: Map(),
      streamParams: Debounced.Default(Value.Default(Map())),
      status: "closed",
      getChunkWithParams: getChunk,
      stream: Sum.Default.right("not initialized"),
    },
  }),
  Updaters: {
    Core: {
      ...simpleUpdaterWithChildren<OneAbstractRendererState>()({
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
          "previewStates",
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
    },
  },
  Operations: {
    GetIdFromContext: <CustomPresentationContext = Unit, ExtraContext = Unit>(
      ctx: OneAbstractRendererReadonlyContext<
        CustomPresentationContext,
        ExtraContext
      >,
    ): ValueOrErrors<string, string | undefined> => {
      if (ctx.value == undefined) {
        return ValueOrErrors.Default.throwOne(undefined);
      }

      /// When initailising, in both stages, inject the id to the get chunk

      const local = ctx.bindings.get("local");
      if (local == undefined) {
        return ValueOrErrors.Default.throwOne(
          `local binding is undefined when intialising one`,
        );
      }

      if (!PredicateValue.Operations.IsRecord(local)) {
        return ValueOrErrors.Default.throwOne(
          `local binding is not a record when intialising one\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[one]"}`,
        );
      }

      if (!local.fields.has("Id")) {
        return ValueOrErrors.Default.throwOne(
          `local binding is missing Id (check casing) when intialising one\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[one]"}`,
        );
      }

      const id = local.fields.get("Id")!; // safe because of above check;
      if (!PredicateValue.Operations.IsString(id)) {
        return ValueOrErrors.Default.throwOne(
          `local Id is not a string when intialising one\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[one]"}`,
        );
      }

      return ValueOrErrors.Default.return(id);
    },
  },
};

export type OneAbstractRendererForeignMutationsExpected<Flags = BaseFlags> = {
  onChange: DispatchOnChange<ValueOption | ValueUnit, BaseFlags>;
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

export type OneAbstractRendererViewForeignMutationsExpected<Flags = BaseFlags> =
  {
    onChange: DispatchOnChange<ValueOption | ValueUnit, Flags>;
    toggleOpen: SimpleCallback<void>;
    setStreamParam: BasicFun2<string, string, void>;
    select: ValueCallbackWithOptionalFlags<ValueRecord, Flags>;
    create: ValueCallbackWithOptionalFlags<ValueRecord, Flags>;
    delete?: VoidCallbackWithOptionalFlags<Flags>;
    clear?: SimpleCallback<void>;
    loadMore: SimpleCallback<void>;
  };

export type OneAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
> = View<
  OneAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > & {
    hasMoreValues: boolean;
  } & CommonAbstractRendererViewOnlyReadonlyContext &
    OneAbstractRendererState,
  OneAbstractRendererState,
  OneAbstractRendererViewForeignMutationsExpected<Flags>,
  {
    DetailsRenderer?: (
      flags: Flags | undefined,
    ) => Template<
      OneAbstractRendererState &
        OneAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        >,
      OneAbstractRendererState,
      OneAbstractRendererViewForeignMutationsExpected<Flags>
    >;
    PreviewRenderer?: (
      value: ValueRecord,
    ) => (
      id: string,
    ) => (
      flags: Flags | undefined,
    ) => Template<
      OneAbstractRendererState &
        OneAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        >,
      OneAbstractRendererState,
      OneAbstractRendererViewForeignMutationsExpected<Flags>
    >;
  }
>;
