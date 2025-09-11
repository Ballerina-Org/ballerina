import {Co} from "./builder";
import {
    Updater, ValueOrErrors,
} from "ballerina-core";
import {Ide} from "../state";
import {listSpecs} from "../api/specs"
import {List} from "immutable";

export const bootstrap =
    Co.Seq([
            Co.SetState(Ide.Updaters.bootstrap.initializing("loading specs for tenant")),
            Co.Await<ValueOrErrors<string[], any>, any>(() =>
                listSpecs(), err => {}).then(res =>
                res.kind == "r" ?
                    Co.SetState(Ide.Updaters.bootstrap.error(List([`Unknown error occured when loading specs: ${res}`])))
                    :
                    Co.SetState(
                        res.value.kind == "value" ? 
                            Updater(
                                Ide.Updaters.bootstrap.ready(res.value.value)
                            )
                            .then(Ide.Updaters.toChoose())
                            
                            : Ide.Updaters.bootstrap.error(res.value.errors))),
        ]
    );