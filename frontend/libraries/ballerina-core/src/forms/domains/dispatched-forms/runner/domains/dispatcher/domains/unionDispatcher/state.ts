import { List, Map } from "immutable";

import {
  DispatcherContext,
  MapRepo,
  Template,
  UnionAbstractRenderer,
  UnionAbstractRendererState,
  ValueOrErrors,
} from "../../../../../../../../../main";

import { UnionType } from "../../../../../deserializer/domains/specification/domains/types/state";
import { UnionRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/union/state";
import { Dispatcher } from "../../state";

export const UnionDispatcher = {
  Operations: {
    Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
      type: UnionType<T>,
      unionRenderer: UnionRenderer<T>,
      dispatcherContext: DispatcherContext<T>,
      isNested: boolean,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      ValueOrErrors.Operations.All(
        List<ValueOrErrors<[string, Template<any, any, any, any>], string>>(
          type.args
            .entrySeq()
            .map(([caseName, caseType]) =>
              MapRepo.Operations.tryFindWithError(
                caseName,
                unionRenderer.cases,
                () => `cannot find case ${caseName}`,
              ).Then((caseRenderer) =>
                Dispatcher.Operations.DispatchAs(
                  caseType,
                  caseRenderer,
                  dispatcherContext,
                  `case ${caseName}`,
                  isNested,
                  caseName,
                ).Then((template) =>
                  ValueOrErrors.Default.return([caseName, template]),
                ),
              ),
            ),
        ),
      )
        .Then((templates) =>
          dispatcherContext
            .defaultState(type, unionRenderer)
            .Then((defaultState) =>
              unionRenderer.renderer.kind != "lookupRenderer"
                ? ValueOrErrors.Default.throwOne<
                    Template<any, any, any, any>,
                    string
                  >(
                    `received non lookup renderer kind "${unionRenderer.renderer.kind}" when resolving defaultState for union`,
                  )
                : dispatcherContext
                    .getConcreteRenderer(
                      "union",
                      unionRenderer.renderer.renderer,
                    )
                    .Then((concreteRenderer) =>
                      ValueOrErrors.Default.return(
                        UnionAbstractRenderer(
                          // TODO better typing for state and consider this pattern for other dispatchers
                          (
                            defaultState as UnionAbstractRendererState<any>
                          ).caseFormStates.map((caseState) => () => caseState),
                          Map(templates),
                          dispatcherContext.IdWrapper,
                          dispatcherContext.ErrorRenderer,
                        ).withView(concreteRenderer),
                      ),
                    ),
            ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested union`),
        ),
  },
};
