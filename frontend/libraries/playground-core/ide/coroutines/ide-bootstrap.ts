import {Co} from "./builder";
import {ValueOrErrors, replaceWith} from "ballerina-core";
import {Ide} from "../state";
import {listSpecs} from "../api/specs"
import {List} from "immutable";
import {BootstrapPhase} from "../domains/phases/bootstrap/state";
import {fromError, fromVoe} from "web/src/domains/ide/views/domains/layout/toaster";

export const bootstrap =
    Co.Seq([
        Co.SetState(
            Ide.Updaters.Core.phase.bootstrap(
                BootstrapPhase.Updaters.Coroutine.init("Retrieving specifications")
            )
        ),
        Co.Await<ValueOrErrors<string[], any>, any>(() =>
            listSpecs(), (_err: any) => {}).then(res => {
            if (res.kind == "r") {
                return Co.SetState(
                    Ide.Updaters.Core.phase.bootstrap(
                        BootstrapPhase.Updaters.Core.errors(
                            replaceWith(List([`Unknown error occured when loading specs: ${res}`])))))
            } else if (res.value.kind == "errors") {
                fromError(res.value, 'Connectivity error');
                return Co.SetState(
                    Ide.Updaters.Core.phase.bootstrap(
                        BootstrapPhase.Updaters.Core.errors(
                            replaceWith(res.value.errors))));
            }
            const value = res.value.value

            return Co.GetState().then((state) => {
                if(state.phase.kind != 'bootstrap') return Co.SetState(state => state)
                    return Co.SetState(
                        Ide.Updaters.Core.phase.bootstrap(
                            BootstrapPhase.Updaters.Core.ready()
                        ).then(
                            Ide.Updaters.Core.phase.toChoosePhase(state.phase.bootstrap.variant, value)
                        )
                    )
                }
            )
        })
    ]);