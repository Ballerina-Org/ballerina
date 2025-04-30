import { List, Map, Set } from "immutable";
import {
  BasicUpdater,
  Bindings,
  DispatchDelta,
  DispatchParsedType,
  Expr,
  FormLabel,
  FormLayout,
  MapRepo,
  PredicateFormLayout,
  PredicateValue,
  Updater,
  Value,
  ValueOrErrors,
  ValueRecord,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";

import {
  RecordAbstractRendererState,
  RecordAbstractRendererView,
} from "./state";
import { DispatchOnChange } from "../../../state";

export const RecordAbstractRenderer = <
  Context extends FormLabel & { bindings: Bindings },
  ForeignMutationsExpected,
>(
  FieldTemplates: Map<
    string,
    {
      template: Template<any, any, any, any>;
      visible?: Expr;
      disabled?: Expr;
      GetDefaultState: () => any;
    }
  >,
  Layout: PredicateFormLayout,
): Template<any, any, any, any> => {
  const embedFieldTemplate = (
    fieldName: string,
    fieldTemplate: Template<any, any, any, any>,
  ): Template<any, any, any, any> =>
    fieldTemplate
      .mapContext(
        (
          _: Value<ValueRecord> & {
            fieldStates: Map<string, any>;
            disabled: boolean;
            bindings: Bindings;
            type: DispatchParsedType<any>;
            extraContext: any;
          },
        ): Value<PredicateValue> & any => ({
          ..._,
          value: _.value.fields.get(fieldName)!,
          ...(_.fieldStates?.get(fieldName) ||
            FieldTemplates.get(fieldName)!.GetDefaultState()),
          disabled: _.disabled,
          bindings: _.bindings,
          extraContext: _.extraContext,
        }),
      )
      .mapState(
        (_: BasicUpdater<any>): Updater<RecordAbstractRendererState> =>
          RecordAbstractRendererState.Updaters.Template.upsertFieldState(
            fieldName,
            FieldTemplates.get(fieldName)!.GetDefaultState,
            _,
          ),
      )
      .mapForeignMutationsFromProps<{
        onChange: DispatchOnChange<ValueRecord>;
      }>(
        (
          props,
        ): {
          onChange: DispatchOnChange<PredicateValue>;
        } => ({
          onChange: (elementUpdater: any, nestedDelta: DispatchDelta) => {
            const delta: DispatchDelta = {
              kind: "RecordField",
              field: [fieldName, nestedDelta],
              recordType: props.context.type,
            };

            props.foreignMutations.onChange(
              (current: ValueRecord): ValueRecord =>
                PredicateValue.Operations.IsRecord(current)
                  ? PredicateValue.Default.record(
                      current.fields.update(
                        fieldName,
                        PredicateValue.Default.unit(),
                        elementUpdater,
                      ),
                    )
                  : current,
              delta,
            );

            props.setState((_) => ({
              ..._,
              commonFormState: {
                ..._.commonFormState,
                modifiedByUser: true,
              },
              fieldStates: MapRepo.Updaters.upsert(
                fieldName,
                () => FieldTemplates.get(fieldName)!.GetDefaultState(),
                (__) => ({
                  ...__,
                  commonFormState: {
                    ...__.commonFormState,
                    modifiedByUser: true,
                  },
                }),
              )(_.fieldStates),
            }));
          },
        }),
      );

  const EmbeddedFieldTemplates = FieldTemplates.map(
    (fieldTemplate, fieldName) =>
      embedFieldTemplate(fieldName, fieldTemplate.template),
  );

  return Template.Default<
    Context & Value<ValueRecord>,
    RecordAbstractRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueRecord>;
    },
    RecordAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => {
    const updatedBindings = props.context.bindings.set(
      "local",
      props.context.value,
    );

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
        <props.view
          context={{
            ...props.context,
            layout: calculatedLayout.value,
          }}
          foreignMutations={{
            ...props.foreignMutations,
          }}
          setState={props.setState}
          EmbeddedFields={EmbeddedFieldTemplates}
          VisibleFieldKeys={visibleFieldKeysSet}
          DisabledFieldKeys={disabledFieldKeysSet}
        />
      </>
    );
  }).any([]);
};
