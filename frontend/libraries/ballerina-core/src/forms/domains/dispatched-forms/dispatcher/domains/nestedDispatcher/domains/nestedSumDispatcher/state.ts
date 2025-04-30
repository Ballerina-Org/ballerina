import {
  DispatcherContext,
  Template,
  ValueOrErrors,
} from "../../../../../../../../../main";

import { NestedDispatcher } from "../../state";
import { SumAbstractRenderer } from "../../../abstract-renderers/sum/template";
import { SumType } from "../../../../../deserializer/domains/specification/domains/types/state";
import { BaseSumRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/baseRenderer/domains/sum/state";
import { BaseSumUnitDateRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/baseRenderer/domains/sumUnitDate/state";

export const NestedSumDispatcher = {
  Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
    type: SumType<T>,
    sumRenderer: BaseSumRenderer<T> | BaseSumUnitDateRenderer<T>,
    dispatcherContext: DispatcherContext<T>,
  ): ValueOrErrors<Template<any, any, any, any>, string> =>
    (sumRenderer.kind == "baseSumRenderer"
      ? NestedDispatcher.Operations.DispatchAs(
          type.args[0],
          sumRenderer.leftRenderer,
          dispatcherContext,
          "left",
        )
      : ValueOrErrors.Default.return<undefined, string>(undefined)
    )
      .Then((leftForm) =>
        (sumRenderer.kind == "baseSumRenderer"
          ? NestedDispatcher.Operations.DispatchAs(
              type.args[1],
              sumRenderer.rightRenderer,
              dispatcherContext,
              "right",
            )
          : ValueOrErrors.Default.return<undefined, string>(undefined)
        ).Then((rightForm) =>
          sumRenderer.kind == "baseSumUnitDateRenderer"
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
