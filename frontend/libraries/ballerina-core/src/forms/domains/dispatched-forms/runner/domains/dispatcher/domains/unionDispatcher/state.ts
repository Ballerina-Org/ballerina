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

import {
  DispatchParsedType,
  StringSerializedType,
  UnionType,
} from "../../../../../deserializer/domains/specification/domains/types/state";
import { UnionRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/union/state";
import { Dispatcher } from "../../state";

export const UnionDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContexts,
    >(
      renderer: UnionRenderer<T>,
      dispatcherContext: DispatcherContext<
        T,
        Flags,
        CustomPresentationContexts
      >,
      isNested: boolean,
      tableApi: string | undefined,
    ): ValueOrErrors<
      [Template<any, any, any, any>, StringSerializedType],
      string
    > =>
      ValueOrErrors.Operations.All(
        List<
          ValueOrErrors<
            [string, Template<any, any, any, any>, StringSerializedType],
            string
          >
        >(
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
                    [
                      string,
                      Template<any, any, any, any>,
                      StringSerializedType,
                    ],
                    string
                  >([caseName, template[0], template[1]]),
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
                .Then((concreteRenderer) => {
                  const serializedType = UnionType.SerializeToString(
                    Map(
                      templates.map((template) => [template[0], template[2]]),
                    ),
                  );
                  return ValueOrErrors.Default.return<
                    [Template<any, any, any, any>, StringSerializedType],
                    string
                  >([
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
                      serializedType,
                    ).withView(concreteRenderer),
                    serializedType,
                  ]);
                }),
            ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested union`),
        ),
  },
};
