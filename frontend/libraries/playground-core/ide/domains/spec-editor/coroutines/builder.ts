import {CoTypedFactory, Synchronized, Unit, Value} from "ballerina-core";
import {RawJsonEditorReadonlyContext, RawJsonEditorWritableState, SpecEditor} from "../state";

export const Co = CoTypedFactory<RawJsonEditorReadonlyContext, RawJsonEditorWritableState>();