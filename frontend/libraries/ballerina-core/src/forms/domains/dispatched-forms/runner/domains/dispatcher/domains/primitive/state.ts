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
import { PrimitiveRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/primitive/state";

export const PrimitiveDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: PrimitiveRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > => {
      const result: ValueOrErrors<
        [Template<any, any, any, any>, StringSerializedType],
        string
      > = (() => {
        const viewKindRes = dispatcherContext.getConcreteRendererKind(
          renderer.concreteRenderer,
        );
        if (viewKindRes.kind == "errors") {
          return viewKindRes;
        }
        const viewKind = viewKindRes.value;
        const serializedType = DispatchPrimitiveType.SerializeToString(
          renderer.type.name as string,
        );
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
              renderer.concreteRenderer,
            )
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                injectedPrimitive
                  .abstractRenderer(
                    dispatcherContext.IdProvider,
                    dispatcherContext.ErrorRenderer,
                    serializedType,
                  )
                  .withView(concreteRenderer),
                serializedType,
              ]),
            );
        }
        if (viewKind == "unit") {
          return dispatcherContext
            .getConcreteRenderer("unit", renderer.concreteRenderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                UnitAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                  serializedType,
                ).withView(concreteRenderer),
                serializedType,
              ]),
            );
        }
        if (viewKind == "string") {
          return dispatcherContext
            .getConcreteRenderer("string", renderer.concreteRenderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                StringAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                  serializedType,
                ).withView(concreteRenderer),
                serializedType,
              ]),
            );
        }
        if (viewKind == "number") {
          return dispatcherContext
            .getConcreteRenderer("number", renderer.concreteRenderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                NumberAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                  serializedType,
                ).withView(concreteRenderer),
                serializedType,
              ]),
            );
        }
        if (viewKind == "boolean") {
          return dispatcherContext
            .getConcreteRenderer("boolean", renderer.concreteRenderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                BoolAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                  serializedType,
                ).withView(concreteRenderer),
                serializedType,
              ]),
            );
        }
        if (viewKind == "secret") {
          return dispatcherContext
            .getConcreteRenderer("secret", renderer.concreteRenderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                SecretAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                  serializedType,
                ).withView(concreteRenderer),
                serializedType,
              ]),
            );
        }
        if (viewKind == "base64File") {
          return dispatcherContext
            .getConcreteRenderer("base64File", renderer.concreteRenderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                Base64FileAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                  serializedType,
                ).withView(concreteRenderer),
                serializedType,
              ]),
            );
        }
        if (viewKind == "date") {
          return dispatcherContext
            .getConcreteRenderer("date", renderer.concreteRenderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return([
                DateAbstractRenderer(
                  dispatcherContext.IdProvider,
                  dispatcherContext.ErrorRenderer,
                  serializedType,
                ).withView(concreteRenderer),
                serializedType,
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
