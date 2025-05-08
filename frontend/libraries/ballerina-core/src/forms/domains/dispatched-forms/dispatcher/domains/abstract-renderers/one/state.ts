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
} from "../../../../../../../../main";
import { Debounced } from "../../../../../../../debounced/state";
import { Value } from "../../../../../../../value/state";
import { DispatchOnChange } from "../../../state";

export type OneAbstractRendererReadonlyContext =
  CommonAbstractRendererReadonlyContext<OneType<any>, ValueOption> & {
    getApi: BasicFun<Guid, Promise<any>>;
    fromApiParser: (value: any) => ValueOrErrors<ValueRecord, string>;
    id: Guid;
  };

export type OneAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: {
    detailsState: RecordAbstractRendererState;
    selectedValue: Synchronized<Unit, ValueOrErrors<ValueOption, string>>;
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
      stream: ValueInfiniteStreamState.Default(10, getChunk("")(Map())),
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
  OneAbstractRendererReadonlyContext &
    Value<ValueOption> &
    OneAbstractRendererState & {
      hasMoreValues: boolean;
      disabled: boolean;
    },
  OneAbstractRendererState,
  {
    onChange: DispatchOnChange<ValueOption>;
    toggleOpen: SimpleCallback<void>;
    // clearSelection: SimpleCallback<void>;
    setSearchText: SimpleCallback<string>;
    select: SimpleCallback<ValueOption>;
    loadMore: SimpleCallback<void>;
    reload: SimpleCallback<void>;
  },
  {
    DetailsRenderer: Template<any, any, any, any>;
    PreviewRenderer: (
      value: ValueRecord,
    ) => Template<any, any, any, any> | undefined;
  }
>;
