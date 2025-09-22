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
} from "ballerina-core";
import { OrderedMap, Map, Set, List } from "immutable";
import React, { useEffect, useState } from "react";
import { DispatchPassthroughFormInjectedTypes } from "../injected-forms/category";
import {VscDiffAdded, VscDiffRemoved, VscNewFile, VscRefresh, VscSurroundWith} from "react-icons/vsc";

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

const layoutMode: "grid" | "flex" = "grid";

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
                  props.context.customFormState.streamParams.value.get(
                    "search",
                  ) ?? ""
                }
                onChange={(e) =>
                  props.foreignMutations.setStreamParam(
                    "search",
                    e.currentTarget.value,
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
                    props.context.customFormState.streamParams.value.get(
                      "search",
                    ) ?? ""
                  }
                  onChange={(e) =>
                    props.foreignMutations.setStreamParam(
                      "search",
                      e.currentTarget.value,
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
                  props.context.customFormState.streamParams.value.get(
                    "search",
                  ) ?? ""
                }
                onChange={(e) =>
                  props.foreignMutations.setStreamParam(
                    "search",
                    e.currentTarget.value,
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
                  props.context.customFormState.streamParams.value.get(
                    "search",
                  ) ?? ""
                }
                onChange={(e) =>
                  props.foreignMutations.setStreamParam(
                    "search",
                    e.currentTarget.value,
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
    bestFriendDaisy: () => (props) => {
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
              <>

                      <fieldset className="fieldset bg-base-200 border-base-300 rounded-box w-xs border p-4">
                          <legend className="fieldset-legend">details</legend>
                          <div className="tooltip">
                              <div className="tooltip-content">
                                  <div className="animate-bounce text-orange-400 -rotate-10 text-2xl font-black">{props.context.tooltip}
                                  </div>
                              </div>
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
                          </div>
   
                      </fieldset>

                      <div className="card-body">
                          <fieldset className="fieldset bg-base-200 border-base-300 rounded-box w-xs border p-4">
                          <legend className="fieldset-legend">{props.context.label}</legend>

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
                                  })}</fieldset>
                              <div className="card-actions justify-end">
                                  <button className="btn btn-primary" disabled={props.context.disabled}
                                          onClick={() => props.foreignMutations.toggleOpen()}>Change
                                  </button>
                              </div>
                          </div>

                  <ul className="timeline timeline-snap-icon max-md:timeline-compact timeline-vertical">
                      <li>
                          <div className="timeline-middle">
                              <svg
                                  xmlns="http://www.w3.org/2000/svg"
                                  viewBox="0 0 20 20"
                                  fill="currentColor"
                                  className="h-5 w-5"
                              >
                                  <path
                                      fillRule="evenodd"
                                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                                      clipRule="evenodd"
                                  />
                              </svg>
                          </div>
                          <div className="timeline-start mb-10 md:text-end">
                              <time className="font-mono italic">1984</time>
                              <div className="text-lg font-black">First Macintosh computer</div>
                              The Apple Macintosh—later rebranded as the Macintosh 128K—is the original Apple Macintosh
                              personal computer. It played a pivotal role in establishing desktop publishing as a general
                              office function. The motherboard, a 9 in (23 cm) CRT monitor, and a floppy drive were housed
                              in a beige case with integrated carrying handle; it came with a keyboard and single-button
                              mouse.
                          </div>
                          <hr />
                      </li>
                      <li>
                          <hr />
                          <div className="timeline-middle">
                              <svg
                                  xmlns="http://www.w3.org/2000/svg"
                                  viewBox="0 0 20 20"
                                  fill="currentColor"
                                  className="h-5 w-5"
                              >
                                  <path
                                      fillRule="evenodd"
                                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                                      clipRule="evenodd"
                                  />
                              </svg>
                          </div>
                          <div className="timeline-end md:mb-10">
                              <time className="font-mono italic">1998</time>
                              <div className="text-lg font-black">iMac</div>
                              iMac is a family of all-in-one Mac desktop computers designed and built by Apple Inc. It has
                              been the primary part of Apple's consumer desktop offerings since its debut in August 1998,
                              and has evolved through seven distinct forms
                          </div>
                          <hr />
                      </li>
                      <li>
                          <hr />
                          <div className="timeline-middle">
                              <svg
                                  xmlns="http://www.w3.org/2000/svg"
                                  viewBox="0 0 20 20"
                                  fill="currentColor"
                                  className="h-5 w-5"
                              >
                                  <path
                                      fillRule="evenodd"
                                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                                      clipRule="evenodd"
                                  />
                              </svg>
                          </div>
                          <div className="timeline-start mb-10 md:text-end">
                              <time className="font-mono italic">2001</time>
                              <div className="text-lg font-black">iPod</div>
                              The iPod is a discontinued series of portable media players and multi-purpose mobile devices
                              designed and marketed by Apple Inc. The first version was released on October 23, 2001, about
                              8+1⁄2 months after the Macintosh version of iTunes was released. Apple sold an estimated 450
                              million iPod products as of 2022. Apple discontinued the iPod product line on May 10, 2022. At
                              over 20 years, the iPod brand is the oldest to be discontinued by Apple
                          </div>
                          <hr />
                      </li>
                      <li>
                          <hr />
                          <div className="timeline-middle">
                              <svg
                                  xmlns="http://www.w3.org/2000/svg"
                                  viewBox="0 0 20 20"
                                  fill="currentColor"
                                  className="h-5 w-5"
                              >
                                  <path
                                      fillRule="evenodd"
                                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                                      clipRule="evenodd"
                                  />
                              </svg>
                          </div>
                          <div className="timeline-end md:mb-10">
                              <time className="font-mono italic">2007</time>
                              <div className="text-lg font-black">iPhone</div>
                              iPhone is a line of smartphones produced by Apple Inc. that use Apple's own iOS mobile
                              operating system. The first-generation iPhone was announced by then-Apple CEO Steve Jobs on
                              January 9, 2007. Since then, Apple has annually released new iPhone models and iOS updates. As
                              of November 1, 2018, more than 2.2 billion iPhones had been sold. As of 2022, the iPhone
                              accounts for 15.6% of global smartphone market share
                          </div>
                          <hr />
                      </li>
                      <li>
                          <hr />
                          <div className="timeline-middle">
                              <svg
                                  xmlns="http://www.w3.org/2000/svg"
                                  viewBox="0 0 20 20"
                                  fill="currentColor"
                                  className="h-5 w-5"
                              >
                                  <path
                                      fillRule="evenodd"
                                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                                      clipRule="evenodd"
                                  />
                              </svg>
                          </div>
                          <div className="timeline-start mb-10 md:text-end">
                              <time className="font-mono italic">2015</time>
                              <div className="text-lg font-black">Apple Watch</div>
                              The Apple Watch is a line of smartwatches produced by Apple Inc. It incorporates fitness
                              tracking, health-oriented capabilities, and wireless telecommunication, and integrates with
                              iOS and other Apple products and services
                          </div>
                      </li>
                  </ul>

                  {props.context.customFormState.status == "closed" ? (
                      <></>
                  ) : (
                      <>
                          <fieldset className="fieldset bg-base-200 border-base-300 rounded-box w-xs border p-4">
                              <legend className="fieldset-legend">Page title</legend>
                              <input type="text" className="input" placeholder="search" disabled={props.context.disabled}
                                     value={
                                         props.context.customFormState.streamParams.value.get(
                                             "search",
                                         ) ?? ""
                                     }
                                     onChange={(e) =>
                                         props.foreignMutations.setStreamParam(
                                             "search",
                                             e.currentTarget.value,
                                         )
                                     }/>
                              <p className="label">You can edit page title later on from settings</p>
                          </fieldset>

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
                  props.context.customFormState.streamParams.value.get(
                    "search",
                  ) ?? ""
                }
                onChange={(e) =>
                  props.foreignMutations.setStreamParam(
                    "search",
                    e.currentTarget.value,
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
      timelineEntry: () => (props) =>{
          
          return (           <li>
              <div className="timeline-middle">
                  <svg
                      xmlns="http://www.w3.org/2000/svg"
                      viewBox="0 0 20 20"
                      fill="currentColor"
                      className="h-5 w-5"
                  >
                      <path
                          fillRule="evenodd"
                          d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                          clipRule="evenodd"
                      />
                  </svg>
              </div>
              <div className="timeline-start mb-10 md:text-end">
                  <time className="font-mono italic">1984</time>
                  <div className="text-lg font-black">First Macintosh computer</div>
                  The Apple Macintosh—later rebranded as the Macintosh 128K—is the original Apple Macintosh
                  personal computer. It played a pivotal role in establishing desktop publishing as a general
                  office function. The motherboard, a 9 in (23 cm) CRT monitor, and a floppy drive were housed
                  in a beige case with integrated carrying handle; it came with a keyboard and single-button
                  mouse.
              </div>
              <hr />
          </li>)
      },
      tailwind: () => (props) => {
          return (
              <div className="w-full">
         
                  <div className="tabs tabs-box">
                      <input type="radio" name="my_tabs_1" className="tab" aria-label="Tab 1" />
                      <input type="radio" name="my_tabs_1" className="tab" aria-label="Tab 2" defaultChecked />
                      <input type="radio" name="my_tabs_1" className="tab" aria-label="Tab 3" />
                  </div>

                  {props.context.layout.valueSeq().map((tab, ti) => {
                      const colCount = tab.columns?.size || 1;
                      const tabContainerClass =
                          layoutMode === "grid"
                              ? "grid gap-4"
                              : "flex gap-4 flex-wrap";

                      const tabContainerStyle =
                          layoutMode === "grid"
                              ? { gridTemplateColumns: `repeat(${colCount}, minmax(0, 1fr))` }
                              : undefined;

                      const columnClass =
                          layoutMode === "grid"
                              ? "flex flex-col gap-4"
                              : "flex-1 basis-0 min-w-[16rem] flex flex-col gap-4"; 

                      return (
                          <div key={`tab-${ti}`} className="mb-6">
                              <div className={tabContainerClass} style={tabContainerStyle}>
                                  {tab.columns.valueSeq().map((column, ci) => (
                                      <div key={`col-${ti}-${ci}`} className={columnClass}>
                                          {column.groups.valueSeq().map((group, gi) => {
                                              return (
                                                  <div key={`grp-${ti}-${ci}-${gi}`} className="space-y-3">
                                                      {group
                                                          .filter((fieldName) => props.VisibleFieldKeys.has(fieldName))
                                                          .map((fieldName) => (
                                                              <div
                                                                  key={`f-${ti}-${ci}-${gi}-${fieldName}`}
                                                                  className="card card-bordered bg-base-100 shadow-sm"
                                                              >
                                                                  <div className="card-body p-4">
                                                                      <div className="form-control">
                                                                          {props.EmbeddedFields.get(fieldName)!(undefined)({
                                                                              ...props,
                                                                              context: {
                                                                                  ...props.context,
                                                                                  disabled: props.DisabledFieldKeys.has(fieldName),
                                                                              },
                                                                              view: unit,
                                                                          })}
                                                                      </div>
                                                                  </div>
                                                              </div>
                                                          ))}
                                                  </div>
                                              );
                                          })}
                                      </div>
                                  ))}
                              </div>
                          </div>
                      );
                  })}
              </div>
          );
      },  
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
        props.foreignMutations.updateFilters(filters);
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
    daisyToggle: () => (props) => (
        <fieldset className="fieldset bg-base-100 border-base-300 rounded-box border p-4">
            {props.context.label && <legend className="fieldset-legend">{props.context.label}</legend>}

            <label className="label">
                <input type="checkbox" className="toggle"
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
                {props.context.details}
            </label>
        </fieldset>

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
      daisyString: () => (props) => {
          return (
              <div className="tooltip" data-tip={props.context.tooltip}>
              
              
              <fieldset className="fieldset">

                  {props.context.customPresentationContext?.listElement
                      ?.isLastListElement && <p>Last</p>}
                  {props.context.label && <legend className="fieldset-legend">{props.context.label}</legend>}
                  <input 
                      type="text" 
                      className="input" 
                      placeholder="text placeholder"
                      disabled={props.context.disabled}
                      onChange={(e) =>
                          props.foreignMutations.setNewValue(e.currentTarget.value, {
                              test: true,
                          })
                      }
                      value={props.context.value}
                  />
                  {props.context.details && <p className="label">{props.context.details}</p>}
              </fieldset></div>
     )
      },

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
    daisyEnum: () => (props) => {

          if (PredicateValue.Operations.IsUnit(props.context.value)) {
              return <></>;
          }


        const isSome = props.context.value.isSome;
        const value =
            isSome && PredicateValue.Operations.IsRecord(props.context.value.value)
                ? props.context.value.value.fields.get("Value")!
                : undefined;
 
          return (
       
                <fieldset className="fieldset bg-base-100 border-base-300 rounded-box border p-4">
                    {props.context.label && <legend className="fieldset-legend">{props.context.label}</legend>}

                    <label className="label">
                        {(props.context.activeOptions == "unloaded" ||
                  
                        props.context.activeOptions == "loading") && <input type="checkbox" className="toggle"
                               checked={
                                   (props.context.activeOptions != "unloaded" &&
                                       props.context.activeOptions != "loading")
                               }
                               onChange={() => props.foreignMutations.loadOptions()}
                        />}
                        {props.context.details}
                        {(props.context.activeOptions != "unloaded" &&
                            props.context.activeOptions != "loading") && <form className="filter">
                            <input className="btn btn-square" type="reset" value="×"/>
                            {

                                props.context.activeOptions.map((o) =>
                                    (<input
                                        className="btn"
                                        type="radio"
                                        name="frameworks"
                                        onChange={(e) =>
                                            props.foreignMutations.setNewValue(
                                                e.currentTarget.value,
                                                undefined,
                                            )
                                        }
                                        aria-label={o.fields.get("Value")! as string}
                                    />)
                                )}

                        </form>}
                    </label>
                </fieldset>

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
      daisyEnumMultiselect: () => (props) =>
          <fieldset className="fieldset bg-base-200 border-base-300 rounded-box w-xs border p-4">
              {props.context.label && <legend className="fieldset-legend">{props.context.label}</legend> }

              <label className="label">
                  <input type="checkbox" className="toggle"
                         checked={
                             (props.context.activeOptions != "unloaded" &&
                                 props.context.activeOptions != "loading")
                         }
                         onChange={() => props.foreignMutations.loadOptions()}
                  />
                  {props.context.details}
                  {(props.context.activeOptions != "unloaded" &&
                      props.context.activeOptions != "loading") && <form>
                      <input className="btn btn-square" type="reset" value="×"/>
                      {

                          props.context.activeOptions.map((o) =>
                              (<input
                                  className="btn"
                                  type="checkbox"
                                  name="frameworks2"
                                  // onClick={(e) =>
                                  //     props.foreignMutations.setNewValue(
                                  //    []
                                  //     )
                                  // }
                                  aria-label={o.fields.get("Value")! as string}
                              />)
                          )}

                  </form>}
              </label>
              { props.context.details && <p className="label">{props.context.details}</p>}
          </fieldset>


      ,
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
        daisyInfiniteStream: () => (props) => (
            <>
                <fieldset className="fieldset bg-base-200 border-base-300 rounded-box w-xs border p-4">
                    {props.context.label && <legend className="fieldset-legend">{props.context.label}</legend>}
                    {props.context.details && <p className="label">{props.context.details}</p>}
                    <label className="label">      {props.context.value.isSome &&
                        ((props.context.value.value as ValueRecord).fields.get(
                            "DisplayValue",
                        ) as string)}{" "}</label>
                    <div className="join">
                        <div>
                            <div>
                                <input className="input join-item" placeholder="Search"
                                       disabled={props.context.disabled}
                                       value={props.context.customFormState.searchText.value}
                                       onChange={(e) =>
                                           props.foreignMutations.setSearchText(e.currentTarget.value)
                                       }/>
                            </div>
                        </div>
    
                        {props.context.customFormState.status == "open" && <div className="indicator">
    
                            <button className="btn join-item"
                                    disabled={props.context.disabled}
                                    onClick={() => props.foreignMutations.clearSelection(undefined)}>
                                <VscDiffRemoved size={20}/></button>
    
                        </div>}
                        {props.context.customFormState.status != "open" && <div className="indicator">
    
                            <button className="btn join-item"
                                    disabled={props.context.disabled}
                                    onClick={() => props.foreignMutations.toggleOpen()}><VscDiffAdded size={20}/></button>
                        </div>}
                        <div className="indicator">
                            <span className="indicator-item badge badge-secondary">new</span>
                            <button className="btn join-item"
                                    onClick={() => props.foreignMutations.reload()}>
                                <VscRefresh size={20}/></button>
                        </div>
                        <div className="indicator">
    
                            <button className="btn join-item"
                                    disabled={props.context.hasMoreValues == false}
                                    onClick={() => props.foreignMutations.loadMore()}><VscSurroundWith size={20}/>
                            </button>
                        </div>
                    </div>
                </fieldset>
    
    
                {props.context.customFormState.status == "closed" ? (
                    <></>
                ) : (
                    <>
                      
                        <div className="tooltip tooltip-open tooltip-top" data-tip={props.context.tooltip}>
                            <select className="select select-error">
                                <option disabled selected>Pick sth</option>
                                {props.context.customFormState.stream.loadedElements
                                    .valueSeq()
                                    .map((chunk) =>
                                        chunk.data.valueSeq().map((element) => (
                                            <option disabled={props.context.disabled}
                                                    onClick={() =>
                                                        props.foreignMutations.select(
                                                            PredicateValue.Default.option(
                                                                true,
                                                                ValueRecord.Default.fromJSON(element),
                                                            ),
                                                            undefined,
                                                        )
                                                    }>{element.DisplayValue}{" "}
                                                {props.context.value.isSome &&
                                                (props.context.value.value as ValueRecord).fields.get(
                                                    "Id",
                                                ) == element.Id
                                                    ? "✅"
                                                    : ""}</option>
    
                                        )),
                                    )}
                            </select>
                        </div>
                       
    
                    </>
                )}
    
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
      timeline: () => (props) => {
          const value = props.context.value;
          if (PredicateValue.Operations.IsUnit(value)) {
              console.error(`Non partial list renderer called with unit value`);
              return <></>;
          }
          return(<>{props.context.label && <h3>{props.context.label}</h3>}
                  {props.context.tooltip && <p>{props.context.tooltip}</p>}
                  {props.context.details && (
                      <p>
                          <em>{props.context.details}</em>
                      </p>
                  )}
                  <ul className="timeline timeline-snap-icon max-md:timeline-compact timeline-vertical">
                      {value.values.map((_, elementIndex) => 
                          props.embeddedElementTemplate(elementIndex)(undefined)({
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
                          })
     
                      )}
        
                  </ul></>)
              ;
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
