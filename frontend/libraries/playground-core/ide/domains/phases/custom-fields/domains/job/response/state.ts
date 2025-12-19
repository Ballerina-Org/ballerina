import {Guid} from "ballerina-core";

export type ResponseWithStatus<result> = {
    id: Guid,
    status: number,
    startedAt: string,
    result: result,
    error: { message: string}
}

export type TypeCheckingDescriptor = {
    valueDescriptorId: Guid
}

export type ConstructionDescriptor = {
    valueId: Guid
}

export type UpdateDescriptor = {
    valueId: Guid
}

export type TypeCheckingJobResponse = ResponseWithStatus<TypeCheckingDescriptor>
export type ConstructionJobResponse = ResponseWithStatus<ConstructionDescriptor>
export type UpdateJobResponse = ResponseWithStatus<UpdateDescriptor>


export type RequestValueJobResponse = {
    id: Guid,
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
    accessors: any,
    value: string,
    lastUpdatedAt: string,
    isArchived: false,
    valueDescriptorId: Guid
}