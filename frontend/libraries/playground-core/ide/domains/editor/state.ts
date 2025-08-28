import {
    simpleUpdater,
    Option,
    replaceWith,
    BasicUpdater,
    Debounced,
    Synchronized,
    ValidationResult, Updater, Fun
} from "ballerina-core";

import { View } from "ballerina-core";
import { Unit, Value, ForeignMutationsInput } from "ballerina-core";
import {BridgeState} from "../bridge/state";


export type JsonEditorReadonlyContext = {};
export type JsonEditorWritableState = BridgeState ;

export type JsonEditorForeignMutationsExpected = Unit

export type JsonEditorView = View<
    JsonEditorReadonlyContext & JsonEditorWritableState,
    JsonEditorWritableState,
    JsonEditorForeignMutationsExpected,
    {
    }
>;