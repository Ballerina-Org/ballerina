import {Option, Value} from "ballerina-core";
import {List} from "immutable";

export type CommonUI = {
    specOrigin: 'existing' | 'create';
    existing: { specs: string[]; selected: Option<string> };
    create: { name: Value<string> };

    bootstrappingError: List<string>,
    choosingError: List<string>,
    lockingError: List<string>,
};

export const CommonUI = {
    Default: () : CommonUI => ({
        specOrigin: 'create',
        existing: { specs: [], selected: Option.Default.none() },
        create: { name: Value.Default("Spec ") },

        bootstrappingError: List(),
        choosingError: List(),
        lockingError: List(),
    })
}
