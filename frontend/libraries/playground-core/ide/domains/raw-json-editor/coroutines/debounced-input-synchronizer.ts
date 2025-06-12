import {apiResultStatuses, PromiseRepo} from "ballerina-core";
import { Synchronize } from "ballerina-core";
import { Synchronized } from "ballerina-core";
import { Debounce } from "ballerina-core";
import { Value } from "ballerina-core";
import { RawJsonEditor } from "../state";
import { Co } from "./builder";

const JsonValidationInterval = 2000; // frontend
const JsonValidationIntervalBackend = 1000; // backend

export const debouncedInputBackendValidator = Co.Repeat(
    Debounce<Synchronized<Value<string>, boolean>>(
        Synchronize<Value<string>, boolean>(
            (value: Value<string>) => PromiseRepo.Default.mock((_) => true),
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

        Debounce<Synchronized<Value<string>, boolean>>(
            Synchronize<Value<string>, boolean>(
                RawJsonEditor.Operations.tryParseJsonAsPromise,
                (_: any) => (_ in apiResultStatuses ? _ : "permanent failure"),
                5,
                150,
            ),
            JsonValidationInterval
        ).embed((ide) => ide.inputString, RawJsonEditor.Updaters.Core.inputString),
);