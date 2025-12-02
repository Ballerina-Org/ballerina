import { Unit } from "ballerina-core";
import { Co } from "./builder";
import {customFields} from "./custom-fields";
import {CustomFields} from "../../domains/phases/custom-fields/state";

export const CustomFieldsRunner =
    Co.Template<Unit>(customFields, {
            runFilter: (props) => {
                const currentTrace = CustomFields.Operations.currentJobTrace(props.context.jobFlow)
                return (props.context.jobFlow.kind != "finished" && currentTrace !== undefined) ;
            }
        }
    );
