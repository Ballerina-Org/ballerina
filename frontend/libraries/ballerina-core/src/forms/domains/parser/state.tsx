import { List, Map, OrderedSet, Set } from "immutable";
import {
  Unit,
  Guid,
  ParsedFormJSON,
  BuiltIns,
  Sum,
  BasicFun,
  Template,
  unit,
  EditFormState,
  EditFormTemplate,
  ApiErrors,
  CreateFormTemplate,
  EntityFormTemplate,
  CommonFormState,
  CreateFormState,
  EditFormContext,
  CreateFormContext,
  Synchronized,
  simpleUpdater,
  defaultValue,
  fromAPIRawValue,
  toAPIRawValue,
  EditFormForeignMutationsExpected,
  ParsedType,
  InjectedPrimitives,
  Injectables,
  ApiConverters,
  ApiResponseChecker,
  Debounced,
  ParsedFormConfig,
  PredicateValue,
  FieldPredicateExpressions,
  FormFieldPredicateEvaluation,
  ValueOrErrors,
  PassthroughFormState,
  PassthroughFormContext,
  PassthroughFormTemplate,
  defaultState,
  FieldName,
  ParsedRecordFormConfig,
  ParsedTableFormConfig,
  TableApiSources,
  PredicateVisibleColumns,
} from "../../../../main";
import { EnumReference } from "../collection/domains/reference/state";
import { SearchableInfiniteStreamState } from "../primitives/domains/searchable-infinite-stream/state";
import { RecordForm } from "../singleton/template";
import { ParsedRenderer } from "./domains/renderer/state";

export type ParsedRecordForm<T> = {
  initialFormState: any;
  formConfig: any;
  formName: string;
  formDef: ParsedRecordFormConfig<T>;
  visibilityPredicateExpressions: FieldPredicateExpressions;
  disabledPredicatedExpressions: FieldPredicateExpressions;
  fieldLabels: Map<FieldName, string | undefined>;
  form: EntityFormTemplate<any, any, any, any>;
};
export type ParsedTableForm<T> = {
  initialFormState: any;
  formConfig: any;
  formName: string;
  formDef: ParsedTableFormConfig<T>;
  visibleColumns: PredicateVisibleColumns;
  columnHeaders: Map<FieldName, string | undefined>;
  form: EntityFormTemplate<any, any, any, any>;
};
export const ParseRecordForm = <T,>(
  formName: string,
  formDef: ParsedRecordFormConfig<T>,
  nestedContainerFormView: any,
  formViews: Record<string, Record<string, any>>,
  forms: ParsedForms<T>,
  fieldsViewsConfig: any,
  infiniteStreamSources: any,
  enumOptionsSources: EnumOptionsSources,
  defaultValue: BasicFun<ParsedType<T>, any>,
  defaultState: BasicFun<ParsedType<T>, any>,
  injectedPrimitives?: InjectedPrimitives<T>,
  tableApiSources?: TableApiSources,
): Omit<ParsedRecordForm<T>, "form"> => {
  const formConfig: any = {};
  let visibilityPredicateExpressions: FieldPredicateExpressions = Map();
  let disabledPredicatedExpressions: FieldPredicateExpressions = Map();
  let fieldLabels: Map<FieldName, string | undefined> = Map();
  const initialFormState: any = {
    commonFormState: CommonFormState.Default(),
    formFieldStates: {},
  };

  const fieldNames = Object.keys(fieldsViewsConfig);

  fieldNames.forEach((fieldName) => {
    const parsedFormConfig = ParsedRenderer.Operations.RendererToForm(
      fieldName,
      {
        formViews,
        forms,
        nestedContainerFormView,
        defaultValue,
        defaultState,
        enumOptionsSources,
        infiniteStreamSources,
        injectedPrimitives,
        tableApiSources,
      },
      formDef.fields.get(fieldName)!,
    );
    if (parsedFormConfig.kind == "errors") {
      console.error(parsedFormConfig.errors.toJS());
      throw Error(`Error parsing form ${fieldsViewsConfig[fieldName]}`);
    }
    formConfig[fieldName] = parsedFormConfig.value.form.renderer;
    visibilityPredicateExpressions = visibilityPredicateExpressions.set(
      fieldName,
      parsedFormConfig.value.visibilityPredicateExpression,
    );
    disabledPredicatedExpressions = disabledPredicatedExpressions.set(
      fieldName,
      parsedFormConfig.value.disabledPredicatedExpression,
    );
    fieldLabels = fieldLabels.set(fieldName, parsedFormConfig.value.label);
    initialFormState["formFieldStates"][fieldName] =
      parsedFormConfig.value.form.initialState;
  });

  return {
    initialFormState,
    formName,
    formDef,
    formConfig,
    visibilityPredicateExpressions,
    disabledPredicatedExpressions,
    fieldLabels,
  };
};

export const ParseTableForm = <T,>(
  formName: string,
  formDef: ParsedTableFormConfig<T>,
  nestedContainerFormView: any,
  formViews: Record<string, Record<string, any>>,
  forms: ParsedForms<T>,
  cellViewsConfig: any,
  infiniteStreamSources: any,
  enumOptionsSources: EnumOptionsSources,
  defaultValue: BasicFun<ParsedType<T>, any>,
  defaultState: BasicFun<ParsedType<T>, any>,
  injectedPrimitives?: InjectedPrimitives<T>,
  tableApiSources?: TableApiSources,
): Omit<ParsedTableForm<T>, "form"> => {
  const formConfig: any = {};

  let columnHeaders: Map<FieldName, string | undefined> = Map();
  // const getChunk = infiniteStreamSources(formDef.api);
  const initialFormState: any = {
    commonFormState: CommonFormState.Default(),
    customFormState: {
      // streamParams: Debounced.Default(Map()),
      // stream: ValueInfiniteStreamState().Default(10, Map()),
      isInitialized: false,
    },
  };

  const columnNames = Object.keys(cellViewsConfig);

  columnNames.forEach((columnName) => {
    const parsedFormConfig = ParsedRenderer.Operations.RendererToForm(
      columnName,
      {
        formViews,
        forms,
        nestedContainerFormView,
        defaultValue,
        defaultState,
        enumOptionsSources,
        infiniteStreamSources,
        injectedPrimitives,
        tableApiSources,
      },
      formDef.columns.get(columnName)!,
    );
    if (parsedFormConfig.kind == "errors") {
      console.error(parsedFormConfig.errors.toJS());
      throw Error(
        `Error parsing column renderer ${cellViewsConfig[columnName]}`,
      );
    }
    formConfig[columnName] = parsedFormConfig.value.form.renderer;

    columnHeaders = columnHeaders.set(columnName, parsedFormConfig.value.label);
  });

  return {
    initialFormState,
    formConfig,
    formDef,
    formName,
    visibleColumns: formDef.visibleColumns,
    columnHeaders,
  };
};

export const ParseForms =
  <T,>(
    builtIns: BuiltIns,
    injectedPrimitives: InjectedPrimitives<T> | undefined,
    nestedContainerFormView: any,
    fieldViews: any,
    infiniteStreamSources: any,
    enumOptionsSources: EnumOptionsSources,
    tableApiSources?: TableApiSources,
  ) =>
  (formsConfig: ParsedFormJSON<T>): ValueOrErrors<ParsedForms<T>, string> => {
    let errors: FormParsingErrors = List();
    let seen = Set<string>();
    let formProcessingOrder = OrderedSet<string>();

    let parsedForms: ParsedForms<T> = Map();
    const traverse = (formDef: ParsedFormConfig<T>) => {
      if (formProcessingOrder.has(formDef.name)) {
        return;
      }
      if (seen.has(formDef.name)) {
        errors.push(
          `aborting: cycle detected when parsing forms: ${JSON.stringify(
            formProcessingOrder.reverse().toArray(),
          )} -> ${formDef.name}`,
        );
        return;
      }
      seen = seen.add(formDef.name);
      const formFields =
        formDef.kind == "recordForm" ? formDef.fields : formDef.columns;
      formFields.forEach((field, fieldName) => {
        if (
          field.type.kind == "lookup" ||
          field.type.kind == "record" ||
          field.type.kind == "table"
        ) {
          traverse(formsConfig.forms.get(field.renderer)!);
        }
        try {
          if (field.kind == "list") {
            if (typeof field.elementRenderer == "string")
              throw Error(
                "Deprecated element renderer as string, use a render object instead - check parser.",
              );
            if (formsConfig.forms.has(field.elementRenderer.renderer))
              traverse(formsConfig.forms.get(field.elementRenderer.renderer)!);
          }
          if (field.kind == "map") {
            const keyRenderer = field.keyRenderer;
            const valueRenderer = field.valueRenderer;
            if (keyRenderer && formsConfig.forms.has(keyRenderer.renderer)) {
              traverse(formsConfig.forms.get(keyRenderer.renderer)!);
            }
            if (
              valueRenderer &&
              formsConfig.forms.has(valueRenderer.renderer)
            ) {
              traverse(formsConfig.forms.get(valueRenderer.renderer)!);
            }
          }
          if (field.kind == "sum") {
            if (
              field.leftRenderer &&
              formsConfig.forms.has(field.leftRenderer.renderer)
            ) {
              traverse(formsConfig.forms.get(field.leftRenderer.renderer)!);
            }
            if (
              field.rightRenderer &&
              formsConfig.forms.has(field.rightRenderer.renderer)
            ) {
              traverse(formsConfig.forms.get(field.rightRenderer.renderer)!);
            }
          }
          if (field.kind == "tuple") {
            field.itemRenderers.forEach((itemRenderer) => {
              if (formsConfig.forms.has(itemRenderer.renderer)) {
                traverse(formsConfig.forms.get(itemRenderer.renderer)!);
              }
            });
          }
        } catch (error: any) {
          console.error(`error parsing field :${fieldName}:: `, error);
          errors.push(error.message ?? error);
        }
      });
      formProcessingOrder = formProcessingOrder.add(formDef.name);
    };
    const allForms = formsConfig.forms.valueSeq().toArray();
    allForms.forEach((form) => {
      seen = seen.clear();
      traverse(form);
    });

    formProcessingOrder.forEach((formName) => {
      const formConfig = formsConfig.forms.get(formName)!;
      try {
        if (formConfig.kind == "recordForm") {
          const formFieldRenderers = formConfig.fields
            .map((field) => field.renderer)
            .toObject();
          const parsedForm = ParseRecordForm(
            formName,
            formConfig,
            nestedContainerFormView,
            fieldViews,
            parsedForms,
            formFieldRenderers,
            infiniteStreamSources,
            enumOptionsSources,
            defaultValue(formsConfig.types, builtIns, injectedPrimitives),
            defaultState(formsConfig.types, builtIns, injectedPrimitives),
            injectedPrimitives,
            tableApiSources,
          );
          const formBuilder = RecordForm<any, any, any>().Default<any>();
          const form = formBuilder
            .template(
              {
                ...parsedForm.formConfig,
              },
              parsedForm.fieldLabels,
            )
            .mapContext<Unit>((_) => {
              return {
                type: parsedForm.formDef.type,
                visible: (_ as any).visible ?? true,
                disabled: (_ as any).disabled ?? false,
                label: (_ as any).label,
                value: (_ as any).value,
                commonFormState: (_ as any).commonFormState,
                formFieldStates: (_ as any).formFieldStates,
                rootValue: (_ as any).rootValue,
                extraContext: (_ as any).extraContext,
                visibilities: (_ as any).visibilities,
                disabledFields: (_ as any).disabledFields,
                globalConfiguration: (_ as any).globalConfiguration,
                visibilityPredicateExpressions:
                  parsedForm.visibilityPredicateExpressions,
                disabledPredicateExpressions:
                  parsedForm.disabledPredicatedExpressions,
                layout: formConfig.tabs,
                fromApiParserByType: (_ as any).fromApiParserByType,
              };
            });

          parsedForms = parsedForms.set(formName, {
            ...parsedForm,
            form,
          });
        }
      } catch (error: any) {
        console.error(error);
        errors.push(error.message ?? error);
      }
    });

    if (errors.size > 0) {
      return ValueOrErrors.Default.throw(errors);
    }

    return ValueOrErrors.Default.return(parsedForms);
  };
export type EditLauncherContext<T, FormState, ExtraContext> = Omit<
  EditFormContext<T, FormState> &
    EditFormState<T, FormState> & {
      extraContext: ExtraContext;
      containerFormView: any;
      submitButtonWrapper: any;
    },
  "api" | "parser" | "actualForm"
>;

export type CreateLauncherContext<T, FormState, ExtraContext> = Omit<
  CreateFormContext<T, FormState> &
    CreateFormState<T, FormState> & {
      extraContext: ExtraContext;
      containerFormView: any;
      submitButtonWrapper: any;
    },
  "api" | "actualForm"
>;

export type PassthroughLauncherContext<T, FormState, ExtraContext> = Omit<
  PassthroughFormContext<T, FormState> &
    PassthroughFormState<T, FormState> & {
      extraContext: ExtraContext;
      containerFormView: any;
      containerWrapper: any;
    },
  "api" | "actualForm"
>;

export type ParsedLaunchers = {
  create: Map<
    string,
    <T, FormState, ExtraContext>() => {
      form: Template<
        CreateLauncherContext<T, FormState, ExtraContext> &
          CreateFormState<T, FormState>,
        CreateFormState<T, FormState>,
        Unit
      >;
      initialState: CreateFormState<T, FormState>;
    }
  >;
  edit: Map<
    string,
    <T, FormState, ExtraContext>() => {
      form: Template<
        EditLauncherContext<T, FormState, ExtraContext> &
          EditFormState<T, FormState>,
        EditFormState<T, FormState>,
        EditFormForeignMutationsExpected<T, FormState>
      >;
      initialState: EditFormState<T, FormState>;
    }
  >;
  passthrough: Map<
    string,
    <T, FormState, ExtraContext>() => {
      form: Template<
        PassthroughLauncherContext<T, FormState, ExtraContext> &
          PassthroughFormState<T, FormState>,
        PassthroughFormState<T, FormState>,
        Unit
      >;
      initialState: PassthroughFormState<T, FormState>;
      fromApiParser: (value: any) => ValueOrErrors<PredicateValue, string>;
      toApiParser: (
        value: PredicateValue,
        formState: any,
        type: ParsedType<any>,
      ) => ValueOrErrors<any, string>;
      parseGlobalConfiguration: (
        raw: any,
      ) => ValueOrErrors<PredicateValue, string>;
      parsedFormType: ParsedType<any>;
      parsedForms: Map<string, ParsedType<any>>;
    }
  >;
};
export type ParsedForms<T> = Map<
  string,
  ParsedRecordForm<T> | ParsedTableForm<T>
>;
export type FormParsingErrors = List<string>;
export type FormParsingResult = Sum<ParsedLaunchers, FormParsingErrors>;
export type EnumName = string;
export type EnumOptionsSources = BasicFun<
  EnumName,
  BasicFun<Unit, Promise<Array<EnumReference>>>
>;
export type StreamName = string;
export type InfiniteStreamSources = BasicFun<
  StreamName,
  SearchableInfiniteStreamState["customFormState"]["getChunk"]
>;
export type ConfigName = string;
export type GlobalConfigurationSources = BasicFun<ConfigName, Promise<any>>;
export type EntityName = string;
export type EntityApis = {
  create: BasicFun<EntityName, BasicFun<any, Promise<Unit>>>;
  default: BasicFun<EntityName, BasicFun<Unit, Promise<any>>>;
  update: BasicFun<EntityName, (id: Guid, entity: any) => Promise<ApiErrors>>;
  get: BasicFun<EntityName, BasicFun<Guid, Promise<any>>>;
};

export const parseFormsToLaunchers =
  <T extends { [key in keyof T]: { type: any; state: any } }>(
    builtIns: BuiltIns,
    injectedPrimitives: InjectedPrimitives<T> | undefined,
    apiConverters: ApiConverters<T>,
    containerFormView: any,
    nestedContainerFormView: any,
    fieldViews: any,
    infiniteStreamSources: InfiniteStreamSources,
    enumOptionsSources: EnumOptionsSources,
    entityApis: EntityApis,
    tableApiSources?: TableApiSources,
  ) =>
  (formsConfig: ParsedFormJSON<T>): FormParsingResult => {
    let parsedLaunchers: ParsedLaunchers = {
      create: Map(),
      edit: Map(),
      passthrough: Map(),
    };

    const parsedFormsResult = ParseForms(
      builtIns,
      injectedPrimitives,
      nestedContainerFormView,
      fieldViews,
      infiniteStreamSources,
      enumOptionsSources,
      tableApiSources,
    )(formsConfig);

    if (parsedFormsResult.kind == "errors") {
      console.error(parsedFormsResult.errors);
      return Sum.Default.right(parsedFormsResult.errors);
    }

    const parsedForms = parsedFormsResult.value;

    formsConfig.launchers.edit.forEach((launcher, launcherName) => {
      const parsedForm = parsedForms.get(launcher.form)! as ParsedRecordForm<T>;
      const form = parsedForm.form;
      const initialState = parsedForm.initialFormState;
      const formType = parsedForm.formDef.type;
      const globalConfigEntity = formsConfig.apis.entities.get(
        launcher.configApi,
      )!;
      const globalConfigurationType = formsConfig.types.get(
        globalConfigEntity.type,
      )!;
      const visibilityPredicateExpressions =
        parsedForm.visibilityPredicateExpressions;
      const disabledPredicatedExpressions =
        parsedForm.disabledPredicatedExpressions;
      const api = {
        getGlobalConfiguration: () => entityApis.get(launcher.configApi)(""),
        get: (id: string) => entityApis.get(launcher.api)(id),
        update: (id: any, parsed: any) => {
          return parsed.kind == "errors"
            ? Promise.reject(parsed.errors)
            : entityApis.update(launcher.api)(id, parsed.value);
        },
      };
      parsedLaunchers.edit = parsedLaunchers.edit.set(launcherName, <
        T,
        FormState,
        ExtraContext,
        Context extends EditLauncherContext<T, FormState, ExtraContext>,
      >() => ({
        form: EditFormTemplate<T, FormState>()
          .mapContext(
            (parentContext: Context) =>
              ({
                value:
                  parentContext.entity.sync.kind == "loaded"
                    ? parentContext.entity.sync.value
                    : undefined,
                entity: parentContext.entity,
                globalConfiguration: parentContext.globalConfiguration,
                entityId: parentContext.entityId,
                commonFormState: parentContext.commonFormState,
                customFormState: parentContext.customFormState,
                formFieldStates: parentContext.formFieldStates,
                extraContext: parentContext.extraContext,
                visibilityPredicateExpressions,
                disabledPredicatedExpressions,
                types: formsConfig.types,
                formType: formType,
                api: api,
                parseGlobalConfiguration: (raw: any) =>
                  fromAPIRawValue(
                    globalConfigurationType,
                    formsConfig.types,
                    builtIns,
                    apiConverters,
                    injectedPrimitives,
                  )(raw),
                fromApiParser: (value: any) =>
                  fromAPIRawValue(
                    formType,
                    formsConfig.types,
                    builtIns,
                    apiConverters,
                    injectedPrimitives,
                  )(value),
                toApiParser: (value: PredicateValue, formState: any) =>
                  toAPIRawValue(
                    formType,
                    formsConfig.types,
                    builtIns,
                    apiConverters,
                    injectedPrimitives,
                  )(value, formState),
                actualForm: form
                  .withView(containerFormView)
                  .mapContext((_: any) => ({
                    value: _.value,
                    toApiParser: parentContext.toApiParser,
                    fromApiParser: parentContext.fromApiParser,
                    parseGlobalConfiguration:
                      parentContext.parseGlobalConfiguration,
                    formFieldStates: parentContext.formFieldStates,
                    rootValue: _.value,
                    extraContext: parentContext.extraContext,
                    commonFormState: parentContext.commonFormState,
                    predicateEvaluations:
                      parentContext.customFormState.predicateEvaluations,
                    visibilities: _.visibilities,
                    disabledFields: _.disabledFields,
                    globalConfiguration: parentContext.globalConfiguration.sync,
                  })),
              }) as any,
          )
          .withViewFromProps((props) => props.context.submitButtonWrapper)
          .mapForeignMutationsFromProps(
            (props) => props.foreignMutations as any,
          ),
        initialState: EditFormState<T, FormState>().Default(
          initialState.formFieldStates,
          initialState.commonFormState,
          {
            initApiChecker: ApiResponseChecker.Default(true),
            updateApiChecker: ApiResponseChecker.Default(true),
            configApiChecker: ApiResponseChecker.Default(true),
            apiRunner: Debounced.Default(Synchronized.Default(unit)),
            predicateEvaluations: Debounced.Default(
              ValueOrErrors.Default.return({
                visiblityPredicateEvaluations:
                  FormFieldPredicateEvaluation.Default.form(false, Map()),
                disabledPredicateEvaluations:
                  FormFieldPredicateEvaluation.Default.form(false, Map()),
              }),
            ),
          },
        ),
      }));
    });

    formsConfig.launchers.create.forEach((launcher, launcherName) => {
      const parsedForm = parsedForms.get(launcher.form)! as ParsedRecordForm<T>;
      const form = parsedForm.form;
      const initialState = parsedForm.initialFormState;
      const formType = parsedForm.formDef.type;
      const globalConfigEntity = formsConfig.apis.entities.get(
        launcher.configApi,
      )!;
      const globalConfigurationType = formsConfig.types.get(
        globalConfigEntity.type,
      )!;
      const visibilityPredicateExpressions =
        parsedForm.visibilityPredicateExpressions;
      const disabledPredicatedExpressions =
        parsedForm.disabledPredicatedExpressions;
      const api = {
        getGlobalConfiguration: () => entityApis.get(launcher.configApi)(""),
        default: (_: Unit) => entityApis.default(launcher.api)(unit),
        create: (parsed: any) =>
          parsed.kind == "errors"
            ? Promise.reject(parsed.errors)
            : entityApis.create(launcher.api)(parsed.value),
      };
      parsedLaunchers.create = parsedLaunchers.create.set(launcherName, <
        T,
        FormState,
        ExtraContext,
        Context extends CreateLauncherContext<T, FormState, ExtraContext>,
      >() => ({
        form: CreateFormTemplate<T, FormState>()
          .mapContext((parentContext: Context) => {
            return {
              value:
                parentContext.entity.sync.kind == "loaded"
                  ? parentContext.entity.sync.value
                  : undefined,
              entity: parentContext.entity,
              globalConfiguration: parentContext.globalConfiguration,
              entityId: parentContext.entityId,
              commonFormState: parentContext.commonFormState,
              customFormState: parentContext.customFormState,
              formFieldStates: parentContext.formFieldStates,
              extraContext: parentContext.extraContext,
              visibilityPredicateExpressions,
              disabledPredicatedExpressions,
              types: formsConfig.types,
              formType: formType,
              api: api,
              parseGlobalConfiguration: (raw: any) =>
                fromAPIRawValue(
                  globalConfigurationType,
                  formsConfig.types,
                  builtIns,
                  apiConverters,
                  injectedPrimitives,
                )(raw),
              fromApiParser: (value: any) =>
                fromAPIRawValue(
                  formType,
                  formsConfig.types,
                  builtIns,
                  apiConverters,
                  injectedPrimitives,
                )(value),
              toApiParser: (value: PredicateValue, formState: any) =>
                toAPIRawValue(
                  formType,
                  formsConfig.types,
                  builtIns,
                  apiConverters,
                  injectedPrimitives,
                )(value, formState),
              actualForm: form
                .withView(containerFormView)
                .mapContext((_: any) => {
                  return {
                    value: _.value,
                    toApiParser: parentContext.toApiParser,
                    fromApiParser: parentContext.fromApiParser,
                    parseGlobalConfiguration:
                      parentContext.parseGlobalConfiguration,
                    formFieldStates: parentContext.formFieldStates,
                    rootValue: _.value,
                    extraContext: parentContext.extraContext,
                    commonFormState: parentContext.commonFormState,
                    predicateEvaluations:
                      parentContext.customFormState.predicateEvaluations,
                    visibilities: _.visibilities,
                    disabledFields: _.disabledFields,
                    globalConfiguration: parentContext.globalConfiguration.sync,
                  };
                }),
            } as any;
          })
          .withViewFromProps((props) => props.context.submitButtonWrapper)
          .mapForeignMutationsFromProps(
            (props) => props.foreignMutations as any,
          ),
        initialState: CreateFormState<T, FormState>().Default(
          initialState.formFieldStates,
          initialState.commonFormState,
          {
            initApiChecker: ApiResponseChecker.Default(true),
            createApiChecker: ApiResponseChecker.Default(true),
            configApiChecker: ApiResponseChecker.Default(true),
            apiRunner: Debounced.Default(Synchronized.Default(unit)),
            predicateEvaluations: Debounced.Default(
              ValueOrErrors.Default.return({
                visiblityPredicateEvaluations:
                  FormFieldPredicateEvaluation.Default.form(false, Map()),
                disabledPredicateEvaluations:
                  FormFieldPredicateEvaluation.Default.form(false, Map()),
              }),
            ),
          },
        ),
      }));
    });

    formsConfig.launchers.passthrough.forEach((launcher, launcherName) => {
      const parsedForm = parsedForms.get(launcher.form)! as ParsedRecordForm<T>;
      const form = parsedForm.form;
      const globalConfigurationType = formsConfig.types.get(
        launcher.configType,
      )!;
      const initialState = parsedForm.initialFormState;
      const formType = parsedForm.formDef.type;
      const visibilityPredicateExpressions =
        parsedForm.visibilityPredicateExpressions;
      const disabledPredicatedExpressions =
        parsedForm.disabledPredicatedExpressions;
      parsedLaunchers.passthrough = parsedLaunchers.passthrough.set(
        launcherName,
        <
          T,
          FormState,
          ExtraContext,
          Context extends PassthroughLauncherContext<
            T,
            FormState,
            ExtraContext
          >,
        >() => ({
          form: PassthroughFormTemplate<T, FormState>()
            .mapContext(
              (parentContext: Context) =>
                ({
                  entity: parentContext.entity,
                  globalConfiguration: parentContext.globalConfiguration,
                  commonFormState: parentContext.commonFormState,
                  customFormState: parentContext.customFormState,
                  formFieldStates: parentContext.formFieldStates,
                  extraContext: parentContext.extraContext,
                  visibilityPredicateExpressions,
                  disabledPredicatedExpressions,
                  types: formsConfig.types,
                  formType: formType,
                  onEntityChange: parentContext.onEntityChange,
                  actualForm: form
                    .withView(containerFormView)
                    .mapContext((_: any) => ({
                      type: _.type,
                      value: _.value,
                      entity: _.entity,
                      formFieldStates: parentContext.formFieldStates,
                      rootValue: _.value,
                      extraContext: parentContext.extraContext,
                      commonFormState: parentContext.commonFormState,
                      predicateEvaluations:
                        parentContext.customFormState.predicateEvaluations,
                      visibilities: _.visibilities,
                      disabledFields: _.disabledFields,
                      globalConfiguration: parentContext.globalConfiguration,
                      fromApiParserByType:
                        (type: ParsedType<any>) =>
                        (value: any): ValueOrErrors<PredicateValue, string> =>
                          fromAPIRawValue(
                            type,
                            formsConfig.types,
                            builtIns,
                            apiConverters,
                            injectedPrimitives,
                          )(value),
                    })),
                }) as any,
            )
            .withViewFromProps((props) => props.context.containerWrapper)
            .mapForeignMutationsFromProps(
              (props) => props.foreignMutations as any,
            ),
          initialState: PassthroughFormState<T, FormState>().Default(
            initialState.formFieldStates,
            initialState.commonFormState,
          ),
          parsedFormType: formType,
          parsedForms: formsConfig.types,
          fromApiParser: (value: any): ValueOrErrors<PredicateValue, string> =>
            fromAPIRawValue(
              formType,
              formsConfig.types,
              builtIns,
              apiConverters,
              injectedPrimitives,
            )(value),
          toApiParser: (
            value: PredicateValue,
            formState: any,
            type: ParsedType<any>,
          ) =>
            toAPIRawValue(
              type,
              formsConfig.types,
              builtIns,
              apiConverters,
              injectedPrimitives,
            )(value, formState),
          parseGlobalConfiguration: (raw: any) =>
            fromAPIRawValue(
              globalConfigurationType,
              formsConfig.types,
              builtIns,
              apiConverters,
              injectedPrimitives,
            )(raw),
        }),
      );
    });

    return Sum.Default.left(parsedLaunchers);
  };

export type FormsParserContext<
  T extends { [key in keyof T]: { type: any; state: any } },
> = {
  containerFormView: any;
  nestedContainerFormView: any;
  fieldViews: any;
  fieldTypeConverters: ApiConverters<T>;
  infiniteStreamSources: InfiniteStreamSources;
  enumOptionsSources: EnumOptionsSources;
  entityApis: EntityApis;
  getFormsConfig: BasicFun<void, Promise<any>>;
  injectedPrimitives?: Injectables<T>;
  tableApiSources?: TableApiSources;
};
export type FormsParserState = {
  formsConfig: Synchronized<Unit, FormParsingResult>;
};
export const FormsParserState = {
  Default: (): FormsParserState => ({
    formsConfig: Synchronized.Default(unit),
  }),
  Updaters: {
    ...simpleUpdater<FormsParserState>()("formsConfig"),
  },
};
