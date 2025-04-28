import {
  DispatcherContext,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";

import { NestedDispatcher } from "../../state";
import { SumAbstractRenderer } from "../../../abstract-renderers/sum/template";
import {
  DispatchParsedType,
  SumType,
} from "../../../../../deserializer/domains/specification/domains/types/state";
import { NestedSumRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/nestedRenderer/domains/sum/state";
import { RecordFieldSumRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/recordFormRenderer/domains/recordFieldRenderer/domains/sum/state";
import { RecordFieldSumUnitDateRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/recordFormRenderer/domains/recordFieldRenderer/domains/sumUnitDate/state";
import { NestedSumUnitDateRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/nestedRenderer/domains/sumUnitDate/state";

export const NestedSumDispatcher = {
  Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
    type: SumType<T>,
    sumRenderer:
      | RecordFieldSumRenderer<T>
      | NestedSumRenderer<T>
      | RecordFieldSumUnitDateRenderer<T>
      | NestedSumUnitDateRenderer<T>,
    dispatcherContext: DispatcherContext<T>,
  ): ValueOrErrors<Template<any, any, any, any>, string> =>
    (sumRenderer.kind == "recordFieldSumRenderer" ||
    sumRenderer.kind == "nestedSumRenderer"
      ? NestedDispatcher.Operations.DispatchAs(
          type.args[0],
          sumRenderer.leftRenderer,
          dispatcherContext,
          "left",
        )
      : ValueOrErrors.Default.return<undefined, string>(undefined)
    )
      .Then((leftForm) =>
        (sumRenderer.kind == "recordFieldSumRenderer" ||
        sumRenderer.kind == "nestedSumRenderer"
          ? NestedDispatcher.Operations.DispatchAs(
              type.args[1],
              sumRenderer.rightRenderer,
              dispatcherContext,
              "right",
            )
          : ValueOrErrors.Default.return<undefined, string>(undefined)
        ).Then((rightForm) =>
          sumRenderer.kind == "nestedSumUnitDateRenderer" ||
          sumRenderer.kind == "recordFieldSumUnitDateRenderer"
            ? dispatcherContext
                .getConcreteRenderer(
                  "sumUnitDate",
                  sumRenderer.concreteRendererName,
                )
                .Then((concreteRenderer) =>
                  ValueOrErrors.Default.return(
                    SumAbstractRenderer(leftForm, rightForm)
                      .mapContext((_: any) => ({
                        ..._,
                        type: sumRenderer.type,
                        label: sumRenderer.label,
                        tooltip: sumRenderer.tooltip,
                        details: sumRenderer.details,
                      }))
                      .withView(concreteRenderer),
                  ),
                )
            : dispatcherContext
                .getConcreteRenderer("sum", sumRenderer.concreteRendererName)
                .Then((concreteRenderer) =>
                  ValueOrErrors.Default.return(
                    SumAbstractRenderer(leftForm, rightForm)
                      .mapContext((_: any) => ({
                        ..._,
                        type: sumRenderer.type,
                        label: sumRenderer.label,
                        tooltip: sumRenderer.tooltip,
                        details: sumRenderer.details,
                      }))
                      .withView(concreteRenderer),
                  ),
                ),
        ),
      )
      .MapErrors((errors) =>
        errors.map(
          (error) =>
            `${error}\n...When dispatching nested sum: ${sumRenderer.concreteRendererName}`,
        ),
      ),
};
