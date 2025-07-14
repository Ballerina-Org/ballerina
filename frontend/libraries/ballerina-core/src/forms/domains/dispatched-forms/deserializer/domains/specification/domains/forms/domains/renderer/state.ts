import { Map } from "immutable";
import { ValueOrErrors } from "../../../../../../../../../../collections/domains/valueOrErrors/state";
import {
  isObject,
  isString,
} from "../../../../../../../../parser/domains/types/state";
import { DispatchParsedType } from "../../../types/state";
import { EnumRenderer, SerializedEnumRenderer } from "./domains/enum/state";
import { ListRenderer, SerializedListRenderer } from "./domains/list/state";
import { LookupRenderer, SerializedLookup } from "./domains/lookup/state";
import { MapRenderer, SerializedMapRenderer } from "./domains/map/state";
import { OneRenderer, SerializedOneRenderer } from "./domains/one/state";
import {
  ReadOnlyRenderer,
  SerializedReadOnlyRenderer,
} from "./domains/readOnly/state";
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
import { PrimitiveRenderer } from "./domains/primitive/state";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  Unit,
} from "../../../../../../../../../../../main";

export type CommonSerializedRendererProperties = {
  renderer?: unknown;
  visible?: unknown;
  disabled?: unknown;
};

export type SerializedRenderer =
  | string
  | SerializedEnumRenderer
  | SerializedListRenderer
  | SerializedMapRenderer
  | SerializedOneRenderer
  | SerializedReadOnlyRenderer
  | SerializedStreamRenderer
  | SerializedSumRenderer
  | SerializedSumUnitDateBaseRenderer
  | SerializedRecordRenderer
  | SerializedUnionRenderer
  | SerializedTupleRenderer
  | SerializedTableRenderer;

export type Renderer<T> =
  | PrimitiveRenderer<T>
  | EnumRenderer<T>
  | LookupRenderer<T>
  | ListRenderer<T>
  | MapRenderer<T>
  | OneRenderer<T>
  | ReadOnlyRenderer<T>
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
    HasColumns: (_: unknown): _ is SerializedTableRenderer =>
      isObject(_) && "columns" in _,
    IsSumUnitDate: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
    ): boolean =>
      isObject(serialized) &&
      "renderer" in serialized &&
      isString(serialized.renderer) &&
      concreteRenderers?.sumUnitDate?.[serialized.renderer] != undefined,
    DeserializeAs: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: DispatchParsedType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      as: string,
      types: Map<string, DispatchParsedType<T>>,
      tableApi: string | undefined,
    ): ValueOrErrors<Renderer<T>, string> =>
      Renderer.Operations.Deserialize(
        type,
        serialized,
        concreteRenderers,
        types,
        tableApi,
      ).MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing as ${as}`),
      ),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      type: DispatchParsedType<T>,
      serialized: unknown,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
      tableApi: string | undefined, // Necessary because the table api is currently defined outside of the renderer, so a lookup has to be able to pass it to the looked up renderer
    ): ValueOrErrors<Renderer<T>, string> =>
      /*
        Important semantics of lookup vs inlined renderers and types:

        A lookup type is a type referenced by a string which can be lookuped in the 'types' section of the specification.
        An inlined type is directly defined within another type.

        A lookup renderer is a renderer which is referenced by a string which can be lookuped in the 'forms' section of the specification.
        An inlined renderer is directly defined within another renderer.
        Currently, only records, tables and unions can be defined at the top level of the forms section.
        
        Consider a component on the form as a renderer and type together.
        There are 4 potential combiniations:
          1. lookup type and lookup renderer
          2. lookup type and inlined renderer
          3. inlined type and lookup renderer
          4. inlined type and inlined renderer

        Each of these combinations has a different semantics meaning in how the componenent is
        represented in the form hierarchy and how it's local bindings are set (Which are used in expressions).

        Having a lookup type invokes the renderer to be rendered inside an abstract lookup renderer (ALR) which is invisible, but passes 
        its type down to the resolved renderer.
        Although, they have no DOM node, ALRs can pass signals down to their child renderers via props.
        E.g. They can pass down a disabled flag. This flag could be set if the ALR is disabled. It also passes down its serialzied type
        to its child. So the child may could use this information to signal that its parent should be disabled.
        Therefore being lookup type or not affects the type hierarchy. If we simply resolve
        the lookup straight away without using a lookup abstract renderer, we lose the information.

        Having a lookup renderer causes the "local" binding of a renderer to be set to its own local value. Inlined renderers receive their
        parent's local binding.

        Therefore, we have to carefully consider the combination of lookup renderer and type when deserializing and dispatching.
        Since lookups have semantic meaning, we must preserve this until the moment we dispatch the renderer (at which point we
        pass this information to the abstract renderer).
      */

      {
        return type.kind == "lookup" // Lookup type
          ? typeof serialized == "string" // lookup type and lookup renderer (case 1)
            ? LookupRenderer.Operations.Deserialize(
                SerializedLookup.Default.LookupTypeLookupRenderer(
                  type,
                  serialized,
                ),
                tableApi,
                concreteRenderers,
                types,
              )
            : LookupRenderer.Operations.Deserialize(
                // lookup type and inlined renderer (case 2)
                SerializedLookup.Default.LookupTypeInlinedRenderer(
                  type,
                  serialized,
                ),
                tableApi,
                concreteRenderers,
                types,
              )
          : typeof serialized == "string"
            ? type.kind == "primitive" // special case
              ? PrimitiveRenderer.Operations.Deserialize(type, serialized)
              : // inlined type and lookup renderer (case 3)
                LookupRenderer.Operations.Deserialize(
                  SerializedLookup.Default.InlinedTypeLookupRenderer(
                    type,
                    serialized,
                  ),
                  tableApi,
                  concreteRenderers,
                  types,
                )
            : // All other cases are inlined renderers and inlined types (case 4)
              Renderer.Operations.HasOptions(serialized) &&
                (type.kind == "singleSelection" ||
                  type.kind == "multiSelection")
              ? EnumRenderer.Operations.Deserialize(type, serialized)
              : Renderer.Operations.HasStream(serialized) &&
                  (type.kind == "singleSelection" ||
                    type.kind == "multiSelection")
                ? StreamRenderer.Operations.Deserialize(type, serialized)
                : Renderer.Operations.HasColumns(serialized) &&
                    type.kind == "table"
                  ? TableRenderer.Operations.Deserialize(
                      type,
                      serialized,
                      concreteRenderers,
                      types,
                      tableApi,
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
                        : type.kind == "readOnly"
                          ? ReadOnlyRenderer.Operations.Deserialize(
                              type,
                              serialized,
                              concreteRenderers,
                              types,
                            )
                          : Renderer.Operations.IsSumUnitDate(
                                serialized,
                                concreteRenderers,
                              ) && type.kind == "sum"
                            ? BaseSumUnitDateRenderer.Operations.Deserialize(
                                type,
                                serialized,
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
                                  : type.kind == "tuple"
                                    ? TupleRenderer.Operations.Deserialize(
                                        type,
                                        serialized,
                                        concreteRenderers,
                                        types,
                                      )
                                    : ValueOrErrors.Default.throwOne<
                                        Renderer<T>,
                                        string
                                      >(
                                        `Unknown renderer ${JSON.stringify(serialized, null, 2)} and type of kind ${
                                          type.kind
                                        }`,
                                      );
      },
  },
};
