import { List } from "immutable";
import { DispatchDelta } from "../../../../../main";
import { BasicUpdater } from "../../../../fun/domains/updater/state";
import { ValueOrErrors } from "../../../../collections/domains/valueOrErrors/state";
import {
  Bindings,
  DispatcherContext,
  PredicateValue,
  Template,
} from "../../../../../main";
import { RecordFormDispatcher } from "./domains/recordFormDispatcher/state";
import { RecordFormRenderer } from "../deserializer/domains/specification/domains/form/domains/renderers/domains/recordFormRenderer/state";
// import { UnionFormRenderer } from "../deserializer/domains/specification/domains/form/domains/renderers/domains/unionFormRenderer/state";
import { DispatchParsedType } from "../deserializer/domains/specification/domains/types/state";

export type DispatchOnChange<Entity> = (
  updater: BasicUpdater<Entity>,
  delta: DispatchDelta,
) => void;

export const FormDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      formName: string,
      type: DispatchParsedType<T>,
      renderer: RecordFormRenderer<T>,
      // | UnionFormRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
    ): ValueOrErrors<Template<any, any, any, any>, string> => {
      const result: ValueOrErrors<
        Template<any, any, any, any>,
        string
      > = (() => {
        if (renderer.kind == "recordForm") {
          if (type.kind != "record") {
            return ValueOrErrors.Default.throwOne(
              `expected a record type, but got a ${type.kind} type`,
            );
          }
          return RecordFormDispatcher.Operations.Dispatch(
            type,
            renderer,
            dispatcherContext,
            false,
          );
        }
        return ValueOrErrors.Default.throwOne(
          `renderer kind ${renderer.kind} not supported for ${formName} `,
        );
      })();

      return result.MapErrors((errors) =>
        errors.map(
          (error) => `${error}\n...When dispatching form: ${formName}`,
        ),
      );

      // if (renderer.kind == "unionForm"){
      //   if (type.kind != "union"){
      //     return ValueOrErrors.Default.throwOne(
      //       `When parsing ${formName} expected a union type, but got a ${type.kind} type`,
      //     );
      //   }
      // }
    },
  },
};
