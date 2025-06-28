import {
  DateAbstractRenderer,
  DispatcherContext,
  ValueOrErrors,
  ConcreteRenderers,
  DispatchInjectablesTypes,
  StringSerializedType,
  DispatchPrimitiveType,
} from "../../../../../../../../../main";
import { Template } from "../../../../../../../../template/state";

import { UnitAbstractRenderer } from "../../../abstract-renderers/unit/template";
import { StringAbstractRenderer } from "../../../abstract-renderers/string/template";
import { NumberAbstractRenderer } from "../../../abstract-renderers/number/template";
import { BoolAbstractRenderer } from "../../../abstract-renderers/boolean/template";
import { SecretAbstractRenderer } from "../../../abstract-renderers/secret/template";
import { Base64FileAbstractRenderer } from "../../../abstract-renderers/base-64-file/template";
import { Renderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/state";

export const PrimitiveDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      renderer: Renderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts
      >,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > => {
      const result: ValueOrErrors<
        [Template<any, any, any, any>, StringSerializedType],
        string
      > = (() => {
        if (
          renderer.kind != "lookupRenderer" ||
          renderer.renderer.kind != "concreteLookup"
        ) {
          return ValueOrErrors.Default.throwOne(
            `expected primitive to have a renderer with kind == "lookupRenderer" but got ${renderer.kind}`,
          );
        }
        const viewKindRes = dispatcherContext.getConcreteRendererKind(
          renderer.renderer.renderer,
        );
        if (viewKindRes.kind == "errors") {
          return viewKindRes;
        }
        const viewKind = viewKindRes.value;
        if (
          dispatcherContext.injectedPrimitives?.has(
            renderer.type.name as keyof T,
          )
        ) {
          const injectedPrimitive = dispatcherContext.injectedPrimitives?.get(
            renderer.type.name as keyof T,
          );
          if (injectedPrimitive == undefined) {
            return ValueOrErrors.Default.throwOne(
              `could not find injected primitive ${renderer.type.name as string}`,
            );
          }
          return dispatcherContext
            .getConcreteRenderer(
              viewKind as keyof ConcreteRenderers<T>,
              renderer.renderer.renderer,
            )
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                injectedPrimitive
                  .abstractRenderer(
                    dispatcherContext.IdProvider,
                    dispatcherContext.ErrorRenderer,
                  )
                  .withView(concreteRenderer),
                DispatchPrimitiveType.SerializeToString(
                  renderer.type.name as string,
                ),
              ]),
            );
        }
        if (viewKind == "unit") {
          return dispatcherContext
            .getConcreteRenderer("unit", renderer.renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                UnitAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
                DispatchPrimitiveType.SerializeToString(
                  renderer.type.name as string,
                ),
              ]),
            );
        }
        if (viewKind == "string") {
          return dispatcherContext
            .getConcreteRenderer("string", renderer.renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                StringAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
                DispatchPrimitiveType.SerializeToString(
                  renderer.type.name as string,
                ),
              ]),
            );
        }
        if (viewKind == "number") {
          return dispatcherContext
            .getConcreteRenderer("number", renderer.renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                NumberAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
                DispatchPrimitiveType.SerializeToString(
                  renderer.type.name as string,
                ),
              ]),
            );
        }
        if (viewKind == "boolean") {
          return dispatcherContext
            .getConcreteRenderer("boolean", renderer.renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                BoolAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
                DispatchPrimitiveType.SerializeToString(
                  renderer.type.name as string,
                ),
              ]),
            );
        }
        if (viewKind == "secret") {
          return dispatcherContext
            .getConcreteRenderer("secret", renderer.renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                SecretAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
                DispatchPrimitiveType.SerializeToString(
                  renderer.type.name as string,
                ),
              ]),
            );
        }
        if (viewKind == "base64File") {
          return dispatcherContext
            .getConcreteRenderer("base64File", renderer.renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                Base64FileAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
                DispatchPrimitiveType.SerializeToString(
                  renderer.type.name as string,
                ),
              ]),
            );
        }
        if (viewKind == "date") {
          return dispatcherContext
            .getConcreteRenderer("date", renderer.renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                DateAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
                DispatchPrimitiveType.SerializeToString(
                  renderer.type.name as string,
                ),
              ]),
            );
        }
        return ValueOrErrors.Default.throwOne(
          `could not resolve primitive concrete renderer for ${viewKind}`,
        );
      })();

      return result.MapErrors((errors) =>
        errors.map(
          (error) =>
            `${error}\n...When dispatching nested primitive: ${renderer}`,
        ),
      );
    },
  },
};
