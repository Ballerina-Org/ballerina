
import {axiosVOE} from "../api";

// import {Guid} from "../../domains/types/Guid";
import {
    ConstructionJob,
    ConstructionJobResponse, RequestValueJobResponse, ResponseWithStatusAndResult,
    TypeCheckingJobResponse, UpdaterJob
} from "../../domains/phases/custom-fields/domains/job/state";
import {TypeCheckingPayload} from "../../domains/phases/custom-fields/domains/type-checking/state";
import {Co} from "../../coroutines/custom-fields/builder";
import {
    apiResultStatuses, BasicFun,
    Debounce,
    Guid,
    Synchronize,
    Synchronized,
    ValidationResult,
    Value,
    ValueOrErrors
} from "ballerina-core";
import {ParentApi} from "../../../parent/apis/mocks";
import {Parent} from "../../../parent/state";
import {CustomFields} from "../../domains/phases/custom-fields/state";

export const typeCheckingJob = async (payload: TypeCheckingPayload) => {
    console.log(JSON.stringify(payload, null, 2));
    return await axiosVOE<Guid, any>({
        baseURL: '/jobs',
        method: "POST",
        url: `/values/typechecks`,
        data: payload
    });
}

export const constructionJob = async (request: ConstructionJob) => {
    console.log(request.ValueDescriptorId)
    return await axiosVOE<Guid, any>({
        baseURL: '/jobs',
        method: "Post",
        url: `/values/constructions`,
        data: request
    });
}

export const updaterJob = async (request: UpdaterJob) =>

    await axiosVOE<Guid, any>({
        baseURL: '/jobs',
        method: "Post",
        url: `/values/updates`,
        data: request
    });
export const getTypeCheckingJobStatus = async (id: Guid) => {
    return await axiosVOE<TypeCheckingJobResponse, any>({
        baseURL: '/jobs',
        method: "GET",
        url: `/values/typechecks/${id}`
    });
}
export const getConstructionJobStatus = async (id: Guid) =>

    await axiosVOE<ConstructionJobResponse, any>({
        baseURL: '/jobs',
        method: "GET",
        url: `/values/constructions/${id}`
    });

export const getUpdaterJobStatus = async (id: Guid) =>

    await axiosVOE<ConstructionJobResponse, any>({
        baseURL: '/jobs',
        method: "GET",
        url: `/values/updates/${id}`
    });
export const getValue = async (id: Guid) =>

    await axiosVOE<RequestValueJobResponse, any>({
        baseURL: '/jobs',
        method: "GET",
        url: `/values/${id}`
    });

export const statusSynchronizer = <result>(call: BasicFun<any, Promise<ValueOrErrors<ResponseWithStatusAndResult<result>, string>>>) => {
    const s = 
        Synchronize<CustomFields, ValueOrErrors<ResponseWithStatusAndResult<result>, string>>(
            call,
            (_: ValueOrErrors<ResponseWithStatusAndResult<result>, string>) =>  
                _.kind != "errors" && (_.value.status == 3 || _.value.status == 1) ? "transient failure" : "permanent failure",
            3,
            5000,
        );
    return s;
}