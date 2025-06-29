import {
  View,
  Template,
  replaceWith,
  Unit,
  simpleUpdater,
  simpleUpdaterWithChildren,
  IdWrapperProps,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  DispatchDelta,
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  Option,
  CommonAbstractRendererReadonlyContext,
  DispatchPrimitiveType,
  CommonAbstractRendererState,
  StringSerializedType,
} from "ballerina-core";

export type CategoryAbstractRendererReadonlyContext<
  CustomPresentationContext = Unit,
> = CommonAbstractRendererReadonlyContext<
  DispatchPrimitiveType<any>,
  DispatchCategory,
  CustomPresentationContext
>;

export type DispatchCategory = {
  kind: "custom";
  value: {
    kind: "child" | "adult" | "senior";
    extraSpecial: boolean;
  };
};

export type DispatchCategoryState = CommonAbstractRendererState & {
  customFormState: {
    likelyOutdated: boolean;
  };
};

export const DispatchCategory = {
  Operations: {
    IsDispatchCategory: (value: unknown): boolean => {
      return (
        typeof value === "object" &&
        value !== null &&
        "kind" in value &&
        value.kind === "custom" &&
        "value" in value &&
        typeof value.value === "object" &&
        value.value !== null &&
        "kind" in value.value &&
        (value.value.kind === "child" ||
          value.value.kind === "adult" ||
          value.value.kind === "senior")
      );
    },
  },
};

export const DispatchCategoryState = {
  Default: (): DispatchCategoryState => ({
    ...CommonAbstractRendererState.Default(),
    customFormState: {
      likelyOutdated: false,
    },
  }),
  Updaters: {
    Core: {
      ...simpleUpdaterWithChildren<DispatchCategoryState>()({
        ...simpleUpdater<DispatchCategoryState["customFormState"]>()(
          "likelyOutdated",
        ),
      })("customFormState"),
    },
  },
};

export type CategoryAbstractRendererForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<DispatchCategory, Flags>;
};

export type CategoryAbstractRendererViewForeignMutationsExpected<Flags> = {
  onChange: DispatchOnChange<DispatchCategory, Flags>;
  setNewValue: ValueCallbackWithOptionalFlags<DispatchCategory, Flags>;
};

export type CategoryAbstractRendererView<
  CustomPresentationContext = Unit,
  Flags = Unit,
> = View<
  CategoryAbstractRendererReadonlyContext<CustomPresentationContext> &
    DispatchCategoryState,
  DispatchCategoryState,
  CategoryAbstractRendererViewForeignMutationsExpected<Flags>
>;

export const CategoryAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  serializedType: StringSerializedType,
) => {
  return Template.Default<
    CategoryAbstractRendererReadonlyContext<CustomPresentationContext> &
      DispatchCategoryState,
    DispatchCategoryState,
    CategoryAbstractRendererForeignMutationsExpected<Flags>,
    CategoryAbstractRendererView<CustomPresentationContext, Flags>
  >((props) => {
    if (!DispatchCategory.Operations.IsDispatchCategory(props.context.value)) {
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Expected dispatch category, got: ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }
    return (
      <>
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId: props.context.identifiers.withoutLauncher,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              setNewValue: (_, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "CustomDelta",
                  value: {
                    kind: "CategoryReplace",
                    replace: _,
                    state: {
                      commonFormState: props.context.commonFormState,
                      customFormState: props.context.customFormState,
                    },
                    type: props.context.type,
                  },
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(replaceWith(_)),
                  delta,
                );
              },
            }}
          />
        </IdProvider>
      </>
    );
  });
};

export type DispatchPassthroughFormInjectedTypes = {
  injectedCategory: {
    type: DispatchCategory;
    state: DispatchCategoryState;
    abstractRenderer: typeof CategoryAbstractRenderer;
    view: CategoryAbstractRendererView<any, any>;
  };
};
