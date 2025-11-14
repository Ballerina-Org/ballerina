import { CoTypedFactory } from "ballerina-core";
import {
    IdeReadonlyContext,
    IdeWritableState
} from "../state";

export const Co = CoTypedFactory<IdeReadonlyContext, IdeWritableState>();