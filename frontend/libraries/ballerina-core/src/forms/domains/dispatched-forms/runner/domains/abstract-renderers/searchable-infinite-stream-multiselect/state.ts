import { SearchableInfiniteStreamAbstractRendererState } from "../searchable-infinite-stream/state";
import { FormLabel } from "../../../../../singleton/domains/form-label/state";
import { View } from "../../../../../../../template/state";
import { Value } from "../../../../../../../value/state";
import { ValueRecord } from "../../../../../parser/domains/predicates/state";
import { CollectionReference } from "../../../../../collection/domains/reference/state";
import {
  SimpleCallback,
  DispatchOnChange,
  DomNodeIdReadonlyContext,
} from "../../../../../../../../main";

export type InfiniteStreamMultiselectAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<ValueRecord> &
    DomNodeIdReadonlyContext &
    SearchableInfiniteStreamAbstractRendererState & {
      hasMoreValues: boolean;
      isLoading: boolean;
      availableOptions: Array<CollectionReference>;
      disabled: boolean;
    },
  SearchableInfiniteStreamAbstractRendererState,
  ForeignMutationsExpected & {
    onChange: DispatchOnChange<ValueRecord>;
    toggleOpen: SimpleCallback<void>;
    clearSelection: SimpleCallback<void>;
    setSearchText: SimpleCallback<string>;
    toggleSelection: SimpleCallback<ValueRecord>;
    loadMore: SimpleCallback<void>;
    reload: SimpleCallback<void>;
  }
>;
