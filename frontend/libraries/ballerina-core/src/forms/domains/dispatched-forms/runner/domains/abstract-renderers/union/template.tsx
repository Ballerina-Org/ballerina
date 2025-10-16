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
  Renderer,
  useRegistryValueAtPath,
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
  ExtraContext = Unit,
>(
  defaultCaseStates: Map<string, () => CommonAbstractRendererState>,
  caseTemplates: Map<
    string,
    Template<
      CommonAbstractRendererReadonlyContext<
        DispatchParsedType<any>,
        PredicateValue,
        CustomPresentationContext,
        ExtraContext
      > &
        CommonAbstractRendererState,
      CommonAbstractRendererState,
      CommonAbstractRendererForeignMutationsExpected<Flags>
    >
  >,
  CaseRenderers: Map<string, Renderer<any>>,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  const embeddedCaseTemplate =
    (caseName: string) => (flags: Flags | undefined) =>
      caseTemplates
        .get(caseName)!
        .mapContext(
          (
            _: UnionAbstractRendererReadonlyContext<
              CustomPresentationContext,
              ExtraContext
            > &
              UnionAbstractRendererState,
          ) => {
            const labelContext =
              CommonAbstractRendererState.Operations.GetLabelContext(
                _.labelContext,
                CaseRenderers.get(caseName)!,
                true,
                caseName,
              );
            return {
              ...(_.caseFormStates.get(caseName)! ??
                defaultCaseStates.get(caseName)!()),
              localBindingsPath: _.localBindingsPath,
              globalBindings: _.globalBindings,
              type: _.type.args.get(caseName)!,
              disabled: _.disabled || _.globallyDisabled,
              globallyDisabled: _.globallyDisabled,
              readOnly: _.readOnly || _.globallyReadOnly,
              globallyReadOnly: _.globallyReadOnly,
              locked: _.locked,
              extraContext: _.extraContext,
              remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
              customPresentationContext: _.customPresentationContext,
              typeAncestors: [_.type as DispatchParsedType<any>].concat(
                _.typeAncestors,
              ),
              domNodeAncestorPath:
                _.domNodeAncestorPath + `[union][${caseName}]`,
              lookupTypeAncestorNames: _.lookupTypeAncestorNames,
              labelContext,
              path: _.path + `[${caseName}]`,
            };
          },
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
              sourceAncestorLookupTypeNames:
                nestedDelta.sourceAncestorLookupTypeNames,
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
    UnionAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      UnionAbstractRendererState,
    UnionAbstractRendererState,
    UnionAbstractRendererForeignMutationsExpected<Flags>,
    UnionAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[union]";

    const value = useRegistryValueAtPath(props.context.path);
    if (!value) {
      return <></>;
    }
    if (!PredicateValue.Operations.IsUnionCase(value)) {
      console.error(
        `UnionCase expected but got: ${JSON.stringify(
          value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: UnionCase value expected but got ${JSON.stringify(
            value,
          )}`}
        />
      );
    }

    return (
      <>
        <IdProvider domNodeId={domNodeId}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId,
              value,
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
