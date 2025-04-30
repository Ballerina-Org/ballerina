import { Map } from "immutable";
import {
  Expr,
  isObject,
  isString,
  MapRepo,
  SerializedTableFormRenderer,
  TableFormRenderer,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { DispatchParsedType } from "../../../../../types/state";

import {
  BasePrimitiveRenderer,
  SerializedPrimitiveBaseRenderer,
} from "./domains/primitive/state";
import {
  BaseEnumRenderer,
  SerializedEnumRecordFieldRenderer,
} from "./domains/enum/state";
import {
  BaseStreamRenderer,
  SerializedStreamBaseRenderer,
} from "./domains/stream/state";
import {
  BaseMapRenderer,
  SerializedBaseMapRenderer,
} from "./domains/map/state";
import {
  BaseSumRenderer,
  SerializedSumBaseRenderer,
} from "./domains/sum/state";
import {
  BaseTupleRenderer,
  SerializedBaseTupleRenderer,
} from "./domains/tuple/state";
import {
  BaseUnionRenderer,
  SerializedBaseUnionRenderer,
} from "./domains/union/state";
import {
  BaseLookupRenderer,
  SerializedBaseLookupRenderer,
} from "./domains/lookup/state";
import {
  BaseListRenderer,
  SerializedBaseListRenderer,
} from "./domains/list/state";
import {
  BaseSumUnitDateRenderer,
  SerializedSumUnitDateBaseRenderer,
} from "./domains/sumUnitDate/state";
import {
  BaseTableRenderer,
  SerializedBaseTableRenderer,
} from "./domains/table/state";
import {
  RecordFormRenderer,
  SerializedRecordFormRenderer,
} from "../recordFormRenderer/state";

export type BaseSerializedBaseRenderer = {
  renderer?: unknown;
  label?: string;
  tooltip?: string;
  details?: string;
  visible?: unknown;
  disabled?: unknown;
  api?: unknown;
};

export type SerializedBaseRenderer =
  | SerializedPrimitiveBaseRenderer
  | SerializedEnumRecordFieldRenderer
  | SerializedStreamBaseRenderer
  | SerializedBaseMapRenderer
  | SerializedSumBaseRenderer
  | SerializedBaseTupleRenderer
  | SerializedBaseUnionRenderer
  | SerializedBaseLookupRenderer
  | SerializedBaseListRenderer
  | SerializedSumUnitDateBaseRenderer
  | SerializedBaseTableRenderer;

export type SerializedInlineRenderer =
  | SerializedTableFormRenderer
  | SerializedRecordFormRenderer;

export type BaseBaseRenderer = {
  visible?: Expr;
  disabled?: Expr;
  label?: string;
  tooltip?: string;
  details?: string;
};

export type ParentContext = "tableColumn" | "recordField" | "nested";

export type BaseRenderer<T> =
  | BasePrimitiveRenderer<T>
  | BaseEnumRenderer<T>
  | BaseStreamRenderer<T>
  | BaseListRenderer<T>
  | BaseLookupRenderer<T>
  | BaseMapRenderer<T>
  | BaseSumRenderer<T>
  | BaseTupleRenderer<T>
  | BaseUnionRenderer<T>
  | BaseSumUnitDateRenderer<T>
  | BaseTableRenderer<T>;

export const BaseRenderer = {
  Operations: {
    hasType: (_: unknown): _ is { type: string } => isObject(_) && "type" in _,
    IsSumUnitDate: (
      serialized: SerializedBaseRenderer,
      fieldViews: any,
    ): boolean =>
      fieldViews?.sumUnitDate?.[serialized.renderer as string] != undefined,
    IsTableForm: (
      serialized: unknown,
    ): serialized is SerializedTableFormRenderer =>
      isObject(serialized) &&
      "columns" in serialized &&
      "visibleColumns" in serialized,
    IsRecordForm: (
      serialized: unknown,
    ): serialized is SerializedRecordFormRenderer =>
      isObject(serialized) && "fields" in serialized && "tabs" in serialized,
    ComputeVisibility: (
      visible: unknown,
      renderingContext: ParentContext,
    ): ValueOrErrors<Expr | undefined, string> =>
      visible == undefined
        ? ValueOrErrors.Default.return(undefined)
        : renderingContext == "recordField"
          ? Expr.Operations.parseAsVisibilityExpression(visible)
          : ValueOrErrors.Default.return(undefined),
    ComputeDisabled: (
      disabled: unknown,
      renderingContext: ParentContext,
    ): ValueOrErrors<Expr | undefined, string> =>
      disabled == undefined
        ? ValueOrErrors.Default.return(undefined)
        : renderingContext == "recordField" || renderingContext == "tableColumn"
          ? Expr.Operations.parseAsDisabledExpression(disabled)
          : ValueOrErrors.Default.return(undefined),
    DeserializeAsInlineRenderer: <T>(
      serialized: SerializedInlineRenderer,
      fieldViews: any,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<TableFormRenderer<T> | RecordFormRenderer<T>, string> =>
      !BaseRenderer.Operations.hasType(serialized)
        ? ValueOrErrors.Default.throwOne<
            TableFormRenderer<T> | RecordFormRenderer<T>,
            string
          >(`inlined renderer missing type ${serialized.renderer}`)
        : !isString(serialized.type)
          ? ValueOrErrors.Default.throwOne<
              TableFormRenderer<T> | RecordFormRenderer<T>,
              string
            >(`inlined renderer type is not a string`)
          : MapRepo.Operations.tryFindWithError(
              serialized.type,
              types,
              () => `cannot find type ${serialized.type} in types`,
            )
              .Then((type) =>
                BaseRenderer.Operations.IsRecordForm(serialized)
                  ? type.kind == "record"
                    ? RecordFormRenderer.Operations.Deserialize(
                        type,
                        serialized,
                        fieldViews,
                        types,
                      )
                    : ValueOrErrors.Default.throwOne<
                        TableFormRenderer<T> | RecordFormRenderer<T>,
                        string
                      >(`record form inlined renderer has non record type`)
                  : TableFormRenderer.Operations.Deserialize(
                      DispatchParsedType.Default.table(
                        "inlined table",
                        [type],
                        "inlined table",
                      ),
                      serialized,
                      types,
                      fieldViews,
                    ),
              )
              .MapErrors((errors) =>
                errors.map(
                  (error) => `${error}\n...When parsing as inline renderer`,
                ),
              ),
    DeserializeAs: <T>(
      type: DispatchParsedType<T>,
      serialized: SerializedBaseRenderer,
      fieldViews: any,
      renderingContext: ParentContext,
      as: string,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<
      BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>,
      string
    > => {
      return BaseRenderer.Operations.Deserialize(
        type,
        serialized,
        fieldViews,
        renderingContext,
        types,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing as ${as}`),
      );
    },
    Deserialize: <T>(
      type: DispatchParsedType<T>,
      serialized: SerializedBaseRenderer | SerializedInlineRenderer,
      fieldViews: any,
      renderingContext: ParentContext,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<
      BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>,
      string
    > => {
      const result: ValueOrErrors<
        BaseRenderer<T> | TableFormRenderer<T> | RecordFormRenderer<T>,
        string
      > = (() => {
        if (
          BaseRenderer.Operations.IsTableForm(serialized) ||
          BaseRenderer.Operations.IsRecordForm(serialized)
        ) {
          return BaseRenderer.Operations.DeserializeAsInlineRenderer(
            serialized,
            fieldViews,
            types,
          );
        }
        if (
          BaseRenderer.Operations.IsSumUnitDate(serialized, fieldViews) &&
          type.kind == "sum"
        ) {
          return BaseSumUnitDateRenderer.Operations.Deserialize(
            type,
            serialized,
            renderingContext,
          );
        }
        if (type.kind == "primitive") {
          return BasePrimitiveRenderer.Operations.Deserialize(
            type,
            serialized,
            renderingContext,
          );
        }
        if (
          (type.kind == "singleSelection" || type.kind == "multiSelection") &&
          "options" in serialized
        ) {
          return BaseEnumRenderer.Operations.Deserialize(
            type,
            serialized,
            renderingContext,
          );
        }
        if (
          (type.kind == "singleSelection" || type.kind == "multiSelection") &&
          "stream" in serialized
        ) {
          return BaseStreamRenderer.Operations.Deserialize(
            type,
            serialized,
            renderingContext,
          );
        }
        if (type.kind == "lookup") {
          return BaseLookupRenderer.Operations.Deserialize(
            type,
            serialized,
            renderingContext,
          );
        }
        if (type.kind == "list") {
          return BaseListRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            renderingContext,
            types,
          );
        }
        if (type.kind == "map") {
          return BaseMapRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            renderingContext,
            types,
          );
        }
        if (type.kind == "sum") {
          return BaseSumRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            renderingContext,
            types,
          );
        }
        if (type.kind == "union") {
          return BaseUnionRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            renderingContext,
            types,
          );
        }
        if (type.kind == "tuple") {
          return BaseTupleRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            renderingContext,
            types,
          );
        }
        if (type.kind == "table") {
          return BaseTableRenderer.Operations.Deserialize(
            type,
            serialized,
            renderingContext,
          );
        }
        return ValueOrErrors.Default.throwOne(
          `Unknown ${renderingContext} renderer ${serialized.renderer} and type ${type.kind}`,
        );
      })();
      return result.MapErrors((errors) =>
        errors.map(
          (error) =>
            `${error}\n...When parsing as ${renderingContext} renderer`,
        ),
      );
    },
  },
};
