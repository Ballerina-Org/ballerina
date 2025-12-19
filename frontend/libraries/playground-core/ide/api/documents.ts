
import {axiosVOE} from "./api";
import {KnownSections} from "../domains/types/Json";
import {WorkspaceVariant} from "../domains/phases/locked/domains/folders/state";
import {INode, Meta} from "../domains/phases/locked/domains/folders/node";
import {Guid} from "ballerina-core";

export const listDocuments = async () => {
    return await axiosVOE<any>({
        method: "GET",
        url: "/documents",
    });
}

export const getDocument = async (id: Guid) => {
    return await axiosVOE<any>({
        method: "GET",
        url: `/documents/${id}`,
    });
}
