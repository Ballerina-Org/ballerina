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
  ReferenceOneType,
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

export type ReferenceOneAbstractRendererReadonlyContext<
  CustomPresentationContext,
  ExtraContext,
> = CommonAbstractRendererReadonlyContext<
  ReferenceOneType<unknown>,
  ValueOption | ValueUnit,
  CustomPresentationContext,
  ExtraContext
> & {
  getApi?: BasicFun<Guid, Promise<unknown>>;
  fromApiParser: (value: unknown) => ValueOrErrors<ValueRecord, string>;
  remoteEntityVersionIdentifier: string;
};

export type ReferenceOneAbstractRendererState = CommonAbstractRendererState & {
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

export const ReferenceOneAbstractRendererState = {
  Default: (
    getChunk:
      | BasicFun<
          string,
          BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
        >
      | undefined,
  ): ReferenceOneAbstractRendererState => ({
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
      ...simpleUpdaterWithChildren<ReferenceOneAbstractRendererState>()({
        ...simpleUpdater<ReferenceOneAbstractRendererState["customFormState"]>()(
          "status",
        ),
        ...simpleUpdater<ReferenceOneAbstractRendererState["customFormState"]>()(
          "stream",
        ),
        ...simpleUpdater<ReferenceOneAbstractRendererState["customFormState"]>()(
          "streamParams",
        ),
        ...simpleUpdater<ReferenceOneAbstractRendererState["customFormState"]>()(
          "detailsState",
        ),
        ...simpleUpdater<ReferenceOneAbstractRendererState["customFormState"]>()(
          "previewStates",
        ),
      })("customFormState"),
      ...simpleUpdaterWithChildren<ReferenceOneAbstractRendererState>()({
        ...simpleUpdater<ReferenceOneAbstractRendererState["commonFormState"]>()(
          "modifiedByUser",
        ),
      })("commonFormState"),
    },
    Template: {
      streamParam: (
        key: string,
        _: BasicUpdater<string>,
        shouldReload: boolean,
      ): Updater<ReferenceOneAbstractRendererState> =>
        ReferenceOneAbstractRendererState.Updaters.Core.customFormState.children.streamParams(
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
      ctx: ReferenceOneAbstractRendererReadonlyContext<
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
          `local binding is undefined when intialising referenceOne`,
        );
      }

      if (!PredicateValue.Operations.IsRecord(local)) {
        return ValueOrErrors.Default.throwOne(
          `local binding is not a record when intialising referenceOne\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[referenceOne]"}`,
        );
      }

      if (!local.fields.has("Id")) {
        return ValueOrErrors.Default.throwOne(
          `local binding is missing Id (check casing) when intialising referenceOne\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[referenceOne]"}`,
        );
      }

      const id = local.fields.get("Id")!; // safe because of above check;
      if (!PredicateValue.Operations.IsString(id)) {
        return ValueOrErrors.Default.throwOne(
          `local Id is not a string when intialising referenceOne\n... in couroutine for\n...${ctx.domNodeAncestorPath + "[referenceOne]"}`,
        );
      }

      return ValueOrErrors.Default.return(id);
    },
  },
};

export type ReferenceOneAbstractRendererForeignMutationsExpected<Flags = BaseFlags> = {
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

export type ReferenceOneAbstractRendererViewForeignMutationsExpected<Flags = BaseFlags> =
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

export type ReferenceOneAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = BaseFlags,
  ExtraContext = Unit,
> = View<
  ReferenceOneAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > & {
    hasMoreValues: boolean;
  } & CommonAbstractRendererViewOnlyReadonlyContext &
    ReferenceOneAbstractRendererState,
  ReferenceOneAbstractRendererState,
  ReferenceOneAbstractRendererViewForeignMutationsExpected<Flags>,
  {
    DetailsRenderer?: (
      flags: Flags | undefined,
    ) => Template<
      ReferenceOneAbstractRendererState &
        ReferenceOneAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        >,
      ReferenceOneAbstractRendererState,
      ReferenceOneAbstractRendererViewForeignMutationsExpected<Flags>
    >;
    PreviewRenderer?: (
      value: ValueRecord,
    ) => (
      id: string,
    ) => (
      flags: Flags | undefined,
    ) => Template<
      ReferenceOneAbstractRendererState &
        ReferenceOneAbstractRendererReadonlyContext<
          CustomPresentationContext,
          ExtraContext
        >,
      ReferenceOneAbstractRendererState,
      ReferenceOneAbstractRendererViewForeignMutationsExpected<Flags>
    >;
  }
>;
