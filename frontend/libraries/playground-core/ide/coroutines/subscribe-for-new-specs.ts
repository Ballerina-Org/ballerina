import { apiResultStatuses } from "ballerina-core";
import { Synchronize } from "ballerina-core";
import { Synchronized } from "ballerina-core";
import { Debounce } from "ballerina-core";
import { Value, BasicFun, PromiseRepo } from "ballerina-core";
import { IDEApi } from "../apis/mocks";
import { IDE } from "../state";
import { ValidationResult } from "ballerina-core";
import { Co } from "./builder";
import { JsonParseState } from "../domains/raw-json-editor/state";

const SubscriptionInterval = 2 * 60 * 1000;

export const specsSubscription = Co.Repeat(
    Debounce<Synchronized<Value<JsonParseState []>, ValidationResult>>(
        Synchronize<Value<JsonParseState []>, ValidationResult>(
            (_) => PromiseRepo.Default.mock((_) => "valid"),
            (_: any) => (_ in apiResultStatuses ? _ : "permanent failure"),
            5,
            150,
        ),
        SubscriptionInterval,
        500,
    ).embed((ide) => ide.availableSpecs, IDE.Updaters.Core.availableSpecs),
);