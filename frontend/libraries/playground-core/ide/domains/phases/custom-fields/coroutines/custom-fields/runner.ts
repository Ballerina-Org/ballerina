import { Unit } from "ballerina-core";
import { Co } from "./builder";
import {customFields} from "./custom-fields";
import {documents} from "./documents";

export const CustomFieldsRunner =
    Co.Template<Unit>(customFields, {
            runFilter: (props) => {
                return (props.context.status.kind === 'job') ;
            }
        }
    );

export const DocumentsRunner =
    Co.Template<Unit>(documents, {
            runFilter: (props) => props.context.documents.available.length == 0
        }
    );
