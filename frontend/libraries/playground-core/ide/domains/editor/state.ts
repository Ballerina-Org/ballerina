import {View} from "ballerina-core";
import { Unit } from "ballerina-core";
import {Ide} from "../../state";

export type JsonEditorReadonlyContext = {};
export type JsonEditorWritableState = Ide ;

export type JsonEditorForeignMutationsExpected = Unit

export type JsonEditorView = View<
   // JsonEditorReadonlyContext & 
    JsonEditorWritableState,
    JsonEditorWritableState,
    JsonEditorForeignMutationsExpected,
    {
    }
>;