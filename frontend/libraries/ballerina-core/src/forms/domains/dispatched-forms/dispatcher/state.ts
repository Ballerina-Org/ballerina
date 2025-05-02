import { DispatchDelta, TableFormDispatcher } from "../../../../../main";
import { BasicUpdater } from "../../../../fun/domains/updater/state";
import { ValueOrErrors } from "../../../../collections/domains/valueOrErrors/state";
import { DispatcherContext, Template } from "../../../../../main";
import { RecordFormDispatcher } from "./domains/recordFormDispatcher/state";
import { DispatchParsedType } from "../deserializer/domains/specification/domains/types/state";
import { Form } from "../deserializer/domains/specification/domains/form/state";

export type DispatchOnChange<Entity> = (
  updater: BasicUpdater<Entity>,
  delta: DispatchDelta,
) => void;

export const FormDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      formName: string,
      type: DispatchParsedType<T>,
      renderer: Form<T>,
      dispatcherContext: DispatcherContext<T>,
      isNested: boolean,
      launcherName?: string,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      (renderer.kind == "recordForm"
        ? type.kind != "record"
          ? ValueOrErrors.Default.throwOne<
              Template<any, any, any, any>,
              string
            >(`expected a record type, but got a ${type.kind} type`)
          : RecordFormDispatcher.Operations.Dispatch(
              type,
              renderer,
              dispatcherContext,
              isNested,
              formName,
              launcherName,
            )
        : ValueOrErrors.Default.throwOne<Template<any, any, any, any>, string>(
            `expected a record form but got a ${renderer.kind} form`,
          )
      ).MapErrors((errors) =>
        errors.map(
          (error) =>
            `${error}\n...When dispatching ${isNested ? "" : "launcher "}form: ${formName}`,
        ),
      ),
  },
};
