
import {axiosVOE} from "../api";

// import {Guid} from "../../domains/types/Guid";
import {
    ConstructionJob,
    ConstructionJobResponse, RequestValueJobResponse, ResponseWithStatus,
    TypeCheckingJobResponse
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
import {CustomEntity} from "../../domains/phases/custom-fields/state";
import {FormsSeedEntity} from "../../domains/types/seeds";
export const fromCustomEntity = async (raw: any, specName: string, entityName: string, guid: Guid) =>

    await axiosVOE<FormsSeedEntity, any>({
        method: "POST",
        url: `/entities/${specName}/${entityName}/custom/${guid}`,
        data: raw
    });

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

    return await axiosVOE<Guid, any>({
        baseURL: '/jobs',
        method: "Post",
        url: `/values/constructions`,
        data: request
    });
}

// export const updaterJob = async (request: UpdaterJob) =>
//
//     await axiosVOE<Guid, any>({
//         baseURL: '/jobs',
//         method: "Post",
//         url: `/values/updates`,
//         data: request
//     });


export const getJobStatus= async <result> (kind: 'typechecking' | 'construction', id: Guid) => {
    return await axiosVOE<ResponseWithStatus<result>, any>({
        baseURL: '/jobs',
        method: "GET",
        url: kind === 'typechecking' ? `/values/typechecks/${id}` : `/values/constructions/${id}`
    });
}

// export const getTypeCheckingJobStatus = async (id: Guid) => {
//     return await axiosVOE<TypeCheckingJobResponse, any>({
//         baseURL: '/jobs',
//         method: "GET",
//         url: `/values/typechecks/${id}`
//     });
// }
// export const getConstructionJobStatus = async (id: Guid) =>
//
//     await axiosVOE<ConstructionJobResponse, any>({
//         baseURL: '/jobs',
//         method: "GET",
//         url: `/values/constructions/${id}`
//     });


export const getValue = async (id: Guid) =>

    await axiosVOE<RequestValueJobResponse, any>({
        baseURL: '/jobs',
        method: "GET",
        url: `/values/${id}`
    });

// export const statusSynchronizer = <result>(call: BasicFun<any, Promise<ValueOrErrors<ResponseWithStatusAndResult<result>, string>>>) => {
//     const s = 
//         Synchronize<CustomEntity, ValueOrErrors<ResponseWithStatusAndResult<result>, string>>(
//             call,
//             (_: ValueOrErrors<ResponseWithStatusAndResult<result>, string>) =>  
//                 _.kind != "errors" && (_.value.status == 3 || _.value.status == 1) ? "transient failure" : "permanent failure",
//             3,
//             5000,
//         );
//     return s;
// }