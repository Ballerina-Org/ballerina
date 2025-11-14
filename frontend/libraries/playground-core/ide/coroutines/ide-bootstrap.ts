import {Co} from "./builder";
import {Updater, ValueOrErrors, Option} from "ballerina-core";
import {Ide} from "../state";
import {listSpecs} from "../api/specs"
import {List} from "immutable";
import {BootstrapPhase} from "../domains/bootstrap/state";
import {CommonUI} from "../domains/common-ui/state";

export const bootstrap =
    Co.Seq([
        Co.SetState(Ide.Updaters.Phases.bootstrapping.update(BootstrapPhase.Updaters.Coroutine.init("Retrieving specifications"))),
        Co.Await<ValueOrErrors<string[], any>, any>(() =>
            listSpecs(), (_err: any) => {}).then(res => {
            if (res.kind == "r") {
                return Co.SetState(CommonUI.Updater.Core.bootstrapErrors(List([`Unknown error occured when loading specs: ${res}`])))
            } else if (res.value.kind == "errors") {
                return Co.SetState(CommonUI.Updater.Core.bootstrapErrors(res.value.errors));
            }
            const value = res.value.value
            return Co.SetState(Updater(Ide.Updaters.Phases.bootstrapping.update(BootstrapPhase.Updaters.Core.ready())
                .then(Updater<Ide>(ide => ({
                    ...ide, specSelection: {specs: value, selected: Option.Default.none()}
                })))
                .then(Ide.Updaters.Phases.bootstrapping.toChoosePhase())))
        })
    ]);