import {List} from "immutable";
import {
    caseUpdater,
    Coroutine,
    ForeignMutationsInput,
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
import {ConstructionJobResponse, RequestValueJobResponse, TypeCheckingJobResponse} from "./domains/job/response/state";
import {TypeCheckingPayload} from "./domains/job/request/state";
import {Co} from "./coroutines/custom-fields/builder";

export type Job =
    (| { kind: "typechecking", payload: TypeCheckingPayload, value: Maybe<TypeCheckingJobResponse>  }
     | { kind: "construction", from: Job, value: Maybe<ConstructionJobResponse> }
     | { kind: "value", from: Job, value: Maybe<RequestValueJobResponse> }
    ) & { status: JobStatus }

export const Job = {
    Updaters: {
        incrementProcessingCount: (): Updater<Job> =>
            Updater<Job>( job => job.status.kind !== 'processing' ? job : ({
                ...job,
                status: { kind: 'processing', processing: 
                        JobProcessing.Updaters.Core.checkCount(replaceWith(job.status.processing.checkCount + 1))
                            .then(JobProcessing.Updaters.Core.checkInterval(replaceWith(job.status.processing.checkInterval * 2)))(job.status.processing)
               }
            } satisfies Job))    
    }
}

export type CustomEntityStatus =
    | { kind: "idle" }
    | { kind: "job", job: Job} 
    | { kind: "result", value: ValueOrErrors<string, string> }

export type CustomEntity = {
    status: CustomEntityStatus,
    trace: Job [],
}

export const CustomEntity = {
    Default: (): CustomEntity => ({
        status: { kind: 'idle' },
        trace: [],
    }),
    Updaters: {
        Core: {
            ...simpleUpdater<CustomEntity>()("status"),
            ...simpleUpdater<CustomEntity>()("trace"),
            ...caseUpdater<CustomEntity>()("status")("job"),
            ...caseUpdater<CustomEntity>()("status")("result"),
        },
        Coroutine: {
            fail: (msg: string | List<string>) => {
                const messages =
                    typeof msg === "string"? List([msg]) : msg;
                const failed = {
                        kind: 'result',
                        value: ValueOrErrors.Default.throw(messages)
                    } as CustomEntityStatus
                
                return CustomEntity.Updaters.Core.status(replaceWith(failed))
            },
            checkIfMaxTries: (count: number) =>
                Updater<CustomEntity>(entity => {
                    if (entity.status.kind !== 'job' || entity.status.job.status.kind !== 'processing' || entity.status.job.status.processing.checkCount <= count) return entity;

                    return CustomEntity.Updaters.Coroutine.fail("Job processing has been canceled due to too many retries")(entity)
                })
        },
        Template: {
            start: (provider: TypeCheckingDataProvider): Updater<CustomEntity> =>
                {
                    const payload = provider.collect()
                    if(payload.kind == "errors") return CustomEntity.Updaters.Coroutine.fail(payload.errors)

                    return Updater<CustomEntity>(entity => ({
                        ...entity,
                        status: {
                            kind: 'job',
                            job: {
                                kind: 'typechecking',
                                payload: payload.value,
                                value: undefined,
                                status: {kind: 'starting'}

                            } satisfies Job
                        },
                        trace: [{
                            kind: 'typechecking',
                            payload: payload.value,
                            value: undefined,
                            status: {kind: 'starting'}

                        }]
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
                    return Option.Default.some(Co.SetState(CustomEntity.Updaters.Coroutine.fail(`Unknown error on ${name}  status, sum: ${JSON.stringify(res)}`)))
                } else if (res.value.kind == "errors")

                    return Option.Default.some(Co.SetState(CustomEntity.Updaters.Coroutine.fail(`Unknown error on ${name}  status, voe: ${JSON.stringify(res)}`)))

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