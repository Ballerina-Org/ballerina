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
  DispatchOnChange,
  DomNodeIdReadonlyContext,
} from "../../../../../../../../main";
import { Debounced } from "../../../../../../../debounced/state";
import { Value } from "../../../../../../../value/state";

export type SearchableInfiniteStreamAbstractRendererState = {
  commonFormState: DispatchCommonFormState;
  customFormState: {
    searchText: Debounced<Value<string>>;
    status: "open" | "closed";
    stream: InfiniteStreamState<CollectionReference>;
    getChunk: BasicFun<
      string,
      InfiniteStreamState<CollectionReference>["getChunk"]
    >;
  };
};

export const SearchableInfiniteStreamAbstractRendererState = {
  Default: (
    getChunk: BasicFun<
      string,
      InfiniteStreamState<CollectionReference>["getChunk"]
    >,
  ): SearchableInfiniteStreamAbstractRendererState => ({
    commonFormState: DispatchCommonFormState.Default(),
    customFormState: {
      searchText: Debounced.Default(Value.Default("")),
      status: "closed",
      getChunk,
      stream: InfiniteStreamState<CollectionReference>().Default(
        10,
        getChunk(""),
      ),
    },
  }),
  Updaters: {
    Core: {
      ...simpleUpdaterWithChildren<SearchableInfiniteStreamAbstractRendererState>()(
        {
          ...simpleUpdater<
            SearchableInfiniteStreamAbstractRendererState["customFormState"]
          >()("status"),
          ...simpleUpdater<
            SearchableInfiniteStreamAbstractRendererState["customFormState"]
          >()("stream"),
          ...simpleUpdater<
            SearchableInfiniteStreamAbstractRendererState["customFormState"]
          >()("searchText"),
        },
      )("customFormState"),
    },
    Template: {
      searchText: (
        _: BasicUpdater<string>,
      ): Updater<SearchableInfiniteStreamAbstractRendererState> =>
        SearchableInfiniteStreamAbstractRendererState.Updaters.Core.customFormState.children.searchText(
          Debounced.Updaters.Template.value(Value.Updaters.value(_)),
        ),
    },
  },
};
export type SearchableInfiniteStreamAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<ValueOption> &
    DomNodeIdReadonlyContext &
    SearchableInfiniteStreamAbstractRendererState & {
      hasMoreValues: boolean;
      disabled: boolean;
    },
  SearchableInfiniteStreamAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueOption>;
    toggleOpen: SimpleCallback<void>;
    clearSelection: SimpleCallback<void>;
    setSearchText: SimpleCallback<string>;
    select: SimpleCallback<ValueOption>;
    loadMore: SimpleCallback<void>;
    reload: SimpleCallback<void>;
  }
>;
