import {Co} from "./builder";
import {
    replaceWith,
    Value,
} from "ballerina-core";
import {Ide} from "../state";
import {listSpecs} from "../api/specs"

export const specNames =
    Co.Seq([
            Co.Await(() =>
                listSpecs(), err => {}).then(res =>
                Co.SetState(
                    Ide.Updaters.Core.specNames(replaceWith(res.kind == "l" && res.value.kind == "value" ?  res.value.value : [])))),
        ]
    );