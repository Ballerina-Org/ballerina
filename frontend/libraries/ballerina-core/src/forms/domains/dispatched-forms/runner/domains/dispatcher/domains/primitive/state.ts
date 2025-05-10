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

export const PrimitiveDispatcher = {
  Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
    type: DispatchPrimitiveType<T>,
    viewKind: string,
    renderer: NestedRenderer<T>,
    dispatcherContext: DispatcherContext<T>,
  ): ValueOrErrors<Template<any, any, any, any>, string> => {
    const result: ValueOrErrors<Template<any, any, any, any>, string> = (() => {
      if (renderer.renderer.kind != "lookupRenderer") {
        return ValueOrErrors.Default.throwOne(
          `expected primitive to have a renderer with kind == "lookupRenderer" but got ${renderer.renderer.kind}`,
        );
      }
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
            renderer.renderer.name,
          )
          .Then((concreteRenderer) =>
            ValueOrErrors.Default.return(
              injectedPrimitive.abstractRenderer(
                concreteRenderer,
                renderer.label,
                renderer.tooltip,
                renderer.details,
              ),
            ),
          );
      }
      if (viewKind == "unit") {
        return dispatcherContext
          .getConcreteRenderer("unit", renderer.renderer.name)
          .Then((concreteRenderer) =>
            ValueOrErrors.Default.return(
              UnitAbstractRenderer()
                .mapContext((_: any) => ({
                  ..._,
                  type: renderer.renderer.type,
                  label: renderer.label,
                  tooltip: renderer.tooltip,
                  details: renderer.details,
                }))
                .withView(concreteRenderer),
            ),
          );
      }
      if (viewKind == "string") {
        return dispatcherContext
          .getConcreteRenderer("string", renderer.renderer.name)
          .Then((concreteRenderer) =>
            ValueOrErrors.Default.return(
              StringAbstractRenderer()
                .mapContext((_: any) => ({
                  ..._,
                  type: renderer.renderer.type,
                  label: renderer.label,
                  tooltip: renderer.tooltip,
                  details: renderer.details,
                }))
                .withView(concreteRenderer),
            ),
          );
      }
      if (viewKind == "number") {
        return dispatcherContext
          .getConcreteRenderer("number", renderer.renderer.name)
          .Then((concreteRenderer) =>
            ValueOrErrors.Default.return(
              NumberAbstractRenderer()
                .mapContext((_: any) => ({
                  ..._,
                  type: renderer.renderer.type,
                  label: renderer.label,
                  tooltip: renderer.tooltip,
                  details: renderer.details,
                }))
                .withView(concreteRenderer),
            ),
          );
      }
      if (viewKind == "boolean") {
        return dispatcherContext
          .getConcreteRenderer("boolean", renderer.renderer.name)
          .Then((concreteRenderer) =>
            ValueOrErrors.Default.return(
              BoolAbstractRenderer()
                .mapContext((_: any) => ({
                  ..._,
                  type: renderer.renderer.type,
                  label: renderer.label,
                  tooltip: renderer.tooltip,
                  details: renderer.details,
                }))
                .withView(concreteRenderer),
            ),
          );
      }
      if (viewKind == "secret") {
        return dispatcherContext
          .getConcreteRenderer("secret", renderer.renderer.name)
          .Then((concreteRenderer) =>
            ValueOrErrors.Default.return(
              SecretAbstractRenderer()
                .mapContext((_: any) => ({
                  ..._,
                  type: renderer.renderer.type,
                  label: renderer.label,
                  tooltip: renderer.tooltip,
                  details: renderer.details,
                }))
                .withView(concreteRenderer),
            ),
          );
      }
      if (viewKind == "base64File") {
        return dispatcherContext
          .getConcreteRenderer("base64File", renderer.renderer.name)
          .Then((concreteRenderer) =>
            ValueOrErrors.Default.return(
              Base64FileAbstractRenderer()
                .mapContext((_: any) => ({
                  ..._,
                  type: renderer.renderer.type,
                  label: renderer.label,
                  tooltip: renderer.tooltip,
                  details: renderer.details,
                }))
                .withView(concreteRenderer),
            ),
          );
      }
      if (viewKind == "date") {
        return dispatcherContext
          .getConcreteRenderer("date", renderer.renderer.name)
          .Then((concreteRenderer) =>
            ValueOrErrors.Default.return(
              DateAbstractRenderer()
                .mapContext((_: any) => ({
                  ..._,
                  type: renderer.renderer.type,
                  label: renderer.label,
                  tooltip: renderer.tooltip,
                  details: renderer.details,
                }))
                .withView(concreteRenderer),
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
};
