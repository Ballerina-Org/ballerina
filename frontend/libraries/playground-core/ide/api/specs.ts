
import {axiosVOE} from "./api";
import {Spec, V1, V2, SpecVx} from "../state";


export const listSpecs = async () =>
    await axiosVOE<string[]>({
        method: "GET",
        url: "/specs",
    });

export const getSpec = async (name: string) =>
    await axiosVOE<Spec>({
        method: "GET",
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