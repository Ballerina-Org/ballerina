import {Option, Updater, Value} from "ballerina-core";
import {List} from "immutable";
import {UploadStep} from "../choose/state";
import { Ide } from "ide/state";

export type Variant =
    (
        | { kind: 'compose' }
        | { kind: 'explore' }
        ) &  { upload: UploadStep }
    | { kind: 'scratch' }

export type CommonUI = {
    specSelection: {
        specs: string[],         
        selected: Option<string>,
    };
    variant: Variant,
    name: Value<string>,
    settingsVisible: boolean,
    heroVisible: boolean,
    bootstrappingError: List<string>,
    choosingError: List<string>,
    lockingError: List<string>,
    formsError: List<string>,
};

export const CommonUI = {
    Default: (variant: Variant) : CommonUI => ({
        specSelection: { specs: [], selected: Option.Default.none() },
        name: Value.Default("Spec Name"),
        variant: variant,
        settingsVisible: false,
        heroVisible: true,
        bootstrappingError: List(),
        choosingError: List(),
        lockingError: List(),
        formsError: List(),
    }),
    Updater: {
        Core: {
            specName: (name: Value<string>): Updater<Ide> => Updater(ide => ({...ide, name: name })),
            lockingErrors: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, lockingError: e})),
            bootstrapErrors: (e: List<string>): Updater<Ide> =>  Updater(ide => ({...ide, bootstrappingError: e})),
            chooseErrors: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, choosingError: e})),
            formsError: (e: List<string>): Updater<Ide> => Updater(ide => ({...ide, formsError: e})),
            toggleSettings: (): Updater<Ide> => Updater(ide => ({...ide, settingsVisible: !ide.settingsVisible})),
            toggleHero: (): Updater<Ide> => Updater(ide => ({...ide, heroVisible: !ide.heroVisible})),
            clearAllErrors : (): Updater<Ide> =>
                Updater(ide => ({
                    ...ide,
                    lockingError: List(),
                    choosingError: List(),
                    bootstrappingError: List(),
                }))
        }
    }
}
