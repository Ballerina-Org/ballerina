import {
  FormLabel,
  View,
  Value,
  OnChange,
  SimpleCallback,
  Template,
  replaceWith,
  Unit,
  simpleUpdater,
  simpleUpdaterWithChildren,
  DeltaCustom,
  ParsedType,
  CommonFormState,
  IdWrapperProps,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
} from "ballerina-core";

export type DispatchCategory = {
  kind: "custom";
  value: {
    kind: "child" | "adult" | "senior";
    extraSpecial: boolean;
  };
};

export type DispatchCategoryState = {
  commonFormState: {
    modifiedByUser: boolean;
  };
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
    commonFormState: {
      modifiedByUser: false,
    },
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

export type CategoryAbstractRendererView<
  Context extends FormLabel,
  ForeignMutationsExpected,
> = View<
  Context &
    Value<DispatchCategory> & {
      commonFormState: CommonFormState;
      customFormState: DispatchCategoryState["customFormState"];
    } & { disabled: boolean; type: ParsedType<any> },
  {
    commonFormState: CommonFormState;
    customFormState: DispatchCategoryState["customFormState"];
  },
  ForeignMutationsExpected & {
    onChange: OnChange<DispatchCategory>;
    setNewValue: SimpleCallback<DispatchCategory>;
  }
>;

export const CategoryAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    Context &
      Value<DispatchCategory> & {
        disabled: boolean;
        type: ParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    {
      commonFormState: CommonFormState;
      customFormState: DispatchCategoryState["customFormState"];
    },
    ForeignMutationsExpected & { onChange: OnChange<DispatchCategory> },
    CategoryAbstractRendererView<Context, ForeignMutationsExpected>
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
        <IdProvider
          id={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
        />
        <props.view
          {...props}
          foreignMutations={{
            ...props.foreignMutations,
            setNewValue: (_) => {
              const delta: DeltaCustom = {
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
              };
              props.foreignMutations.onChange(replaceWith(_), delta);
            },
          }}
        />
      </>
    );
  });
};

export type DispatchPassthroughFormInjectedTypes = {
  injectedCategory: { type: DispatchCategory; state: DispatchCategoryState };
};
