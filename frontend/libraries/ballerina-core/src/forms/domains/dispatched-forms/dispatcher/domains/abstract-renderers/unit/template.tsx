import { UnitAbstractRendererState, UnitAbstractRendererView } from "./state";
import {
  DispatchDelta,
  FormLabel,
  PredicateValue,
  Template,
  Unit,
} from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { DispatchOnChange } from "../../../state";

export const UnitAbstractRenderer = <Context extends FormLabel>() =>
  Template.Default<
    Context & { disabled: boolean; type: DispatchParsedType<any> },
    UnitAbstractRendererState,
    { onChange: DispatchOnChange<Unit> },
    UnitAbstractRendererView<Context>
  >((props) => (
    <props.view
      {...props}
      foreignMutations={{
        ...props.foreignMutations,
        onChange: (_) => {
          const delta: DispatchDelta = {
            kind: "UnitReplace",
            replace: PredicateValue.Default.unit(),
            state: {
              commonFormState: props.context.commonFormState,
              customFormState: props.context.customFormState,
            },
            type: props.context.type,
          };
          props.foreignMutations.onChange(_, delta);
        },
      }}
    />
  ));
