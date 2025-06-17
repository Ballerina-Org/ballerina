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
  DispatchDelta,
  DispatchOnChange,
  ValueCallbackWithOptionalFlags,
  Option,
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
  Flags = Unit,
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
    onChange: DispatchOnChange<DispatchCategory, Flags>;
    setNewValue: ValueCallbackWithOptionalFlags<DispatchCategory, Flags>;
  }
>;

export const CategoryAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
  Flags = Unit,
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
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<DispatchCategory, Flags>;
    },
    CategoryAbstractRendererView<Context, ForeignMutationsExpected, Flags>
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
  injectedCategory: { type: DispatchCategory; state: DispatchCategoryState };
};
