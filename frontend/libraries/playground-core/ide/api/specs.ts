
import {axiosVOE} from "./api";
import {Spec, SpecVx} from "../domains/spec/state";
import {Content, EncodedContent, toSlimJson} from "../domains/spec/backend-model";


export const listSpecs = async () =>
    await axiosVOE<string[]>({
        method: "GET",
        url: "/specs",
    });

export const getSpec = async (name: string) =>
    await axiosVOE<Content, EncodedContent>({
        method: "GET",
        url: `/specs/${name}`,
    }, toSlimJson);

export const initSpec = async (name: string) =>
    await axiosVOE<Spec>({
        method: "Post",
        url: `/specs/${name}`,
    });

export const seed = async (name: string) =>
    await axiosVOE<any>({
        method: "PUT",
        url: `/specs/${name}/seed`,
    });

export const update = async (name: string, vspec: SpecVx) =>
    await axiosVOE<any>({
        method: "PUT",
        data: vspec,
        url: `/specs/${name}`,
    });