import {apiResultStatuses, PromiseRepo} from "ballerina-core";
import { Synchronize } from "ballerina-core";
import { Synchronized } from "ballerina-core";
import { Debounce, replaceWith } from "ballerina-core";
import { Value, BasicFun } from "ballerina-core";
import { IDEApi } from "../../../apis/mocks";
import { RawJsonEditor } from "../state";
import { Co } from "./builder";

const JsonValidationInterval = 200; // frontend
const JsonValidationIntervalBackend = 1000; // backend

export const debouncedInputBackendValidator = Co.Repeat(
    Debounce<Synchronized<Value<string>, ParsingError>>(
        Synchronize<Value<string>, ParsingError>(
            (value: Value<string>) => PromiseRepo.Default.mock((_) => ({ success: false, error: "backend not yet there"})),
            (_: any) => (_ in apiResultStatuses ? _ : "permanent failure"),
            5,
            150,
        ),
        JsonValidationIntervalBackend,
        500,
    ).embed((ide) => ide.inputString, RawJsonEditor.Updaters.Core.inputString),
);

type ParsingError = { success: true; value: any } | { success: false; error: string }

export const debouncedInputValidator = Co.Repeat(
    Debounce<Synchronized<Value<string>, ParsingError>>(
        Synchronize<Value<string>, ParsingError>(
            RawJsonEditor.Operations.tryParseJsonAsPromise,
            (_: any) => (_ in apiResultStatuses ? _ : "permanent failure"),
            5,
            JsonValidationInterval,
        ),
        500,
    ).embed((ide) => ide.inputString, RawJsonEditor.Updaters.Core.inputString),
);