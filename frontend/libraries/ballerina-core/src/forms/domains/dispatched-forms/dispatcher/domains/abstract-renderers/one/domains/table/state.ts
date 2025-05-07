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
} from "../../../../../../../../../../main";
import { Debounced } from "../../../../../../../../../debounced/state";
import { Value } from "../../../../../../../../../value/state";
import { DispatchOnChange } from "../../../../../state";

export type OneTableAbstractRendererReadonlyContext =
  | CommonAbstractRendererReadonlyContext<OneType<any>, ValueOption> & {
      api: DispatchTableApiSource;
      fromTableApiParser: (value: any) => ValueOrErrors<PredicateValue, string>;
      id: Guid;
    };

export type OneTableAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: {
    selectedValue: Synchronized<Unit, ValueOption>;
    searchText: Debounced<Value<string>>;
    status: "open" | "closed";
    stream: ValueInfiniteStreamState;
    getChunkWithParams: BasicFun<
      string,
      BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
    >;
  };
};

export const OneTableAbstractRendererState = {
  Default: (
    getChunk: BasicFun<
      string,
      BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
    >,
  ): OneTableAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {
      selectedValue: Synchronized.Default(unit),
      searchText: Debounced.Default(Value.Default("")),
      status: "closed",
      getChunkWithParams: getChunk,
      stream: ValueInfiniteStreamState.Default(10, getChunk("")(Map())),
    },
  }),
  Updaters: {
    Core: {
      ...simpleUpdaterWithChildren<OneTableAbstractRendererState>()({
        ...simpleUpdater<OneTableAbstractRendererState["customFormState"]>()(
          "selectedValue",
        ),
        ...simpleUpdater<OneTableAbstractRendererState["customFormState"]>()(
          "status",
        ),
        ...simpleUpdater<OneTableAbstractRendererState["customFormState"]>()(
          "stream",
        ),
        ...simpleUpdater<OneTableAbstractRendererState["customFormState"]>()(
          "searchText",
        ),
      })("customFormState"),
    },
    Template: {
      searchText: (
        _: BasicUpdater<string>,
      ): Updater<OneTableAbstractRendererState> =>
        OneTableAbstractRendererState.Updaters.Core.customFormState.children.searchText(
          Debounced.Updaters.Template.value(Value.Updaters.value(_)),
        ),
    },
  },
};
export type OneTableAbstractRendererView = View<
  OneTableAbstractRendererReadonlyContext &
    Value<ValueOption> &
    OneTableAbstractRendererState & {
      hasMoreValues: boolean;
      disabled: boolean;
    },
  OneTableAbstractRendererState,
  {
    onChange: DispatchOnChange<ValueOption>;
    toggleOpen: SimpleCallback<void>;
    clearSelection: SimpleCallback<void>;
    setSearchText: SimpleCallback<string>;
    select: SimpleCallback<ValueOption>;
    loadMore: SimpleCallback<void>;
    reload: SimpleCallback<void>;
  }
>;
