import {
  FormLabel,
  View,
  Value,
  OnChange,
  SimpleCallback,
  BasicFun,
  FieldValidation,
  Template,
  replaceWith,
  ValidateRunner,
  FieldValidationWithPath,
  Unit,
  simpleUpdater,
  simpleUpdaterWithChildren,
  DeltaCustom,
  ParsedType,
  CommonFormState,
} from "ballerina-core";
import {
  CategoryState,
  CategoryView,
} from "src/domains/person-from-config/injected-forms/category";

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

export type DispatchCategoryView<
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

export const DispatchCategoryForm = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>() => {
  return Template.Default<
    Context &
      Value<DispatchCategory> & { disabled: boolean; type: ParsedType<any> },
    {
      commonFormState: CommonFormState;
      customFormState: DispatchCategoryState["customFormState"];
    },
    ForeignMutationsExpected & { onChange: OnChange<DispatchCategory> },
    DispatchCategoryView<Context, ForeignMutationsExpected>
  >((props) => (
    <>
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
  ));
};

export const dispatchCategoryForm = (
  concreteRenderer: any,
  label: string,
  tooltip?: string,
  details?: string,
) =>
  DispatchCategoryForm<any & FormLabel, Unit>()
    .withView(concreteRenderer)
    .mapContext<any & CommonFormState & Value<DispatchCategory>>((_) => ({
      ..._,
      type: {
        kind: "primitive",
        name: "injectedCategory",
      },
      label,
      tooltip,
      details,
    })) as any;

export type DispatchPassthroughFormInjectedTypes = {
  injectedCategory: { type: DispatchCategory; state: DispatchCategoryState };
};
