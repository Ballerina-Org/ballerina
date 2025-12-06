import { Unit } from "ballerina-core";
import { Co } from "./builder";
import {customFields} from "./custom-fields";

export const CustomFieldsRunner =
    Co.Template<Unit>(customFields, {
            runFilter: (props) => {
                return (props.context.status.kind === 'job') ;
            }
        }
    );
