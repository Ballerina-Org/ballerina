import {Synchronize, apiResultStatuses, Value, Debounce, Synchronized, replaceWith, id, Option} from "ballerina-core";
import { IDEApi } from "../apis/spec";
import { Co} from "./builder";
import {EditorStep, IDE} from "../state";
import {RawJsonEditor} from "../domains/spec-editor/state";

export const runEditor =
    Co.Repeat(
        Co.While(ide => ide[0].shouldRun,
            Co.Seq([
                Co.GetState().then(ide =>
                    Co.Seq([
                        Co.SetState(IDE.Updaters.Core.rawEditor(
                            RawJsonEditor.Updaters.Core.step(
                                replaceWith(EditorStep.validating())
                            )
                        )),
                        Co.Await(() => IDEApi.validateSpec(ide.rawEditor.inputString), id)
                            .then((result) =>
                                {
                                    if(result.kind == "l") {
                                        return Co.Seq([
                                            Co.SetState(IDE.Updaters.Core.rawEditor(
                                                RawJsonEditor.Updaters.Core.validatedSpec(
                                                    replaceWith(Option.Default.some(ide.rawEditor.inputString.value))
                                                )
                                            )),
                                            Co.SetState(IDE.Updaters.Core.rawEditor(
                                                RawJsonEditor.Updaters.Core.step(
                                                    replaceWith(EditorStep.running())
                                                )
                                            )),
                                        ]);
                                    }
                                    else {
                                        return Co.Seq([
                                            Co.SetState(IDE.Updaters.Core.rawEditor(
                                                RawJsonEditor.Updaters.Core.validatedSpec(
                                                    replaceWith(Option.Default.none())
                                                )
                                            )),
                                        ]);
                                    }
                                }
                            )
                    ])
                ),
                Co.SetState(IDE.Updaters.Core.shouldRun(replaceWith(false)))
            ])
        )
    );