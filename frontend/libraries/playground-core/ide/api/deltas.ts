import {axiosVOE} from "./api";
import {FormsSeedEntity} from "../domains/seeds/state";
import {Guid} from "ballerina-core";

export const sendDelta = async (name: string, entityName: string, id: Guid, delta: any, path: string []) => {
    const query = new URLSearchParams(path.map(p => ["path", p])).toString();
    await axiosVOE<any, any>({
        method: "POST",
        url: `/entities/${name}/${entityName}/delta/${id}?${query}`,
        data: delta
    });
}


