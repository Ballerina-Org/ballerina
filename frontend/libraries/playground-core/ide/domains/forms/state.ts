import {DeltaDrain} from "./deltas/state";
import {Option} from "ballerina-core";

export type IdeFormProps = {
    spec:any, 
    specName: string,
    //typeName: string,
    setState: (state: any) => void,
    launcher: string,
    deltas: Option<DeltaDrain>,
    path: string [],
    // launcherConfigName: string,
}