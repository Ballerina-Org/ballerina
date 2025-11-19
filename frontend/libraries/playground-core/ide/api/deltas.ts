import {axiosVOE} from "./api";
import {FormsSeedEntity} from "../domains/seeds/state";
import {Guid} from "ballerina-core";
import {DeltaDrain} from "../domains/forms/deltas/state";

export const sendDelta = 
    async (
        name: string,
        entityName: string, 
        id: Guid, 
        delta: DeltaDrain, 
        path: string [], 
        launcherName: string) => {
    const query = new URLSearchParams(path.map(p => ["path", p])).toString();
    await axiosVOE<any, any>({
        method: "POST",
        url: `/entities/${name}/${entityName}/delta/${id}/${launcherName}?${query}`,
        data: { deltas: delta.left}
    });
}


