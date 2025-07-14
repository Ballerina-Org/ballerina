import {
  AsyncState,
  unit,
  PredicateValue,
  ValueRecord,
  DateFormState,
  UnitFormState,
  replaceWith,
  Maybe,
  Sum,
  SumAbstractRendererState,
  DispatchDelta,
  Option,
  ConcreteRenderers,
} from "ballerina-core";
import { OrderedMap, Set } from "immutable";
import { DispatchPassthroughFormInjectedTypes } from "../injected-forms/category";

export type DispatchPassthroughFormFlags = {
  test: boolean;
};

export type ListElementCustomPresentationContext = {
  isLastListElement: boolean;
};

export type DispatchPassthroughFormExtraContext = {
  flags: Set<string>;
};

export type DispatchPassthroughFormCustomPresentationContext = {
  listElement: ListElementCustomPresentationContext;
};

export const DispatchPassthroughFormConcreteRenderers: ConcreteRenderers<
  DispatchPassthroughFormInjectedTypes,
  DispatchPassthroughFormFlags,
  DispatchPassthroughFormCustomPresentationContext,
  DispatchPassthroughFormExtraContext
> = {
  one: {
    admin: () => (props) => {
      const propsLocal = props;
      const fm = propsLocal.foreignMutations;
      const ctx = propsLocal.context;
      if (
        ctx.kind == "uninitialized" ||
        fm.kind == "uninitialized" ||
        propsLocal.kind == "uninitialized"
      ) {
        return <></>;
      }
      if (
        !AsyncState.Operations.hasValue(ctx.customFormState.selectedValue.sync)
      ) {
        return <></>;
      }
      if (ctx.customFormState.selectedValue.sync.value.kind == "errors") {
        console.error(
          ctx.customFormState.selectedValue.sync.value.errors
            .join("\n")
            .concat(`\n...When parsing the "one" field value\n...`),
        );
        return <></>;
      }

      if (PredicateValue.Operations.IsUnit(ctx.value)) {
        return <></>;
      }

      return (
        <div
          style={{
            border: "1px solid red",
            display: "flex",
            flexDirection: "column",
            alignItems: "centre",
            justifyContent: "center",
            gap: "10px",
          }}
        >
          <p>DetailsRenderer</p>
          {propsLocal.DetailsRenderer(undefined)({
            ...propsLocal,
            context: {
              ...ctx,
            },
            foreignMutations: {
              ...fm,
            },
            view: unit,
          })}
          <p>PreviewRenderer</p>
          <button disabled={ctx.disabled} onClick={() => fm.toggleOpen()}>
            {propsLocal?.PreviewRenderer &&
              propsLocal.PreviewRenderer(ctx.value)("unique-id")(undefined)?.({
                ...propsLocal,
                context: {
                  ...ctx,
                },
                foreignMutations: {
                  ...fm,
                },
                view: unit,
              })}
            {ctx.customFormState.status == "open" ? "➖" : "➕"}
          </button>
          {ctx.customFormState.status == "closed" ? (
            <></>
          ) : (
            <>
              <input
                disabled={ctx.disabled}
                value={
                  ctx.customFormState.streamParams.value.get("search") ?? ""
                }
                onChange={(e) =>
                  fm.setStreamParam("search", e.currentTarget.value)
                }
              />
              <ul>
                {ctx.customFormState.stream.loadedElements
                  .entrySeq()
                  .map(([key, chunk]) =>
                    chunk.data.valueSeq().map((element: any) => {
                      return (
                        <li>
                          <button
                            disabled={ctx.disabled}
                            onClick={() => fm.select(element, undefined)}
                          >
                            <div
                              onClick={() => fm.select(element, undefined)}
                              style={{
                                display: "flex",
                                flexDirection: "row",
                                gap: "10px",
                              }}
                            />
                            {propsLocal?.PreviewRenderer &&
                              propsLocal.PreviewRenderer(element)(
                                key.toString(),
                              )(undefined)?.({
                                ...propsLocal,
                                context: {
                                  ...ctx,
                                },
                                foreignMutations: {
                                  ...fm,
                                },
                                view: unit,
                              })}
                          </button>
                        </li>
                      );
                    }),
                  )}
              </ul>
            </>
          )}
          <button
            disabled={ctx.hasMoreValues == false}
            onClick={() => fm.loadMore()}
          >
            ⋯
          </button>
          <button onClick={() => fm.reload()}>🔄</button>
        </div>
      );
    },
    partialAdmin: () => (props) => {
      const propsLocal = props;
      const fm = propsLocal.foreignMutations;
      const ctx = propsLocal.context;
      if (
        ctx.kind == "uninitialized" ||
        fm.kind == "uninitialized" ||
        propsLocal.kind == "uninitialized"
      ) {
        return <></>;
      }
      if (
        !AsyncState.Operations.hasValue(ctx.customFormState.selectedValue.sync)
      ) {
        return <></>;
      }
      if (ctx.customFormState.selectedValue.sync.value.kind == "errors") {
        console.error(
          ctx.customFormState.selectedValue.sync.value.errors
            .join("\n")
            .concat(`\n...When parsing the "one" field value\n...`),
        );
        return <></>;
      }

      if (PredicateValue.Operations.IsUnit(ctx.value)) {
        return (
          <>
            <p>one admin renderer</p>
            <p>DetailsRenderer</p>
            {propsLocal.DetailsRenderer(undefined)({
              ...propsLocal,
              context: {
                ...ctx,
              },
              foreignMutations: {
                ...fm,
              },
              view: unit,
            })}
            <p>PreviewRenderer</p>
            <button disabled={ctx.disabled} onClick={() => fm.toggleOpen()}>
              {ctx.customFormState.status == "open" ? "➖" : "➕"}
            </button>
            {ctx.customFormState.status == "closed" ? (
              <></>
            ) : (
              <>
                <input
                  disabled={ctx.disabled}
                  value={
                    ctx.customFormState.streamParams.value.get("search") ?? ""
                  }
                  onChange={(e) =>
                    fm.setStreamParam("search", e.currentTarget.value)
                  }
                />
                <ul>
                  {ctx.customFormState.stream.loadedElements
                    .entrySeq()
                    .map(([key, chunk]) =>
                      chunk.data.valueSeq().map((element: any, idx: number) => {
                        return (
                          <li>
                            <button
                              disabled={ctx.disabled}
                              onClick={() => fm.select(element, undefined)}
                            >
                              <div
                                onClick={() => fm.select(element, undefined)}
                                style={{
                                  display: "flex",
                                  flexDirection: "row",
                                  gap: "10px",
                                }}
                              />
                              {propsLocal?.PreviewRenderer &&
                                propsLocal?.PreviewRenderer(element)(
                                  key.toString(),
                                )(undefined)?.({
                                  ...propsLocal,
                                  context: {
                                    ...ctx,
                                  },
                                  foreignMutations: {
                                    ...fm,
                                  },
                                  view: unit,
                                })}
                            </button>
                          </li>
                        );
                      }),
                    )}
                </ul>
              </>
            )}
            <button
              disabled={ctx.hasMoreValues == false}
              onClick={() => fm.loadMore()}
            >
              ⋯
            </button>
            <button onClick={() => fm.reload()}>🔄</button>
          </>
        );
      }

      return (
        <>
          <p>one admin renderer</p>
          <p>DetailsRenderer</p>
          {propsLocal.DetailsRenderer(undefined)({
            ...propsLocal,
            context: {
              ...ctx,
            },
            foreignMutations: {
              ...fm,
            },
            view: unit,
          })}
          <p>PreviewRenderer</p>
          <button disabled={ctx.disabled} onClick={() => fm.toggleOpen()}>
            {propsLocal?.PreviewRenderer &&
              propsLocal.PreviewRenderer(ctx.value)("unique-id")(undefined)?.({
                ...propsLocal,
                context: {
                  ...ctx,
                },
                foreignMutations: {
                  ...fm,
                },
                view: unit,
              })}
            {ctx.customFormState.status == "open" ? "➖" : "➕"}
          </button>
          {ctx.customFormState.status == "closed" ? (
            <></>
          ) : (
            <>
              <input
                disabled={ctx.disabled}
                value={
                  ctx.customFormState.streamParams.value.get("search") ?? ""
                }
                onChange={(e) =>
                  fm.setStreamParam("search", e.currentTarget.value)
                }
              />
              <ul>
                {ctx.customFormState.stream.loadedElements
                  .entrySeq()
                  .map(([key, chunk]) =>
                    chunk.data.valueSeq().map((element: any) => {
                      return (
                        <li>
                          <button
                            disabled={ctx.disabled}
                            onClick={() => fm.select(element, undefined)}
                          >
                            <div
                              onClick={() => fm.select(element, undefined)}
                              style={{
                                display: "flex",
                                flexDirection: "row",
                                gap: "10px",
                              }}
                            />
                            {propsLocal?.PreviewRenderer &&
                              propsLocal.PreviewRenderer(element)(
                                key.toString(),
                              )(undefined)?.({
                                ...propsLocal,
                                context: {
                                  ...ctx,
                                },
                                foreignMutations: {
                                  ...fm,
                                },
                                view: unit,
                              })}
                          </button>
                        </li>
                      );
                    }),
                  )}
              </ul>
            </>
          )}
          <button
            disabled={ctx.hasMoreValues == false}
            onClick={() => fm.loadMore()}
          >
            ⋯
          </button>
          <button onClick={() => fm.reload()}>🔄</button>
        </>
      );
    },
    bestFriend: () => (props) => {
      const propsLocal = props;
      const fm = propsLocal.foreignMutations;
      const ctx = propsLocal.context;
      if (
        ctx.kind == "uninitialized" ||
        fm.kind == "uninitialized" ||
        propsLocal.kind == "uninitialized"
      ) {
        return <></>;
      }
      if (
        !AsyncState.Operations.hasValue(ctx.customFormState.selectedValue.sync)
      ) {
        return <></>;
      }
      if (ctx.customFormState.selectedValue.sync.value.kind == "errors") {
        console.error(
          ctx.customFormState.selectedValue.sync.value.errors
            .join("\n")
            .concat(`\n...When parsing the "one" field value\n...`),
        );
        return <></>;
      }

      if (PredicateValue.Operations.IsUnit(ctx.value)) {
        return <></>;
      }

      return (
        <div
          style={{
            border: "1px solid red",
            display: "flex",
            flexDirection: "column",
            alignItems: "centre",
            justifyContent: "center",
            gap: "10px",
            width: "50%",
            margin: "auto",
          }}
        >
          <h2>{ctx.label}</h2>
          <li>
            {propsLocal.DetailsRenderer(undefined)({
              ...propsLocal,
              context: {
                ...ctx,
              },
              foreignMutations: {
                ...fm,
              },
              view: unit,
            })}
          </li>
          <li>
            <button disabled={ctx.disabled} onClick={() => fm.toggleOpen()}>
              {propsLocal?.PreviewRenderer &&
                propsLocal?.PreviewRenderer(ctx.value)("unique-id")(
                  undefined,
                )?.({
                  ...propsLocal,
                  context: {
                    ...ctx,
                  },
                  foreignMutations: {
                    ...fm,
                  },
                  view: unit,
                })}
              {ctx.customFormState.status == "open" ? "➖" : "➕"}
            </button>
          </li>
          {ctx.customFormState.status == "closed" ? (
            <></>
          ) : (
            <>
              <input
                disabled={ctx.disabled}
                value={
                  ctx.customFormState.streamParams.value.get("search") ?? ""
                }
                onChange={(e) =>
                  fm.setStreamParam("search", e.currentTarget.value)
                }
              />
              <ul>
                {ctx.customFormState.stream.loadedElements
                  .entrySeq()
                  .map(([key, chunk]) =>
                    chunk.data.valueSeq().map((element: any) => {
                      return (
                        <li>
                          <button
                            disabled={ctx.disabled}
                            onClick={() => fm.select(element, undefined)}
                          >
                            <div
                              onClick={() => fm.select(element, undefined)}
                              style={{
                                display: "flex",
                                flexDirection: "row",
                                gap: "10px",
                              }}
                            />
                            {propsLocal?.PreviewRenderer &&
                              propsLocal.PreviewRenderer(element)(
                                key.toString(),
                              )(undefined)?.({
                                ...propsLocal,
                                context: {
                                  ...ctx,
                                },
                                foreignMutations: {
                                  ...fm,
                                },
                                view: unit,
                              })}
                          </button>
                        </li>
                      );
                    }),
                  )}
              </ul>
            </>
          )}
          <button
            disabled={ctx.hasMoreValues == false}
            onClick={() => fm.loadMore()}
          >
            ⋯
          </button>
          <button onClick={() => fm.reload()}>🔄</button>
        </div>
      );
    },
  },
  union: {
    personCases: () => (props) => {
      return (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.embeddedCaseTemplate(props.context.value.caseName)(undefined)({
            ...props,
            view: unit,
          })}
        </>
      );
    },
    job: () => (props) => {
      return (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.embeddedCaseTemplate(props.context.value.caseName)(undefined)({
            ...props,
            view: unit,
          })}
        </>
      );
    },
  },
  record: {
    personDetails: () => (props) => {
      return (
        <>
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
                              {props.EmbeddedFields.get(fieldName)!(undefined)({
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
    userDetails: () => (props) => {
      return (
        <>
          {props.context.layout.valueSeq().map((tab) =>
            tab.columns.valueSeq().map((column) => (
              <div style={{ display: "block", float: "left" }}>
                {column.groups.valueSeq().map((group) =>
                  group
                    .filter((fieldName) =>
                      props.VisibleFieldKeys.has(fieldName),
                    )
                    .map((fieldName) => (
                      <>
                        {/* <>{console.debug("fieldName", fieldName)}</> */}
                        <div style={{ display: "block" }}>
                          {props.EmbeddedFields.get(fieldName)!(undefined)({
                            ...props,
                            context: {
                              ...props.context,
                              disabled: props.DisabledFieldKeys.has(fieldName),
                            },
                            view: unit,
                          })}
                        </div>
                      </>
                    )),
                )}
              </div>
            )),
          )}
        </>
      );
    },
    friendsDetails: () => (props) => {
      return (
        <>
          {props.context.layout.valueSeq().map((tab) =>
            tab.columns.valueSeq().map((column) => (
              <div style={{ display: "block", float: "left" }}>
                {column.groups.valueSeq().map((group) =>
                  group
                    .filter((fieldName) =>
                      props.VisibleFieldKeys.has(fieldName),
                    )
                    .map((fieldName) => (
                      <>
                        {/* <>{console.debug("fieldName", fieldName)}</> */}
                        <div style={{ display: "block" }}>
                          {props.EmbeddedFields.get(fieldName)!(undefined)({
                            ...props,
                            context: {
                              ...props.context,
                              disabled: props.DisabledFieldKeys.has(fieldName),
                            },
                            view: unit,
                          })}
                        </div>
                      </>
                    )),
                )}
              </div>
            )),
          )}
        </>
      );
    },
    preview: () => (props) => {
      return (
        <>
          {props.context.layout.valueSeq().map((tab) =>
            tab.columns.valueSeq().map((column) => (
              <div style={{ display: "flex" }}>
                {column.groups.valueSeq().map((group) =>
                  group
                    .filter((fieldName) =>
                      props.VisibleFieldKeys.has(fieldName),
                    )
                    .map((fieldName) => (
                      <>
                        {/* <>{console.debug("fieldName", fieldName)}</> */}
                        <div style={{ display: "block" }}>
                          {props.EmbeddedFields.get(fieldName)!(undefined)({
                            ...props,
                            context: {
                              ...props.context,
                              disabled: props.DisabledFieldKeys.has(fieldName),
                            },
                            view: unit,
                          })}
                        </div>
                      </>
                    )),
                )}
              </div>
            )),
          )}
        </>
      );
    },
    address: () => (props) => {
      return (
        <>
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
                            {props.EmbeddedFields.get(fieldName)!(undefined)({
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
    table: () => (_props) => <>Test</>,
    finiteTable: () => (props) => {
      return (
        <>
          <table>
            <thead style={{ border: "1px solid black" }}>
              <tr style={{ border: "1px solid black" }}>
                {props.context.tableHeaders.map((header: string) => (
                  <th style={{ border: "1px solid black" }}>
                    {props.context.columnLabels.get(header) ??
                      "no label specified"}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {props.TableData.valueSeq()
                .toArray()
                .map((row) => (
                  <tr style={{ border: "1px solid black" }}>
                    {props.context.tableHeaders.map((header: string) => (
                      <td style={{ border: "1px solid black" }}>
                        {row.get(header)!(undefined)({
                          ...props,
                          view: unit,
                        })}
                      </td>
                    ))}
                  </tr>
                ))}
            </tbody>
          </table>
        </>
      );
    },
    streamingTable: () => (props) => {
      return (
        <>
          <h3>{props.context.label}</h3>
          <div
            style={{
              display: "flex",
              flexDirection: "row",
              gap: "10px",
              minWidth: "100%",
            }}
          >
            <div
              style={{
                display: "flex",
                flexDirection: "column",
                gap: "10px",
              }}
            >
              <table>
                <thead style={{ border: "1px solid black" }}>
                  <tr style={{ border: "1px solid black" }}>
                    <th>
                      <button
                        onClick={() =>
                          props.foreignMutations.add &&
                          props.foreignMutations.add(undefined)
                        }
                      >
                        {"➕"}
                      </button>
                    </th>
                    <th>
                      <input
                        type="checkbox"
                        checked={
                          props.context.customFormState.selectedRows.size > 0
                        }
                        onClick={() =>
                          props.context.customFormState.selectedRows.size > 0
                            ? props.foreignMutations.clearRows()
                            : props.foreignMutations.selectAllRows()
                        }
                      />
                    </th>
                    {props.context.tableHeaders.map((header: any) => (
                      <th style={{ border: "1px solid black" }}>{header}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {props.TableData.entrySeq()
                    .toArray()
                    .map(([id, row], idx) => (
                      <tr style={{ border: "1px solid black" }}>
                        <button
                          onClick={() =>
                            props.context.customFormState.selectedDetailRow &&
                            props.context.customFormState
                              .selectedDetailRow[1] == id
                              ? props.foreignMutations.clearDetailView()
                              : props.foreignMutations.selectDetailView(id)
                          }
                        >
                          {props.context.customFormState.selectedDetailRow &&
                          props.context.customFormState.selectedDetailRow[1] ==
                            id
                            ? "🙉"
                            : "🙈"}
                        </button>
                        <button
                          onClick={() =>
                            props.foreignMutations.remove &&
                            props.foreignMutations.remove(id, undefined)
                          }
                        >
                          {"❌"}
                        </button>
                        <button
                          onClick={() =>
                            props.foreignMutations.duplicate &&
                            props.foreignMutations.duplicate(id, undefined)
                          }
                        >
                          {"👥"}
                        </button>
                        <select
                          onChange={(_) =>
                            props.foreignMutations.moveTo &&
                            props.foreignMutations.moveTo(
                              id,
                              props.TableData.keySeq().get(
                                Number(_.currentTarget.value),
                              )!,
                              undefined,
                            )
                          }
                        >
                          {props.TableData.entrySeq().map((_, optIdx) => (
                            <option key={_[0]} selected={optIdx === idx}>
                              {optIdx}
                            </option>
                          ))}
                        </select>
                        <td style={{ border: "1px solid black" }}>
                          <input
                            type="checkbox"
                            checked={props.context.customFormState.selectedRows.has(
                              id,
                            )}
                            onClick={() => props.foreignMutations.selectRow(id)}
                          />
                        </td>
                        {props.context.tableHeaders.map((header: string) => (
                          <td style={{ border: "1px solid black" }}>
                            {row.get(header)!(undefined)({
                              ...props,
                              view: unit,
                            })}
                          </td>
                        ))}
                      </tr>
                    ))}
                </tbody>
              </table>
              <button onClick={() => props.foreignMutations.loadMore()}>
                Load More
              </button>
            </div>

            <div key={props.context.customFormState.selectedDetailRow?.[1]}>
              {props.context.customFormState.selectedDetailRow ? (
                <>
                  <div
                    style={{
                      display: "flex",
                      flexDirection: "column",
                      gap: "10px",
                      minWidth: "300px",
                      maxWidth: "300px",
                      backgroundColor: "dimgray",
                      alignItems: "center",
                      justifyContent: "center",
                      borderRadius: "10px",
                    }}
                  >
                    <h3>Detail View</h3>
                    {props.DetailsRenderer &&
                      props.DetailsRenderer(undefined)({
                        ...props,
                        view: unit,
                      })}
                    {/* {DetailView({
                    ...props,
                    view: unit,
                  })} */}
                  </div>
                </>
              ) : (
                <div
                  style={{
                    display: "flex",
                    flexDirection: "column",
                    gap: "10px",
                    minWidth: "300px",
                  }}
                />
              )}
            </div>
          </div>
        </>
      );
    },
  },
  injectedCategory: {
    defaultCategory: () => (props) => {
      return (
        <>
          {props.context.customPresentationContext?.listElement
            ?.isLastListElement && <p>Last</p>}
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
              props.foreignMutations.setNewValue(
                {
                  kind: "custom",
                  value: {
                    kind: "child",
                    extraSpecial: false,
                  },
                },
                undefined,
              )
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
              props.foreignMutations.setNewValue(
                {
                  kind: "custom",
                  value: {
                    kind: "adult",
                    extraSpecial: false,
                  },
                },
                undefined,
              )
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
              props.foreignMutations.setNewValue(
                {
                  kind: "custom",
                  value: {
                    kind: "senior",
                    extraSpecial: false,
                  },
                },
                undefined,
              )
            }
          >
            senior
          </button>
        </>
      );
    },
  },
  boolean: {
    defaultBoolean: () => (props) => (
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
            props.foreignMutations.setNewValue(
              e.currentTarget.checked,
              undefined,
            )
          }
        />
      </>
    ),
    secondBoolean: () => (props) => (
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
            props.foreignMutations.setNewValue(
              e.currentTarget.checked,
              undefined,
            )
          }
        />
      </>
    ),
  },
  number: {
    defaultNumber: () => (props) => (
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
              undefined,
            )
          }
        />
      </>
    ),
  },
  string: {
    defaultString: () => (props) => {
      return (
        <>
          {props.context.customPresentationContext?.listElement
            ?.isLastListElement && <p>Last</p>}
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
              props.foreignMutations.setNewValue(e.currentTarget.value, {
                test: true,
              })
            }
          />
        </>
      );
    },
    otherString: () => (props) => {
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
              props.foreignMutations.setNewValue(e.currentTarget.value, {
                test: true,
              })
            }
          />
        </>
      );
    },
  },
  date: {
    defaultDate: () => (props) => {
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
              props.foreignMutations.setNewValue(
                e.currentTarget.value,
                undefined,
              )
            }
          />
        </>
      );
    },
  },
  enumSingleSelection: {
    defaultEnum: () => (props) => {
      const isSome = props.context.value.isSome;
      const value =
        isSome && PredicateValue.Operations.IsRecord(props.context.value.value)
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
          {props.context.activeOptions == "unloaded" ||
          props.context.activeOptions == "loading" ? (
            <select
              value={value as string | undefined}
              onClick={() => props.foreignMutations.loadOptions()}
            >
              <>
                {value && (
                  <option value={value as string}>{value as string}</option>
                )}
              </>
            </select>
          ) : (
            <select
              value={value as string | undefined}
              onChange={(e) =>
                props.foreignMutations.setNewValue(
                  e.currentTarget.value,
                  undefined,
                )
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
    defaultEnumMultiselect: () => (props) => {
      return (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}
          {props.context.activeOptions == "unloaded" ||
          props.context.activeOptions == "loading" ? (
            <select
              multiple
              value={props.context.selectedIds}
              onClick={() => props.foreignMutations.loadOptions()}
            >
              <>
                {props.context.value.fields.map((o) => (
                  <option
                    value={(o as ValueRecord).fields.get("Value")! as string}
                  >
                    {(o as ValueRecord).fields.get("Value") as string}
                  </option>
                ))}
              </>
            </select>
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
                  undefined,
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
      );
    },
  },
  streamSingleSelection: {
    defaultInfiniteStream: () => (props) => (
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
          onClick={() => props.foreignMutations.clearSelection(undefined)}
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
                            undefined,
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
    defaultInfiniteStreamMultiselect: () => (props) => {
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
                (_) => (_ as ValueRecord).fields.get("DisplayValue") as string,
              )
              .join(", ")}{" "}
            {props.context.customFormState.status == "open" ? "➖" : "➕"}
          </button>
          <button
            disabled={props.context.disabled}
            onClick={() => props.foreignMutations.clearSelection(undefined)}
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
                            undefined,
                          )
                        }
                      >
                        {element.DisplayValue}{" "}
                        {props.context.value.fields.has(element.Id) ? "✅" : ""}
                      </button>
                    </li>
                  );
                })}
              </ul>
              <button
                disabled={props.context.disabled}
                onClick={() =>
                  props.foreignMutations.replace(
                    PredicateValue.Default.record(
                      OrderedMap(
                        props.context.availableOptions
                          .slice(0, 2)
                          .map((opt) => [
                            opt.Id,
                            ValueRecord.Default.fromJSON(opt),
                          ]),
                      ),
                    ),
                    undefined,
                  )
                }
              >
                select first 2
              </button>
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
    defaultList: () => (props) => {
      return (
        <div style={{ border: "1px solid darkblue" }}>
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
                    alignItems: "center",
                    justifyContent: "center",
                  }}
                >
                  {props.embeddedElementTemplate(elementIndex)(undefined)({
                    ...props,
                    context: {
                      ...props.context,
                      customPresentationContext: {
                        listElement: {
                          isLastListElement:
                            elementIndex == props.context.value.values.size - 1,
                        },
                      },
                    },
                    view: unit,
                  })}
                  <div style={{ display: "flex" }}>
                    {props.foreignMutations.remove && (
                      <button
                        onClick={() =>
                          props.foreignMutations.remove!(
                            elementIndex,
                            undefined,
                          )
                        }
                        disabled={props.context.disabled}
                      >
                        ❌
                      </button>
                    )}
                    {props.foreignMutations.move && (
                      <button
                        onClick={() =>
                          props.foreignMutations.move!(
                            elementIndex,
                            elementIndex - 1,
                            undefined,
                          )
                        }
                        disabled={props.context.disabled}
                      >
                        ⬆️
                      </button>
                    )}
                    {props.foreignMutations.move && (
                      <button
                        onClick={() =>
                          props.foreignMutations.move!(
                            elementIndex,
                            elementIndex + 1,
                            undefined,
                          )
                        }
                        disabled={props.context.disabled}
                      >
                        ⬇️
                      </button>
                    )}
                    {props.foreignMutations.duplicate && (
                      <button
                        onClick={() =>
                          props.foreignMutations.duplicate!(
                            elementIndex,
                            undefined,
                          )
                        }
                        disabled={props.context.disabled}
                      >
                        📑
                      </button>
                    )}
                    {props.foreignMutations.add && (
                      <button
                        onClick={() =>
                          props.foreignMutations.insert!(
                            elementIndex + 1,
                            undefined,
                          )
                        }
                        disabled={props.context.disabled}
                      >
                        ➕
                      </button>
                    )}
                  </div>
                </li>
              );
            })}
          </ul>
          {props.foreignMutations.add && (
            <button
              onClick={() => {
                props.foreignMutations.add!({ test: false });
              }}
              disabled={props.context.disabled}
            >
              ➕
            </button>
          )}
        </div>
      );
    },
  },
  base64File: {
    defaultBase64File: () => (props) => (
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
            props.foreignMutations.setNewValue(e.currentTarget.value, undefined)
          }
        />
      </>
    ),
  },
  secret: {
    defaultSecret: () => (props) => (
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
            props.foreignMutations.setNewValue(e.currentTarget.value, undefined)
          }
        />
      </>
    ),
  },
  map: {
    defaultMap: () => (props) => (
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
                  onClick={() =>
                    props.foreignMutations.remove(elementIndex, undefined)
                  }
                >
                  ❌
                </button>
                {props.embeddedKeyTemplate(elementIndex)(undefined)({
                  ...props,
                  view: unit,
                })}
                {props.embeddedValueTemplate(elementIndex)(undefined)({
                  ...props,
                  view: unit,
                })}
              </li>
            );
          })}
        </ul>
        <button
          onClick={() => {
            props.foreignMutations.add({ test: true });
          }}
        >
          ➕
        </button>
      </>
    ),
  },
  tuple: {
    defaultTuple2: () => (props) => (
      <>
        {props.context.label && <h3>{props.context.label}</h3>}
        <div>
          {props.context.value.values.map((_, elementIndex) => {
            return (
              <>
                {props.embeddedItemTemplates(elementIndex)(undefined)({
                  ...props,
                  view: unit,
                })}
              </>
            );
          })}
        </div>
      </>
    ),
    defaultTuple3: () => (props) => {
      return (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          <div>
            {props.embeddedItemTemplates(0)(undefined)({
              ...props,
              view: unit,
            })}
            {props.embeddedItemTemplates(1)(undefined)({
              ...props,
              view: unit,
            })}
          </div>
          <div>
            {props.embeddedItemTemplates(2)(undefined)({
              ...props,
              view: unit,
            })}
          </div>
        </>
      );
    },
  },
  sum: {
    defaultSum: () => (props) => {
      return (
        <>
          {props.context.value.value.kind == "l"
            ? props?.embeddedLeftTemplate?.(undefined)({
                ...props,
                view: unit,
              })
            : props?.embeddedRightTemplate?.(undefined)({
                ...props,
                view: unit,
              })}
        </>
      );
    },
    alwaysRight: () => (props) => {
      return (
        <>
          {props?.embeddedRightTemplate?.(undefined)({
            ...props,
            context: {
              ...props.context,
              value: PredicateValue.Default.sum(
                Sum.Default.right(PredicateValue.Default.unit()),
              ),
            },
            view: unit,
          })}
        </>
      );
    },
    maybeDate: () => (props) => {
      const displayValue =
        props.context.value.value.kind == "l"
          ? ""
          : props.context.customFormState.right.commonFormState.modifiedByUser
            ? (props.context.customFormState.right as DateFormState)
                .customFormState.possiblyInvalidInput
            : (props.context.value.value.value as Date)
                .toISOString()
                .slice(0, 10);

      const setNewValue = (_: Maybe<string>) => {
        props.setState(
          SumAbstractRendererState.Updaters.Core.customFormState((__) => ({
            ...__,
            right: DateFormState.Updaters.Core.customFormState.children
              .possiblyInvalidInput(replaceWith(_))
              .then(
                DateFormState.Updaters.Core.commonFormState((___) => ({
                  ...___,
                  modifiedByUser: true,
                })),
              )(__.right as DateFormState),
          })),
        );
        const newValue = _ == undefined ? _ : new Date(_);
        if (!(newValue == undefined || isNaN(newValue.getTime()))) {
          const delta: DispatchDelta<DispatchPassthroughFormFlags> = {
            kind: "SumReplace",
            replace: PredicateValue.Default.sum(Sum.Default.right(newValue)),
            state: {
              commonFormState: props.context.commonFormState,
              customFormState: props.context.customFormState,
            },
            type: props.context.type,
            flags: {
              test: true,
            },
            sourceAncestorLookupTypeNames:
              props.context.lookupTypeAncestorNames,
          };
          setTimeout(() => {
            props.foreignMutations.onChange(
              Option.Default.some(
                replaceWith(
                  PredicateValue.Default.sum(Sum.Default.right(newValue)),
                ),
              ),
              delta,
            );
          }, 0);
        }
      };

      const clearValue = () => {
        props.setState(
          SumAbstractRendererState.Updaters.Core.customFormState((__) => ({
            ...__,
            left: UnitFormState.Updaters.Core.commonFormState((___) => ({
              ...___,
              modifiedByUser: true,
            }))(__.left as UnitFormState),
          })),
        );
        const delta: DispatchDelta<DispatchPassthroughFormFlags> = {
          kind: "SumReplace",
          replace: PredicateValue.Default.sum(
            Sum.Default.left(PredicateValue.Default.unit()),
          ),
          state: {
            commonFormState: props.context.commonFormState,
            customFormState: props.context.customFormState,
          },
          type: props.context.type,
          flags: {
            test: true,
          },
          sourceAncestorLookupTypeNames: props.context.lookupTypeAncestorNames,
        };
        setTimeout(() => {
          props.foreignMutations.onChange(
            Option.Default.some(
              replaceWith(
                PredicateValue.Default.sum(
                  Sum.Default.left(PredicateValue.Default.unit()),
                ),
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
    },
  },
  unit: {
    defaultUnit: () => (props) => {
      return (
        <>
          {props.context.label && <h3>{props.context.label}</h3>}
          Unit field
        </>
      );
    },
  },
  readOnly: {
    ReadOnly: () => (props) => {
      return (
        <>
          {props.embeddedTemplate({
            ...props,
            view: unit,
          })}
        </>
      );
    },
  },
  sumUnitDate: {
    maybeDate: () => (props) => {
      const displayValue =
        props.context.value.value.kind == "l"
          ? ""
          : props.context.customFormState.right.commonFormState.modifiedByUser
            ? (props.context.customFormState.right as DateFormState)
                .customFormState.possiblyInvalidInput
            : (props.context.value.value.value as Date)
                .toISOString()
                .slice(0, 10);

      const setNewValue = (_: Maybe<string>) => {
        props.setState(
          SumAbstractRendererState.Updaters.Core.customFormState((__) => ({
            ...__,
            right: DateFormState.Updaters.Core.customFormState.children
              .possiblyInvalidInput(replaceWith(_))
              .then(
                DateFormState.Updaters.Core.commonFormState((___) => ({
                  ...___,
                  modifiedByUser: true,
                })),
              )(__.right as DateFormState),
          })),
        );
        const newValue = _ == undefined ? _ : new Date(_);
        if (!(newValue == undefined || isNaN(newValue.getTime()))) {
          const delta: DispatchDelta<DispatchPassthroughFormFlags> = {
            kind: "SumReplace",
            replace: PredicateValue.Default.sum(Sum.Default.right(newValue)),
            state: {
              commonFormState: props.context.commonFormState,
              customFormState: props.context.customFormState,
            },
            type: props.context.type,
            flags: {
              test: true,
            },
            sourceAncestorLookupTypeNames:
              props.context.lookupTypeAncestorNames,
          };
          setTimeout(() => {
            props.foreignMutations.onChange(
              Option.Default.some(
                replaceWith(
                  PredicateValue.Default.sum(Sum.Default.right(newValue)),
                ),
              ),
              delta,
            );
          }, 0);
        }
      };

      const clearValue = () => {
        props.setState(
          SumAbstractRendererState.Updaters.Core.customFormState((__) => ({
            ...__,
            left: UnitFormState.Updaters.Core.commonFormState((___) => ({
              ...___,
              modifiedByUser: true,
            }))(__.left as UnitFormState),
          })),
        );
        const delta: DispatchDelta<DispatchPassthroughFormFlags> = {
          kind: "SumReplace",
          replace: PredicateValue.Default.sum(
            Sum.Default.left(PredicateValue.Default.unit()),
          ),
          state: {
            commonFormState: props.context.commonFormState,
            customFormState: props.context.customFormState,
          },
          type: props.context.type,
          flags: {
            test: true,
          },
          sourceAncestorLookupTypeNames: props.context.lookupTypeAncestorNames,
        };
        setTimeout(() => {
          props.foreignMutations.onChange(
            Option.Default.some(
              replaceWith(
                PredicateValue.Default.sum(
                  Sum.Default.left(PredicateValue.Default.unit()),
                ),
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
    },
  },
};
