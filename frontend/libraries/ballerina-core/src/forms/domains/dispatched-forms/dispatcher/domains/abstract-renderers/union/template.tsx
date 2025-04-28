import React, { Context } from "react";
import {
  BasicFun,
  BasicUpdater,
  FormLabel,
  PredicateValue,
  Updater,
  Value,
  ValueUnionCase,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";

import { UnionAbstractRendererState, UnionAbstractRendererView } from "./state";
import { Map, Set } from "immutable";
import { DispatchOnChange } from "../../../state";
import { DispatchCommonFormState } from "../../../../built-ins/state";

export const UnionAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>(
  //TODO: Use state and values
  defaultState: { Default: () => any },
  defaultValues: { Default: () => PredicateValue },
  caseTemplate: Template<any, any, any, any>,
) => {
  const embeddedCaseTemplate = (caseName: string) =>
    caseTemplate
      .mapForeignMutationsFromProps<
        ForeignMutationsExpected & {
          onChange: DispatchOnChange<ValueUnionCase>;
        }
      >(
        (
          props,
        ): ForeignMutationsExpected & {
          onChange: DispatchOnChange<PredicateValue>;
        } => ({
          ...props.foreignMutations,
          onChange: (elementUpdater: any, path: any) => {
            props.foreignMutations.onChange(
              (_) => ({
                ..._,
                fields: elementUpdater(_.fields),
              }),
              path,
            );
            props.setState((_) => ({ ..._, modifiedByUser: true }));
          },
        }),
      )
      .mapContext(
        (
          _: Context & Value<ValueUnionCase> & UnionAbstractRendererState,
        ): Context & Value<ValueUnionCase> & { caseNames: Set<string> } => {
          const context: Context &
            Value<ValueUnionCase> & { caseNames: Set<string> } = {
            ..._,
            ..._.customFormState.caseState,
            value: _.value.fields,
            commonFormState: {
              modifiedByUser: true,
            },
          };
          return context;
        },
      )
      .mapState(
        (
          _: BasicUpdater<{
            formFieldStates: any;
            commonFormState: DispatchCommonFormState;
          }>,
        ): Updater<UnionAbstractRendererState> =>
          UnionAbstractRendererState().Updaters.Core.customFormState((__) => ({
            ...__,
            caseState: _(__.caseState),
          })),
      );
  return Template.Default<
    Context & Value<ValueUnionCase>,
    UnionAbstractRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueUnionCase>;
    },
    UnionAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => {
    return (
      <>
        <props.view
          {...props}
          context={props.context}
          foreignMutations={{
            ...props.foreignMutations,
          }}
          embeddedCaseTemplate={embeddedCaseTemplate}
        />
      </>
    );
  }).any([]);
};
