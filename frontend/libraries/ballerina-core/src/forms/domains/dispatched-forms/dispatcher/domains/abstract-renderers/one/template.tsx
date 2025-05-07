import { Map } from "immutable";
import {
  DispatchDelta,
  id,
  oneDebouncerRunner,
  oneLoaderRunner,
  PredicateValue,
  replaceWith,
  Template,
  ValueInfiniteStreamState,
  ValueOption,
} from "../../../../../../../../main";
import { DispatchOnChange } from "../../../state";
import {
  OneAbstractRendererReadonlyContext,
  OneAbstractRendererState,
  OneAbstractRendererView,
} from "./state";

export const OneAbstractRenderer = Template.Default<
  OneAbstractRendererReadonlyContext,
  OneAbstractRendererState,
  {
    onChange: DispatchOnChange<ValueOption>;
  },
  OneAbstractRendererView
>((props) => {
  if (!PredicateValue.Operations.IsOption(props.context.value)) {
    console.error(
      `Option expected but got: ${JSON.stringify(
        props.context.value,
      )}\n...When rendering "one" field\n...${
        props.context.identifiers.withLauncher
      }`,
    );
    return (
      <p>
        {props.context.label && `${props.context.label}: `}RENDER ERROR: Option
        value expected for "one" field but got something else
      </p>
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
              OneAbstractRendererState.Updaters.Core.customFormState.children
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
                    ? OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
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
            props.foreignMutations.onChange(
              replaceWith(
                PredicateValue.Default.option(
                  false,
                  PredicateValue.Default.unit(),
                ),
              ),
              delta,
            );
          },
          setSearchText: (_) =>
            props.setState(
              OneAbstractRendererState.Updaters.Template.searchText(
                replaceWith(_),
              ),
            ),
          loadMore: () =>
            props.setState(
              OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
                ValueInfiniteStreamState.Updaters.Template.loadMore(),
              ),
            ),
          reload: () =>
            props.setState(
              OneAbstractRendererState.Updaters.Template.searchText(
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
            props.foreignMutations.onChange(replaceWith(_), delta);
          },
        }}
      />
    </span>
  );
}).any([
  oneLoaderRunner,
  oneDebouncerRunner.mapContextFromProps((props) => ({
    ...props.context,
    onDebounce: () =>
      props.setState(
        OneAbstractRendererState.Updaters.Core.customFormState.children.stream(
          ValueInfiniteStreamState.Updaters.Template.reload(
            props.context.customFormState.getChunkWithParams(
              props.context.customFormState.searchText.value,
            )(Map()),
          ),
        ),
      ),
  })),
]);
