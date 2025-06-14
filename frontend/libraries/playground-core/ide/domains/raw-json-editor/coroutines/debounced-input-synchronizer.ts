import {apiResultStatuses, PromiseRepo} from "ballerina-core";
import { Synchronize } from "ballerina-core";
import { Synchronized } from "ballerina-core";
import { Debounce } from "ballerina-core";
import { Value } from "ballerina-core";
import {RawJsonEditor, SpecValidationResult} from "../state";
import { Co } from "./builder";

const JsonValidationInterval = 2000;

//
// export const debouncedInputValidator = Co.Repeat(
//
//         Debounce<Synchronized<Value<string>, SpecValidationResult>>(
//             Synchronize<Value<string>, SpecValidationResult>(
//                 RawJsonEditor.Operations.tryParse,
//                 (_: any) => (_ in apiResultStatuses ? _ : "permanent failure"),
//                 5,
//                 150,
//             ),
//             JsonValidationInterval
//         ).embed((ide) => ide.inputString, RawJsonEditor.Updaters.Core.inputString),
// );