import {Option, Value} from "ballerina-core";
import {List} from "immutable";

export type CommonUI = {
    existing: { specs: string[]; selected: Option<string> },
    name: Value<string>,

    bootstrappingError: List<string>,
    choosingError: List<string>,
    lockingError: List<string>,
    formsError: List<string>,
};

export const CommonUI = {
    Default: () : CommonUI => ({
        existing: { specs: [], selected: Option.Default.none() },
        name: Value.Default("Spec Name"),

        bootstrappingError: List(),
        choosingError: List(),
        lockingError: List(),
        formsError: List(),
    })
}
