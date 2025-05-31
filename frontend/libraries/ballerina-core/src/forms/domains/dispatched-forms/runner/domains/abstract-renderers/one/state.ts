import { Map } from "immutable";

import {
  BasicFun,
  BasicUpdater,
  CollectionReference,
  FormLabel,
  InfiniteStreamState,
  SimpleCallback,
  Updater,
  ValueOption,
  View,
  simpleUpdater,
  simpleUpdaterWithChildren,
  DispatchCommonFormState,
  ValueInfiniteStreamState,
  CommonAbstractRendererReadonlyContext,
  OneType,
  DispatchOneSource,
  DispatchTableApiSource,
  PredicateValue,
  ValueOrErrors,
  Guid,
  AsyncState,
  Synchronized,
  Unit,
  unit,
  CommonAbstractRendererState,
  Template,
  ValueRecord,
  RecordAbstractRendererState,
  ValueUnit,
  DispatchPrimitiveType,
  DispatchOnChange,
  DomNodeIdReadonlyContext,
  MapRepo,
  BasicFun2,
  Value,
} from "../../../../../../../../main";
import { Debounced } from "../../../../../../../debounced/state";

export type OneAbstractRendererReadonlyContext =
  CommonAbstractRendererReadonlyContext<
    OneType<any>,
    ValueOption | ValueUnit
  > & {
    getApi: BasicFun<Guid, Promise<any>>;
    fromApiParser: (value: any) => ValueOrErrors<ValueRecord, string>;
  };

export type OneAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: {
    detailsState: RecordAbstractRendererState;
    selectedValue: Synchronized<
      Unit,
      ValueOrErrors<ValueRecord | ValueUnit, string>
    >;
    streamParams: Debounced<Value<Map<string, string>>>;
    status: "open" | "closed";
    stream: ValueInfiniteStreamState;
    getChunkWithParams: BasicFun<
      string,
      BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
    >;
  };
};

export const OneAbstractRendererState = {
  Default: (
    getChunk: BasicFun<
      string,
      BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
    >,
  ): OneAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {
      detailsState: RecordAbstractRendererState.Default.zero(),
      selectedValue: Synchronized.Default(unit),
      streamParams: Debounced.Default(Value.Default(Map())),
      status: "closed",
      getChunkWithParams: getChunk,
      stream: ValueInfiniteStreamState.Default(10, getChunk("")(Map())), // always overriden during initialisation to inject id
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
};
export type OneAbstractRendererView<Context> = View<
  (
    | (Omit<OneAbstractRendererReadonlyContext, "value"> & {
        value: ValueRecord | ValueUnit;
      } & OneAbstractRendererState & {
          hasMoreValues: boolean;
          disabled: boolean;
          kind: "initialized";
        })
    | {
        kind: "uninitialized";
      }
  ) &
    DomNodeIdReadonlyContext &
    Context,
  OneAbstractRendererState,
  | {
      kind: "initialized";
      onChange: DispatchOnChange<ValueRecord | ValueUnit>;
      toggleOpen: SimpleCallback<void>;
      // clearSelection: SimpleCallback<void>;
      setStreamParam: BasicFun2<string, string, void>;
      select: SimpleCallback<ValueRecord | ValueUnit>;
      loadMore: SimpleCallback<void>;
      reload: SimpleCallback<void>;
    }
  | {
      kind: "uninitialized";
    },
  | {
      kind: "initialized";
      DetailsRenderer: Template<any, any, any, any>;
      PreviewRenderer?: (value: ValueRecord) => Template<any, any, any, any>;
    }
  | {
      kind: "uninitialized";
    }
>;
