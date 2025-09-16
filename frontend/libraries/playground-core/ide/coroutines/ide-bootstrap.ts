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
            Co.SetState(Ide.Updaters.bootstrap(Bootstrap.Updaters.Core.init("Retrieving specifications from the server"))),
            Co.Wait(1000),
            Co.Await<ValueOrErrors<string[], any>, any>(() =>
                listSpecs(), (_err: any) => {}).then(res =>
                res.kind == "r" ?
                    Co.SetState(Ide.Updaters.bootstrap(Bootstrap.Updaters.Core.error(List([`Unknown error occured when loading specs: ${res}`]))))
                    :
                    Co.SetState(
                        res.value.kind == "value" ? 
                            Updater(
                                Ide.Updaters.bootstrap(Bootstrap.Updaters.Core.ready(res.value.value))
                            )
                            .then(Ide.Updaters.toChoose())
                            : Ide.Updaters.bootstrap(Bootstrap.Updaters.Core.error(res.value.errors)))),
        ]
    );