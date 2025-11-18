import {
  BaseFieldWithoutMetadata,
  BaseInfiniteDropdownMultiple,
  BaseInfiniteDropdownV3,
  BaseInputTags,
  BaseInputV3,
  BaseLabelV3,
} from "@blp-private-npm/ui";
import {
  PredicateValue,
  SearchableInfiniteStreamAbstractRendererView,
  SearchableInfiniteStreamMultiselectAbstractRendererView,
  ValueRecord,
} from "ballerina-core";
import { Map, OrderedMap } from "immutable";
import React from "react";
import { CustomPresentationContexts } from "../common/custom-presentation-contexts";
import { IdeFlags } from "../common/ide-flags";
import { translateForCustomDataDrivenTranslations, translateForDataDrivenTranslationsWithContext } from "../common/translate";
import { mapTypeToFlags } from "../common/map-type-to-flag";
import {IdeConcreteRenderers} from "../common/concrete-renderers.ts";
import { FieldExtraContext } from "../common/field-extra-context";

const InfiniteStreamComponent = (
  props: React.ComponentProps<
    SearchableInfiniteStreamAbstractRendererView<
      CustomPresentationContexts,
      IdeFlags,
      FieldExtraContext
    >
  >
) =>
  props.context.readOnly ? (
    <ReadonlyInfiniteStreamComponent {...props} />
  ) : (
    <EditableInfiniteStreamComponent {...props} />
  );

const EditableInfiniteStreamComponent = (
  props: React.ComponentProps<
    SearchableInfiniteStreamAbstractRendererView<
      CustomPresentationContexts,
      IdeFlags,
      FieldExtraContext
    >
  >
) => {
  const ddTranslations = translateForCustomDataDrivenTranslations(
    props.context.extraContext.locale,
    props.context.extraContext.namespace
  );

  const missingDisplayValueLabel = ddTranslations("fieldIsMissingDisplayValue");

  const flags =
    props.context.type.kind === "singleSelection"
      ? mapTypeToFlags({
          typeStr: props.context.type.args[0].asString(),
          ancestors: props.context.lookupTypeAncestorNames,
          documentType: "FIXME: Document Type",
        })
      : undefined;

  return (
    <BaseInfiniteDropdownV3
      id={props.context.domNodeId}
      disabled={props.context.disabled}
      size="m"
      variant={
        props.context?.customPresentationContext?.isCell ? "cell" : undefined
      }
      shape={
        props.context?.customPresentationContext?.isCell
          ? "rectangular"
          : "square"
      }
      handleClear={
        !props.context.customPresentationContext?.disableClearable &&
        props.context.customPresentationContext?.sum?.clearCallback
          ? props.context.customPresentationContext.sum.clearCallback
          : // not in a sum
            () => props.foreignMutations.clearSelection(flags)
      }
      valueProps={{
        value: props.context.value.isSome
          ? ((props.context.value.value as ValueRecord).fields.get(
              "DisplayValue"
            ) as string)
          : undefined,
        valueId: props.context.value.isSome
          ? ((props.context.value.value as ValueRecord).fields.get(
              "Id"
            ) as string)
          : undefined,
        invalid: false,
        placeholder: ddTranslations(
          props.context.customPresentationContext?.placeholder ??
            "pleaseSelectValue"
        ),
        copyable: props.context.customPresentationContext?.disableCopyable
          ? false
          : true,
        customAiApplied: Boolean(
          props.context?.customPresentationContext?.customAiApplied
        ),
        startAdornment:
          props.context?.customPresentationContext?.customAiApplied,
        isWrapped: props.context?.customPresentationContext?.isWrapped,
        endAdornment: props.context?.customPresentationContext?.endAdornment,
      }}
      infiniteStreamProps={{
        hasMoreData: props.context.hasMoreValues,
        isFetchingMoreData:
          props.context.customFormState.stream.position.shouldLoad ==
          "loadMore",
        searchTooVague: Boolean(
          props.context.customFormState.searchText.value &&
            props.context.customFormState.searchText.value.length < 2
        ),
        fetchMoreData: props.foreignMutations.loadMore,
        open: props.context.customFormState.status === "open",
        setOpen: () => {
          props.foreignMutations.toggleOpen();
        },
      }}
      searchProps={{
        searchText: props.context.customFormState.searchText.value,
        setSearchText: (v) => {
          props.foreignMutations.setSearchText(v);
        },
        placeholder: ddTranslations("typeAndFilterMin3Characters"),
      }}
      listProps={{
        options:
          // avoid showing "no data available" on first load
          (props.context.customFormState.stream.loadedElements.size === 0 &&
            props.context.customFormState.stream.position.shouldLoad ==
              "loadMore") ||
          props.context.customFormState.stream.position.shouldLoad == "reload"
            ? {
                kind: "loading",
              }
            : {
                kind: "loaded",
                items: props.context.customFormState.stream.loadedElements
                  .valueSeq()
                  .flatMap((chunk) => chunk.data.valueSeq())
                  .map((el) => ({
                    id: el.Id,
                    label:
                      el.DisplayValue.length > 0
                        ? el.DisplayValue
                        : missingDisplayValueLabel,
                    hasWarning: el.DisplayValue.length === 0,
                  }))
                  .toArray(),
              },

        select: (v) => {
          props.foreignMutations.select(
            PredicateValue.Default.option(
              true,
              PredicateValue.Default.record(
                Map({
                  Id: v.id,
                  DisplayValue: v.label,
                })
              )
            ),
            flags
          );
          props.foreignMutations.toggleOpen();
        },
      }}
    />
  );
};

const ReadonlyInfiniteStreamComponent = (
  props: React.ComponentProps<
    SearchableInfiniteStreamAbstractRendererView<
      CustomPresentationContexts,
      IdeFlags,
      FieldExtraContext
    >
  >
) => (
  <BaseInputV3
    variant={
      props.context.customPresentationContext?.isCell ? "cell" : "outlined"
    }
    shape={
      props.context.customPresentationContext?.isCell ? "rectangular" : "square"
    }
    kind="readOnly"
    fullWidth
    copyable={
      props.context.customPresentationContext?.disableCopyable ? false : true
    }
    value={
      props.context.value.isSome
        ? ((props.context.value.value as ValueRecord).fields.get(
            "DisplayValue"
          ) as string)
        : props.context.extraContext.locale.t("noValueSelected")
    }
    onClick={() => {}}
    disabled={props.context.disabled}
  />
);

const InfiniteStreamMultiComponent = (
  props: React.ComponentProps<
    SearchableInfiniteStreamMultiselectAbstractRendererView<
      CustomPresentationContexts,
      IdeFlags,
      FieldExtraContext
    >
  >
) => {
  return props.context.readOnly ? (
    <ReadonlyInfiniteStreamMultiComponent {...props} />
  ) : (
    <EditableInfiniteStreamMultiComponent {...props} />
  );
};

const EditableInfiniteStreamMultiComponent = (
  props: React.ComponentProps<
    SearchableInfiniteStreamMultiselectAbstractRendererView<
      CustomPresentationContexts,
      IdeFlags,
      FieldExtraContext
    >
  >
) => {
  const ddTranslations = translateForCustomDataDrivenTranslations(
    props.context.extraContext.locale,
    props.context.extraContext.namespace
  );

  return (
    <BaseInfiniteDropdownMultiple
      disabled={props.context.disabled}
      size="m"
      shape={
        props.context?.customPresentationContext?.isCell
          ? "rectangular"
          : "square"
      }
      handleClear={() => props.foreignMutations.clearSelection(undefined)}
      valueProps={{
        values: PredicateValue.Operations.IsRecord(props.context.value)
          ? props.context.value.fields
              .valueSeq()
              .map((v) => ({
                id: (v as ValueRecord).fields.get("Id") as string,
                label: (v as ValueRecord).fields.get("DisplayValue") as string,
              }))
              .toArray()
          : [],
        invalid: false,
        placeholder: ddTranslations(
          props.context?.customPresentationContext?.placeholder ??
            "pleaseSelectValues"
        ),
        copyable: props.context.customPresentationContext?.disableCopyable
          ? false
          : true,
      }}
      infiniteStreamProps={{
        hasMoreData: props.context.hasMoreValues,
        isFetchingMoreData:
          props.context.customFormState.stream.position.shouldLoad ==
          "loadMore",
        searchTooVague: Boolean(
          props.context.customFormState.searchText.value &&
            props.context.customFormState.searchText.value.length < 3
        ),
        fetchMoreData: () => {
          props.foreignMutations.loadMore();
        },
        open: props.context.customFormState.status === "open",
        setOpen: () => {
          props.foreignMutations.toggleOpen();
        },
      }}
      searchProps={{
        searchText: props.context.customFormState.searchText.value,
        setSearchText: (v) => {
          props.foreignMutations.setSearchText(v);
        },
        placeholder: ddTranslations("typeAndFilterMin3Characters"),
      }}
      listProps={{
        options:
          (props.context.customFormState.stream.loadedElements.size === 0 &&
            props.context.customFormState.stream.position.shouldLoad ==
              "loadMore") ||
          props.context.customFormState.stream.position.shouldLoad == "reload"
            ? {
                kind: "loading",
              }
            : {
                kind: "loaded",
                items: props.context.customFormState.stream.loadedElements
                  .valueSeq()
                  .flatMap((chunk) => chunk.data.valueSeq())
                  .map((el) => ({
                    id: el.Id,
                    label: el.DisplayValue,
                  }))
                  .toArray(),
              },

        select: (v) => {
          props.foreignMutations.replace(
            PredicateValue.Default.record(
              OrderedMap(
                v.map((v) => [
                  v.id,
                  PredicateValue.Default.record(
                    Map({
                      Id: v.id,
                      DisplayValue: v.label,
                    })
                  ),
                ])
              )
            ),
            undefined
          );
          props.foreignMutations.toggleOpen();
        },
      }}
    />
  );
};

const ReadonlyInfiniteStreamMultiComponent = (
  props: React.ComponentProps<
    SearchableInfiniteStreamMultiselectAbstractRendererView<
      CustomPresentationContexts,
      IdeFlags,
      FieldExtraContext
    >
  >
) => (
  <BaseInputTags
    disabled={props.context.disabled}
    size={"m"}
    shape={"square"}
    variant={"outlined"}
    error={false}
    readOnly
    values={
      PredicateValue.Operations.IsRecord(props.context.value)
        ? props.context.value.fields
            .valueSeq()
            .map((v) => ({
              id: (v as ValueRecord).fields.get("Id") as string,
              label: (v as ValueRecord).fields.get("DisplayValue") as string,
            }))
            .toArray()
        : []
    }
  />
);

export const StreamSingleSelectionFieldViews = {
  infiniteStreamWithMenuContext: () => (props) => {
    return (
      <BaseFieldWithoutMetadata
        disabled={props.context.disabled}
        domNodeId={props.context.domNodeId}
      >
        {({ showMenu, endAdornment }) => (
          <InfiniteStreamComponent
            {...props}
            context={{
              ...props.context,
              customPresentationContext: {
                ...props.context.customPresentationContext,
                isWrapped: showMenu,
                endAdornment,
              },
            }}
          />
        )}
      </BaseFieldWithoutMetadata>
    );
  },
  infiniteStream: () => (props) => {
    return <InfiniteStreamComponent {...props} />;
  },
  infiniteStreamWithLabel: () => (props) => {
    const ddTranslationsWithCtx = translateForDataDrivenTranslationsWithContext(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );

    return (
      <BaseLabelV3
        dense
        label={ddTranslationsWithCtx(
          props.context.label,
          props.context.labelContext,
          props.context.domNodeId
        )}
        highlightedValue={
          props.context?.customPresentationContext?.highlightLabelValue
        }
      >
        <InfiniteStreamComponent {...props} />
      </BaseLabelV3>
    );
  },
  infiniteStreamCell: () => (props) => {
    return (
      <InfiniteStreamComponent
        {...props}
        context={{
          ...props.context,
          customPresentationContext: {
            ...props.context.customPresentationContext,
            isCell: true,
          },
        }}
      />
    );
  },
  infiniteStreamCellNotEditable: () => (props) => {
    return (
      <ReadonlyInfiniteStreamComponent
        {...props}
        context={{
          ...props.context,
          customPresentationContext: {
            ...props.context.customPresentationContext,
            isCell: true,
          },
        }}
      />
    );
  },
  infiniteStreamNotEditable: () => (props) => {
    return <ReadonlyInfiniteStreamComponent {...props} />;
  },
} satisfies IdeConcreteRenderers["streamSingleSelection"];

export const StreamMultiSelectionFieldViews = {
  infiniteStreamMultiselect: () => (props) => {
    return <InfiniteStreamMultiComponent {...props} />;
  },
  infiniteStreamMultiselectWithMenuContext: () => (props) => {
    return (
      <BaseFieldWithoutMetadata
        disabled={props.context.disabled}
        domNodeId={props.context.domNodeId}
      >
        {({ showMenu, endAdornment }) => (
          <InfiniteStreamMultiComponent
            {...props}
            context={{
              ...props.context,
              customPresentationContext: {
                ...props.context.customPresentationContext,
                isWrapped: showMenu,
                endAdornment,
              },
            }}
          />
        )}
      </BaseFieldWithoutMetadata>
    );
  },
  infiniteStreamMultiselectWithLabel: () => (props) => {
    const ddTranslationsWithCtx = translateForDataDrivenTranslationsWithContext(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );

    return (
      <BaseLabelV3
        dense
        label={ddTranslationsWithCtx(
          props.context.label,
          props.context.labelContext,
          props.context.domNodeId
        )}
        highlightedValue={
          props.context?.customPresentationContext?.highlightLabelValue
        }
      >
        <InfiniteStreamMultiComponent {...props} />
      </BaseLabelV3>
    );
  },
  infiniteStreamMultiselectNotEditable: () => (props) => {
    return (
      <BaseInputTags
        disabled={props.context.disabled}
        size={"m"}
        shape={"square"}
        variant={"outlined"}
        error={false}
        readOnly
        values={
          PredicateValue.Operations.IsRecord(props.context.value)
            ? props.context.value.fields
                .valueSeq()
                .map((v) => ({
                  id: (v as ValueRecord).fields.get("Id") as string,
                  label: (v as ValueRecord).fields.get(
                    "DisplayValue"
                  ) as string,
                }))
                .toArray()
            : []
        }
      />
    );
  },
} satisfies IdeConcreteRenderers["streamMultiSelection"];
