
import {axiosVOE} from "./api";

import {KnownSections} from "../domains/types/Json";
import {FormsSeedEntity} from "../domains/types/seeds";
import {Guid} from "ballerina-core";

export const expand = async (name: string, launcherName: string, path: string []) =>

    await axiosVOE<FormsSeedEntity, any>({
        method: "POST",
        url: `/specs/${name}/forms/${launcherName}/expand`,
        data: path
    });


export const validateBridge = async (name: string, path: string []) =>

    await axiosVOE<FormsSeedEntity, any>({
        method: "POST",
        url: `/specs/${name}/bridge/validate`,
        data: path
    });
