import { SimpleCallback, ValueRecord } from "../../../../../../main";
import { View } from "../../../../../template/state";
import { Value } from "../../../../../value/state";
import { CollectionReference } from "../../../collection/domains/reference/state";
import { FormLabel } from "../../../singleton/domains/form-label/state";
import { OnChange } from "../../../singleton/state";
import { SearchableInfiniteStreamState } from "../searchable-infinite-stream/state";

export type InfiniteStreamMultiselectView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<ValueRecord> &
    SearchableInfiniteStreamState & {
      hasMoreValues: boolean;
      isLoading: boolean;
      availableOptions: Array<CollectionReference>;
      disabled: boolean;
      visible: boolean;
    },
  SearchableInfiniteStreamState,
  ForeignMutationsExpected & {
    onChange: OnChange<ValueRecord>;
    toggleOpen: SimpleCallback<void>;
    clearSelection: SimpleCallback<void>;
    setSearchText: SimpleCallback<string>;
    toggleSelection: SimpleCallback<ValueRecord>;
    loadMore: SimpleCallback<void>;
    reload: SimpleCallback<void>;
  }
>;
