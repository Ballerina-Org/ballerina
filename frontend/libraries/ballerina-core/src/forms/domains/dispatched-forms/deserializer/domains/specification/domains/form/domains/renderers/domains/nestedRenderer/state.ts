import { ValueOrErrors } from "../../../../../../../../../../../../../main";
import {
  NestedLookupRenderer,
  SerializedNestedLookupRenderer,
} from "./domains/lookup/state";
import {
  NestedEnumRenderer,
  SerializedNestedEnumRenderer,
} from "./domains/enum/state";
import {
  NestedPrimitiveRenderer,
  SerializedNestedPrimitiveRenderer,
} from "./domains/primitive/state";
import {
  NestedStreamRenderer,
  SerializedNestedStreamRenderer,
} from "./domains/stream/state";
import {
  NestedListRenderer,
  SerializedNestedListRenderer,
} from "./domains/list/state";
import {
  NestedMapRenderer,
  SerializedNestedMapRenderer,
} from "./domains/map/state";
import {
  NestedSumRenderer,
  SerializedNestedSumRenderer,
} from "./domains/sum/state";
import {
  NestedUnionRenderer,
  SerializedNestedUnionRenderer,
} from "./domains/union/state";
import {
  NestedTupleRenderer,
  SerializedNestedTupleRenderer,
} from "./domains/tuple/state";
import {
  NestedSumUnitDateRenderer,
  SerializedNestedSumUnitDateRenderer,
} from "./domains/sumUnitDate/state";
import { DispatchParsedType, SumType } from "../../../../../types/state";

export type BaseSerializedNestedRenderer = {
  renderer?: unknown;
  label?: string;
  tooltip?: string;
  details?: string;
};

export type SerializedNestedRenderer =
  | SerializedNestedPrimitiveRenderer
  | SerializedNestedEnumRenderer
  | SerializedNestedStreamRenderer
  | SerializedNestedLookupRenderer
  | SerializedNestedListRenderer
  | SerializedNestedMapRenderer
  | SerializedNestedSumRenderer
  | SerializedNestedUnionRenderer
  | SerializedNestedTupleRenderer
  | SerializedNestedSumUnitDateRenderer;

export type BaseNestedRenderer = {
  label?: string;
  tooltip?: string;
  details?: string;
};

export type NestedRenderer<T> =
  | NestedPrimitiveRenderer<T>
  | NestedEnumRenderer<T>
  | NestedStreamRenderer<T>
  | NestedLookupRenderer<T>
  | NestedListRenderer<T>
  | NestedMapRenderer<T>
  | NestedSumRenderer<T>
  | NestedUnionRenderer<T>
  | NestedTupleRenderer<T>
  | NestedSumUnitDateRenderer<T>;

export const NestedRenderer = {
  Operations: {
    IsSumUnitDate: (
      serialized: SerializedNestedRenderer,
      fieldViews: any,
    ): boolean => {
      return (
        fieldViews?.sumUnitDate?.[serialized.renderer as string] != undefined
      );
    },
    DeserializeAs: <T>(
      type: DispatchParsedType<T>,
      serialized: SerializedNestedRenderer,
      fieldViews: any,
      as: string,
    ): ValueOrErrors<NestedRenderer<T>, string> => {
      return NestedRenderer.Operations.Deserialize(
        type,
        serialized,
        fieldViews,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing as ${as} renderer`),
      );
    },
    Deserialize: <T>(
      type: DispatchParsedType<T>,
      serialized: SerializedNestedRenderer,
      fieldViews: any,
    ): ValueOrErrors<NestedRenderer<T>, string> => {
      const result: ValueOrErrors<NestedRenderer<T>, string> = (() => {
        if (NestedRenderer.Operations.IsSumUnitDate(serialized, fieldViews)) {
          return NestedSumUnitDateRenderer.Operations.Deserialize(
            type as SumType<T>,
            serialized,
          );
        }
        if (type.kind == "primitive") {
          return NestedPrimitiveRenderer.Operations.Deserialize(
            type,
            serialized,
          );
        }
        if (
          (type.kind == "singleSelection" || type.kind == "multiSelection") &&
          "options" in serialized
        ) {
          return NestedEnumRenderer.Operations.Deserialize(type, serialized);
        }
        if (
          (type.kind == "singleSelection" || type.kind == "multiSelection") &&
          "stream" in serialized
        ) {
          return NestedStreamRenderer.Operations.Deserialize(type, serialized);
        }
        if (
          (type.kind == "singleSelection" || type.kind == "multiSelection") &&
          !("stream" in serialized) &&
          !("options" in serialized)
        ) {
          return ValueOrErrors.Default.throwOne(
            `singleSelection or multiSelection renderer must have either stream or options`,
          );
        }
        if (type.kind == "lookup") {
          return NestedLookupRenderer.Operations.Deserialize(type, serialized);
        }
        if (type.kind == "list") {
          return NestedListRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
          );
        }
        if (type.kind == "map") {
          return NestedMapRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
          );
        }
        if (type.kind == "sum") {
          return NestedSumRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
          );
        }
        if (type.kind == "union") {
          return NestedUnionRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
          );
        }
        if (type.kind == "tuple") {
          return NestedTupleRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
          );
        }
        return ValueOrErrors.Default.throwOne(`Unsupported type ${type.kind}`);
      })();

      return result.MapErrors((errors) =>
        errors.map(
          (error) =>
            `${error}\n...When parsing nested renderer ${serialized.renderer}`,
        ),
      );
    },
  },
};
