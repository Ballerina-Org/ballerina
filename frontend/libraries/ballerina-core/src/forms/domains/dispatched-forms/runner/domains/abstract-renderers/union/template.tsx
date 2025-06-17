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
  Option,
  Unit,
  DispatchDelta,
  replaceWith,
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
  Flags = Unit,
>(
  defaultCaseStates: Map<string, () => CaseFormState>,
  caseTemplates: Map<string, Template<any, any, any, any>>,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const embeddedCaseTemplate =
    (caseName: string) => (flags: Flags | undefined) =>
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
                withLauncher: _.identifiers.withLauncher.concat(
                  `[${caseName}]`,
                ),
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
            onChange: DispatchOnChange<ValueUnionCase, Flags>;
          }
        >(
          (
            props,
          ): ForeignMutationsExpected & {
            onChange: DispatchOnChange<ValueUnionCase, Flags>;
          } => ({
            ...props.foreignMutations,
            onChange: (
              elementUpdater: Option<BasicUpdater<ValueUnionCase>>,
              nestedDelta: DispatchDelta<Flags>,
            ) => {
              const delta: DispatchDelta<Flags> = {
                kind: "UnionCase",
                caseName: [caseName, nestedDelta],
                flags,
              };
              props.foreignMutations.onChange(elementUpdater, delta);
              props.setState((_) => ({ ..._, modifiedByUser: true }));
            },
          }),
        );

  return Template.Default<
    UnionAbstractRendererReadonlyContext,
    UnionAbstractRendererState<CaseFormState>,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueUnionCase, Flags>;
    },
    UnionAbstractRendererView<CaseFormState, ForeignMutationsExpected, Flags>
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
