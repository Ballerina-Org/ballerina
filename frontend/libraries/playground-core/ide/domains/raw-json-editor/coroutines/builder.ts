import { CoTypedFactory } from "ballerina-core";
import {RawJsonEditorReadonlyContext, RawJsonEditorWritableState, RawJsonEditor} from "../state";

export const Co = CoTypedFactory<RawJsonEditorReadonlyContext, RawJsonEditorWritableState>();
