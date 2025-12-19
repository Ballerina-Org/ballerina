import {List} from "immutable";
import {
    caseUpdater,
    Coroutine,
    ForeignMutationsInput, Guid,
    Maybe,
    Option,
    replaceWith,
    simpleUpdater,
    Sum,
    Unit,
    Updater,
    ValueOrErrors,
    View,
} from "ballerina-core";
import {INode, Meta} from "../locked/domains/folders/node";
import {
    JobProcessing, JobStatus,
} from "./domains/job/state";
import {TypeCheckingDataProvider} from "./domains/data-provider/state";
import {
    ConstructionJobResponse,
    RequestValueJobResponse,
    TypeCheckingJobResponse,
    UpdateJobResponse
} from "./domains/job/response/state";
import {TypeCheckingPayload} from "./domains/job/request/state";
import {Co} from "./coroutines/custom-fields/builder";
import {getDocument} from "../../../api/documents";


export type Document = {
    id: Guid,
    name: string
}
type DocumentsBase = {
    available: Document[];
};
export type Documents =
    (| { kind: 'selected', document: Document }
     | { kind: 'loaded', document: Document, content: string } 
     | { kind: 'not-selected' }) & DocumentsBase

const noDocuments: Document[] = [];

export type Job =
    (| { kind: "typechecking", payload: TypeCheckingPayload, value: Maybe<TypeCheckingJobResponse>  }
     | { kind: "construction", from: Job, value: Maybe<ConstructionJobResponse> }
     | { kind: "value", from: Job, value: Maybe<RequestValueJobResponse>, valueId: Guid } 
     | { kind: 'updater', delta: string,  value: Maybe<UpdateJobResponse> }
    ) & { status: JobStatus }


const incrementProcessingCountForJob = (): Updater<Job> => {

    return Updater<Job>(job => job.status.kind !== 'processing' ? job : ({
        ...job,
        status: {
            kind: 'processing', processing:
                JobProcessing.Updaters.Core.checkCount(replaceWith(job.status.processing.checkCount + 1))
                    .then(JobProcessing.Updaters.Core.checkInterval(replaceWith(job.status.processing.checkInterval + 1000)))(job.status.processing)
        } satisfies JobStatus
    } satisfies Job))
}

export const Job = {
    Updaters: {
        incrementProcessingCount: (): Updater<CustomEntity> => {

            return Updater<CustomEntity>(ce => { 
                if(ce.status.kind !== 'job' || ce.status.job.status.kind !== 'processing') return ce;
                
                const job = incrementProcessingCountForJob()(ce.status.job);
                
                return ({
                ...ce,
                status: {
                    kind: 'job',
                    job: job,
                    
                } satisfies CustomEntityStatus,
                trace: [...ce.trace.slice(0, -1), job]
            } satisfies CustomEntity)})
        }
    }
}

export type CustomEntityStatus =
    | { kind: "idle" }
    | { kind: "job", job: Job} 
    | { kind: "result", value: ValueOrErrors<string, string> }

export type CustomEntity = {
    documents: Documents,
    status: CustomEntityStatus,
    trace: Job [],
}

export const CustomEntity = {
    Default: (): CustomEntity => ({
        documents: { kind: 'not-selected', available: noDocuments} ,
        status: { kind: 'idle' },
        trace: [],
    }),
    Updaters: {
        Core: {
            ...simpleUpdater<CustomEntity>()("status"),
            ...simpleUpdater<CustomEntity>()("trace"),
            ...simpleUpdater<CustomEntity>()("documents"),
            ...caseUpdater<CustomEntity>()("status")("job"),
            ...caseUpdater<CustomEntity>()("status")("result"),
            fail: (msg: string | List<string>) => {
                const messages =
                    typeof msg === "string" ? List([msg]) : msg;
                const failed = {
                    kind: 'result',
                    value: ValueOrErrors.Default.throw(messages)
                } as CustomEntityStatus

                return CustomEntity.Updaters.Core.status(replaceWith(failed))
            },
        },
        Coroutine: {
            checkIfMaxTries: (count: number) =>
                Updater<CustomEntity>(entity => {
                    if (entity.status.kind !== 'job' || entity.status.job.status.kind !== 'processing' || entity.status.job.status.processing.checkCount <= count) return entity;

                    return CustomEntity.Updaters.Core.fail("Job processing has been canceled due to too many retries")(entity)
                })
        },
        Template: {
            selectDocument: (doc: string, id: string) => {

                return Updater<CustomEntity>(e => {
                    const document = e.documents.available.find(x => x.id == id)!
                    return CustomEntity.Updaters.Core.documents(d=>({...d, kind: 'selected', document: document }))
                        .then(CustomEntity.Updaters.Core.documents(d=>({...d, kind: 'loaded', document: document, content: doc })))(e)
                })

            }, 
            start: (provider: TypeCheckingDataProvider): Updater<CustomEntity> =>
                Updater<CustomEntity>(entity =>
                {
                    if(entity.documents.kind !== 'loaded') 
                        return CustomEntity.Updaters.Core.fail(List(["Can't start CE workflow without loaded document"]))(entity);
                    
                    const payload = provider.collect(entity.documents.content)
                    debugger
                    if(payload.kind == "errors") return CustomEntity.Updaters.Core.fail(payload.errors)(entity);

                    const job = {
                        kind: 'typechecking',
                        payload: payload.value,
                        value: undefined,
                        status: {kind: 'starting'}

                    } satisfies Job
                    return ({
                        ...entity,
                        status: {
                            kind: 'job',
                            job: job
                        },
                        trace: [job]
                    } satisfies CustomEntity)
                }),
            update: (delta: string): Updater<CustomEntity> =>
            {
                const job = {
                    kind: 'updater',
                    value: undefined,
                    status: {kind: 'starting'},
                    delta: delta

                } satisfies Job;
                return Updater<CustomEntity>(entity => ({
                    ...entity,
                    status: {
                        kind: 'job',
                        job: job
                    },
                    trace: [...entity.trace, job]
                } satisfies CustomEntity))
            },
        }
    },
    Operations: {
        idle: (): CustomEntityStatus => ({ kind: 'idle' }),
        isAvailable: (node: INode<Meta>): boolean => {
            // custom entity runner should be enabled when we have all necessary files
            // in the current workspace
            return true
        },
        /** Handy for processing the Await coroutine for both the Sum and ValueOrError error at once  */
        checkResponseForErrors:
            (res: Sum<ValueOrErrors<any, any>, any>, name: string): Option<Coroutine<CustomEntity, CustomEntity, Unit>> => {
                if (res.kind == "r") {
                    return Option.Default.some(Co.SetState(CustomEntity.Updaters.Core.fail(`Unknown error on ${name}  status, sum: ${JSON.stringify(res)}`)))
                } else if (res.value.kind == "errors")

                    return Option.Default.some(Co.SetState(CustomEntity.Updaters.Core.fail(`Unknown error on ${name}  status, voe: ${JSON.stringify(res)}`)))

                return Option.Default.none()
            },
    },
    ForeignMutations: (
        _: ForeignMutationsInput<Unit, CustomEntity>,
    ) => ({
    }),
};

export type CustomEntityForeignMutationsExpected = {}

export type CustomFieldsView = View<
    CustomEntity,
    CustomEntity,
    CustomEntityForeignMutationsExpected,
    {
    }
>;