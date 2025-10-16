
import {axiosVOE} from "./api";
import {Node} from "../domains/locked/vfs/upload/model";
import {SpecMode} from "../domains/spec/state";
import {KnownSections} from "../domains/types/Json";
import {FormsSeedEntity} from "../domains/seeds/state";



export const expand = async (name: string, launcherName: string, path: string []) =>

    await axiosVOE<FormsSeedEntity, any>({
        method: "POST",
        url: `/specs/${name}/forms/${launcherName}/expand`,
        data: path
    });
