import {CustomFieldsContext, CustomFieldsProcess } from "../../state";
import {Updater, ValueOrErrors} from "ballerina-core";
import {narrowed} from "../../../../types/narrowed";
import {List} from "immutable";
import {JobInitialResponse, JobRequest, JobResult, JobTrace, JobFlow, JobResponse } from "../job/state";

// ∇ main goal of this is to help reason inside the coroutine
export type DomainEvent =
    | { kind: "begin-type-checking" }
    | { kind: "begin-construction" }
    | { kind: "begin-updater" }
    | { kind: "finish-flow"; value: ValueOrErrors<any, any> };

export type JobLifecycleEvent =
    | { kind: "start-job"; job: JobRequest }
    | { kind: "initial"; initial: JobInitialResponse }
    | { kind: "completed"; result: JobResult }
    | { kind: "fail"; error: string };

export type CustomFieldsEvent =
    | DomainEvent
    | JobLifecycleEvent;

export const transitionUpdater = (event: CustomFieldsEvent): Updater<CustomFieldsContext> =>
    Updater((ctx:CustomFieldsContext) => {
        const { state, flow } = ctx;
        
        if (event.kind === "begin-type-checking") {
            return { state: { kind: "type-checking" }, flow } as CustomFieldsContext;
        }

        if (event.kind === "begin-construction") {
            return { state: { kind: "construction" }, flow };
        }

        if (event.kind === "begin-updater") {
            return { state: { kind: "updater" }, flow };
        }

        if (event.kind === "finish-flow") {
            return {
                state: { kind: "result", value: event.value },
                flow: {
                    ...flow,
                    kind: "finished",
                    result: event.value
                }
            };
        }

        if (event.kind === "start-job") {
            const newTrace: JobTrace = {
                kind: "requested",
                job: event.job
            };

            return {
                state: { kind: "type-checking" },
                flow: {
                    kind: "in-progress",
                    traces: [...flow.traces, newTrace]
                }
            };
        }

        if (event.kind === "initial") {
            const last = flow.traces[flow.traces.length - 1];
            if (!last || last.kind !== "requested")
                return narrowed("Initial response without requested state");

            const updated: JobTrace = {
                kind: "initial",
                job: last.job,
                initial: event.initial
            };

            return {
                state: { kind: "construction" },
                flow: {
                    kind: "in-progress",
                    traces: [...flow.traces.slice(0, -1), updated]
                }
            };
        }

        if (event.kind === "completed") {
            const last = flow.traces[flow.traces.length - 1];
            if (!last || last.kind !== "initial")
                return narrowed("Completed result without initial");

            const updated: JobTrace = {
                kind: "completed",
                job: last.job,
                initial: last.initial,
                result: event.result
            };

            return {
                state: { kind: "updater" },
                flow: {
                    kind: "in-progress",
                    traces: [...flow.traces.slice(0, -1), updated]
                }
            };
        }

        if (event.kind === "fail") {
            const errorResult = ValueOrErrors.Default.throw(
                List([event.error])
            );

            return {
                state: { kind: "result", value: errorResult },
                flow: {
                    kind: "finished",
                    traces: flow.traces,
                    result: errorResult
                }
            };
        }

        return narrowed("Unhandled transition event");
    
    });