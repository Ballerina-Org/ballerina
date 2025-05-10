import { Map } from "immutable";
import { ValueOrErrors } from "../../../../../../../../../../collections/domains/valueOrErrors/state";
import {
  isObject,
  isString,
} from "../../../../../../../../parser/domains/types/state";
import { DispatchParsedType } from "../../../types/state";
import { EnumRenderer, SerializedEnumRenderer } from "./domains/enum/state";
import { ListRenderer, SerializedListRenderer } from "./domains/list/state";
import {
  LookupRenderer,
  SerializedLookupRenderer,
} from "./domains/lookup/state";
import { MapRenderer, SerializedMapRenderer } from "./domains/map/state";
import { OneRenderer, SerializedOneRenderer } from "./domains/one/state";
import {
  SerializedStreamRenderer,
  StreamRenderer,
} from "./domains/stream/state";
import { SerializedSumRenderer, SumRenderer } from "./domains/sum/state";
import {
  BaseSumUnitDateRenderer,
  SerializedSumUnitDateBaseRenderer,
} from "./domains/sumUnitDate/state";
import {
  RecordRenderer,
  SerializedRecordRenderer,
} from "./domains/record/state";
import { SerializedUnionRenderer, UnionRenderer } from "./domains/union/state";
import { SerializedTupleRenderer, TupleRenderer } from "./domains/tuple/state";
import { SerializedTableRenderer, TableRenderer } from "./domains/table/state";
import { ConcreteRendererKinds } from "../../../../../../../built-ins/state";

export type CommonSerializedRendererProperties = {
  renderer?: unknown;
  visible?: unknown;
  disabled?: unknown;
};

//  detailsRenderer?: unknown; // only for tables at the moment
//  api?: unknown; // only for tables at the moment

export type SerializedRenderer =
  | SerializedLookupRenderer
  | SerializedEnumRenderer
  | SerializedListRenderer
  | SerializedMapRenderer
  | SerializedOneRenderer
  | SerializedStreamRenderer
  | SerializedSumRenderer
  | SerializedSumUnitDateBaseRenderer
  | SerializedRecordRenderer
  | SerializedUnionRenderer
  | SerializedTupleRenderer
  | SerializedTableRenderer;

// | SerializedTableFormRenderer
// | SerializedRecordFormRenderer;

export type Renderer<T> =
  | EnumRenderer<T>
  | LookupRenderer<T>
  | ListRenderer<T>
  | MapRenderer<T>
  | OneRenderer<T>
  | StreamRenderer<T>
  | SumRenderer<T>
  | BaseSumUnitDateRenderer<T>
  | RecordRenderer<T>
  | UnionRenderer<T>
  | TupleRenderer<T>
  | TableRenderer<T>;

export const Renderer = {
  Operations: {
    HasOptions: (_: unknown): _ is SerializedEnumRenderer =>
      isObject(_) && "options" in _,
    HasStream: (_: unknown): _ is SerializedStreamRenderer =>
      isObject(_) && "stream" in _,
    IsSumUnitDate: (
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
    ): boolean =>
      isObject(serialized) &&
      "renderer" in serialized &&
      isString(serialized.renderer) &&
      concreteRenderers?.sumUnitDate?.[serialized.renderer] != undefined,
    DeserializeAs: <T>(
      type: DispatchParsedType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      as: string,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<Renderer<T>, string> => {
      return Renderer.Operations.Deserialize(
        type,
        serialized,
        concreteRenderers,
        types,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing as ${as}`),
      );
    },
    Deserialize: <T>(
      type: DispatchParsedType<T>,
      serialized: unknown,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<Renderer<T>, string> =>
      typeof serialized == "string"
        ? LookupRenderer.Operations.Deserialize(type, serialized)
        : Renderer.Operations.HasOptions(serialized) &&
          (type.kind == "singleSelection" || type.kind == "multiSelection")
        ? EnumRenderer.Operations.Deserialize(
            type,
            serialized,
            concreteRenderers,
            types,
          )
        : Renderer.Operations.HasStream(serialized) &&
          (type.kind == "singleSelection" || type.kind == "multiSelection")
        ? StreamRenderer.Operations.Deserialize(
            type,
            serialized,
            concreteRenderers,
            types,
          )
        : type.kind == "list"
        ? ListRenderer.Operations.Deserialize(
            type,
            serialized,
            concreteRenderers,
            types,
          )
        : type.kind == "map"
        ? MapRenderer.Operations.Deserialize(
            type,
            serialized,
            concreteRenderers,
            types,
          )
        : type.kind == "one"
        ? OneRenderer.Operations.Deserialize(
            type,
            serialized,
            concreteRenderers,
            types,
          )
        : Renderer.Operations.IsSumUnitDate(serialized, concreteRenderers) &&
          type.kind == "sum"
        ? BaseSumUnitDateRenderer.Operations.Deserialize(
            type,
            serialized,
            concreteRenderers,
            types,
          )
        : type.kind == "sum"
        ? SumRenderer.Operations.Deserialize(
            type,
            serialized,
            concreteRenderers,
            types,
          )
        : type.kind == "record"
        ? RecordRenderer.Operations.Deserialize(
            type,
            serialized,
            concreteRenderers,
            types,
          )
        : type.kind == "union"
        ? UnionRenderer.Operations.Deserialize(
            type,
            serialized,
            concreteRenderers,
            types,
          )
        : ValueOrErrors.Default.throwOne<Renderer<T>, string>(
            `Unknown renderer ${JSON.stringify(serialized)} and type of kind ${
              type.kind
            }`,
          ),
  },
};
