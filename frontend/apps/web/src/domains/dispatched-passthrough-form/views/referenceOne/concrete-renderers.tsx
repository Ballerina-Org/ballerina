import { ConcreteRenderers, PredicateValue, unit, ValueRecord } from 'ballerina-core';
import { DispatchPassthroughFormInjectedTypes } from '../../injected-forms/category';
import { DispatchPassthroughFormCustomPresentationContext, DispatchPassthroughFormExtraContext, DispatchPassthroughFormFlags } from '../concrete-renderers';

type ReferenceOneConcreteRenderersType = ConcreteRenderers<
  DispatchPassthroughFormInjectedTypes,
  DispatchPassthroughFormFlags,
  DispatchPassthroughFormCustomPresentationContext,
  DispatchPassthroughFormExtraContext
>["referenceOne"]

export const ReferenceOneConcreteRenderers: ReferenceOneConcreteRenderersType = {
  readonlyReferenceOne: () => (props) => {
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
      console.error("referenceOne option inner value is not a record", optionValue);
      return (
        <>
          <h2>{props.context.label}</h2>
          <>Error: referenceOne option inner value is not a record</>
        </>
      );
    }

    // if (props.context.customFormState.stream.kind === "l") {
    //   console.error("stream incorrectly provided for readonly referenceOne");
    //   return (
    //     <>
    //       <h2>{props.context.label}</h2>
    //       <>Error: stream incorrectly provided for readonly referenceOne</>
    //     </>
    //   );
    // }

    return (
      <div
        style={{
          border: "1px solid pink",
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
          <button
            disabled={true}
            onClick={() => props.foreignMutations.toggleOpen()}
          >
            {props?.PreviewRenderer &&
              props?.PreviewRenderer(optionValue)("unique-id")(undefined)?.({
                ...props,
                context: props.context,
                foreignMutations: props.foreignMutations,
                view: unit,
              })}
            </button>
        </li>
        <li>
          {props.DetailsRenderer?.(undefined)({
            ...props, 
            context: props.context,
            foreignMutations: props.foreignMutations,
            view: unit,
          })}
        </li>
      </div>
    );
  },
  editableReferenceOne: () => (props) => {
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
      console.error("referenceOne option inner value is not a record", optionValue);
      return (
        <>
          <h2>{props.context.label}</h2>
          <>Error: referenceOne option inner value is not a record</>
        </>
      );
    }

    if (props.context.customFormState.stream.kind === "r") {
      console.error("stream missing from editable referenceOne");
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
          border: "1px solid pink",
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
            Update selection
            <br />
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
                  chunk.data.valueSeq().map((element: ValueRecord) => {
                    const maybeId = element.fields.get("Id")
                    if (maybeId == undefined)
                      return <>Error: no Id provided</>
                    if (typeof maybeId != "string")
                      return <>Error: provided Id is not a string</>
                    
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
                            props.PreviewRenderer(element)(maybeId)(
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
}