import {axiosVOE} from "./api";

export const getSchemaEntitiesAndLookups = async (name: string) =>

    await axiosVOE<{ 
            entities: string[], 
            lookups: { lookupName: string, sourceEntity: string, targetEntity: string}[]
        }, any>({
        method: "GET",
        url: `/schema/${name}/entities-and-lookups`,
    });