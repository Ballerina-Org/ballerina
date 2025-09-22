
import {axiosVOE} from "./api";
import {KnownSections} from "../domains/locked/vfs/state";
import {ValueOrErrors} from "ballerina-core";
import {FlatNode} from "../domains/locked/vfs/upload/model";


export const listSpecs = async () =>
    await axiosVOE<string[]>({
        method: "GET",
        url: "/specs",
    });

export const getSpec = async (name: string) =>

    await axiosVOE<FlatNode, any>({
        method: "GET",
        url: `/specs/${name}`,
    });

export const initSpec = async (name: string) =>
    await axiosVOE<any>({
        method: "Post",
        url: `/specs/${name}`,
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
export const getOrInitSpec = async (origin: 'create' | 'existing', name: string) =>
    origin == 'existing' ? await getSpec(name) : await initSpec(name).then(() => getSpec(name));

export const validate = async (name: string) =>
    await axiosVOE<KnownSections>({
        method: "Post",
        url: `/specs/${name}/validate`,
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