import {
  ConcreteRenderers,
  DispatcherContext,
  DispatchInjectablesTypes,
  DispatchParsedType,
  LookupType,
  MapRepo,
  Renderer,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { Map } from "immutable";

export type SerializedLookup<T> =
  | { kind: "lookupType-lookupRenderer"; renderer: string; type: LookupType }
  | { kind: "lookupType-inlinedRenderer"; renderer: unknown; type: LookupType }
  | {
      kind: "inlinedType-lookupRenderer";
      renderer: string;
      type: DispatchParsedType<T>;
    };

export const SerializedLookup = {
  Default: {
    LookupTypeLookupRenderer: <T>(
      type: LookupType,
      renderer: string,
    ): SerializedLookup<T> => ({
      kind: "lookupType-lookupRenderer",
      renderer,
      type,
    }),
    LookupTypeInlinedRenderer: <T>(
      type: LookupType,
      renderer: unknown,
    ): SerializedLookup<T> => ({
      kind: "lookupType-inlinedRenderer",
      renderer,
      type,
    }),
    InlinedTypeLookupRenderer: <T>(
      type: DispatchParsedType<T>,
      renderer: string,
    ): SerializedLookup<T> => ({
      kind: "inlinedType-lookupRenderer",
      renderer,
      type,
    }),
  },
};

export type Lookup<T> =
  | { kind: "formLookup"; renderer: string }
  | { kind: "inlinedFormLookup"; renderer: Renderer<T> };

export const Lookup = {
  Default: {
    FormLookup: <T>(renderer: string): Lookup<T> => ({
      kind: "formLookup",
      renderer,
    }),
    InlinedFormLookup: <T>(renderer: Renderer<T>): Lookup<T> => ({
      kind: "inlinedFormLookup",
      renderer,
    }),
  },
};

export type LookupRenderer<T> =
  | {
      kind: "lookupType-lookupRenderer";
      type: LookupType;
      lookupRenderer: string;
      tableApi: string | undefined;
    }
  | {
      kind: "lookupType-inlinedRenderer";
      inlinedRenderer: Renderer<T>;
      type: LookupType;
      tableApi: string | undefined;
    }
  | {
      kind: "inlinedType-lookupRenderer";
      lookupRenderer: string;
      type: DispatchParsedType<T>;
      tableApi: string | undefined;
    };

export const LookupRenderer = {
  Default: {
    LookupTypeLookupRenderer: <T extends DispatchInjectablesTypes<T>>(
      type: LookupType,
      lookupRenderer: string,
      tableApi: string | undefined,
    ): LookupRenderer<T> => ({
      kind: "lookupType-lookupRenderer",
      lookupRenderer,
      type,
      tableApi,
    }),
    LookupTypeInlinedRenderer: <T extends DispatchInjectablesTypes<T>>(
      type: LookupType,
      inlinedRenderer: Renderer<T>,
      tableApi: string | undefined,
    ): LookupRenderer<T> => ({
      kind: "lookupType-inlinedRenderer",
      inlinedRenderer,
      type,
      tableApi,
    }),
    InlinedTypeLookupRenderer: <T extends DispatchInjectablesTypes<T>>(
      type: DispatchParsedType<T>,
      lookupRenderer: string,
      tableApi: string | undefined,
    ): LookupRenderer<T> => ({
      kind: "inlinedType-lookupRenderer",
      lookupRenderer,
      type,
      tableApi,
    }),
  },
  Operations: {
    ResolveRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: LookupRenderer<T>,
      forms: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >["forms"],
    ): ValueOrErrors<Renderer<T>, string> =>
      renderer.kind == "lookupType-inlinedRenderer"
        ? ValueOrErrors.Default.return(renderer.inlinedRenderer)
        : MapRepo.Operations.tryFindWithError(
            renderer.lookupRenderer,
            forms,
            () =>
              `cannot find renderer ${JSON.stringify(renderer.lookupRenderer, null, 2)}`,
          ),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      serialized: SerializedLookup<T>,
      tableApi: string | undefined,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<LookupRenderer<T>, string> =>
      serialized.kind == "lookupType-inlinedRenderer"
        ? DispatchParsedType.Operations.ResolveLookupType(
            serialized.type.name,
            types,
          ).Then((resolvedType) =>
            Renderer.Operations.Deserialize(
              resolvedType,
              serialized.renderer,
              concreteRenderers,
              types,
              tableApi,
            ).Then((renderer) =>
              ValueOrErrors.Default.return(
                LookupRenderer.Default.LookupTypeInlinedRenderer(
                  serialized.type,
                  renderer,
                  tableApi,
                ),
              ),
            ),
          )
        : serialized.kind == "lookupType-lookupRenderer"
          ? ValueOrErrors.Default.return(
              LookupRenderer.Default.LookupTypeLookupRenderer(
                serialized.type,
                serialized.renderer,
                tableApi,
              ),
            )
          : ValueOrErrors.Default.return(
              LookupRenderer.Default.InlinedTypeLookupRenderer(
                serialized.type,
                serialized.renderer,
                tableApi,
              ),
            ),
  },
};
