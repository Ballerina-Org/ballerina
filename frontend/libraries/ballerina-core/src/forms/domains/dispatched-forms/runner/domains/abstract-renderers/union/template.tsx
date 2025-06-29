import {
  BasicUpdater,
  CommonAbstractRendererReadonlyContext,
  DispatchParsedType,
  MapRepo,
  PredicateValue,
  Updater,
  ValueUnionCase,
  IdWrapperProps,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  Option,
  Unit,
  DispatchDelta,
  CommonAbstractRendererState,
  CommonAbstractRendererForeignMutationsExpected,
  StringSerializedType,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";

import {
  UnionAbstractRendererForeignMutationsExpected,
  UnionAbstractRendererReadonlyContext,
  UnionAbstractRendererState,
  UnionAbstractRendererView,
} from "./state";
import { Map } from "immutable";

export const UnionAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
>(
  defaultCaseStates: Map<string, () => CommonAbstractRendererState>,
  caseTemplates: Map<
    string,
    Template<
      CommonAbstractRendererReadonlyContext<
        DispatchParsedType<any>,
        PredicateValue,
        CustomPresentationContext
      > &
        CommonAbstractRendererState,
      CommonAbstractRendererState,
      CommonAbstractRendererForeignMutationsExpected<Flags>
    >
  >,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
) => {
  const embeddedCaseTemplate =
    (caseName: string) => (flags: Flags | undefined) =>
      caseTemplates
        .get(caseName)!
        .mapContext(
          (
            _: UnionAbstractRendererReadonlyContext<CustomPresentationContext> &
              UnionAbstractRendererState,
          ) => ({
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
            disabled: _.disabled,
            bindings: _.bindings,
            extraContext: _.extraContext,
            domNodeId: _.domNodeId,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            CustomPresentationContext: _.CustomPresentationContext,
            serializedTypeHierarchy: _.serializedTypeHierarchy,
          }),
        )
        .mapState(
          (
            _: BasicUpdater<CommonAbstractRendererState>,
          ): Updater<UnionAbstractRendererState> =>
            UnionAbstractRendererState.Updaters.Core.caseFormStates(
              MapRepo.Updaters.upsert(
                caseName,
                defaultCaseStates.get(caseName)!,
                _,
              ),
            ),
        )

        .mapForeignMutationsFromProps<
          UnionAbstractRendererForeignMutationsExpected<Flags>
        >((props) => ({
          onChange: (
            updater: Option<BasicUpdater<PredicateValue>>,
            nestedDelta: DispatchDelta<Flags>,
          ) => {
            const delta: DispatchDelta<Flags> = {
              kind: "UnionCase",
              caseName: [caseName, nestedDelta],
              flags,
            };
            const caseUpdater =
              updater.kind == "r"
                ? Option.Default.some(
                    ValueUnionCase.Updaters.fields(updater.value),
                  )
                : Option.Default.none<BasicUpdater<ValueUnionCase>>();
            props.foreignMutations.onChange(caseUpdater, delta);
            props.setState((_) => ({ ..._, modifiedByUser: true }));
          },
        }));

  return Template.Default<
    UnionAbstractRendererReadonlyContext<CustomPresentationContext> &
      UnionAbstractRendererState,
    UnionAbstractRendererState,
    UnionAbstractRendererForeignMutationsExpected<Flags>,
    UnionAbstractRendererView<CustomPresentationContext, Flags>
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

    const serializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

    return (
      <>
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId: props.context.identifiers.withoutLauncher,
              serializedTypeHierarchy,
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
