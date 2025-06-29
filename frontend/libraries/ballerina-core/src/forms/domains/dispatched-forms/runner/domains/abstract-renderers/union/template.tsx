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
            disabled: _.disabled,
            bindings: _.bindings,
            extraContext: _.extraContext,
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
        )}\n...When rendering union case field\n...${SerializedType}`,
      );
      return (
        <ErrorRenderer
          message={`${SerializedType}: UnionCase value expected for union case but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

    const completeSerializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

    const domNodeId = completeSerializedTypeHierarchy.join(".");

    return (
      <>
        <IdProvider domNodeId={domNodeId}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId,
              completeSerializedTypeHierarchy,
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
