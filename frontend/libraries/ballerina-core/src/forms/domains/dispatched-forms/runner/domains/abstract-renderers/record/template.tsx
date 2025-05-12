import { List, Map, Set } from "immutable";
import {
  BasicUpdater,
  Bindings,
  DispatchCommonFormState,
  DispatchDelta,
  DispatchParsedType,
  Expr,
  FormLabel,
  FormLayout,
  MapRepo,
  PredicateFormLayout,
  PredicateValue,
  RecordType,
  replaceWith,
  Updater,
  Value,
  ValueOrErrors,
  ValueRecord,
  DispatchOnChange,
  IdWrapperProps,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../template/state";

import {
  RecordAbstractRendererState,
  RecordAbstractRendererView,
} from "./state";

export const RecordAbstractRenderer = <
  Context extends FormLabel & { bindings: Bindings } & {
    identifiers: { withLauncher: string; withoutLauncher: string };
  },
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
  IdWrapper: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
): Template<any, any, any, any> => {
  const embedFieldTemplate = (
    fieldName: string,
    fieldTemplate: Template<any, any, any, any>,
  ): Template<any, any, any, any> =>
    fieldTemplate
      .mapContext(
        (
          _: Value<ValueRecord> & {
            identifiers: { withLauncher: string; withoutLauncher: string };
            fieldStates: Map<string, any>;
            disabled: boolean;
            bindings: Bindings;
            type: DispatchParsedType<any>;
            extraContext: any;
          },
        ): Value<PredicateValue> & { type: DispatchParsedType<any> } => ({
          ..._,
          identifiers: {
            withLauncher: _.identifiers.withLauncher.concat(`[${fieldName}]`),
            withoutLauncher: _.identifiers.withoutLauncher.concat(
              `[${fieldName}]`,
            ),
          },
          value: _.value.fields.get(fieldName)!,
          type:
            _.type.kind === "record" ? _.type.fields.get(fieldName) : undefined,
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

  return Template.Default<
    Context & Value<ValueRecord>,
    RecordAbstractRendererState,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueRecord>;
    },
    RecordAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => {
    if (!PredicateValue.Operations.IsRecord(props.context.value)) {
      console.error(
        `Record expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering record\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Record value expected for record but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

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
      <IdWrapper
        id={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
      >
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
      </IdWrapper>
    );
  }).any([]);
};
