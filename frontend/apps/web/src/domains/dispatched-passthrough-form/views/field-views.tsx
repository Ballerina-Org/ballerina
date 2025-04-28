import {
  CommonFormState,
  AsyncState,
  FormLabel,
  BooleanView,
  NumberView,
  StringFormView,
  DateView,
  EnumView,
  EnumMultiselectView,
  SearchableInfiniteStreamView,
  InfiniteStreamMultiselectView,
  DispatchBaseEnumContext,
  ListFieldView,
  unit,
  MapFieldView,
  Base64FileFormView,
  SecretFormView,
  PredicateValue,
  ValueRecord,
  TupleFormView,
  SumFormView,
  UnitFormView,
  DateFormState,
  UnitFormState,
  replaceWith,
  Maybe,
  SumFormState,
  id,
  Sum,
  DateForm,
  Delta,
  ParsedType,
  TupleAbstractRendererView,
  BoolAbstractRendererView,
  NumberAbstractRendererView,
  StringAbstractRendererView,
  DateAbstractRendererView,
  EnumAbstractRendererView,
  EnumMultiselectAbstractRendererView,
  SearchableInfiniteStreamAbstractRendererView,
  InfiniteStreamMultiselectAbstractRendererView,
  ListAbstractRendererView,
  Base64FileAbstractRendererView,
  SecretAbstractRendererView,
  MapAbstractRendererView,
  SumAbstractRendererView,
  UnitAbstractRendererView,
  SumAbstractRendererState,
  DispatchDelta,
  DispatchParsedType,
  Bindings,
  Unit,
  FormLayout,
  RecordAbstractRendererView,
} from "ballerina-core";
import { DispatchCategoryView } from "../injected-forms/category";
import { Map } from "immutable";

export const PersonConcreteRenderers = {
  record: {
    person:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): RecordAbstractRendererView<{ layout: FormLayout }, Unit> =>
      (props) => {
        return (
          <>
            <h1>Record!</h1>
            <table>
              <tbody>
                {/* {JSON.stringify(props.VisibleFieldKeys.toArray())} */}
                {props.context.layout.valueSeq().map((tab) =>
                  tab.columns.valueSeq().map((column) => (
                    <tr style={{ display: "block", float: "left" }}>
                      {column.groups.valueSeq().map((group) =>
                        group
                          .filter((fieldName) =>
                            props.VisibleFieldKeys.has(fieldName),
                          )
                          .map((fieldName) => (
                            <>
                              {/* <>{console.debug("fieldName", fieldName)}</> */}
                              <td style={{ display: "block" }}>
                                {props.EmbeddedFields.get(fieldName)!({
                                  ...props,
                                  context: {
                                    ...props.context,
                                    disabled:
                                      props.DisabledFieldKeys.has(fieldName),
                                  },
                                  view: unit,
                                })}
                              </td>
                            </>
                          )),
                      )}
                    </tr>
                  )),
                )}
              </tbody>
            </table>
          </>
        );
      },
    address:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): RecordAbstractRendererView<{ layout: FormLayout }, Unit> =>
      (props) => {
        return (
          <>
            <h3>Address!</h3>
            <table>
              <tbody>
                {props.context.layout.valueSeq().map((tab) =>
                  tab.columns.valueSeq().map((column) => (
                    <tr style={{ display: "block", float: "left" }}>
                      {column.groups.valueSeq().map((group) =>
                        group
                          .filter((fieldName) =>
                            props.VisibleFieldKeys.has(fieldName),
                          )
                          .map((fieldName) => (
                            <td style={{ display: "block" }}>
                              {props.EmbeddedFields.get(fieldName)!({
                                ...props,
                                context: {
                                  ...props.context,
                                  disabled:
                                    props.DisabledFieldKeys.has(fieldName),
                                },
                                view: unit,
                              })}
                            </td>
                          )),
                      )}
                    </tr>
                  )),
                )}
              </tbody>
            </table>
          </>
        );
      },
  },
  table: {
    finiteTable: (props: any) => {
      return (
        <table>
          <thead style={{ border: "1px solid black" }}>
            <tr style={{ border: "1px solid black" }}>
              {props.VisibleColumns.map((header: any) => (
                <th style={{ border: "1px solid black" }}>{header}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {props.TableValues.map(([chunkIndex, key]: [number, string]) => (
              <tr style={{ border: "1px solid black" }}>
                {props.VisibleColumns.map((header: any) => {
                  return (
                    <td style={{ border: "1px solid black" }}>
                      {props.EmbeddedCellTemplates[header](chunkIndex)(key)({
                        ...props,
                        view: unit,
                      })}
                    </td>
                  );
                })}
              </tr>
            ))}
          </tbody>
        </table>
      );
    },
    streamingTable: (props: any) => {
      return (
        <>
          <table>
            <thead style={{ border: "1px solid black" }}>
              <tr style={{ border: "1px solid black" }}>
                {props.VisibleColumns.map((header: any) => (
                  <th style={{ border: "1px solid black" }}>{header}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {props.TableValues.map(([chunkIndex, key]: [number, string]) => (
                <tr style={{ border: "1px solid black" }}>
                  {props.VisibleColumns.map((header: any) => {
                    return (
                      <td style={{ border: "1px solid black" }}>
                        {props.EmbeddedCellTemplates[header](chunkIndex)(key)({
                          ...props,
                          view: unit,
                        })}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
          <button onClick={() => props.foreignMutations.loadMore()}>
            Load More
          </button>
        </>
      );
    },
  },
  injectedCategory: {
    defaultCategory:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): DispatchCategoryView<Context, ForeignMutationsExpected> =>
      (props) => {
        return (
          <>
            {props.context.label && <h3>{props.context.label}</h3>}
            {props.context.tooltip && <p>{props.context.tooltip}</p>}
            {props.context.details && (
              <p>
                <em>{props.context.details}</em>
              </p>
            )}
            <button
              style={
                props.context.value.value.kind == "child"
                  ? { borderColor: "red" }
                  : {}
              }
              onClick={(_) =>
                props.foreignMutations.setNewValue({
                  kind: "custom",
                  value: {
                    kind: "child",
                    extraSpecial: false,
                  },
                })
              }
            >
              child
            </button>
            <button
              style={
                props.context.value.value.kind == "adult"
                  ? { borderColor: "red" }
                  : {}
              }
              onClick={(_) =>
                props.foreignMutations.setNewValue({
                  kind: "custom",
                  value: {
                    kind: "adult",
                    extraSpecial: false,
                  },
                })
              }
            >
              adult
            </button>
            <button
              style={
                props.context.value.value.kind == "senior"
                  ? { borderColor: "red" }
                  : {}
              }
              onClick={(_) =>
                props.foreignMutations.setNewValue({
                  kind: "custom",
                  value: {
                    kind: "senior",
                    extraSpecial: false,
                  },
                })
              }
            >
              senior
            </button>
          </>
        );
      },
  },
  boolean: {
    defaultBoolean:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): BoolAbstractRendererView<Context, ForeignMutationsExpected> =>
      (props) => (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}
          <input
            disabled={props.context.disabled}
            type="checkbox"
            checked={props.context.value}
            onChange={(e) =>
              props.foreignMutations.setNewValue(e.currentTarget.checked)
            }
          />
        </>
      ),
    secondBoolean:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): BoolAbstractRendererView<Context, ForeignMutationsExpected> =>
      (props) => (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}
          <input
            disabled={props.context.disabled}
            type="checkbox"
            checked={props.context.value}
            onChange={(e) =>
              props.foreignMutations.setNewValue(e.currentTarget.checked)
            }
          />
        </>
      ),
  },
  number: {
    defaultNumber:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): NumberAbstractRendererView<Context, ForeignMutationsExpected> =>
      (props) => (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}
          <input
            disabled={props.context.disabled}
            type="number"
            value={props.context.value}
            onChange={(e) =>
              props.foreignMutations.setNewValue(
                ~~parseInt(e.currentTarget.value),
              )
            }
          />
        </>
      ),
  },
  string: {
    defaultString:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): StringAbstractRendererView<Context, ForeignMutationsExpected> =>
      (props) => {
        return (
          <>
            {props.context.label && <h3>{props.context.label}</h3>}
            {props.context.tooltip && <p>{props.context.tooltip}</p>}
            {props.context.details && (
              <p>
                <em>{props.context.details}</em>
              </p>
            )}
            <input
              disabled={props.context.disabled}
              value={props.context.value}
              onChange={(e) =>
                props.foreignMutations.setNewValue(e.currentTarget.value)
              }
            />
          </>
        );
      },
  },
  date: {
    defaultDate:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): DateAbstractRendererView<Context, ForeignMutationsExpected> =>
      (props) => {
        const displayValue = props.context.commonFormState.modifiedByUser
          ? props.context.customFormState.possiblyInvalidInput
          : props.context.value?.toISOString().slice(0, 10);
        return (
          <>
            {props.context.label && <h3>{props.context.label}</h3>}
            {props.context.tooltip && <p>{props.context.tooltip}</p>}
            {props.context.details && (
              <p>
                <em>{props.context.details}</em>
              </p>
            )}
            <input
              disabled={props.context.disabled}
              type="date"
              value={displayValue}
              onChange={(e) =>
                props.foreignMutations.setNewValue(e.currentTarget.value)
              }
            />
          </>
        );
      },
  },
  enumSingleSelection: {
    defaultEnum:
      <
        Context extends FormLabel & DispatchBaseEnumContext,
        ForeignMutationsExpected,
      >(): EnumAbstractRendererView<Context, ForeignMutationsExpected> =>
      (props) => {
        const isSome = props.context.value.isSome;
        const value =
          isSome &&
          PredicateValue.Operations.IsRecord(props.context.value.value)
            ? props.context.value.value.fields.get("Value")!
            : undefined;

        return (
          <>
            {props.context.label && <h3>{props.context.label}</h3>}
            {props.context.details && (
              <p>
                <em>{props.context.details}</em>
              </p>
            )}
            {props.context.activeOptions == "loading" ? (
              "loading options"
            ) : (
              <select
                value={value as string | undefined}
                onChange={(e) =>
                  props.foreignMutations.setNewValue(e.currentTarget.value)
                }
              >
                <>
                  <option></option>
                  {props.context.activeOptions.map((o) => (
                    <option value={o.fields.get("Value")! as string}>
                      {o.fields.get("Value") as string}
                    </option>
                  ))}
                </>
              </select>
            )}
          </>
        );
      },
  },
  enumMultiSelection: {
    defaultEnumMultiselect:
      <
        Context extends FormLabel & DispatchBaseEnumContext,
        ForeignMutationsExpected,
      >(): EnumMultiselectAbstractRendererView<
        Context,
        ForeignMutationsExpected
      > =>
      (props) => (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}
          {props.context.activeOptions == "loading" ? (
            "loading options"
          ) : (
            <select
              multiple
              value={props.context.selectedIds}
              disabled={props.context.disabled}
              onChange={(e) =>
                props.foreignMutations.setNewValue(
                  Array.from(e.currentTarget.options)
                    .filter((_) => _.selected)
                    .map((_) => _.value),
                )
              }
            >
              <>
                {props.context.activeOptions.map((o) => (
                  <option value={o.fields.get("Value")! as string}>
                    {o.fields.get("Value") as string}
                  </option>
                ))}
              </>
            </select>
          )}
        </>
      ),
  },
  streamSingleSelection: {
    defaultInfiniteStream:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): SearchableInfiniteStreamAbstractRendererView<
        Context,
        ForeignMutationsExpected
      > =>
      (props) => (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.tooltip && <p>{props.context.tooltip}</p>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}
          <button
            disabled={props.context.disabled}
            onClick={() => props.foreignMutations.toggleOpen()}
          >
            {props.context.value.isSome &&
              ((props.context.value.value as ValueRecord).fields.get(
                "DisplayValue",
              ) as string)}{" "}
            {props.context.customFormState.status == "open" ? "➖" : "➕"}
          </button>
          <button
            disabled={props.context.disabled}
            onClick={() => props.foreignMutations.clearSelection()}
          >
            ❌
          </button>
          {props.context.customFormState.status == "closed" ? (
            <></>
          ) : (
            <>
              <input
                disabled={props.context.disabled}
                value={props.context.customFormState.searchText.value}
                onChange={(e) =>
                  props.foreignMutations.setSearchText(e.currentTarget.value)
                }
              />
              <ul>
                {props.context.customFormState.stream.loadedElements
                  .valueSeq()
                  .map((chunk) =>
                    chunk.data.valueSeq().map((element) => (
                      <li>
                        <button
                          disabled={props.context.disabled}
                          onClick={() =>
                            props.foreignMutations.select(
                              PredicateValue.Default.option(
                                true,
                                ValueRecord.Default.fromJSON(element),
                              ),
                            )
                          }
                        >
                          {element.DisplayValue}{" "}
                          {props.context.value.isSome &&
                          (props.context.value.value as ValueRecord).fields.get(
                            "Id",
                          ) == element.Id
                            ? "✅"
                            : ""}
                        </button>
                      </li>
                    )),
                  )}
              </ul>
            </>
          )}
          <button
            disabled={props.context.hasMoreValues == false}
            onClick={() => props.foreignMutations.loadMore()}
          >
            ⋯
          </button>
          <button onClick={() => props.foreignMutations.reload()}>🔄</button>
        </>
      ),
  },
  streamMultiSelection: {
    defaultInfiniteStreamMultiselect:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): InfiniteStreamMultiselectAbstractRendererView<
        Context,
        ForeignMutationsExpected
      > =>
      (props) => {
        return (
          <>
            {props.context.label && <h3>{props.context.label}</h3>}
            {props.context.details && (
              <p>
                <em>{props.context.details}</em>
              </p>
            )}
            <button
              disabled={props.context.disabled}
              onClick={() => props.foreignMutations.toggleOpen()}
            >
              {props.context.value.fields
                .map(
                  (_) =>
                    (_ as ValueRecord).fields.get("DisplayValue") as string,
                )
                .join(", ")}{" "}
              {props.context.customFormState.status == "open" ? "➖" : "➕"}
            </button>
            <button
              disabled={props.context.disabled}
              onClick={() => props.foreignMutations.clearSelection()}
            >
              ❌
            </button>
            {props.context.customFormState.status == "closed" ? (
              <></>
            ) : (
              <>
                <input
                  disabled={props.context.disabled}
                  value={props.context.customFormState.searchText.value}
                  onChange={(e) =>
                    props.foreignMutations.setSearchText(e.currentTarget.value)
                  }
                />
                <ul>
                  {props.context.availableOptions.map((element) => {
                    return (
                      <li>
                        <button
                          disabled={props.context.disabled}
                          onClick={() =>
                            props.foreignMutations.toggleSelection(
                              ValueRecord.Default.fromJSON(element),
                            )
                          }
                        >
                          {element.DisplayValue}{" "}
                          {props.context.value.fields.has(element.Id)
                            ? "✅"
                            : ""}
                        </button>
                      </li>
                    );
                  })}
                </ul>
              </>
            )}
            <button
              disabled={
                props.context.disabled || props.context.hasMoreValues == false
              }
              onClick={() => props.foreignMutations.loadMore()}
            >
              ⋯
            </button>
            <button
              disabled={props.context.disabled}
              onClick={() => props.foreignMutations.reload()}
            >
              🔄
            </button>
          </>
        );
      },
  },
  list: {
    defaultList:
      <
        Context extends FormLabel & { disabled: boolean } & {
          bindings: Bindings;
          extraContext: any;
        },
        ForeignMutationsExpected,
      >(): ListAbstractRendererView<Context, ForeignMutationsExpected> =>
      (props) => {
        return (
          <>
            {props.context.label && <h3>{props.context.label}</h3>}
            {props.context.tooltip && <p>{props.context.tooltip}</p>}
            {props.context.details && (
              <p>
                <em>{props.context.details}</em>
              </p>
            )}
            <ul>
              {props.context.value.values.map((_, elementIndex) => {
                return (
                  <li
                    style={{
                      display: "flex",
                      flexDirection: "column",
                      gap: "10px",
                    }}
                  >
                    {props.embeddedElementTemplate(elementIndex)({
                      ...props,
                      context: {
                        ...props.context,
                      },
                      view: unit,
                    })}
                    <div style={{ display: "flex" }}>
                      <button
                        onClick={() =>
                          props.foreignMutations.remove(elementIndex)
                        }
                        disabled={props.context.disabled}
                      >
                        ❌
                      </button>
                      <button
                        onClick={() =>
                          props.foreignMutations.move(
                            elementIndex,
                            elementIndex - 1,
                          )
                        }
                        disabled={props.context.disabled}
                      >
                        ⬆️
                      </button>
                      <button
                        onClick={() =>
                          props.foreignMutations.move(
                            elementIndex,
                            elementIndex + 1,
                          )
                        }
                        disabled={props.context.disabled}
                      >
                        ⬇️
                      </button>
                      <button
                        onClick={() =>
                          props.foreignMutations.duplicate(elementIndex)
                        }
                        disabled={props.context.disabled}
                      >
                        📑
                      </button>
                      <button
                        onClick={() =>
                          props.foreignMutations.insert(elementIndex + 1)
                        }
                        disabled={props.context.disabled}
                      >
                        ➕
                      </button>
                    </div>
                  </li>
                );
              })}
            </ul>
            <button
              onClick={() => {
                props.foreignMutations.add(unit);
              }}
              disabled={props.context.disabled}
            >
              ➕
            </button>
          </>
        );
      },
  },
  base64File: {
    defaultBase64File:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): Base64FileAbstractRendererView<Context, ForeignMutationsExpected> =>
      (props) => (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}
          <input
            type="text"
            value={props.context.value}
            onChange={(e) =>
              props.foreignMutations.setNewValue(e.currentTarget.value)
            }
          />
        </>
      ),
  },
  secret: {
    defaultSecret:
      <
        Context extends FormLabel,
        ForeignMutationsExpected,
      >(): SecretAbstractRendererView<Context, ForeignMutationsExpected> =>
      (props) => (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}
          <input
            type="password"
            value={props.context.value}
            onChange={(e) =>
              props.foreignMutations.setNewValue(e.currentTarget.value)
            }
          />
        </>
      ),
  },
  map: {
    defaultMap:
      <
        KeyFormState,
        ValueFormState,
        Context extends FormLabel & { bindings: Bindings; extraContext: any },
        ForeignMutationsExpected,
      >(): MapAbstractRendererView<
        KeyFormState,
        ValueFormState,
        Context,
        ForeignMutationsExpected
      > =>
      (props) => (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.tooltip && <p>{props.context.tooltip}</p>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}
          <ul>
            {props.context.value.values.map((_, elementIndex) => {
              return (
                <li>
                  <button
                    onClick={() => props.foreignMutations.remove(elementIndex)}
                  >
                    ❌
                  </button>
                  {props.embeddedKeyTemplate(elementIndex)({
                    ...props,
                    view: unit,
                  })}
                  {props.embeddedValueTemplate(elementIndex)({
                    ...props,
                    view: unit,
                  })}
                </li>
              );
            })}
          </ul>
          <button
            onClick={() => {
              props.foreignMutations.add(unit);
            }}
          >
            ➕
          </button>
        </>
      ),
  },
  tuple: {
    defaultTuple2:
      <
        Context extends FormLabel & { bindings: Bindings; extraContext: any },
        ForeignMutationsExpected,
      >(): TupleAbstractRendererView<Context, ForeignMutationsExpected> =>
      (props) => (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          <div>
            {props.context.value.values.map((_, elementIndex) => {
              return (
                <>
                  {props.embeddedItemTemplates(elementIndex)({
                    ...props,
                    view: unit,
                  })}
                </>
              );
            })}
          </div>
        </>
      ),
    defaultTuple3:
      <
        Context extends FormLabel & { bindings: Bindings; extraContext: any },
        ForeignMutationsExpected,
      >(): TupleAbstractRendererView<Context, ForeignMutationsExpected> =>
      (props) => {
        return (
          <>
            {props.context.label && <h3>{props.context.label}</h3>}
            <div>
              {props.embeddedItemTemplates(0)({
                ...props,
                view: unit,
              })}
              {props.embeddedItemTemplates(1)({
                ...props,
                view: unit,
              })}
            </div>
            <div>
              {props.embeddedItemTemplates(2)({
                ...props,
                view: unit,
              })}
            </div>
          </>
        );
      },
  },
  sum: {
    defaultSum: <
      LeftFormState,
      RightFormState,
      Context extends FormLabel & { bindings: Bindings; extraContext: any },
      ForeignMutationsExpected,
    >(): SumAbstractRendererView<
      LeftFormState,
      RightFormState,
      Context,
      ForeignMutationsExpected
    > => {
      return (props) => {
        if (
          props.embeddedLeftTemplate == undefined ||
          props.embeddedRightTemplate == undefined
        ) {
          console.error(
            "embeddedLeftTemplate or embeddedRightTemplate is undefined, but both are expected in defaultSum",
          );
          return <></>;
        }
        return (
          <>
            {props.embeddedLeftTemplate({
              ...props,
              view: unit,
            })}
            {props.embeddedRightTemplate({
              ...props,
              view: unit,
            })}
          </>
        );
      };
    },
    maybeDate: <
      Context extends FormLabel & { disabled: boolean; type: ParsedType<any> },
      ForeignMutationsExpected,
    >(): SumFormView<
      UnitFormState,
      DateFormState,
      Context,
      ForeignMutationsExpected
    > => {
      return (props) => {
        const displayValue =
          props.context.value.value.kind == "l"
            ? ""
            : props.context.customFormState.right.commonFormState.modifiedByUser
              ? props.context.customFormState.right.customFormState
                  .possiblyInvalidInput
              : (props.context.value.value.value as Date)
                  .toISOString()
                  .slice(0, 10);

        const setNewValue = (_: Maybe<string>) => {
          props.setState(
            SumFormState<
              UnitFormState,
              DateFormState
            >().Updaters.Core.customFormState((__) => ({
              ...__,
              right: DateFormState.Updaters.Core.customFormState.children
                .possiblyInvalidInput(replaceWith(_))
                .then(
                  DateFormState.Updaters.Core.commonFormState((___) => ({
                    ...___,
                    modifiedByUser: true,
                  })),
                )(__.right),
            })),
          );
          const newValue = _ == undefined ? _ : new Date(_);
          if (!(newValue == undefined || isNaN(newValue.getTime()))) {
            const delta: Delta = {
              kind: "SumReplace",
              replace: PredicateValue.Default.sum(Sum.Default.right(newValue)),
              state: {
                commonFormState: props.context.commonFormState,
                customFormState: props.context.customFormState,
              },
              type: props.context.type,
            };
            setTimeout(() => {
              props.foreignMutations.onChange(
                replaceWith(
                  PredicateValue.Default.sum(Sum.Default.right(newValue)),
                ),
                delta,
              );
            }, 0);
          }
        };

        const clearValue = () => {
          props.setState(
            SumFormState<
              UnitFormState,
              DateFormState
            >().Updaters.Core.customFormState((__) => ({
              ...__,
              left: UnitFormState.Updaters.Core.commonFormState((___) => ({
                ...___,
                modifiedByUser: true,
              }))(__.left),
            })),
          );
          const delta: Delta = {
            kind: "SumReplace",
            replace: PredicateValue.Default.sum(
              Sum.Default.left(PredicateValue.Default.unit()),
            ),
            state: {
              commonFormState: props.context.commonFormState,
              customFormState: props.context.customFormState,
            },
            type: props.context.type,
          };
          setTimeout(() => {
            props.foreignMutations.onChange(
              replaceWith(
                PredicateValue.Default.sum(
                  Sum.Default.left(PredicateValue.Default.unit()),
                ),
              ),
              delta,
            );
          }, 0);
        };

        return (
          <>
            {props.context.label && <h3>{props.context.label}</h3>}
            <input
              disabled={props.context.disabled}
              value={displayValue}
              type="date"
              onChange={(e) => {
                if (e.currentTarget.value == "") {
                  clearValue();
                } else {
                  setNewValue(e.currentTarget.value);
                }
              }}
            />
          </>
        );
      };
    },
  },
  unit: {
    defaultUnit: <
      Context extends FormLabel,
    >(): UnitAbstractRendererView<Context> => {
      return (props) => {
        return (
          <>
            {props.context.label && <h3>{props.context.label}</h3>}
            Unit field
          </>
        );
      };
    },
  },
  sumUnitDate: {
    maybeDate: <
      Context extends FormLabel & {
        disabled: boolean;
        type: DispatchParsedType<any>;
      },
      ForeignMutationsExpected,
    >(): SumAbstractRendererView<
      UnitFormState,
      DateFormState,
      Context,
      ForeignMutationsExpected
    > => {
      return (props) => {
        const displayValue =
          props.context.value.value.kind == "l"
            ? ""
            : props.context.customFormState.right.commonFormState.modifiedByUser
              ? props.context.customFormState.right.customFormState
                  .possiblyInvalidInput
              : (props.context.value.value.value as Date)
                  .toISOString()
                  .slice(0, 10);

        const setNewValue = (_: Maybe<string>) => {
          props.setState(
            SumAbstractRendererState<
              UnitFormState,
              DateFormState
            >().Updaters.Core.customFormState((__) => ({
              ...__,
              right: DateFormState.Updaters.Core.customFormState.children
                .possiblyInvalidInput(replaceWith(_))
                .then(
                  DateFormState.Updaters.Core.commonFormState((___) => ({
                    ...___,
                    modifiedByUser: true,
                  })),
                )(__.right),
            })),
          );
          const newValue = _ == undefined ? _ : new Date(_);
          if (!(newValue == undefined || isNaN(newValue.getTime()))) {
            const delta: DispatchDelta = {
              kind: "SumReplace",
              replace: PredicateValue.Default.sum(Sum.Default.right(newValue)),
              state: {
                commonFormState: props.context.commonFormState,
                customFormState: props.context.customFormState,
              },
              type: props.context.type,
            };
            setTimeout(() => {
              props.foreignMutations.onChange(
                replaceWith(
                  PredicateValue.Default.sum(Sum.Default.right(newValue)),
                ),
                delta,
              );
            }, 0);
          }
        };

        const clearValue = () => {
          props.setState(
            SumAbstractRendererState<
              UnitFormState,
              DateFormState
            >().Updaters.Core.customFormState((__) => ({
              ...__,
              left: UnitFormState.Updaters.Core.commonFormState((___) => ({
                ...___,
                modifiedByUser: true,
              }))(__.left),
            })),
          );
          const delta: DispatchDelta = {
            kind: "SumReplace",
            replace: PredicateValue.Default.sum(
              Sum.Default.left(PredicateValue.Default.unit()),
            ),
            state: {
              commonFormState: props.context.commonFormState,
              customFormState: props.context.customFormState,
            },
            type: props.context.type,
          };
          setTimeout(() => {
            props.foreignMutations.onChange(
              replaceWith(
                PredicateValue.Default.sum(
                  Sum.Default.left(PredicateValue.Default.unit()),
                ),
              ),
              delta,
            );
          }, 0);
        };

        return (
          <>
            {props.context.label && <h3>{props.context.label}</h3>}
            <input
              disabled={props.context.disabled}
              value={displayValue}
              type="date"
              onChange={(e) => {
                if (e.currentTarget.value == "") {
                  clearValue();
                } else {
                  setNewValue(e.currentTarget.value);
                }
              }}
            />
          </>
        );
      };
    },
  },
};
