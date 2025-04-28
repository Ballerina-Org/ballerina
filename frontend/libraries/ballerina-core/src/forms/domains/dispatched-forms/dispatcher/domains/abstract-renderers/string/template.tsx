import { List } from "immutable";
import {
  StringAbstractRendererState,
  StringAbstractRendererView,
} from "./state";
import {
  DispatchDelta,
  FormLabel,
  replaceWith,
  Template,
  ValidateRunner,
  Value,
} from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { DispatchOnChange } from "../../../state";
import React from "react";

export const StringAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>() => {
  return Template.Default<
    Context &
      Value<string> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
      },
    StringAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<string> },
    StringAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => (
    <>
      <props.view
        {...props}
        foreignMutations={{
          ...props.foreignMutations,
          setNewValue: (_) => {
            const delta: DispatchDelta = {
              kind:
                props.context.type.kind == "primitive" &&
                props.context.type.typeName == "string"
                  ? "StringReplace"
                  : "GuidReplace",
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
    </>
  )).any([]);
};
