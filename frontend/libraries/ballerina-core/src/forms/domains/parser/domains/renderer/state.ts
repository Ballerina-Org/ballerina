import { List, Map, OrderedMap, Set } from "immutable";
import {
  Base64FileForm,
  BaseEnumContext,
  BasicFun,
  BooleanForm,
  BoolExpr,
  CollectionReference,
  CollectionSelection,
  CommonFormState,
  DateForm,
  DateFormState,
  EnumForm,
  EnumFormState,
  EnumMultiselectForm,
  EnumOptionsSources,
  EnumReference,
  Expr,
  FieldPredicateExpression,
  FormLabel,
  Guid,
  InfiniteMultiselectDropdownForm,
  InjectedPrimitives,
  ListFieldState,
  ListForm,
  MapFieldState,
  MapForm,
  Maybe,
  NumberForm,
  ParsedApplicationType,
  ParsedForms,
  ParsedType,
  PredicateValue,
  SearchableInfiniteStreamForm,
  SearchableInfiniteStreamState,
  SecretForm,
  StringForm,
  Sum,
  SumFormState,
  SumForm,
  Template,
  TupleFormState,
  TupleForm,
  unit,
  Unit,
  UnitForm,
  Value,
  ValueOption,
  ValueRecord,
} from "../../../../../../main";
import { ValueOrErrors } from "../../../../../collections/domains/valueOrErrors/state";

type Form = {
  renderer: Template<any, any, any, any>;
  initialValue: any;
  initialState: any;
};
export type RawRenderer = {
  renderer?: any;
  label?: any;
  tooltip?: any;
  visible?: any;
  disabled?: any;
  stream?: any;
  options?: any;
  elementRenderer?: any;
  keyRenderer?: any;
  valueRenderer?: any;
  itemRenderers?: Array<any>;
  leftRenderer?: any;
  rightRenderer?: any;
  details?: any;
};

export type ParsedRecordFormRenderer<T> = {
  kind: "recordForm";
  type: ParsedType<T>;
  fields: Map<string, ParsedRecordFieldRenderer<T>>;
};

export type ParsedRecordFieldRenderer<T> = {
  kind: "recordField";
  type: ParsedType<T>;
  renderer: ParsedRenderer<T>;
};

export type ParsedUnionFormRenderer<T> = {
  kind: "unionForm";
  type: ParsedType<T>;
  cases: Map<string, ParsedUnionCaseRenderer<T>>;
};

export type ParsedUnionCaseRenderer<T> = {
  kind: "unionCase";
  name: string;
  fields: ParsedRenderer<T>;
};

export type ParsedRenderer<T> = (
  | { kind: "primitive" }
  | { kind: "unit" }
  | { kind: "lookup"; name: string }
  | { kind: "enum"; options: string }
  | { kind: "stream"; stream: string }
  | { kind: "list"; elementRenderer: ParsedRenderer<T> }
  | {
      kind: "map";
      keyRenderer: ParsedRenderer<T>;
      valueRenderer: ParsedRenderer<T>;
    }
  | {
      kind: "tuple";
      itemRenderers: Array<ParsedRenderer<T>>;
    }
  | {
      kind: "sum";
      leftRenderer?: ParsedRenderer<T>;
      rightRenderer?: ParsedRenderer<T>;
    }
  | {
      kind: "union";
      cases: Map<string, ParsedRenderer<T>>;
    }
  | {
      kind: "unionCase";
      name: string;
      // What is the type of the fields?
      // fields: ParsedRenderer<T>; 
    }
) & {
  renderer: string;
  type: ParsedType<T>;
  label?: string;
  tooltip?: string;
  visible?: BoolExpr<any>;
  disabled?: BoolExpr<any>;
  details?: any;
};
export const ParsedRenderer = {
  Default: {
    primitive: <T>(
      type: ParsedType<T>,
      renderer: string,
      visible: any,
      disabled: any,
      label?: string,
      tooltip?: string,
      details?: string,
    ): ParsedRenderer<T> => ({
      kind: "primitive",
      type,
      renderer,
      label,
      tooltip,
      details,
      visible,
      disabled: disabled != undefined ? disabled : false,
    }),
    record: <T>(
      type: ParsedType<T>,
      renderer: string,
      visible: any,
      disabled: any,
      label?: string,
      tooltip?: string,
      details?: string,
    ): ParsedRenderer<T> => ({
      kind: "record",
      type,
      renderer,
      label,
      tooltip,
      details,
      visible,
      disabled: disabled != undefined ? disabled : false,
    }),
    enum: <T>(
      type: ParsedType<T>,
      renderer: string,
      visible: any,
      disabled: any,
      options: string,
      label?: string,
      tooltip?: string,
      details?: string,
    ): ParsedRenderer<T> => ({
      kind: "enum",
      type,
      renderer,
      label,
      tooltip,
      details,
      visible,
      disabled: disabled != undefined ? disabled : false,
      options,
    }),
    stream: <T>(
      type: ParsedType<T>,
      renderer: string,
      visible: any,
      disabled: any,
      stream: string,
      label?: string,
      tooltip?: string,
      details?: string,
    ): ParsedRenderer<T> => ({
      kind: "stream",
      type,
      renderer,
      label,
      tooltip,
      details,
      visible,
      disabled: disabled != undefined ? disabled : false,
      stream,
    }),
    list: <T>(
      type: ParsedType<T>,
      renderer: string,
      visible: any,
      disabled: any,
      elementRenderer: ParsedRenderer<T>,
      label?: string,
      tooltip?: string,
      details?: string,
    ): ParsedRenderer<T> => ({
      kind: "list",
      type,
      renderer,
      label,
      tooltip,
      details,
      visible,
      disabled: disabled != undefined ? disabled : false,
      elementRenderer,
    }),
    map: <T>(
      type: ParsedType<T>,
      renderer: string,
      visible: any,
      disabled: any,
      keyRenderer: ParsedRenderer<T>,
      valueRenderer: ParsedRenderer<T>,
      label?: string,
      tooltip?: string,
      details?: string,
    ): ParsedRenderer<T> => ({
      kind: "map",
      type,
      renderer,
      label,
      tooltip,
      details,
      visible,
      disabled: disabled != undefined ? disabled : false,
      keyRenderer,
      valueRenderer,
    }),
    tuple: <T>(
      type: ParsedType<T>,
      renderer: string,
      visible: any,
      disabled: any,
      itemRenderers: Array<ParsedRenderer<T>>,
      label?: string,
      tooltip?: string,
      details?: string,
    ): ParsedRenderer<T> => ({
      kind: "tuple",
      type,
      renderer,
      label,
      tooltip,
      details,
      visible,
      disabled: disabled != undefined ? disabled : false,
      itemRenderers,
    }),
    sum: <T>(
      type: ParsedType<T>,
      renderer: string,
      visible: any,
      disabled: any,
      leftRenderer?: ParsedRenderer<T>,
      rightRenderer?: ParsedRenderer<T>,
      label?: string,
      tooltip?: string,
      details?: string,
    ): ParsedRenderer<T> => ({
      kind: "sum",
      type,
      renderer,
      label,
      tooltip,
      details,
      visible,
      disabled: disabled != undefined ? disabled : false,
      leftRenderer,
      rightRenderer,
    }),
    unit: <T>(
      type: ParsedType<T>,
      renderer: string,
      visible: any,
      disabled: any,
      label?: string,
      tooltip?: string,
      details?: string,
    ): ParsedRenderer<T> => ({
      kind: "unit",
      type,
      renderer,
      label,
      tooltip,
      details,
      visible,
      disabled: disabled != undefined ? disabled : false,
    }),
  },
  Operations: {
    ParseRenderer: <T>(
      fieldType: ParsedType<T>,
      field: RawRenderer,
      types: Map<string, ParsedType<T>>,
    ): ParsedRenderer<T> => {
      if (fieldType.kind == "primitive")
        return ParsedRenderer.Default.primitive(
          fieldType,
          field.renderer,
          field.visible,
          field.disabled,
          field.label,
          field.tooltip,
          field.details,
        );
      if (fieldType.kind == "record")
        return ParsedRenderer.Default.record(
          fieldType,
          field.renderer,
          field.visible,
          field.disabled,
          field.label,
          field.tooltip,
          field.details,
        );
      if (fieldType.kind == "application" && "options" in field)
        return ParsedRenderer.Default.enum(
          fieldType,
          field.renderer,
          field.visible,
          field.disabled,
          field.options,
          field.label,
          field.tooltip,
          field.details,
        );
      if (fieldType.kind == "application" && "stream" in field)
        return ParsedRenderer.Default.stream(
          fieldType,
          field.renderer,
          field.visible,
          field.disabled,
          field.stream,
          field.label,
          field.tooltip,
          field.details,
        );
      if (fieldType.kind == "application" && fieldType.value == "List")
        return ParsedRenderer.Default.list(
          fieldType,
          field.renderer,
          field.visible,
          field.disabled,
          ParsedRenderer.Operations.ParseRenderer(
            fieldType.args[0],
            field.elementRenderer,
            types,
          ),
          field.label,
          field.tooltip,
          field.details,
        );
      if (fieldType.kind == "application" && fieldType.value == "Map")
        return ParsedRenderer.Default.map(
          fieldType,
          field.renderer,
          field.visible,
          field.disabled,
          ParsedRenderer.Operations.ParseRenderer(
            fieldType.args[0],
            field.keyRenderer,
            types,
          ),
          ParsedRenderer.Operations.ParseRenderer(
            fieldType.args[1],
            field.valueRenderer,
            types,
          ),
          field.label,
          field.tooltip,
          field.details,
        );

      if (fieldType.kind == "application" && fieldType.value == "Tuple")
        return ParsedRenderer.Default.tuple(
          fieldType,
          field.renderer,
          field.visible,
          field.disabled,
          field.itemRenderers?.map((item, i) =>
            ParsedRenderer.Operations.ParseRenderer(
              fieldType.args[i],
              item,
              types,
            ),
          ) ?? [],
          field.label,
          field.tooltip,
          field.details,
        );

      if (fieldType.kind == "application" && fieldType.value == "Sum")
        return ParsedRenderer.Default.sum(
          fieldType,
          field.renderer,
          field.visible,
          field.disabled,
          field.leftRenderer != undefined
            ? ParsedRenderer.Operations.ParseRenderer(
                fieldType.args[0],
                field.leftRenderer,
                types,
              )
            : undefined,
          field.rightRenderer != undefined
            ? ParsedRenderer.Operations.ParseRenderer(
                fieldType.args[1],
                field.rightRenderer,
                types,
              )
            : undefined,
          field.label,
          field.tooltip,
          field.details,
        );
      if (fieldType.kind == "lookup") {
        return ParsedRenderer.Operations.ParseRenderer(
          types.get(fieldType.name)!,
          field,
          types,
        );
      }
      console.error(
        `Invalid field type ${JSON.stringify(
          fieldType,
        )} for field ${JSON.stringify(field)}`,
      );
      throw new Error("Invalid field type");
    },
    FormViewToViewKind: (
      viewName: string | undefined,
      formViews: Record<string, any>,
      formNames: Set<string>,
    ) => {
      if (viewName == undefined) {
        throw Error(`cannot resolve view ${viewName}`); // TODO -- better error handling
      }
      if (formNames.has(viewName)) {
        return "form";
      }
      const viewTypes = Object.keys(formViews);
      for (const viewType of viewTypes) {
        if (viewName in formViews[viewType]) {
          return viewType;
        }
      }
      throw Error(`cannot resolve view ${viewName}`); // TODO -- better error handling
    },
    RendererToForm: <T>(
      fieldName: string,
      parsingContext: {
        formViews: Record<string, Record<string, any>>;
        forms: ParsedForms<T>;
        nestedContainerFormView: any;
        defaultValue: BasicFun<ParsedType<T>, any>;
        defaultState: BasicFun<ParsedType<T>, any>;
        enumOptionsSources: EnumOptionsSources;
        infiniteStreamSources: any;
        injectedPrimitives?: InjectedPrimitives<T>;
      },
      parsedRenderer: ParsedRenderer<T>,
    ): ValueOrErrors<
      {
        form: Form;
        visibilityPredicateExpression: FieldPredicateExpression;
        disabledPredicatedExpression: FieldPredicateExpression;
      },
      string
    > => {
      const viewKind = ParsedRenderer.Operations.FormViewToViewKind(
        parsedRenderer.renderer,
        parsingContext.formViews,
        parsingContext.forms.keySeq().toSet(),
      );
      switch (parsedRenderer.kind) {
        case "primitive":
        case "enum":
        case "stream":
          return Expr.Operations.parse(parsedRenderer.visible ?? true).Then(
            (visibilityExpr) => {
              return Expr.Operations.parse(
                parsedRenderer.disabled ?? false,
              ).Then((disabledExpr) =>
                ValueOrErrors.Default.return({
                  form: {
                    renderer: ParsedRenderer.Operations.FormRenderers(
                      parsedRenderer,
                      parsingContext.formViews,
                      viewKind,
                      parsedRenderer.renderer,
                      parsedRenderer.label,
                      parsedRenderer.tooltip,
                      parsedRenderer.details,
                      parsingContext.enumOptionsSources,
                      parsingContext.injectedPrimitives,
                    ),
                    initialValue: parsingContext.defaultValue(
                      parsedRenderer.type,
                    ),
                    initialState: ParsedRenderer.Operations.FormStates(
                      viewKind,
                      parsedRenderer.renderer,
                      parsingContext.infiniteStreamSources,
                      parsingContext.injectedPrimitives,
                      parsedRenderer.kind == "stream"
                        ? parsedRenderer.stream
                        : undefined,
                    ),
                  },
                  visibilityPredicateExpression:
                    FieldPredicateExpression.Default.primitive(visibilityExpr),
                  disabledPredicatedExpression:
                    FieldPredicateExpression.Default.primitive(disabledExpr),
                }),
              );
            },
          );
        case "unit":
          return Expr.Operations.parse(parsedRenderer.visible ?? true).Then(
            (visibilityExpr) =>
              Expr.Operations.parse(parsedRenderer.disabled ?? false).Then(
                (disabledExpr) =>
                  ValueOrErrors.Default.return({
                    form: {
                      renderer: UnitForm<any & FormLabel>()
                        .withView(
                          parsingContext.formViews[viewKind][
                            parsedRenderer.renderer
                          ]() as any,
                        )
                        .mapContext<any>((_) => ({
                          ..._,
                          label: parsedRenderer.label,
                          tooltip: parsedRenderer.tooltip,
                          details: parsedRenderer.details,
                        })),
                      initialValue: parsingContext.defaultValue(
                        parsedRenderer.type,
                      ),
                      initialState: {
                        commonFormState: CommonFormState.Default(),
                      },
                    },
                    visibilityPredicateExpression:
                      FieldPredicateExpression.Default.unit(visibilityExpr),
                    disabledPredicatedExpression:
                      FieldPredicateExpression.Default.unit(disabledExpr),
                  }),
              ),
          );
        case "record":
          return Expr.Operations.parse(parsedRenderer.visible ?? true).Then(
            (visibilityExpr) =>
              Expr.Operations.parse(parsedRenderer.disabled ?? false).Then(
                (disabledExpr) =>
                  ValueOrErrors.Default.return({
                    form: {
                      renderer: parsingContext.forms
                        .get(parsedRenderer.renderer)!
                        .form.withView(parsingContext.nestedContainerFormView)
                        .mapContext<any>((_) => ({
                          ..._,
                          label: parsedRenderer.label,
                          tooltip: parsedRenderer.tooltip,
                          details: parsedRenderer.details,
                        })),
                      initialValue: parsingContext.defaultValue(
                        parsedRenderer.type,
                      ),
                      initialState: parsingContext.forms.get(
                        parsedRenderer.renderer,
                      )!.initialFormState,
                    },
                    visibilityPredicateExpression:
                      FieldPredicateExpression.Default.record(
                        visibilityExpr,
                        parsingContext.forms.get(parsedRenderer.renderer)!
                          .visibilityPredicateExpressions,
                      ),
                    disabledPredicatedExpression:
                      FieldPredicateExpression.Default.record(
                        disabledExpr,
                        parsingContext.forms.get(parsedRenderer.renderer)!
                          .disabledPredicatedExpressions,
                      ),
                  }),
              ),
          );
        case "list":
          return Expr.Operations.parse(parsedRenderer.visible ?? true).Then(
            (visibilityExpr) =>
              Expr.Operations.parse(parsedRenderer.disabled ?? false).Then(
                (disabledExpr) =>
                  ParsedRenderer.Operations.RendererToForm(
                    fieldName,
                    parsingContext,
                    parsedRenderer.elementRenderer,
                  ).Then((parsedElementRenderer) =>
                    ValueOrErrors.Default.return({
                      form: {
                        renderer: ListForm<any, any & FormLabel, Unit>(
                          {
                            Default: () =>
                              parsedElementRenderer.form.initialState,
                          },
                          {
                            Default: () =>
                              parsedElementRenderer.form.initialValue,
                          },
                          parsedElementRenderer.form.renderer,
                        )
                          .withView(
                            parsingContext.formViews[viewKind][
                              parsedRenderer.renderer
                            ]() as any,
                          )
                          .mapContext<any>((_) => {
                            return {
                              ..._,
                              label: parsedRenderer.label,
                              tooltip: parsedRenderer.tooltip,
                              details: parsedRenderer.details,
                            };
                          }),
                        initialValue: parsingContext.defaultValue(
                          parsedRenderer.type,
                        ),
                        initialState: ListFieldState<any>().Default(Map()),
                      },
                      visibilityPredicateExpression:
                        FieldPredicateExpression.Default.list(
                          visibilityExpr,
                          parsedElementRenderer.visibilityPredicateExpression,
                        ),
                      disabledPredicatedExpression:
                        FieldPredicateExpression.Default.list(
                          disabledExpr,
                          parsedElementRenderer.disabledPredicatedExpression,
                        ),
                    }),
                  ),
              ),
          );
        case "map":
          return Expr.Operations.parse(parsedRenderer.visible ?? true).Then(
            (visibilityExpr) =>
              Expr.Operations.parse(parsedRenderer.disabled ?? false).Then(
                (disabledExpr) =>
                  ParsedRenderer.Operations.RendererToForm(
                    fieldName,
                    parsingContext,
                    parsedRenderer.keyRenderer,
                  ).Then((parsedKeyRenderer) =>
                    ParsedRenderer.Operations.RendererToForm(
                      fieldName,
                      parsingContext,
                      parsedRenderer.valueRenderer,
                    ).Then((parsedValueRenderer) =>
                      ValueOrErrors.Default.return({
                        form: {
                          renderer: MapForm<any, any, any & FormLabel, Unit>(
                            {
                              Default: () =>
                                parsedKeyRenderer.form.initialState,
                            },
                            {
                              Default: () =>
                                parsedValueRenderer.form.initialState,
                            },
                            {
                              Default: () =>
                                parsedKeyRenderer.form.initialValue,
                            },
                            {
                              Default: () =>
                                parsedValueRenderer.form.initialValue,
                            },
                            parsedKeyRenderer.form.renderer,
                            parsedValueRenderer.form.renderer,
                          )
                            .withView(
                              parsingContext.formViews[viewKind][
                                parsedRenderer.renderer
                              ]() as any,
                            )
                            .mapContext<any>((_) => ({
                              ..._,
                              label: parsedRenderer.label,
                              tooltip: parsedRenderer.tooltip,
                              details: parsedRenderer.details,
                            })),
                          initialValue: parsingContext.defaultValue(
                            parsedRenderer.type,
                          ),
                          initialState: MapFieldState<any, any>().Default(
                            Map(),
                          ),
                        },
                        visibilityPredicateExpression:
                          FieldPredicateExpression.Default.map(
                            visibilityExpr,
                            parsedKeyRenderer.visibilityPredicateExpression,
                            parsedValueRenderer.visibilityPredicateExpression,
                          ),
                        disabledPredicatedExpression:
                          FieldPredicateExpression.Default.map(
                            disabledExpr,
                            parsedKeyRenderer.disabledPredicatedExpression,
                            parsedValueRenderer.disabledPredicatedExpression,
                          ),
                      }),
                    ),
                  ),
              ),
          );
        case "tuple":
          return Expr.Operations.parse(parsedRenderer.visible ?? true).Then(
            (visibilityExpr) =>
              Expr.Operations.parse(parsedRenderer.disabled ?? false).Then(
                (disabledExpr) => {
                  return ValueOrErrors.Operations.All(
                    List(
                      parsedRenderer.itemRenderers.map((item) =>
                        ParsedRenderer.Operations.RendererToForm(
                          fieldName,
                          parsingContext,
                          item,
                        ),
                      ),
                    ),
                  ).Then((itemRenderers) =>
                    ValueOrErrors.Default.return({
                      form: {
                        renderer: TupleForm<any, any & FormLabel, Unit>(
                          itemRenderers.map((item) => item.form.initialState),
                          itemRenderers.map((item) => item.form.renderer),
                        )
                          .withView(
                            parsingContext.formViews[viewKind][
                              parsedRenderer.renderer
                            ]() as any,
                          )
                          .mapContext<any>((_) => ({
                            ..._,
                            label: parsedRenderer.label,
                            tooltip: parsedRenderer.tooltip,
                            details: parsedRenderer.details,
                          })),
                        initialValue: parsingContext.defaultValue(
                          parsedRenderer.type,
                        ),
                        initialState: TupleFormState<any>().Default(
                          itemRenderers.map((item) => item.form.initialState),
                        ),
                      },
                      visibilityPredicateExpression:
                        FieldPredicateExpression.Default.tuple(
                          visibilityExpr,
                          itemRenderers
                            .map((item) => item.visibilityPredicateExpression)
                            .toArray(),
                        ),
                      disabledPredicatedExpression:
                        FieldPredicateExpression.Default.tuple(
                          disabledExpr,
                          itemRenderers
                            .map((item) => item.disabledPredicatedExpression)
                            .toArray(),
                        ),
                    }),
                  );
                },
              ),
          );
        case "sum":
          return Expr.Operations.parse(parsedRenderer.visible ?? true).Then(
            (visibilityExpr) =>
              Expr.Operations.parse(parsedRenderer.disabled ?? false).Then(
                (disabledExpr) => {
                  // Sums may be generic, they may not have a left or right renderer when
                  // control to switch between left and right is needed by the parent.
                  const parsedLeftRenderer =
                    parsedRenderer.leftRenderer != undefined
                      ? ParsedRenderer.Operations.RendererToForm(
                          fieldName,
                          parsingContext,
                          parsedRenderer.leftRenderer,
                        )
                      : undefined;
                  const parsedRightRenderer =
                    parsedRenderer.rightRenderer != undefined
                      ? ParsedRenderer.Operations.RendererToForm(
                          fieldName,
                          parsingContext,
                          parsedRenderer.rightRenderer,
                        )
                      : undefined;

                  if (
                    parsedLeftRenderer != undefined &&
                    parsedLeftRenderer.kind == "errors" &&
                    parsedRightRenderer != undefined &&
                    parsedRightRenderer.kind == "errors"
                  ) {
                    return ValueOrErrors.Default.throw(
                      parsedLeftRenderer.errors.concat(
                        parsedRightRenderer.errors,
                      ),
                    );
                  }
                  if (
                    parsedLeftRenderer != undefined &&
                    parsedLeftRenderer.kind == "errors"
                  ) {
                    return parsedLeftRenderer;
                  }
                  if (
                    parsedRightRenderer != undefined &&
                    parsedRightRenderer.kind == "errors"
                  ) {
                    return parsedRightRenderer;
                  }

                  const leftFormState =
                    parsedLeftRenderer != undefined
                      ? parsedLeftRenderer.value.form.initialState
                      : parsingContext.defaultState(
                          (parsedRenderer.type as ParsedApplicationType<T>)
                            .args[0],
                        );
                  const rightFormState =
                    parsedRightRenderer != undefined
                      ? parsedRightRenderer.value.form.initialState
                      : parsingContext.defaultState(
                          (parsedRenderer.type as ParsedApplicationType<T>)
                            .args[1],
                        );

                  return ValueOrErrors.Default.return({
                    form: {
                      renderer: SumForm<any, any, any & FormLabel, Unit>(
                        leftFormState,
                        rightFormState,
                        parsedLeftRenderer?.value.form.renderer,
                        parsedRightRenderer?.value.form.renderer,
                      )
                        .withView(
                          parsingContext.formViews[viewKind][
                            parsedRenderer.renderer
                          ]() as any,
                        )
                        .mapContext<any>((_) => ({
                          ..._,
                          label: parsedRenderer.label,
                          tooltip: parsedRenderer.tooltip,
                          details: parsedRenderer.details,
                        })),
                      initialValue: parsingContext.defaultValue(
                        parsedRenderer.type,
                      ),
                      initialState: SumFormState<any, any>().Default({
                        left:
                          parsedLeftRenderer?.value.form.initialState ??
                          leftFormState,
                        right:
                          parsedRightRenderer?.value.form.initialState ??
                          rightFormState,
                      }),
                    },
                    visibilityPredicateExpression:
                      FieldPredicateExpression.Default.sum(
                        visibilityExpr,
                        parsedLeftRenderer?.value.visibilityPredicateExpression,
                        parsedRightRenderer?.value
                          .visibilityPredicateExpression,
                      ),
                    disabledPredicatedExpression:
                      FieldPredicateExpression.Default.sum(
                        disabledExpr,
                        parsedLeftRenderer?.value.disabledPredicatedExpression,
                        parsedRightRenderer?.value.disabledPredicatedExpression,
                      ),
                  });
                },
              ),
          );
        default:
          return ValueOrErrors.Default.throw(
            List([
              `error: the kind for ${viewKind}::${parsedRenderer} cannot be found`,
            ]),
          );
      }
    },
    FormRenderers: <T>(
      rendererConfig: ParsedRenderer<T>,
      formViews: Record<string, Record<string, any>>,
      viewKind: string,
      viewName: any,
      label: string | undefined,
      tooltip: string | undefined,
      details: string | undefined,
      enumOptionsSources: EnumOptionsSources,
      injectedPrimitives?: InjectedPrimitives<T>,
    ): any => {
      if (viewKind == "unit") {
        return UnitForm<any & FormLabel>()
          .withView(formViews[viewKind][viewName]())
          .mapContext<any & CommonFormState & Value<Unit>>((_) => ({
            ..._,
            label,
            tooltip,
            details,
          }));
      }
      if (viewKind == "boolean")
        return BooleanForm<any & FormLabel, Unit>()
          .withView(formViews[viewKind][viewName]())
          .mapContext<any & CommonFormState & Value<boolean>>((_) => ({
            ..._,
            label,
            tooltip,
            details,
          }));
      if (viewKind == "date")
        return DateForm<any & FormLabel, Unit>()
          .withView(formViews[viewKind][viewName]())
          .mapContext<any & DateFormState & Value<Maybe<Date>>>((_) => ({
            ..._,
            label,
            tooltip,
            details,
          }));
      if (viewKind == "number")
        return NumberForm<any & FormLabel, Unit>()
          .withView(formViews[viewKind][viewName]())
          .mapContext<any & CommonFormState & Value<number>>((_) => ({
            ..._,
            label,
            tooltip,
            details,
          }));
      if (viewKind == "string")
        return StringForm<any & FormLabel, Unit>()
          .withView(formViews[viewKind][viewName]())
          .mapContext<any & CommonFormState & Value<string>>((_) => ({
            ..._,
            label,
            tooltip,
            details,
          }));
      if (viewKind == "enumSingleSelection" && rendererConfig.kind == "enum")
        return EnumForm<any & FormLabel & BaseEnumContext, Unit>()
          .withView(formViews[viewKind][viewName]())
          .mapContext<any & EnumFormState & ValueOption>((_) => {
            return {
              ..._,
              label,
              tooltip,
              details,
              getOptions: (): Promise<OrderedMap<Guid, ValueRecord>> => {
                return enumOptionsSources(rendererConfig.options)(unit).then(
                  (options) =>
                    OrderedMap(
                      options.map((o: EnumReference) => [
                        o.Value,
                        PredicateValue.Default.record(Map(o)),
                      ]),
                    ),
                );
              },
            };
          });
      if (viewKind == "enumMultiSelection" && rendererConfig.kind == "enum")
        return EnumMultiselectForm<any & FormLabel & BaseEnumContext, Unit>()
          .withView(formViews[viewKind][viewName]())
          .mapContext<EnumFormState & Value<OrderedMap<Guid, ValueRecord>>>(
            (_) => ({
              ..._,
              label,
              details,
              tooltip,
              getOptions: (): Promise<OrderedMap<Guid, ValueRecord>> => {
                return enumOptionsSources(rendererConfig.options)(unit).then(
                  (options) =>
                    OrderedMap(
                      options.map((o: EnumReference) => [
                        o.Value,
                        PredicateValue.Default.record(Map(o)),
                      ]),
                    ),
                );
              },
            }),
          );
      if (viewKind == "streamSingleSelection")
        return SearchableInfiniteStreamForm<any & FormLabel, Unit>()
          .withView(formViews[viewKind][viewName]())
          .mapContext<
            any &
              SearchableInfiniteStreamState &
              Value<CollectionSelection<CollectionReference>>
          >((_) => ({ ..._, label, tooltip, details }));
      if (viewKind == "streamMultiSelection")
        return InfiniteMultiselectDropdownForm<any & FormLabel, Unit>()
          .withView(formViews[viewKind][viewName]())
          .mapContext<
            any &
              FormLabel &
              CommonFormState &
              SearchableInfiniteStreamState &
              Value<OrderedMap<Guid, CollectionReference>>
          >((_) => ({
            ..._,
            label,
            tooltip,
            details,
          }));
      if (viewKind == "base64File")
        return Base64FileForm<any & FormLabel, Unit>()
          .withView(formViews[viewKind][viewName]())
          .mapContext<any & FormLabel & CommonFormState & Value<string>>(
            (_) => ({ ..._, label, tooltip, details }),
          );
      if (viewKind == "secret")
        return SecretForm<any & FormLabel, Unit>()
          .withView(formViews[viewKind][viewName]())
          .mapContext<any & FormLabel & CommonFormState & Value<string>>(
            (_) => ({ ..._, label, tooltip, details }),
          );
      // check injectedViews
      if (injectedPrimitives?.injectedPrimitives.has(viewKind as keyof T)) {
        //TODO error handling instead of cast
        const injectedPrimitive = injectedPrimitives.injectedPrimitives.get(
          viewKind as keyof T,
        ); //TODO error handling instead of cast
        return injectedPrimitive?.fieldView(
          formViews,
          viewKind,
          viewName,
          label,
          tooltip,
          details,
        );
      }
      return `error: the view for ${viewKind as string}::${
        viewName as string
      } cannot be found`;
    },
    FormStates: <T>(
      viewType: any,
      viewName: any,
      InfiniteStreamSources: any,
      injectedPrimitives?: InjectedPrimitives<T>,
      stream?: string,
    ): any => {
      if (
        viewType == "unit" ||
        viewType == "boolean" ||
        viewType == "number" ||
        viewType == "string" ||
        viewType == "base64File" ||
        viewType == "secret"
      )
        return { commonFormState: CommonFormState.Default() };
      if (injectedPrimitives?.injectedPrimitives.has(viewType)) {
        const injectedPrimitiveDefaultState =
          injectedPrimitives.injectedPrimitives.get(viewType)?.defaultState;
        return injectedPrimitiveDefaultState != undefined
          ? {
              customFormState: injectedPrimitiveDefaultState,
              commonFormState: CommonFormState.Default(),
            }
          : { commonFormState: CommonFormState.Default() };
      }
      if (viewType == "date") return DateFormState.Default();
      if (viewType == "enumSingleSelection" || viewType == "enumMultiSelection")
        return EnumFormState().Default();
      if (
        (viewType == "streamSingleSelection" ||
          viewType == "streamMultiSelection") &&
        stream != undefined
      ) {
        return SearchableInfiniteStreamState().Default(
          "",
          (InfiniteStreamSources as any)(stream),
        );
      }
      return `error: the view for ${viewType as string}::${
        viewName as string
      } cannot be found when creating the corresponding field form state`;
    },
  },
};
