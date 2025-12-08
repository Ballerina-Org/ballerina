import {Co} from "./builder";
import {
    ValueOrErrors,
    replaceWith,Guid, Updater, BasicFun
} from "ballerina-core";

import {CustomEntity, CustomEntityStatus, Job} from "../../state"
import { JobProcessing } from "../../domains/job/state"
import {RequestValueJobResponse, ResponseWithStatus} from "../../domains/job/response/state";
import {constructionJob, getJobStatus, getValue, typeCheckingJob} from "../../../../../api/custom-fields/client";
import {AI_Value_Mock} from "../../domains/mock";

const awaitProcessingJob = <result>(complete: BasicFun<ResponseWithStatus<result>,Job>) => {

    return Co.While(
        ([entity]: CustomEntity[]) => entity.status.kind === 'job' && entity.status.job.status.kind === 'processing',
        Co.GetState().then(entity => {
            if(!(entity.status.kind === 'job' && entity.status.job.status.kind === 'processing'))
                return Co.Do(()=>{})
            
            const id = entity.status.job.status.processing.jobId;
            const kind = entity.status.job.kind as 'typechecking' | 'construction'
            return Co.Seq([
                Co.SetState(CustomEntity.Updaters.Coroutine.checkIfMaxTries(3)),
                Co.Wait(entity.status.job.status.processing.checkInterval),
                Co.Await<ValueOrErrors<ResponseWithStatus<result>, any>, any>(() =>
                    getJobStatus<result>(kind, id), (_err: any) => {
                }).then(res => {
                    const response = CustomEntity.Operations.checkResponseForErrors(res, `${entity.status.kind} status`)
                    if (response.kind == "r") return response.value
                    if (res.value.value.status == 3) return Co.SetState(CustomEntity.Updaters.Coroutine.fail(res.value.value.error.message));
                    if (res.value.value.status == 1) return Co.SetState(CustomEntity.Updaters.Core.job(Job.Updaters.incrementProcessingCount()));

                    const completed = complete(res.value.value);

                    const u = Updater<CustomEntity>(entity =>
                        ({
                            ...entity,
                            status: {kind: 'job', job: completed},
                            trace: [...entity.trace.slice(0, -1), completed]
                        })
                    )
                    return Co.SetState(u)
                })
            ])
        })
    )
}

export const customFields =
    Co.Repeat(
        Co.GetState().then((entity: CustomEntity) => {
            if(!(
                entity.status.kind === 'job'
                && entity.status.job.kind === 'typechecking'
                && entity.status.job.status.kind === 'starting'
            )) return Co.SetState(_ => entity);
            
            const payload = entity.status.job.payload;
            
            return Co.Seq([
                Co.Await<ValueOrErrors<Guid, any>, any>(() =>
                    typeCheckingJob(payload), (_err: any) => {}).then(res => {
                    const response = CustomEntity.Operations.checkResponseForErrors(res, 'typechecking')
                    if(response.kind == "r") return response.value

                    const job = {
                        kind: 'typechecking',
                        payload: payload,
                        value: undefined,
                        status: { kind: 'processing', processing: JobProcessing.Default(res.value.value) }
                    } satisfies Job
                    const u = Updater<CustomEntity>(entity =>
                        ({
                            ...entity,
                            status: { kind: 'job', job: job },
                            trace: [...entity.trace.slice(0, -1), job]
                        })
                    )
                    return Co.SetState(u)
                }),
                awaitProcessingJob<{ valueDescriptorId: Guid}>((res: ResponseWithStatus<{
                        valueDescriptorId: Guid
                }>): Job =>
                        ({
                        kind: 'typechecking',
                        payload: payload,
                        value: res,
                        status: { kind: 'completed', how: JobProcessing.Default(res.id), took: 1234 }
                    } satisfies Job)
                ),
                Co.GetState().then((entity: CustomEntity) => {
                    if(!( entity.status.kind === "job"
                        && entity.status.job.kind == "typechecking"
                        && entity.status.job.status.kind == "completed"
                        && entity.status.job.value != undefined)) return Co.SetState(_ => entity);
                    const typeCheckingJobResponse = entity.status.job.value
                    const construction = {
                        kind: 'construction',
                        from: entity.status.job,
                        value: undefined,
                        status: { kind: 'starting' }
                    } satisfies Job
                    return Co.Seq([
                        Co.SetState(CustomEntity.Updaters.Core.job(replaceWith<Job>(construction)).then(CustomEntity.Updaters.Core.trace(
                            replaceWith([
                                ...entity.trace,
                                construction,
                            ])))),
                        Co.Await<ValueOrErrors<Guid, any>, any>(() =>
                            constructionJob({ValueDescriptorId: typeCheckingJobResponse.result.valueDescriptorId }), (_err: any) => {}).then(res => {
                            const response = CustomEntity.Operations.checkResponseForErrors(res, 'construction job')
                            if(response.kind == "r") return response.value
                            const job = {
                                ...construction,
                                status: {kind: 'processing', processing: JobProcessing.Default(res.value.value)}
                            } satisfies Job 
                            const next: Updater<CustomEntityStatus> = Updater<CustomEntityStatus> (curr => ({
                                kind: 'job',
                                job: job
                            } satisfies CustomEntityStatus))
                            const u = Updater<CustomEntity>(entity =>
                                ({
                                    ...entity,
                                    status: next(entity.status),
                                    trace: [...entity.trace.slice(0, -1), job]
                                })
                            )
                            return Co.SetState(u)
                        }),
                        awaitProcessingJob<{ valueId: Guid }>((res) =>
                            ({
                                ...construction,
                                value: res,
                                status: { kind: 'completed', how: JobProcessing.Default(res.id), took: 1234 }
                            } satisfies Job)
                        ),
                        Co.GetState().then((entity: CustomEntity) => {
                            if(!( entity.status.kind === "job"
                                && entity.status.job.kind == "construction"
                                && entity.status.job.status.kind == "completed"
                                && entity.status.job.value != undefined)) return Co.SetState(_ => entity);
                            const currentJob = entity.status.job
                            const currentJobResult = entity.status.job.value
                            return Co.Seq([
                                Co.SetState(Updater<CustomEntity>(entity => {
                                    const job = {
                                        kind: 'value',
                                        from: currentJob,
                                        value: undefined,
                                        status: { kind: 'starting' }
                                    } satisfies Job
                                    return ({
                                    status: { kind: 'job', job: job },
                                    trace: [
                                        ...entity.trace,
                                        job,
                                    ]} satisfies CustomEntity)
                                })),
                                Co.Await<ValueOrErrors<RequestValueJobResponse, any>, any>(() =>
                                    getValue(currentJobResult.result.valueId), (_err: any) => {}).then(res => {
                                    const response = CustomEntity.Operations.checkResponseForErrors(res, 'request value')
                                    if(response.kind == "r") return response.value
                                    debugger
                                    const completed = {
                                        kind: 'value',
                                        from: currentJob,
                                        value: res.value.value,
                                        status: { kind: 'completed', how: JobProcessing.Default(res.value.value.id), took: 0 }
                                    } satisfies Job;
                                    return Co.SetState(Updater<CustomEntity>(entity => {
                                        return ({
                                        ...entity,
                                        trace: [...entity.trace.slice(0, -1), completed],
                                        status: { kind: 'result', value: ValueOrErrors.Default.return(JSON.stringify(AI_Value_Mock))},
                                    })}))
                                }),
                            ]) }),
                    ]) }),
            ])})
    );