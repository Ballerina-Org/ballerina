import {Guid, simpleUpdater} from "ballerina-core";

export type JobProcessing = {
    checkInterval: number,
    checkCount: number,
    jobId: Guid
}

export const JobProcessing = {
    Default: (id: Guid, overrides?: Partial<JobProcessing>): JobProcessing => ({
        checkInterval: 8000,
        checkCount: 0,
        jobId: id,
        ...overrides,
    }),
    Updaters: {
        Core: {
            ...simpleUpdater<JobProcessing>()("checkInterval"),
            ...simpleUpdater<JobProcessing>()("checkCount"),
            ...simpleUpdater<JobProcessing>()("jobId"),
        }
    },
}

export type JobStatus =
    | { kind: 'starting' }
    | { kind: 'processing', processing: JobProcessing }
    | { kind: 'completed', how: JobProcessing, took: number }
