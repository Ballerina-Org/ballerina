import {
  ConcreteRenderers,
  DispatcherContext,
  DispatchInjectablesTypes,
  DispatchParsedType,
  LookupType,
  MapRepo,
  ValueOrErrors,
} from "../../../../../../../../../../../../../main";
import { Map } from "immutable";
import { Renderer } from "../../state";

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
    }
  | {
      kind: "lookupType-inlinedRenderer";
      inlinedRenderer: Renderer<T>;
      type: LookupType;
    }
  | {
      kind: "inlinedType-lookupRenderer";
      lookupRenderer: string;
      type: DispatchParsedType<T>;
    };

export const LookupRenderer = {
  Default: {
    LookupTypeLookupRenderer: <T extends DispatchInjectablesTypes<T>>(
      type: LookupType,
      lookupRenderer: string,
    ): LookupRenderer<T> => ({
      kind: "lookupType-lookupRenderer",
      lookupRenderer,
      type,
    }),
    LookupTypeInlinedRenderer: <T extends DispatchInjectablesTypes<T>>(
      type: LookupType,
      inlinedRenderer: Renderer<T>,
    ): LookupRenderer<T> => ({
      kind: "lookupType-inlinedRenderer",
      inlinedRenderer,
      type,
    }),
    InlinedTypeLookupRenderer: <T extends DispatchInjectablesTypes<T>>(
      type: DispatchParsedType<T>,
      lookupRenderer: string,
    ): LookupRenderer<T> => ({
      kind: "inlinedType-lookupRenderer",
      lookupRenderer,
      type,
    }),
  },
  Operations: {
    ResolveRenderer: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: LookupRenderer<T>,
      forms: DispatcherContext<
        T,
        Flags,
        CustomPresentationContext,
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
      CustomPresentationContext,
      ExtraContext,
    >(
      serialized: SerializedLookup<T>,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      types: Map<string, DispatchParsedType<T>>,
      forms: object,
      alreadyParsedForms: Map<string, Renderer<T>>,
    ): ValueOrErrors<[LookupRenderer<T>, Map<string, Renderer<T>>], string> => {
      return serialized.kind == "lookupType-inlinedRenderer"
        ? DispatchParsedType.Operations.ResolveLookupType(
            serialized.type.name,
            types,
          ).Then((resolvedType) =>
            Renderer.Operations.Deserialize(
              resolvedType,
              serialized.renderer,
              concreteRenderers,
              types,
              forms,
              alreadyParsedForms,
            ).Then(([renderer, alreadyParsedForms]) => {
              return ValueOrErrors.Default.return([
                LookupRenderer.Default.LookupTypeInlinedRenderer(
                  serialized.type,
                  renderer,
                ),
                alreadyParsedForms,
              ]);
            }),
          )
        : serialized.kind == "lookupType-lookupRenderer"
          ? alreadyParsedForms.has(serialized.renderer)
            ? ValueOrErrors.Default.return<
                [LookupRenderer<T>, Map<string, Renderer<T>>],
                string
              >([
                LookupRenderer.Default.LookupTypeLookupRenderer(
                  serialized.type,
                  serialized.renderer,
                ),
                alreadyParsedForms,
              ])
            : DispatchParsedType.Operations.ResolveLookupType(
                serialized.type.name,
                types,
              ).Then((resolvedType) => {
                return Renderer.Operations.Deserialize(
                  resolvedType,
                  Reflect.get(forms, serialized.renderer),
                  concreteRenderers,
                  types,
                  forms,
                  alreadyParsedForms,
                ).Then(([renderer, alreadyParsedForms]) =>
                  ValueOrErrors.Default.return([
                    LookupRenderer.Default.LookupTypeLookupRenderer(
                      serialized.type,
                      serialized.renderer,
                    ),
                    alreadyParsedForms.set(serialized.renderer, renderer),
                  ]),
                );
              })
          : alreadyParsedForms.has(serialized.renderer)
            ? ValueOrErrors.Default.return<
                [LookupRenderer<T>, Map<string, Renderer<T>>],
                string
              >([
                LookupRenderer.Default.InlinedTypeLookupRenderer(
                  serialized.type,
                  serialized.renderer,
                ),
                alreadyParsedForms,
              ])
            : Renderer.Operations.Deserialize(
                serialized.type,
                Reflect.get(forms, serialized.renderer),
                concreteRenderers,
                types,
                forms,
                alreadyParsedForms,
              ).Then(([renderer, alreadyParsedForms]) => {
                return ValueOrErrors.Default.return([
                  LookupRenderer.Default.InlinedTypeLookupRenderer(
                    serialized.type,
                    serialized.renderer,
                  ),
                  alreadyParsedForms.set(serialized.renderer, renderer),
                ]);
              });
    },
  },
};
