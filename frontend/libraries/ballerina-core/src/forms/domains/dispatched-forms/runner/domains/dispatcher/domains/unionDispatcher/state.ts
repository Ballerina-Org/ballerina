import { List, Map } from "immutable";

import {
  DispatcherContext,
  DispatchInjectablesTypes,
  MapRepo,
  Template,
  UnionAbstractRenderer,
  UnionAbstractRendererState,
  ValueOrErrors,
} from "../../../../../../../../../main";

import { UnionRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/union/state";
import { Dispatcher } from "../../state";

export const UnionDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
      ExtraContext,
    >(
      renderer: UnionRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >,
      isNested: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<Template<any, any, any, any>, string> =>
      ValueOrErrors.Operations.All(
        List<ValueOrErrors<[string, Template<any, any, any, any>], string>>(
          renderer.type.args
            .entrySeq()
            .map(([caseName]) =>
              MapRepo.Operations.tryFindWithError(
                caseName,
                renderer.cases,
                () => `cannot find case ${caseName}`,
              ).Then((caseRenderer) =>
                Dispatcher.Operations.DispatchAs(
                  caseRenderer,
                  dispatcherContext,
                  `case ${caseName}`,
                  isNested,
                  false,
                  tableApi,
                ).Then((template) =>
                  ValueOrErrors.Default.return<
                    [string, Template<any, any, any, any>],
                    string
                  >([caseName, template]),
                ),
              ),
            ),
        ),
      )
        .Then((templates) =>
          dispatcherContext
            .defaultState(renderer.type, renderer)
            .Then((defaultState) =>
              dispatcherContext
                .getConcreteRenderer("union", renderer.concreteRenderer)
                .Then((concreteRenderer) =>
                  ValueOrErrors.Default.return<
                    Template<any, any, any, any>,
                    string
                  >(
                    UnionAbstractRenderer(
                      // TODO better typing for state and consider this pattern for other dispatchers
                      (
                        defaultState as UnionAbstractRendererState
                      ).caseFormStates.map((caseState) => () => caseState),
                      Map(
                        templates.map((template) => [template[0], template[1]]),
                      ),
                      dispatcherContext.IdProvider,
                      dispatcherContext.ErrorRenderer,
                    )
                      .mapContext((_: any) => ({
                        ..._,
                        type: renderer.type,
                      }))
                      .withView(concreteRenderer),
                  ),
                ),
            ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested union`),
        ),
  },
};
