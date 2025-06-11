import { CoTypedFactory } from "ballerina-core";
import {
    IDEReadonlyContext,
    IDEWritableState
} from "../state";

export const Co = CoTypedFactory<IDEReadonlyContext, IDEWritableState>();
