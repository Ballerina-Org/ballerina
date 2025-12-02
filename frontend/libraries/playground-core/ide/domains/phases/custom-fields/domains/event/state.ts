import {CustomFields, CustomFieldsContext, CustomFieldsProcess} from "../../state";
import {Updater, ValueOrErrors, Option} from "ballerina-core";
import {narrowed} from "../../../../types/narrowed";
import {List} from "immutable";
import {
    JobInitialResponse,
    JobRequest,
    JobResult,
    JobTrace,
    JobFlow,
    JobResponse,
    TypeCheckingJobResponse, RequestValueJobResponse, ConstructionJobResponse
} from "../job/state";
import {Guid} from "../../../../types/Guid";

export type DomainEvent =
    | { kind: "begin-type-checking" }
    | { kind: "begin-construction" }
    | { kind: "begin-updater" }
    | { kind: "begin-value", valueId: Guid }
    | { kind: "begin-result", result: ValueOrErrors<any, any> };

export type JobLifecycleEvent =
    | { kind: "start-job"; job: JobRequest }
    | { kind: "initial"; initial: JobInitialResponse }
    | { kind: "completed"; result: JobResult }

export type CustomFieldsEvent =
    | DomainEvent
    | JobLifecycleEvent;

export const CustomFieldsEvent = {
    typeCheckingCompleted: (res: TypeCheckingJobResponse): CustomFieldsEvent =>
        ( {
            kind: 'completed',
            result: {
                kind: "type-checking",
                result: res
            }
        }),
    beginConstruction: () : CustomFieldsEvent =>  ({kind: 'begin-construction'}),
    startConstruction: (valueDescriptorId: Guid) : CustomFieldsEvent => (
        {
            kind: 'start-job', job:{
                kind: "construction",
                payload: { ValueDescriptorId: valueDescriptorId }
            }
        }),
    initConstruction: (job: Guid): CustomFieldsEvent =>
        ({
            kind: 'initial', initial: {
                kind: "construction",
                jobId: job
            }
        }),
    completeConstruction: (job: ConstructionJobResponse): CustomFieldsEvent =>
        ({
            kind: 'completed',
            result: {
                kind: "construction",
                result: job
            }
        }),
    typeCheckingInit: (jobId: Guid): CustomFieldsEvent => ({
        kind: 'initial',
        initial:  {
            kind: 'type-checking',
            jobId: jobId
        }}),
    requestValue: (id: Guid): CustomFieldsEvent => ({
        kind: 'start-job', job:{
            kind: "request-value",
            payload: id
        }
    }),
    valueCompleted: (res: RequestValueJobResponse): CustomFieldsEvent => ({
        kind: 'completed', result: {
            kind: "request-value",
            result: res
        }
    }),
    conclude: (res: ValueOrErrors<RequestValueJobResponse,string>): CustomFieldsEvent => ({
        kind: 'begin-result', result: res
    })
}

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
            return { state: { kind: "updater" }, flow: {...flow, kind: 'in-progress' } };
        }

        if (event.kind === "begin-value") {
            return {
                state: { kind: "value" }, flow
            };
        }

        if (event.kind === "begin-result") {
            return {
                state: { kind: "result", value: event.result },
                flow: {
                    kind: "finished",
                    traces: flow.traces,
                    result: event.result
                }
            };
        }

        if (event.kind === "start-job") {
            const newTrace: JobTrace = {
                kind: "requested",
                job: event.job
            };

            return {
                state: state,
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
                kind: "dispatched",
                job: last.job,
                initial: Option.Default.some(event.initial)
            };

            return {
                state: state,
                flow: {
                    kind: "in-progress",
                    traces: [...flow.traces.slice(0, -1), updated]
                }
            };
        }

        if (event.kind === "completed") {
            const last = flow.traces[flow.traces.length - 1];

            const updated: JobTrace = {
                kind: "completed",
                job: last.job,
                initial: Option.Default.none(), 
                result: event.result
            };

            return {
                state: state,
                flow: {
                    kind: "in-progress",
                    traces: [...flow.traces.slice(0, -1), updated]
                }
            };
        }


        return narrowed("Unhandled transition event");
    
    });