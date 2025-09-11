import {Option, View} from "ballerina-core";
import { Unit } from "ballerina-core";
import {BridgeState} from "../bridge/state";


export type JsonEditorReadonlyContext = {};
export type JsonEditorWritableState = Option<BridgeState> ;

export type JsonEditorForeignMutationsExpected = Unit

export type JsonEditorView = View<
   // JsonEditorReadonlyContext & 
    JsonEditorWritableState,
    JsonEditorWritableState,
    JsonEditorForeignMutationsExpected,
    {
    }
>;