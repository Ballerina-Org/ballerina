import { List, Map } from "immutable";

import {
  DispatcherContext,
  DispatchInjectablesTypes,
  MapRepo,
  PredicateValue,
  Template,
  UnionAbstractRenderer,
  UnionAbstractRendererState,
  ValueOrErrors,
} from "../../../../../../../../../main";

import { UnionRenderer } from "../../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/union/state";
import { Dispatcher } from "../../state";
import { DispatcherContextWithApiSources } from "../../../../state";

export const UnionDispatcher = {
  Operations: {
    Dispatch: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      renderer: UnionRenderer<T>,
      dispatcherContext: DispatcherContextWithApiSources<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      isInlined: boolean,
      isNested: boolean,
      tableApi: string | undefined,
      currentLookupRenderer: string | undefined,
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
                  ValueOrErrors.Operations.All(
                    List(
                      renderer.cases
                        .map((caseRenderer, caseName) =>
                          renderer.type.args.has(caseName)
                            ? dispatcherContext
                                .defaultValue(
                                  renderer.type.args.get(caseName)!,
                                  caseRenderer,
                                )
                                .Map((_): [string, PredicateValue] => [
                                  caseName,
                                  _,
                                ])
                            : ValueOrErrors.Default.throwOne<
                                PredicateValue,
                                string
                              >(
                                `case ${caseName} not found in type ${JSON.stringify(renderer.type, null, 2)}`,
                              ).Map((_): [string, PredicateValue] => [
                                caseName,
                                _,
                              ]),
                        )
                        .valueSeq(),
                    ),
                  ).Then((defaultCaseValues) =>
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
                          defaultCaseValues.map(([caseName, defaultValue]) => [
                            caseName,
                            () => defaultValue,
                          ]),
                        ),
                        Map(
                          templates.map((template) => [
                            template[0],
                            template[1],
                          ]),
                        ),
                        renderer.cases,
                        dispatcherContext.IdProvider,
                        dispatcherContext.ErrorRenderer,
                      )
                        .mapContext((_: any) => ({
                          ..._,
                          type: renderer.type,
                          layoutAncestorPath: currentLookupRenderer
                            ? `[${currentLookupRenderer}]`
                            : isInlined
                              ? _.layoutAncestorPath + "[inline]"
                              : _.layoutAncestorPath,
                        }))
                        .withView(concreteRenderer),
                    ),
                  ),
                ),
            ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When dispatching nested union`),
        ),
  },
};
