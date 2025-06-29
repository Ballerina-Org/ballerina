import { View } from "../../../../../../../template/state";
import { Value } from "../../../../../../../value/state";
import { ValueRecord } from "../../../../../parser/domains/predicates/state";
import { CollectionReference } from "../../../../../collection/domains/reference/state";
import {
  SimpleCallback,
  DispatchOnChange,
  VoidCallbackWithOptionalFlags,
  Unit,
  ValueCallbackWithOptionalFlags,
  CommonAbstractRendererReadonlyContext,
  MultiSelectionType,
  CommonAbstractRendererState,
  Debounced,
  InfiniteStreamState,
  BasicFun,
  simpleUpdaterWithChildren,
  simpleUpdater,
  BasicUpdater,
  Updater,
  CommonAbstractRendererViewOnlyReadonlyContext,
} from "../../../../../../../../main";

export type SearchableInfiniteStreamMultiselectAbstractRendererReadonlyContext<
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = CommonAbstractRendererReadonlyContext<
  MultiSelectionType<unknown>,
  ValueRecord,
  CustomPresentationContext,
  ExtraContext
>;

export type SearchableInfiniteStreamMultiselectAbstractRendererState =
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

export const SearchableInfiniteStreamMultiselectAbstractRendererState = {
  Default: (
    getChunk: BasicFun<
      string,
      InfiniteStreamState<CollectionReference>["getChunk"]
    >,
  ): SearchableInfiniteStreamMultiselectAbstractRendererState => ({
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
      ...simpleUpdaterWithChildren<SearchableInfiniteStreamMultiselectAbstractRendererState>()(
        {
          ...simpleUpdater<
            SearchableInfiniteStreamMultiselectAbstractRendererState["customFormState"]
          >()("status"),
          ...simpleUpdater<
            SearchableInfiniteStreamMultiselectAbstractRendererState["customFormState"]
          >()("stream"),
          ...simpleUpdater<
            SearchableInfiniteStreamMultiselectAbstractRendererState["customFormState"]
          >()("searchText"),
        },
      )("customFormState"),
    },
    Template: {
      searchText: (
        _: BasicUpdater<string>,
      ): Updater<SearchableInfiniteStreamMultiselectAbstractRendererState> =>
        SearchableInfiniteStreamMultiselectAbstractRendererState.Updaters.Core.customFormState.children.searchText(
          Debounced.Updaters.Template.value(Value.Updaters.value(_)),
        ),
    },
  },
};

export type SearchableInfiniteStreamMultiselectAbstractRendererForeignMutationsExpected<
  Flags = Unit,
> = {
  onChange: DispatchOnChange<ValueRecord, Flags>;
  toggleOpen: SimpleCallback<void>;
  clearSelection: VoidCallbackWithOptionalFlags<Flags>;
  setSearchText: SimpleCallback<string>;
  replace: ValueCallbackWithOptionalFlags<ValueRecord, Flags>;
  toggleSelection: ValueCallbackWithOptionalFlags<ValueRecord, Flags>;
  loadMore: SimpleCallback<void>;
  reload: SimpleCallback<void>;
};

export type SearchableInfiniteStreamMultiselectAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
> = View<
  SearchableInfiniteStreamMultiselectAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > &
    SearchableInfiniteStreamMultiselectAbstractRendererState & {
      hasMoreValues: boolean;
      isLoading: boolean;
      availableOptions: Array<CollectionReference>;
      disabled: boolean;
    } & CommonAbstractRendererViewOnlyReadonlyContext,
  SearchableInfiniteStreamMultiselectAbstractRendererState,
  SearchableInfiniteStreamMultiselectAbstractRendererForeignMutationsExpected<Flags>
>;
