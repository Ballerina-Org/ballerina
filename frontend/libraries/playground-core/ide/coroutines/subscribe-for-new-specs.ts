import { apiResultStatuses } from "ballerina-core";
import { Synchronize } from "ballerina-core";
import { Synchronized } from "ballerina-core";
import { Debounce } from "ballerina-core";
import { Value, PromiseRepo } from "ballerina-core";
import { IDE } from "../state";
import { ValidationResult } from "ballerina-core";
import { Co } from "./builder";
import { JsonValue } from "../domains/spec-editor/state";

const SubscriptionInterval = 1000;

export const specsSubscription = Co.Repeat(
    Debounce<Synchronized<Value<JsonValue []>, ValidationResult>>(
        Synchronize<Value<JsonValue []>, ValidationResult>(
            (_) => PromiseRepo.Default.mock((_) =>  {
                return "valid"}),
            (_: any) => (_ in apiResultStatuses ? _ : "permanent failure"),
            5,
            150,
        ),
        SubscriptionInterval,
        500,
    ).embed((ide) => ide.availableSpecs, IDE.Updaters.Core.availableSpecs),
);