import {LayoutActions} from "./domains/actions/state";
import {SpecEditor} from "../spec-editor/state";
import {LayoutIndicators} from "./domains/indicators/state";

export type Layout = {
    //actions: LayoutActions,
    indicators: LayoutIndicators
};

export const Layout = {
    Default: () => {
        return {
            //actions: LayoutActions.Default(),
            indicators: LayoutIndicators.Default()
        };
    },
}
