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
  ReferenceType,
  ValueOrErrors,
  Guid,
  Unit,
  Template,
  ValueRecord,
  RecordAbstractRendererState,
  ValueUnit,
  MapRepo,
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
  replaceWith,
} from "../../../../../../../../main";
import { Debounced } from "../../../../../../../debounced/state";

export type ReferenceAbstractRendererReadonlyContext<
  CustomPresentationContext,
  ExtraContext,
> = CommonAbstractRendererReadonlyContext<
  ReferenceType<unknown>,
  ValueOption | ValueUnit,
  CustomPresentationContext,
  ExtraContext
> & {
  getApi?: BasicFun<Guid, Promise<unknown>>;
  fromApiParser: (value: unknown) => ValueOrErrors<ValueRecord, string>;
  remoteEntityVersionIdentifier: string;
};

export type ReferenceAbstractRendererState = CommonAbstractRendererState & {
  customFormState: {
    detailsState: RecordAbstractRendererState;
    previewStates: Map<string, RecordAbstractRendererState>;
    streamParams: Debounced<Value<[Map<string, string>, boolean]>>;
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

export const ReferenceAbstractRendererState = {
  Default: (
    getChunk:
      | BasicFun<
          string,
          BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
        >
      | undefined,
  ): ReferenceAbstractRendererState => ({
    ...CommonAbstractRendererState.Default(),
    customFormState: {
      detailsState: RecordAbstractRendererState.Default.zero(),
      previewStates: Map(),
      streamParams: Debounced.Default(Value.Default([Map(), false])),
      status: "closed",
      getChunkWithParams: getChunk,
      stream: Sum.Default.right("not initialized"),
    },
  }),
  Updaters: {
    Core: {
      ...simpleUpdaterWithChildren<ReferenceAbstractRendererState>()({
        ...simpleUpdater<ReferenceAbstractRendererState["customFormState"]>()(
          "status",
        ),
        ...simpleUpdater<ReferenceAbstractRendererState["customFormState"]>()(
          "stream",
        ),
        ...simpleUpdater<ReferenceAbstractRendererState["customFormState"]>()(
          "streamParams",
        ),
        ...simpleUpdater<ReferenceAbstractRendererState["customFormState"]>()(
          "detailsState",
        ),
        ...simpleUpdater<ReferenceAbstractRendererState["customFormState"]>()(
          "previewStates",
        ),
      })("customFormState"),
      ...simpleUpdaterWithChildren<ReferenceAbstractRendererState>()({
        ...simpleUpdater<ReferenceAbstractRendererState["commonFormState"]>()(
          "modifiedByUser",
        ),
      })("commonFormState"),
    },
    Template: {
      streamParam: (
        key: string,
        _: BasicUpdater<string>,
        shouldReload: boolean,
      ): Updater<ReferenceAbstractRendererState> =>
        ReferenceAbstractRendererState.Updaters.Core.customFormState.children.streamParams(
          Debounced.Updaters.Template.value(
            Value.Updaters.value((__) => [
              MapRepo.Updaters.upsert(key, () => "", _)(__[0]),
              shouldReload,
            ]),
          ),
        ),
    },
  },
  Operations: {
    GetIdFromContext: <CustomPresentationContext = Unit, ExtraContext = Unit>(
      ctx: ReferenceAbstractRendererReadonlyContext<
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
          `local binding is undefined when intialising reference`,
        );
      }

      if (!PredicateValue.Operations.IsRecord(local)) {
        return ValueOrErrors.Default.throwOne(
          `local binding is not a record when intialising reference\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[reference]"}`,
        );
      }

      if (!local.fields.has("Id")) {
        return ValueOrErrors.Default.throwOne(
          `local binding is missing Id (check casing) when intialising reference\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[reference]"}`,
        );
      }

      const id = local.fields.get("Id")!; // safe because of above check;
      if (!PredicateValue.Operations.IsString(id)) {
        return ValueOrErrors.Default.throwOne(
          `local Id is not a string when intialising reference\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[reference]"}`,
        );
      }

      return ValueOrErrors.Default.return(id);
    },
  },
};

export type ReferenceAbstractRendererForeignMutationsExpected<Flags = BaseFlags> = {
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

export type ReferenceAbstractRendererViewForeignMutationsExpected<Flags = BaseFlags> =
  {
    onChange: DispatchOnChange<ValueOption | ValueUnit, Flags>;
    toggleOpen: SimpleCallback<void>;
    setStreamParam: (key: string, value: string, shouldReload: boolean) => void;
    select: ValueCallbackWithOptionalFlags<ValueRecord, Flags>;
    create: ValueCallbackWithOptionalFlags<ValueRecord, Flags>;
    delete?: VoidCallbackWithOptionalFlags<Flags>;
    clear?: SimpleCallback<void>;
    loadMore: SimpleCallback<void>;
    reinitializeStream: SimpleCallback<void>;
  };

export type ReferenceAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
> = View<
  ReferenceAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > & {
    hasMoreValues: boolean;
  } & CommonAbstractRendererViewOnlyReadonlyContext &
    ReferenceAbstractRendererState,
  ReferenceAbstractRendererState,
  ReferenceAbstractRendererViewForeignMutationsExpected<Flags>,
  {
    DetailsRenderer?: (
      flags: Flags | undefined,
    ) => Template<
      ReferenceAbstractRendererState &
        ReferenceAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        >,
      ReferenceAbstractRendererState,
      ReferenceAbstractRendererViewForeignMutationsExpected<Flags>
    >;
    PreviewRenderer?: (
      value: ValueRecord,
    ) => (
      id: string,
    ) => (
      flags: Flags | undefined,
    ) => Template<
      ReferenceAbstractRendererState &
        ReferenceAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        >,
      ReferenceAbstractRendererState,
      ReferenceAbstractRendererViewForeignMutationsExpected<Flags>
    >;
  }
>;
