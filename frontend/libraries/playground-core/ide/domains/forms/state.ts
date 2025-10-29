import {DeltaDrain} from "./deltas/state";
import {Option} from "ballerina-core";

export type IdeFormProps = {
    spec:any, 
    specName: string,
    setState: (state: any) => void,
    launcher: string,
    deltas: Option<DeltaDrain>,
    showDeltas: boolean,
    path: string [],
}