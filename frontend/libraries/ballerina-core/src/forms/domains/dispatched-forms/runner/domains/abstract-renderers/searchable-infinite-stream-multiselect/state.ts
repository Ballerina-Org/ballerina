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
  VoidCallbackWithOptionalFlags,
  Unit,
  ValueCallbackWithOptionalFlags,
} from "../../../../../../../../main";

export type InfiniteStreamMultiselectAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
  Flags = Unit,
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
    onChange: DispatchOnChange<ValueRecord, Flags>;
    toggleOpen: SimpleCallback<void>;
    clearSelection: VoidCallbackWithOptionalFlags<Flags>;
    setSearchText: SimpleCallback<string>;
    replace: ValueCallbackWithOptionalFlags<ValueRecord, Flags>;
    toggleSelection: ValueCallbackWithOptionalFlags<ValueRecord, Flags>;
    loadMore: SimpleCallback<void>;
    reload: SimpleCallback<void>;
  }
>;
