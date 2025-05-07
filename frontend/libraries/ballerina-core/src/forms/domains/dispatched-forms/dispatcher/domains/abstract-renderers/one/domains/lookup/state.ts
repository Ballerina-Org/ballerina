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
} from "../../../../../../../../../../main";
import { Debounced } from "../../../../../../../../../debounced/state";
import { Value } from "../../../../../../../../../value/state";
import { DispatchOnChange } from "../../../../../state";

export type OneAbstractRendererReadonlyContext =
  | (CommonAbstractRendererReadonlyContext<OneType<any>, ValueOption> & {
      api: {
        kind: "one";
        source: DispatchOneSource;
      } | {
        kind: "table";
        source: DispatchTableApiSource;
      };
      fromTableApiParser: (value: any) => ValueOrErrors<PredicateValue, string>;
      fromOneApiParser: (value: any) => ValueOrErrors<PredicateValue, string>;
    })


export type OneAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: {
    searchText: Debounced<Value<string>>;
    status: "open" | "closed";
    stream: ValueInfiniteStreamState;
    getChunkWithParams: BasicFun<string, BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>>;
  };
};

export const OneAbstractRendererState = {
  Default: (
    getChunk: BasicFun<string, BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>>,
  ): OneAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {
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
          "status",
        ),
        ...simpleUpdater<OneAbstractRendererState["customFormState"]>()(
          "stream",
        ),
        ...simpleUpdater<OneAbstractRendererState["customFormState"]>()(
          "searchText",
        ),
      })("customFormState"),
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
    clearSelection: SimpleCallback<void>;
    setSearchText: SimpleCallback<string>;
    select: SimpleCallback<ValueOption>;
    loadMore: SimpleCallback<void>;
    reload: SimpleCallback<void>;
  }
>;
