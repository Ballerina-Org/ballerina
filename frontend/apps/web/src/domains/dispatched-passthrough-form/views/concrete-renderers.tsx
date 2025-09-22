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
  MapRepo,
  id,
  FilterTypeKind,
  ListRepo,
  BasicUpdater,
  ValueTuple,
} from "ballerina-core";
import { OrderedMap, Map, Set, List } from "immutable";
import { useEffect, useState } from "react";
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

export type ColumnFilter = {
  kind: FilterTypeKind;
  value: PredicateValue;
};

export type ColumnFilters = Map<string, List<ColumnFilter>>;

export const DispatchPassthroughFormConcreteRenderers: ConcreteRenderers<
  DispatchPassthroughFormInjectedTypes,
  DispatchPassthroughFormFlags,
  DispatchPassthroughFormCustomPresentationContext,
  DispatchPassthroughFormExtraContext
> = {
  one: {
    admin: () => (props) => {
      if (PredicateValue.Operations.IsUnit(props.context.value)) {
        return <></>;
      }

      if (!PredicateValue.Operations.IsOption(props.context.value)) {
        return <></>;
      }

      if (!props.context.value.isSome) {
        console.debug("loading");
        return <>Loading...</>;
      }

      const optionValue = props.context.value.value;

      if (!PredicateValue.Operations.IsRecord(optionValue)) {
        console.error("one option inner value is not a record", optionValue);
        return <></>;
      }

      if (props.context.customFormState.stream.kind === "r") {
        // TODO: check this
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
          {props.DetailsRenderer?.(undefined)({
            ...props,
            context: {
              ...props.context,
            },
            foreignMutations: {
              ...props.foreignMutations,
            },
            view: unit,
          })}
          <p>PreviewRenderer</p>
          <button
            disabled={props.context.disabled}
            onClick={() => props.foreignMutations.toggleOpen()}
          >
            {props?.PreviewRenderer &&
              props.PreviewRenderer(optionValue)("unique-id")(undefined)?.({
                ...props,
                context: {
                  ...props.context,
                },
                foreignMutations: {
                  ...props.foreignMutations,
                },
                view: unit,
              })}
            {props.context.customFormState.status == "open" ? "➖" : "➕"}
          </button>
          {props.context.customFormState.status == "closed" ? (
            <></>
          ) : (
            <>
              <input
                disabled={props.context.disabled}
                value={
                  props.context.customFormState.streamParams.value[0].get(
                    "search",
                  ) ?? ""
                }
                onChange={(e) =>
                  props.foreignMutations.setStreamParam(
                    "search",
                    e.currentTarget.value,
                    true,
                  )
                }
              />
              <ul>
                {props.context.customFormState.stream.value.loadedElements
                  .entrySeq()
                  .map(([key, chunk]) =>
                    chunk.data.valueSeq().map((element: any) => {
                      return (
                        <li>
                          <button
                            disabled={props.context.disabled}
                            onClick={() =>
                              props.foreignMutations.select(element, undefined)
                            }
                          >
                            <div
                              onClick={() =>
                                props.foreignMutations.select(
                                  element,
                                  undefined,
                                )
                              }
                              style={{
                                display: "flex",
                                flexDirection: "row",
                                gap: "10px",
                              }}
                            />
                            {props?.PreviewRenderer &&
                              props.PreviewRenderer(element)(key.toString())(
                                undefined,
                              )?.({
                                ...props,
                                context: {
                                  ...props.context,
                                },
                                foreignMutations: {
                                  ...props.foreignMutations,
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
            disabled={props.context.hasMoreValues == false}
            onClick={() => props.foreignMutations.loadMore()}
          >
            ⋯
          </button>
        </div>
      );
    },
    partialAdmin: () => (props) => {
      if (props.context.customFormState.stream.kind === "r") {
        // TODO: check this
        return <></>;
      }

      if (PredicateValue.Operations.IsUnit(props.context.value)) {
        return (
          <>
            <p>one admin renderer</p>
            <p>DetailsRenderer</p>
            {props.DetailsRenderer?.(undefined)({
              ...props,
              context: {
                ...props.context,
              },
              foreignMutations: {
                ...props.foreignMutations,
              },
              view: unit,
            })}
            <p>PreviewRenderer</p>
            <button
              disabled={props.context.disabled}
              onClick={() => props.foreignMutations.toggleOpen()}
            >
              {props.context.customFormState.status == "open" ? "➖" : "➕"}
            </button>
            {props.context.customFormState.status == "closed" ? (
              <></>
            ) : (
              <>
                <input
                  disabled={props.context.disabled}
                  value={
                    props.context.customFormState.streamParams.value[0].get(
                      "search",
                    ) ?? ""
                  }
                  onChange={(e) =>
                    props.foreignMutations.setStreamParam(
                      "search",
                      e.currentTarget.value,
                      true,
                    )
                  }
                />
                <ul>
                  {props.context.customFormState.stream.value.loadedElements
                    .entrySeq()
                    .map(([key, chunk]) =>
                      chunk.data.valueSeq().map((element: any, idx: number) => {
                        return (
                          <li>
                            <button
                              disabled={props.context.disabled}
                              onClick={() =>
                                props.foreignMutations.select(
                                  element,
                                  undefined,
                                )
                              }
                            >
                              <div
                                onClick={() =>
                                  props.foreignMutations.select(
                                    element,
                                    undefined,
                                  )
                                }
                                style={{
                                  display: "flex",
                                  flexDirection: "row",
                                  gap: "10px",
                                }}
                              />
                              {props?.PreviewRenderer &&
                                props?.PreviewRenderer(element)(key.toString())(
                                  undefined,
                                )?.({
                                  ...props,
                                  context: {
                                    ...props.context,
                                  },
                                  foreignMutations: {
                                    ...props.foreignMutations,
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
              disabled={props.context.hasMoreValues == false}
              onClick={() => props.foreignMutations.loadMore()}
            >
              ⋯
            </button>
          </>
        );
      }

      if (!PredicateValue.Operations.IsOption(props.context.value)) {
        return <></>;
      }

      if (!props.context.value.isSome) {
        console.debug("loading");
        return <>Loading...</>;
      }

      const optionValue = props.context.value.value;

      if (!PredicateValue.Operations.IsRecord(optionValue)) {
        console.error("one option inner value is not a record", optionValue);
        return <></>;
      }

      return (
        <>
          <p>one admin renderer</p>
          <p>DetailsRenderer</p>
          {props.DetailsRenderer?.(undefined)({
            ...props,
            context: {
              ...props.context,
            },
            foreignMutations: {
              ...props.foreignMutations,
            },
            view: unit,
          })}
          <p>PreviewRenderer</p>
          <button
            disabled={props.context.disabled}
            onClick={() => props.foreignMutations.toggleOpen()}
          >
            {props?.PreviewRenderer &&
              props.PreviewRenderer(optionValue)("unique-id")(undefined)?.({
                ...props,
                context: {
                  ...props.context,
                },
                foreignMutations: {
                  ...props.foreignMutations,
                },
                view: unit,
              })}
            {props.context.customFormState.status == "open" ? "➖" : "➕"}
          </button>
          {props.context.customFormState.status == "closed" ? (
            <></>
          ) : (
            <>
              <input
                disabled={props.context.disabled}
                value={
                  props.context.customFormState.streamParams.value[0].get(
                    "search",
                  ) ?? ""
                }
                onChange={(e) =>
                  props.foreignMutations.setStreamParam(
                    "search",
                    e.currentTarget.value,
                    true,
                  )
                }
              />
              <ul>
                {props.context.customFormState.stream.value.loadedElements
                  .entrySeq()
                  .map(([key, chunk]) =>
                    chunk.data.valueSeq().map((element: any) => {
                      return (
                        <li>
                          <button
                            disabled={props.context.disabled}
                            onClick={() =>
                              props.foreignMutations.select(element, undefined)
                            }
                          >
                            <div
                              onClick={() =>
                                props.foreignMutations.select(
                                  element,
                                  undefined,
                                )
                              }
                              style={{
                                display: "flex",
                                flexDirection: "row",
                                gap: "10px",
                              }}
                            />
                            {props?.PreviewRenderer &&
                              props.PreviewRenderer(element)(key.toString())(
                                undefined,
                              )?.({
                                ...props,
                                context: {
                                  ...props.context,
                                },
                                foreignMutations: {
                                  ...props.foreignMutations,
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
            disabled={props.context.hasMoreValues == false}
            onClick={() => props.foreignMutations.loadMore()}
          >
            ⋯
          </button>
        </>
      );
    },
    bestFriend: () => (props) => {
      const maybeOption = props.context.value;
      if (PredicateValue.Operations.IsUnit(maybeOption)) {
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: value option expected but got unit</>
          </>
        );
      }

      if (!PredicateValue.Operations.IsOption(maybeOption)) {
        console.error("value option expected but got", maybeOption);
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: value option expected but got</>
          </>
        );
      }

      if (!maybeOption.isSome) {
        console.debug("loading");
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Loading...</>
          </>
        );
      }

      const optionValue = maybeOption.value;

      if (!PredicateValue.Operations.IsRecord(optionValue)) {
        console.error("one option inner value is not a record", optionValue);
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: one option inner value is not a record</>
          </>
        );
      }

      if (props.context.customFormState.stream.kind === "r") {
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: stream missing</>
          </>
        );
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
          <h2>{props.context.label}</h2>
          <li>
            {props.DetailsRenderer?.(undefined)({
              ...props,
              context: {
                ...props.context,
              },
              foreignMutations: {
                ...props.foreignMutations,
              },
              view: unit,
            })}
          </li>
          <li>
            <button
              disabled={props.context.disabled}
              onClick={() => props.foreignMutations.toggleOpen()}
            >
              {props?.PreviewRenderer &&
                props?.PreviewRenderer(optionValue)("unique-id")(undefined)?.({
                  ...props,
                  context: {
                    ...props.context,
                  },
                  foreignMutations: {
                    ...props.foreignMutations,
                  },
                  view: unit,
                })}
              {props.context.customFormState.status == "open" ? "➖" : "➕"}
            </button>
          </li>
          {props.context.customFormState.status == "closed" ? (
            <></>
          ) : (
            <>
              <input
                disabled={props.context.disabled}
                value={
                  props.context.customFormState.streamParams.value[0].get(
                    "search",
                  ) ?? ""
                }
                onChange={(e) =>
                  props.foreignMutations.setStreamParam(
                    "search",
                    e.currentTarget.value,
                    true,
                  )
                }
              />
              <ul>
                {props.context.customFormState.stream.value.loadedElements
                  .entrySeq()
                  .map(([key, chunk]) =>
                    chunk.data.valueSeq().map((element: any) => {
                      return (
                        <li>
                          <button
                            disabled={props.context.disabled}
                            onClick={() =>
                              props.foreignMutations.select(element, undefined)
                            }
                          >
                            <div
                              onClick={() =>
                                props.foreignMutations.select(
                                  element,
                                  undefined,
                                )
                              }
                              style={{
                                display: "flex",
                                flexDirection: "row",
                                gap: "10px",
                              }}
                            />
                            {props?.PreviewRenderer &&
                              props.PreviewRenderer(element)(key.toString())(
                                undefined,
                              )?.({
                                ...props,
                                context: {
                                  ...props.context,
                                },
                                foreignMutations: {
                                  ...props.foreignMutations,
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
            disabled={props.context.hasMoreValues == false}
            onClick={() => props.foreignMutations.loadMore()}
          >
            ⋯
          </button>
        </div>
      );
    },
    eagerEditableOne: () => (props) => {
      const maybeOption = props.context.value;
      if (PredicateValue.Operations.IsUnit(maybeOption)) {
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: value option expected but got unit</>
          </>
        );
      }

      if (!PredicateValue.Operations.IsOption(maybeOption)) {
        console.error("value option expected but got", maybeOption);
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: value option expected but got</>
          </>
        );
      }

      if (!maybeOption.isSome) {
        console.error("no value for eager editable one");
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: no value for eager editable one</>
          </>
        );
      }

      const optionValue = maybeOption.value;

      if (!PredicateValue.Operations.IsRecord(optionValue)) {
        console.error("one option inner value is not a record", optionValue);
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: one option inner value is not a record</>
          </>
        );
      }

      if (props.context.customFormState.stream.kind === "r") {
        console.error("stream missing from eager editable one");
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: stream missing</>
          </>
        );
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
          <h2>{props.context.label}</h2>
          <li>
            {props.DetailsRenderer?.(undefined)({
              ...props,
              context: {
                ...props.context,
              },
              foreignMutations: {
                ...props.foreignMutations,
              },
              view: unit,
            })}
          </li>
          <li>
            <button
              disabled={props.context.disabled}
              onClick={() => props.foreignMutations.toggleOpen()}
            >
              {props?.PreviewRenderer &&
                props?.PreviewRenderer(optionValue)("unique-id")(undefined)?.({
                  ...props,
                  context: {
                    ...props.context,
                  },
                  foreignMutations: {
                    ...props.foreignMutations,
                  },
                  view: unit,
                })}
              {props.context.customFormState.status == "open" ? "➖" : "➕"}
            </button>
          </li>
          {props.context.customFormState.status == "closed" ? (
            <></>
          ) : (
            <>
              <input
                disabled={props.context.disabled}
                value={
                  props.context.customFormState.streamParams.value[0].get(
                    "search",
                  ) ?? ""
                }
                onChange={(e) =>
                  props.foreignMutations.setStreamParam(
                    "search",
                    e.currentTarget.value,
                    true,
                  )
                }
              />
              <ul>
                {props.context.customFormState.stream.value.loadedElements
                  .entrySeq()
                  .map(([key, chunk]) =>
                    chunk.data.valueSeq().map((element: any) => {
                      return (
                        <li>
                          <button
                            disabled={props.context.disabled}
                            onClick={() =>
                              props.foreignMutations.select(element, undefined)
                            }
                          >
                            <div
                              onClick={() =>
                                props.foreignMutations.select(
                                  element,
                                  undefined,
                                )
                              }
                              style={{
                                display: "flex",
                                flexDirection: "row",
                                gap: "10px",
                              }}
                            />
                            {props?.PreviewRenderer &&
                              props.PreviewRenderer(element)(key.toString())(
                                undefined,
                              )?.({
                                ...props,
                                context: {
                                  ...props.context,
                                },
                                foreignMutations: {
                                  ...props.foreignMutations,
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
            disabled={props.context.hasMoreValues == false}
            onClick={() => props.foreignMutations.loadMore()}
          >
            ⋯
          </button>
        </div>
      );
    },
    lazyReadonlyOne: () => (props) => {
      const maybeOption = props.context.value;
      if (PredicateValue.Operations.IsUnit(maybeOption)) {
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: value option expected but got unit</>
          </>
        );
      }

      if (!PredicateValue.Operations.IsOption(maybeOption)) {
        console.error("value option expected but got", maybeOption);
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: value option expected but got</>
          </>
        );
      }

      if (!maybeOption.isSome) {
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Loading...</>
          </>
        );
      }

      const optionValue = maybeOption.value;

      if (!PredicateValue.Operations.IsRecord(optionValue)) {
        console.error("one option inner value is not a record", optionValue);
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: one option inner value is not a record</>
          </>
        );
      }

      if (props.context.customFormState.stream.kind === "l") {
        console.error("stream incorrectly provided for lazy readonly one");
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: stream incorrectly provided for lazy readonly one</>
          </>
        );
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
          <h2>{props.context.label}</h2>
          <li>
            {props.DetailsRenderer?.(undefined)({
              ...props,
              context: {
                ...props.context,
              },
              foreignMutations: {
                ...props.foreignMutations,
              },
              view: unit,
            })}
          </li>
          <li>
            <button
              disabled={true}
              onClick={() => props.foreignMutations.toggleOpen()}
            >
              {props?.PreviewRenderer &&
                props?.PreviewRenderer(optionValue)("unique-id")(undefined)?.({
                  ...props,
                  context: {
                    ...props.context,
                  },
                  foreignMutations: {
                    ...props.foreignMutations,
                  },
                  view: unit,
                })}
            </button>
          </li>
        </div>
      );
    },
    eagerReadonlyOne: () => (props) => {
      const maybeOption = props.context.value;
      if (PredicateValue.Operations.IsUnit(maybeOption)) {
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: value option expected but got unit</>
          </>
        );
      }

      if (!PredicateValue.Operations.IsOption(maybeOption)) {
        console.error("value option expected but got", maybeOption);
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: value option expected but got</>
          </>
        );
      }

      if (!maybeOption.isSome) {
        console.error("no value for eager readonly one");
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: no value for eager readonly one</>
          </>
        );
      }

      const optionValue = maybeOption.value;

      if (!PredicateValue.Operations.IsRecord(optionValue)) {
        console.error("one option inner value is not a record", optionValue);
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: one option inner value is not a record</>
          </>
        );
      }

      console.debug("bestFriend", props);

      if (props.context.customFormState.stream.kind === "l") {
        console.error("stream incorrectly provided for eager readonly one");
        return (
          <>
            <h2>{props.context.label}</h2>
            <>Error: stream incorrectly provided for eager readonly one</>
          </>
        );
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
          <h2>{props.context.label}</h2>
          <li>
            {props.DetailsRenderer?.(undefined)({
              ...props,
              context: {
                ...props.context,
              },
              foreignMutations: {
                ...props.foreignMutations,
              },
              view: unit,
            })}
          </li>
          <li>
            <button
              disabled={true}
              onClick={() => props.foreignMutations.toggleOpen()}
            >
              {props?.PreviewRenderer &&
                props?.PreviewRenderer(optionValue)("unique-id")(undefined)?.({
                  ...props,
                  context: {
                    ...props.context,
                  },
                  foreignMutations: {
                    ...props.foreignMutations,
                  },
                  view: unit,
                })}
            </button>
          </li>
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
      console.log({
        userDetails: props,
      });

      if (PredicateValue.Operations.IsUnit(props.context.value)) {
        return <>select user (unit renderer)</>;
      }

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
    incomeLineItem: () => (props) => {
      const value = props.context.value;
      if (PredicateValue.Operations.IsUnit(value)) {
        return (
          <tr>
            {props.FieldLabels.map((label) => (
              <th>{label}</th>
            ))}
          </tr>
        );
      }
      return (
        <tr>
          <>
            {value.fields.keySeq().map((fieldName) => (
              <td key={`${props.context.domNodeId}`}>
                {props.EmbeddedFields.get(fieldName)!(undefined)({
                  ...props,
                  context: {
                    ...props.context,
                  },
                  view: unit,
                })}
              </td>
            ))}
          </>
        </tr>
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
      const [colFilterDisplays, setColFilterDisplays] = useState<
        Map<string, boolean>
      >(Map(props.AllowedFilters.map((_) => false)));

      // TODO: initialise with response from new endpoint
      const [colFilters, setColFilters] = useState<ColumnFilters>(
        Map(props.AllowedFilters.map((_) => List([]))),
      );

      useEffect(() => {
        const filters = colFilters.map((_) =>
          _.map(({ kind, value }) =>
            PredicateValue.Operations.KindAndValueToFilter(kind, value),
          ),
        );
        props.foreignMutations.updateFilters(filters, true);
      }, [colFilters]);

      const handleFilterValueChange = (
        columnName: string,
        filterIndex: number,
        updater: BasicUpdater<PredicateValue>,
      ) => {
        const filterValue =
          colFilters.get(columnName)!.get(filterIndex)?.value ??
          props.AllowedFilters.get(columnName)!.GetDefaultValue();

        const filterKind =
          colFilters.get(columnName)!.get(filterIndex)?.kind ??
          props.AllowedFilters.get(columnName)!.filters[0].kind;

        const newFilter = {
          kind: filterKind,
          value: updater(filterValue),
        };

        if (colFilters.get(columnName)!.get(filterIndex)) {
          setColFilters(
            MapRepo.Updaters.update(
              columnName,
              ListRepo.Updaters.update(filterIndex, replaceWith(newFilter)),
            ),
          );
        } else {
          setColFilters(
            MapRepo.Updaters.update(
              columnName,
              ListRepo.Updaters.push(newFilter),
            ),
          );
        }
      };

      const handleFilterKindChange = (
        columnName: string,
        filterIndex: number,
        filterKind: FilterTypeKind,
      ) => {
        const filterValue =
          colFilters.get(columnName)!.get(filterIndex)?.value ??
          props.AllowedFilters.get(columnName)!.GetDefaultValue();

        const newFilter = {
          kind: filterKind,
          value: filterValue,
        };

        if (colFilters.get(columnName)!.get(filterIndex)) {
          setColFilters(
            MapRepo.Updaters.update(
              columnName,
              ListRepo.Updaters.update(filterIndex, replaceWith(newFilter)),
            ),
          );
        } else {
          setColFilters(
            MapRepo.Updaters.update(
              columnName,
              ListRepo.Updaters.push(newFilter),
            ),
          );
        }
      };

      const handleFilterRemove = (columnName: string, filterIndex: number) => {
        setColFilters(
          MapRepo.Updaters.update(
            columnName,
            ListRepo.Updaters.remove(filterIndex),
          ),
        );
      };

      const handleSortingChange = (columnName: string) => {
        if (props.AllowedSorting.includes(columnName)) {
          if (props.context.customFormState.sorting.has(columnName)) {
            if (
              props.context.customFormState.sorting.get(columnName) ==
              "Ascending"
            ) {
              props.foreignMutations.addSorting(columnName, "Descending");
            } else {
              props.foreignMutations.addSorting(columnName, "Ascending");
            }
          } else {
            props.foreignMutations.addSorting(columnName, "Ascending");
          }
        }
      };

      const handleSortingRemove = (columnName: string) => {
        if (props.AllowedSorting.includes(columnName)) {
          props.foreignMutations.removeSorting(columnName);
        }
      };

      return (
        <>
          <h3>{props.context.label}</h3>
          <div
            style={{
              display: "flex",
              flexDirection: "row",
              gap: "10px",
            }}
          >
            {props.HighlightedFilters.map((filterName: string) => (
              <div
                style={{
                  display: "flex",
                  flexDirection: "row",
                  gap: "10px",
                  alignItems: "center",
                  justifyContent: "center",
                }}
              >
                <select
                  style={{ height: "30px", width: "100px" }}
                  onChange={(_) =>
                    handleFilterKindChange(
                      filterName,
                      0,
                      _.currentTarget.value as FilterTypeKind,
                    )
                  }
                >
                  {props.AllowedFilters.get(filterName)!.filters.map(
                    (filter) => (
                      <option value={filter.kind}>{filter.kind}</option>
                    ),
                  )}
                </select>
                {props.AllowedFilters.get(filterName)!.template(0)({
                  ...props,
                  context: {
                    ...props.context,
                    value:
                      colFilters.get(filterName)!.get(0)?.value ??
                      props.AllowedFilters.get(filterName)!.GetDefaultValue(),
                  },
                  foreignMutations: {
                    onChange: (updaterOption) => {
                      if (updaterOption.kind == "r") {
                        handleFilterValueChange(
                          filterName,
                          0,
                          updaterOption.value,
                        );
                      }
                    },
                  },
                  view: unit,
                })}
                <button onClick={() => handleFilterRemove(filterName, 0)}>
                  ❌
                </button>
              </div>
            ))}
          </div>
          <div
            style={{
              display: "flex",
              flexDirection: "row",
              gap: "10px",
            }}
          ></div>
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
                      <th style={{ border: "1px solid black" }}>
                        <div
                          style={{
                            display: "flex",
                            flexDirection: "row",
                            gap: "10px",
                            alignItems: "center",
                            justifyContent: "center",
                          }}
                        >
                          {header}
                          {props.AllowedFilters.has(header) && (
                            <button
                              onClick={() =>
                                setColFilterDisplays(
                                  MapRepo.Updaters.update(header, (_) => !_),
                                )
                              }
                            >
                              🔎
                            </button>
                          )}
                          {props.AllowedSorting.includes(header) &&
                            props.context.customFormState.sorting.get(
                              header,
                            ) && (
                              <>
                                <button
                                  onClick={() => handleSortingChange(header)}
                                >
                                  {props.context.customFormState.sorting.get(
                                    header,
                                  ) == "Ascending"
                                    ? "⬆️"
                                    : "⬇️"}
                                </button>
                                <button
                                  onClick={() => handleSortingRemove(header)}
                                >
                                  ❌
                                </button>
                              </>
                            )}
                          {props.AllowedSorting.includes(header) &&
                            !props.context.customFormState.sorting.get(
                              header,
                            ) && (
                              <button
                                onClick={() => handleSortingChange(header)}
                              >
                                ⬆️
                              </button>
                            )}
                        </div>
                        {colFilterDisplays.get(header) && (
                          <div
                            style={{
                              display: "flex",
                              flexDirection: "row",
                              gap: "10px",
                              alignItems: "center",
                              justifyContent: "center",
                            }}
                          >
                            <select
                              style={{ height: "30px", width: "100px" }}
                              onChange={(_) =>
                                handleFilterKindChange(
                                  header,
                                  0,
                                  _.currentTarget.value as FilterTypeKind,
                                )
                              }
                            >
                              {props.AllowedFilters.get(header)!.filters.map(
                                (filter) => (
                                  <option value={filter.kind}>
                                    {filter.kind}
                                  </option>
                                ),
                              )}
                            </select>
                            {props.AllowedFilters.get(header)!.template(0)({
                              ...props,
                              context: {
                                ...props.context,
                                value:
                                  colFilters.get(header)!.get(0)?.value ??
                                  props.AllowedFilters.get(
                                    header,
                                  )!.GetDefaultValue(),
                              },
                              foreignMutations: {
                                onChange: (updaterOption) => {
                                  if (updaterOption.kind == "r") {
                                    handleFilterValueChange(
                                      header,
                                      0,
                                      updaterOption.value,
                                    );
                                  }
                                },
                              },
                              view: unit,
                            })}
                            <button
                              onClick={() => handleFilterRemove(header, 0)}
                            >
                              ❌
                            </button>
                          </div>
                        )}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {props.TableData.entrySeq()
                    .toArray()
                    .map(([id, row], idx) => {
                      const isSelected =
                        props.context.customFormState.selectedDetailRow == id;

                      return (
                        <tr style={{ border: "1px solid black" }}>
                          <button
                            onClick={() =>
                              isSelected
                                ? props.foreignMutations.clearDetailView()
                                : props.foreignMutations.selectDetailView(id)
                            }
                          >
                            {isSelected ? "hide details" : "show details"}
                          </button>
                          <button
                            onClick={() =>
                              props.foreignMutations.remove &&
                              props.foreignMutations.remove(id, undefined)
                            }
                          >
                            remove
                          </button>
                          <button
                            onClick={() =>
                              props.foreignMutations.duplicate &&
                              props.foreignMutations.duplicate(id, undefined)
                            }
                          >
                            duplicate
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
                              onClick={() =>
                                props.foreignMutations.selectRow(id)
                              }
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
                      );
                    })}
                </tbody>
              </table>
              <button onClick={() => props.foreignMutations.loadMore()}>
                Load More
              </button>
            </div>

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
                props.context.customFormState.selectedDetailRow &&
                props.DetailsRenderer(undefined)({
                  ...props,
                  context: {
                    ...props.context,
                    customFormState: {
                      ...props.context.customFormState,
                      selectedDetailRow:
                        props.context.customFormState.selectedDetailRow,
                    },
                  },
                  view: unit,
                })}
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
          checked={
            PredicateValue.Operations.IsBoolean(props.context.value)
              ? props.context.value
              : false
          }
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
          checked={
            PredicateValue.Operations.IsBoolean(props.context.value)
              ? props.context.value
              : false
          }
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
      if (PredicateValue.Operations.IsUnit(props.context.value)) {
        return <></>;
      }

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
      const value = props.context.value;
      if (PredicateValue.Operations.IsUnit(value)) {
        console.error(`Non partial list renderer called with unit value`);
        return <></>;
      }
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
            {value.values.map((_, elementIndex) => {
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
                            elementIndex == value.values.size - 1,
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
            {props.embeddedPlaceholderElementTemplate()(undefined)({
              ...props,
              context: {
                ...props.context,
                customPresentationContext: {
                  listElement: {
                    isLastListElement: false,
                  },
                },
              },
              view: unit,
              foreignMutations: {
                ...props.foreignMutations,
                onChange: (upd, delta) => {
                  props.foreignMutations.add?.(undefined)(
                    upd.kind == "l" ? undefined : upd.value,
                  );
                },
              },
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
    partialList: () => (props) => {
      const value = props.context.value;

      if (PredicateValue.Operations.IsUnit(value)) {
        return (
          <>
            {props.embeddedElementTemplate(0)(undefined)({
              ...props,
              context: {
                ...props.context,
              },
              view: unit,
            })}
          </>
        );
      }

      return (
        <div>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.tooltip && <p>{props.context.tooltip}</p>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}

          {value.values.map((_, elementIndex) => {
            return (
              <>
                {props.embeddedElementTemplate(elementIndex)(undefined)({
                  ...props,
                  context: {
                    ...props.context,
                    customPresentationContext: {
                      listElement: {
                        isLastListElement:
                          elementIndex == value.values.size - 1,
                      },
                    },
                  },
                  view: unit,
                })}
                <div style={{ display: "flex" }}>
                  {props.foreignMutations.remove && (
                    <button
                      onClick={() =>
                        props.foreignMutations.remove!(elementIndex, undefined)
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
              </>
            );
          })}

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
    listWithPartialList: () => (props) => {
      const value = props.context.value;

      if (PredicateValue.Operations.IsUnit(value)) {
        console.error(`Non partial list renderer called with unit value`);
        return <></>;
      }

      return (
        <div style={{ border: "1px solid darkblue" }}>
          {props.context.label && <h3>{props.context.label}</h3>}
          {props.context.tooltip && <p>{props.context.tooltip}</p>}
          {props.context.details && (
            <p>
              <em>{props.context.details}</em>
            </p>
          )}
          <table>
            <thead>
              {props.embeddedElementTemplate(0)(undefined)({
                ...props,
                context: {
                  ...props.context,
                  value: PredicateValue.Default.unit(),
                },
                view: unit,
              })}
            </thead>

            <tr>
              {value.values.map((_, elementIndex) => {
                return (
                  <>
                    <>
                      {props.embeddedElementTemplate(elementIndex)(undefined)({
                        ...props,
                        context: {
                          ...props.context,
                          customPresentationContext: {
                            listElement: {
                              isLastListElement:
                                elementIndex == value.values.size - 1,
                            },
                          },
                        },
                        view: unit,
                      })}
                    </>
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
                  </>
                );
              })}
            </tr>
          </table>
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
      if (PredicateValue.Operations.IsUnit(props.context.value)) {
        return <></>;
      }

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
      if (PredicateValue.Operations.IsUnit(props.context.value)) {
        return <></>;
      }

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
      if (PredicateValue.Operations.IsUnit(props.context.value)) {
        return <></>;
      }

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
