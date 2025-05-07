import { Map } from "immutable";
import React from "react";
import {
  AsyncState,
  DispatchDelta,
  id,
  PredicateValue,
  replaceWith,
  Synchronized,
  Template,
  unit,
  ValueInfiniteStreamState,
  ValueOption,
} from "../../../../../../../../../../main";
import { DispatchOnChange } from "../../../../../state";
import {
  OneTableAbstractRendererReadonlyContext,
  OneTableAbstractRendererState,
  OneTableAbstractRendererView,
} from "./state";
import {
  initializeOneRunner,
  oneTableDebouncerRunner,
  oneTableLoaderRunner,
} from "./coroutines/runner";

export const OneTableAbstractRenderer =


Template.Default<
  OneTableAbstractRendererReadonlyContext,
  OneTableAbstractRendererState,
  {
    onChange: DispatchOnChange<ValueOption>;
  },
  OneTableAbstractRendererView
>((props) => {
  if (
    !PredicateValue.Operations.IsOption(props.context.value) &&
    !PredicateValue.Operations.IsUnit(props.context.value)
  ) {
    console.error(
      `Option or unit  expected but got: ${JSON.stringify(
        props.context.value,
      )}\n...When rendering "one" field\n...${
        props.context.identifiers.withLauncher
      }`,
    );
    return (
      <></>
      // <p>
      //   {props.context.label && `${props.context.label}: `}RENDER ERROR: Option
      //   value expected for "one" field but got something else
      // </p>
    );
  }
  return (
    <span
      className={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
    >
      <props.view
        {...props}
        context={{
          ...props.context,
          hasMoreValues: !(
            props.context.customFormState.stream.loadedElements.last()
              ?.hasMoreValues == false
          ),
        }}
        foreignMutations={{
          ...props.foreignMutations,
          toggleOpen: () =>
            props.setState(
              OneTableAbstractRendererState.Updaters.Core.customFormState.children
                .status(
                  replaceWith(
                    props.context.customFormState.status == "closed"
                      ? "open"
                      : "closed",
                  ),
                )
                .then(
                  props.context.customFormState.stream.loadedElements.count() ==
                    0
                    ? OneTableAbstractRendererState.Updaters.Core.customFormState.children.stream(
                        ValueInfiniteStreamState.Updaters.Template.loadMore(),
                      )
                    : id,
                ),
            ),
          clearSelection: () => {
            const delta: DispatchDelta = {
              kind: "OptionReplace",
              replace: PredicateValue.Default.option(
                false,
                PredicateValue.Default.unit(),
              ),
              state: {
                commonFormState: props.context.commonFormState,
                customFormState: props.context.customFormState,
              },
              type: props.context.type,
            };
            props.foreignMutations.onChange(id, delta);
            props.setState(
              OneTableAbstractRendererState.Updaters.Core.customFormState.children.selectedValue(
                Synchronized.Updaters.sync(AsyncState.Updaters.toLoaded(unit)),
              ),
            );
          },
          setSearchText: (_) =>
            props.setState(
              OneTableAbstractRendererState.Updaters.Template.searchText(
                replaceWith(_),
              ),
            ),
          loadMore: () =>
            props.setState(
              OneTableAbstractRendererState.Updaters.Core.customFormState.children.stream(
                ValueInfiniteStreamState.Updaters.Template.loadMore(),
              ),
            ),
          reload: () =>
            props.setState(
              OneTableAbstractRendererState.Updaters.Template.searchText(
                replaceWith(""),
              ),
            ),
          select: (_) => {
            const delta: DispatchDelta = {
              kind: "OptionReplace",
              replace: _,
              state: {
                commonFormState: props.context.commonFormState,
                customFormState: props.context.customFormState,
              },
              type: props.context.type,
            };
            props.setState(
              OneTableAbstractRendererState.Updaters.Core.customFormState.children.selectedValue(
                Synchronized.Updaters.sync(AsyncState.Updaters.toLoaded(_)),
              ),
            );
            props.foreignMutations.onChange(id, delta);
          },
        }}
      />
    </span>
  );
}).any([
  initializeOneRunner,
  oneTableLoaderRunner,
  oneTableDebouncerRunner.mapContextFromProps((props) => ({
    ...props.context,
    onDebounce: () =>
      props.setState(
        OneTableAbstractRendererState.Updaters.Core.customFormState.children.stream(
          ValueInfiniteStreamState.Updaters.Template.reload(
            props.context.customFormState.getChunkWithParams(
              props.context.customFormState.searchText.value,
            )(Map()),
          ),
        ),
      ),
  })),
]);
