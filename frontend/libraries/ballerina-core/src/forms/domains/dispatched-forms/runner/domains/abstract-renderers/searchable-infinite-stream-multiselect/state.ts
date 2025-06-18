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
} from "../../../../../../../../main";

export type InfiniteStreamMultiselectAbstractRendererReadonlyContext<
  CustomContext = Unit,
> = CommonAbstractRendererReadonlyContext<
  MultiSelectionType<unknown>,
  ValueRecord,
  CustomContext
>;

export type InfiniteStreamMultiselectAbstractRendererState =
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

export const InfiniteStreamMultiselectAbstractRendererState = {
  Default: (
    getChunk: BasicFun<
      string,
      InfiniteStreamState<CollectionReference>["getChunk"]
    >,
  ): InfiniteStreamMultiselectAbstractRendererState => ({
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
      ...simpleUpdaterWithChildren<InfiniteStreamMultiselectAbstractRendererState>()(
        {
          ...simpleUpdater<
            InfiniteStreamMultiselectAbstractRendererState["customFormState"]
          >()("status"),
          ...simpleUpdater<
            InfiniteStreamMultiselectAbstractRendererState["customFormState"]
          >()("stream"),
          ...simpleUpdater<
            InfiniteStreamMultiselectAbstractRendererState["customFormState"]
          >()("searchText"),
        },
      )("customFormState"),
    },
    Template: {
      searchText: (
        _: BasicUpdater<string>,
      ): Updater<InfiniteStreamMultiselectAbstractRendererState> =>
        InfiniteStreamMultiselectAbstractRendererState.Updaters.Core.customFormState.children.searchText(
          Debounced.Updaters.Template.value(Value.Updaters.value(_)),
        ),
    },
  },
};

export type InfiniteStreamMultiselectAbstractRendererForeignMutationsExpected<
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

export type InfiniteStreamMultiselectAbstractRendererView<
  CustomContext = Unit,
  Flags = Unit,
> = View<
  InfiniteStreamMultiselectAbstractRendererReadonlyContext<CustomContext> &
    InfiniteStreamMultiselectAbstractRendererState & {
      hasMoreValues: boolean;
      isLoading: boolean;
      availableOptions: Array<CollectionReference>;
      disabled: boolean;
    },
  InfiniteStreamMultiselectAbstractRendererState,
  InfiniteStreamMultiselectAbstractRendererForeignMutationsExpected<Flags>
>;
