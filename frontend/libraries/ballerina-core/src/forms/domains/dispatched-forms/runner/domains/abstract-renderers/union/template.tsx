import {
  BasicUpdater,
  CommonAbstractRendererReadonlyContext,
  DispatchCommonFormState,
  DispatchParsedType,
  MapRepo,
  PredicateValue,
  UnionType,
  Updater,
  ValueUnionCase,
  DispatchOnChange,
  IdWrapperProps,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";

import {
  UnionAbstractRendererReadonlyContext,
  UnionAbstractRendererState,
  UnionAbstractRendererView,
} from "./state";
import { Map } from "immutable";

export const UnionAbstractRenderer = <
  ForeignMutationsExpected,
  CaseFormState extends { commonFormState: DispatchCommonFormState },
>(
  defaultCaseStates: Map<string, () => CaseFormState>,
  caseTemplates: Map<string, Template<any, any, any, any>>,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const embeddedCaseTemplate = (caseName: string) =>
    caseTemplates
      .get(caseName)!
      .mapContext(
        (
          _: UnionAbstractRendererReadonlyContext &
            UnionAbstractRendererState<CaseFormState> & {
              type: UnionType<any>;
            },
        ): CommonAbstractRendererReadonlyContext<
          UnionType<any>,
          ValueUnionCase
        > & {
          type: DispatchParsedType<any>;
        } & UnionAbstractRendererState<CaseFormState> => {
          const context = {
            ..._,
            ...(_.caseFormStates.get(caseName)! ??
              defaultCaseStates.get(caseName)!()),
            value: _.value.fields,
            type: _.type.args.get(caseName)!,
            identifiers: {
              withLauncher: _.identifiers.withLauncher.concat(`[${caseName}]`),
              withoutLauncher: _.identifiers.withoutLauncher.concat(
                `[${caseName}]`,
              ),
            },
          };
          return context;
        },
      )
      .mapState(
        (
          _: BasicUpdater<CaseFormState>,
        ): Updater<UnionAbstractRendererState<CaseFormState>> =>
          UnionAbstractRendererState<CaseFormState>().Updaters.Core.caseFormStates(
            MapRepo.Updaters.upsert(
              caseName,
              defaultCaseStates.get(caseName)!,
              _,
            ),
          ),
      )

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
      );

  return Template.Default<
    UnionAbstractRendererReadonlyContext,
    UnionAbstractRendererState<CaseFormState>,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueUnionCase>;
    },
    UnionAbstractRendererView<CaseFormState, ForeignMutationsExpected>
  >((props) => {
    if (!PredicateValue.Operations.IsUnionCase(props.context.value)) {
      console.error(
        `UnionCase expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering union case field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: UnionCase value expected for union case but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }
    return (
      <>
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId: props.context.identifiers.withoutLauncher,
            }}
            foreignMutations={{
              ...props.foreignMutations,
            }}
            embeddedCaseTemplate={embeddedCaseTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
