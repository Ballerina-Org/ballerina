import {Option, Template} from "ballerina-core";
import {CustomFieldsCtx, CustomFields, CustomFieldsForeignMutationsExpected, CustomFieldsView} from "./state";
import {CustomFieldsRunner } from "../../../coroutines/custom-fields/runner";

export const CustomFieldsTemplate = Template.Default<
    CustomFieldsCtx,
    CustomFields,
    CustomFieldsForeignMutationsExpected,
    CustomFieldsView
>((props) =>
    <props.view
        {...props}
    />
).any([CustomFieldsRunner]);