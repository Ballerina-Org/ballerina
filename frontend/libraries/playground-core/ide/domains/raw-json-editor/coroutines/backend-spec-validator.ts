import { Synchronize,apiResultStatuses, Value, Debounce, Synchronized } from "ballerina-core";
import { IDEApi } from "../../../apis/spec";
import { Co} from "./builder";
import { RawJsonEditor } from "../state";

export const validateSpecification =
    Co.Repeat(
        Debounce<Synchronized<Value<string>, boolean>>(
            Synchronize<Value<string>, boolean>(
                IDEApi.validateSpec,
                (_: any) => (_ in apiResultStatuses ? _ : "permanent failure"),
                5,
                150,
            ),
            1000
        ).embed((ide) => ide.inputString, RawJsonEditor.Updaters.Core.inputString),
    );