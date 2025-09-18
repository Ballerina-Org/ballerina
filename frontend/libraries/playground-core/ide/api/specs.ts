
import {axiosVOE} from "./api";
//import {Spec, SpecVx} from "../domains/spec/state";
import {fromSlimJson, toSlimJson} from "../domains/spec/backend-model";
import {VirtualFolderNode} from "../domains/locked/vfs/state";
import {KnownSections} from "../domains/locked/vfs/state";
import {ValueOrErrors} from "ballerina-core";


export const listSpecs = async () =>
    await axiosVOE<string[]>({
        method: "GET",
        url: "/specs",
    });

export const getSpec = async (name: string) =>
    await axiosVOE<VirtualFolderNode, any>({
        method: "GET",
        url: `/specs/${name}`,
    }, fromSlimJson);

export const initSpec = async (name: string) =>
    await axiosVOE<any>({
        method: "Post",
        url: `/specs/${name}`,
    });

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

// export const update = async (name: string, vspec: SpecVx) =>
//     await axiosVOE<any>({
//         method: "PUT",
//         data: vspec,
//         url: `/specs/${name}`,
//     });