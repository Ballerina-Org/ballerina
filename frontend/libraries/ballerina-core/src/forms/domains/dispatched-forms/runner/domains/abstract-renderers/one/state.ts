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
} from "../../../../../../../../main";
import { Debounced } from "../../../../../../../debounced/state";
import { Value } from "../../../../../../../value/state";

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
    searchText: Debounced<Value<string>>;
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
      searchText: Debounced.Default(Value.Default("")),
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
          "searchText",
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
      searchText: (
        _: BasicUpdater<string>,
      ): Updater<OneAbstractRendererState> =>
        OneAbstractRendererState.Updaters.Core.customFormState.children.searchText(
          Debounced.Updaters.Template.value(Value.Updaters.value(_)),
        ),
    },
  },
};
export type OneAbstractRendererView = View<
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
    DomNodeIdReadonlyContext,
  OneAbstractRendererState,
  | {
      kind: "initialized";
      onChange: DispatchOnChange<ValueRecord | ValueUnit>;
      toggleOpen: SimpleCallback<void>;
      // clearSelection: SimpleCallback<void>;
      setSearchText: SimpleCallback<string>;
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
      PreviewRenderer: (
        value: ValueRecord,
      ) => Template<any, any, any, any> | undefined;
    }
  | {
      kind: "uninitialized";
    }
>;
