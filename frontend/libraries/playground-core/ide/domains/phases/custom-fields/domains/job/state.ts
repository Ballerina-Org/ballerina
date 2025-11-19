import {Unit, ValueOrErrors} from "ballerina-core";
import {TypeCheckingPayload} from "../type-checking/state";
import {Guid} from "../../../../types/Guid";

export type JobResponse = Guid

export type TypeCheckingJob = TypeCheckingPayload
export type TypeCheckingJobResponse = JobResponse

export type ConstructionJob = Unit
export type ConstructionJobResponse = JobResponse

export type UpdaterJob = Unit
export type UpdaterJobResponse = JobResponse

export type RequestValueJob = Unit
export type RequestValueJobResponse = JobResponse

export type JobRequest =
    | { kind: 'type-checking'; payload: TypeCheckingJob }
    | { kind: 'construction'; payload: ConstructionJob }
    | { kind: 'updater'; payload: UpdaterJob }
    | { kind: 'request-value'; payload: RequestValueJob }

export type JobInitialResponse =
    | { kind: 'type-checking'; jobId: Guid }
    | { kind: 'construction'; jobId: Guid }
    | { kind: 'updater'; jobId: Guid }
    | { kind: 'request-value'; jobId: Guid }

export type JobResult =
    | { kind: 'type-checking'; result: TypeCheckingJobResponse }
    | { kind: 'construction'; result: ConstructionJobResponse }
    | { kind: 'updater'; result: UpdaterJobResponse }
    | { kind: 'request-value'; result: RequestValueJobResponse }


// ∇ main goal of this is to help reason in progressive UI

export type JobTrace =
    | { kind: "requested"; job: JobRequest }
    | { kind: "initial"; job: JobRequest; initial: JobInitialResponse }
    | { kind: "completed"; job: JobRequest; initial: JobInitialResponse; result: JobResult };


export type JobFlow =
    { traces: JobTrace[] }
    & (
    | { kind: "in-progress" }
    | { kind: "finished"; result: ValueOrErrors<any, any> }
    );