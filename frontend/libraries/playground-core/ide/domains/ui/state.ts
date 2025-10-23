import {Option, Updater, Value} from "ballerina-core";
import {List} from "immutable";
import {Ide} from "../../state";

export type CommonUI = {
    specSelection: {
        specs: string[],         
        selected: Option<string>,
    };
    name: Value<string>,
    settingsVisible: boolean,
    heroVisible: boolean,
    bootstrappingError: List<string>,
    choosingError: List<string>,
    lockingError: List<string>,
    formsError: List<string>,
};

export const CommonUI = {
    Default: () : CommonUI => ({
        specSelection: { specs: [], selected: Option.Default.none() },
        name: Value.Default("Spec Name"),
        settingsVisible: false,
        heroVisible: true,
        bootstrappingError: List(),
        choosingError: List(),
        lockingError: List(),
        formsError: List(),
    })
}
