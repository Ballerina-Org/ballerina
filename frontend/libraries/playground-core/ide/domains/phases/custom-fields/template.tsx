import {Option, Template} from "ballerina-core";
import {CustomFields, CustomFieldsForeignMutationsExpected, CustomFieldsView} from "./state";
import {INode, Meta} from "../locked/domains/folders/node";
import {CustomFieldsRunner } from "../../../coroutines/custom-fields/runner";

export const CustomFieldsTemplate = Template.Default<
    CustomFields & {node: Option<INode<Meta>>},
    CustomFields,
    CustomFieldsForeignMutationsExpected,
    CustomFieldsView
>((props) =>
    <props.view
        {...props}
    />
).any([CustomFieldsRunner]);