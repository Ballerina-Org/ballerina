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
  Unit,
  VoidCallbackWithOptionalFlags,
  ValueCallbackWithOptionalFlags,
  CommonAbstractRendererReadonlyContext,
  SingleSelectionType,
  CommonAbstractRendererState,
} from "../../../../../../../../main";
import { Debounced } from "../../../../../../../debounced/state";
import { Value } from "../../../../../../../value/state";

export type SearchableInfiniteStreamAbstractRendererReadonlyContext<
  CustomContext,
> = CommonAbstractRendererReadonlyContext<
  SingleSelectionType<unknown>,
  ValueOption,
  CustomContext
>;

export type SearchableInfiniteStreamAbstractRendererState =
  CommonAbstractRendererState & {
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
    ...CommonAbstractRendererState.Default(),
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

export type SearchableInfiniteStreamAbstractRendererForeignMutationsExpected<
  Flags = Unit,
> = {
  onChange: DispatchOnChange<ValueOption, Flags>;
};

export type SearchableInfiniteStreamAbstractViewRendererForeignMutationsExpected<
  Flags = Unit,
> = {
  onChange: DispatchOnChange<ValueOption, Flags>;
  toggleOpen: SimpleCallback<void>;
  clearSelection: VoidCallbackWithOptionalFlags<Flags>;
  setSearchText: SimpleCallback<string>;
  select: ValueCallbackWithOptionalFlags<ValueOption, Flags>;
  loadMore: SimpleCallback<void>;
  reload: SimpleCallback<void>;
};

export type SearchableInfiniteStreamAbstractRendererView<
  CustomContext = Unit,
  Flags = Unit,
> = View<
  SearchableInfiniteStreamAbstractRendererReadonlyContext<CustomContext> &
    SearchableInfiniteStreamAbstractRendererState & {
      hasMoreValues: boolean;
    },
  SearchableInfiniteStreamAbstractRendererState,
  SearchableInfiniteStreamAbstractViewRendererForeignMutationsExpected<Flags>
>;
