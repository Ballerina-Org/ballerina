import { List, Map, Set } from "immutable";
import {
  BasicUpdater,
  DispatchCommonFormState,
  DispatchDelta,
  DispatchParsedType,
  Expr,
  FormLayout,
  PredicateFormLayout,
  PredicateValue,
  replaceWith,
  Updater,
  ValueOrErrors,
  ValueRecord,
  DispatchOnChange,
  IdWrapperProps,
  ErrorRendererProps,
  Option,
  Unit,
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonAbstractRendererForeignMutationsExpected,
  StringSerializedType,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";

import {
  RecordAbstractRendererReadonlyContext,
  RecordAbstractRendererForeignMutationsExpected,
  RecordAbstractRendererState,
  RecordAbstractRendererView,
} from "./state";

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
      visible?: Expr;
      disabled?: Expr;
      label?: string;
      GetDefaultState: () => CommonAbstractRendererState;
    }
  >,
  Layout: PredicateFormLayout,
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  isInlined: boolean,
  SerializedType: StringSerializedType,
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
          ) => ({
            value: _.value.fields.get(fieldName)!,
            type: _.type.fields.get(fieldName)!,
            ...(_.fieldStates?.get(fieldName) ||
              FieldTemplates.get(fieldName)!.GetDefaultState()),
            disabled: _.disabled,
            bindings: isInlined ? _.bindings : _.bindings.set("local", _.value),
            extraContext: _.extraContext,
            customPresentationContext: _.customPresentationContext,
            remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
            domNodeAncestorPath:
              _.domNodeAncestorPath + `[record][${fieldName}]`,
            serializedTypeHierarchy: [SerializedType].concat(
              _.serializedTypeHierarchy,
            ),
          }),
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
    const completeSerializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

    const domNodeId = props.context.domNodeAncestorPath + "[record]";

    if (!PredicateValue.Operations.IsRecord(props.context.value)) {
      console.error(
        `Record expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Record value expected but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

    const updatedBindings = isInlined
      ? props.context.bindings
      : props.context.bindings.set("local", props.context.value);

    const calculatedLayout = FormLayout.Operations.ComputeLayout(
      updatedBindings,
      Layout,
    );

    // TODO -- set error template up top
    if (calculatedLayout.kind == "errors") {
      console.error(calculatedLayout.errors.map((error) => error).join("\n"));
      return <></>;
    }

    const visibleFieldKeys = ValueOrErrors.Operations.All(
      List(
        FieldTemplates.map(({ visible }, fieldName) =>
          visible == undefined
            ? ValueOrErrors.Default.return(fieldName)
            : Expr.Operations.EvaluateAs("visibility predicate")(
                updatedBindings,
              )(visible).Then((value) =>
                ValueOrErrors.Default.return(
                  PredicateValue.Operations.IsBoolean(value) && value
                    ? fieldName
                    : null,
                ),
              ),
        ).valueSeq(),
      ),
    );

    if (visibleFieldKeys.kind == "errors") {
      console.error(visibleFieldKeys.errors.map((error) => error).join("\n"));
      return <></>;
    }

    const visibleFieldKeysSet = Set(
      visibleFieldKeys.value.filter((fieldName) => fieldName != null),
    );

    const disabledFieldKeys = ValueOrErrors.Operations.All(
      List(
        FieldTemplates.map(({ disabled }, fieldName) =>
          disabled == undefined
            ? ValueOrErrors.Default.return(null)
            : Expr.Operations.EvaluateAs("disabled predicate")(updatedBindings)(
                disabled,
              ).Then((value) =>
                ValueOrErrors.Default.return(
                  PredicateValue.Operations.IsBoolean(value) && value
                    ? fieldName
                    : null,
                ),
              ),
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
        <IdProvider domNodeId={domNodeId}>
          <props.view
            context={{
              ...props.context,
              completeSerializedTypeHierarchy,
              domNodeId,
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
