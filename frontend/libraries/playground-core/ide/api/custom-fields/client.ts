
import {axiosVOE} from "../api";

import {Guid} from "../../domains/types/Guid";
import {TypeCheckingJobResponse} from "../../domains/phases/custom-fields/domains/job/state";
import {TypeCheckingPayload} from "../../domains/phases/custom-fields/domains/type-checking/state";

export const typeCheckingJob = async (payload: TypeCheckingPayload) =>

    await axiosVOE<TypeCheckingJobResponse, any>({
        baseURL: '/jobs',
        method: "POST",
        url: `/values/typecheck`,
        data: payload
    });
