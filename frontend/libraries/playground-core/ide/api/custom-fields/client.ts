import {axiosVOE} from "../api";
import {
    Guid,
} from "ballerina-core";
import {FormsSeedEntity} from "../../domains/types/seeds";
import {ConstructionJob, TypeCheckingPayload} from "../../domains/phases/custom-fields/domains/job/request/state";
import {
    RequestValueJobResponse,
    ResponseWithStatus
} from "../../domains/phases/custom-fields/domains/job/response/state";

export const fromCustomEntity = async (raw: any, specName: string, entityName: string, guid: Guid) =>

    await axiosVOE<FormsSeedEntity, any>({
        method: "POST",
        url: `/entities/${specName}/${entityName}/custom/${guid}`,
        data: raw
    });

export const typeCheckingJob = async (payload: TypeCheckingPayload) => {
    return await axiosVOE<Guid, any>({
        baseURL: '/jobs',
        method: "POST",
        url: `/values/typechecks`,
        data: payload
    });
}

export const constructionJob = async (request: ConstructionJob) => {
    return await axiosVOE<Guid, any>({
        baseURL: '/jobs',
        method: "Post",
        url: `/values/constructions`,
        data: request
    });
}

export const getJobStatus= async <result> (kind: 'typechecking' | 'construction', id: Guid) => {
    return await axiosVOE<ResponseWithStatus<result>, any>({
        baseURL: '/jobs',
        method: "GET",
        url: kind === 'typechecking' ? `/values/typechecks/${id}` : `/values/constructions/${id}`
    });
}

export const getValue = async (id: Guid) =>

    await axiosVOE<RequestValueJobResponse, any>({
        baseURL: '/jobs',
        method: "GET",
        url: `/values/${id}`
    });