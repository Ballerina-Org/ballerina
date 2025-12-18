import { List, Map, Set } from "immutable";
import {
  BasicUpdater,
  DispatchCommonFormState,
  DispatchDelta,
  DispatchParsedType,
  PredicateValue,
  replaceWith,
  Updater,
  ValueRecord,
  DispatchOnChange,
  IdWrapperProps,
  ErrorRendererProps,
  Option,
  Unit,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererForeignMutationsExpected,
  FormLayout,
  ValueOrErrors,
  PredicateFormLayout,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";

import {
  RecordAbstractRendererReadonlyContext,
  RecordAbstractRendererForeignMutationsExpected,
  RecordAbstractRendererState,
  RecordAbstractRendererView,
} from "./state";
import { RecordFieldRenderer } from "../../../../deserializer/domains/specification/domains/forms/domains/renderer/domains/record/domains/recordFieldRenderer/state";

export const RecordAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  FieldTemplates: Map<
    string,
    {
      template: Template<
        CommonAbstractRendererReadonlyContext<
          DispatchParsedType<any>,
          PredicateValue,
          CustomPresentationContext,
          ExtraContext
        >,
        CommonAbstractRendererState,
        CommonAbstractRendererForeignMutationsExpected<Flags>
      >;
      label?: string;
      GetDefaultState: () => CommonAbstractRendererState;
    }
  >,
  FieldRenderers: Map<string, RecordFieldRenderer<any>>,
  Layout: PredicateFormLayout,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  isInlined: boolean,
): Template<
  RecordAbstractRendererReadonlyContext<
    CustomPresentationContext,
    ExtraContext
  > &
    RecordAbstractRendererState,
  RecordAbstractRendererState,
  RecordAbstractRendererForeignMutationsExpected<Flags>,
  RecordAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
> => {
  const embedFieldTemplate =
    (
      fieldName: string,
      fieldTemplate: Template<
        CommonAbstractRendererReadonlyContext<
          DispatchParsedType<any>,
          PredicateValue,
          CustomPresentationContext,
          ExtraContext
        >,
        CommonAbstractRendererState,
        CommonAbstractRendererForeignMutationsExpected<Flags>
      >,
    ) =>
    (flags: Flags | undefined) =>
      fieldTemplate
        .mapContext(
          (
            _: RecordAbstractRendererReadonlyContext<
              CustomPresentationContext,
              ExtraContext
            > &
              RecordAbstractRendererState,
          ) => {
            const fieldRenderer = FieldRenderers.get(fieldName)!;
            const labelContext =
              CommonAbstractRendererState.Operations.GetLabelContext(
                _.labelContext,
                fieldRenderer,
              );
            return {
              value: PredicateValue.Operations.IsUnit(_.value)
                ? _.value
                : _.value.fields.get(fieldName)!,
              type: fieldRenderer.renderer.type,
              ...(_.fieldStates?.get(fieldName) ||
                FieldTemplates.get(fieldName)!.GetDefaultState()),
              disabled: _.disabled || _.globallyDisabled,
              globallyDisabled: _.globallyDisabled,
              readOnly: _.readOnly || _.globallyReadOnly,
              globallyReadOnly: _.globallyReadOnly,
              locked: _.locked,
              bindings: isInlined
                ? _.bindings
                : _.bindings.set("local", _.value),
              extraContext: _.extraContext,
              customPresentationContext: _.customPresentationContext,
              remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
              domNodeAncestorPath: _.domNodeAncestorPath + `[${fieldName}]`,
              legacy_domNodeAncestorPath:
                _.legacy_domNodeAncestorPath + `[record][${fieldName}]`,
              predictionAncestorPath:
                _.predictionAncestorPath + `[${fieldName}]`,
              layoutAncestorPath:
                _.layoutAncestorPath + `[record][${fieldName}]`,
              labelContext,
              typeAncestors: [_.type as DispatchParsedType<any>].concat(
                _.typeAncestors,
              ),
              lookupTypeAncestorNames: _.lookupTypeAncestorNames,
              preprocessedSpecContext: _.preprocessedSpecContext,
              usePreprocessor: _.usePreprocessor,
            };
          },
        )
        .mapState(
          (
            _: BasicUpdater<CommonAbstractRendererState>,
          ): Updater<RecordAbstractRendererState> =>
            RecordAbstractRendererState.Updaters.Template.upsertFieldState(
              fieldName,
              FieldTemplates.get(fieldName)!.GetDefaultState,
              _,
            ),
        )
        .mapForeignMutationsFromProps<{
          onChange: DispatchOnChange<ValueRecord, Flags>;
        }>(
          (
            props,
          ): {
            onChange: DispatchOnChange<PredicateValue, Flags>;
          } => ({
            onChange: (
              elementUpdater: Option<BasicUpdater<PredicateValue>>,
              nestedDelta: DispatchDelta<Flags>,
            ) => {
              const delta: DispatchDelta<Flags> = {
                kind: "RecordField",
                field: [fieldName, nestedDelta],
                recordType: props.context.type,
                flags,
                sourceAncestorLookupTypeNames:
                  nestedDelta.sourceAncestorLookupTypeNames,
              };

              props.foreignMutations.onChange(
                elementUpdater.kind == "l"
                  ? Option.Default.none()
                  : Option.Default.some((current: ValueRecord) =>
                      PredicateValue.Default.record(
                        current.fields.update(
                          fieldName,
                          PredicateValue.Default.unit(),
                          elementUpdater.value,
                        ),
                      ),
                    ),
                delta,
              );

              props.setState(
                RecordAbstractRendererState.Updaters.Core.commonFormState(
                  DispatchCommonFormState.Updaters.modifiedByUser(
                    replaceWith(true),
                  ),
                ).then(
                  RecordAbstractRendererState.Updaters.Template.upsertFieldState(
                    fieldName,
                    FieldTemplates.get(fieldName)!.GetDefaultState,
                    (_) => ({
                      ..._,
                      commonFormState:
                        DispatchCommonFormState.Updaters.modifiedByUser(
                          replaceWith(true),
                        )(_.commonFormState),
                    }),
                  ),
                ),
              );
            },
          }),
        );

  const EmbeddedFieldTemplates = FieldTemplates.map(
    (fieldTemplate, fieldName) =>
      embedFieldTemplate(fieldName, fieldTemplate.template),
  );

  const FieldLabels = FieldTemplates.map(
    (fieldTemplate) => fieldTemplate.label,
  );

  return Template.Default<
    RecordAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      RecordAbstractRendererState,
    RecordAbstractRendererState,
    RecordAbstractRendererForeignMutationsExpected<Flags>,
    RecordAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath;
    const legacy_domNodeId =
      props.context.legacy_domNodeAncestorPath + "[record]";
    if (
      !PredicateValue.Operations.IsRecord(props.context.value) &&
      !PredicateValue.Operations.IsUnit(props.context.value)
    ) {
      console.error(
        `Record or unit value expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Record or unit value expected but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

    const updatedBindings = isInlined
      ? props.context.bindings
      : props.context.bindings.set("local", props.context.value);

    const layoutFromPreprocessor =
      props.context.preprocessedSpecContext?.formLayouts.get(
        props.context.layoutAncestorPath,
      );

    if (layoutFromPreprocessor == undefined) {
      console.warn("Layout not found for " + props.context.layoutAncestorPath);
    }

    const calculatedLayout =
      layoutFromPreprocessor != undefined
        ? ValueOrErrors.Default.return(layoutFromPreprocessor)
        : FormLayout.Operations.ComputeLayout(updatedBindings, Layout);

    // TODO -- set error template up top
    if (calculatedLayout.kind == "errors") {
      console.error(calculatedLayout.errors.map((error) => error).join("\n"));
      return <></>;
    }

    const visibleFieldKeys = ValueOrErrors.Operations.All(
      List(
        FieldTemplates.map((_, fieldName) =>
          ValueOrErrors.Default.return<string, string>(fieldName),
        ).valueSeq(),
      ),
    );

    if (visibleFieldKeys.kind == "errors") {
      console.error(visibleFieldKeys.errors.map((error) => error).join("\n"));
      return <></>;
    }

    // TODO: find a better way to warn about missing fields without cluttering the console
    // visibleFieldKeys.value.forEach((field) => {
    //   if (field != null && !FieldTemplates.has(field)) {
    //     console.warn(
    //       `Field ${field} is defined in the visible fields, but not in the FieldTemplates. A renderer in the record fields is missing for this field.
    //       \n...When rendering \n...${domNodeId}
    //       `,
    //     );
    //   }
    // });

    const visibleFieldKeysSet = Set(
      visibleFieldKeys.value.filter((fieldName) => fieldName != null),
    );

    const disabledFieldKeys = ValueOrErrors.Operations.All(
      List(
        FieldTemplates.map((_, fieldName) =>
          props.context.preprocessedSpecContext?.disabledFields?.has(
            props.context.predictionAncestorPath + `[${fieldName}]`,
          )
            ? ValueOrErrors.Default.return(fieldName)
            : ValueOrErrors.Default.return(null),
        ).valueSeq(),
      ),
    );

    // TODO -- set the top level state as error
    if (disabledFieldKeys.kind == "errors") {
      console.error(disabledFieldKeys.errors.map((error) => error).join("\n"));
      return <></>;
    }

    const disabledFieldKeysSet = Set(
      disabledFieldKeys.value.filter((fieldName) => fieldName != null),
    );

    return (
      <>
        <IdProvider domNodeId={props.context.usePreprocessor ? domNodeId : legacy_domNodeId}>
          <props.view
            context={{
              ...props.context,
              domNodeId,
              legacy_domNodeId,
              layout: calculatedLayout.value,
            }}
            foreignMutations={{
              ...props.foreignMutations,
            }}
            setState={props.setState}
            EmbeddedFields={EmbeddedFieldTemplates}
            VisibleFieldKeys={visibleFieldKeysSet}
            DisabledFieldKeys={disabledFieldKeysSet}
            FieldLabels={FieldLabels}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};
