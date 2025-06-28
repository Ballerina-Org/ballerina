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

export type SerializedLookup =
  | { kind: "concreteLookup"; renderer: string }
  | { kind: "formLookup"; renderer: string }
  | { kind: "inlinedFormLookup"; renderer: unknown; type: LookupType };

export const SerializedLookup = {
  Default: {
    ConcreteLookup: (renderer: string): SerializedLookup => ({
      kind: "concreteLookup",
      renderer,
    }),
    FormLookup: (renderer: string): SerializedLookup => ({
      kind: "formLookup",
      renderer,
    }),
    InlinedFormLookup: (
      renderer: unknown,
      type: LookupType,
    ): SerializedLookup => ({
      kind: "inlinedFormLookup",
      renderer,
      type,
    }),
  },
};

export type Lookup<T> =
  | { kind: "concreteLookup"; renderer: string }
  | { kind: "formLookup"; renderer: string }
  | { kind: "inlinedFormLookup"; renderer: Renderer<T> };

export const Lookup = {
  Default: {
    ConcreteLookup: <T>(renderer: string): Lookup<T> => ({
      kind: "concreteLookup",
      renderer,
    }),
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

export type LookupRenderer<T> = {
  kind: "lookupRenderer";
  renderer: Lookup<T>;
  type: DispatchParsedType<T>;
  tableApi: string | undefined; // Necessary because the table api is currently defined outside of the renderer, so a lookup has to be able to pass it to the looked up renderer
};

export const LookupRenderer = {
  Default: <T extends DispatchInjectablesTypes<T>>(
    type: DispatchParsedType<T>,
    renderer: Lookup<T>,
    tableApi: string | undefined,
  ): LookupRenderer<T> => ({
    kind: "lookupRenderer",
    renderer,
    type,
    tableApi,
  }),
  Operations: {
    ResolveRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      renderer: LookupRenderer<T>,
      forms: DispatcherContext<T, Flags, CustomPresentationContexts>["forms"],
    ): ValueOrErrors<Renderer<T>, string> =>
      renderer.renderer.kind == "inlinedFormLookup"
        ? ValueOrErrors.Default.return(renderer.renderer.renderer)
        : MapRepo.Operations.tryFindWithError(
            renderer.renderer.renderer,
            forms,
            () =>
              `cannot find renderer ${JSON.stringify(renderer.renderer, null, 2)}`,
          ),
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      type: DispatchParsedType<T>,
      serialized: SerializedLookup,
      tableApi: string | undefined,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts
      >,
      types: Map<string, DispatchParsedType<T>>,
    ): ValueOrErrors<LookupRenderer<T>, string> =>
      serialized.kind == "inlinedFormLookup"
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
              true, // TODO: possibly we can remove this now
            ).Then((renderer) =>
              ValueOrErrors.Default.return(
                LookupRenderer.Default(
                  type,
                  Lookup.Default.InlinedFormLookup(renderer),
                  tableApi,
                ),
              ),
            ),
          )
        : ValueOrErrors.Default.return(
            LookupRenderer.Default(type, serialized, tableApi),
          ),
  },
};
