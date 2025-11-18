import {List} from "immutable";
import {Maybe, SimpleCallback, simpleUpdater, View} from "ballerina-core";

export type HeroPhase = { 
    errors: List<string>
}

export const HeroPhase = {
    Default: (): HeroPhase => ({
        errors: List<string>()
    }),
    Updaters: {
        Core: {
            ...simpleUpdater<HeroPhase>()("errors")
        }
    }
}

export type HeroPhaseForeignMutationsExpected = {
    onSelect1: SimpleCallback,
    onSelect2: SimpleCallback,
    //onSelect3: SimpleCallback,
}

export type HeroPhaseView = View<
    Maybe<HeroPhase>,
    Maybe<HeroPhase>,
    HeroPhaseForeignMutationsExpected,
    {
    }
>;
