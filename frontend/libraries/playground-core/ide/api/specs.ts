
import {axiosVOE} from "./api";
import {KnownSections} from "../domains/locked/vfs/state";
import {ValueOrErrors} from "ballerina-core";
import {FlatNode} from "../domains/locked/vfs/upload/model";
import {SpecMode} from "../domains/spec/state";


export const listSpecs = async () =>
    await axiosVOE<string[]>({
        method: "GET",
        url: "/specs",
    });

export const getSpec = async (name: string) =>

    await axiosVOE<{ folders: FlatNode, settings: {workspaceMode: string, dataEntry: string}}, any>({
        method: "GET",
        url: `/specs/${name}`,
    });

export const initSpec = async (name: string, formsMode: SpecMode) =>
    await axiosVOE<{ folders: FlatNode, settings: {workspaceMode: string}}, any>({
        method: "Post",
        url: `/specs/${name}`,
        data: {
            workspaceMode: formsMode.mode,
            dataEntry: formsMode.entry,
        }
    });

export const postVfs = async (name: string, node: FlatNode) =>
    await axiosVOE<any>({
        method: "Post",
        url: `/specs/${name}/vfs`,
        data: node
    });
// export const postVfsNode = async (name: string, node: FlatNode) =>
//     await axiosVOE<any>({
//         method: "Post",
//         url: `/specs/${name}/vfs/node`,
//         data: node
//     });
export const getOrInitSpec = async (origin: 'create' | 'existing', formsMode: SpecMode, name: string) =>
    origin == 'existing' ? await getSpec(name) : await initSpec(name, formsMode).then(() => getSpec(name));

export const validateCompose = async (name: string) =>
    await axiosVOE<KnownSections>({
        method: "Post",
        timeout:10 * 60 * 1000,
        url: `/specs/${name}/validateCompose`,
    });
export const validateExplore = async (name: string, path: string []) =>
    await axiosVOE<KnownSections>({
        method: "Post",
        timeout:10 * 60 * 1000,
        url: `/specs/${name}/validateExplore`,
        data: path
    });

export const seed = async (name: string) =>
    await axiosVOE<any>({
        method: "PUT",
        url: `/specs/${name}/seed`,
    });

export const update = async (name: string, path: string, content: any) =>
    await axiosVOE<any>({
        method: "PUT",
        data: { path: path, content: content },
        url: `/specs/${name}/vfs`,
    });