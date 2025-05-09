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
import { RecordRenderer, SerializedRecordRenderer } from "../formRenderer/domains/recordFormRenderer/state";

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
  | SerializedRecordRenderer;

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
  | RecordRenderer<T>;
// | TableFormRenderer<T>
// | RecordFormRenderer<T>;

export const Renderer = {
  Operations: {
    HasOptions: (_: unknown): _ is SerializedEnumRenderer =>
      isObject(_) && "options" in _,
    HasStream: (_: unknown): _ is SerializedStreamRenderer =>
      isObject(_) && "stream" in _,
    IsSumUnitDate: (serialized: unknown, fieldViews: any): boolean =>
      isObject(serialized) &&
      "renderer" in serialized &&
      isString(serialized.renderer) &&
      fieldViews?.sumUnitDate?.[serialized.renderer] != undefined,
    // hasValidMetadata: (
    //   _: unknown,
    // ): _ is {
    //   label?: string;
    //   tooltip?: string;
    //   details?: string;
    // } =>
    //   isObject(_) &&
    //   (!("label" in _) || isString(_["label"])) &&
    //   (!("tooltip" in _) || isString(_["tooltip"])) &&
    //   (!("details" in _) || isString(_["details"])),

    // IsTableForm: (
    //   serialized: unknown,
    // ): serialized is SerializedTableFormRenderer =>
    //   isObject(serialized) &&
    //   "columns" in serialized &&
    //   "visibleColumns" in serialized,
    // IsRecordForm: (
    //   serialized: unknown,
    // ): serialized is SerializedRecordFormRenderer =>
    //   isObject(serialized) && "fields" in serialized && "tabs" in serialized,
    // DeserializeAsInlineRenderer: <T>(
    //   serialized: SerializedInlineRenderer,
    //   fieldViews: any,
    //   types: Map<string, DispatchParsedType<T>>,
    // ): ValueOrErrors<TableFormRenderer<T> | RecordFormRenderer<T>, string> =>
    //   !Renderer.Operations.hasType(serialized)
    //     ? ValueOrErrors.Default.throwOne<
    //         TableFormRenderer<T> | RecordFormRenderer<T>,
    //         string
    //       >(`inlined renderer missing type ${serialized.renderer}`)
    //     : !isString(serialized.type)
    //     ? ValueOrErrors.Default.throwOne<
    //         TableFormRenderer<T> | RecordFormRenderer<T>,
    //         string
    //       >(`inlined renderer type is not a string`)
    //     : MapRepo.Operations.tryFindWithError(
    //         serialized.type,
    //         types,
    //         () => `cannot find type ${serialized.type} in types`,
    //       )
    //         .Then((type) =>
    //           Renderer.Operations.IsRecordForm(serialized)
    //             ? type.kind == "record"
    //               ? RecordFormRenderer.Operations.Deserialize(
    //                   type,
    //                   serialized,
    //                   fieldViews,
    //                   types,
    //                 )
    //               : ValueOrErrors.Default.throwOne<
    //                   TableFormRenderer<T> | RecordFormRenderer<T>,
    //                   string
    //                 >(`record form inlined renderer has non record type`)
    //             : TableFormRenderer.Operations.Deserialize(
    //                 DispatchParsedType.Default.table(
    //                   "inlined table",
    //                   [type],
    //                   "inlined table",
    //                 ),
    //                 serialized,
    //                 types,
    //                 fieldViews,
    //               ),
    //         )
    //         .MapErrors((errors) =>
    //           errors.map(
    //             (error) => `${error}\n...When parsing as inline renderer`,
    //           ),
    //         ),
    DeserializeAs: <T>(
      type: DispatchParsedType<T>,
      serialized: unknown,
      fieldViews: any,
      as: string,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<Renderer<T>, string> => {
      return Renderer.Operations.Deserialize(
        type,
        serialized,
        fieldViews,
        types,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing as ${as}`),
      );
    },
    Deserialize: <T>(
      type: DispatchParsedType<T>,
      serialized: unknown,
      fieldViews: any,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<Renderer<T>, string> =>
      typeof serialized == "string"
        ? LookupRenderer.Operations.Deserialize(type, serialized)
        : Renderer.Operations.HasOptions(serialized) &&
          (type.kind == "singleSelection" || type.kind == "multiSelection")
        ? EnumRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            types,
          )
        : Renderer.Operations.HasStream(serialized) &&
          (type.kind == "singleSelection" || type.kind == "multiSelection")
        ? StreamRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            types,
          )
        : type.kind == "list"
        ? ListRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            types,
          )
        : type.kind == "map"
        ? MapRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            types,
          )
        : type.kind == "one"
        ? OneRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            types,
          )
        : Renderer.Operations.IsSumUnitDate(serialized, fieldViews) &&
          type.kind == "sum"
        ? BaseSumUnitDateRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            types,
          )
        : type.kind == "sum"
        ? SumRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            types,
          )
        : type.kind == "record" 
        ? RecordRenderer.Operations.Deserialize(
            type,
            serialized,
            fieldViews,
            types,
          )
        : ValueOrErrors.Default.throwOne<Renderer<T>, string>(
            `Unknown renderer ${JSON.stringify(serialized)} and type of kind ${
              type.kind
            }`,
          ),

    // lookup types need to be resolved

    // if (
    //   (type.kind == "singleSelection" || type.kind == "multiSelection") &&
    //   "stream" in serialized
    // ) {
    //   return BaseStreamRenderer.Operations.Deserialize(
    //     type,
    //     serialized,
    //     renderingContext,
    //   );
    // }

    //   {
    //   const result: ValueOrErrors<
    //     Renderer<T>,
    //     string
    //   > = (() => {
    //     // if (
    //     //   Renderer.Operations.IsTableForm(serialized) ||
    //     //   Renderer.Operations.IsRecordForm(serialized)
    //     // ) {
    //     //   return Renderer.Operations.DeserializeAsInlineRenderer(
    //     //     serialized,
    //     //     fieldViews,
    //     //     types,
    //     //   );
    //     // }
    //     // if (
    //     //   Renderer.Operations.IsSumUnitDate(serialized, fieldViews) &&
    //     //   type.kind == "sum"
    //     // ) {
    //     //   return BaseSumUnitDateRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     renderingContext,
    //     //   );
    //     // }
    //     // if (type.kind == "primitive") {
    //     //   return BasePrimitiveRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     renderingContext,
    //     //   );
    //     // }
    //     // if (
    //     //   (type.kind == "singleSelection" || type.kind == "multiSelection") &&
    //     //   "options" in serialized
    //     // ) {
    //     //   return BaseEnumRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     renderingContext,
    //     //   );
    //     // }
    //     // if (
    //     //   (type.kind == "singleSelection" || type.kind == "multiSelection") &&
    //     //   "stream" in serialized
    //     // ) {
    //     //   return BaseStreamRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     renderingContext,
    //     //   );
    //     // }
    //     // if (type.kind == "lookup") {
    //     //   return BaseLookupRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     renderingContext,
    //     //   );
    //     // }
    //     // if (type.kind == "list") {
    //     //   return BaseListRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     fieldViews,
    //     //     renderingContext,
    //     //     types,
    //     //   );
    //     // }
    //     // if (type.kind == "map") {
    //     //   return BaseMapRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     fieldViews,
    //     //     renderingContext,
    //     //     types,
    //     //   );
    //     // }
    //     // if (type.kind == "sum") {
    //     //   return BaseSumRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     fieldViews,
    //     //     renderingContext,
    //     //     types,
    //     //   );
    //     // }
    //     // if (type.kind == "union") {
    //     //   return BaseUnionRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     fieldViews,
    //     //     renderingContext,
    //     //     types,
    //     //   );
    //     // }
    //     // if (type.kind == "tuple") {
    //     //   return BaseTupleRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     fieldViews,
    //     //     renderingContext,
    //     //     types,
    //     //   );
    //     // }
    //     // if (type.kind == "one") {
    //     //   return BaseOneRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     renderingContext,
    //     //     fieldViews,
    //     //     types,
    //     //   );
    //     // }
    //     // TODO -- verify and remove
    //     // if (type.kind == "table") {
    //     //   return BaseTableRenderer.Operations.Deserialize(
    //     //     type,
    //     //     serialized,
    //     //     renderingContext,
    //     //   );
    //     // }
    //     // return ValueOrErrors.Default.throwOne(
    //     //   `Unknown ${renderingContext} renderer ${serialized.renderer} and type ${type.kind}`,
    //     // );
    //   })();
    //   return result.MapErrors((errors) =>
    //     errors.map(
    //       (error) =>
    //         `${error}\n...When parsing as ${renderingContext} renderer`,
    //     ),
    //   );
    // },
  },
};
