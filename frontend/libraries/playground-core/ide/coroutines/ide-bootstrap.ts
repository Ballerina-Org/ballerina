import {Co} from "./builder";
import {
    Updater, ValueOrErrors,
} from "ballerina-core";
import {Ide} from "../state";
import {listSpecs} from "../api/specs"
import {List} from "immutable";
import {Bootstrap} from "../domains/bootstrap/state";

export const bootstrap =
    Co.Seq([
            Co.SetState(Bootstrap.Updaters.Core.init("Retrieving specifications")),
            Co.Wait(1000),
            Co.Await<ValueOrErrors<string[], any>, any>(() =>
                listSpecs(), (_err: any) => {}).then(res =>{
                   
                return res.kind == "r" ?
                    Co.SetState(Ide.Updaters.CommonUI.bootstrapErrors(List([`Unknown error occured when loading specs: ${res}`])))
                    :
                    Co.SetState(
                        res.value.kind == "value" ? 
                            Updater(Bootstrap.Updaters.Core.ready(res.value.value)
                            )
                            .then(Ide.Updaters.Phases.toChoosePhase())
                            : Ide.Updaters.CommonUI.bootstrapErrors(res.value.errors))}),
        ]
    );