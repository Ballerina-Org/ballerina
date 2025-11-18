import {axiosVOE} from "./api";
import {Guid, ValueOrErrors} from "ballerina-core";
import {DeltaDrain} from "../domains/phases/locked/domains/forms/domains/delta/state";

export const sendDelta = 
    async (
        name: string,
        entityName: string, 
        id: Guid, 
        delta: DeltaDrain, 
        path: string [], 
        launcherName: string): Promise<ValueOrErrors<any, any>> => {
    const query = new URLSearchParams(path.map(p => ["path", p])).toString();
    return await axiosVOE<any, any>({
        method: "POST",
        url: `/entities/${name}/${entityName}/delta/${id}/${launcherName}?${query}`,
        data: { deltas: delta.left}
    });
}


