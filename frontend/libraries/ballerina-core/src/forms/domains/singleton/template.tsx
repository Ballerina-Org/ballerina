import { OrderedSet, Map, OrderedMap } from "immutable";
import {
  BasicUpdater,
  id,
  Unit,
  Debounced,
  Synchronized,
  unit,
  replaceWith,
  CoTypedFactory,
  Debounce,
  Synchronize,
  BasicFun,
  EntityFormState,
  EntityFormContext,
  EntityFormForeignMutationsExpected,
  EntityFormTemplate,
  EntityFormView,
  FieldTemplates,
  FieldValidationWithPath,
  FormValidatorSynchronized,
  OnChange,
  CommonFormState,
  DirtyStatus,
  FieldName,
  PredicateValue,
  ValueRecord,
  Delta,
  ParsedType,
  Sum,
  AsyncState,
  FormLayout,
  InfiniteStreamSources,
  ValueOrErrors,
  TableApiSource,
} from "../../../../main";
import { Template } from "../../../template/state";
import { Value } from "../../../value/state";

export const RecordForm = <
  FieldStates extends { formFieldStates: any },
  Context extends {
    tableApiSource: TableApiSource;
    fromApiParserByType: (
      value: any,
      type: ParsedType<any>,
    ) => ValueOrErrors<PredicateValue, string>;
  },
  ForeignMutationsExpected,
>() => ({
  Default: <Fields extends keyof FieldStates["formFieldStates"]>() => {
    type State = EntityFormState<
      Fields,
      FieldStates,
      Context,
      ForeignMutationsExpected
    >;
    type FieldTemplate<f extends Fields> = Template<
      Context & {
        customFormState: State["formFieldStates"][f]["customFormState"];
        commonFormState: State["formFieldStates"][f]["commonFormState"];
      } & Value<f> & {
          disabled: boolean;
          visible: boolean;
          type: ParsedType<any>;
          tableApiSource: TableApiSource;
          fromApiParserByType: (
            value: any,
            type: ParsedType<any>,
          ) => ValueOrErrors<PredicateValue, string>;
        },
      {
        customFormState: State["formFieldStates"][f]["customFormState"];
        commonFormState: State["formFieldStates"][f]["commonFormState"];
      },
      ForeignMutationsExpected & { onChange: OnChange<PredicateValue> }
    >;
    type EntityFormConfig = { [f in Fields]: FieldTemplate<f> };

    return {
      config: id<EntityFormConfig>,
      template: (
        config: EntityFormConfig,
        fieldLabels: Map<FieldName, string | undefined>,
        validation?: BasicFun<PredicateValue, Promise<FieldValidationWithPath>>,
      ): EntityFormTemplate<
        Fields,
        FieldStates,
        Context,
        ForeignMutationsExpected
      > => {
        const fieldTemplates: FieldTemplates<
          Fields,
          FieldStates,
          Context & {
            tableApiSource: TableApiSource;
            fromApiParserByType: (
              value: any,
              type: ParsedType<any>,
            ) => ValueOrErrors<PredicateValue, string>;
          },
          ForeignMutationsExpected
        > = {} as FieldTemplates<
          Fields,
          FieldStates,
          Context,
          ForeignMutationsExpected
        >;
        const setFieldTemplate = <field extends Fields>(field: field) => {
          fieldTemplates[field] = config[field]
            .mapContext<
              EntityFormContext<
                Fields,
                FieldStates,
                Context,
                ForeignMutationsExpected
              > & {
                disabled: boolean;
                visible: boolean;
                type: ParsedType<any>;
                tableApiSource: TableApiSource;
                fromApiParserByType: (
                  value: any,
                  type: ParsedType<any>,
                ) => ValueOrErrors<PredicateValue, string>;
              }
            >((_) => {
              // disabled flag is passed in from the wrapping container when mapping over fields
              const visibilitiesFromParent =
                _.visibilities?.kind == "form"
                  ? _.visibilities?.fields.get(field as string)!
                  : undefined;

              const disabledFieldsFromParent =
                _.disabledFields?.kind == "form"
                  ? _.disabledFields?.fields.get(field as string)!
                  : undefined;

              return {
                rootValue: _.rootValue,
                value: (_.value as ValueRecord).fields.get(field as string),
                extraContext: _.extraContext,
                disabled: _.disabled,
                visible: _.visible,
                commonFormState: _.formFieldStates[field].commonFormState,
                customFormState: _.formFieldStates[field].customFormState,
                formFieldStates: _.formFieldStates[field].formFieldStates,
                elementFormStates: _.formFieldStates[field].elementFormStates,
                visibilities: visibilitiesFromParent,
                disabledFields: disabledFieldsFromParent,
                globalConfiguration: _.globalConfiguration,
                tableApiSource: _.tableApiSource,
                fromApiParserByType: _.fromApiParserByType,
              } as any;
            })
            .mapState<State>((_) => (current) => {
              return {
                ...current,
                formFieldStates: {
                  ...current.formFieldStates,
                  [field]: _(current.formFieldStates[field]),
                },
              };
            })
            .mapForeignMutationsFromProps<
              EntityFormForeignMutationsExpected<
                Fields,
                FieldStates,
                Context,
                ForeignMutationsExpected
              >
            >((props) => ({
              ...props.foreignMutations,
              onChange: (_: BasicUpdater<PredicateValue>, nestedDelta) => {
                const stateUpdater: BasicUpdater<
                  EntityFormState<
                    Fields,
                    FieldStates,
                    Context,
                    ForeignMutationsExpected
                  >
                > = validation
                  ? (_) => ({
                      ..._,
                      commonFormState: {
                        ..._.commonFormState,
                        modifiedByUser: true,
                        validation:
                          Debounced.Updaters.Template.value<FormValidatorSynchronized>(
                            Synchronized.Updaters.value(replaceWith(unit)),
                          )(_.commonFormState.validation),
                      },
                      formFieldStates: {
                        ..._.formFieldStates,
                        [field]: {
                          ..._.formFieldStates[field],
                          commonFormState: {
                            ..._.formFieldStates[field].commonFormState,
                            modifiedByUser: true,
                            validation:
                              Debounced.Updaters.Template.value<FormValidatorSynchronized>(
                                Synchronized.Updaters.value(replaceWith(unit)),
                              )(_.commonFormState.validation),
                          },
                        },
                      },
                    })
                  : (_) => ({
                      ..._,
                      commonFormState: {
                        ..._.commonFormState,
                        modifiedByUser: true,
                      },
                      formFieldStates: {
                        ..._.formFieldStates,
                        [field]: {
                          ..._.formFieldStates[field],
                          commonFormState: {
                            ..._.formFieldStates[field].commonFormState,
                            modifiedByUser: true,
                          },
                        },
                      },
                    });
                setTimeout(() => {
                  props.setState(stateUpdater);
                }, 0);

                const delta: Delta = {
                  kind: "RecordField",
                  field: [field as string, nestedDelta],
                  recordType: props.context.type,
                };

                props.foreignMutations.onChange(
                  (current: PredicateValue): PredicateValue =>
                    PredicateValue.Operations.IsRecord(current)
                      ? PredicateValue.Default.record(
                          current.fields.update(
                            field as string,
                            PredicateValue.Default.unit(),
                            _,
                          ),
                        )
                      : current,
                  delta,
                );
              },
            }));
        };
        Object.keys(config).forEach((_) => {
          const field = _ as Fields;
          setFieldTemplate(field);
        });
        return Template.Default<
          EntityFormContext<
            Fields,
            FieldStates,
            Context,
            ForeignMutationsExpected
          >,
          State,
          EntityFormForeignMutationsExpected<
            Fields,
            FieldStates,
            Context,
            ForeignMutationsExpected
          >,
          EntityFormView<Fields, FieldStates, Context, ForeignMutationsExpected>
        >((props) => {
          const globalConfig: Sum<PredicateValue, "not initialized"> = (() => {
            if (
              props.context.globalConfiguration.kind != "l" &&
              props.context.globalConfiguration.kind != "r"
            ) {
              // global config is in an async state
              if (
                AsyncState.Operations.hasValue(
                  props.context.globalConfiguration,
                )
              ) {
                return Sum.Default.left<PredicateValue, "not initialized">(
                  props.context.globalConfiguration.value,
                );
              }
              return Sum.Default.right<PredicateValue, "not initialized">(
                "not initialized",
              );
            }
            return props.context.globalConfiguration as Sum<
              PredicateValue,
              "not initialized"
            >;
          })();

          if (globalConfig.kind == "r") {
            console.error("global configuration is not initialized");
            return <></>;
          }

          const Layout = FormLayout.Operations.ComputeLayout(
            OrderedMap([["global", globalConfig.value]]),
            props.context.layout,
          );
          if (Layout.kind == "errors") {
            return <>Error parsing layout {JSON.stringify(Layout.errors)}</>;
          }
          const visibleFieldKeys: OrderedSet<FieldName> = (() => {
            if (
              props.context.visibilities == undefined ||
              props.context.visible == false ||
              props.context.visibilities.kind != "form"
            )
              return OrderedSet();

            return props.context.visibilities.fields
              .filter((_) => _.value == true)
              .keySeq()
              .toOrderedSet();
          })();

          const disabledFieldKeys: OrderedSet<FieldName> = (() => {
            if (
              props.context.disabledFields == undefined ||
              props.context.disabled ||
              props.context.disabledFields.kind != "form"
            )
              return OrderedSet(
                Object.keys(props.context.value.fields.toJS() as object),
              );

            return props.context.disabledFields.fields
              .filter((_) => _.value == true)
              .keySeq()
              .toOrderedSet();
          })();

          return (
            <>
              <props.view
                {...props}
                context={{
                  ...props.context,
                  layout: Layout.value,
                }}
                VisibleFieldKeys={visibleFieldKeys}
                DisabledFieldKeys={disabledFieldKeys}
                EmbeddedFields={fieldTemplates}
                FieldLabels={fieldLabels}
              />
            </>
          );
        }).any([
          ValidateRunner<
            EntityFormContext<
              Fields,
              FieldStates,
              Context,
              ForeignMutationsExpected
            >,
            State,
            EntityFormForeignMutationsExpected<
              Fields,
              FieldStates,
              Context,
              ForeignMutationsExpected
            >,
            PredicateValue
          >(validation),
        ]);
      },
    };
  },
});

// TODO: Validate runner and dirty status are also used to ensure to element is initialised, but this should be further debugged with a more correct solution
export const ValidateRunner = <
  Context,
  FormState extends { commonFormState: CommonFormState },
  ForeignMutationsExpected,
  Entity extends PredicateValue,
>(
  validation?: BasicFun<Entity, Promise<FieldValidationWithPath>>,
) => {
  const Co = CoTypedFactory<Context & Value<Entity> & FormState, FormState>();
  return Co.Template<ForeignMutationsExpected & { onChange: OnChange<Entity> }>(
    validation
      ? Co.Repeat(
          Debounce<FormValidatorSynchronized, Value<Entity>>(
            Synchronize<Unit, FieldValidationWithPath, Value<Entity>>(
              (_) => (validation ? validation(_.value) : Promise.resolve([])),
              () => "transient failure",
              3,
              50,
            ),
            50,
          ).embed(
            (_) => ({ ..._.commonFormState.validation, value: _.value }),
            (_) => (curr) => ({
              ...curr,
              commonFormState: {
                ...curr.commonFormState,
                validation: _(curr.commonFormState.validation),
              },
            }),
          ),
        )
      : Co.SetState((curr) => ({
          ...curr,
          commonFormState: {
            ...curr.commonFormState,
            validation: Debounced.Updaters.Core.dirty(
              replaceWith<DirtyStatus>("not dirty"),
            ),
          },
        })),
    {
      interval: 15,
      runFilter: (props) =>
        Debounced.Operations.shouldCoroutineRun(
          props.context.commonFormState.validation,
        ),
    },
  );
};
