import {Option, Unit, ValueOrErrors} from "ballerina-core";
import {TypeCheckingPayload} from "../type-checking/state";
import {Guid} from "../../../../types/Guid";

export type JobResponse = Guid

export type TypeCheckingJob = TypeCheckingPayload

export type ResponseWithStatusAndResult<result> = { status: number, result: result }

export type TypeCheckingJobResponse = {
    id: JobResponse
    status: number,
    startedAt: string,
    result: {
        valueDescriptorId: Guid
    },
    error: { message: string}
}

export type ConstructionJob = {
    ValueDescriptorId: Guid
}
export type ConstructionJobResponse = {
    id: JobResponse
    status: number,
    startedAt: string,
    result: {
        valueId: Guid
    },
    error: { message: string}
}

export type UpdaterJob = {
    ValueId: Guid,
    Parameter: { Delta: any }
}
export type UpdaterJobResponse = {
    id: JobResponse
    status: number,
    startedAt: string,
    result: {
        valueId: Guid
    },
    error:  { message: string}
}

export type RequestValueJob = Guid
export type RequestValueJobResponse = {
    uncertainties: 
        {
            id: Guid,
            isFailing: boolean,
            message: string
        }[]
    ,
    evidence: 
        {
            id: Guid
            page: number,
            cells: number []
        }[]
    ,
    lastUpdatedAt: string,
    isArchived: false,
    valueDescriptorId: Guid
}

export type JobRequest =
    | { kind: 'type-checking'; payload: TypeCheckingJob }
    | { kind: 'construction'; payload: ConstructionJob }
    | { kind: 'updater'; payload: UpdaterJob }
    | { kind: 'request-value'; payload: RequestValueJob }

export type JobInitialResponse =
    | { kind: 'type-checking'; jobId: Guid }
    | { kind: 'construction'; jobId: Guid }
    | { kind: 'updater'; jobId: Guid }
    //| { kind: 'request-value'; jobId: Guid }

export type JobResult =
    | { kind: 'type-checking'; result: TypeCheckingJobResponse }
    | { kind: 'construction'; result: ConstructionJobResponse }
    | { kind: 'updater'; result: UpdaterJobResponse }
    | { kind: 'request-value'; result: RequestValueJobResponse }

export type JobTrace =
    | { kind: "requested";  job: JobRequest }
    | { kind: "dispatched"; job: JobRequest; initial: Option<JobInitialResponse> }
    | { kind: "completed";  job: JobRequest; initial: Option<JobInitialResponse>; result: JobResult };

export type JobFlow =
    { traces: JobTrace[] }
    & (
    | { kind: "in-progress" }
    | { kind: "finished"; result: ValueOrErrors<RequestValueJobResponse, any> }
    );