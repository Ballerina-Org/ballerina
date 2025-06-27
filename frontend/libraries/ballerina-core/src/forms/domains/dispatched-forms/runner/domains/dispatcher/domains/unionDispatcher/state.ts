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
            .map(([caseName, caseType]) =>
              MapRepo.Operations.tryFindWithError(
                caseName,
                renderer.cases,
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
              renderer.renderer.kind != "lookupRenderer"
                ? ValueOrErrors.Default.throwOne<
                    [Template<any, any, any, any>, StringSerializedType],
                    string
                  >(
                    `received non lookup renderer kind "${renderer.renderer.kind}" when resolving defaultState for union`,
                  )
                : dispatcherContext
                    .getConcreteRenderer(
                      "union",
                      renderer.renderer.renderer,
                    )
                    .Then((concreteRenderer) =>
                      ValueOrErrors.Default.return<
                        [Template<any, any, any, any>, StringSerializedType],
                        string
                      >([
                        UnionAbstractRenderer(
                          // TODO better typing for state and consider this pattern for other dispatchers
                          (
                            defaultState as UnionAbstractRendererState
                          ).caseFormStates.map((caseState) => () => caseState),
                          Map(
                            templates.map((template) => [
                              template[0],
                              template[1],
                            ]),
                          ),
                          dispatcherContext.IdProvider,
                          dispatcherContext.ErrorRenderer,
                        ).withView(concreteRenderer),
                        UnionType.SerializeToString(
                          Map(
                            templates.map((template) => [
                              template[0],
                              template[2],
                            ]),
                          ),
                        ),
                      ]),
                    ),
            ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested union`),
        ),
  },
};
