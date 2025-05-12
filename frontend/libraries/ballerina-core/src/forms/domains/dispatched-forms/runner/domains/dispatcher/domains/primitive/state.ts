import {
  DateAbstractRenderer,
  DispatcherContext,
  NestedRenderer,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { Template } from "../../../../../../../../template/state";

import { DispatchPrimitiveType } from "../../../../../deserializer/domains/specification/domains/types/state";
import { UnitAbstractRenderer } from "../../../abstract-renderers/unit/template";
import { StringAbstractRenderer } from "../../../abstract-renderers/string/template";
import { NumberAbstractRenderer } from "../../../abstract-renderers/number/template";
import { BoolAbstractRenderer } from "../../../abstract-renderers/boolean/template";
import { SecretAbstractRenderer } from "../../../abstract-renderers/secret/template";
import { Base64FileAbstractRenderer } from "../../../abstract-renderers/base-64-file/template";
import { ConcreteRendererKinds } from "../../../../../built-ins/state";
import { Renderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/state";

export const PrimitiveDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: DispatchPrimitiveType<T>,
      renderer: Renderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> => {
      const result: ValueOrErrors<
        Template<any, any, any, any>,
        string
      > = (() => {
        if (renderer.kind != "lookupRenderer") {
          return ValueOrErrors.Default.throwOne(
            `expected primitive to have a renderer with kind == "lookupRenderer" but got ${renderer.kind}`,
          );
        }
        const viewKindRes = dispatcherContext.getConcreteRendererKind(
          renderer.renderer,
        );
        if (viewKindRes.kind == "errors") {
          return viewKindRes;
        }
        const viewKind = viewKindRes.value;
        if (
          dispatcherContext.injectedPrimitives?.injectedPrimitives.has(
            type.name as keyof T,
          )
        ) {
          const injectedPrimitive =
            dispatcherContext.injectedPrimitives.injectedPrimitives.get(
              type.name as keyof T,
            );
          if (injectedPrimitive == undefined) {
            return ValueOrErrors.Default.throwOne(
              `could not find injected primitive ${type.name as string}`,
            );
          }
          return dispatcherContext
            .getConcreteRenderer(
              viewKind as keyof ConcreteRendererKinds,
              renderer.renderer,
            )
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                injectedPrimitive.abstractRenderer(concreteRenderer),
              ),
            );
        }
        if (viewKind == "unit") {
          return dispatcherContext
            .getConcreteRenderer("unit", renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                UnitAbstractRenderer(
                  dispatcherContext.IdWrapper,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
              ),
            );
        }
        if (viewKind == "string") {
          return dispatcherContext
            .getConcreteRenderer("string", renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                StringAbstractRenderer(
                  dispatcherContext.IdWrapper,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
              ),
            );
        }
        if (viewKind == "number") {
          return dispatcherContext
            .getConcreteRenderer("number", renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                NumberAbstractRenderer(
                  dispatcherContext.IdWrapper,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
              ),
            );
        }
        if (viewKind == "boolean") {
          return dispatcherContext
            .getConcreteRenderer("boolean", renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                BoolAbstractRenderer(
                  dispatcherContext.IdWrapper,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
              ),
            );
        }
        if (viewKind == "secret") {
          return dispatcherContext
            .getConcreteRenderer("secret", renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                SecretAbstractRenderer(
                  dispatcherContext.IdWrapper,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
              ),
            );
        }
        if (viewKind == "base64File") {
          return dispatcherContext
            .getConcreteRenderer("base64File", renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                Base64FileAbstractRenderer(
                  dispatcherContext.IdWrapper,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
              ),
            );
        }
        if (viewKind == "date") {
          return dispatcherContext
            .getConcreteRenderer("date", renderer.renderer)
            .Then((concreteRenderer) =>
              ValueOrErrors.Default.return(
                DateAbstractRenderer(
                  dispatcherContext.IdWrapper,
                  dispatcherContext.ErrorRenderer,
                ).withView(concreteRenderer),
              ),
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
