import {
  Expr,
  ValueOrErrors,
} from "../../../../../../../../../../../../../../../main";
import { DispatchParsedType, SumType } from "../../../../../../../types/state";

import {
  RecordFieldPrimitiveRenderer,
  SerializedPrimitiveRecordFieldRenderer,
} from "./domains/primitive/state";
import {
  RecordFieldEnumRenderer,
  SerializedEnumRecordFieldRenderer,
} from "./domains/enum/state";
import {
  RecordFieldStreamRenderer,
  SerializedStreamRecordFieldRenderer,
} from "./domains/stream/state";
import {
  RecordFieldMapRenderer,
  SerializedMapRecordFieldRenderer,
} from "./domains/map/state";
import {
  RecordFieldSumRenderer,
  SerializedSumRecordFieldRenderer,
} from "./domains/sum/state";
import {
  RecordFieldTupleRenderer,
  SerializedTupleRecordFieldRenderer,
} from "./domains/tuple/state";
import {
  RecordFieldUnionRenderer,
  SerializedUnionRecordFieldRenderer,
} from "./domains/union/state";
import {
  RecordFieldLookupRenderer,
  SerializedLookupRecordFieldRenderer,
} from "./domains/lookup/state";
import {
  RecordFieldListRenderer,
  SerializedListRecordFieldRenderer,
} from "./domains/list/state";
import {
  RecordFieldSumUnitDateRenderer,
  SerializedSumUnitDateFieldRenderer,
} from "./domains/sumUnitDate/state";

export type BaseSerializedRecordFieldRenderer = {
  renderer?: unknown;
  label?: string;
  tooltip?: string;
  details?: string;
  visible?: unknown;
  disabled?: unknown;
};

export type SerializedRecordFieldRenderer =
  | SerializedPrimitiveRecordFieldRenderer
  | SerializedEnumRecordFieldRenderer
  | SerializedStreamRecordFieldRenderer
  | SerializedMapRecordFieldRenderer
  | SerializedSumRecordFieldRenderer
  | SerializedTupleRecordFieldRenderer
  | SerializedUnionRecordFieldRenderer
  | SerializedLookupRecordFieldRenderer
  | SerializedListRecordFieldRenderer
  | SerializedSumUnitDateFieldRenderer;

export type RecordFieldBaseRenderer<T> = {
  fieldName: string;
  visible: Expr;
  disabled: Expr;
  label?: string;
  tooltip?: string;
  details?: string;
};

export type RecordFieldRenderer<T> =
  | RecordFieldPrimitiveRenderer<T>
  | RecordFieldEnumRenderer<T>
  | RecordFieldStreamRenderer<T>
  | RecordFieldListRenderer<T>
  | RecordFieldLookupRenderer<T>
  | RecordFieldMapRenderer<T>
  | RecordFieldSumRenderer<T>
  | RecordFieldTupleRenderer<T>
  | RecordFieldUnionRenderer<T>
  | RecordFieldSumUnitDateRenderer<T>;

export const RecordFieldRenderer = {
  Operations: {
    IsSumUnitDate: (
      serialized: SerializedRecordFieldRenderer,
      fieldViews: any,
    ): boolean =>
      fieldViews?.sumUnitDate?.[serialized.renderer as string] != undefined,
    Deserialize: <T>(
      type: DispatchParsedType<T>,
      fieldName: string,
      serialized: SerializedRecordFieldRenderer,
      fieldViews: any,
    ): ValueOrErrors<RecordFieldRenderer<T>, string> => {
      const result: ValueOrErrors<RecordFieldRenderer<T>, string> = (() => {
        if (
          RecordFieldRenderer.Operations.IsSumUnitDate(serialized, fieldViews)
        ) {
          return RecordFieldSumUnitDateRenderer.Operations.Deserialize(
            type as SumType<T>,
            fieldName,
            serialized,
          );
        }
        if (type.kind == "primitive") {
          return RecordFieldPrimitiveRenderer.Operations.Deserialize(
            type,
            fieldName,
            serialized,
          );
        }
        if (
          (type.kind == "singleSelection" || type.kind == "multiSelection") &&
          "options" in serialized
        ) {
          return RecordFieldEnumRenderer.Operations.Deserialize(
            type,
            fieldName,
            serialized,
          );
        }
        if (
          (type.kind == "singleSelection" || type.kind == "multiSelection") &&
          "stream" in serialized
        ) {
          return RecordFieldStreamRenderer.Operations.Deserialize(
            type,
            fieldName,
            serialized,
          );
        }
        if (type.kind == "lookup") {
          return RecordFieldLookupRenderer.Operations.Deserialize(
            type,
            fieldName,
            serialized,
          );
        }
        if (type.kind == "list") {
          return RecordFieldListRenderer.Operations.Deserialize(
            type,
            fieldName,
            serialized,
            fieldViews,
          );
        }
        if (type.kind == "map") {
          return RecordFieldMapRenderer.Operations.Deserialize(
            type,
            fieldName,
            serialized,
            fieldViews,
          );
        }
        if (type.kind == "sum") {
          return RecordFieldSumRenderer.Operations.Deserialize(
            type,
            fieldName,
            serialized,
            fieldViews,
          );
        }
        if (type.kind == "union") {
          return RecordFieldUnionRenderer.Operations.Deserialize(
            type,
            fieldName,
            serialized,
            fieldViews,
          );
        }
        if (type.kind == "tuple") {
          return RecordFieldTupleRenderer.Operations.Deserialize(
            type,
            fieldName,
            serialized,
            fieldViews,
          );
        }

        return ValueOrErrors.Default.throwOne(
          `Unknown record field renderer type for ${fieldName}`,
        );
      })();
      return result.MapErrors((errors) =>
        errors.map(
          (error) =>
            `${error}\n...When parsing "${fieldName}" as record field renderer`,
        ),
      );
    },
  },
};
