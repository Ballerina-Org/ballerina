import {Template} from "ballerina-core";
import {CustomEntity, CustomEntityForeignMutationsExpected, CustomFieldsView} from "./state";
import {CustomFieldsRunner } from "./coroutines/custom-fields/runner"

export const CustomFieldsTemplate = Template.Default<
    CustomEntity,
    CustomEntity,
    CustomEntityForeignMutationsExpected,
    CustomFieldsView
>((props) =>
    <props.view
        {...props}
    />
).any([CustomFieldsRunner]);