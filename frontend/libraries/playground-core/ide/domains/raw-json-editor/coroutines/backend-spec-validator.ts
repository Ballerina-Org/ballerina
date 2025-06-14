import { Synchronize,apiResultStatuses, Value, Debounce, Synchronized } from "ballerina-core";
import { IDEApi } from "../../../apis/spec";
import { Co} from "./builder";
import {RawJsonEditor, SpecValidationResult} from "../state";
import {IDE} from "../../../state";

// export const validateSpecification =
//     Co.Repeat(
//         Debounce<Synchronized<Value<string>, SpecValidationResult>>(
//             Synchronize<Value<string>, SpecValidationResult>(
//                 IDEApi.validateSpec,
//                 (_: any) => (_ in apiResultStatuses ? _ : "permanent failure"),
//                 5,
//                 150,
//             ),
//             3000
//         ).embed((ide) => 
//             ide.inputString, 
//                 RawJsonEditor.Updaters.Core.inputString),
//     );