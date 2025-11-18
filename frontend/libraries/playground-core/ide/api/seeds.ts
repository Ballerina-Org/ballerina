import {
    Guid,
    ValueOrErrors
} from "ballerina-core";
import {axiosVOE} from "./api";
import {FormsSeedEntity} from "../domains/types/seeds";

export const getSeed = async (specName: string, entityName: string): Promise<ValueOrErrors<FormsSeedEntity, any>> =>
    await axiosVOE<FormsSeedEntity[]>({
        method: "GET",
        url: `/entities/${specName}/${entityName}?skip=0&take=1`,
    }).then( res =>
    
        ValueOrErrors.Operations.Map((x: FormsSeedEntity[]) => x[0])(res)
    );
export const getSeeds= async (specName: string, entityName: string, skip: number, take: number) =>
    await axiosVOE<FormsSeedEntity[]>({
        method: "GET",
        url: `/entities/${specName}/${entityName}?skip=${skip}&take=${take}`,
    });

export const getLookups= async (specName: string, entityName: string,  id: Guid,skip: number, take: number) =>
    await axiosVOE<any[]>({
        method: "GET",
        url: `/lookups/${specName}/${entityName}/${id}?skip=${skip}&take=${take}`,
    });
