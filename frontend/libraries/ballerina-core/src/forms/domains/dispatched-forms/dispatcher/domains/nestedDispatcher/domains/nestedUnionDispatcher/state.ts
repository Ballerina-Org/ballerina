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
import { NestedDispatcher } from "../../state";
import { BaseUnionRenderer } from "../../../../../deserializer/domains/specification/domains/form/domains/renderers/domains/baseRenderer/domains/union/state";

export const NestedUnionDispatcher = {
  Dispatch: <T extends { [key in keyof T]: { type: any; state: any } }>(
    type: UnionType<T>,
    unionRenderer: BaseUnionRenderer<T>,
    dispatcherContext: DispatcherContext<T>,
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
              NestedDispatcher.Operations.DispatchAs(
                caseType,
                caseRenderer,
                dispatcherContext,
                `case ${caseName}`,
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
            ValueOrErrors.Default.return(
              UnionAbstractRenderer(
                // TODO better typing for state and consider this pattern for other dispatchers
                (
                  defaultState as UnionAbstractRendererState<any>
                ).caseFormStates.map((caseState) => () => caseState),
                Map(templates),
              ),
            ),
          ),
      )
      .MapErrors((errors) =>
        errors.map((error) => `${error}\n...When dispatching nested union`),
      ),
};
