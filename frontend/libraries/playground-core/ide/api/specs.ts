
import {axiosVOE} from "./api";
import {Node} from "../domains/locked/vfs/upload/model";
import {SpecMode} from "../domains/spec/state";
import {KnownSections} from "../domains/types/Json";


export const listSpecs = async () =>
    await axiosVOE<string[]>({
        method: "GET",
        url: "/specs",
    });

export const getSpec = async (name: string) =>

    await axiosVOE<Node, any>({
        method: "GET",
        url: `/specs/${name}`,
    });
export const getKeys = async (name: string, keysName: string, path: string []) =>

    await axiosVOE<string [], any>({
        method: "POST",
        url: `/specs/${name}/keys/${keysName}`,
        data: path
    });

export const getZippedWorkspace = async (name: string) =>

    await axiosVOE<any, any>({
        method: "GET",
        url: `/specs/${name}/download-zip`,
        responseType: "blob"
    });

export const initSpec = async (name: string, formsMode: SpecMode) =>
    await axiosVOE<{ folders: Node, settings: {workspaceMode: string}}, any>({
        method: "Post",
        url: `/specs/${name}`,
        data: {
            workspaceMode: formsMode.mode,
            dataEntry: formsMode.entry,
        }
    });

export const postVfs = async (name: string, node: Node) =>
    await axiosVOE<any>({
        method: "Post",
        url: `/specs/${name}/vfs`,
        data: node
    });

export const postCodegen = async (name: string, node: Node) =>
    await axiosVOE<any>({
        method: "Post",
        url: `/specs/${name}/codegen`,
        data: node
    });

export const getOrInitSpec = async (origin: 'create' | 'existing', formsMode: SpecMode, name: string) =>
    origin == 'existing' ? await getSpec(name) : await initSpec(name, formsMode).then(() => getSpec(name));

export const validateCompose = async (name: string) =>
    await axiosVOE<KnownSections>({
        method: "Post",
        timeout:10 * 60 * 1000,
        url: `/specs/${name}/validate/compose`,
    });
export const validateExplore = async (name: string, path: string []) =>
    await axiosVOE<KnownSections>({
        method: "Post",
        timeout:10 * 60 * 1000,
        url: `/specs/${name}/validate/explore`,
        data: path
    });

export const seed = async (name: string) =>
    await axiosVOE<any>({
        method: "PUT",
        url: `/specs/${name}/seed`,
    });

export const seedPath = async (name: string, path: string[]) => {
    const query = new URLSearchParams(path.map(p => ["path", p])).toString();

    return await axiosVOE<any>({
        method: "PUT",
        url: `/specs/${name}/seed?${query}`,
    });
};

export const addMissingVfsFiles = async (name: string, path: string[]) => {
    const query = new URLSearchParams(path.map(p => ["path", p])).toString();

    return await axiosVOE<any>({
        method: "POST",
        url: `/specs/${name}/vfs/missing?${query}`,
    });
};

export const update = async (name: string, path: string, content: any) =>
    await axiosVOE<any>({
        method: "PUT",
        data: { path: path, content: content },
        url: `/specs/${name}/vfs`,
    });

export const moveIntoOwnFolder = async (name: string, path: string[]) => {
    const query = new URLSearchParams(path.map(p => ["path", p])).toString();

    return await axiosVOE<any>({
        method: "POST",
        url: `/specs/${name}/vfs/move-to-own-folder?${query}`,
    });
};